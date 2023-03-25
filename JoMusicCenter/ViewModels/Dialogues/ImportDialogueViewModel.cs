using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using MusicCrawler.Download;
using MusicLibrary;

namespace JoMusicCenter.ViewModels
{
    public class ImportDialogueViewModel : TransportDialogueViewModel
    {
        //private readonly List<string> supportedAudioTypes = new() { "m4a", "flac", "mp3", "mp4", "wav", "wma", "aac" };

        public ImportDialogueViewModel()
        {
            PathChanged += ChangedVirtualPath;
            IsExport = false;
        }

        protected override async Task CommitCommand(object? p)
        {
            if (folderPath != null)
            {
                await Task.Run(() =>
                {
                    //get files with ext filters: https://stackoverflow.com/questions/13301053/directory-getfiles-of-certain-extension
                    var files = from file in Directory.EnumerateFiles(folderPath, "*.*", RecursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                where FileManager.SupportedAudioTypes.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant())
                                select file;

                    //建立本地下载任务
                    foreach (var file in files)
                    {
                        var downloadTask = new LocalDownloadTask(new LocalSongStreamData(file));
                        downloadTask.DownloadCompleted += DownloadTask_DownloadCompleted;
                        //downloadTask.MadeProgress += () => { System.Diagnostics.Debug.WriteLine($"downloading: {downloadTask.TaskName}, progress: {downloadTask.Progress}"); };
                        DownloadManager.AddTask(DownloadTaskType.Local, downloadTask);
                    }

                    async void DownloadTask_DownloadCompleted(object? sender, EventArgs e)
                    {
                        if (sender != null && hierarchicalLocation != null)
                        {
                            //取出结果数据
                            var songStreamData = ((LocalDownloadTask)sender).DownloadedContent;
                            //记录到文件夹
                            await FileManager.RecordToVirtualFolder(songStreamData, hierarchicalLocation.ToArray());
                        }
                    }
                });
            }
        }

        void ChangedVirtualPath()
        {
            if (folderPath != null)
            {
                //更改默认路径
                defaultLocation = new() { "导入", Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar)) };

                PathText = DefaultLocation;
            }
        }
    }
}
