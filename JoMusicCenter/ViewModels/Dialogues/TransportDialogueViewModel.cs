using MusicCrawler.Download;
using MusicLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    public class TransportDialogueViewModel : PathMappingDialogueViewModel
    {
        public string Header => IsExport ? "导出" : "导入";
        public bool IsExport { get; protected set; }

        protected override Task CommitCommand(object? p)
        {
            throw new NotImplementedException();
        }


    }
}
