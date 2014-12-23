/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;


#endregion

namespace ASC.Common.Threading.Workers
{
    public class WorkerQueue<T>
    {
        private readonly AutoResetEvent _emptyEvent = new AutoResetEvent(false);
        private readonly int _errorCount;
        private readonly ICollection<WorkItem<T>> _items = new List<WorkItem<T>>();
        private readonly bool _stopAfterFinsih;

        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly AutoResetEvent _waitEvent = new AutoResetEvent(false);
        private readonly int _waitInterval;
        private int _workerCount;
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private Action<T> _action;
        private bool _isThreadSet;
        private ILog _log = LogManager.GetLogger("ASC.WorkerQueue");

        public object SynchRoot { get { return Items; } }

        public WorkerQueue(int workerCount, TimeSpan waitInterval)
            : this(workerCount, waitInterval, 1, false)
        {
        }

        public WorkerQueue(int workerCount, TimeSpan waitInterval, int errorCount, bool stopAfterFinsih)
        {
            WorkerCount = workerCount;
            _errorCount = errorCount;
            _stopAfterFinsih = stopAfterFinsih;
            _waitInterval = (int)waitInterval.TotalMilliseconds;
        }

        private WaitHandle[] WaitObjects()
        {
            return new WaitHandle[] { _stopEvent, _waitEvent };
        }

        protected WaitHandle StopEvent { get { return _stopEvent; } }

        public bool IsStarted { get; set; }


        public int WorkerCount
        {
            get { return _workerCount; }
            set
            {
                if (value != _workerCount)
                {
                    _workerCount = value;

                    //Do a restart
                    if (_isThreadSet && _action != null)
                    {
                        Stop();
                        Start(_action);
                    }
                }
            }
        }

        public virtual ICollection<WorkItem<T>> Items
        {
            get { return _items; }
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
            _waitEvent.Set();
            ReviveThreads();
        }

        public virtual void Add(T item)
        {
            lock (Items)
            {
                Items.Add(new WorkItem<T>(item));
            }
            _waitEvent.Set();
            ReviveThreads();
        }

        private void ReviveThreads()
        {
            if (_workerThreads.Count != 0)
            {
                bool haveLiveThread = _workerThreads.Count(x => x.IsAlive) > 0;
                if (!haveLiveThread)
                {
                    Restart();
                }
            }
        }

        private void Restart()
        {
            Stop();
            Start(_action);
        }

        public void Remove(T item)
        {
            lock (Items)
            {
                WorkItem<T> existing = Items.Where(x => Equals(x.Item, item)).SingleOrDefault();
                RemoveInternal(existing);
            }
        }

        public IEnumerable<T> GetItems()
        {
            lock (Items)
            {
                return Items.Select(x => x.Item).ToList();
            }
        }

        public void Start(Action<T> starter)
        {
            Start(starter, true);
        }

        public void Start(Action<T> starter, bool backgroundThreads)
        {
            lock (Items)
            {
                _action = starter;
            }
            if (!_isThreadSet)
            {
                _log.DebugFormat("Creating threads");
                _isThreadSet = true;
                for (int i = 0; i < WorkerCount; i++)
                {
                    _workerThreads.Add(new Thread(DoWork) { Name = "queue_worker_" + (i + 1), IsBackground = backgroundThreads });
                }
            }
            if (!IsStarted)
            {
                IsStarted = true;
                _stopEvent.Reset();
                _waitEvent.Reset();
                _log.DebugFormat("Starting threads");
                foreach (Thread workerThread in _workerThreads)
                {
                    workerThread.Start(_stopAfterFinsih);
                }
            }
        }

        public void WaitForCompletion()
        {
            _emptyEvent.WaitOne();
        }

        public void Terminate()
        {
            if (IsStarted)
            {
                IsStarted = false;
                _stopEvent.Set();
                _waitEvent.Set();

                _log.DebugFormat("Stoping queue. Terminating threads");
                foreach (Thread workerThread in
                    _workerThreads.Where(workerThread => workerThread != Thread.CurrentThread))
                {
                    workerThread.Abort();
                }
                _isThreadSet = false;
                if (_workerThreads.Contains(Thread.CurrentThread))
                {
                    _workerThreads.Clear();
                    _log.DebugFormat("Terminate called from current worker thread. Terminating");
                    Thread.CurrentThread.Abort();
                }
                _workerThreads.Clear();
                _log.DebugFormat("Queue stoped. Threads cleared");
            }
        }

        public void Stop()
        {
            if (IsStarted)
            {
                IsStarted = false;
                _stopEvent.Set();
                _waitEvent.Set();

                _log.DebugFormat("Stoping queue. Joining threads");
                foreach (Thread workerThread in _workerThreads)
                {
                    workerThread.Join();
                }
                _isThreadSet = false;
                _workerThreads.Clear();
                _log.DebugFormat("Queue stoped. Threads cleared");
            }
        }

        protected virtual WorkItem<T> Selector()
        {
            return Items.Where(x => !x.IsProcessed).OrderBy(x => x.Added).FirstOrDefault();
        }

        protected virtual void PostComplete(WorkItem<T> item)
        {
            item.Completed = DateTime.UtcNow;
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

            item.Error = exception;
            item.IsProcessed = false;
            item.Added = DateTime.Now;
        }

        private void DoWork(object state)
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
                    localAction = _action;
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
                                _emptyEvent.Set();
                                fallSleep = QueueEmpty(true);
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
                    }
                    catch (Exception e)
                    {
                        lock (Items)
                        {

                            Error(item, e);
                            item.ErrorCount++;
                            if (item.ErrorCount > _errorCount)
                            {
                                ErrorLimit(item);
                            }
                        }
                    }
                }
                else
                {
                    if (stopAfterFinsih || WaitHandle.WaitAny(WaitObjects(), GetSleepInterval(), false) == 0)
                    {
                        break;
                    }
                }
            } while (true);
        }

        protected virtual bool QueueEmpty(bool fallAsleep)
        {
            return fallAsleep;
        }

        protected virtual int GetSleepInterval()
        {
            return _waitInterval;
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
    }
}