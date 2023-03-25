using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class FolderIdMap
    {
        public long OldId { get; set; }
        public long NewId { get; set; }

        public virtual FolderNode New { get; set; }
        public virtual FolderNode Old { get; set; }
    }
}
