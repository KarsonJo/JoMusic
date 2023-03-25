using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class SongArtist
    {
        public long SongId { get; set; }
        public string ArtistName { get; set; }

        public virtual SongFileMetum Song { get; set; }
    }
}
