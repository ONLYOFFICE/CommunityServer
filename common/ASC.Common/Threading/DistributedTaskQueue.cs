/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.Common.Caching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Common.Threading
{
    public class DistributedTaskQueue
    {
        public static readonly string InstanseId;

        private readonly string key;
        private readonly ICache cache;
        private readonly ICacheNotify notify;
        private readonly TaskScheduler scheduler;
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> cancelations = new ConcurrentDictionary<string, CancellationTokenSource>();


        static DistributedTaskQueue()
        {
            InstanseId = Process.GetCurrentProcess().Id.ToString();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of queue</param>
        /// <param name="maxThreadsCount">limit of threads count; Default: -1 - no limit</param>
        public DistributedTaskQueue(string name, int maxThreadsCount = -1)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            key = name + GetType().Name;
            cache = AscCache.Default;
            notify = AscCache.Notify;
            scheduler = maxThreadsCount <= 0
                ? TaskScheduler.Default
                : new LimitedConcurrencyLevelTaskScheduler(maxThreadsCount);

            notify.Subscribe<DistributedTaskCancelation>((c, a) =>
            {
                CancellationTokenSource s;
                if (cancelations.TryGetValue(c.Id, out s))
                {
                    s.Cancel();
                }
            });
        }


        public void QueueTask(Action<DistributedTask, CancellationToken> action, DistributedTask distributedTask = null)
        {
            if (distributedTask == null)
            {
                distributedTask = new DistributedTask();
            }

            distributedTask.InstanseId = InstanseId;

            var cancelation = new CancellationTokenSource();
            var token = cancelation.Token;
            cancelations[distributedTask.Id] = cancelation;

            var task = new Task(() => action(distributedTask, token), token, TaskCreationOptions.LongRunning);
            task
                .ConfigureAwait(false)
                .GetAwaiter()
                .OnCompleted(() => OnCompleted(task, distributedTask.Id));

            distributedTask.Status = DistributedTaskStatus.Running;

            if (distributedTask.Publication == null)
            {
                distributedTask.Publication = GetPublication();
            }
            distributedTask.PublishChanges();

            task.Start(scheduler);
        }

        public void CancelTask(string id)
        {
            notify.Publish(new DistributedTaskCancelation(id), CacheNotifyAction.Remove);
        }

        public IEnumerable<DistributedTask> GetTasks()
        {
            var tasks = new List<DistributedTask>(cache.HashGetAll<DistributedTask>(key).Values);
            tasks.ForEach(t =>
            {
                if (t.Publication == null)
                {
                    t.Publication = GetPublication();
                }
            });
            return tasks;
        }

        public DistributedTask GetTask(string id)
        {
            var task = cache.HashGet<DistributedTask>(key, id);
            if (task != null && task.Publication == null)
            {
                task.Publication = GetPublication();
            }
            return task;
        }

        public void SetTask(DistributedTask task)
        {
            cache.HashSet(key, task.Id, task);
        }

        public void RemoveTask(string id)
        {
            cache.HashSet(key, id, (DistributedTask)null);
        }


        private void OnCompleted(Task task, string id)
        {
            var distributedTask = GetTask(id);
            if (distributedTask != null)
            {
                distributedTask.Status = DistributedTaskStatus.Completed;
                distributedTask.Exception = task.Exception;
                if (task.IsFaulted)
                {
                    distributedTask.Status = DistributedTaskStatus.Failted;
                }
                if (task.IsCanceled)
                {
                    distributedTask.Status = DistributedTaskStatus.Canceled;
                }
                CancellationTokenSource s;
                cancelations.TryRemove(id, out s);

                distributedTask.PublishChanges();
            }
        }

        private Action<DistributedTask> GetPublication()
        {
            return (t) => SetTask(t);
        }


        [Serializable]
        class DistributedTaskCancelation
        {
            public string Id { get; private set; }

            public DistributedTaskCancelation(string id)
            {
                Id = id;
            }
        }
    }
}
