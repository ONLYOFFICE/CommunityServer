/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;


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

                log.DebugFormat("Stoping queue. Joining threads");
                foreach (var workerThread in threads)
                {
                    workerThread.Join();
                }
                threads.Clear();
                log.DebugFormat("Queue stoped. Threads cleared");
            }
        }

        public void Terminate()
        {
            if (started)
            {
                started = false;
                
                stopEvent.Set();
                waitEvent.Set();

                log.DebugFormat("Stoping queue. Terminating threads");
                foreach (var worker in threads.Where(t => t != Thread.CurrentThread))
                {
                    worker.Abort();
                }
                if (threads.Contains(Thread.CurrentThread))
                {
                    threads.Clear();
                    log.DebugFormat("Terminate called from current worker thread. Terminating");
                    Thread.CurrentThread.Abort();
                }
                threads.Clear();
                log.DebugFormat("Queue stoped. Threads cleared");
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

                log.DebugFormat("Creating threads");
                for (var i = 0; i < workerCount; i++)
                {
                    threads.Add(new Thread(DoWork) { IsBackground = backgroundThreads });
                }

                log.DebugFormat("Starting threads");
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