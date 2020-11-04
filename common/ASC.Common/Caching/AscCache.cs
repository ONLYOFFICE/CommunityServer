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
            Default = ConfigurationManagerExtension.GetSection("redisCacheClient") != null ? new RedisCache() : Memory;
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