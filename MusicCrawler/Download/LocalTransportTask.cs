using MusicLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicCrawler.Download
{
    /// <summary>
    /// 只传输，不记录
    /// </summary>
    public class LocalTransportTask : DownloadTask
    {
        string fromPath;
        string toPath;

        public override string TaskName => "导出文件：" + Path.GetFileName(fromPath);

        public LocalTransportTask(string fromPath, string toPath)
        {
            this.fromPath = Path.IsPathRooted(fromPath) ? fromPath : Path.GetFullPath(fromPath);
            this.toPath = toPath;
        }

        protected override async Task DownloadLogic(CancellationToken cancellationToken = default)
        {
            FileSizeB = new FileInfo(fromPath).Length;

            //0 检测重复
            if (File.Exists(toPath))
            {
                State = TaskState.Failed;
                Notification = "存在重名文件，已跳过任务";
            }
            else
            {
                //1 创建目录
                Directory.CreateDirectory(Path.GetDirectoryName(toPath));
                //2 本地拷贝
                await DownloadFile(toPath, new Uri(fromPath), cancellationToken);
            }
        }
    }
}
