/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using ASC.Core;
using ASC.Notify.Config;
using ASC.Notify.Messages;
using log4net;

namespace ASC.Notify
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class NotifyService : INotifyService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NotifyService));

        private readonly DbWorker db = new DbWorker();

        private AutoResetEvent waiter;
        private Thread threadManager;
        private volatile bool work;
        private int threadsCount;


        public void SendNotifyMessage(NotifyMessage notifyMessage)
        {
            try
            {
                db.SaveMessage(notifyMessage);
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }

        public void InvokeSendMethod(string service, string method, int tenant, params object[] parameters)
        {
            var serviceType = Type.GetType(service, true);
            var getinstance = serviceType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);

            var instance = getinstance.GetValue(serviceType, null);
            if (instance == null)
            {
                throw new Exception("Service instance not found.");
            }

            var methodInfo = serviceType.GetMethod(method);
            if (methodInfo == null)
            {
                throw new Exception("Method not found.");
            }

            CoreContext.TenantManager.SetCurrentTenant(tenant);
            methodInfo.Invoke(instance, parameters);
        }


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

            waiter.Set();
            threadManager.Join();
            threadManager = null;

            waiter.Close();
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
