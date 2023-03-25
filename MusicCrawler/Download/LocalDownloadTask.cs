using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicCrawler.Download
{
    /// <summary>
    /// 从本地建立下载任务
    /// 事实上是拷贝，不过会在任务列表中显示进度
    /// </summary>
    public class LocalDownloadTask : DownloadTask<List<SongFileMetum>>
    {
        LocalSongStreamData localSongStreamData;

        public override string TaskName => "导入文件：" + localSongStreamData.FileNameWithExtension;

        public LocalDownloadTask(LocalSongStreamData localSongStreamData)
        {
            this.localSongStreamData = localSongStreamData;
        }

        protected override async Task DownloadLogic(CancellationToken cancellationToken = default)
        {
            FileSizeB = new System.IO.FileInfo(localSongStreamData.LocalMusicFilePath).Length;

            //0 检测重复
            var duplicateFile = await FileManager.CheckFileExistence(localSongStreamData);
            if (duplicateFile == null)
            {
                //1 本地拷贝
                await DownloadFile(FileManager.GetMusicFilePath(localSongStreamData.FileNameWithExtension), new Uri(localSongStreamData.LocalMusicFilePath), cancellationToken);

                //2 写入数据
                DownloadedContent = await FileManager.WriteSongMetaData(new List<SongStreamData>() { localSongStreamData });
            }
            else
            {
                //1 直接下完
                Report(1);
                DownloadedContent = new() { duplicateFile };
            }


        }
    }
}
