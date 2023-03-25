using JoMusicCenter.Commands;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace JoMusicCenter.ViewModels
{
    public class SongViewModel:NotifyPropertyChangedObject
    {
        private readonly SongFileMetum songFileMetum;
        private readonly MediaPlayerHelper mediaPlayer;


        public string SongName => songFileMetum.SongName;
        public string AlbumName => songFileMetum.AlbumName;
        public string FileName => songFileMetum.FileName;
        public ICollection<SongArtist> SongArtists => songFileMetum.SongArtists;

        public bool Playing => mediaPlayer.PlayingSong == this;

        public SongViewModel(SongFileMetum songFileMetum, MediaPlayerHelper playerInstance)
        {
            this.songFileMetum = songFileMetum;
            mediaPlayer = playerInstance;

            mediaPlayer.SongSwitched += () =>
                 {
                     OnPropertyChanged(nameof(Playing));
                 };
        }


        private ICommand? switchSongCommand;

        public ICommand SwitchSongCommand
        {
            get
            {
                if (switchSongCommand == null)
                {
                    switchSongCommand = new RelayCommand(null,
                        p =>
                        {
                            mediaPlayer.SeekTo(this);
                        });
                }
                return switchSongCommand;
            }
        }

        private ICommand? removeCommand;

        public ICommand RemoveCommand
        {
            get
            {
                if (removeCommand == null)
                {
                    removeCommand = new RelayCommand(null,
                        p =>
                        {
                            mediaPlayer.RemoveSong(this);
                        });
                }
                return removeCommand;
            }
        }
    }
}
