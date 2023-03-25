using JoMusicCenter.Commands;
using MusicCrawler.Download;
using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JoMusicCenter.ViewModels
{
    public enum ClipboardState
    {
        None,
        Cut,
        Copy
    }

    public class MainWindowViewModel : NotifyPropertyChangedObject
    {

        private readonly NavigationHelper navigationHelper = new();

        private readonly MediaPlayerHelper mediaPlayerHelper = new();

        // ========== clipboard ==========
        private readonly ClipboardHelper clipboardHelper = new();
        public long cutDirectory;
        private ClipboardState clipboardState;

        private bool windowEnable = true;

        //private long currentDirectory;
        //private bool canGoForward;
        //private bool canGoBack;
        //private bool atRootDictionary;
        //public int navigatePosition;


        public MainWindowViewModel()
        {

            InitializeNavigator();
            InitializeMediaPlayer();

            long root = CurrentDirectoryId;
            SidebarLibraryFolders.Add(new SidebarFolderViewModel(new FolderNode() { Dirname = "根目录" , Id = 1}, SidebarFolderType.FileFolder));
            SidebarLibraryFolders.Add(new SidebarFolderViewModel(NavigationInfo.Navigation_Albums, SidebarFolderType.MusicFolder));
            SidebarLibraryFolders.Add(new SidebarFolderViewModel(NavigationInfo.Navigation_Artists, SidebarFolderType.FileFolder));
            SidebarLibraryFolders.Add(new SidebarFolderViewModel(NavigationInfo.Navigation_Uncategorized, SidebarFolderType.MusicFolder));

            //SidebarFavouriteFolders.Add(new SidebarFolderViewModel("Playlist1", SidebarFolderIconType.PlaylistFolderIcon, null!));
            //SidebarFavouriteFolders.Add(new SidebarFolderViewModel("Playlist2", SidebarFolderIconType.PlaylistFolderIcon, null!));
            //SidebarFavouriteFolders.Add(new SidebarFolderViewModel("Playlist3", SidebarFolderIconType.PlaylistFolderIcon, null!));

            LoadSubMenuCommand.Execute(null);
            ToggleShuffleCommand.Execute(false);

            UpdateSidebarFavouriteFolderAsync().FireAndForgetSafeAsync();


            //loadFileBgWorker.DoWork += FileLoaderDoWorkAsync;
            //loadFileBgWorker.ProgressChanged += FileLoaderProgressChanged;
            //loadFileBgWorker.RunWorkerCompleted += FileLoaderRunWorkerCompleted;

            void InitializeNavigator()
            {
                navigationHelper.Navigated += () =>
                {
                    //都有可能需要更新
                    OnPropertyChanged(nameof(AtRootDirectory));
                    OnPropertyChanged(nameof(CurrentDirectoryId));
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(CanGoForward));
                    OnPropertyChanged(nameof(NavigationPath));
                };

                //先导航到根目录
                var task = navigationHelper.NavigateToVirtualFolderAsync(new NavigationInfo(new FolderNode() { Dirname = "dummy", Id = 1}));
                DisplayFolderAndFileAsync(task.Result);
            }

            void InitializeMediaPlayer()
            {
                mediaPlayerHelper.PlayStateChanged += () =>
                {
                    OnPropertyChanged(nameof(IsPlayingSong));
                };
                mediaPlayerHelper.SongSwitched += () =>
                {
                    OnPropertyChanged(nameof(PlayingSongCover));
                    OnPropertyChanged(nameof(PlayingSong));
                    OnPropertyChanged(nameof(MediaLength));
                    MediaProgress = 0;
                    OnPropertyChanged(nameof(MediaProgress));

                };
                mediaPlayerHelper.TickUpdate += () =>
                {
                    if (!Dragging)
                    {
                        OnPropertyChanged(nameof(MediaProgress));
                    }
                };
            }

            DownloadManager.DownloadListAdded += (queue, added) =>
            {
                foreach (var item in added)
                {
                    //将UI更改保持在该线程：https://stackoverflow.com/questions/18331723/this-type-of-collectionview-does-not-support-changes-to-its-sourcecollection-fro
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadList.Add(new DownloadItemViewModel(item));
                    });
                }
            };

            DownloadManager.DownloadListRemoved += (queue, removed) =>
            {
                for (int i = DownloadList.Count; i >= 0; i--)
                {
                    if (removed.Contains(DownloadList[i].DownloadTask))
                    {
                        DownloadList.RemoveAt(i);
                    }
                }
            };
        }

        public string Title => "Jo音乐";

        /// <summary>
        /// 侧栏的子VM
        /// </summary>
        public ObservableCollection<SidebarFolderViewModel> SidebarLibraryFolders { get; set; } = new();
        public ObservableCollection<SidebarFolderViewModel> SidebarFavouriteFolders { get; set; } = new();
        public ObservableCollection<FileItemViewModel> NavigatedFolderFiles { get; set; } = new();
        public ObservableCollection<SubmenuButtonViewModel> OperationTabSubMenu { get; set; } = new();
        public ObservableCollection<SubmenuButtonViewModel> ResourceTabSubMenu { get; set; } = new();
        public ObservableCollection<SubmenuButtonViewModel> ViewTabSubMenu { get; set; } = new();

        public ObservableCollection<DownloadItemViewModel> DownloadList { get; set; } = new();

        public ObservableCollection<SongViewModel> Playlist => mediaPlayerHelper.Playlist;

        public ObservableCollection<long> PathHistoryCollection { get; set; } = new();

        public long CurrentDirectoryId => navigationHelper.CurrentDirectory.directoryId;

        public bool CanGoForward => navigationHelper.CanGoForward;
        public bool CanGoBack => navigationHelper.CanGoBack;
        public bool AtRootDirectory => navigationHelper.AtRootDirectory;
        public string NavigationPath => navigationHelper.NavigtionHierarchyPath;
        public bool IsPlayingSong { get => mediaPlayerHelper.IsPlaying; }
        public double Volume
        {
            get => mediaPlayerHelper.Volume;
            set => mediaPlayerHelper.Volume = value;
        }
        public bool IsMuted
        {
            get => mediaPlayerHelper.IsMuted;
            set => mediaPlayerHelper.IsMuted = value;
        }
        public SongViewModel? PlayingSong => mediaPlayerHelper.PlayingSong;

        public double MediaLength => mediaPlayerHelper.mediaTotalSeconds;

        public bool Dragging { get; set; }
        public double MediaProgress { get => mediaPlayerHelper.mediaProgressSeconds; set => mediaPlayerHelper.mediaProgressSeconds = value; }

        public BitmapImage? PlayingSongCover
        {
            get
            {
                if (PlayingSong == null)
                {
                    return FileItemViewModel.Images[3];
                }
                BitmapImage cover = new();
                cover.BeginInit();
                cover.StreamSource = MusicTagModifier.GetMusicCover(PlayingSong.FileName);
                cover.EndInit();
                return cover;
            }
        }

        public bool PlaylistActive
        {
            get => playlistActive;
            set
            {
                playlistActive = value;
                OnPropertyChanged(nameof(playlistActive));
            }
        }
        public bool WindowEnable
        {
            get => windowEnable;
            set
            {
                windowEnable = value;
                OnPropertyChanged(nameof(WindowEnable));
            }
        }


        private ICommand? loadSubMenuCommand;
        public ICommand LoadSubMenuCommand
        {
            get
            {
                if (loadSubMenuCommand == null)
                {
                    loadSubMenuCommand = new RelayCommand(
                        null,
                        p =>
                        {
                            OperationTabSubMenu = new ObservableCollection<SubmenuButtonViewModel>()
                            {
                                new SubmenuButtonViewModel()
                                {
                                    Name="设为喜欢",
                                    IconFont="\xE734",
                                    ButtonClick=AddFavouriteAsync
                                },                                
                                new SubmenuButtonViewModel()
                                {
                                    Name="播放",
                                    IconFont="\xE768",
                                    ButtonClick=UpdatePlaylistAsync
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="复制",
                                    IconFont="\xE8C8",
                                    ButtonClick=CopyAsync
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="剪切",
                                    IconFont="\xE8C6",
                                    ButtonClick=CutAsync

                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="粘贴",
                                    IconFont="\xE77F",
                                    ButtonClick=PasteAsync

                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="删除",
                                    IconFont="\xE74D",
                                    ButtonClick=DeleteAsync
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="重命名",
                                    IconFont="\xE8AC",
                                    ButtonClick=RenameAsync
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="新文件夹",
                                    IconFont="\xE8F4",
                                    ButtonClick=NewFolderAsync
                                },
                            };

                            ResourceTabSubMenu = new ObservableCollection<SubmenuButtonViewModel>()
                            {
                                new SubmenuButtonViewModel()
                                {
                                    Name="网易云",
                                    IconFont="\xE896",
                                    ButtonClick=NeteaseDownloadAsync
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="导入",
                                    IconFont="\xE8B6",
                                    ButtonClick=LocalImportAsync
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="导出",
                                    IconFont="\xEA53",
                                    ButtonClick=LocalExportAsync
                                },
                            };

                            ViewTabSubMenu = new ObservableCollection<SubmenuButtonViewModel>()
                            {
                                new SubmenuButtonViewModel()
                                {
                                    Name="List",
                                    IconFont="\xE8FD"
                                },
                                new SubmenuButtonViewModel()
                                {
                                    Name="Grid",
                                    IconFont="\xE8A9"
                                },
                            };
                        });
                }
                return loadSubMenuCommand;
            }
        }

        //private ICommand? loadFoldersCommand;
        //public ICommand LoadFoldersCommand
        //{
        //    get
        //    {
        //        if (loadFoldersCommand == null)
        //        {
        //            loadFoldersCommand = new RelayCommand(null,
        //            async p =>
        //            {
        //                if (p is not NavigatableObject folder)
        //                {
        //                    return;
        //                }
        //                if (folder.NavigationNode != null)
        //                {
        //                    DisplayFolderAndFileAsync(await navigationHelper.NavigateToVirtualFolderAsync(folder.NavigationNode));
        //                }
        //            });
        //        }
        //        return loadFoldersCommand;
        //    }
        //}

        private ICommand? openItemCommand;
        public ICommand OpenItemCommand
        {
            get
            {
                if (openItemCommand == null)
                {
                    openItemCommand = new RelayCommand(null,
                        async p =>
                        {
                            //是歌
                            if (p is FileItemViewModel file)
                            {
                                if (file.SongFile != null)
                                {
                                    await UpdatePlaylistAsync(new List<FileItemViewModel>() { file });
                                    return;
                                }
                            }

                            //是其它
                            if (p is NavigatableObject item)
                            {
                                if (item.NavigationNode != null)
                                {
                                    DisplayFolderAndFileAsync(await navigationHelper.NavigateToVirtualFolderAsync(item.NavigationNode));
                                }
                            }
                        });
                }
                return openItemCommand;
            }
        }

        private ICommand? closePlaylistCommand;
        public ICommand ClosePlaylistCommand
        {
            get
            {
                if (closePlaylistCommand == null)
                {
                    closePlaylistCommand = new RelayCommand(null,
                        p =>
                        {
                            PlaylistActive = false;
                        });
                }
                return closePlaylistCommand;
            }
        }

        private ICommand? goToPreviousDirectoryCommand;

        public ICommand GoToPreviousDirectoryCommand
        {
            get
            {
                if (goToPreviousDirectoryCommand == null)
                {
                    goToPreviousDirectoryCommand = new RelayCommand(null,
                        async p =>
                        {
                            DisplayFolderAndFileAsync(await navigationHelper.NavigateBackAsync());
                            //OnPropertyChanged(nameof(CanGoBack));
                        });
                }
                return goToPreviousDirectoryCommand;
            }
        }

        private ICommand? goToForwardDirectoryCommand;

        public ICommand GoToForwardDirectoryCommand
        {
            get
            {
                if (goToForwardDirectoryCommand == null)
                {
                    goToForwardDirectoryCommand = new RelayCommand(null,
                        async p =>
                        {
                            DisplayFolderAndFileAsync(await navigationHelper.NavigateForwardAsync());
                            //OnPropertyChanged(nameof(CanGoForward));
                        });
                }
                return goToForwardDirectoryCommand;
            }
        }

        private ICommand? goToParentDirectoryCommand;

        public ICommand GoToParentDirectoryCommand
        {
            get
            {
                if (goToParentDirectoryCommand == null)
                {
                    goToParentDirectoryCommand = new RelayCommand(null,
                        async p =>
                        {
                            DisplayFolderAndFileAsync(await navigationHelper.NavigateToParentAsync());
                        });
                }
                return goToParentDirectoryCommand;
            }
        }

        private ICommand? searchCommand;

        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                {
                    searchCommand = new RelayCommand(null,
                        async p =>
                        {
                            if (p is not string kw)
                            {
                                return;
                            }
                            NavigatedFolderFiles.Clear();

                            navigationHelper.SetToSearchDirectory();
                            OnPropertyChanged(nameof(NavigationPath));

                            foreach (var folder in await FileManager.FolderLikeSearch(kw, AppConfigManager.QueryMaximum))
                            {
                                NavigatedFolderFiles.Add(new FileItemViewModel(folder));
                            }

                            foreach (var file in await FileManager.SongWithArtistsLikeSearch(kw, AppConfigManager.QueryMaximum))
                            {
                                NavigatedFolderFiles.Add(new FileItemViewModel(file));
                            }
                        });
                }
                return searchCommand;
            }
        }

        private ICommand? switchPlayModeCommand;

        public ICommand SwitchPlayModeCommand
        {
            get
            {
                if (switchPlayModeCommand == null)
                {
                    switchPlayModeCommand = new RelayCommand(null,
                        p =>
                        {
                            System.Diagnostics.Debug.WriteLine(p.GetType());
                            if (p is not System.Windows.Controls.Primitives.ButtonBase toggleButton)
                            {
                                return;
                            }

                            var state = (string)toggleButton.Tag;

                            if (state == "RepeatOff")
                            {
                                toggleButton.Tag = "Repeat";
                                mediaPlayerHelper.PlayMode = PlayMode.AllRepeat;
                            }
                            else if (state == "Repeat")
                            {
                                toggleButton.Tag = "RepeatOnce";
                                mediaPlayerHelper.PlayMode = PlayMode.RepectOnce;
                            }
                            else
                            {
                                toggleButton.Tag = "RepeatOff";
                                mediaPlayerHelper.PlayMode = PlayMode.Order;
                            }
                        });
                }
                return switchPlayModeCommand;
            }
        }

        private ICommand? previousSongCommand;

        public ICommand PreviousSongCommand
        {
            get
            {
                if (previousSongCommand == null)
                {
                    previousSongCommand = new RelayCommand(null,
                        p =>
                        {
                            mediaPlayerHelper.Previous();
                        });
                }
                return previousSongCommand;
            }
        }


        private ICommand? playPauseCommand;

        public ICommand PlayPauseCommand
        {
            get
            {
                if (playPauseCommand == null)
                {
                    playPauseCommand = new RelayCommand(null,
                        p =>
                        {
                            if (p is not bool newState)
                            {
                                return;
                            }

                            if (newState)
                            {
                                mediaPlayerHelper.Play();
                            }
                            else
                            {
                                mediaPlayerHelper.Pause();
                            }
                        });
                }
                return playPauseCommand;
            }
        }


        private ICommand? nextSongCommand;

        public ICommand NextSongCommand
        {
            get
            {
                if (nextSongCommand == null)
                {
                    nextSongCommand = new RelayCommand(null,
                        p =>
                        {
                            mediaPlayerHelper.Next();
                        });
                }
                return nextSongCommand;
            }
        }

        private ICommand? toggleShuffleCommand;

        public ICommand ToggleShuffleCommand
        {
            get
            {
                if (toggleShuffleCommand == null)
                {
                    toggleShuffleCommand = new RelayCommand(null,
                        p =>
                        {
                            if (p is not bool isChecked)
                            {
                                return;
                            }

                            if (isChecked)
                            {
                                mediaPlayerHelper.ShuffleEnable = true;
                            }
                            else
                            {
                                mediaPlayerHelper.ShuffleEnable = false;
                            }
                        });
                }
                return toggleShuffleCommand;
            }
        }

        private ICommand? playlistLostFocusCommand;
        private bool playlistActive;

        public ICommand PlaylistLostFocusCommand
        {
            get
            {
                if (playlistLostFocusCommand == null)
                {
                    playlistLostFocusCommand = new RelayCommand(null,
                        p =>
                        {
                            PlaylistActive = false;
                        });
                }
                return playlistLostFocusCommand;
            }
        }

        private ICommand? removePlaylistSongsCommand;

        public ICommand RemovePlaylistSongsCommand
        {
            get
            {
                if (removePlaylistSongsCommand == null)
                {
                    removePlaylistSongsCommand = new RelayCommand(null,
                        p =>
                        {
                            if (p is not System.Collections.IList fileViews)
                            {
                                return;
                            }
                            mediaPlayerHelper.RemoveSong(fileViews.Cast<SongViewModel>());
                        });
                }
                return removePlaylistSongsCommand;
            }
        }


        //private async Task DisplayFolderAndFileAsync(List<FolderNode>? folders)
        //{
        //    NavigatedFolderFiles.Clear();
        //    if (folders == null)
        //    {
        //        return;
        //    }
        //    foreach (var folder in folders)
        //    {
        //        NavigatedFolderFiles.Add(new FileItemViewModel(folder));
        //    }

        //    var songMetas = await FileManager.GetSongsWitArtistsData(CurrentDirectory);
        //    foreach (var meta in songMetas)
        //    {
        //        NavigatedFolderFiles.Add(new FileItemViewModel(meta));
        //    }
        //}

        private void DisplayFolderAndFileAsync(List<FileItemViewModel>? folders)
        {
            if (folders != null)
            {
                NavigatedFolderFiles.Clear();

                foreach (var item in folders)
                {
                    NavigatedFolderFiles.Add(item);
                }

            }
        }

        private async Task UpdateSidebarFavouriteFolderAsync()
        {
            SidebarFavouriteFolders.Clear();

            //var favFolders = await FileManager.GetFavouritePlaylists();

            //foreach (var favFolder in favFolders)
            //{
            //    SidebarFavouriteFolders.Add(new SidebarFolderViewModel(favFolder) { PlayButtonClick = UpdatePlaylistAsync, UnpinButtonClick = RemoveFavouriteAsync });
            //}

            var playlistNavInfo = await navigationHelper.GetPlaylistNavigationInfo();

            foreach (var info in playlistNavInfo)
            {
                SidebarFavouriteFolders.Add(new(info) { PlayButtonClick = UpdatePlaylistAsync, UnpinButtonClick = RemoveFavouriteAsync });
            }
        }

        private async Task AddFavouriteAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            IEnumerable<PlayList> playlists = from item in selectedItems where item.NavigationNode != null select item.NavigationNode!.GetPlaylistNode();

            if (playlists.Any())
            {
                await FileManager.AddFavouritePlaylists(playlists);

                await UpdateSidebarFavouriteFolderAsync();
            }
        }

        private Task CopyAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(CopyTask);

            void CopyTask()
            {
                clipboardState = ClipboardState.Copy;
                ChangeClipBoard(selectedItems);
            }

        }

        private Task CutAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(CutTask);

            void CutTask()
            {
                clipboardState = ClipboardState.Cut;
                cutDirectory = CurrentDirectoryId;
                ChangeClipBoard(selectedItems);
            }
        }

        private void ChangeClipBoard(IEnumerable<FileItemViewModel> selectedItems)
        {
            //FolderClipboard.Clear();
            //FileClipboard.Clear();

            //foreach (var item in selectedItems)
            //{
            //    if (item.Folder != null)
            //    {
            //        FolderClipboard.Add(item.Folder);
            //    }
            //    else if (item.SongFile != null)
            //    {
            //        FileClipboard.Add(item.SongFile);
            //    }
            //}

            //SelectFoldersAndFile(selectedItems, FolderClipboard, FileClipboard);

            clipboardHelper.Clear();
            clipboardHelper.SelectItems(selectedItems);
        }


        private async Task PasteAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            //if (FolderClipboard.Count == 0 && FileClipboard.Count == 0)
            //{
            //    return;
            //}
            switch (clipboardState)
            {
                case ClipboardState.None:
                    return;
                case ClipboardState.Cut:
                    if (clipboardHelper.CutAny)
                    {
                        //剪切文件夹
                        foreach (var item in clipboardHelper.SelectedFolders)
                        {
                            await FileManager.CutVirtualFolderRecursively(item.Id, CurrentDirectoryId);
                        }

                        //剪切文件
                        if (clipboardHelper.SelectedFiles.Count > 0)
                        {
                            await FileManager.CutMetaData(clipboardHelper.SelectedFiles, cutDirectory, CurrentDirectoryId);
                        }

                        UpdateFolderNode();
                        clipboardState = ClipboardState.None;
                    }
                    break;
                case ClipboardState.Copy:
                    if (clipboardHelper.CopyAny)
                    {
                        ////复制文件夹
                        //foreach (var item in clipboardHelper.SelectedFolders)
                        //{
                        //    await FileManager.CopyVirtualFolderRecursively(item.Id, CurrentDirectoryId);
                        //}

                        ////复制文件
                        //if (clipboardHelper.SelectedFiles.Count > 0)
                        //{
                        //    await FileManager.RecordMetaData(clipboardHelper.SelectedFiles, CurrentDirectoryId);
                        //}


                        ////递归生成特殊文件夹
                        //foreach (var item in clipboardHelper.SelectedSpecials)
                        //{
                        //    await FileManager.NewFolder(new FolderCreate() { Ancestor = CurrentDirectoryId, Dirname = item.DisplayName }, true);
                        //}
                        //没有办法单次查询搞定，出此下策
                        var tempClipboard = new ClipboardHelper();

                        async Task CopyNormalFolderAndFIles(ClipboardHelper clipboard, long targetDirectoryId)
                        {
                            //复制文件夹
                            foreach (var item in clipboard.SelectedFolders)
                            {
                                await FileManager.CopyVirtualFolderRecursively(item.Id, targetDirectoryId);
                            }

                            //复制文件
                            if (clipboard.SelectedFiles.Count > 0)
                            {
                                await FileManager.RecordMetaData(clipboard.SelectedFiles, targetDirectoryId);
                            }

                            //递归生成特殊文件夹
                            foreach (var item in clipboard.SelectedSpecials)
                            {
                                //生成文件夹
                                var newId = await FileManager.NewFolderWithReturning(new FolderCreate() { Ancestor = targetDirectoryId, Dirname = item.DisplayName });

                                //获取该目录下的文件，递归复制
                                tempClipboard.Clear();
                                tempClipboard.SelectItems(await navigationHelper.GetDirectoryAndFileAsync(item));
                                if (tempClipboard.CopyAny)
                                {
                                    await CopyNormalFolderAndFIles(tempClipboard, newId);
                                }
                            }
                        }

                        await CopyNormalFolderAndFIles(clipboardHelper, CurrentDirectoryId);


                        UpdateFolderNode();
                    }
                    break;
                default:
                    break;
            }


        }

        private Task DeleteAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(DeleteTask);
           
            void DeleteTask()
            {
                List<FolderNode> nodesToDelete = new();
                List<SongFileMetum> songsToDelete = new();

                //foreach (var item in selectedItems)
                //{
                //    if (item.Folder != null)
                //    {
                //        nodesToDelete.Add(item.Folder);
                //    }
                //    else if (item.SongFile != null)
                //    {
                //        SongsToDelete.Add(item.SongFile);
                //    }
                //}

                SelectFoldersAndFile(selectedItems, nodesToDelete, songsToDelete);

                if (nodesToDelete.Count == 0 && songsToDelete.Count == 0)
                {
                    return;
                }

                //foreach (var item in nodesToDelete)
                //{
                //    await FileManager.DeleteVirtualFolderRecursively(item.Id);
                //}

                //if (SongsToDelete.Count > 0)
                //{
                //    await FileManager.DeleteFileNodes(SongsToDelete, CurrentDirectoryId);
                //}

                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEnable = false;
                    var deleteViewModel = new DeleteDialogueViewModel(nodesToDelete, songsToDelete, CurrentDirectoryId);
                    var dialogue = new DeleteDialogue(deleteViewModel)
                    {
                        Owner = Application.Current.MainWindow,
                        ShowInTaskbar = false,
                        Topmost = true,
                    };
                    deleteViewModel.DialogueCommitted += Commited;
                    dialogue.Closing += Dialogue_Closed;
                    dialogue.ShowDialog();

                    void Commited()
                    {
                        dialogue?.Close();
                        UpdateFolderNode();

                    }
                });

            }
        }

        private Task RenameAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(RenameTask);

            void RenameTask()
            {
                if (!selectedItems.Any())
                {
                    return;
                }
                var target = selectedItems.First().Folder;

                //文件夹
                if (target != null)
                {
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    WindowEnable = false;
                    //    var dialogue = new RenameDialogue(new RenameDialogueViewModel(target, UpdateFolderNode))
                    //    {
                    //        Owner = Application.Current.MainWindow,
                    //        ShowInTaskbar = true,
                    //        Topmost = true,
                    //    };
                    //    dialogue.Closing += Dialogue_Closed;
                    //    dialogue.ShowDialog();
                    //});
                    var renameViewModel = new RenameDialogueViewModel(target);
                    renameViewModel.DialogueCommitted += UpdateFolderNode;
                    InitializeRename(renameViewModel);

                    return;
                }

                var song = selectedItems.First().SongFile;

                if (song != null)
                {
                    var renameViewModel = new RenameDialogueViewModel(song);
                    renameViewModel.DialogueCommitted += UpdateFolderNode;
                    InitializeRename(renameViewModel);
                    return;
                }

            }

            void InitializeRename(RenameDialogueViewModel dialogueViewModel)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEnable = false;

                    var dialogue = new RenameDialogue(dialogueViewModel)
                    {
                        Owner = Application.Current.MainWindow,
                        ShowInTaskbar = true,
                        Topmost = true,
                    };

                    dialogue.Closing += Dialogue_Closed;
                    dialogue.ShowDialog();
                });
            }
        }

        private Task NewFolderAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(RenameTask);

            void RenameTask()
            {
                FolderCreate target = new() { Ancestor = CurrentDirectoryId, Dirname = "" };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEnable = false;
                    var newViewModel = new NewDialogueViewModel(target);
                    newViewModel.DialogueCommitted += UpdateFolderNode;
                    var dialogue = new RenameDialogue(newViewModel)
                    {
                        Owner = Application.Current.MainWindow,
                        ShowInTaskbar = false,
                        Topmost = true,
                    };
                    dialogue.Closing += Dialogue_Closed;
                    dialogue.ShowDialog();
                });
            }
        }

        private Task NeteaseDownloadAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(DwonloadTask);

            void DwonloadTask()
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEnable = false;
                    var dialogue = new DownloadDialogue(new DownloadDialogueViewModel())
                    {
                        Owner = Application.Current.MainWindow,
                        ShowInTaskbar = false,
                        Topmost = true,
                    };
                    dialogue.Closing += Dialogue_Closed;
                    dialogue.ShowDialog();
                });
            }
        }

        private Task LocalImportAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(DwonloadTask);

            void DwonloadTask()
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEnable = false;
                    var dialogue = new ImportDialogue(new ImportDialogueViewModel())
                    {
                        Owner = Application.Current.MainWindow,
                        ShowInTaskbar = false,
                        Topmost = true,
                    };
                    dialogue.Closing += Dialogue_Closed;
                    dialogue.ShowDialog();
                });
            }
        }

        private Task LocalExportAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            return Task.Run(DwonloadTask);

            void DwonloadTask()
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    var itemsInList = selectedItems.ToList();
                    //选择了任意项
                    if (itemsInList != null && itemsInList.Count > 0)
                    {
                        //选择了一项
                        if (itemsInList.Count == 1)
                        {
                            var item = itemsInList[0];
                            if (item != null && item.Folder != null)
                            {
                                string[] path = await FileManager.GetNavigationPath(item.Folder.Id);
                                WindowEnable = false;
                                var dialogue = new ImportDialogue(new ExportDialogueViewModel(item.Folder, path))
                                {
                                    Owner = Application.Current.MainWindow,
                                    ShowInTaskbar = false,
                                    Topmost = true,
                                };
                                dialogue.Closing += Dialogue_Closed;
                                dialogue.ShowDialog();
                            }
                        }
                        //选择了多于一项
                        else
                        {
                            string path = $"共选择{itemsInList.Count}个文件夹";

                            WindowEnable = false;
                            var dialogue = new ImportDialogue(new ExportDialogueViewModel(from item in itemsInList where item.Folder != null select item.Folder, path))
                            {
                                Owner = Application.Current.MainWindow,
                                ShowInTaskbar = false,
                                Topmost = true,
                            };
                            dialogue.Closing += Dialogue_Closed;
                            dialogue.ShowDialog();
                        }
                    }

                });
            }
        }

        async void UpdateFolderNode()
        {
            DisplayFolderAndFileAsync(await navigationHelper.UpdateDirectoryAsync());
            await UpdateSidebarFavouriteFolderAsync();
        }

        void Dialogue_Closed(object? sender, EventArgs e)
        {
            WindowEnable = true;
        }

        /// <summary>
        /// 后面两个用于接受数据，需要先传入初始化过的列表
        /// </summary>
        /// <param name="items"></param>
        /// <param name="folderContainer"></param>
        /// <param name="fileMetaContainer"></param>
        void SelectFoldersAndFile(IEnumerable<FileItemViewModel> items, List<FolderNode> folderContainer, List<SongFileMetum> fileMetaContainer)
        {
            foreach (var item in items)
            {
                if (item.Folder != null)
                {
                    folderContainer.Add(item.Folder);
                }
                else if (item.SongFile != null)
                {
                    fileMetaContainer.Add(item.SongFile);
                }
                else if (item.NavigationNode != null)
                {

                }

            }
        }

        async Task UpdatePlaylistAsync(IEnumerable<FileItemViewModel> selectedItems)
        {
            List<NavigationInfo> directory = new();
            List<SongFileMetum> files = new();
            foreach (var item in selectedItems)
            {
                if (item.NavigationNode != null)
                {
                    directory.Add(item.NavigationNode);
                }
                else if (item.SongFile != null)
                {
                    files.Add(item.SongFile);
                }
            }

            await UpdatePlaylistAsync(directory, files);
        }

        async Task UpdatePlaylistAsync(IEnumerable<SidebarFolderViewModel> selectedItems)
        {
            List<NavigationInfo> directory = new();
            List<SongFileMetum> files = new();
            foreach (var item in selectedItems)
            {
                if (item.NavigationNode != null && item.NavigationNode.DirectoryType != DirectoryType.None)
                {
                    directory.Add(item.NavigationNode);
                }
            }

            await UpdatePlaylistAsync(directory, files);
        }

        public async Task UpdatePlaylistAsync(List<NavigationInfo> navigationNode, List<SongFileMetum> files)
        {
            //播放歌单：清空、加入、重头播放
            if (navigationNode.Count > 0)
            {
                mediaPlayerHelper.ClearSong();

                foreach (var folder in navigationNode)
                {
                    var songs = await navigationHelper.GetSongsOfDirectoryAsync(folder);
                    if (songs != null)
                    {
                        mediaPlayerHelper.AddRangeSong(songs);
                    }
                }

                mediaPlayerHelper.AddRangeSong(files);

                mediaPlayerHelper.ResetPlayingPosition();
                mediaPlayerHelper.Next();
            }
            //只有单曲：插入，下一首播放
            else if (files.Count > 0)
            {
                mediaPlayerHelper.InsertRangeSong(files);
                mediaPlayerHelper.Next();
            }
            //否则不播放
        }

        private async Task RemoveFavouriteAsync(IEnumerable<SidebarFolderViewModel> selectedItems)
        {
            await FileManager.RemoveFavouritePlaylists(from item in selectedItems where item.NavigationNode != null select item.NavigationNode!.GetPlaylistNode());

            await UpdateSidebarFavouriteFolderAsync();
        }

        public void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //保存播放音量
            AppConfigManager.Volume = mediaPlayerHelper.Volume;
        }
    }
}
