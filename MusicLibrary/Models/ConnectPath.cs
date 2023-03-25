using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class ConnectPath
    {
        public long? Ancestor { get; set; }
        public long? Descendant { get; set; }
    }
}
