using JoMusicCenter.Commands;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    /// <summary>
    /// 提供映射实际路径与虚拟路径相互映射的会话
    /// 存储实际路径、虚拟路径等数据
    /// 主输入被用作实际路径的输入
    /// </summary>
    public abstract class PathMappingDialogueViewModel : PathDialogueViewModel
    {
        private ICommand? selectFolderCommand;
        protected string? folderPath;

        public event Action? PathChanged;

        public ICommand SelectFolderCommand
        {
            get
            {
                if (selectFolderCommand == null)
                {
                    selectFolderCommand = new RelayCommand(null,
                        p =>
                        {
                            //选择文件夹
                            CommonOpenFileDialog dialog = new CommonOpenFileDialog() { };
                            dialog.InitialDirectory = "C:\\Users";
                            dialog.IsFolderPicker = true;
                            if (dialog.ShowDialog(p as Window) == CommonFileDialogResult.Ok)
                            {
                                //MessageBox.Show("You selected: " + dialog.FileName);
                                folderPath = dialog.FileName;

                                //更改显示
                                InputText = folderPath;

                                PathChanged?.Invoke();
                            }
                        });
                }
                return selectFolderCommand;
            }
        }

        private bool recursiveSearch;

        public bool RecursiveSearch
        {
            get { return recursiveSearch; }
            set
            {
                recursiveSearch = value;
                OnPropertyChanged(nameof(RecursiveSearch));
            }
        }
    }
}
