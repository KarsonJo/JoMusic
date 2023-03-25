using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class FolderCreate
    {
        public long? Ancestor { get; set; }
        public string Dirname { get; set; }
    }
}
