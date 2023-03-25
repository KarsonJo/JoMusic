using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary
{
    /// <summary>
    /// 歌曲信息+流文件
    /// </summary>
    public class SongStreamData
    {
        private string fileName;
        private string songName;
        string fileExtension;

        /// <summary>
        /// 当SongName没有赋值时，尝试返回fileName
        /// </summary>
        public string SongName 
        { 
            get
            {
                if (songName != null)
                {
                    return songName;
                }
                else
                {
                    return FileNameOnly;
                }
            }
            set => songName = value; 
        }
        public string AlbumName { get; set; }

        public string FileExtension
        {
            get => fileExtension;
            set
            {
                fileExtension = value.TrimStart('.');
            }
        }
        public virtual string Md5 => CalculateMd5(FileStream);

        /// <summary>
        /// 获取文件大小，默认抛出异常，需要重写
        /// </summary>
        public virtual long FileLength => throw new NotImplementedException();

        private string ArtistString => string.Join(", ", ArtistNames);
        private string ArtistsSongName => $"{ArtistString} - {SongName}";
        public string FileNameWithExtension
        {
            get
            {
                if (string.IsNullOrEmpty(FileExtension))
                {
                    throw new ArgumentNullException(nameof(FileExtension));
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    return string.Concat($"{ArtistsSongName}.{FileExtension}".Split(@"[\/:*?""<>|]"));
                }
                else
                {
                    return string.Concat($"{fileName}.{FileExtension}");
                }
            }
        }

        public string FileNameOnly
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = string.Concat(value.Split(@"[\/:*?""<>|]"));
            }
        }
        public string[] ArtistNames { get; set; }

        public Stream FileStream { get; set; }

        public SongFileMetum GetSongFileMetum()
        {
            return new SongFileMetum()
            {
                SongName = SongName,
                AlbumName = AlbumName,
                FileName = FileNameWithExtension,
                Md5 = Md5,
            };
        }

        public List<SongArtist> GetSongArtists()
        {
            List<SongArtist> artists = new();
            foreach (var artist in ArtistNames)
            {
                artists.Add(new SongArtist()
                {
                    ArtistName = artist
                });
            }
            return artists;
        }
        /// <summary>
        /// 从头计算MD5，并把Stream指针置于起始位置
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static string CalculateMd5(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }
            stream.Position = 0;
            using var md5 = MD5.Create();
            var byteHash = md5.ComputeHash(stream);
            stream.Position = 0;
            return Convert.ToHexString(byteHash);
        }

        public static async Task<string> CalculateMd5Async(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }
            stream.Position = 0;
            using var md5 = MD5.Create();
            var byteHash = await md5.ComputeHashAsync(stream);
            stream.Position = 0;
            return Convert.ToHexString(byteHash);
        }

        public void LoadStreamFromPath(string filePath)
        {
            FileStream = File.Open(filePath, FileMode.Open);
        }
    }
}
