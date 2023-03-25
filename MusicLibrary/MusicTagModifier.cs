using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace MusicLibrary
{
    public class MusicInformation
    {
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public string Album { get; set; }
        public string Comment { get; set; }
        public Stream ImageStream { get; set; }
        /// <summary>
        /// 输入后缀，输出Mime类型标识
        /// </summary>
        public string PictureMimeMapping 
        { 
            set
            {
                if (!_mappings.ContainsKey(value))
                {
                    throw new KeyNotFoundException("input must be a common picture extension");
                }
                MimeType = _mappings[value];
            }
        }

        public string MimeType { get; set; }
        private static readonly IDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"jpe", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"jpg", "image/jpeg"},
            {"png", "image/png"}
        };
    }


    public static class MusicTagModifier
    {
        public static void WriteMusicTags(string musicFilePath, MusicInformation info)
        {
            var file = TagLib.File.Create(musicFilePath);
            var tag = file.Tag;

            //信息
            if (!string.IsNullOrEmpty(info.Title))
            {
                tag.Title = info.Title;
            }
            if (info.Artists != null && info.Artists.Length > 0)
            {
                tag.Performers = info.Artists;
            }
            if (!string.IsNullOrEmpty(info.Album))
            {
                tag.Album = info.Album;
            }
            if (!string.IsNullOrEmpty(info.Comment))
            {
                tag.Comment = info.Comment;
            }

            //图片
           if (info.ImageStream != null)
            {
                if (string.IsNullOrEmpty(info.MimeType))
                {
                    throw new ArgumentNullException(nameof(info.MimeType), "Mime type of cover image is null or empty");
                }
                Picture pic = new();
                pic.Type = PictureType.FrontCover;
                pic.Description = "Cover";
                pic.MimeType = info.MimeType;

                pic.Data = ByteVector.FromStream(info.ImageStream);
                file.Tag.Pictures = new IPicture[] { pic };
                file.Save();
            }
        }

        public static MusicInformation GetMusicTags(string musicFilePath)
        {
            var file = TagLib.File.Create(musicFilePath);
            var tag = file.Tag;
            MusicInformation info = new()
            {
                Title = tag.Title,
                Artists = tag.Performers,
                Album = tag.Album,
                Comment = tag.Comment,
            };
            if (tag.Pictures.Length > 0)
            {
                var ms = new MemoryStream(tag.Pictures[0].Data.Data);
                ms.Seek(0, SeekOrigin.Begin);
                info.ImageStream = ms;
            }
            return info;
        }

        public static Stream GetMusicCover(string musicFilePath)
        {
            return GetMusicTags(FileManager.GetMusicFilePath(musicFilePath)).ImageStream;
        }
    }
}
