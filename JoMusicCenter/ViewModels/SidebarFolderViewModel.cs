using JoMusicCenter.Commands;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JoMusicCenter.ViewModels
{
    public enum SidebarFolderType
    {
        FileFolder,
        MusicFolder,
        PlaylistFolder
    }

    public class SidebarFolderViewModel : NavigatableObject
    {
        /// <summary>
        /// 资源字典
        /// </summary>
        private readonly ResourceDictionary resourceDic = (ResourceDictionary)Application.LoadComponent(new Uri("/JoMusicCenter;component/Styles/Icons.xaml", UriKind.Relative));

        private readonly SidebarFolderType iconType;

        //private readonly string folderName;

        public Func<IEnumerable<SidebarFolderViewModel>, Task>? PlayButtonClick { get; set; }
        public Func<IEnumerable<SidebarFolderViewModel>, Task>? UnpinButtonClick { get; set; }



        public Geometry Icon => (Geometry)resourceDic[iconType.ToString()];
        public string FolderName => NavigationNode?.DisplayName ?? "name not exists";
        public bool IsPlaylist => iconType == SidebarFolderType.PlaylistFolder;

        public SidebarFolderViewModel(FolderNode folder, SidebarFolderType icon = SidebarFolderType.PlaylistFolder)
        {
            //this.Folder = folder;
            //folderName = folder.Dirname;
            iconType = icon;
            NavigationNode = new(folder);
        }

        //public SidebarFolderViewModel(PlayList playlist, SidebarFolderType icon = SidebarFolderType.PlaylistFolder)
        //{
        //    //this.Folder = null;
        //    folderName = playlist.NavValue;
        //    iconType = icon;
        //    NavigationNode = new(playlist);
        //}

        //public SidebarFolderViewModel(string folderName, SidebarFolderType iconType, Func<Task<List<FolderNode>?>> loadLogic)
        //{
        //    this.folderName = folderName;
        //    this.iconType = iconType;
        //    SpecialLoadLogic = loadLogic;
        //}

        //public SidebarFolderViewModel(string folderName, SidebarFolderType iconType, NavigationInfo navigationInfo)
        //{
        //    //this.folderName = folderName;
        //    this.iconType = iconType;
        //    NavigationNode = navigationInfo;
        //}

        public SidebarFolderViewModel(NavigationInfo navigationInfo, SidebarFolderType icon = SidebarFolderType.PlaylistFolder)
        {
            this.iconType = icon;
            NavigationNode = navigationInfo;
        }

        private ICommand? playCommand;
        public ICommand PlayCommand
        {
            get
            {
                if (playCommand == null)
                {
                    playCommand = new RelayCommand(
                        null,
                        async p =>
                        {
                            if (p is not SidebarFolderViewModel fileViews)
                            {
                                return;
                            }
                            else if (PlayButtonClick != null)
                            {
                                await PlayButtonClick(new List<SidebarFolderViewModel>() { fileViews });
                            }

                        });
                }
                return playCommand;
            }
        }

        private ICommand? unpinCommand;
        public ICommand UnpinCommand
        {
            get
            {
                if (unpinCommand == null)
                {
                    unpinCommand = new RelayCommand(
                        null,
                        async p =>
                        {
                            if (p is not SidebarFolderViewModel fileViews)
                            {
                                return;
                            }
                            else if (UnpinButtonClick != null)
                            {
                                await UnpinButtonClick(new List<SidebarFolderViewModel>() { fileViews });
                            }

                        });
                }
                return unpinCommand;
            }
        }
    }
}
