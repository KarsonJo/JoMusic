using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary
{
    public static class AppConfigManager
    {
        public static int NeteaseDownloadQuality
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings[nameof(NeteaseDownloadQuality)]);
            }
            set
            {
                UpdateSetting(nameof(NeteaseDownloadQuality), value.ToString());
            }
        }
        public static string NeteaseCookies
        {
            get
            {
                return ConfigurationManager.AppSettings[nameof(NeteaseCookies)];
            }
            set
            {
                UpdateSetting(nameof(NeteaseCookies), value.ToString());
            }
        }

        public static int NeteaseRetryLimit
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings[nameof(NeteaseRetryLimit)]);
            }
            set
            {
                UpdateSetting(nameof(NeteaseRetryLimit), value.ToString());
            }
        }

        public static double NeteaseRetryBaseDuration
        {
            get
            {
                return double.Parse(ConfigurationManager.AppSettings[nameof(NeteaseRetryBaseDuration)]);
            }
            set
            {
                UpdateSetting(nameof(NeteaseRetryBaseDuration), value.ToString());
            }
        }

        public static bool Write163Key
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings[nameof(Write163Key)]);
            }
            set
            {
                UpdateSetting(nameof(Write163Key), value.ToString());
            }
        }

        public static int NeteaseTransportTaskLimit
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings[nameof(NeteaseTransportTaskLimit)]);
            }
            set
            {
                UpdateSetting(nameof(NeteaseTransportTaskLimit), value.ToString());
            }
        }

        public static int LocalTransportTaskLimit
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings[nameof(LocalTransportTaskLimit)]);
            }
            set
            {
                UpdateSetting(nameof(LocalTransportTaskLimit), value.ToString());
            }
        }

        public static int QueryMaximum
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings[nameof(QueryMaximum)]);
            }
            set
            {
                UpdateSetting(nameof(QueryMaximum), value.ToString());
            }
        }

        public static string MusicDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings[nameof(MusicDirectory)];
            }
            set
            {
                UpdateSetting(nameof(MusicDirectory), value.ToString());
            }
        }

        public static double Volume
        {
            get
            {
                return double.Parse(ConfigurationManager.AppSettings[nameof(Volume)]);
            }
            set
            {
                UpdateSetting(nameof(Volume), value.ToString());
            }
        }
        public static string DefaultMusicDirectory => @".\Download";

        private static void UpdateSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }

        public static void UpdateSettings(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (var pair in keyValuePairs)
            {
                if (configuration.AppSettings.Settings[pair.Key].Value != pair.Value)
                {
                    configuration.AppSettings.Settings[pair.Key].Value = pair.Value;
                }
            }
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
