using MusicCrawler.Download;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JoMusicCenter.ViewModels
{
    public class DownloadItemViewModel : NotifyPropertyChangedObject
    {
        private static readonly SolidColorBrush successBursh = new(Colors.Green);
        private static readonly SolidColorBrush failBrush = new(Colors.Red);
        private static readonly SolidColorBrush downloadingBrush = new(Colors.DodgerBlue);
        private static readonly SolidColorBrush inactiveBrush = new(Colors.Gray);

        public DownloadTask DownloadTask { get; private set; }

        public string FileName => DownloadTask.TaskName;

        public double Progress => DownloadTask.Progress;
        public string? Notification => DownloadTask.Notification;

        public string Size => DownloadTask.FileSizeMB.ToString("0.##") + "MB";

        public string State { get; private set; } = "状态未定义";
        public Brush StateBrush { get; private set; } = inactiveBrush;

        public DownloadItemViewModel(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;

            downloadTask.GotFileSize += () => { OnPropertyChanged(nameof(Size)); };
            downloadTask.MadeProgress += () => { OnPropertyChanged(nameof(Progress)); };
            downloadTask.NotificationChanged += () => { OnPropertyChanged(nameof(Notification)); };
            downloadTask.StateChanged += () =>
            {
                UpdateState(downloadTask.State);
            };

            UpdateState(downloadTask.State);
        }

        void UpdateState(TaskState state)
        {
            switch (state)
            {
                case TaskState.Started:
                    State = "下载中";
                    StateBrush = downloadingBrush;
                    break;
                case TaskState.Paused:
                    State = "已暂停";
                    StateBrush = inactiveBrush;
                    break;
                case TaskState.Waiting:
                    State = "等待中";
                    StateBrush = inactiveBrush;
                    break;
                case TaskState.Failed:
                    State = "未完成";
                    StateBrush = failBrush;
                    break;
                case TaskState.Completed:
                    State = "已完成";
                    StateBrush = successBursh;
                    break;
                default:
                    State = "状态未定义";
                    StateBrush = inactiveBrush;
                    break;
            }
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(StateBrush));
        }
    }
}
