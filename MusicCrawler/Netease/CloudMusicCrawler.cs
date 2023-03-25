using Microsoft.EntityFrameworkCore;
using MusicCrawler.Download;
using MusicLibrary;
using MusicLibrary.Models;
using MusicLibrary.Netease;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicCrawler.Netease
{
    public enum UrlHandleType
    {
        None,
        Song,
        Playlist,
    }

    public class CloudMusicCrawler
    {
        //private readonly NeteaseEncrypt neteaseEncrypt;
        //private readonly NeteaseFileManager neteaseFileManager = new();


        private readonly static ReadOnlyCollection<string> songDirectoryPreset = new List<string>() { "单曲" }.AsReadOnly();
        private readonly static ReadOnlyCollection<string> playlistDirectoryPreset = new List<string>() { "歌单" }.AsReadOnly();
        public readonly static Dictionary<UrlHandleType, ReadOnlyCollection<string>> DownloadDirectoryPreset = new()
        {
            { UrlHandleType.Song, songDirectoryPreset },
            { UrlHandleType.Playlist, playlistDirectoryPreset }
        };


        public static async Task<string> SendPostRequest(string url, Dictionary<string, object> formData)
        {
            HttpResponseMessage response;


            //try
            //{
            //    string responseText;
            //    Random rnd = new();
            //    for (int i = 1; i <= AppConfigManager.NeteaseRetryLimit; i++)
            //    {
            //        //每次都重新生成加密参数
            //        var formContent = new FormUrlEncodedContent(NeteaseEncrypt.ParamsEncrypt(formData));

            //        //随机等待，瞬间访问会引起网易云怀疑：
            //        await Task.Delay(rnd.Next(i * i * 100, i * i * (int)(AppConfigManager.NeteaseRetryBaseDuration * 1000 + 200)));

            //        //https://stackoverflow.com/questions/35704788/c-sharp-how-to-pass-on-a-cookie-using-a-shared-httpclient
            //        var request = new HttpRequestMessage(HttpMethod.Post, url);
            //        request.Headers.TryAddWithoutValidation("Cookie", AppConfigManager.NeteaseCookies);
            //        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            //        request.Content = formContent;
            //        Debug.WriteLine(request.Headers);
            //        response = await HttpClientHolder.Client.SendAsync(request);
            //        //response = await HttpClientHolder.Client.PostAsync(url, formContent);

            //        response.EnsureSuccessStatusCode();

            //        //网易云有时在连续请求时会返回空
            //        responseText = await response.Content.ReadAsStringAsync();
            //        if (!string.IsNullOrEmpty(responseText))
            //        {
            //            return responseText;
            //        }
            //        //空则继续
            //        Debug.WriteLine($"{url} 返回内容为空，进行第{i + 1}次重试");
            //    }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine($"SendPostRequest error: {e.GetType()}：");
            //    Debug.WriteLine(e.Message);
            //    while (e.InnerException != null)
            //    {
            //        Debug.WriteLine(e.InnerException.Message);
            //        e = e.InnerException;
            //    }
            //}

            string responseText;
            Random rnd = new();
            for (int i = 1; i <= AppConfigManager.NeteaseRetryLimit; i++)
            {
                //每次都重新生成加密参数
                var formContent = new FormUrlEncodedContent(NeteaseEncrypt.ParamsEncrypt(formData));

                //随机等待，瞬间访问会引起网易云怀疑：
                await Task.Delay(rnd.Next(i * i * 100, i * i * (int)(AppConfigManager.NeteaseRetryBaseDuration * 1000 + 200)));

                //https://stackoverflow.com/questions/35704788/c-sharp-how-to-pass-on-a-cookie-using-a-shared-httpclient
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.TryAddWithoutValidation("Cookie", AppConfigManager.NeteaseCookies);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.Content = formContent;
                Debug.WriteLine(request.Headers);
                response = await HttpClientHolder.Client.SendAsync(request);
                //response = await HttpClientHolder.Client.PostAsync(url, formContent);

                response.EnsureSuccessStatusCode();

                //网易云有时在连续请求时会返回空
                responseText = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseText))
                {
                    return responseText;
                }
                //空则继续
                Debug.WriteLine($"{url} 返回内容为空，进行第{i + 1}次重试");
            }

            throw new HttpRequestException($"超过最大重连次数：{AppConfigManager.NeteaseRetryLimit}");
            //return await response.Content.ReadAsStringAsync();
        }

        #region 歌曲获取
        private async Task<(List<string>, string)> GetSongFromPlayListAsync(string playlistId)
        {
            Dictionary<string, object> playlistParams = new()
            {
                { "id", playlistId },
                { "offset", "0" },
                { "total", "false" },
                { "limit", "0" },
                { "n", "0" },
                { "csrf_token", "" }
            };

            string response = await SendPostRequest("https://music.163.com/weapi/v6/playlist/detail", playlistParams);
            
            var trackIds = Regex.Match(response, @"""trackIds""\:\[(.*?)\]").Groups[1].Value;
            var idMatches = Regex.Matches(trackIds, "\"id\":(.*?),");

            var playlistName = Regex.Match(response, "\"name\":\"([^\"]*?)\"").Groups[1].Value;

            return ((from idMatch in idMatches select idMatch.Groups[1].Value).ToList(), playlistName);
        }

        #endregion

        #region 歌曲下载
        private async Task<List<SongData>> GetSongDataAsync(IEnumerable<string> ids)
        {
            Dictionary<string, object> downloadParams = new()
            {
                { "c", $"[{string.Join(",", from id in ids select $"{{\"id\":{ulong.Parse(id) }}}")}]" }, //可以多首歌："[{\"id\":1306386504},{\"id\":1470907433}]"
                { "csrf_token", "" }
            };


            string response = await SendPostRequest("http://music.163.com/weapi/v3/song/detail?csrf_token=", downloadParams);
            Console.WriteLine(response);

            JObject data2 = JObject.Parse(response);


            foreach (var songData in data2["songs"])
            {
                Console.WriteLine(songData["name"]);
                Console.WriteLine(songData["al"]);
                Console.WriteLine(songData["ar"]);
            }


            return (from songs in JObject.Parse(response)["songs"] select new SongData(songs)).ToList();
        }

        /// <summary>

        /// </summary>
        /// <param name="songDatas"></param>
        /// <returns>(需要下载, 不需要下载)</returns>
        private async Task<(List<SongData>, List<SongData>)> FilterSongDataAsync(List<SongData> songDatas)
        {
            var idsInRecord = await NeteaseFileManager.TagUndownloadedSongs(from songData in songDatas select songData.SongId);

            var songToDownload = new List<SongData>();
            var songNotToDownload = new List<SongData>();
            foreach (var songData in songDatas)
            {
                if (idsInRecord.Contains(songData.SongId))
                {
                    songNotToDownload.Add(songData);
                }
                else
                {
                    songToDownload.Add(songData);
                }
            }

            //var songToDownload = (from songData in songDatas
            //                      where idsInRecord.Contains(songData.SongId) == false
            //                      select songData).ToList();

            ////不需要下载
            //if (songToDownload.Count == 0)
            //{
            //    return songToDownload;
            //}

            //foreach (var songData in songToDownload)
            //{
            //    Console.WriteLine($"{songData.SongId} {songData.SongName}");
            //}

            ////请求参数
            //Dictionary<string, object> downloadParams = new()
            //{
            //    { "ids", $"[{string.Join(",", from songData in songToDownload select ulong.Parse(songData.SongId))}]" },
            //    { "level", DownloadQuality.standard.ToString() },
            //    { "encodeType", "aac" },
            //    { "csrf_token", "" }
            //};
            ////加密，然后发送请求
            //string response = await SendPostRequest("https://music.163.com/weapi/song/enhance/player/url/v1?csrf_token=", downloadParams);
            //Console.WriteLine(response);


            //Dictionary<string, SongData> songDataDic = songToDownload.ToDictionary(song => song.SongId);
            //var downloadDatas = (from download in JObject.Parse(response)["data"] select new DownloadData(download)).ToList();

            //foreach (var downloadData in downloadDatas)
            //{
            //    if (songDataDic.TryGetValue(downloadData.SongId, out var songData))
            //    {
            //        songData.DownloadData = downloadData;
            //    }
            //}

            //foreach (var songData in songDatas)
            //{
            //    Console.WriteLine(songData.DownloadData);
            //}

            return (songToDownload, songNotToDownload);
        }

        //private async Task DownloadAsync(IEnumerable<SongData> loadedDatas)
        //{
        //    if (loadedDatas != null)
        //    {
        //        foreach (var songData in loadedDatas)
        //        {
        //            DownloadData downloadData = songData.DownloadData;
        //            if (downloadData != null && !string.IsNullOrEmpty(downloadData.DownloadUrl))
        //            {
        //                Console.WriteLine($"md5 from cloud: {downloadData.OriginalMD5}");
        //                await DownloadMusicFile(songData);
        //            }
        //            else
        //            {
        //                Console.WriteLine($"{songData.ArtistsSongName}({songData.SongId}) 无下载权限");
        //            }

        //        }
        //    }

        //    async Task DownloadMusicFile(SongData songData)
        //    {
        //        string downloadedPath = null;
        //        NeteaseSongStreamData songStreamData = songData.CreateNeteaseStreamData();

        //        //下载到本地（暂时这样吧）
        //        using (Stream scourceStream = await httpClient.GetStreamAsync(songData.DownloadData.DownloadUrl))
        //        {
        //            downloadedPath = await neteaseFileManager.CopyAndSaveFileStreamAsync(songStreamData.FileNameWithExtension, scourceStream);
        //        }

        //        //下载图片，写入歌曲数据
        //        using (Stream imageStream = await httpClient.GetStreamAsync(songData.CoverImageUrl))
        //        {
        //            using (MemoryStream memoryStream = new MemoryStream())
        //            {
        //                await imageStream.CopyToAsync(memoryStream);
        //                memoryStream.Position = 0;
        //                MusicInfomation musicInfomation = new()
        //                {
        //                    Title = songData.SongName,
        //                    Artists = songData.ArtistNames.ToArray(),
        //                    Album = songData.AlbumName,
        //                    Comment = songData.Netease163Key,
        //                    PictureMimeMapping = songData.CoverImageExtension,
        //                    ImageStream = memoryStream
        //                };

        //            MusicTagModifier.WriteMusicTags(downloadedPath, musicInfomation);
        //            }
        //        }

        //        //从本地读出文件
        //        songStreamData.LoadStreamFromPath(downloadedPath);

        //        //写入记录（包含针对网易云的id数据）
        //        List<SongFileMetum> metas = await NeteaseFileManager.WriteSongMetaDataWithNeteaseId(new List<NeteaseSongStreamData>() { songStreamData });
        //    }
        //}
        #endregion

        #region 用户调用
        //public async Task TestDownloadSongDataAsync(List<string> ids)
        //{
        //    var allSongDatas = await GetSongDataAsync(ids);
        //    var data =  await GetAndLoadDownloadDataAsync(allSongDatas);
        //    await DownloadAsync(data);


        //    //记录到虚拟文件夹
        //    await neteaseFileManager.RecordToVirtualFolder(
        //        await neteaseFileManager.RetrieveSongFileMetaByNeteaseId(
        //            from songData in allSongDatas
        //            select songData.SongId),
        //        new string[] { "单曲" });
        //}


        public static KeyValuePair<UrlHandleType, string> ParseUrlString(string urlString)
        {
            Match match;

            match = Regex.Match(urlString, @"song\?id=(\d*)");
            if (match.Success && match.Groups.Count > 0)
            {
                return new(UrlHandleType.Song, match.Groups[1].Value);
            }

            match = Regex.Match(urlString, @"playlist\?id=(\d*)");
            if (match.Success && match.Groups.Count > 0)
            {
                return new(UrlHandleType.Playlist, match.Groups[1].Value);
            }

            return new(UrlHandleType.None, null);
        }

        public async Task CrawlFromUrlAsync(KeyValuePair<UrlHandleType, string> handleInfo, List<string> hierarchicalLocation = null)
        {
            List<SongData> allSongDatas = null;
            List<SongData> songToDownload = null;
            List<SongData> songInRecord = null;

            try
            {
                switch (handleInfo.Key)
                {
                    case UrlHandleType.None:
                        break;
                    case UrlHandleType.Song:
                        allSongDatas = await GetSongDataAsync(new List<string>() { handleInfo.Value });
                        (songToDownload, songInRecord) = await FilterSongDataAsync(allSongDatas);
                        if (hierarchicalLocation == null)
                        {
                            hierarchicalLocation = new(DownloadDirectoryPreset[UrlHandleType.Song]);
                        }
                        //await 
                        break;
                    case UrlHandleType.Playlist:
                        (List<string> songIds, string playlistName) = await GetSongFromPlayListAsync(handleInfo.Value);
                        allSongDatas = await GetSongDataAsync(songIds);
                        (songToDownload, songInRecord) = await FilterSongDataAsync(allSongDatas);
                        if (hierarchicalLocation == null)
                        {
                            hierarchicalLocation = new(DownloadDirectoryPreset[UrlHandleType.Playlist]);
                        }
                        hierarchicalLocation.Add(playlistName);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"SendPostRequest error: {e.GetType()}：");
                Debug.WriteLine(e.Message);
                while (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message);
                    e = e.InnerException;
                }
                throw;
            }

            ////下载需要下载的歌
            //await DownloadAsync(downloadSongDatas);

            ////记录到虚拟文件夹
            //await neteaseFileManager.RecordToVirtualFolder(
            //    await neteaseFileManager.RetrieveSongFileMetaByNeteaseId(
            //        from songData in allSongDatas 
            //        select songData.SongId),
            //    hierarchicalLocation);

            //先记录已在库的歌
            await FileManager.RecordToVirtualFolder(
                await NeteaseFileManager.RetrieveSongFileMetaByNeteaseId(
                    from songData in songInRecord select songData.SongId),
                hierarchicalLocation.ToArray());

            //====================用于测试，应该由DownloadManager执行=======================
            //下载需要下载的歌
            foreach (var song in songToDownload)
            {
                var downloadTask = new NeteaseDownloadTask(song);
                downloadTask.DownloadCompleted += DownloadTask_DownloadCompleted;
                //downloadTask.MadeProgress += () => { Debug.WriteLine($"downloading: {downloadTask.TaskName}, progress: {downloadTask.Progress}"); };
                DownloadManager.AddTask(DownloadTaskType.Netease, downloadTask);

            }

            async void DownloadTask_DownloadCompleted(object sender, EventArgs e)
            {
                //取出结果数据
                var songStreamData = ((NeteaseDownloadTask)sender).DownloadedContent;
                //记录到文件夹
                await FileManager.RecordToVirtualFolder(songStreamData, hierarchicalLocation.ToArray());
            }
        }



        #endregion
    }
}
