using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    public class NavigatableObject : NotifyPropertyChangedObject
    {
        public NavigationInfo? NavigationNode { get; protected set; }
    }

    //public class FolderBaseViewModel : NavigatableObject
    //{
    //    //public Func<Task<List<FolderNode>?>>? SpecialLoadLogic { get; protected set; }

    //    public FolderNode? Folder => NavigationNode?.FolderNode;
    //}
}
