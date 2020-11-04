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


using ASC.Common.Caching;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using System;
using System.Collections.Generic;

namespace ASC.Core.Caching
{
    public class CachedTenantService : ITenantService
    {
        private const string KEY = "tenants";
        private readonly ITenantService service;
        private readonly ICache cache;
        private readonly ICacheNotify cacheNotify;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }

        public TimeSpan SettingsExpiration
        {
            get;
            set;
        }


        public CachedTenantService(ITenantService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            cache = AscCache.Memory;
            CacheExpiration = TimeSpan.FromMinutes(2);
            SettingsExpiration = TimeSpan.FromMinutes(2);
            cacheNotify = AscCache.Notify;
            cacheNotify.Subscribe<Tenant>((t, a) =>
            {
                var tenants = GetTenantStore();
                tenants.Remove(t.TenantId);
                tenants.Clear();
            });
            cacheNotify.Subscribe<TenantSetting>((s, a) =>
            {
                cache.Remove(s.Key);
            });
        }


        public void ValidateDomain(string domain)
        {
            service.ValidateDomain(domain);
        }

        public IEnumerable<Tenant> GetTenants(string login, string passwordHash)
        {
            return service.GetTenants(login, passwordHash);
        }

        public IEnumerable<Tenant> GetTenants(DateTime from, bool active = true)
        {
            return service.GetTenants(from, active);
        }

        public Tenant GetTenant(int id)
        {
            var tenants = GetTenantStore();
            var t = tenants.Get(id);
            if (t == null)
            {
                t = service.GetTenant(id);
                if (t != null)
                {
                    tenants.Insert(t);
                }
            }
            return t;
        }

        public Tenant GetTenant(string domain)
        {
            var tenants = GetTenantStore();
            var t = tenants.Get(domain);
            if (t == null)
            {
                t = service.GetTenant(domain);
                if (t != null)
                {
                    tenants.Insert(t);
                }
            }
            return t;
        }

        public Tenant GetTenantForStandaloneWithoutAlias(string ip)
        {
            var tenants = GetTenantStore();
            var t = tenants.Get(ip);
            if (t == null)
            {
                t = service.GetTenantForStandaloneWithoutAlias(ip);
                if (t != null)
                {
                    tenants.Insert(t, ip);
                }
            }
            return t;
        }

        public Tenant SaveTenant(Tenant tenant)
        {
            tenant = service.SaveTenant(tenant);
            cacheNotify.Publish(new Tenant() { TenantId = tenant.TenantId }, CacheNotifyAction.InsertOrUpdate);
            return tenant;
        }

        public void RemoveTenant(int id, bool auto = false)
        {
            service.RemoveTenant(id, auto);
            cacheNotify.Publish(new Tenant() { TenantId = id }, CacheNotifyAction.InsertOrUpdate);
        }

        public IEnumerable<TenantVersion> GetTenantVersions()
        {
            return service.GetTenantVersions();
        }

        public byte[] GetTenantSettings(int tenant, string key)
        {
            var cacheKey = string.Format("settings/{0}/{1}", tenant, key);
            var data = cache.Get<byte[]>(cacheKey);
            if (data == null)
            {
                data = service.GetTenantSettings(tenant, key);
                cache.Insert(cacheKey, data ?? new byte[0], DateTime.UtcNow + SettingsExpiration);
            }
            return data == null ? null : data.Length == 0 ? null : data;
        }

        public void SetTenantSettings(int tenant, string key, byte[] data)
        {
            service.SetTenantSettings(tenant, key, data);
            var cacheKey = string.Format("settings/{0}/{1}", tenant, key);
            cacheNotify.Publish(new TenantSetting { Key = cacheKey }, CacheNotifyAction.Any);
        }

        private TenantStore GetTenantStore()
        {
            var store = cache.Get<TenantStore>(KEY);
            if (store == null)
            {
                cache.Insert(KEY, store = new TenantStore(), DateTime.UtcNow.Add(CacheExpiration));
            }
            return store;
        }


        [Serializable]
        class TenantSetting
        {
            public string Key { get; set; }
        }


        class TenantStore
        {
            private readonly Dictionary<int, Tenant> byId = new Dictionary<int, Tenant>();
            private readonly Dictionary<string, Tenant> byDomain = new Dictionary<string, Tenant>();
            private readonly object locker = new object();


            public Tenant Get(int id)
            {
                Tenant t;
                lock (locker)
                {
                    byId.TryGetValue(id, out t);
                }
                return t;
            }

            public Tenant Get(string domain)
            {
                if (string.IsNullOrEmpty(domain)) return null;

                Tenant t;
                lock (locker)
                {
                    byDomain.TryGetValue(domain, out t);
                }
                return t;
            }

            public void Insert(Tenant t, string ip = null)
            {
                if (t == null)
                {
                    return;
                }

                Remove(t.TenantId);
                lock (locker)
                {
                    byId[t.TenantId] = t;
                    byDomain[t.TenantAlias] = t;
                    if (!string.IsNullOrEmpty(t.MappedDomain)) byDomain[t.MappedDomain] = t;
                    if (!string.IsNullOrEmpty(ip)) byDomain[ip] = t;
                }
            }

            public void Remove(int id)
            {
                var t = Get(id);
                if (t != null)
                {
                    lock (locker)
                    {
                        byId.Remove(id);
                        byDomain.Remove(t.TenantAlias);
                        if (!string.IsNullOrEmpty(t.MappedDomain))
                        {
                            byDomain.Remove(t.MappedDomain);
                        }
                    }
                }
            }

            internal void Clear()
            {
                if(!CoreContext.Configuration.Standalone) return;
                lock (locker)
                {
                    byId.Clear();
                    byDomain.Clear();
                }
            }
        }
    }
}
