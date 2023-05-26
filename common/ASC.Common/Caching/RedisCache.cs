/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.LegacyConfiguration;
using StackExchange.Redis.Extensions.Newtonsoft;

using LogManager = ASC.Common.Logging.BaseLogManager;

namespace ASC.Common.Caching
{
    public class RedisCache : ICache, ICacheNotify
    {
        private readonly IRedisDatabase _redis;

        public RedisCache()
        {
            var configuration = ConfigurationManagerExtension.GetSection("redisCacheClient") as RedisCachingSectionHandler;

            if (configuration == null)
                throw new ConfigurationErrorsException("Unable to locate <redisCacheClient> section into your configuration file. Take a look https://github.com/imperugo/StackExchange.Redis.Extensions");

            var redisConfiguration = RedisCachingSectionHandler.GetConfig();

            var connectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);
         
            _redis = new RedisCacheClient(connectionPoolManager, new NewtonsoftSerializer(), redisConfiguration).GetDbFromConfiguration();
        }

        public T Get<T>(string key) where T : class
        {
            return Task.Run(() => _redis.GetAsync<T>(key))
                       .GetAwaiter()
                       .GetResult();
        }

        public void Insert(string key, object value, TimeSpan sligingExpiration)
        {
            Task.Run(() => _redis.ReplaceAsync(key, value, sligingExpiration))
                .GetAwaiter()
                .GetResult();
        }

        public void Insert(string key, object value, DateTime absolutExpiration)
        {
            Task.Run(() => _redis.ReplaceAsync(key, value, absolutExpiration == DateTime.MaxValue ? DateTimeOffset.MaxValue : new DateTimeOffset(absolutExpiration)))
                       .GetAwaiter()
                       .GetResult();
        }

        public void Remove(string key)
        {
            Task.Run(() => _redis.RemoveAsync(key))
                       .GetAwaiter()
                       .GetResult();
        }

        public void Remove(Regex pattern)
        {
            var glob = pattern.ToString().Replace(".*", "*").Replace(".", "?");
            var keys = Task.Run(() => _redis.SearchKeysAsync(glob))
                           .GetAwaiter()
                           .GetResult();

            if (keys.Any())
            {
                Task.Run(() => _redis.RemoveAllAsync(keys))
                    .GetAwaiter()
                    .GetResult();
            }

        }

        public IDictionary<string, T> HashGetAll<T>(string key)
        {
            var dic = _redis.Database.HashGetAll(key);

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
                            if (!e.Value.IsNullOrEmpty)
                            {
                                LogManager.GetLogger("ASC").ErrorFormat("RedisCache HashGetAll value: {0}", e.Value);
                            }

                            LogManager.GetLogger("ASC").Error(string.Format("RedisCache HashGetAll key: {0}", key), ex);
                        }

                        return new { Key = (string)e.Name, Value = val };
                    })
                .Where(e => e.Value != null && !e.Value.Equals(default(T)))
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public T HashGet<T>(string key, string field)
        {
            var value = (string)(_redis.Database.HashGet(key, field));

            try
            {
                return value != null ? JsonConvert.DeserializeObject<T>(value) : default(T);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error(string.Format("RedisCache HashGet key: {0}, field: {1}", key, field), ex);

                return default(T);
            }
        }

        public void HashSet<T>(string key, string field, T value)
        {
            if (value != null)
            {
                _redis.Database.HashSet(key, field, JsonConvert.SerializeObject(value));
            }
            else
            {
                _redis.Database.HashDelete(key, field);
            }
        }

        public void Publish<T>(T obj, CacheNotifyAction cacheNotifyAction)
        {
            var channelName = $"asc:channel:{cacheNotifyAction}:{typeof(T).FullName}".ToLowerInvariant();

            Task.Run(() => _redis.PublishAsync(channelName, new RedisCachePubSubItem<T>() { Object = obj, Action = cacheNotifyAction }))
                .GetAwaiter()
                .GetResult();
        }

        public void Subscribe<T>(Action<T, CacheNotifyAction> onchange)
        {
            foreach (var cacheNotifyAction in Enum.GetNames(typeof(CacheNotifyAction)))
            {
                var channelName = $"asc:channel:{cacheNotifyAction}:{typeof(T).FullName}".ToLowerInvariant();

                Task.Run(() => _redis.SubscribeAsync<RedisCachePubSubItem<T>>(channelName, (i) =>
                {
                    onchange(i.Object, i.Action);

                    return Task.FromResult(true);
                })).GetAwaiter()
                   .GetResult();
            }
        }

        public void PushMailAction<T>(string QueueName, T value) where T : class
        {
            if (value != null)
            {
                Task.Run(() => _redis.ListAddToLeftAsync<T>(QueueName, value))
                    .GetAwaiter()
                    .GetResult();
            }
        }

        class RedisCachePubSubItem<T>
        {
            public T Object { get; set; }

            public CacheNotifyAction Action { get; set; }
        }
    }
}
