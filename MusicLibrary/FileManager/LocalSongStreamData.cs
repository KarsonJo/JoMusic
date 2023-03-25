using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary
{
    /// <summary>
    /// 本地的音乐流文件
    /// 打开时会直接载入流，不需要额外载入
    /// </summary>
    public class LocalSongStreamData : SongStreamData
    {
        private string md5;
        public string LocalMusicFilePath { get; }
        public override string Md5
        {
            get
            {
                //因为不对本地文件做修改，对md5进行缓存
                if (md5 == null)
                {
                    md5 = CalculateMd5(FileStream);
                }
                return md5;
            }
        }

        public override long FileLength => new FileInfo(LocalMusicFilePath).Length;
        public LocalSongStreamData(string localMusicFilePath, bool loadTags = true)
        {
            LocalMusicFilePath = localMusicFilePath;

            LoadStreamFromLocalPath(loadTags);
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }

        private void LoadStreamFromLocalPath(bool loadTags)
        {
            //stream
            FileStream = File.Open(LocalMusicFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            if (loadTags)
            {
                //tags
                var musictags = MusicTagModifier.GetMusicTags(LocalMusicFilePath);

                SongName = musictags.Title;
                AlbumName = musictags.Album;
                ArtistNames = musictags.Artists;
                FileNameOnly = Path.GetFileNameWithoutExtension(LocalMusicFilePath);
                FileExtension = Path.GetExtension(LocalMusicFilePath);
            }
        }
    }
}
