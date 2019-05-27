/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
