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


using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ASC.Xmpp.Server.Utils
{
    static class IdleWatcher
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(IdleWatcher));

        private static readonly object locker = new object();
        private static readonly TimeSpan timerPeriod = TimeSpan.FromSeconds(1.984363);
        private static readonly Timer timer;
        private static int timerInWork;

        private static readonly IDictionary<string, IdleItem> items = new Dictionary<string, IdleItem>();


        static IdleWatcher()
        {
            timer = new Timer(OnTimer, null, timerPeriod, timerPeriod);
        }

        public static void StartWatch(string id, TimeSpan timeout, EventHandler<TimeoutEventArgs> handler)
        {
            StartWatch(id, timeout, handler, null);
        }

        public static void StartWatch(string id, TimeSpan timeout, EventHandler<TimeoutEventArgs> handler, object data)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (timeout == TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }

            lock (locker)
            {
                if (items.ContainsKey(id))
                {
                    log.WarnFormat("An item with the same key ({0}) has already been added.", id);
                }
                items[id] = new IdleItem(id, timeout, handler, data);
            }
            log.DebugFormat("Start watch idle object: {0}, timeout: {1}", id, timeout);
        }

        public static void UpdateTimeout(string id, TimeSpan timeout)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            lock (locker)
            {
                IdleItem item;
                if (items.TryGetValue(id, out item))
                {
                    item.UpdateItemTimeout(timeout);
                }
            }
            log.DebugFormat("Update timeout idle object: {0}, timeout: {1}", id, timeout);
        }

        public static bool StopWatch(string id)
        {
            var result = false;

            if (id == null)
            {
                return result;
            }

            lock (locker)
            {
                result = items.Remove(id);
            }
            log.DebugFormat("Stop watch idle object: {0}" + (result ? "" : " - idle object not found."), id);

            return result;
        }

        private static void OnTimer(object _)
        {
            if (Interlocked.CompareExchange(ref timerInWork, 1, 0) == 0)
            {
                try
                {
                    var expiredItems = new List<IdleItem>();
                    lock (locker)
                    {
                        foreach (var item in new Dictionary<string, IdleItem>(items))
                        {
                            if (item.Value.IsExpired())
                            {
                                items.Remove(item.Key);
                                expiredItems.Add(item.Value);
                            }
                        }
                    }

                    foreach (var item in expiredItems)
                    {
                        try
                        {
                            log.DebugFormat("Find idle object: {0}, invoke handler.", item.Id);
                            item.InvokeHandler();
                        }
                        catch (Exception err)
                        {
                            log.ErrorFormat("Error in handler idle object: {0}", err);
                        }
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
                finally
                {
                    timerInWork = 0;
                }
            }
            else
            {
                log.WarnFormat("Idle timer works more than {0}s.", Math.Round(timerPeriod.TotalSeconds, 2));
            }
        }


        private class IdleItem
        {
            private readonly EventHandler<TimeoutEventArgs> handler;
            private readonly object data;
            private DateTime created;
            private TimeSpan timeout;

            public string Id { get; private set; }


            public IdleItem(string id, TimeSpan timeout, EventHandler<TimeoutEventArgs> handler, object data)
            {
                Id = id;
                this.data = data;
                this.handler = handler;
                UpdateItemTimeout(timeout);
            }

            public void UpdateItemTimeout(TimeSpan timeout)
            {
                created = DateTime.UtcNow;
                if (timeout != TimeSpan.Zero)
                {
                    this.timeout = timeout.Duration();
                }
            }

            public bool IsExpired()
            {
                return timeout < (DateTime.UtcNow - created);
            }

            public void InvokeHandler()
            {
                handler(this, new TimeoutEventArgs(Id, data));
            }
        }
    }

    class TimeoutEventArgs : EventArgs
    {
        public string Id
        {
            get;
            private set;
        }

        public object Data
        {
            get;
            private set;
        }

        public TimeoutEventArgs(string id, object data)
        {
            Id = id;
            Data = data;
        }
    }
}