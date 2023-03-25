using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JoMusicCenter.ViewModels
{
    public class FileItemViewModel : NavigatableObject
    {
        private readonly static List<BitmapImage> images = new()
        {
            new BitmapImage(new Uri(@"/Images/folder.png", UriKind.Relative)),//0, folder
            new BitmapImage(new Uri(@"/Images/krystal.jpg", UriKind.Relative)),//1, background
            new BitmapImage(new Uri(@"/Images/blue mosaic.png", UriKind.Relative)),//2, fallback
            new BitmapImage(new Uri(@"/Images/red mosaic.png", UriKind.Relative)),//3, idle cover
        };

        public static List<BitmapImage> Images => images;

        public SongFileMetum? SongFile { get; protected set; }

        public FolderNode? Folder => NavigationNode?.FolderNode;

        public string Name
        {
            get
            {
                if (Folder != null)
                {
                    return Folder.Dirname;
                }
                else if (SongFile != null)
                {
                    return SongFile.SongName;
                }
                else if (NavigationNode != null && !string.IsNullOrEmpty(NavigationNode.NavInfo))
                {
                    return NavigationNode.NavInfo;
                }
                else
                {
                    return "Null";
                }
            }
        }

        public string Tooltip
        {
            get
            {
                if (Folder != null)
                {
                    return Folder.Dirname;
                }
                else if (SongFile != null)
                {
                    return $"{SongFile.SongName} - {string.Join("/", from artist in SongFile.SongArtists select artist.ArtistName)}";
                }
                else if (NavigationNode != null && !string.IsNullOrEmpty(NavigationNode.NavInfo))
                {
                    return NavigationNode.NavInfo;
                }
                else
                {
                    return "Null";
                }
            }
        }
        //public override long Id => Folder != null ? base.Id : (SongFile != null ? SongFile.Id : -1);


        public BitmapImage Cover
        {
            /* https://stackoverflow.com/questions/27641606/loading-a-large-amount-of-images-to-be-displayed-in-a-wrappanel
             * Lazy load images ...
             * Save my day, pal
             */
            get
            {
                if (SongFile != null)
                {
                    BitmapImage cover = new();
                    var coverSource = MusicTagModifier.GetMusicCover(SongFile.FileName);
                    if (coverSource != null)
                    {
                        cover.BeginInit();
                        cover.StreamSource = coverSource;
                        cover.EndInit();
                        return cover;
                    }
                    else
                    {
                        return Images[2];
                    }

                }
                else if (Folder != null || NavigationNode != null)
                {
                    return Images[0];
                }

                else
                {
                    return null!;
                }
            }
        }

        public FileItemViewModel(FolderNode folder)
        {
            //this.Folder = folder;
            NavigationNode = new(folder);
        }

        public FileItemViewModel(SongFileMetum songFile)
        {
            this.SongFile = songFile;
        }

        public FileItemViewModel(NavigationInfo navigationInfo)
        {
            NavigationNode = navigationInfo;
        }
    }
}
