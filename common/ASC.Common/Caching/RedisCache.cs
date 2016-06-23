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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASC.Common.Caching
{
    public class RedisCache : ICache, ICacheNotify
    {
        private readonly string CacheId = Guid.NewGuid().ToString();
        private readonly StackExchangeRedisCacheClient redis;
        private readonly ConcurrentDictionary<Type, Action<object, CacheNotifyAction>> actions = new ConcurrentDictionary<Type, Action<object, CacheNotifyAction>>();


        public RedisCache()
        {
            redis = new StackExchangeRedisCacheClient(new Serializer());
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
                .Select(e => new { Key = (string)e.Name, Value = ((string)e.Value != null ? JsonConvert.DeserializeObject<T>((string)e.Value) : default(T)) })
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public T HashGet<T>(string key, string field)
        {
            var value = (string)redis.Database.HashGet(key, field);
            return value != null ? JsonConvert.DeserializeObject<T>(value) : default(T);
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

            Action<object, CacheNotifyAction> onchange;
            actions.TryGetValue(typeof(T), out onchange);
            if (onchange != null)
            {
                onchange(obj, action);
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
                actions[typeof(T)] = (o, a) => onchange((T)o, a);
            }
            else
            {
                Action<object, CacheNotifyAction> removed;
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
                var s = JsonConvert.SerializeObject(item);
                return enc.GetBytes(s);
            }

            public object Deserialize(byte[] obj)
            {
                var resolver = new ContractResolver();
                var settings = new JsonSerializerSettings { ContractResolver = resolver };
                var s = enc.GetString(obj);
                return JsonConvert.DeserializeObject(s, typeof(object), settings);
            }

            public T Deserialize<T>(byte[] obj)
            {
                var resolver = new ContractResolver();
                var settings = new JsonSerializerSettings { ContractResolver = resolver };
                var s = enc.GetString(obj);
                return JsonConvert.DeserializeObject<T>(s, settings);
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
