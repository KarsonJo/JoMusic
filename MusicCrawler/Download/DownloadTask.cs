using MusicCrawler.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicCrawler.Download
{
    public enum TaskState
    {
        Started,
        Paused,
        Waiting,
        Failed,
        Completed
    }

    public abstract class DownloadTask : IProgress<double>
    {
        CancellationTokenSource cts;
        private string? notification;
        private double progress;
        private long? fileSizeB;
        private TaskState state = TaskState.Waiting;

        [JsonIgnore]
        public TaskState State 
        { 
            get => state; 
            protected set
            {
                state = value;
                StateChanged?.Invoke();
            }
        }
        [JsonIgnore]
        public string? Notification
        {
            get => notification;
            set
            {
                if (notification != value)
                {
                    notification = value;
                    NotificationChanged?.Invoke();
                }
            }

        }

        public abstract string TaskName { get; }
        [JsonIgnore]
        public double Progress
        {
            get => progress;
            private set
            {
                progress = value;
                MadeProgress?.Invoke();
            }
        }

        public long? FileSizeB
        {
            get => fileSizeB;
            protected set
            {
                fileSizeB = value;
                GotFileSize?.Invoke();
            }
        }

        public double FileSizeMB => FileSizeB.HasValue ? (double)FileSizeB / 1048576 : -1;

        public delegate void DownloadEventHandler();
        public event DownloadEventHandler MadeProgress;
        public event DownloadEventHandler GotFileSize;
        public event DownloadEventHandler NotificationChanged;
        public event DownloadEventHandler StateChanged;
        public event EventHandler DownloadCanceling;
        public event EventHandler DownloadCompleted;

        /// <summary>
        /// 开始下载，该函数只应被下载管理器统一调用
        /// </summary>
        /// <returns></returns>
        public async Task StartDownload()
        {
            try
            {
                using (cts = new CancellationTokenSource())
                {
                    State = TaskState.Started;
                    await DownloadLogic(/*this, */cts.Token);
                    DownloadCompleted?.Invoke(this, null);
                    State = TaskState.Completed;
                }
            }
            catch (Exception e)
            {
                Notification = e.Message;
                Debug.WriteLine(e.Message);
                State = TaskState.Failed;
            }
        }

        protected abstract Task DownloadLogic(/*IProgress<double> progress = null, */CancellationToken cancellationToken = default);

        public void StopDownload()
        {
            if (cts != null)
            {
                DownloadCanceling?.Invoke(this, null);
                cts.Cancel();
                cts.Dispose();
                State = TaskState.Paused;
            }
        }

        protected Stream CreateStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        }

        public void Report(double value)
        {
            Progress = value;
        }

        protected async Task DownloadFile(string targetPath, Uri downloadFromUri, CancellationToken cancellationToken)
        {
            
            try
            {

                //1 创建文件
                using (var file = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //网上冲浪
                    if (downloadFromUri.Scheme == "http" || downloadFromUri.Scheme == "https")
                    {
                        await HttpClientHolder.Client.GetStreamAsync(downloadFromUri);
                        //2.1 与下载服务器取得联系
                        using (var response = await HttpClientHolder.Client.GetAsync(downloadFromUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        {
                            //2.2 获取文件大小
                            FileSizeB = response.Content.Headers.ContentLength;

                            //2.3 下载文件
                            await HttpClientHolder.Client.DownloadAsync(response, file, this, cancellationToken);
                        }
                    }
                    //本地开导
                    else if (downloadFromUri.Scheme == "file")
                    {
                        string sourceLocalPath = downloadFromUri.LocalPath;

                        if (FileSizeB == null)
                        {
                            FileSizeB = new FileInfo(sourceLocalPath).Length;
                        }

                        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                        var relativeProgress = new Progress<long>(totalBytes => Report(totalBytes / (double)FileSizeB));
                        // Use extension method to report progress while downloading
                        await File.OpenRead(sourceLocalPath).CopyToAsync(file, 81920, relativeProgress, cancellationToken);
                        Report(1);
                    }
                    else
                    {
                        throw new ArgumentException($"Not supported uri scheme when downloading: {downloadFromUri.Scheme}");
                    }
                }

            }
            catch (OperationCanceledException e)
            {
                //取消任务，处理异常
                Debug.WriteLine($"下载任务取消：{TaskName}, {e.Message}");
                File.Delete(targetPath);
            }
            catch
            {
                //其它异常，处理后继续抛出
                File.Delete(targetPath);
                throw;
            }
        }
    }

    public abstract class DownloadTask<T> : DownloadTask
    {
        public T DownloadedContent { get; protected set; }
    }
}
