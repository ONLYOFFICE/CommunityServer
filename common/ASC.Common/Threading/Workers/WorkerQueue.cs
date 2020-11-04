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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Common.Logging;


namespace ASC.Common.Threading.Workers
{
    public class WorkerQueue<T>
    {
        private static ILog log = LogManager.GetLogger("ASC.WorkerQueue");

        private readonly ICollection<WorkItem<T>> items = new List<WorkItem<T>>();
        private readonly List<Thread> threads = new List<Thread>();

        private readonly AutoResetEvent waitEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);

        private readonly int workerCount;
        private readonly bool stopAfterFinsih;
        private readonly int errorCount;
        private readonly int waitInterval;

        private Action<T> action;
        private volatile bool started;

        public object SynchRoot { get { return Items; } }

        protected virtual ICollection<WorkItem<T>> Items { get { return items; } }

        public bool IsStarted { get { return started; } }


        public WorkerQueue(int workerCount, TimeSpan waitInterval)
            : this(workerCount, waitInterval, 1, false)
        {
        }

        public WorkerQueue(int workerCount, TimeSpan waitInterval, int errorCount, bool stopAfterFinsih)
        {
            this.workerCount = workerCount;
            this.errorCount = errorCount;
            this.stopAfterFinsih = stopAfterFinsih;
            this.waitInterval = (int)waitInterval.TotalMilliseconds;
        }


        public void Start(Action<T> starter)
        {
            Start(starter, true);
        }

        public IEnumerable<T> GetItems()
        {
            lock (Items)
            {
                return Items.Select(x => x.Item).ToList();
            }
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            lock (Items)
            {
                foreach (var item in items)
                {
                    Items.Add(new WorkItem<T>(item));
                }
            }
            waitEvent.Set();
            ReviveThreads();
        }

        public virtual void Add(T item)
        {
            lock (Items)
            {
                Items.Add(new WorkItem<T>(item));
            }
            waitEvent.Set();
            ReviveThreads();
        }

        public void Remove(T item)
        {
            lock (Items)
            {
                var existing = Items.Where(x => Equals(x.Item, item)).SingleOrDefault();
                RemoveInternal(existing);
            }
        }

        public void Clear()
        {
            lock (Items)
            {
                foreach (var workItem in Items)
                {
                    workItem.Dispose();
                }
                Items.Clear();
            }
        }

        public void Stop()
        {
            if (started)
            {
                started = false;
                
                stopEvent.Set();
                waitEvent.Set();

                log.Debug("Stoping queue. Joining threads");
                foreach (var workerThread in threads)
                {
                    workerThread.Join();
                }
                threads.Clear();
                log.Debug("Queue stoped. Threads cleared");
            }
        }

        public void Terminate()
        {
            if (started)
            {
                started = false;
                
                stopEvent.Set();
                waitEvent.Set();

                log.Debug("Stoping queue. Terminating threads");
                foreach (var worker in threads.Where(t => t != Thread.CurrentThread))
                {
                    worker.Abort();
                }
                if (threads.Contains(Thread.CurrentThread))
                {
                    threads.Clear();
                    log.Debug("Terminate called from current worker thread. Terminating");
                    Thread.CurrentThread.Abort();
                }
                threads.Clear();
                log.Debug("Queue stoped. Threads cleared");
            }
        }


        protected virtual WorkItem<T> Selector()
        {
            return Items.Where(x => !x.IsProcessed).OrderBy(x => x.Added).FirstOrDefault();
        }

        protected virtual void PostComplete(WorkItem<T> item)
        {
            RemoveInternal(item);
        }

        protected void RemoveInternal(WorkItem<T> item)
        {
            if (item != null)
            {
                Items.Remove(item);
                item.Dispose();
            }
        }

        protected virtual void ErrorLimit(WorkItem<T> item)
        {
            RemoveInternal(item);
        }

        protected virtual void Error(WorkItem<T> item, Exception exception)
        {
            LogManager.GetLogger("ASC.Common.Threading.Workers").Error(item, exception);

            item.IsProcessed = false;
            item.Added = DateTime.Now;
        }


        private WaitHandle[] WaitObjects()
        {
            return new WaitHandle[] { stopEvent, waitEvent };
        }

        private void ReviveThreads()
        {
            if (threads.Count != 0)
            {
                var haveLiveThread = threads.Count(x => x.IsAlive) > 0;
                if (!haveLiveThread)
                {
                    Stop();
                    Start(action);
                }
            }
        }

        private void Start(Action<T> starter, bool backgroundThreads)
        {
            if (!started)
            {
                started = true;
                action = starter; 
                
                stopEvent.Reset();
                waitEvent.Reset();

                log.Debug("Creating threads");
                for (var i = 0; i < workerCount; i++)
                {
                    threads.Add(new Thread(DoWork) { IsBackground = backgroundThreads });
                }

                log.Debug("Starting threads");
                foreach (var thread in threads)
                {
                    thread.Start(stopAfterFinsih);
                }
            }
        }

        private void DoWork(object state)
        {
            try
            {
                bool stopAfterFinsih = false;
                if (state != null && state is bool)
                {
                    stopAfterFinsih = (bool)state;
                }
                do
                {
                    WorkItem<T> item;
                    Action<T> localAction;
                    lock (Items)
                    {
                        localAction = action;
                        item = Selector();
                        if (item != null)
                        {
                            item.IsProcessed = true;
                        }
                    }
                    if (localAction == null)
                        break;//Exit if action is null

                    if (item != null)
                    {
                        try
                        {
                            localAction(item.Item);
                            bool fallSleep = false;
                            lock (Items)
                            {
                                PostComplete(item);
                                if (Items.Count == 0)
                                {
                                    fallSleep = true;
                                }
                            }
                            if (fallSleep)
                            {
                                if (stopAfterFinsih || WaitHandle.WaitAny(WaitObjects(), Timeout.Infinite, false) == 0)
                                {
                                    break;
                                }
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            return;
                        }
                        catch (Exception e)
                        {
                            lock (Items)
                            {

                                Error(item, e);
                                item.ErrorCount++;
                                if (item.ErrorCount > errorCount)
                                {
                                    ErrorLimit(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (stopAfterFinsih || WaitHandle.WaitAny(WaitObjects(), waitInterval, false) == 0)
                        {
                            break;
                        }
                    }
                } while (true);
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                log.Error(err);
            }
        }
    }
}