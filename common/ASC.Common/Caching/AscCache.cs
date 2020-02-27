/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using StackExchange.Redis.Extensions.Core.Extensions;

namespace ASC.Common.Caching
{
    public class AscCache : ICache, ICacheNotify
    {
        public static readonly ICache Default;

        public static readonly ICache Memory;

        public static readonly ICacheNotify Notify;


        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object, CacheNotifyAction>>> actions =
            new ConcurrentDictionary<Type, ConcurrentBag<Action<object, CacheNotifyAction>>>();

        static AscCache()
        {
            Memory = new AscCache();
            Default = ConfigurationManager.GetSection("redisCacheClient") != null ? new RedisCache() : Memory;
            Notify = (ICacheNotify)Default;
            try
            {
                Notify.Subscribe<AscCacheItem>((item, action) => { OnClearCache(); });
            }
            catch (Exception)
            {

            }
        }

        private AscCache()
        {
        }

        public T Get<T>(string key) where T : class
        {
            var cache = GetCache();
            return cache.Get(key) as T;
        }

        public void Insert(string key, object value, TimeSpan sligingExpiration)
        {
            var cache = GetCache();
            cache.Set(key, value, new CacheItemPolicy { SlidingExpiration = sligingExpiration });
        }

        public void Insert(string key, object value, DateTime absolutExpiration)
        {
            var cache = GetCache();
            cache.Set(key, value,
                absolutExpiration == DateTime.MaxValue ? DateTimeOffset.MaxValue : new DateTimeOffset(absolutExpiration));
        }

        public void Remove(string key)
        {
            var cache = GetCache();
            cache.Remove(key);
        }

        public void Remove(Regex pattern)
        {
            var cache = GetCache();

            var copy = cache.ToDictionary(p => p.Key, p => p.Value);

            var keys = copy.Select(p => p.Key).Where(k => pattern.IsMatch(k)).ToArray();
            foreach (var key in keys)
            {
                cache.Remove(key);
            }
        }


        public IDictionary<string, T> HashGetAll<T>(string key)
        {
            var cache = GetCache();
            var dic = (IDictionary<string, T>)cache.Get(key);
            return dic != null ? new Dictionary<string, T>(dic) : new Dictionary<string, T>();
        }

        public T HashGet<T>(string key, string field)
        {
            var cache = GetCache();
            T value;
            var dic = (IDictionary<string, T>)cache.Get(key);
            if (dic != null && dic.TryGetValue(field, out value))
            {
                return value;
            }
            return default(T);
        }

        public void HashSet<T>(string key, string field, T value)
        {
            var cache = GetCache();
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


        public void Subscribe<T>(Action<T, CacheNotifyAction> onchange)
        {
            if (onchange != null)
            {
                Action<object, CacheNotifyAction> action = (o, a) => onchange((T)o, a);
                actions.AddOrUpdate(typeof(T),
                    new ConcurrentBag<Action<object, CacheNotifyAction>> { action },
                    (type, bag) =>
                    {
                        bag.Add(action);
                        return bag;
                    });
            }
            else
            {
                ConcurrentBag<Action<object, CacheNotifyAction>> removed;
                actions.TryRemove(typeof(T), out removed);
            }
        }

        public void Publish<T>(T obj, CacheNotifyAction action)
        {
            ConcurrentBag<Action<object, CacheNotifyAction>> onchange;
            actions.TryGetValue(typeof(T), out onchange);

            if (onchange != null)
            {
                onchange.ToArray().ForEach(r => r(obj, action));
            }
        }

        public static void ClearCache()
        {
            Notify.Publish(new AscCacheItem(), CacheNotifyAction.Any);
        }

        private MemoryCache GetCache()
        {
            return MemoryCache.Default;
        }

        public static void OnClearCache()
        {
            Default.Remove(new Regex(".*"));
            var keys = MemoryCache.Default.Select(r => r.Key).ToList();

            foreach (var k in keys)
            {
                MemoryCache.Default.Remove(k);
            }
        }
    }

    [Serializable]
    public class AscCacheItem
    {
        public Guid Id { get; set; }

        public AscCacheItem()
        {
            Id = Guid.NewGuid();
        }
    }
}