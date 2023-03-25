using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class NeteaseDatum
    {
        public long SongId { get; set; }
        public long NeteaseId { get; set; }

        public virtual SongFileMetum Song { get; set; }
    }
}
