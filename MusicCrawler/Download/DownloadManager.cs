using MusicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCrawler.Download
{


    //public static class DownloadManager
    //{
    //    public static int ParallelDownloadLimit { get; set; } = 2;
    //    public static List<DownloadTask> DownloadTasks { get; private set; } = new();

    //    public delegate void ListModifyEventHandler(HashSet<DownloadTask> changed);
    //    public static event ListModifyEventHandler DownloadListAdded;
    //    public static event ListModifyEventHandler DownloadListRemoved;

    //    private static void RegistryTaskWithoutStart(DownloadTask downloadTask)
    //    {
    //        downloadTask.StateChanged += () =>
    //        {
    //            //状态改变时，可能可以增加新的并行任务
    //            if (downloadTask.State != TaskState.Started)
    //            {
    //                StartNextDownloads();
    //            }
    //        };
    //        DownloadTasks.Add(downloadTask);
    //    }
    //    public static void AddTask(DownloadTask downloadTask)
    //    {
    //        if (downloadTask != null)
    //        {
    //            RegistryTaskWithoutStart(downloadTask);
    //            DownloadListAdded?.Invoke(new HashSet<DownloadTask>() { downloadTask });
    //            StartNextDownloads();
    //        }

    //    }

    //    public static void AddRangeTask(IEnumerable<DownloadTask> downloadTasks)
    //    {
    //        HashSet<DownloadTask> added = new();
    //        foreach (var downloadTask in downloadTasks)
    //        {
    //            RegistryTaskWithoutStart(downloadTask);
    //            added.Add(downloadTask);
    //        }

    //        if (added.Count > 0)
    //        {
    //            DownloadListAdded?.Invoke(added);
    //            StartNextDownloads();
    //        }
    //    }

    //    public static void RemoveTask(DownloadTask downloadTask)
    //    {
    //        if (downloadTask != null)
    //        {
    //            DownloadTasks.Remove(downloadTask);
    //            DownloadListRemoved?.Invoke(new HashSet<DownloadTask>() { downloadTask });
    //            StartNextDownloads();
    //        }
    //    }

    //    public static void RemoveTask(IEnumerable<DownloadTask> downloadTasks)
    //    {
    //        var deleted = downloadTasks.ToHashSet();
    //        if (deleted.Count > 0)
    //        {
    //            for (int i = DownloadTasks.Count - 1; i >= 0; i--)
    //            {
    //                if (deleted.Contains(DownloadTasks[i]))
    //                {
    //                    DownloadTasks.RemoveAt(i);
    //                }
    //            }
    //            DownloadListRemoved?.Invoke(deleted);
    //            StartNextDownloads();
    //        }

    //    }

    //    private static void StartNextDownloads()
    //    {
    //        int downloadingCount = (from downloadTask in DownloadTasks where downloadTask.State == TaskState.Started select 1).Count();
    //        foreach (var downloadTask in DownloadTasks)
    //        {
    //            if (downloadingCount >= ParallelDownloadLimit)
    //            {
    //                break;
    //            }
    //            else if (downloadTask.State == TaskState.Waiting)
    //            {
    //                downloadTask.StartDownload();
    //                downloadingCount++;
    //            }
    //        }
    //    }
    //}

    public enum DownloadTaskType
    {
        Local,
        Netease
    }

    /// <summary>
    /// 集中管理所有下载队列（及其任务）的管理类
    /// </summary>
    public static class DownloadManager
    {
        static Dictionary<string, DownloadList> typedTaskList = new();
        public static int NewQueueDefaultLimit = 5;

        public delegate void ListModifyEventHandler(string queueId, HashSet<DownloadTask> changed);
        public static event ListModifyEventHandler DownloadListAdded;
        public static event ListModifyEventHandler DownloadListRemoved;

        static DownloadManager()
        {
            //预制的队列
            RegistryDownloadList(DownloadTaskType.Local.ToString(), new LocalDownloadList());
            RegistryDownloadList(DownloadTaskType.Netease.ToString(), new NeteaseDownloadList());
        }

        /// <summary>
        /// 根据类型获取一个下载队列，不存在时会创建一个长度为0的队列
        /// (Lazy create)
        /// </summary>
        /// <param name="queueType"></param>
        /// <returns></returns>
        public static DownloadList GetDownloadList(DownloadTaskType queueType)
        {
            return GetOrCreateDownloadList(queueType.ToString());
        }

        /// <summary>
        /// 根据名字获取一个下载队列，不存在时会返回空
        /// (No create)
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public static DownloadList GetDownloadList(string queueId)
        {
            typedTaskList.TryGetValue(queueId, out DownloadList list);
            return list;
        }

        /// <summary>
        /// 创建新队列，若已存在不做任何改动
        /// 因为涉及事件订阅，所有受管理的队列都应该从该函数创建
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="limit"></param>
        /// <returns>是否成功创建</returns>
        public static bool CreateAndRegistryDownloadList(string queueId, int limit)
        {
            if (!typedTaskList.ContainsKey(queueId))
            {
                DownloadList downloadList = new(limit);
                return RegistryDownloadList(queueId, downloadList);
            }
            return false;
        }

        public static bool RegistryDownloadList(string queueId, DownloadList downloadList)
        {
            if (!typedTaskList.ContainsKey(queueId))
            {
                typedTaskList[queueId] = downloadList;

                //订阅事件
                typedTaskList[queueId].DownloadListAdded += (changed) => DownloadListAdded?.Invoke(queueId, changed);
                typedTaskList[queueId].DownloadListRemoved += (changed) => DownloadListRemoved?.Invoke(queueId, changed);
                return true;
            }
            return false;
        }

        private static DownloadList GetOrCreateDownloadList(string queueId)
        {
            CreateAndRegistryDownloadList(queueId, NewQueueDefaultLimit);
            return typedTaskList[queueId];
        }

        /// <summary>
        /// 创建一个任务，队列不存在时会创建
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="downloadTask"></param>
        public static void AddTask(string queueId, DownloadTask downloadTask)
        {
            GetOrCreateDownloadList(queueId).AddTask(downloadTask);
        }

        /// <summary>
        /// 创建一系列任务，队列不存在时会创建
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="downloadTasks"></param>
        public static void AddRangeTask(string queueId, IEnumerable<DownloadTask> downloadTasks)
        {
            GetOrCreateDownloadList(queueId).AddRangeTask(downloadTasks);
        }

        /// <summary>
        /// 移除一个任务，队列不存在时无事发生
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="downloadTask"></param>
        public static void RemoveTask(string queueId, DownloadTask downloadTask)
        {
            if (typedTaskList.ContainsKey(queueId))
            {
                GetOrCreateDownloadList(queueId).RemoveTask(downloadTask);
            }
        }

        /// <summary>
        /// 移除一系列任务，队列不存在时无事发生
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="downloadTasks"></param>
        public static void RemoveTask(string queueId, IEnumerable<DownloadTask> downloadTasks)
        {
            if (typedTaskList.ContainsKey(queueId))
            {
                GetOrCreateDownloadList(queueId).RemoveTask(downloadTasks);
            }
        }

        /// <summary>
        /// 创建一个任务
        /// </summary>
        /// <param name="queueType"></param>
        /// <param name="downloadTask"></param>
        public static void AddTask(DownloadTaskType queueType, DownloadTask downloadTask)
        {
            GetDownloadList(queueType).AddTask(downloadTask);
        }

        /// <summary>
        /// 创建一系列任务
        /// </summary>
        /// <param name="queueType"></param>
        /// <param name="downloadTasks"></param>
        public static void AddRangeTask(DownloadTaskType queueType, IEnumerable<DownloadTask> downloadTasks)
        {
            GetDownloadList(queueType).AddRangeTask(downloadTasks);
        }

        /// <summary>
        /// 移除一个任务
        /// </summary>
        /// <param name="queueType"></param>
        /// <param name="downloadTask"></param>
        public static void RemoveTask(DownloadTaskType queueType, DownloadTask downloadTask)
        {
            GetDownloadList(queueType).RemoveTask(downloadTask);
        }

        /// <summary>
        /// 移除一系列任务
        /// </summary>
        /// <param name="queueType"></param>
        /// <param name="downloadTasks"></param>
        public static void RemoveTask(DownloadTaskType queueType, IEnumerable<DownloadTask> downloadTasks)
        {
            GetDownloadList(queueType).RemoveTask(downloadTasks);
        }
    }

    /// <summary>
    /// 一个下载队列，进行队列中任务的控制
    /// </summary>
    public class DownloadList
    {
        public virtual int ParallelDownloadLimit { get; set; } = 2;
        public List<DownloadTask> DownloadTasks { get; private set; } = new();

        public delegate void ListModifyEventHandler(HashSet<DownloadTask> changed);
        public event ListModifyEventHandler DownloadListAdded;
        public event ListModifyEventHandler DownloadListRemoved;

        public DownloadList() { }

        public DownloadList(int downloadLimit)
        {
            ParallelDownloadLimit = downloadLimit;
        }

        private void RegistryTaskWithoutStart(DownloadTask downloadTask)
        {
            downloadTask.StateChanged += () =>
            {
                //状态改变时，可能可以增加新的并行任务
                if (downloadTask.State != TaskState.Started)
                {
                    StartNextDownloads();
                }
            };
            DownloadTasks.Add(downloadTask);
        }
        public void AddTask(DownloadTask downloadTask)
        {
            if (downloadTask != null)
            {
                RegistryTaskWithoutStart(downloadTask);
                DownloadListAdded?.Invoke(new HashSet<DownloadTask>() { downloadTask });
                StartNextDownloads();
            }

        }

        public void AddRangeTask(IEnumerable<DownloadTask> downloadTasks)
        {
            HashSet<DownloadTask> added = new();
            foreach (var downloadTask in downloadTasks)
            {
                RegistryTaskWithoutStart(downloadTask);
                added.Add(downloadTask);
            }

            if (added.Count > 0)
            {
                DownloadListAdded?.Invoke(added);
                StartNextDownloads();
            }
        }

        public void RemoveTask(DownloadTask downloadTask)
        {
            if (downloadTask != null)
            {
                DownloadTasks.Remove(downloadTask);
                DownloadListRemoved?.Invoke(new HashSet<DownloadTask>() { downloadTask });
                StartNextDownloads();
            }
        }

        public void RemoveTask(IEnumerable<DownloadTask> downloadTasks)
        {
            var deleted = downloadTasks.ToHashSet();
            if (deleted.Count > 0)
            {
                for (int i = DownloadTasks.Count - 1; i >= 0; i--)
                {
                    if (deleted.Contains(DownloadTasks[i]))
                    {
                        DownloadTasks.RemoveAt(i);
                    }
                }
                DownloadListRemoved?.Invoke(deleted);
                StartNextDownloads();
            }

        }

        private void StartNextDownloads()
        {
            int downloadingCount = (from downloadTask in DownloadTasks where downloadTask.State == TaskState.Started select 1).Count();
            foreach (var downloadTask in DownloadTasks)
            {
                if (downloadingCount >= ParallelDownloadLimit)
                {
                    break;
                }
                else if (downloadTask.State == TaskState.Waiting)
                {
                    downloadTask.StartDownload();
                    downloadingCount++;
                }
            }
        }
    }

    public class LocalDownloadList : DownloadList
    {
        public override int ParallelDownloadLimit { get => AppConfigManager.LocalTransportTaskLimit; set => AppConfigManager.LocalTransportTaskLimit = value; }
    }

    public class NeteaseDownloadList : DownloadList
    {
        public override int ParallelDownloadLimit { get => AppConfigManager.NeteaseTransportTaskLimit; set => AppConfigManager.NeteaseTransportTaskLimit = value; }
    }
}
