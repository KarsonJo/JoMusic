using MusicCrawler.Netease;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JoMusicCenter.ViewModels
{
    public class DownloadDialogueViewModel : PathDialogueViewModel, IDataErrorInfo
    {
        KeyValuePair<UrlHandleType, string> downloadUrl;

        //private string[]? hierarchicalLocation;


        public string Description { get; } =
@"下载网易云的歌曲，目前支持：单曲、歌单
请输入分享链接";



        private bool useCustomPath;

        public bool UseCustomPath
        {
            get => useCustomPath; 
            set 
            { 
                useCustomPath = value;
                if (!value)
                {
                    PathNotification = "正在使用默认路径";
                }
                OnPropertyChanged(nameof(UseCustomPath));
                OnPropertyChanged(nameof(PathNotification));
            }
        }

        public string this[string columnName] => throw new NotImplementedException();

        public string Error => throw new NotImplementedException();

        protected override async Task CommitCommand(object? p)
        {
            if (!Valid)
            {
                MessageBox.Show("参数无效", "下载启动失败", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (hierarchicalLocation == null || hierarchicalLocation.Count == 0)
            {
                MessageBox.Show("下载路径为空", "下载启动失败", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            CloudMusicCrawler downloader = new();
            try
            {
                await downloader.CrawlFromUrlAsync(downloadUrl, hierarchicalLocation);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "下载启动失败", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// 地址输入改变
        /// </summary>
        protected override void MainInputChanged()
        {
            downloadUrl = CloudMusicCrawler.ParseUrlString(InputText);
            if (downloadUrl.Key != UrlHandleType.None)
            {
                switch (downloadUrl.Key)
                {
                    case UrlHandleType.Song:
                        MainNotification = "下载网易云单曲";
                        break;
                    case UrlHandleType.Playlist:
                        MainNotification = "下载网易云歌单";
                        break;
                    default:
                        throw new InvalidEnumArgumentException("为什么会出现这种情况");
                }

                //更改路径
                if (!UseCustomPath)
                {
                    hierarchicalLocation = new(CloudMusicCrawler.DownloadDirectoryPreset[downloadUrl.Key]);
                    PathText = string.Join(@"\", hierarchicalLocation);
                }
                mainInput.NoConflicts = true;
            }
            else
            {
                MainNotification = "地址形式错误";
                mainInput.NoConflicts = false;
            }
        }

        /// <summary>
        /// 路径输入改变
        /// </summary>
        protected override void TextInputChanged()
        {
            if (UseCustomPath)
            {
                //主要是检测非空路径
                if (pathInput.Valid)
                {
                    //转换路径
                    hierarchicalLocation = new(PathText.Split('\\'));
                }
                else
                {
                    PathNotification = $"输入无效";
                    return;
                }
            }

            //显示
            if (hierarchicalLocation != null && hierarchicalLocation.Count > 0)
            {
                PathNotification = $"将记录至如下位置：{string.Join(" -> ", hierarchicalLocation)}";
            }
        }

    }
}
