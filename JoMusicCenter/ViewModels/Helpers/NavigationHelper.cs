using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    public enum DirectoryType
    {
        None = 0,
        Search = -1,
        Artists = -2,
        Albums = -3,
        Uncategorized = -4,
        ArtistDetails = -5,
        AlbumDetails = -6,
        Folder = -7
    }

    public class NavigationInfo: IEquatable<NavigationInfo>
    {
        private static readonly NavigationInfo navigation_None = new NavigationInfo(DirectoryType.None, "文件夹无效或已删除");
        private static readonly NavigationInfo navigation_Search = new NavigationInfo(DirectoryType.Search, "搜索结果");
        private static readonly NavigationInfo navigation_Artists = new NavigationInfo(DirectoryType.Artists, "艺术家");
        private static readonly NavigationInfo navigation_Albums = new NavigationInfo(DirectoryType.Albums, "专辑");
        private static readonly NavigationInfo navigation_Uncategorized = new NavigationInfo(DirectoryType.Uncategorized, "未分类");


        public static NavigationInfo Navigation_None => navigation_None;
        public static NavigationInfo Navigation_Search => navigation_Search;
        public static NavigationInfo Navigation_Artists => navigation_Artists;
        public static NavigationInfo Navigation_Albums => navigation_Albums;
        public static NavigationInfo Navigation_Uncategorized => navigation_Uncategorized;


        public FolderNode? FolderNode { get; }
        public long directoryId => FolderNode?.Id ?? -1;
        public DirectoryType DirectoryType { get; }
        //public bool IsSpecialDirectory => folderNode == null;

        //暂时只需要一个额外的字符串
        public string NavInfo { get; set; }

        /// <summary>
        /// 展示名：
        /// 如果是文件夹，则是文件夹名(Dirname)
        /// 否则，是导航的附加信息(NavInfo)
        /// </summary>
        public string DisplayName => FolderNode?.Dirname ?? NavInfo;

        /// <summary>
        /// 从文件夹结点获取导航信息
        /// </summary>
        /// <param name="folderNode"></param>
        public NavigationInfo(FolderNode folderNode)
        {
            DirectoryType = DirectoryType.Folder;
            FolderNode = folderNode;
            NavInfo = string.Empty;


            //this.folderNode = folderNode;
        }

        /// <summary>
        /// 指定特殊文件夹以指派导航信息
        /// </summary>
        /// <param name="specialDirectory"></param>
        private NavigationInfo(DirectoryType specialDirectory)
        {
            if (specialDirectory == DirectoryType.Folder)
            {
                throw new ArgumentException("类型不能为普通Folder，请使用FolderNode参数构造导航对象");
            }
            DirectoryType = specialDirectory;
            NavInfo = string.Empty;
        }

        /// <summary>
        /// 指定特殊文件夹以指派导航信息
        /// </summary>
        /// <param name="specialDirectory"></param>
        /// <param name="additionInfo">额外的导航信息</param>
        public NavigationInfo(DirectoryType specialDirectory, string additionInfo)
        {
            if (specialDirectory == DirectoryType.Folder)
            {
                throw new ArgumentException("类型不能为普通Folder，请使用FolderNode参数构造导航对象");
            }
            DirectoryType = specialDirectory;
            NavInfo = additionInfo;
        }

        /// <summary>
        /// 从歌单结点获取导航信息
        /// </summary>
        /// <param name="playList"></param>
        //public NavigationInfo(PlayList playList)
        //{
        //    try
        //    {
        //        DirectoryType = Enum.Parse<DirectoryType>(playList.NavType);
        //    }
        //    catch(ArgumentException)
        //    {
        //        System.Diagnostics.Debug.WriteLine("未知的歌单类型");
        //        DirectoryType = DirectoryType.None;
        //    }
        //    NavInfo = playList.NavValue;
        //}

        /// <summary>
        /// 获取一个记录了该结点完整导航信息的Playlist实例
        /// 可用于持久化保存为歌单
        /// </summary>
        /// <returns></returns>
        public PlayList GetPlaylistNode()
        {
            if (FolderNode != null)
            {
                return new PlayList() { NavType = DirectoryType.ToString(), NavValue = FolderNode.Id.ToString() };
            }
            else
            {
                return new PlayList() { NavType = DirectoryType.ToString(), NavValue = NavInfo };
            }
        }
        

        #region equals
        //References:
        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type
        public override bool Equals(object? obj) => this.Equals(obj as NavigationInfo);

        public bool Equals(NavigationInfo? p)
        {
            if (p is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return DirectoryType == p.DirectoryType && NavInfo == p.NavInfo;
        }

        public override int GetHashCode() => (DirectoryType.ToString() + NavInfo).GetHashCode();

        public static bool operator ==(NavigationInfo? lhs, NavigationInfo? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(NavigationInfo? lhs, NavigationInfo? rhs) => !(lhs == rhs);
        #endregion
    }

    public class NavigationHelper
    {



        public bool CanGoForward => navigatePosition < PathHistoryCollection1.Count - 1;
        public bool CanGoBack => navigatePosition > 0;
        public bool AtRootDirectory => ParentId.DirectoryType == DirectoryType.None;
        public NavigationInfo CurrentDirectory { get; set; } = null!;
        private NavigationInfo ParentId { get; set; } = NavigationInfo.Navigation_None;

        private int navigatePosition = -1;
        private string navigtionHierarchyPath = "";

        public delegate void NavigateEventHandler();

        public event NavigateEventHandler? Navigated;

        public event NavigateEventHandler? NavigatedBack;

        public event NavigateEventHandler? NavigatedForward;

        //private List<long> PathHistoryCollection { get; set; } = new();
        private List<NavigationInfo> PathHistoryCollection1 { get; set; } = new();

        public string NavigtionHierarchyPath => CurrentDirectory.DirectoryType == DirectoryType.Folder ? navigtionHierarchyPath : CurrentDirectory.DirectoryType.ToString();


        //public async Task<List<FolderNode>?> NavigateToVirtualFolderAsync(FolderBaseViewModel folder)
        //{
        //    List<FolderNode>? folders = null;
        //    if (folder.SpecialLoadLogic != null)
        //    {
        //        folders = await folder.SpecialLoadLogic();
        //    }
        //    else if (folder.Id > 0)
        //    {
        //        folders = await NavigationManagementAsync(folder.Id);
        //    }
        //    return folders;
        //}

        //public async Task<List<FolderNode>?> NavigateToVirtualFolderAsync(SpecialDirectory directory)
        //{
        //    if (directory != SpecialDirectory.Search)
        //    {
        //        return await NavigationManagementAsync((long)directory);
        //    }
        //    return null;
        //}

        //public async Task<List<FolderNode>?> NavigateToVirtualFolderAsync(long targetId)
        //{
        //    if (targetId > 0)
        //    {
        //        return await NavigationManagementAsync(targetId);
        //    }
        //    return null;
        //}

        public async Task<List<NavigationInfo>> GetPlaylistNavigationInfo()
        {
            var playlists = await FileManager.GetFavouritePlaylists();
            List<PlayList> invalidPlaylists = new();

            //一次查询获取所有folder信息
            var playlistFolders = (await FileManager.GetFolders(from playlist in playlists where playlist.NavType == DirectoryType.Folder.ToString() select long.Parse(playlist.NavValue))).ToDictionary(x => x.Id.ToString());

            List<NavigationInfo> playlistNavInfo = new();
            //保持原有顺序添加
            foreach (var playlist in playlists)
            {
                if (playlist.NavType == DirectoryType.Folder.ToString())
                {
                    if (playlistFolders.ContainsKey(playlist.NavValue))
                    {
                        playlistNavInfo.Add(new(playlistFolders[playlist.NavValue]));
                    }
                    else
                    {
                        invalidPlaylists.Add(playlist);
                        //playlistNavInfo.Add(NavigationInfo.Navigation_None);
                    }
                }
                else
                {
                    if (Enum.TryParse(playlist.NavType, out DirectoryType result))
                    {
                        playlistNavInfo.Add(new(result, playlist.NavValue));
                    }
                    else
                    {
                        invalidPlaylists.Add(playlist);
                        //playlistNavInfo.Add(NavigationInfo.Navigation_None);
                    }
                }
            }

            if (invalidPlaylists.Count > 0)
            {
                await FileManager.RemoveFavouritePlaylists(invalidPlaylists);
            }

            return playlistNavInfo;
        }

        public async Task<List<FileItemViewModel>> NavigateToVirtualFolderAsync(NavigationInfo targetNavInfo)
        {
            navigatePosition++;
            while (navigatePosition < PathHistoryCollection1.Count)
            {
                PathHistoryCollection1.RemoveAt(PathHistoryCollection1.Count - 1);
            }
            PathHistoryCollection1.Add(targetNavInfo);

            return await LoadDirectoryAndFileAsync(targetNavInfo);
        }

        //private async Task<List<FolderNode>?> NavigationManagementAsync(long targetId)
        //{
        //    navigatePosition++;
        //    while (navigatePosition < PathHistoryCollection.Count)
        //    {
        //        PathHistoryCollection.RemoveAt(PathHistoryCollection.Count - 1);
        //    }
        //    PathHistoryCollection.Add(targetId);

        //    switch (targetId)
        //    {
        //        case (long)SpecialDirectory.Artists:
        //            break;
        //        case (long)SpecialDirectory.Albums:
        //            break;
        //        case (long)SpecialDirectory.Uncategorized:
        //        default:
        //            return await LoadDirectoryAsync(targetId);
        //    }

        //    //return await LoadDirectoryAsync(targetId);
        //}

        //public async Task<List<FolderNode>?> NavigateBackAsync()
        //{
        //    if (CanGoBack)
        //    {
        //        navigatePosition--;
        //        var result = await LoadDirectoryAsync(PathHistoryCollection1[navigatePosition]);
        //        NavigatedBack?.Invoke();
        //        return result;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public async Task<List<FileItemViewModel>?> NavigateBackAsync()
        {
            if (CanGoBack)
            {
                navigatePosition--;
                var result = await LoadDirectoryAndFileAsync(PathHistoryCollection1[navigatePosition]);
                NavigatedBack?.Invoke();
                return result;
            }
            else
            {
                return null;
            }
        }

        //public async Task<List<FolderNode>?> NavigateForwardAsync()
        //{
        //    if (CanGoForward)
        //    {
        //        navigatePosition++;
        //        var result = await LoadDirectoryAsync(PathHistoryCollection1[navigatePosition]);
        //        NavigatedForward?.Invoke();
        //        return result;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public async Task<List<FileItemViewModel>?> NavigateForwardAsync()
        {
            if (CanGoForward)
            {
                navigatePosition++;
                var result = await LoadDirectoryAndFileAsync(PathHistoryCollection1[navigatePosition]);
                NavigatedForward?.Invoke();
                return result;
            }
            else
            {
                return null;
            }
        }

        //public async Task<List<FolderNode>?> NavigateToParentAsync()
        //{
        //    if (ParentId > 0)
        //    {
        //        var result = await NavigationManagementAsync(ParentId);
        //        return result;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public async Task<ObservableCollection<FileItemViewModel>?> NavigateToParentAsync()
        //{
        //    if (ParentId > 0)
        //    {
        //        var result = await NavigationManagementAsync(ParentId);
        //        return result;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        public async Task<List<FileItemViewModel>?> NavigateToParentAsync()
        {
            if (ParentId.DirectoryType == DirectoryType.Folder)
            {
                var result = await NavigateToVirtualFolderAsync(ParentId);
                return result;
            }
            else
            {
                return null;
            }
        }

        private async Task UpdateNavigationLocation(NavigationInfo targetNavInfo)
        {
            CurrentDirectory = targetNavInfo;

            var parentNode = await FileManager.GetParentFolders(CurrentDirectory.directoryId);
            ParentId = parentNode == null ? NavigationInfo.Navigation_None : new(parentNode);

            navigtionHierarchyPath = await GetNavigationPath();
            Navigated?.Invoke();
        }

        /// <summary>
        /// 获取给定导航点下的所有歌曲
        /// </summary>
        public async Task<List<SongFileMetum>?> GetSongsOfDirectoryAsync(NavigationInfo targetNavInfo)
        {
            List<SongFileMetum>? songs = null;
            switch (targetNavInfo.DirectoryType)
            {
                case DirectoryType.Uncategorized:
                    songs = await FileManager.SongsWithArtistsNotCategorized(AppConfigManager.QueryMaximum);
                    break;
                case DirectoryType.ArtistDetails:
                    songs = await FileManager.SongsWithArtistsOfArtist(targetNavInfo.NavInfo); 
                    break;
                case DirectoryType.AlbumDetails:
                    songs = await FileManager.SongsWithArtistsOfAlbum(targetNavInfo.NavInfo);
                    break;
                case DirectoryType.Folder:
                    songs = await FileManager.GetSongsWithArtistsData(targetNavInfo.directoryId);
                    break;
                default:
                    break;
            }
            return songs;
        }

        /// <summary>
        /// 只加载目录文件夹的对象
        /// </summary>
        /// <param name="navigationInfo"></param>
        /// <returns></returns>
        private async Task<List<FileItemViewModel>> LoadDirectoryOnlyAsync(NavigationInfo navigationInfo)
        {
            List<FileItemViewModel> fileItems = new List<FileItemViewModel>();
            switch (navigationInfo.DirectoryType)
            {
                //所有歌手
                case DirectoryType.Artists:
                    var artistNames = await FileManager.NamesLikeSearch(SearchType.ArtistName, null, AppConfigManager.QueryMaximum);
                    foreach (var artistName in artistNames)
                    {
                        //子菜单全是特殊文件夹
                        fileItems.Add(new FileItemViewModel(new NavigationInfo(DirectoryType.ArtistDetails, artistName)));
                    }
                    break;
                //所有专辑
                case DirectoryType.Albums:
                    var albumNames = await FileManager.NamesLikeSearch(SearchType.AlbumName, null, AppConfigManager.QueryMaximum);
                    foreach (var albumName in albumNames)
                    {
                        //子菜单全是特殊文件夹
                        fileItems.Add(new FileItemViewModel(new NavigationInfo(DirectoryType.AlbumDetails, albumName)));
                    }
                    break;
                //普通文件夹
                case DirectoryType.Folder:
                    var folders = await FileManager.GetDirectSubFolders(CurrentDirectory.directoryId);

                    if (folders != null)
                    {
                        //加入文件夹
                        foreach (var folder in folders)
                        {
                            fileItems.Add(new FileItemViewModel(folder));
                        }
                    }
                    break;
            }
            return fileItems;
        }

        /// <summary>
        /// 加载完整的目录内容（包括文件夹和歌曲）
        /// 直接调用该函数不会改变导航历史，也不会更变导航位置
        /// </summary>
        /// <param name="targetNavInfo"></param>
        /// <returns></returns>
        public async Task<List<FileItemViewModel>> GetDirectoryAndFileAsync(NavigationInfo targetNavInfo)
        {
            //获取文件夹
            var fileItems = await LoadDirectoryOnlyAsync(targetNavInfo);

            //加入文件
            var songs = await GetSongsOfDirectoryAsync(targetNavInfo);
            if (songs != null)
            {
                foreach (var song in songs)
                {
                    fileItems.Add(new FileItemViewModel(song));
                }
            }

            return fileItems;
        }

        private async Task<List<FileItemViewModel>> LoadDirectoryAndFileAsync(NavigationInfo targetNavInfo)
        {
            await UpdateNavigationLocation(targetNavInfo);



            //List<FileItemViewModel> fileItems = new();

            //var folders = await FileManager.GetDirectSubFolders(CurrentDirectory.directoryId);

            //if (folders == null)
            //{
            //    return fileItems;
            //}
            ////加入文件夹
            //foreach (var folder in folders)
            //{
            //    fileItems.Add(new FileItemViewModel(folder));
            //}

            ////加入文件
            //var songMetas = await FileManager.GetSongsWitArtistsData(CurrentDirectory.directoryId);
            //foreach (var meta in songMetas)
            //{
            //    fileItems.Add(new FileItemViewModel(meta));
            //}
            //if (targetNavInfo.IsSpecialDirectory)
            //{



            //switch (targetNavInfo.DirectoryType)
            //{
            //    //所有歌手
            //    case DirectoryType.Artists:
            //        {
            //            var artistNames = await FileManager.NamesLikeSearch(SearchType.ArtistName);
            //            foreach (var artistName in artistNames)
            //            {
            //                //子菜单全是特殊文件夹
            //                fileItems.Add(new FileItemViewModel(new NavigationInfo(DirectoryType.ArtistDetails, artistName)));
            //            }
            //            break;
            //        }
            //    //所有专辑
            //    case DirectoryType.Albums:
            //        {
            //            var albumNames = await FileManager.NamesLikeSearch(SearchType.AlbumName);
            //            foreach (var albumName in albumNames)
            //            {
            //                //子菜单全是特殊文件夹
            //                fileItems.Add(new FileItemViewModel(new NavigationInfo(DirectoryType.AlbumDetails, albumName)));
            //            }
            //            break;
            //        }
            //    case DirectoryType.Uncategorized:
            //        {
            //            var songs = await FileManager.SongsWithArtistsNotCategorized();
            //            foreach (var song in songs)
            //            {
            //                fileItems.Add(new FileItemViewModel(song));
            //            }
            //        }
            //        break;
            //    //一个歌手的详细
            //    case DirectoryType.ArtistDetails:
            //        {
            //            var songs = await FileManager.SongsWithArtistsOfArtist(targetNavInfo.NavInfo);
            //            foreach (var song in songs)
            //            {
            //                //子菜单全是歌曲文件夹
            //                fileItems.Add(new FileItemViewModel(song));
            //            }
            //            break;
            //        }
            //    //一个专辑的详细
            //    case DirectoryType.AlbumDetails:
            //        {
            //            var songs = await FileManager.SongsWithArtistsOfAlbum(targetNavInfo.NavInfo);
            //            foreach (var song in songs)
            //            {
            //                //子菜单全是歌曲文件夹
            //                fileItems.Add(new FileItemViewModel(song));
            //            }
            //            break;
            //        }
            //    //普通文件夹
            //    case DirectoryType.Folder:
            //        {

            //            var folders = await FileManager.GetDirectSubFolders(CurrentDirectory.directoryId);

            //            if (folders == null)
            //            {
            //                return fileItems;
            //            }
            //            //加入文件夹
            //            foreach (var folder in folders)
            //            {
            //                fileItems.Add(new FileItemViewModel(folder));
            //            }

            //            //加入文件
            //            var songMetas = await FileManager.GetSongsWithArtistsData(CurrentDirectory.directoryId);
            //            foreach (var meta in songMetas)
            //            {
            //                fileItems.Add(new FileItemViewModel(meta));
            //            }
            //            break;
            //        }
            //    default:
            //        break;

            //}



            //await UpdateNavigationLocation(targetNavInfo);
            //}
            ////普通文件夹
            //else
            //{

            //    var folders = await FileManager.GetDirectSubFolders(CurrentDirectory.directoryId);

            //    if (folders == null)
            //    {
            //        return fileItems;
            //    }
            //    //加入文件夹
            //    foreach (var folder in folders)
            //    {
            //        fileItems.Add(new FileItemViewModel(folder));
            //    }

            //    //加入文件
            //    var songMetas = await FileManager.GetSongsWithArtistsData(CurrentDirectory.directoryId);
            //    foreach (var meta in songMetas)
            //    {
            //        fileItems.Add(new FileItemViewModel(meta));
            //    }
            //}

            return await GetDirectoryAndFileAsync(targetNavInfo);
        }

        //private async Task<List<FolderNode>?> LoadDirectoryAsync(long directoryId)
        //{
        //    CurrentDirectory = directoryId;
        //    ParentId = (await FileManager.GetParentFolders(directoryId))?.Id ?? -1;

        //    navigtionHierarchyPath = await GetNavigationPath();
        //    Navigated?.Invoke();

        //    return await FileManager.GetDirectSubFolders(directoryId);
        //}
        private async Task<string> GetNavigationPath()
        {
            return string.Join(@"\", await FileManager.GetNavigationPath(CurrentDirectory.directoryId));

            //return string.Join(@"\", from node in await FileManager.GetHierarchicalLocation(CurrentDirectory.directoryId) select node.Dirname);
        }

        public async Task<List<FileItemViewModel>?> UpdateDirectoryAsync()
        {
            return await LoadDirectoryAndFileAsync(CurrentDirectory);
        }

        public void SetToSearchDirectory()
        {
            CurrentDirectory = NavigationInfo.Navigation_Search;
        }
    }
}
