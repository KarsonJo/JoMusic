using System;
using System.Collections.Generic;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class FolderNode
    {
        public FolderNode()
        {
            FileNodes = new HashSet<FileNode>();
            FolderIdMapNews = new HashSet<FolderIdMap>();
            FolderIdMapOlds = new HashSet<FolderIdMap>();
            FolderPathAncestorNavigations = new HashSet<FolderPath>();
            FolderPathDescendantNavigations = new HashSet<FolderPath>();
        }

        public long Id { get; set; }
        public string Dirname { get; set; }

        public virtual ICollection<FileNode> FileNodes { get; set; }
        public virtual ICollection<FolderIdMap> FolderIdMapNews { get; set; }
        public virtual ICollection<FolderIdMap> FolderIdMapOlds { get; set; }
        public virtual ICollection<FolderPath> FolderPathAncestorNavigations { get; set; }
        public virtual ICollection<FolderPath> FolderPathDescendantNavigations { get; set; }
    }
}
