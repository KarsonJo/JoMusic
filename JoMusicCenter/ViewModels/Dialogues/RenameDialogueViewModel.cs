using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JoMusicCenter.ViewModels
{
    public class RenameDialogueViewModel : DialogueViewModel
    {
        FolderNode? folderWithNewName;
        SongFileMetum? songWithNewName;

        public string Title { get; }
        protected override async Task CommitCommand(object? p)
        {
            if (folderWithNewName != null)
            {
                if (await FileManager.RenameFolder(folderWithNewName))
                {
                    OnCloseRequest();
                }
                else
                {
                    MainNotification = "文件夹重命名失败，未知错误";
                }
            }
            else if (songWithNewName != null)
            {
                if (await FileManager.RenameFile(songWithNewName))
                {
                    OnCloseRequest();
                }
                else
                {
                    MainNotification = "曲目重命名失败，未知错误";
                }
            }
            else
            {
                MainNotification = "错误：重命名目标为空";
            }
        }

        //public RenameDialogueViewModel()
        //{
        //}
        public RenameDialogueViewModel(FolderNode targetFolder)
        {
            this.folderWithNewName = targetFolder;
            this.Title = "重命名文件夹：" + targetFolder.Dirname;
        }

        public RenameDialogueViewModel(SongFileMetum targetSong)
        {
            songWithNewName = targetSong;
            this.Title = "重命名歌曲：" + targetSong.SongName;
        }

        protected override async void MainInputChanged()
        {
            //对文件夹重命名检测
            if (folderWithNewName != null)
            {
                folderWithNewName.Dirname = InputText;
                if (await FileManager.CheckDuplicateName(folderWithNewName))
                {
                    MainNotification = "名称与现有文件夹重复";
                    mainInput.NoConflicts = false;
                }
                else
                {
                    MainNotification = "";
                    mainInput.NoConflicts = true;
                }
            }
            else if (songWithNewName != null)
            {
                songWithNewName.SongName = InputText;
            }
        }
    }
}
