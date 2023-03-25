using JoMusicCenter.Utilities;
using MusicLibrary;
using MusicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace JoMusicCenter.ViewModels
{
    public enum CirculationMode
    {
        None,
        Playlist,
        Song
    }

    public enum PlayMode
    {
        AllRepeat,
        Order,
        RepectOnce
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-control-a-mediaelement-play-pause-stop-volume-and-speed?view=netframeworkdesktop-4.8
    /// </summary>
    //public class MediaPlayerHelper
    //{
    //    MediaPlayer mediaPlayer = new MediaPlayer();

    //    private int playingPosition = -1;
    //    private bool isPlaying = false;

    //    ObservableCollection<SongViewModel> playlist = new();

    //    DispatcherTimer timer = new();





    //    public ObservableCollection<SongViewModel> Playlist => playlist;

    //    public SongViewModel? PlayingSong => PlayingPosition < 0 ? null : playlist[PlayingPosition];

    //    public bool ShuffleEnable { get; set; } = false;
    //    public CirculationMode CirculationMode { get; set; } = CirculationMode.Playlist;
    //    public PlayMode PlayMode { get; set; } = PlayMode.AllRepeat;

    //    public double mediaTotalSeconds => mediaPlayer.NaturalDuration.HasTimeSpan? mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds : 0;

    //    public double mediaProgressSeconds
    //    { 
    //        get => mediaPlayer.Position.TotalSeconds;
    //        set
    //        {
    //            mediaPlayer.Position = TimeSpan.FromSeconds(value);
    //        }
    //    }

    //    public bool IsPlaying
    //    {
    //        get => isPlaying;
    //        private set
    //        {
    //            if (isPlaying != value)
    //            {
    //                isPlaying = value;
    //                PlayStateChanged?.Invoke();
    //            }
    //        }
    //    }

    //    public int PlayingPosition 
    //    {
    //        get => playingPosition; 
    //        set
    //        {
    //            if (playingPosition != value)
    //            {
    //                playingPosition = value;
    //            }
    //        }
    //    }

    //    public delegate void PlayerEventHandler();

    //    public event PlayerEventHandler? PlayStateChanged;
    //    /// <summary>
    //    /// 当一首歌的文件打开时调用
    //    /// </summary>
    //    public event PlayerEventHandler? SongOpened;
    //    /// <summary>
    //    /// 当歌单中的播放歌曲改变时调用
    //    /// </summary>
    //    public event PlayerEventHandler? SongSwitched;
    //    public event PlayerEventHandler? TickUpdate;

    //    public MediaPlayerHelper()
    //    {
    //        mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
    //        mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;


    //        timer.Interval = TimeSpan.FromSeconds(1);
    //        timer.Tick += Timer_Tick;
    //        timer.Start();

    //    }

    //    private void Timer_Tick(object? sender, EventArgs e)
    //    {
    //        TickUpdate?.Invoke();
    //    }

    //    private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
    //    {
    //        switch (PlayMode)
    //        {
    //            case PlayMode.AllRepeat:
    //                break;
    //            case PlayMode.Order:
    //                break;
    //            case PlayMode.RepectOnce:
    //                break;
    //            default:
    //                break;
    //        }
    //        SeekToAutoPlayNextPosition();
    //        if (LoadMediaOfPlayingPosition())
    //        {
    //            Play();
    //        }
    //    }

    //    private void MediaPlayer_MediaOpened(object? sender, EventArgs e)
    //    {
    //        SongOpened?.Invoke();
    //    }

    //    public bool LoadMediaOfPlayingPosition()
    //    {
    //        if (PlayingPosition < 0)
    //        {
    //            return false;
    //        }
    //        try
    //        {
    //            LoadMedia(playlist[PlayingPosition].FileName);
    //            return true;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }
    //    public void LoadMedia(string FileName)
    //    {
    //        SongSwitched?.Invoke();
    //        mediaPlayer.Open(new Uri(FileManager.GetMusicFilePath(FileName),UriKind.Relative));
    //    }

    //    public void Play()
    //    {
    //        if (playlist.Count == 0)
    //        {
    //            return;
    //        }
    //        if (mediaPlayer.Source == null)
    //        {
    //            SeekToAutoPlayNextPosition();
    //            if (!LoadMediaOfPlayingPosition())
    //            {
    //                return;
    //            }
    //        }



    //        mediaPlayer.Play();
    //        IsPlaying = true;
    //    }

    //    public void Pause()
    //    {
    //        mediaPlayer.Pause();
    //        IsPlaying = false;
    //    }

    //    public void Previous()
    //    {
    //       if (playlist.Count > 0)
    //        {
    //            PlayingPosition = (PlayingPosition - 1).Mod(playlist.Count);

    //            LoadMediaOfPlayingPosition();

    //            Play();
    //        }
    //    }

    //    public void Next()
    //    {
    //        if (playlist.Count > 0)
    //        {
    //            PlayingPosition++;

    //            if (PlayingPosition >= playlist.Count)
    //            {
    //                if (ShuffleEnable)
    //                {
    //                    ShufflePlaylist();
    //                }

    //                PlayingPosition = 0;
    //            }

    //            LoadMediaOfPlayingPosition();


    //            Play();
    //        }
    //    }

    //    public void SeekToAutoPlayNextPosition()
    //    {
    //        if (CirculationMode != CirculationMode.Song)
    //        {
    //            PlayingPosition++;

    //            //playback finished
    //            if (PlayingPosition >= playlist.Count)
    //            {
    //                if (CirculationMode == CirculationMode.None)
    //                {
    //                    PlayingPosition = -1;
    //                }

    //                //else CirculationMode is Playlist, loop back

    //                //shuffle if needed
    //                if (ShuffleEnable)
    //                {
    //                    ShufflePlaylist();
    //                }

    //                PlayingPosition = 0;
    //            }
    //        }
    //    }

    //    public void SeekTo(SongViewModel target)
    //    {
    //        var index = playlist.IndexOf(target);
    //        if (index >= 0)
    //        {
    //            PlayingPosition = index;
    //            //LoadMediaOfPlayingPosition();

    //            //if (IsPlaying)
    //            //{
    //            //    Play();
    //            //}
    //            UpdatePlayingSong();
    //        }
    //    }

    //    /// <summary>
    //    /// 更新播放的歌曲
    //    /// 当操作无条件更变了之前播放的歌曲时需要调用此函数
    //    /// </summary>
    //    private void UpdatePlayingSong()
    //    {
    //        // 如果超出下界，尝试置0
    //        if (playingPosition < 0 && playlist.Count > 0)
    //            playingPosition = 0;

    //        //载入当前位置的歌曲
    //        if (LoadMediaOfPlayingPosition())
    //        {
    //            //正在播放
    //            if (IsPlaying)
    //            {
    //                Play();
    //            }
    //        }
    //        else
    //        {
    //            //暂停，但也许应该进入无歌曲状态
    //            Pause();
    //        }
    //    }

    //    private void ShufflePlaylist()
    //    {
    //        //To prevent a observablecollection update times, make a copy first
    //        List<SongViewModel> tempPlaylist = new(playlist);
    //        playlist.Clear();

    //        tempPlaylist.Shuffle();
    //        foreach (var item in tempPlaylist)
    //        {
    //            playlist.Add(item);
    //        }
    //    }

    //    private void ShufflePlaylist(int startIndex)
    //    {
    //        if (startIndex >= playlist.Count - 1)
    //        {
    //            return;
    //        }

    //        //To prevent a observablecollection update times, make a copy first
    //        List<SongViewModel> inOrderPlaylist = new();
    //        List<SongViewModel> shufflePlaylist = new();

    //        for (int i = 0; i < playlist.Count; i++)
    //        {
    //            if (i < startIndex)
    //            {
    //                inOrderPlaylist.Add(playlist[i]);
    //            }
    //            else
    //            {
    //                shufflePlaylist.Add(playlist[i]);
    //            }
    //        }

    //        playlist.Clear();

    //        shufflePlaylist.Shuffle();

    //        foreach (var item in inOrderPlaylist)
    //        {
    //            playlist.Add(item);
    //        }

    //        foreach (var item in shufflePlaylist)
    //        {
    //            playlist.Add(item);
    //        }
    //    }

    //    #region List Modifier

    //    public void ClearSong()
    //    {
    //        playlist.Clear();
    //    }

    //    public void AddSong(SongFileMetum song)
    //    {
    //        playlist.Add(new(song, this));
    //    }

    //    /// <summary>
    //    /// 移除歌曲
    //    /// </summary>
    //    /// <param name="song"></param>
    //    /// <returns>示意是否影响了正在播放的歌曲</returns>
    //    public bool RemoveSong(SongViewModel song)
    //    {
    //        int removeIndex = playlist.IndexOf(song);



    //        if (removeIndex < 0)
    //        {
    //            return false;
    //        }

    //        bool affectedPlaying = removeIndex == playingPosition;

    //        //affect playing song index?
    //        if (removeIndex <= PlayingPosition)
    //        {
    //            PlayingPosition--;
    //        }
    //        //is last song
    //        else if (PlayingPosition == playlist.Count - 1)
    //        {
    //            PlayingPosition--;
    //        }

    //        playlist.RemoveAt(removeIndex);

    //        //update if affected
    //        if (affectedPlaying)
    //        {
    //            UpdatePlayingSong();
    //        }

    //        return affectedPlaying;
    //    }

    //    public void AddRangeSong(IEnumerable<SongFileMetum> songs)
    //    {
    //        foreach (var song in songs)
    //        {
    //            playlist.Add(new(song, this));
    //        }
    //    }

    //    /// <summary>
    //    /// 移除歌曲
    //    /// </summary>
    //    /// <param name="songs"></param>
    //    /// <returns>示意是否影响了正在播放的歌曲</returns>
    //    public bool RemoveSong(IEnumerable<SongViewModel> songs)
    //    {
    //        //var playingSong = playlist[PlayingPosition];

    //        var set = songs.ToHashSet();

    //        //for (int i = playlist.Count - 1; i >= 0; i--)
    //        //{
    //        //    if (set.Contains(playlist[i]))
    //        //    {
    //        //        playlist.RemoveAt(i);
    //        //    }
    //        //}


    //        ////rebuild position
    //        //int newPosition = playlist.IndexOf(playingSong);
    //        //if (newPosition >= 0)
    //        //{
    //        //    PlayingPosition = newPosition;
    //        //}


    //        //affect playing position
    //        if (set.Contains(playlist[playingPosition]))
    //        {
    //            playingPosition = RemoveBlock() - 1;

    //            //update if affected
    //            UpdatePlayingSong();
    //            return true;
    //        }
    //        else
    //        {
    //            var playingSong = playlist[PlayingPosition];

    //            RemoveBlock();

    //            //rebuild position
    //            int newPosition = playlist.IndexOf(playingSong);
    //            PlayingPosition = newPosition;
    //            return false;
    //        }

    //        //删除的主体，返回删除位置中最先的一个
    //        int RemoveBlock()
    //        {
    //            int frontmost = playlist.Count - 1;
    //            for (int i = frontmost; i >= 0; i--)
    //            {
    //                if (set.Contains(playlist[i]))
    //                {
    //                    playlist.RemoveAt(i);
    //                    frontmost = i;
    //                }
    //            }
    //            return frontmost;
    //        }
    //    }

    //    public void ResetPlayingPosition()
    //    {
    //        PlayingPosition = -1;
    //    }

    //    public void InsertRangeSong(IEnumerable<SongFileMetum> songs)
    //    {
    //        int index = PlayingPosition;
    //        foreach (var song in songs)
    //        {
    //            index++;
    //            playlist.Insert(index, new(song, this));
    //        }
    //    }
    //    #endregion
    //}

    public class MediaPlayerHelper
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer timer = new();

        private bool isPlaying = false;
        private int playingPosition = -1;
        private bool shuffleEnable = false;

        /// <summary>
        /// 原始播放列表
        /// </summary>
        private List<SongViewModel> OriginalPlaylist { get; } = new();

        /// <summary>
        /// 顺序处理过的最终播放列表，可同步UI
        /// </summary>
        public SmartCollection<SongViewModel> Playlist { get; set; } = new();

        /// <summary>
        /// 正在播放的歌曲
        /// </summary>
        public SongViewModel? PlayingSong => PlayingPosition < 0 ? null : Playlist[PlayingPosition];

        /// <summary>
        /// 启用随机播放
        /// </summary>
        public bool ShuffleEnable 
        {
            get => shuffleEnable;
            set 
            {
                shuffleEnable = value;
                if (ShuffleEnable)
                {
                    ShufflePlaylist();
                }
                else
                {
                    RestoreOriginalOrder();
                }
            }
        }

        /// <summary>
        /// 播放模式
        /// </summary>
        public PlayMode PlayMode { get; set; } = PlayMode.AllRepeat;

        /// <summary>
        /// 媒体的总播放时间
        /// </summary>
        public double mediaTotalSeconds => mediaPlayer.NaturalDuration.HasTimeSpan ? mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds : 0;

        /// <summary>
        /// 媒体播放的音量：[0, 1]
        /// </summary>
        public double Volume
        {
            get => mediaPlayer.Volume;
            set => mediaPlayer.Volume = value;
        }

        /// <summary>
        /// 媒体播放器是否静音
        /// </summary>
        public bool IsMuted
        {
            get => mediaPlayer.IsMuted;
            set
            {
                mediaPlayer.IsMuted = value;
                //Debug.WriteLine($"{value} {Volume}");
            }
        }

        /// <summary>
        /// 设置播放时间的位置
        /// </summary>
        public double mediaProgressSeconds
        {
            get => mediaPlayer.Position.TotalSeconds;
            set
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// 是否正在播放歌曲
        /// </summary>
        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    PlayStateChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 播放歌曲的索引
        /// -1作为特殊标志，表示未播放，同时在下次顺序播放时会触发洗牌等动作
        /// </summary>
        public int PlayingPosition
        {
            get => playingPosition;
            set
            {
                if (playingPosition != value)
                {
                    playingPosition = value;
                    //歌曲置空
                    if (PlayingPosition < 0)
                    {
                        SongSwitched?.Invoke();
                        Pause();
                    }
                }
            }
        }

        public delegate void PlayerEventHandler();

        /// <summary>
        /// 播放/暂停状态改变时调用
        /// </summary>
        public event PlayerEventHandler? PlayStateChanged;
        /// <summary>
        /// 当一首歌的成功切换（已打开文件）或置空时调用
        /// </summary>
        public event PlayerEventHandler? SongSwitched;
        ///// <summary>
        ///// 当歌单中的播放歌曲改变时调用
        ///// </summary>
        //public event PlayerEventHandler? SongSwitched;
        /// <summary>
        /// 每Tick（默认为1秒）调用
        /// </summary>
        public event PlayerEventHandler? TickUpdate;

        public MediaPlayerHelper()
        {
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;


            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            mediaPlayer.Volume = AppConfigManager.Volume;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TickUpdate?.Invoke();
        }

        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            //单曲循环？
            if (PlayMode == PlayMode.RepectOnce)
            {
                BeginPlaybackLoadedSong();
            }
            else
            {
                AutoNext();
            }
        }

        private void MediaPlayer_MediaOpened(object? sender, EventArgs e)
        {
            SongSwitched?.Invoke();
        }

        /// <summary>
        /// 载入播放索引i处的媒体
        /// </summary>
        /// <returns></returns>
        private bool LoadMediaOfPlayingPosition()
        {
            if (PlayingPosition < 0 || PlayingPosition >= Playlist.Count)
            {
                return false;
            }
            try
            {
                mediaPlayer.Close();
                mediaPlayer.Open(new Uri(FileManager.GetMusicFilePath(Playlist[PlayingPosition].FileName), UriKind.Relative));
                //SongSwitched?.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 按顺序载入并播放指定索引的歌曲
        /// 如果超出歌单范围会报错
        /// </summary>
        /// <returns></returns>
        private bool SeekToAndPlaybackSong(int playingPosition)
        {
            PlayingPosition = playingPosition;
            var loadSuccess = LoadMediaOfPlayingPosition();
            if (loadSuccess)
            {
                BeginPlaybackLoadedSong();
            }
            return loadSuccess;
        }

        /// <summary>
        /// 尝试载入指定歌曲
        /// 播放与否取决于调用时的状态
        /// </summary>
        /// <param name="target"></param>
        public void SeekTo(SongViewModel target)
        {
            var index = Playlist.IndexOf(target);
            if (index >= 0)
            {
                PlayingPosition = index;
                UpdatePlayingSong();
            }
        }

        /// <summary>
        /// 从头播放载入的歌
        /// </summary>
        private void BeginPlaybackLoadedSong()
        {
            //重置：播放时间为0
            mediaPlayer.Position = TimeSpan.Zero;
            ResumePlaybackLoadedSong();
        }

        /// <summary>
        /// 继续播放载入的歌
        /// </summary>
        private void ResumePlaybackLoadedSong()
        {
            //播放：载入的歌曲
            mediaPlayer.Play();
            IsPlaying = true;
        }

        /// <summary>
        /// 播放歌单歌曲的逻辑
        /// 包括“继续播放”与“开始播放”的处理
        /// </summary>
        public void Play()
        {
            //有歌吗？
            if (Playlist.Count == 0)
            {
                return;
            }

            //新的一轮？
            if (PlayingPosition == -1)
            {
                //随机播放？
                if (ShuffleEnable)
                {
                    //整个歌单洗牌
                    ShufflePlaylist();
                }
                //索引至：下一首歌曲(0)
                PlayingPosition++;
            }
            else
            {
                //媒体已载入？
                if (mediaPlayer.Source != null)
                {
                    //继续播放
                    ResumePlaybackLoadedSong();
                    return;
                }
            }

            //载入歌曲，从头播放
            if (LoadMediaOfPlayingPosition())
            {
                BeginPlaybackLoadedSong();
            }
        }

        /// <summary>
        /// 自动播放下一首
        /// </summary>
        private void AutoNext()
        {
            //最后一首？
            if (PlayingPosition >= Playlist.Count - 1)
            {
                //顺序播放
                if (PlayMode == PlayMode.Order)
                {
                    //重置：播放位置索引为-1
                    PlayingPosition = -1;
                    mediaPlayer.Close();
                }
                //循环播放
                else if (PlayMode == PlayMode.AllRepeat)
                {
                    //重置：播放位置索引为-1（但不触发事件）
                    playingPosition = -1;
                    Play();
                }
            }
            else
            {
                SeekToAndPlaybackSong(PlayingPosition + 1);
            }
        }

        /// <summary>
        /// 手动请求播放下一首
        /// </summary>
        public void Next()
        {
            //最后一首？
            if (PlayingPosition >= Playlist.Count - 1)
            {
                //重置：播放位置索引为-1
                playingPosition = -1;
                Play();
            }
            else
            {
                SeekToAndPlaybackSong(PlayingPosition + 1);
            }
        }

        /// <summary>
        /// 手动请求播放上一首
        /// </summary>
        public void Previous()
        {
            SeekToAndPlaybackSong((PlayingPosition - 1).Mod(Playlist.Count));
        }



        public void Pause()
        {
            mediaPlayer.Pause();
            IsPlaying = false;
        }



        /// <summary>
        /// 更新播放的歌曲
        /// 当操作改变了PlayingPosition但没更变播放歌曲时调用
        /// </summary>
        private void UpdatePlayingSong()
        {
            // 如果超出下界，尝试置0
            if (PlayingPosition < 0 && Playlist.Count > 0)
                PlayingPosition = 0;

            //载入当前位置的歌曲
            if (LoadMediaOfPlayingPosition())
            {
                //正在播放
                if (IsPlaying)
                {
                    Play();
                }
            }
        }

        private void RestoreOriginalOrder()
        {
            if (OriginalPlaylist.Count == 0)
            {
                return;
            }
            //记录正在播放的歌
            var PlayingSong = this.PlayingSong;

            //刷新歌单
            Playlist.Clear();
            Playlist.AddRange(OriginalPlaylist);

            //找回
            if (PlayingSong != null)
            {
                PlayingPosition = Playlist.IndexOf(PlayingSong);
            }
        }

        private void ShufflePlaylist()
        {
            if (OriginalPlaylist.Count == 0)
            {
                return;
            }

            //To prevent a observablecollection update times, make a copy first
            //List<SongViewModel> tempPlaylist = new(Playlist);
            //Playlist.Clear();

            //tempPlaylist.Shuffle();
            //foreach (var item in tempPlaylist)
            //{
            //    Playlist.Add(item);
            //}

            List<SongViewModel> shuffledResult;

            if (PlayingPosition < 0)
            {
                shuffledResult = new(OriginalPlaylist);
                shuffledResult.Shuffle();
            }
            else
            {
                int realPlayingIndex = OriginalPlaylist.IndexOf(Playlist[PlayingPosition]);
                shuffledResult = new();
                // [0, i] 不变
                var keptPart = OriginalPlaylist.GetRange(0, realPlayingIndex + 1);
                shuffledResult.AddRange(keptPart);
                // [i + 1, Count) 随机
                var shuffledPart = OriginalPlaylist.GetRange(realPlayingIndex + 1, OriginalPlaylist.Count - realPlayingIndex - 1);
                shuffledPart.Shuffle();
                shuffledResult.AddRange(shuffledPart);
            }




            Playlist.Clear();
            Playlist.AddRange(shuffledResult);
        }

        #region List Modifier

        public void ClearSong()
        {
            Playlist.Clear();
            OriginalPlaylist.Clear();
        }

        public void AddSong(SongFileMetum song)
        {
            SongViewModel songViewModel = new(song, this);
            Playlist.Add(songViewModel);
            OriginalPlaylist.Add(songViewModel);
        }

        /// <summary>
        /// 移除歌曲
        /// </summary>
        /// <param name="song"></param>
        /// <returns>示意是否影响了正在播放的歌曲</returns>
        public bool RemoveSong(SongViewModel song)
        {
            int removeIndex = Playlist.IndexOf(song);



            if (removeIndex < 0)
            {
                return false;
            }

            bool affectedPlaying = removeIndex == PlayingPosition;

            //影响正在播放的歌曲？
            if (removeIndex <= PlayingPosition)
            {
                PlayingPosition--;
            }
            //是最后一首歌？
            else if (PlayingPosition == Playlist.Count - 1)
            {
                PlayingPosition--;
            }

            Playlist.RemoveAt(removeIndex);
            OriginalPlaylist.Remove(song);

            //更新播放歌曲（如果受影响）
            if (affectedPlaying)
            {
                UpdatePlayingSong();
            }

            return affectedPlaying;
        }

        public void AddRangeSong(IEnumerable<SongFileMetum> songs)
        {
            var adds = (from song in songs select new SongViewModel(song, this)).ToList();
            Playlist.AddRange(adds);
            OriginalPlaylist.AddRange(adds);

            //foreach (var song in songs)
            //{
            //    Playlist.Add(new(song, this));
            //}
        }

        /// <summary>
        /// 移除歌曲
        /// </summary>
        /// <param name="songs"></param>
        /// <returns>示意是否影响了正在播放的歌曲</returns>
        public bool RemoveSong(IEnumerable<SongViewModel> songs)
        {
            //var playingSong = playlist[PlayingPosition];

            var set = songs.ToHashSet();

            //for (int i = playlist.Count - 1; i >= 0; i--)
            //{
            //    if (set.Contains(playlist[i]))
            //    {
            //        playlist.RemoveAt(i);
            //    }
            //}


            ////rebuild position
            //int newPosition = playlist.IndexOf(playingSong);
            //if (newPosition >= 0)
            //{
            //    PlayingPosition = newPosition;
            //}


            //影响播放位置
            if (set.Contains(Playlist[PlayingPosition]))
            {
                PlayingPosition = RemoveBlock() - 1;

                //更新播放的歌曲
                UpdatePlayingSong();
                return true;
            }
            else
            {
                var playingSong = Playlist[PlayingPosition];

                RemoveBlock();

                //重建位置
                int newPosition = Playlist.IndexOf(playingSong);
                PlayingPosition = newPosition;
                return false;
            }

            //删除的主体，返回删除位置中最先的一个
            int RemoveBlock()
            {
                int frontmost = Playlist.Count - 1;
                for (int i = frontmost; i >= 0; i--)
                {
                    if (set.Contains(Playlist[i]))
                    {
                        Playlist.RemoveAt(i);
                        frontmost = i;
                    }
                    if (set.Contains(OriginalPlaylist[i]))
                    {
                        OriginalPlaylist.RemoveAt(i);
                    }
                }
                return frontmost;
            }
        }

        /// <summary>
        /// 重置播放位置，不触发切换事件
        /// </summary>
        public void ResetPlayingPosition()
        {
            playingPosition = -1;
        }

        public void InsertRangeSong(IEnumerable<SongFileMetum> songs)
        {
            var inserts = (from song in songs select new SongViewModel(song, this)).ToList();
            int index = PlayingPosition;

            if (ShuffleEnable)
            {
                foreach (var insert in inserts)
                {
                    index++;
                    Playlist.Insert(index, insert);
                }
                OriginalPlaylist.AddRange(inserts);
            }
            else
            {
                foreach (var insert in inserts)
                {
                    index++;
                    Playlist.Insert(index, insert);
                    OriginalPlaylist.Insert(index, insert);
                }
            }


        }
        #endregion
    }
}
