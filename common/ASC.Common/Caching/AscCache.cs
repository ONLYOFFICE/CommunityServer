/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace ASC.Common.Caching
{
    /*
     * add locks MemoryCache to fix bug: https://bugzilla.xamarin.com/show_bug.cgi?id=25522
     * see also https://github.com/alexanderkyte/mono/commit/311e03221901d24435aa1560dac0b046b9dfe4fc
     * remove locks when mono bug will be fixed
     */
    public class AscCache : ICache, ICacheNotify
    {
        private static readonly object locker = new object();


        public static readonly ICache Default;

        public static readonly ICache Memory;

        public static readonly ICacheNotify Notify;


        private readonly ConcurrentDictionary<Type, Action<object, CacheNotifyAction>> actions = new ConcurrentDictionary<Type, Action<object, CacheNotifyAction>>();


        static AscCache()
        {
            Memory = new AscCache();
            Default = RedisCachingSectionHandlerExtension.IsEnabled() ? (ICache)new RedisCache() : Memory;
            Notify = (ICacheNotify)Default;
        }

        private AscCache()
        {

        }


        public T Get<T>(string key) where T : class
        {
            var cache = GetCache();
            lock (locker)
            {
                return cache.Get(key) as T;
            }
        }

        public void Insert(string key, object value, TimeSpan sligingExpiration)
        {
            var cache = GetCache();
            lock (locker)
            {
                cache.Set(key, value, new CacheItemPolicy { SlidingExpiration = sligingExpiration });
            }
        }

        public void Insert(string key, object value, DateTime absolutExpiration)
        {
            var cache = GetCache();
            lock (locker)
            {
                cache.Set(key, value, absolutExpiration == DateTime.MaxValue ? DateTimeOffset.MaxValue : new DateTimeOffset(absolutExpiration));
            }
        }

        public void Remove(string key)
        {
            var cache = GetCache();
            lock (locker)
            {
                cache.Remove(key);
            }
        }

        public void Remove(Regex pattern)
        {
            var cache = GetCache();
            Dictionary<string, object> copy;
            lock (locker)
            {
                copy = cache.ToDictionary(p => p.Key, p => p.Value);
            }

            var keys = copy.Select(p => p.Key).Where(k => pattern.IsMatch(k)).ToArray();
            lock (locker)
            {
                foreach (var key in keys)
                {
                    cache.Remove(key);
                }
            }
        }


        public IDictionary<string, T> HashGetAll<T>(string key)
        {
            var cache = GetCache();
            lock (locker)
            {
                var dic = (IDictionary<string, T>)cache.Get(key);
                return dic != null ? new Dictionary<string, T>(dic) : new Dictionary<string, T>();
            }
        }

        public T HashGet<T>(string key, string field)
        {
            var cache = GetCache();
            lock (locker)
            {
                T value;
                var dic = (IDictionary<string, T>)cache.Get(key);
                if (dic != null && dic.TryGetValue(field, out value))
                {
                    return value;
                }
                return default(T);
            }
        }

        public void HashSet<T>(string key, string field, T value)
        {
            var cache = GetCache();
            lock (locker)
            {
                var dic = (IDictionary<string, T>)cache.Get(key);
                if (value != null)
                {
                    if (dic == null)
                    {
                        dic = new Dictionary<string, T>();
                    }
                    dic[field] = value;
                    cache.Set(key, dic, null);
                }
                else if (dic != null)
                {
                    dic.Remove(field);
                    if (dic.Count == 0)
                    {
                        cache.Remove(key);
                    }
                    else
                    {
                        cache.Set(key, dic, null);
                    }
                }
            }
        }


        public void Subscribe<T>(Action<T, CacheNotifyAction> onchange)
        {
            if (onchange != null)
            {
                actions[typeof(T)] = (o, a) => onchange((T)o, a);
            }
            else
            {
                Action<object, CacheNotifyAction> removed;
                actions.TryRemove(typeof(T), out removed);
            }
        }

        public void Publish<T>(T obj, CacheNotifyAction action)
        {
            Action<object, CacheNotifyAction> onchange;
            actions.TryGetValue(typeof(T), out onchange);
            if (onchange != null)
            {
                onchange(obj, action);
            }
        }


        private MemoryCache GetCache()
        {
            return MemoryCache.Default;
        }
    }
}
