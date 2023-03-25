using MusicCrawler.Download;
using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    /// <summary>
    /// 导出的模型
    /// 虚拟路径应该从构造时给定并不再更改
    /// </summary>
    public class ExportDialogueViewModel : TransportDialogueViewModel
    {
        private readonly IEnumerable<FolderNode> folderIds;

        public ExportDialogueViewModel(IEnumerable<FolderNode> folderIds, string inputText)
        {
            IsExport = true;

            this.folderIds = folderIds;
            //直接设置，不改变PathNotification
            pathInput.InputText = inputText;
        }

        public ExportDialogueViewModel(FolderNode folderId, string[] location)
        {
            IsExport = true;

            folderIds = new List<FolderNode>() { folderId };
            hierarchicalLocation = new(location);
            PathText = string.Join(@"\", hierarchicalLocation);
        }
        protected override async Task CommitCommand(object? p)
        {
            if (folderPath != null)
            {
                foreach (var folderId in folderIds)
                {
                    var songs = await FileManager.GetSongsWithoutArtistsData(folderId.Id);

                    foreach (var song in songs)
                    {
                        var downloadTask = new LocalTransportTask(FileManager.GetMusicFilePath(song.FileName), Path.Combine(folderPath, folderId.Dirname, song.FileName));
                        //downloadTask.MadeProgress += () => { System.Diagnostics.Debug.WriteLine($"downloading: {downloadTask.TaskName}, progress: {downloadTask.Progress}"); };
                        DownloadManager.AddTask(DownloadTaskType.Local, downloadTask);
                    }
                }
            }
        }
    }
}
