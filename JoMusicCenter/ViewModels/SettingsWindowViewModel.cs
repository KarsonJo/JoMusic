using JoMusicCenter.Commands;
using MusicLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    public class SettingsWindowViewModel : NotifyPropertyChangedObject
    {
        public ObservableCollection<int> Skip5Ints { get; } = new() { 5, 10, 15, 20, 25, 30 };
        public ObservableCollection<int> Skip1Ints { get; } = new() { 1, 2, 3, 4, 5, 6 };

        Dictionary<string, string> updatedValues = new();

        public int LocalTransportTaskLimit
        {
            get
            {
                return int.Parse(updatedValues[nameof(AppConfigManager.LocalTransportTaskLimit)]);
            }
            set
            {
                if (LocalTransportTaskLimit == value)
                {
                    return;
                }
                if (Skip5Ints.Contains(value))
                {
                    updatedValues[nameof(AppConfigManager.LocalTransportTaskLimit)] = value.ToString();
                }
                OnPropertyChanged(nameof(LocalTransportTaskLimit));
            }
        }

        public int NeteaseTransportTaskLimit
        {
            get => int.Parse(updatedValues[nameof(AppConfigManager.NeteaseTransportTaskLimit)]);
            set
            {
                if (NeteaseTransportTaskLimit == value)
                {
                    return;
                }
                if (Skip1Ints.Contains(value))
                {
                    updatedValues[nameof(AppConfigManager.NeteaseTransportTaskLimit)] = value.ToString();
                }
                OnPropertyChanged(nameof(NeteaseTransportTaskLimit));
            }
        }

        public int NeteaseRetryLimit
        {
            get => int.Parse(updatedValues[nameof(AppConfigManager.NeteaseRetryLimit)]);
            set
            {
                if (NeteaseRetryLimit == value)
                {
                    return;
                }
                if (Skip1Ints.Contains(value))
                {
                    updatedValues[nameof(AppConfigManager.NeteaseRetryLimit)] = value.ToString();
                }
                OnPropertyChanged(nameof(NeteaseRetryLimit));
            }
        }

        public string NeteaseRetryBaseDuration
        {
            get => updatedValues[nameof(AppConfigManager.NeteaseRetryBaseDuration)];
            set
            {
                if (NeteaseRetryBaseDuration == value)
                {
                    return;
                }
                if (double.TryParse(value, out double _))
                {
                    updatedValues[nameof(AppConfigManager.NeteaseRetryBaseDuration)] = value;
                }
                OnPropertyChanged(nameof(NeteaseRetryBaseDuration));
            }
        }

        public int NeteaseDownloadQuality
        {
            get => int.Parse(updatedValues[nameof(AppConfigManager.NeteaseDownloadQuality)]);
            set
            {
                if (NeteaseDownloadQuality == value)
                {
                    return;
                }
                updatedValues[nameof(AppConfigManager.NeteaseDownloadQuality)] = value.ToString();
                OnPropertyChanged(nameof(NeteaseDownloadQuality));
            }
        }

        public bool Write163Key
        {
            get => bool.Parse(updatedValues[nameof(AppConfigManager.Write163Key)]);
            set
            {
                if (Write163Key == value)
                {
                    return;
                }
                updatedValues[nameof(AppConfigManager.Write163Key)] = value.ToString();
                OnPropertyChanged(nameof(Write163Key));
            }
        }
        public string NeteaseCookies
        {
            get => updatedValues[nameof(AppConfigManager.NeteaseCookies)];
            set
            {
                if (NeteaseCookies == value)
                {
                    return;
                }
                updatedValues[nameof(AppConfigManager.NeteaseCookies)] = value.ToString();
                OnPropertyChanged(nameof(NeteaseCookies));
            }
        }

        public string MaxQueryLimit
        {
            get => updatedValues[nameof(AppConfigManager.QueryMaximum)];
            set
            {
                if (MaxQueryLimit == value)
                {
                    return;
                }
                if (int.TryParse(value, out int _))
                {
                    updatedValues[nameof(AppConfigManager.QueryMaximum)] = value;
                }
                OnPropertyChanged(nameof(MaxQueryLimit));
            }
        }

        public string MusicDirectory
        {
            get => updatedValues[nameof(AppConfigManager.MusicDirectory)];
            set
            {
                if (MusicDirectory == value)
                {
                    return;
                }
                if (Directory.Exists(value))
                {
                    updatedValues[nameof(AppConfigManager.MusicDirectory)] = value.ToString();
                }
                OnPropertyChanged(nameof(MusicDirectory));
            }
        }

        private ICommand? commitCommand;

        public ICommand CommitCommand
        {
            get
            {
                if (commitCommand == null)
                {
                    commitCommand = new RelayCommand(null,
                        p =>
                        {
                            AppConfigManager.UpdateSettings(updatedValues);
                        });
                }
                return commitCommand;
            }
        }

        private ICommand? cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(null,
                        p =>
                        {
                            ResetValues();
                            OnPropertyChanged(nameof(LocalTransportTaskLimit));
                            OnPropertyChanged(nameof(NeteaseTransportTaskLimit));
                            OnPropertyChanged(nameof(NeteaseRetryLimit));
                            OnPropertyChanged(nameof(NeteaseRetryBaseDuration));
                            OnPropertyChanged(nameof(NeteaseDownloadQuality));
                            OnPropertyChanged(nameof(Write163Key));
                            OnPropertyChanged(nameof(NeteaseCookies));
                            OnPropertyChanged(nameof(MaxQueryLimit));
                            OnPropertyChanged(nameof(MusicDirectory));
                        });
                }
                return cancelCommand;
            }
        }

        public SettingsWindowViewModel()
        {
            ResetValues();
        }

        void ResetValues()
        {
            updatedValues[nameof(AppConfigManager.LocalTransportTaskLimit)] = AppConfigManager.LocalTransportTaskLimit.ToString();
            updatedValues[nameof(AppConfigManager.NeteaseTransportTaskLimit)] = AppConfigManager.NeteaseTransportTaskLimit.ToString();
            updatedValues[nameof(AppConfigManager.NeteaseRetryLimit)] = AppConfigManager.NeteaseRetryLimit.ToString();
            updatedValues[nameof(AppConfigManager.NeteaseRetryBaseDuration)] = AppConfigManager.NeteaseRetryBaseDuration.ToString("0.##");
            updatedValues[nameof(AppConfigManager.NeteaseDownloadQuality)] = AppConfigManager.NeteaseDownloadQuality.ToString();
            updatedValues[nameof(AppConfigManager.Write163Key)] = AppConfigManager.Write163Key.ToString();
            updatedValues[nameof(AppConfigManager.NeteaseCookies)] = AppConfigManager.NeteaseCookies.ToString();
            updatedValues[nameof(AppConfigManager.QueryMaximum)] = AppConfigManager.QueryMaximum.ToString();
            updatedValues[nameof(AppConfigManager.MusicDirectory)] = AppConfigManager.MusicDirectory.ToString();
        }
    }
}
