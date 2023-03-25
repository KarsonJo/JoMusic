using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    public class DeleteDialogueViewModel : CommitableObject
    {
        private bool deleteFile;
        private readonly List<FolderNode> nodesToDelete;
        private readonly List<SongFileMetum> songsToDelete;
        private readonly long directoryId;

        public bool DeleteFile
        {
            get { return deleteFile; }
            set { deleteFile = value; }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set 
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }


        public DeleteDialogueViewModel()
        {

        }
        public DeleteDialogueViewModel(List<FolderNode> nodesToDelete, List<SongFileMetum> songsToDelete, long directoryId)
        {
            this.nodesToDelete = nodesToDelete;
            this.songsToDelete = songsToDelete;
            this.directoryId = directoryId;

            int totalCount = nodesToDelete.Count + songsToDelete.Count;
            if (totalCount >= 2)
            {
                Description = $"共删除{totalCount}个项目";
            }
            else
            {
                if (nodesToDelete.Count == 1)
                {
                    Description = $"删除文件夹：{nodesToDelete[0].Dirname}";
                }
                else if (songsToDelete.Count == 1)
                {
                    Description = $"删除歌曲：{songsToDelete[0].SongName}";
                }
                else
                {
                    throw new ArgumentOutOfRangeException("删除项不能为0");
                }
            }
        }

        protected override async Task CommitCommand(object? p)
        {
            foreach (var item in nodesToDelete)
            {
                await FileManager.DeleteVirtualFolderRecursively(item.Id, deleteFile);
            }

            if (songsToDelete.Count > 0)
            {
                //无论如何，删除该结点，以及时刷新
                await FileManager.DeleteFileNodes(songsToDelete, directoryId);

                if (deleteFile)
                {
                    //删除真实文件
                    FileManager.DeleteRealFiles(from song in songsToDelete select song.FileName);
                }
            }
        }

    }
}
