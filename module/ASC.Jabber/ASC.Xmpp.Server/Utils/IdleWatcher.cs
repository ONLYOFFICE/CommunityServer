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


using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ASC.Xmpp.Server.Utils
{
    static class IdleWatcher
    {
        private static readonly object syncRoot;

        private static readonly IDictionary<object, IdleItem> items;

        private static readonly TimeSpan timerPeriod;

        private static readonly Timer timer;

        private static bool timerStoped;

        private static readonly ILog log = LogManager.GetLogger(typeof(IdleWatcher));

        private const double TIMER_PERIOD = 1.984363;

        static IdleWatcher()
        {
            syncRoot = new object();
            items = new Dictionary<object, IdleItem>();
            timerPeriod = TimeSpan.FromSeconds(TIMER_PERIOD);
            timer = new Timer(TimerCallback, null, Timeout.Infinite, 0);
            timerStoped = true;
        }

        public static void StartWatch(object idleObject, TimeSpan timeout, EventHandler<TimeoutEventArgs> handler)
        {
            StartWatch(idleObject, timeout, handler, null);
        }

        public static void StartWatch(object idleObject, TimeSpan timeout, EventHandler<TimeoutEventArgs> handler, object data)
        {
            if (idleObject == null) throw new ArgumentNullException("idleObject");
            if (handler == null) throw new ArgumentNullException("handler");
            if (timeout == TimeSpan.Zero) throw new ArgumentOutOfRangeException("timeout");

            lock (syncRoot)
            {
                if (items.ContainsKey(idleObject))
                {
                    log.WarnFormat("An item with the same key ({0}) has already been added.", idleObject);
                }
                items[idleObject] = new IdleItem(idleObject, timeout, handler, data);
                if (timerStoped)
                {
                    timer.Change(timerPeriod, timerPeriod);
                    timerStoped = false;
                    log.DebugFormat("Timer started.");
                }
            }
            log.DebugFormat("Start watch idle object: {0}, timeout: {1}", idleObject, timeout);
        }

        public static void UpdateTimeout(object idleObject)
        {
            UpdateTimeout(idleObject, TimeSpan.Zero);
        }

        public static void UpdateTimeout(object idleObject, TimeSpan timeout)
        {
            if (idleObject == null) throw new ArgumentNullException("idleObject");
            lock (syncRoot)
            {
                if (items.ContainsKey(idleObject)) items[idleObject].UpdateTimeout(timeout);
            }
            log.DebugFormat("Update timeout idle object: {0}, timeout: {1}", idleObject, timeout);
        }

        public static bool StopWatch(object idleObject)
        {
            bool result = false;
            if (idleObject != null)
            {
                lock (syncRoot)
                {
                    try
                    {
                        result = items.Remove(idleObject);
                        StopTimerIfNeeded();
                    }
                    catch (Exception ex)
                    {
                        log.DebugFormat("Error stop watch idle object: {0}, ex: {1}", idleObject, ex);
                    }
                }
            }
            if (result) log.DebugFormat("Stop watch idle object: {0}", idleObject);
            else log.DebugFormat("Stop watch idle object: {0} - idle object not found.", idleObject);

            return result;
        }

        private static void TimerCallback(object state)
        {
            try
            {
                lock (syncRoot)
                {
                    foreach (var item in new Dictionary<object, IdleItem>(items))
                    {
                        if (!item.Value.IsExpired()) continue;

                        log.DebugFormat("Find idle object: {0}, invoke handler.", item.Key);
                        item.Value.InvokeHandler();
                        items.Remove(item.Key);
                    }
                    StopTimerIfNeeded();
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }
        }

        private static void StopTimerIfNeeded()
        {
            lock (syncRoot)
            {
                if (timerStoped || items.Count != 0) return;
            }
            timer.Change(Timeout.Infinite, 0);
            timerStoped = true;
            log.DebugFormat("Timer stopped.");
        }


        private class IdleItem
        {
            private DateTime created;

            private EventHandler<TimeoutEventArgs> handler;

            private TimeSpan timeout;

            private object obj;

            private object data;

            public IdleItem(object obj, TimeSpan timeout, EventHandler<TimeoutEventArgs> handler, object data)
            {
                this.obj = obj;
                this.data = data;
                this.handler = handler;
                UpdateTimeout(timeout);
            }

            public void UpdateTimeout(TimeSpan timeout)
            {
                created = DateTime.UtcNow;
                if (timeout != TimeSpan.Zero) this.timeout = timeout.Duration();
            }

            public bool IsExpired()
            {
                return timeout < (DateTime.UtcNow - created);
            }

            public void InvokeHandler()
            {
                try
                {
                    handler.BeginInvoke(this, new TimeoutEventArgs(obj, data), null, null);
                }
                catch { }
            }
        }
    }

    class TimeoutEventArgs : EventArgs
    {
        public object IdleObject
        {
            get;
            private set;
        }

        public object Data
        {
            get;
            private set;
        }

        public TimeoutEventArgs(object obj, object data)
        {
            IdleObject = obj;
            Data = data;
        }
    }
}