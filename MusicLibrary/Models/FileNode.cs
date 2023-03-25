using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class FileNode
    {
        public long SongId { get; set; }
        public long FolderId { get; set; }

        public virtual FolderNode Folder { get; set; }
        public virtual SongFileMetum Song { get; set; }
    }
}
