using Microsoft.EntityFrameworkCore;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Netease
{
    public class NeteaseFileManager : FileManager
    {
        private class NeteaseSongKey
        {
            public int Id { get; set; }
        }

        private static async Task CreateTempIdTable(DbContext dbcontext, IEnumerable<string> songIds)
        {
            string queryCommand;
            //创建临时表
            queryCommand = @"
                CREATE TEMP TABLE DownloadIds(
                Id INTEGER PRIMARY KEY
                );
            ";
            await dbcontext.Database.ExecuteSqlRawAsync(queryCommand);

            //把数据转移到临时表
            List<string> insertBody = new();
            foreach (var songId in songIds)
            {
                insertBody.Add($"({songId})");
            }
            queryCommand = $@"
                INSERT INTO DownloadIds
                VALUES {string.Join(",", insertBody)};
            ";
            await dbcontext.Database.ExecuteSqlRawAsync(queryCommand);
        }

        /// <summary>
        /// 输入网易云id列表
        /// 筛选出未有记录（需要下载）的歌曲id
        /// </summary>
        /// <param name="songIds"></param>
        /// <returns></returns>
        public static async Task<HashSet<string>> TagUndownloadedSongs(IEnumerable<string> songIds)
        {
            if (!songIds.Any())
            {
                return new();
            }
            using var dbcontext = new MusicLibraryGenericContext<NeteaseSongKey>();

            //开始事务
            var transaction = await dbcontext.Database.BeginTransactionAsync();

            //创建临时表
            await CreateTempIdTable(dbcontext, songIds);

            //查找记录中的补集
            var idList = dbcontext.QueryModel.FromSqlRaw(@"
                SELECT Id FROM DownloadIds
                INTERSECT SELECT NeteaseId FROM NeteaseData;
            ");

            //标记不需要下载的歌曲
            HashSet<string> songHashSet = new();
            foreach (var id in idList)
            {
                songHashSet.Add(id.Id.ToString());
            }

            //处理后事
            await dbcontext.Database.ExecuteSqlRawAsync("DROP TABLE DownloadIds");
            transaction.Commit();

            return songHashSet;
        }

        /// <summary>
        /// 输入网易云Id列表
        /// 筛选出有记录的文件数据
        /// </summary>
        /// <param name="neteaseIds"></param>
        /// <returns></returns>
        public static async Task<List<SongFileMetum>> RetrieveSongFileMetaByNeteaseId(IEnumerable<string> neteaseIds)
        {
            if (!neteaseIds.Any())
            {
                return new();
            }
            using var dbcontext = new MusicLibraryContext();

            //开始事务
            var transaction = await dbcontext.Database.BeginTransactionAsync();

            //创建临时表
            await CreateTempIdTable(dbcontext, neteaseIds);

            //查找有效的文件记录
            var songFileMeta = await dbcontext.SongFileMeta.FromSqlRaw(@"
                SELECT SongFileMeta.* FROM SongFileMeta, NeteaseData, DownloadIds
                WHERE SongFileMeta.Id = SongId AND NeteaseId = DownloadIds.Id;
            ").ToListAsync();

            //处理后事
            await dbcontext.Database.ExecuteSqlRawAsync("DROP TABLE DownloadIds");
            transaction.Commit();

            return songFileMeta;
        }

        /// <summary>
        /// 对于网易云音乐的数据
        /// 不仅写入元数据
        /// 而且写入网易云Id
        /// </summary>
        /// <param name="songMetas"></param>
        /// <returns></returns>
        public static async Task<List<SongFileMetum>> WriteSongMetaDataWithNeteaseId(List<NeteaseSongStreamData> songStreams)
        {
            using var context = new MusicLibraryContext();

            //插入歌曲和歌手
            //以及网易云Id
            List<SongFileMetum> songMetas = new();
            foreach (var songStream in songStreams)
            {
                var songMeta = SetOneSongMetaData(context, songStream);
                songMeta.NeteaseDatum = new NeteaseDatum() { NeteaseId = songStream.NeteaseId };
                songMetas.Add(songMeta);
            }
            await context.SaveChangesAsync();


            //foreach (var songmeta in songMetas)
            //{
            //    Console.WriteLine($"{songmeta.Id},{songmeta.FileName}");
            //}

            return songMetas;
        }
    }
}
