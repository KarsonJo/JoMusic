using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class SongFileMetum
    {
        public SongFileMetum()
        {
            FileNodes = new HashSet<FileNode>();
            SongArtists = new HashSet<SongArtist>();
        }

        public long Id { get; set; }
        public string SongName { get; set; }
        public string AlbumName { get; set; }
        public string FileName { get; set; }
        public string Md5 { get; set; }

        public virtual NeteaseDatum NeteaseDatum { get; set; }
        public virtual ICollection<FileNode> FileNodes { get; set; }
        public virtual ICollection<SongArtist> SongArtists { get; set; }
    }
}
