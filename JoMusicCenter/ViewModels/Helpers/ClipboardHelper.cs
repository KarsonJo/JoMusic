using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    internal class ClipboardHelper
    {
        public List<FolderNode> SelectedFolders { get; private set; } = new();
        public List<SongFileMetum> SelectedFiles { get; private set; } = new();
        public List<NavigationInfo> SelectedSpecials { get; private set; } = new();

        public event Action? OnClipBoardChanges;

        /// <summary>
        /// 粘贴板中可用于复制的个数
        /// </summary>
        public int CopyCount => CutCount + SelectedSpecials.Count;
        public bool CopyAny => CutAny || SelectedSpecials.Any();

        /// <summary>
        /// 粘贴板中可用于剪切的个数
        /// </summary>
        public int CutCount => SelectedFolders.Count + SelectedFiles.Count;
        public bool CutAny => SelectedFolders.Any() || SelectedFiles.Any();

        public void SelectItems(IEnumerable<FileItemViewModel> items)
        {
            foreach (var item in items)
            {
                if (item.Folder != null)
                {
                    SelectedFolders.Add(item.Folder);
                }
                else if (item.SongFile != null)
                {
                    SelectedFiles.Add(item.SongFile);
                }
                else if (item.NavigationNode != null)
                {
                    SelectedSpecials.Add(item.NavigationNode);
                }
            }

            OnClipBoardChanges?.Invoke();
        }

        public void Clear()
        {
            SelectedFolders.Clear();
            SelectedFiles.Clear();
            SelectedSpecials.Clear();
        }
    }
}
