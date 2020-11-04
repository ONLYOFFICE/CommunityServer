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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ASC.Common.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.LegacyConfiguration;

namespace ASC.Common.Caching
{
    public class RedisCache : ICache, ICacheNotify
    {
        private readonly string CacheId = Guid.NewGuid().ToString();
        private readonly StackExchangeRedisCacheClient redis;
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object, CacheNotifyAction>>> actions = new ConcurrentDictionary<Type, ConcurrentBag<Action<object, CacheNotifyAction>>>();


        public RedisCache()
        {
            var configuration = ConfigurationManagerExtension.GetSection("redisCacheClient") as RedisCachingSectionHandler;
            if (configuration == null)
                throw new ConfigurationErrorsException("Unable to locate <redisCacheClient> section into your configuration file. Take a look https://github.com/imperugo/StackExchange.Redis.Extensions");

            var stringBuilder = new StringBuilder();
            using (var stream = new StringWriter(stringBuilder))
            {
                var opts = RedisCachingSectionHandler.GetConfig().ConfigurationOptions;
                opts.SyncTimeout = 60000;
                var connectionMultiplexer = (IConnectionMultiplexer)ConnectionMultiplexer.Connect(opts, stream);
                redis = new StackExchangeRedisCacheClient(connectionMultiplexer, new Serializer());
                LogManager.GetLogger("ASC").Debug(stringBuilder.ToString());
            }
        }


        public T Get<T>(string key) where T : class
        {
            return redis.Get<T>(key);
        }

        public void Insert(string key, object value, TimeSpan sligingExpiration)
        {
            redis.Replace(key, value, sligingExpiration);
        }

        public void Insert(string key, object value, DateTime absolutExpiration)
        {
            redis.Replace(key, value, absolutExpiration == DateTime.MaxValue ? DateTimeOffset.MaxValue : new DateTimeOffset(absolutExpiration));
        }

        public void Remove(string key)
        {
            redis.Remove(key);
        }

        public void Remove(Regex pattern)
        {
            var glob = pattern.ToString().Replace(".*", "*").Replace(".", "?");
            var keys = redis.SearchKeys(glob);
            if (keys.Any())
            {
                redis.RemoveAll(keys);
            }
        }

        public IDictionary<string, T> HashGetAll<T>(string key)
        {
            var dic = redis.Database.HashGetAll(key);
            return dic
                .Select(e =>
                    {
                        var val = default(T);
                        try
                        {
                            val = (string)e.Value != null ? JsonConvert.DeserializeObject<T>(e.Value) : default(T);
                        }
                        catch (Exception ex)
                        {
                            LogManager.GetLogger("ASC").Error("RedisCache HashGetAll", ex);
                        }

                        return new { Key = (string)e.Name, Value = val };
                    })
                .Where(e => e.Value != null && !e.Value.Equals(default(T)))
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public T HashGet<T>(string key, string field)
        {
            var value = (string)redis.Database.HashGet(key, field);
            try
            {
                return value != null ? JsonConvert.DeserializeObject<T>(value) : default(T);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error("RedisCache HashGet", ex);
                return default(T);
            }
        }

        public void HashSet<T>(string key, string field, T value)
        {
            if (value != null)
            {
                redis.Database.HashSet(key, field, JsonConvert.SerializeObject(value));
            }
            else
            {
                redis.Database.HashDelete(key, field);
            }
        }

        public void Publish<T>(T obj, CacheNotifyAction action)
        {
            redis.Publish("asc:channel:" + typeof(T).FullName, new RedisCachePubSubItem<T>() { CacheId = CacheId, Object = obj, Action = action });

            ConcurrentBag<Action<object, CacheNotifyAction>> onchange;
            actions.TryGetValue(typeof(T), out onchange);
            if (onchange != null)
            {
                onchange.ToArray().ForEach(r => r(obj, action));
            }
        }

        public void Subscribe<T>(Action<T, CacheNotifyAction> onchange)
        {
            redis.Subscribe<RedisCachePubSubItem<T>>("asc:channel:" + typeof(T).FullName, (i) =>
            {
                if (i.CacheId != CacheId)
                {
                    onchange(i.Object, i.Action);
                }
            });

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


        [Serializable]
        class RedisCachePubSubItem<T>
        {
            public string CacheId { get; set; }

            public T Object { get; set; }

            public CacheNotifyAction Action { get; set; }
        }

        class Serializer : ISerializer
        {
            private readonly Encoding enc = Encoding.UTF8;


            public byte[] Serialize(object item)
            {
                try
                {
                    var s = JsonConvert.SerializeObject(item);
                    return enc.GetBytes(s);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error("Redis Serialize", e);
                    throw;
                }
            }

            public object Deserialize(byte[] obj)
            {
                try
                {
                    var resolver = new ContractResolver();
                    var settings = new JsonSerializerSettings { ContractResolver = resolver };
                    var s = enc.GetString(obj);
                    return JsonConvert.DeserializeObject(s, typeof(object), settings);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error("Redis Deserialize", e);
                    throw;
                }
            }

            public T Deserialize<T>(byte[] obj)
            {
                try
                {
                    var resolver = new ContractResolver();
                    var settings = new JsonSerializerSettings { ContractResolver = resolver };
                    var s = enc.GetString(obj);
                    return JsonConvert.DeserializeObject<T>(s, settings);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error("Redis Deserialize<T>", e);
                    throw;
                }
            }

            public async Task<byte[]> SerializeAsync(object item)
            {
                return await Task.Factory.StartNew(() => Serialize(item));
            }

            public Task<object> DeserializeAsync(byte[] obj)
            {
                return Task.Factory.StartNew(() => Deserialize(obj));
            }

            public Task<T> DeserializeAsync<T>(byte[] obj)
            {
                return Task.Factory.StartNew(() => Deserialize<T>(obj));
            }


            class ContractResolver : DefaultContractResolver
            {
                protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
                {
                    var prop = base.CreateProperty(member, memberSerialization);
                    if (!prop.Writable)
                    {
                        var property = member as PropertyInfo;
                        if (property != null)
                        {
                            var hasPrivateSetter = property.GetSetMethod(true) != null;
                            prop.Writable = hasPrivateSetter;
                        }
                    }
                    return prop;
                }
            }
        }
    }
}
