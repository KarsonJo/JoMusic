using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MusicLibrary
{
    public enum SearchType
    {
        ArtistName,
        AlbumName

    }

    public abstract class FileManager
    {
        public static event Action LibraryPathChanged;

        private static readonly List<string> supportedAudioTypes = new() { "m4a", "flac", "mp3", "mp4", "wav", "wma", "aac" };
        public static List<string> SupportedAudioTypes => supportedAudioTypes;

        private class StringInfo
        {
            public string StringName { get; set; }
        }

        private static string filePath => AppConfigManager.MusicDirectory;
        public static string FilePath
        {
            get => filePath;
            //set
            //{
            //    if (Directory.Exists(value))
            //    {
            //        filePath = value;
            //        LibraryPathChanged?.Invoke();
            //    }
            //}
        }

        static FileManager()
        {
            Directory.CreateDirectory(filePath);

            FileMonitor.Initialize();

            FileMonitor.SyncAllFilesNow().FireAndForgetSafeAsync();
        }

        public static string GetMusicFilePath(string FileNameWithExtension, bool absolute = false)
        {
            string path = Path.Combine(FilePath, FileNameWithExtension);
            if (absolute)
            {
                path = Path.GetFullPath(path);
            }
            return path;
        }

        public static async Task<FolderNode> GetParentFolders(long descendantId)
        {
            using var context = new MusicLibraryContext();

            return await context.FolderNodes.FromSqlRaw(@$"
                SELECT Ancestor AS Id, Dirname AS Dirname FROM FolderPaths
                JOIN FolderNodes
                ON FolderPaths.Ancestor = Id
                WHERE Length = 1
                AND Descendant = {descendantId}
            ").FirstOrDefaultAsync();
        }

        public static async Task<List<FolderNode>> GetFolders(IEnumerable<long> ids)
        {
            using var context = new MusicLibraryContext();

            return await context.FolderNodes.FromSqlRaw($@"
                SELECT * FROM FolderNodes
                WHERE Id IN ({string.Join(",", ids)})"
            ).ToListAsync();
        }

        public static async Task<List<FolderNode>> GetAllSubFolders(long ancestorId)
        {
            using var context = new MusicLibraryContext();

            return await context.FolderNodes.FromSqlRaw($@"
                SELECT Descendant AS Id, FolderNodes.Dirname FROM FolderPaths 
                JOIN FolderNodes
                ON Descendant = Id
                WHERE Ancestor = {ancestorId} ORDER BY Length DESC;"
            ).ToListAsync();
        }

        public static async Task<List<FolderNode>> GetDirectSubFolders(long ancestorId)
        {
            using var context = new MusicLibraryContext();

            return await context.FolderNodes.FromSqlRaw($"SELECT Descendant AS Id, DescendantName AS Dirname FROM DirectSubFolder WHERE Ancestor = {ancestorId}").ToListAsync();
        }

        /// <summary>
        /// 根据给出的字符路径
        /// 获取虚拟文件夹的结点集
        /// 如果无法返回完全匹配的结点集将报错
        /// </summary>
        /// <param name="autoCreate">是否在不能完全匹配时创建对应文件夹</param>
        /// <param name="hierarchicalLocation">字符路径</param>
        /// <returns></returns>
        protected static async Task<List<FolderNode>> GetHierarchicalLocation(bool autoCreate, params string[] hierarchicalLocation)
        {
            using var context = new MusicLibraryContext();
            //这坨东西可以查出存在的、对应的层级目录
            var FolderNodes = await context.FolderNodes.FromSqlRaw($@"
                WITH RECURSIVE
                --查询的目录树
                PathName (Dirname) AS(
                VALUES {string.Join(",", from loc in hierarchicalLocation select $"('{loc}')")}),
                Path(Depth, Dirname) AS(
                SELECT row_number() OVER(), Dirname From PathName
                ),
                --当前的递归文件夹位置
                Track(TrackDepth, AncestorId, AncestorName) AS(
                SELECT 1,  Id, Dirname
                FROM FolderNodes
                WHERE Id = 1
                UNION ALL
                SELECT Track.TrackDepth + 1, D.Descendant, D.DescendantName FROM DirectSubFolder AS D, Track
                JOIN Path
                ON Track.TrackDepth = Path.Depth
                WHERE D.Ancestor = Track.AncestorId
                AND D.DescendantName = Path.Dirname)
                SELECT AncestorId As ID, AncestorName AS Dirname FROM Track;
            ").ToListAsync();

            //看看返回结果是否能对上层级
            if (FolderNodes.Count - 1 != hierarchicalLocation.Length)
            {
                //是否自动创建不存在的目录？
                if (autoCreate)
                {
                    var transaction = await context.Database.BeginTransactionAsync();
                    try
                    {
                        for (int i = FolderNodes.Count - 1; i < hierarchicalLocation.Length; i++)
                        {
                            //创建一个子目录
                            await context.Database.ExecuteSqlRawAsync($@"
                                INSERT INTO FolderCreate
                                VALUES ({FolderNodes[i].Id}, '{hierarchicalLocation[i]}')
                            ");
                            //取回刚创建的目录
                            FolderNodes.Add(await context.FolderNodes.FromSqlRaw($@"
                                SELECT Descendant AS Id, DescendantName AS Dirname FROM DirectSubFolder 
                                WHERE Ancestor = {FolderNodes[i].Id} AND DescendantName='{hierarchicalLocation[i]}'
                            ").FirstAsync());
                        }

                        //成功的事务应该提交
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        //查询出错涅
                        await transaction.RollbackAsync();
                        throw new DirectoryNotFoundException("Query failed when creating directory");
                    }

                    //再看看能不能对上层级？
                    if (FolderNodes.Count - 1 != hierarchicalLocation.Length)
                    {
                        throw new DirectoryNotFoundException("Unknown error occurred when creating directory");
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException("Virtual location not exists");
                }
            }

            return FolderNodes;
        }

        public static async Task<List<FolderNode>> GetHierarchicalLocation(long folderId)
        {
            using var context = new MusicLibraryContext();

            return await context.FolderNodes.FromSqlRaw($@"
                SELECT Ancestor AS Id, FolderNodes.Dirname FROM FolderPaths
                JOIN FolderNodes
                ON Ancestor = Id
                WHERE Descendant = {folderId} ORDER BY Length DESC
            ").ToListAsync();
        }

        public static async Task<string[]> GetNavigationPath(long folderId)
        {
            return (from node in await GetHierarchicalLocation(folderId) select node.Dirname).ToArray();
            //return string.Join(@"\", );
        }

        public static async Task<List<PlayList>> GetFavouritePlaylists()
        {
            using var context = new MusicLibraryContext();

            return await context.PlayLists.ToListAsync();
            //return await (from folderNode in context.FolderNodes join playlist in context.PlayLists on folderNode.Id equals playlist.FolderId select folderNode).ToListAsync();
        }

        public static async Task AddFavouritePlaylists(IEnumerable<PlayList> playlists)
        {
            using var context = new MusicLibraryContext();



            await context.PlayLists.AddRangeAsync(playlists);

            await context.SaveChangesAsync();
        }

        public static async Task RemoveFavouritePlaylists(IEnumerable<PlayList> playlists)
        {
            using var context = new MusicLibraryContext();

            context.PlayLists.RemoveRange(playlists);

            await context.SaveChangesAsync();
        }

        public static async Task CutVirtualFolderRecursively(long sourceId, long targetId)
        {

            //检测是否原地剪切
            if ((await GetParentFolders(sourceId)).Id == targetId)
            {
                return;
            }

            //检测是否循环剪切
            var subnodes = await GetAllSubFolders(sourceId);
            foreach (var node in subnodes)
            {
                if (node.Id == targetId)
                    return;
            }

            using var context = new MusicLibraryContext();

            var transaction = await context.Database.BeginTransactionAsync();

            await context.Database.ExecuteSqlRawAsync($@"
                UPDATE FolderCut
                SET Ancestor = /*目标父结点*/{targetId}
                WHERE Ancestor = /*复制结点*/{sourceId};
            ");


            //重命名文件夹
            await CopyFolderRename(context);

            await transaction.CommitAsync();
        }

        public static async Task CopyVirtualFolderRecursively(long sourceId, long targetId)
        {
            using var context = new MusicLibraryContext();

            var transaction = await context.Database.BeginTransactionAsync();

            await context.Database.ExecuteSqlRawAsync($@"
                UPDATE FolderCopy
                SET Ancestor = /*目标父结点*/{targetId}
                WHERE Ancestor = /*复制结点*/{sourceId};
            ");

            //重命名文件夹
            await CopyFolderRename(context);

            await transaction.CommitAsync();
        }

        private static async Task CopyFolderRename(MusicLibraryContext context)
        {
            var connection = context.Database.GetDbConnection() as Microsoft.Data.Sqlite.SqliteConnection;
            //往里导正则表达式函数
            //https://www.bricelam.net/2017/08/22/sqlite-efcore-udf-all-the-things.html
            //https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/user-defined-functions
            connection.CreateFunction(
                "REGEXP",
                (string pattern, string input) => Regex.IsMatch(input, pattern));

            //检查重名，并重命名文件夹
            //约定复制文件夹的特殊命名为[Dirname]@!Copy[id]#@
            var foldersToRename = context.DirectSubFolders.FromSqlRaw(@"SELECT * FROM DirectSubFolder WHERE DescendantName REGEXP '(.*?)(@!Copy\d*#@)+'");

            foreach (var folderToRename in foldersToRename)
            {
                //如果出于某些原因匹配失败，跳过
                var match = Regex.Match(folderToRename.DescendantName, @"(.*?)(@!Copy\d*#@)+");
                if (match == null || match.Groups.Count == 0)
                {
                    continue;
                }
                string baseDirname = match.Groups[1].ToString();
                //string dirname = baseDirname;
                //for (int i = 1; ; i++)
                //{
                //    //查找重名
                //    if ((from folder in context.DirectSubFolders where folder.Ancestor == folderToRename.Ancestor && folder.DescendantName == dirname select folder).Any())
                //    {
                //        dirname = $"{baseDirname}({i})";
                //        continue;
                //    }
                //    else
                //    {
                //        //改名
                //        folderToRename.DescendantName = dirname;
                //        context.FolderNodes.Update(new FolderNode() { Id = (long)folderToRename.Descendant, Dirname = dirname });
                //        await context.SaveChangesAsync();

                //        break;
                //    }
                //}

                await ConflictFolderRename(context, baseDirname, (long)folderToRename.Ancestor, (long)folderToRename.Descendant);
            }
        }

        public static async Task ConflictFolderRename(MusicLibraryContext context, string baseDirname, long ancestorId, long targetId)
        {
            string dirname = baseDirname;
            for (int i = 1; ; i++) 
            {
                //查找重名
                if ((from folder in context.DirectSubFolders where folder.Ancestor == ancestorId && folder.DescendantName == dirname select folder).Any())
                {
                    dirname = $"{baseDirname}({i})";
                    continue;
                }
                else
                {
                    //改名
                    //folderToRename.DescendantName = dirname;
                    context.FolderNodes.Update(new FolderNode() { Id = targetId, Dirname = dirname });
                    await context.SaveChangesAsync();

                    break;
                }
            }
        }

        public static async Task DeleteVirtualFolderRecursively(long sourceId, bool deleteRealFile = false)
        {
            using var context = new MusicLibraryGenericContext<StringInfo>();

            var transaction = await context.Database.BeginTransactionAsync();

            if (deleteRealFile)
            {
                var filenames = await context.QueryModel.FromSqlRaw($@"
                    SELECT DISTINCT FileName AS StringName
                    FROM FolderPaths, FileNodes, SongFileMeta 
                    WHERE Ancestor = {sourceId} AND FolderId = Descendant AND SongId = Id")
                    .ToListAsync();

                DeleteRealFiles(from filename in filenames select filename.StringName);
            }


            await context.Database.ExecuteSqlRawAsync($@"
                DELETE FROM DirectSubFolder WHERE Descendant = {sourceId};
            ");

            await transaction.CommitAsync();
        }

        public static async Task<bool> CheckDuplicateName(FolderNode nodeWithNewName)
        {
            using var context = new MusicLibraryContext();

            var conflicts = context.DirectSubFolders.FromSqlRaw($@"
                SELECT 1 FROM DirectSubFolder AS A, DirectSubFolder AS B
                WHERE A.Descendant = /*目标*/{nodeWithNewName.Id}
                AND B.Ancestor = A.Ancestor
                AND B.DescendantName = /*新名*/'{nodeWithNewName.Dirname}'
            ");

            return await conflicts.AnyAsync();
        }

        public static async Task<bool> CheckDuplicateName(FolderCreate ancestorWithName)
        {
            using var context = new MusicLibraryContext();

            var conflicts = context.DirectSubFolders.FromSqlRaw($@"
                SELECT 1 FROM DirectSubFolder
                WHERE Ancestor = /*父亲*/{ancestorWithName.Ancestor}
                AND DescendantName = /*新名*/'{ancestorWithName.Dirname}'
            ");

            return await conflicts.AnyAsync();
        }


        public static async Task<bool> RenameFile(string oldName, string newName)
        {
            using var context = new MusicLibraryContext();

            var rowAffected = await context.Database.ExecuteSqlRawAsync($"UPDATE SongFileMeta SET FileName='{newName}' WHERE FileName='{oldName}'");

            return rowAffected > 0;
        }
        public static async Task<bool> RenameFolder(FolderNode nodeWithNewName)
        {
            //检测重名
            if (await CheckDuplicateName(nodeWithNewName))
            {
                return false;
            }
            else
            {
                try
                {
                    using var context = new MusicLibraryContext();
                    context.FolderNodes.Update(nodeWithNewName);
                    await context.SaveChangesAsync();
                }
                catch 
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> RenameFile(SongFileMetum songMeta)
        {
            try
            {
                using var context = new MusicLibraryContext();
                context.SongFileMeta.Update(songMeta);
                await context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 创建新文件夹，返回新建的Id
        /// 效率较差，若条件允许应该使用NewFolder代替
        /// </summary>
        /// <param name="NewFolder"></param>
        /// <returns></returns>
        public static async Task<long> NewFolderWithReturning(FolderCreate NewFolder)
        {

            try
            {
                using var context = new MusicLibraryContext();

                var transaction = await context.Database.BeginTransactionAsync();

                string taggedDirname = NewFolder.Dirname + "@!Copy0000#@";

                //插入带copy tag的特殊名称，以之后修改
                await context.Database.ExecuteSqlRawAsync($@"
                        INSERT INTO FolderCreate
                        VALUES ({NewFolder.Ancestor}, '{taggedDirname}')
                    ");

                //按特征字符串查找，如果存在多个（极极极小概率事件）就祈祷吧
                var createdFolder = await (from folder in context.FolderNodes where folder.Dirname == taggedDirname select folder).AsNoTracking().FirstAsync();

                await CopyFolderRename(context);

                transaction.Commit();

                return createdFolder.Id;
            }
            catch
            {
                return -1;
            }

        }

        public static async Task<bool> NewFolder(FolderCreate NewFolder)
        {
            //检测重名
            if (await CheckDuplicateName(NewFolder))
            {
                return false;
            }
            else
            {
                try
                {
                    using var context = new MusicLibraryContext();

                    var transaction = await context.Database.BeginTransactionAsync();

                    await context.Database.ExecuteSqlRawAsync($@"
                        INSERT INTO FolderCreate
                        VALUES ({NewFolder.Ancestor}, '{NewFolder.Dirname}')
                    ");

                    transaction.Commit();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<List<SongFileMetum>> GetSongsWithoutArtistsData(long folderId)
        {
            using var context = new MusicLibraryContext();
            return await (from songMeta in context.SongFileMeta join fileNode in context.FileNodes on songMeta.Id equals fileNode.SongId where fileNode.FolderId == folderId select songMeta)
                .ToListAsync();
        }
        public static async Task<List<SongFileMetum>> GetSongsWithArtistsData(long folderId)
        {
            using var context = new MusicLibraryContext();
            return await (from songMeta in context.SongFileMeta join fileNode in context.FileNodes on songMeta.Id equals fileNode.SongId where fileNode.FolderId == folderId select songMeta)
                .Include(x=>x.SongArtists)
                .ToListAsync();
        }

        public static async Task CutMetaData(List<SongFileMetum> songFileMeta, long fromFolderId, long toFolderId)
        {
            if (fromFolderId == toFolderId)
            {
                return;
            }
            await RecordMetaData(songFileMeta, toFolderId);
            await DeleteFileNodes(songFileMeta, fromFolderId);
        }

        public static async Task RecordMetaData(List<SongFileMetum> songFileMeta, long targetFolderId)
        {
            if (!songFileMeta.Any())
            {
                return;
            }

            List<FileNode> newFiles = new();
            ////记录到目标虚拟文件夹
            //foreach (var songMeta in songFileMeta)
            //{
            //    newFiles.Add(new FileNode() { FolderId = targetFolderId, SongId = songMeta.Id });
            //}
            ////应用更改
            //using var context = new MusicLibraryContext();
            //context.FileNodes.AddRange(newFiles);

            using var context = new MusicLibraryContext();

            List<string> insertBody = new();
            foreach (var songMeta in songFileMeta)
            {
                insertBody.Add($"({songMeta.Id},{targetFolderId})");
            }


            var transaction = await context.Database.BeginTransactionAsync();

            await context.Database.ExecuteSqlRawAsync($@"
                INSERT OR IGNORE INTO FileNodes
                VALUES {string.Join(",", insertBody)}
            ");

            transaction.Commit();
        }

        /// <summary>
        /// 根据真实文件夹的文件名删除数据库中的文件元信息，以及对应的文件节点
        /// </summary>
        /// <param name="fileName">真实文件夹的文件名</param>
        /// <returns></returns>
        public static async Task DeleteAllFileNodeWithFileName(string fileName)
        {
            using var context = new MusicLibraryContext();
            //只移除filemeta, filenode设置了级联删除
            context.SongFileMeta.RemoveRange(from fileMeta in context.SongFileMeta where fileMeta.FileName == fileName select fileMeta);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除相对路径所对应的真实文件
        /// 如果有启动监视，所有文件结点也会一并删除
        /// </summary>
        /// <param name="fileMeta"></param>
        /// <param name="folderNode"></param>
        /// <returns></returns>
        public static void DeleteRealFiles(IEnumerable<string> filenames)
        {
            foreach (var filename in filenames)
            {
                File.Delete(GetMusicFilePath(filename));
            }
        }

        public static async Task DeleteFileNodes(IEnumerable<SongFileMetum> fileMeta, long folderNode)
        {
            using var context = new MusicLibraryContext();

            List<FileNode> fileNodes = new();
            foreach (var fileNode in from fileNodeId in fileMeta select fileNodeId.Id)
            {
                fileNodes.Add(new() { FolderId = folderNode, SongId = fileNode });
            }

            context.FileNodes.RemoveRange(fileNodes);
            await context.SaveChangesAsync();
        }

        public static async Task<List<FolderNode>> FolderLikeSearch(string keyword, int limit = 5000)
        {
            using var context = new MusicLibraryContext();

            return await context.FolderNodes.FromSqlRaw($@"
                SELECT * FROM FolderNodes WHERE Dirname LIKE '%{keyword}%' LIMIT {limit}
            ").ToListAsync();
        }

        public static async Task<List<SongFileMetum>> SongWithArtistsLikeSearch(string keyword, int limit = 5000)
        {
            using var context = new MusicLibraryContext();

            return await context.SongFileMeta.FromSqlRaw($@"
                SELECT * FROM SongFileMeta WHERE SongName LIKE '%{keyword}%' LIMIT {limit}
            ")
            .Include(x=>x.SongArtists)
            .ToListAsync();
        }


        public static async Task<List<string>> NamesLikeSearch(SearchType searchType, string keyword = null, int limit = 5000)
        {
            using var context = new MusicLibraryGenericContext<StringInfo>();

            List<StringInfo> result;
            if (string.IsNullOrEmpty(keyword))
            {
                switch (searchType)
                {
                    case SearchType.ArtistName:
                        result = await context.QueryModel.FromSqlRaw($"SELECT DISTINCT ArtistName AS StringName FROM SongArtists LIMIT {limit}").ToListAsync();
                        break;
                    case SearchType.AlbumName:
                        result = await context.QueryModel.FromSqlRaw($"SELECT DISTINCT AlbumName AS StringName FROM SongFileMeta LIMIT {limit}").ToListAsync();
                        break;
                    default:
                        result = null;
                        break;
                }
            }
            else
            {
                switch (searchType)
                {
                    case SearchType.ArtistName:
                        result = await context.QueryModel.FromSqlRaw($"SELECT DISTINCT ArtistName AS StringName FROM SongArtists WHERE ArtistName LIKE '%{keyword}%' LIMIT {limit}").ToListAsync();
                        break;
                    case SearchType.AlbumName:
                        result = await context.QueryModel.FromSqlRaw($"SELECT DISTINCT AlbumName AS StringName FROM SongFileMeta WHERE AlbumName LIKE '%{keyword}%' LIMIT {limit}").ToListAsync();
                        break;
                    default:
                        result = null;
                        break;
                }
            }
            return (from r in result select r.StringName).ToList();
        }

        public static async Task<List<SongFileMetum>> SongsWithArtistsOfArtist(string artistName)
        {
            using var context = new MusicLibraryContext();

            return await (from song in context.SongFileMeta join artist in context.SongArtists on song.Id equals artist.SongId where artist.ArtistName == artistName select song).Include(x => x.SongArtists).ToListAsync();
        }

        public static async Task<List<SongFileMetum>> SongsWithArtistsOfAlbum(string albumName)
        {
            using var context = new MusicLibraryContext();

            return await (from song in context.SongFileMeta where song.AlbumName == albumName select song).Include(x => x.SongArtists).ToListAsync();
        }

        public static async Task<List<SongFileMetum>> SongsWithArtistsNotCategorized(int limit = 5000)
        {
            using var context = new MusicLibraryContext();
            return await context.SongFileMeta.FromSqlRaw($"SELECT * FROM SongFileMeta WHERE Id NOT IN (SELECT DISTINCT SongId FROM FileNodes) LIMIT {limit}").ToListAsync();
        }

        protected static SongFileMetum SetOneSongMetaData(MusicLibraryContext context, SongStreamData songStream)
        {
            var songMeta = songStream.GetSongFileMetum();
            var artists = songStream.GetSongArtists();

            songMeta.SongArtists = artists;

            context.SongFileMeta.Add(songMeta);
            context.SongArtists.AddRange(artists);

            return songMeta;
        }

        /// <summary>
        /// 把歌曲的元数据写入数据库
        /// 返回写入的数据对象
        /// </summary>
        /// <param name="songStreams"></param>
        /// <returns></returns>
        public static async Task<List<SongFileMetum>> WriteSongMetaData(List<SongStreamData> songStreams)
        {
            using var context = new MusicLibraryContext();

            //插入歌曲和歌手
            List<SongFileMetum> songMetas = new();
            foreach (var songStream in songStreams)
            {
                songMetas.Add(SetOneSongMetaData(context, songStream));
            }
            await context.SaveChangesAsync();


            //foreach (var songmeta in songMetas)
            //{
            //    Console.WriteLine($"{songmeta.Id},{songmeta.FileName}");
            //}

            return songMetas;

        }
        /// <summary>
        /// 把文件数据记录到虚拟文件夹中
        /// </summary>
        /// <param name="songMetaData"></param>
        /// <param name="hierarchicalLocation"></param>
        /// <returns></returns>
        public static async Task RecordToVirtualFolder(List<SongFileMetum> songMetaData, params string[] hierarchicalLocation)
        {
            if (!songMetaData.Any())
            {
                return;
            }

            //层级结构
            var locations = await GetHierarchicalLocation(true, hierarchicalLocation);
            //获取最后一个结点
            var targetLocation = locations[^1];
            //记录到目标虚拟文件夹
            //foreach (var songMeta in songMetaData)
            //{
            //    targetLocation.FileNodes.Add(new FileNode() { FolderId = targetLocation.Id, SongId = songMeta.Id });
            //}
            ////应用更改
            //using var context = new MusicLibraryContext();
            //context.FileNodes.AddRange(targetLocation.FileNodes);
            //await context.SaveChangesAsync();
            await RecordMetaData(songMetaData, targetLocation.Id);
        }

        /// <summary>
        /// 检测库中是否有该文件，有则返回相应的结点
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public static async Task<SongFileMetum> CheckFileExistence(SongStreamData songStream)
        {
            using var context = new MusicLibraryContext();
            try
            {
                //MD5相同
                var songs = await (from song in context.SongFileMeta where song.Md5 == songStream.Md5 select song).ToListAsync();
                foreach (var song in songs)
                {
                    //文件大小相同
                    if (new FileInfo(GetMusicFilePath(song.FileName)).Length == songStream.FileLength)
                    {
                        //此时极大概率是相同文件
                        return song;
                    }
                }
                //不匹配返回空
                return null;
            }
            catch
            {
                //异常返回空
                return null;
            }
        }

        /// <summary>
        /// 把音乐流媒体拷贝到管理目录下
        /// </summary>
        /// <param name="songStreams"></param>
        /// <returns>保存后的路径</returns>
        public async Task<string> CopyAndSaveFileStreamAsync(string FileNameWithExtension, Stream dataStream)
        {

            string filePath = GetMusicFilePath(FileNameWithExtension);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using Stream stream = File.Open(filePath, FileMode.Create);
            await dataStream.CopyToAsync(stream);

            return filePath;
        }

        private static async Task CreateTempStringTable(DbContext dbcontext, IEnumerable<string> fileNames)
        {
            //创建临时表
            string queryCommand;
            //创建临时表
            queryCommand = @"
                CREATE TEMP TABLE Paths(
	                StringName TEXT
                );
            ";

            await dbcontext.Database.ExecuteSqlRawAsync(queryCommand);

            if (fileNames.Any())
            {
                //把数据转移到临时表
                List<string> insertBody = new();
                foreach (var fileName in fileNames)
                {
                    insertBody.Add($"('{fileName.Replace("'", "''")}')");
                }
                queryCommand = $@"
                INSERT INTO Paths
                VALUES {string.Join(",", insertBody)};
            ";
                await dbcontext.Database.ExecuteSqlRawAsync(queryCommand);

                //创建索引
                await dbcontext.Database.ExecuteSqlRawAsync("CREATE INDEX IX_Paths_StringName ON Paths(StringName)");
            }
        }

        private static async Task<HashSet<string>> MatchFileInfo(IEnumerable<string> source, string columnName)
        {
            using var dbcontext = new MusicLibraryGenericContext<StringInfo>();

            //开始事务
            var transaction = await dbcontext.Database.BeginTransactionAsync();

            //创建临时表
            await CreateTempStringTable(dbcontext, source);


            //查找记录中的交集
            var vals = await dbcontext.QueryModel.FromSqlRaw(
                $"SELECT StringName FROM Paths, SongFileMeta WHERE StringName = {columnName}").ToListAsync();


            //返回包含的记录
            HashSet<string> fileHashSet = new();
            foreach (var val in vals)
            {
                fileHashSet.Add(val.StringName.ToString());
            }

            //处理后事
            await dbcontext.Database.ExecuteSqlRawAsync("DROP TABLE Paths");
            transaction.Commit();

            return fileHashSet;
        }

        /// <summary>
        /// 返回fileNames列表中有记录的值
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static async Task<HashSet<string>> MatchFileNames(IEnumerable<string> fileNames)
        {
            return await MatchFileInfo(fileNames, "FileName");
        }

        /// <summary>
        /// 返回md5列表中有记录的值
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public static async Task<HashSet<string>> MatchMd5(IEnumerable<string> md5)
        {
            return await MatchFileInfo(md5, "Md5");
        }

        public static async Task<int> DeleteAllFileNodeNotIn(IEnumerable<string> fileNames)
        {
            using var dbcontext = new MusicLibraryGenericContext<StringInfo>();

            //开始事务
            var transaction = await dbcontext.Database.BeginTransactionAsync();

            //创建临时表，导入数据
            await CreateTempStringTable(dbcontext, fileNames);

            //为了提高性能，不使用efcore，而使用传统bulk delete
            var affected = await dbcontext.Database.ExecuteSqlRawAsync(@"
            DELETE FROM SongFileMeta WHERE NOT EXISTS (
                SELECT 1 FROM Paths WHERE StringName = FileName)");

            //处理后事
            await dbcontext.Database.ExecuteSqlRawAsync("DROP TABLE Paths");
            transaction.Commit();

            return affected;
        }

        /// <summary>
        /// 可以一次提交多个插入请求
        /// 但似乎每个请求还是会分别执行(并不能节省访问次数)
        /// </summary>
        /// <param name="songStream"></param>
        /// <returns></returns>
        public static async Task TestRecordFileAsync(SongStreamData songStream)
        {
            using var context = new MusicLibraryContext();
            var transaction = context.Database.BeginTransaction();
            var songMeta = songStream.GetSongFileMetum();
            var artists = songStream.GetSongArtists();
            songMeta.SongArtists = artists;

            context.SongFileMeta.Add(songMeta);
            context.SongArtists.AddRange(artists);

            var song2 = new SongStreamData()
            {
                SongName = "SongName2",
                AlbumName = "AlbumName2",
                ArtistNames = new string[] { "Artist3", "Artist4" },
                FileExtension = "m4a",
            };
            songMeta = song2.GetSongFileMetum();
            artists = song2.GetSongArtists();
            songMeta.SongArtists = artists;

            context.SongFileMeta.Add(songMeta);
            context.SongArtists.AddRange(artists);

            await context.SaveChangesAsync();

            transaction.Commit();
        }
    }
}
