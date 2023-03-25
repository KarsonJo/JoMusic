using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCrawler.Netease
{
    /// <summary>
    /// 一首歌的Json数据
    /// 以及一些用于快捷获取的属性
    /// </summary>
    public class SongData
    {
        private readonly JToken songData;

        public JToken this[string i]
        {
            get { return songData[i]; }
        }

        public JToken AlbumData => songData["al"];
        public JToken ArtistData => songData["ar"];

        private string RawCoverImageUrl => AlbumData["picUrl"].ToString();
        public string CoverImageUrl => RawCoverImageUrl + "?param=500y500";
        public string CoverImageExtension => RawCoverImageUrl[(RawCoverImageUrl.LastIndexOf(".") + 1)..];

        public string SongId => songData["id"].ToString();
        public string SongName => songData["name"].ToString();
        public string AlbumName => AlbumData["name"].ToString();
        public List<string> ArtistNames => (from artist in ArtistData select artist["name"].ToString()).ToList();
        private string ArtistNamesString => string.Join(", ", ArtistNames);
        public string ArtistsSongName => $"{ArtistNamesString} - {SongName}";

        //public string Netease163Key
        //{
        //    get
        //    {
        //        Dictionary<string, object> meta = new()
        //        {
        //            { "album", AlbumData["name"] },
        //            { "albumId", AlbumData["id"] },
        //            { "albumPic", AlbumData["picUrl"] },
        //            { "albumPicDocId", AlbumData["pic"] },
        //            { "musicId", this["id"] },
        //            { "musicName", this["name"] },
        //            { "mvId", this["mv"] },
        //            { "duration", this["dt"] },
        //            {
        //                "artist", /*$"[{string.Join(",", from ar in ArtistData select string.Format("[{0},{1}]", ar["name"], ar["id"]))}]" */
        //                from ar in ArtistData select new List<object>() { ar["name"], ar["id"] }
        //            },
        //            { "alias", this["alia"] },
        //            { "transNames", Array.Empty<string>() },
        //            { "format", DownloadData["type"] },
        //            { "bitrate", DownloadData["br"] },
        //            { "mp3DocId", DownloadData["md5"] }

        //        };

        //        string my163Key = "music:" + JsonConvert.SerializeObject(meta, Formatting.None);
        //        Console.WriteLine(my163Key);
        //        return "163 key(Don't modify):" + NeteaseEncrypt.Netease163KeyEncrypt(my163Key);
        //    }
        //}
        //public DownloadData DownloadData { get; set; }

        public SongData(JToken oneSongJson)
        {
            songData = oneSongJson;
        }

        //public NeteaseSongStreamData CreateNeteaseStreamData()
        //{
        //    return new NeteaseSongStreamData()
        //    {
        //        SongName = SongName,
        //        AlbumName = AlbumName,
        //        ArtistNames = ArtistNames,
        //        FileNameOnly = ArtistsSongName,
        //        FileExtension = DownloadData.FileExtension,
        //        NeteaseId = long.Parse(SongId)

        //    };
        //}
    }

    /// <summary>
    /// 一首歌下载信息的Json数据
    /// 以及一些用于快捷获取的属性
    /// </summary>
    public class DownloadData
    {
        public DownloadData(JToken oneDownloadData)
        {
            downloadData = oneDownloadData;
        }
        private readonly JToken downloadData;

        public string SongId => downloadData["id"].ToString();
        public string DownloadUrl => downloadData["url"].ToString();
        public string FileExtension => downloadData["type"].ToString().ToLower();
        public string Bitrate => downloadData["br"].ToString();
        public string OriginalMD5 => downloadData["md5"].ToString();
        public JToken this[string i]
        {
            get { return downloadData[i]; }
        }
    }
}
