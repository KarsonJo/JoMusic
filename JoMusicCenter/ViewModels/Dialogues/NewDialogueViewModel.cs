using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    public class NewDialogueViewModel : DialogueViewModel
    {
        FolderCreate folderWithNewName;

        public string Title { get; }
        protected override async Task CommitCommand(object? p)
        {
            if (await FileManager.NewFolder(folderWithNewName))
            {
                OnCloseRequest();
            }
        }

        public NewDialogueViewModel()
        {
        }
        public NewDialogueViewModel(FolderCreate targetFolder)
        {
            this.folderWithNewName = targetFolder;
            this.Title = "新建文件夹";
        }

        protected override async void MainInputChanged()
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
            OnPropertyChanged(nameof(MainNotification));
        }
    }
}
