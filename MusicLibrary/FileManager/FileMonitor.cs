using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary
{
    /// <summary>
    /// 应对用户直接对真实文件夹中的修改
    /// </summary>
    public static class FileMonitor
    {
        /// <summary>
        /// FileManager.FilePath的别名
        /// </summary>
        private static string WatchingPath => FileManager.FilePath;

        static FileSystemWatcher watcher;

        public static FileSystemWatcher Watcher => watcher;

        public static event FileSystemEventHandler FileChanged { add => watcher.Changed += value; remove => watcher.Changed -= value; }
        public static event FileSystemEventHandler FileCreated { add => watcher.Created += value; remove => watcher.Created -= value; }
        public static event FileSystemEventHandler FileDeleted { add => watcher.Deleted += value; remove => watcher.Deleted -= value; }
        public static event RenamedEventHandler FileRenamed { add => watcher.Renamed += value; remove => watcher.Renamed -= value; }
        public static event ErrorEventHandler MonitorError { add => watcher.Error += value; remove => watcher.Error -= value; }


        //static FileMonitor()
        //{
        //    Debug.WriteLine("============FileMonitor============");
        //    Debugger.Break();
        //    //订阅事件
        //    FileManager.LibraryPathChanged += FilePathListener;
        //    FilePathListener();
        //}

        public static void Initialize()
        {
            //订阅事件
            FileManager.LibraryPathChanged += FilePathListener;
            FilePathListener();
        }


        private static void FilePathListener()
        {
            Debug.WriteLine("============FilePathListener============");
            //清除残余
            watcher?.Dispose();
            //初始化Watcher
            watcher = new FileSystemWatcher(WatchingPath);
            watcher.InternalBufferSize = 81920; //80KB

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            FileCreated += SyncCreate;
            FileDeleted += SyncDelete;
            FileRenamed += SyncRename;

            foreach (var extension in FileManager.SupportedAudioTypes)
            {
                watcher.Filters.Add($"*.{extension}");
            }
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 同步本地的新增到用户级文件系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void SyncCreate(object sender, FileSystemEventArgs e)
        {
            try
            {
                await FileManager.WriteSongMetaData(new List<SongStreamData>() { new LocalSongStreamData(e.Name) });
            }
            catch
            {
                //正由其它程序占用（多半是因为文件由本程序的下载器创建，因此不需要处理）
            }
        }

        /// <summary>
        /// 同步本地的删除到用户级文件系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void SyncDelete(object sender, FileSystemEventArgs e)
        {
            await FileManager.DeleteAllFileNodeWithFileName(Path.GetFileName(e.Name));
        }

        /// <summary>
        /// 同步本地的重命名到用户级文件系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void SyncRename(object sender, RenamedEventArgs e)
        {
            await FileManager.RenameFile(Path.GetFileName(e.OldName), Path.GetFileName(e.Name));
        }

        /// <summary>
        /// 对管理文件夹进行全面扫描，做如下操作：
        /// 文件夹有，库中无：
        ///     md5+size匹配成功：
        ///         匹配结点无效：
        ///             SyncRename
        ///         匹配结点仍有效：
        ///             重复文件，删除该文件
        ///     md5+size匹配失败：
        ///         SyncCreate
        /// 文件夹无，库中有：
        ///     SyncDelete
        /// </summary>
        /// <returns></returns>
        public static async Task SyncAllFilesNow()
        {
            var fileNames = (from file in Directory.EnumerateFiles(WatchingPath, "*.*", SearchOption.TopDirectoryOnly)
                        select Path.GetFileName(file)).ToList();

            //记录文件夹有，库中也有的文件
            HashSet<string> matchedFiles = await FileManager.MatchFileNames(fileNames);
            //文件夹有，库中无：
            foreach (var filename in fileNames)
            {
                if (!matchedFiles.Contains(filename))
                {
                    
                    LocalSongStreamData songData = new(FileManager.GetMusicFilePath(filename));
                    //尝试寻找一个相等的文件
                    //一首首处理，比较低效但简单
                    var song = await FileManager.CheckFileExistence(songData);
                    //匹配到歌曲
                    if (song != null)
                    {
                        //如果结点的文件有效，则当前文件是重复文件
                        if (File.Exists(FileManager.GetMusicFilePath(song.FileName)))
                        {
                            songData.Dispose();
                            File.Delete(FileManager.GetMusicFilePath(filename));
                        }
                        //否则该文件是重命名后文件：SyncRename
                        else
                        {
                            song.FileName = Path.GetFileName(filename);
                            await FileManager.RenameFile(song);
                        }
  
                    }
                    //SyncCreate
                    else
                    {
                        await FileManager.WriteSongMetaData(new List<SongStreamData>() { songData });
                    }
                }
            }
            //文件夹无，库中有：
            //SyncDelete
            await FileManager.DeleteAllFileNodeNotIn(fileNames);

        }
    }
}
