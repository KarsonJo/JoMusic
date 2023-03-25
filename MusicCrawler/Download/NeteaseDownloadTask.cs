using MusicCrawler.Netease;
using MusicCrawler.Utilities;
using MusicLibrary;
using MusicLibrary.Models;
using MusicLibrary.Netease;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicCrawler.Download
{
    public enum DownloadQuality
    {
        standard,
        high,
        lossless
    }
    public class NeteaseDownloadTask : DownloadTask<List<SongFileMetum>>
    {
        //public SongFileMetum DownloadedContent { get; private set; }
        public SongData SongData { get; }

        public override string TaskName => SongData?.ArtistsSongName ?? "Unknown";

        public NeteaseDownloadTask(SongData songData)
        {
            SongData = songData;
        }

        /// <summary>
        /// 网易云音乐的下载逻辑
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async Task DownloadLogic(/*IProgress<double> progress = null, */CancellationToken cancellationToken = default)
        {
            if (SongData == null)
            {
                throw new ArgumentNullException(nameof(SongData), $"{nameof(SongData)} is null");
            }
            //获取下载数据
            var downloadData = await GetDownloadUrl();

            //下载
            if (downloadData != null && !string.IsNullOrEmpty(downloadData.DownloadUrl))
            {
                Console.WriteLine($"md5 from cloud: {downloadData.OriginalMD5}");

                //创建新的流数据（但目前为止并不真正载入流数据）
                NeteaseSongStreamData songStreamData = CreateNeteaseStreamData(SongData, downloadData);

                //保存路径
                string filePath = FileManager.GetMusicFilePath(songStreamData.FileNameWithExtension);
                try
                {
                    //1 创建文件
                    //using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    //{
                    //    //2.1 与下载服务器取得联系
                    //    using (var response = await HttpClientHolder.Client.GetAsync(downloadData.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    //    {
                    //        //2.2 获取文件大小
                    //        FileSizeB = response.Content.Headers.ContentLength;

                    //        //2.3 下载文件
                    //        await HttpClientHolder.Client.DownloadAsync(response, file, progress, cancellationToken);
                    //    }
                    //}

                    //1 创建文件
                    await DownloadFile(filePath, new Uri(downloadData.DownloadUrl), cancellationToken);

                        //3 下载封面图，写入歌曲数据
                    using (Stream imageStream = await HttpClientHolder.Client.GetStreamAsync(SongData.CoverImageUrl))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await imageStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            MusicInformation musicInfomation = new()
                            {
                                Title = SongData.SongName,
                                Artists = SongData.ArtistNames.ToArray(),
                                Album = SongData.AlbumName,
                                Comment = AppConfigManager.Write163Key ? Calculate163Key(SongData, downloadData) : null,
                                PictureMimeMapping = SongData.CoverImageExtension,
                                ImageStream = memoryStream
                            };

                            //4 写入数据
                            MusicTagModifier.WriteMusicTags(filePath, musicInfomation);
                        }
                    }

                    //5 从本地读出文件
                    songStreamData.LoadStreamFromPath(filePath);

                    //6 写入记录（包含针对网易云的id数据）
                    DownloadedContent = await NeteaseFileManager.WriteSongMetaDataWithNeteaseId(new List<NeteaseSongStreamData>() { songStreamData });
                }
                catch (OperationCanceledException e)
                {
                    //取消任务，处理异常
                    Debug.WriteLine($"下载任务取消：{songStreamData.FileNameWithExtension}, {e.Message}");
                    File.Delete(filePath);
                }
                catch
                {
                    //其它异常，处理后继续抛出
                    File.Delete(filePath);
                    throw;
                }
            }
            else
            {
                throw new Exception($"无下载权限：{SongData.SongName}({SongData.SongId})");
            }
  
        }

        private async Task<DownloadData> GetDownloadUrl()
        {
            //请求参数
            Dictionary<string, object> downloadParams = new()
            {
                { "ids", $"[{ulong.Parse(SongData.SongId)}]" },
                { "level", ((DownloadQuality)AppConfigManager.NeteaseDownloadQuality).ToString() },
                { "encodeType", "aac" },
                { "csrf_token", "" }
            };
            //加密，然后发送请求
            string response = await CloudMusicCrawler.SendPostRequest("https://music.163.com/weapi/song/enhance/player/url/v1?csrf_token=", downloadParams);
            Debug.WriteLine(response);
            return new DownloadData(JObject.Parse(response)["data"][0]);
        }

        private NeteaseSongStreamData CreateNeteaseStreamData(SongData songData, DownloadData downloadData)
        {
            return new NeteaseSongStreamData()
            {
                SongName = songData.SongName,
                AlbumName = songData.AlbumName,
                ArtistNames = songData.ArtistNames.ToArray(),
                FileNameOnly = songData.ArtistsSongName,
                FileExtension = downloadData.FileExtension,
                NeteaseId = long.Parse(songData.SongId)
            };
        }

        /// <summary>
        /// 163Key用于在网易云音乐中匹配曲库数据
        /// </summary>
        /// <param name="songData"></param>
        /// <param name="downloadData"></param>
        /// <returns></returns>
        private string Calculate163Key(SongData songData, DownloadData downloadData)
        {
            Dictionary<string, object> meta = new()
            {
                { "album", songData.AlbumData["name"] },
                { "albumId", songData.AlbumData["id"] },
                { "albumPic", songData.AlbumData["picUrl"] },
                { "albumPicDocId", songData.AlbumData["pic"] },
                { "musicId", songData["id"] },
                { "musicName", songData["name"] },
                { "mvId", songData["mv"] },
                { "duration", songData["dt"] },
                {
                    "artist", /*$"[{string.Join(",", from ar in ArtistData select string.Format("[{0},{1}]", ar["name"], ar["id"]))}]" */
                    from ar in songData.ArtistData select new List<object>() { ar["name"], ar["id"] }
                },
                { "alias", songData["alia"] },
                { "transNames", Array.Empty<string>() },
                { "format", downloadData["type"] },
                { "bitrate", downloadData["br"] },
                { "mp3DocId", downloadData["md5"] }

            };

            string my163Key = "music:" + JsonConvert.SerializeObject(meta, Formatting.None);
            Console.WriteLine(my163Key);
            return "163 key(Don't modify):" + NeteaseEncrypt.Netease163KeyEncrypt(my163Key);
        }
    }
}
