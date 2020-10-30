/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
