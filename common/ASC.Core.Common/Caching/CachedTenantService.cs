/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

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
            cache = AscCache.Default;

            CacheExpiration = TimeSpan.FromMinutes(2);
            SettingsExpiration = TimeSpan.FromMinutes(2);
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
            var tenants = cache.Get(KEY) as TenantStore;
            if (tenants != null)
            {
                tenants.Insert(tenant);
            }
            return tenant;
        }

        public void RemoveTenant(int id)
        {
            service.RemoveTenant(id);
            var tenants = cache.Get(KEY) as TenantStore;
            if (tenants != null)
            {
                tenants.Remove(id);
            }
        }

        public IEnumerable<TenantVersion> GetTenantVersions()
        {
            return service.GetTenantVersions();
        }

        public byte[] GetTenantSettings(int tenant, string key)
        {
            var cacheKey = string.Format("settings/{0}/{1}", tenant, key);
            var data = cache.Get(cacheKey) as byte[] ?? service.GetTenantSettings(tenant, key);
            cache.Insert(cacheKey, data ?? new byte[0], DateTime.UtcNow + SettingsExpiration);
            return data == null ? null : data.Length == 0 ? null : data;
        }

        public void SetTenantSettings(int tenant, string key, byte[] data)
        {
            service.SetTenantSettings(tenant, key, data);
            cache.Insert(string.Format("settings/{0}/{1}", tenant, key), data ?? new byte[0], DateTime.UtcNow + SettingsExpiration);
        }


        private TenantStore GetTenantStore()
        {
            var store = cache.Get(KEY) as TenantStore;
            if (store == null)
            {
                cache.Insert(KEY, store = new TenantStore(), DateTime.UtcNow.Add(CacheExpiration));
            }
            return store;
        }


        private class TenantStore
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
