using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class FolderPath
    {
        public long Ancestor { get; set; }
        public long Descendant { get; set; }
        public long Length { get; set; }

        public virtual FolderNode AncestorNavigation { get; set; }
        public virtual FolderNode DescendantNavigation { get; set; }
    }
}
