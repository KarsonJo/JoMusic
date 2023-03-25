using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class DirectSubFolder
    {
        public long? Ancestor { get; set; }
        public long? Descendant { get; set; }
        public string DescendantName { get; set; }
    }
}
