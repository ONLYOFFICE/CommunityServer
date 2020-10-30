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
using System.Threading;

using ASC.Notify.Config;
using ASC.Notify.Messages;
using ASC.Common.Logging;

namespace ASC.Notify
{
    class NotifySender
    {
        private static readonly ILog log = LogManager.GetLogger("ASC");

        private readonly DbWorker db = new DbWorker();

        private AutoResetEvent waiter;
        private Thread threadManager;
        private volatile bool work;
        private int threadsCount;


        public void StartSending()
        {
            db.ResetStates();

            work = true;
            threadsCount = 0;
            waiter = new AutoResetEvent(true);
            threadManager = new Thread(ThreadManagerWork)
            {
                Priority = ThreadPriority.BelowNormal,
                Name = "NotifyThreadManager",
            };
            threadManager.Start();
        }

        public void StopSending()
        {
            work = false;
            if (waiter != null)
            {
                waiter.Set();
            }
            if (threadManager != null)
            {
                threadManager.Join();
                threadManager = null;
            }
            if (waiter != null)
            {
                waiter.Close();
            }
            waiter = null;
        }


        private void ThreadManagerWork()
        {
            while (work)
            {
                try
                {
                    waiter.WaitOne(TimeSpan.FromSeconds(5));
                    if (!work)
                    {
                        return;
                    }

                    var count = Thread.VolatileRead(ref threadsCount);
                    if (count < NotifyServiceCfg.MaxThreads)
                    {
                        var messages = db.GetMessages(NotifyServiceCfg.BufferSize);
                        if (0 < messages.Count)
                        {
                            var t = new Thread(SendMessages)
                            {
                                Priority = ThreadPriority.BelowNormal,
                                Name = "NotifyThread " + DateTime.UtcNow.ToString("o"),
                                IsBackground = true,
                            };
                            t.Start(messages);
                            Interlocked.Increment(ref threadsCount);
                            waiter.Set();

                            log.DebugFormat("{0} started.", t.Name);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
        }

        private void SendMessages(object messages)
        {
            try
            {
                foreach (var m in (IDictionary<int, NotifyMessage>)messages)
                {
                    if (!work) return;

                    var result = MailSendingState.Sended;
                    try
                    {
                        NotifyServiceCfg.Senders[m.Value.Sender].Send(m.Value);
                        log.DebugFormat("Notify #{0} has been sent.", m.Key);
                    }
                     catch (Exception e)
                    {
                        result = MailSendingState.FatalError;
                        log.Error(e);
                    }
                    db.SetState(m.Key, result);
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                Interlocked.Decrement(ref threadsCount);
                try
                {
                    waiter.Set();
                }
                catch (ObjectDisposedException) { }
                log.DebugFormat("{0} stopped.", Thread.CurrentThread.Name);
            }
        }
    }
}
