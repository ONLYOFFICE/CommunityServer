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


using ASC.Common.Caching;
using ASC.Core.Tenants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<Tenant> GetTenants(DateTime from)
        {
            return service.GetTenants(from);
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

            public void Insert(Tenant t)
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
        }
    }
}
