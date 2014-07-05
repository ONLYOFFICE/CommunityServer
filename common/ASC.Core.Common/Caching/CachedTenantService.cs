/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
        private readonly TrustInterval interval;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }

        public TimeSpan DbExpiration
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
            cache = new AspCache();
            interval = new TrustInterval();

            CacheExpiration = TimeSpan.FromHours(2);
            DbExpiration = TimeSpan.FromSeconds(5);
            SettingsExpiration = TimeSpan.FromMinutes(10);
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
            lock (cache)
            {
                var fromdb = false;
                var tenants = GetTenantStore(ref fromdb);
                return (from == default(DateTime) ? tenants : tenants.Where(t => t.LastModified >= from)).ToList();
            }
        }

        public Tenant GetTenant(int id)
        {
            lock (cache)
            {
                var fromdb = false;
                var tenants = GetTenantStore(ref fromdb);
                var t = tenants.Get(id);

                if (!fromdb && t == null)
                {
                    fromdb = true;
                    tenants = GetTenantStore(ref fromdb);
                    t = tenants.Get(id);
                }

                return t;
            }
        }

        public Tenant GetTenant(string domain)
        {
            lock (cache)
            {
                var fromdb = false;
                var tenants = GetTenantStore(ref fromdb);
                var t = tenants.Get(domain);

                if (!fromdb && t == null)
                {
                    fromdb = true;
                    tenants = GetTenantStore(ref fromdb);
                    t = tenants.Get(domain);
                }

                return t;
            }
        }

        public Tenant SaveTenant(Tenant tenant)
        {
            tenant = service.SaveTenant(tenant);
            lock (cache)
            {
                var tenants = cache.Get(KEY) as TenantStore;
                if (tenants != null) tenants.Insert(tenant);
            }
            return tenant;
        }

        public void RemoveTenant(int id)
        {
            service.RemoveTenant(id);
            lock (cache)
            {
                var tenants = cache.Get(KEY) as TenantStore;
                if (tenants != null) tenants.Remove(id);
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
            cache.Insert(cacheKey, data ?? new byte[0], SettingsExpiration);
            return data == null ? null : data.Length == 0 ? null : data;
        }

        public void SetTenantSettings(int tenant, string key, byte[] data)
        {
            service.SetTenantSettings(tenant, key, data);
            cache.Insert(string.Format("settings/{0}/{1}", tenant, key), data ?? new byte[0], SettingsExpiration);
        }


        private TenantStore GetTenantStore(ref bool fromdb)
        {
            var store = cache.Get(KEY) as TenantStore;
            if (store == null || interval.Expired || fromdb)
            {
                fromdb = true;
                var date = store != null ? interval.StartTime.Add(DbExpiration.Negate()) : DateTime.MinValue;
                interval.Start(DbExpiration);

                var tenants = service.GetTenants(date);
                if (store == null) cache.Insert(KEY, store = new TenantStore(), CacheExpiration);

                foreach (var t in tenants)
                {
                    store.Insert(t);
                }
            }
            return store;
        }


        private class TenantStore : IEnumerable<Tenant>
        {
            private readonly Dictionary<int, Tenant> byId = new Dictionary<int, Tenant>();
            private readonly Dictionary<string, Tenant> byDomain = new Dictionary<string, Tenant>();


            public Tenant Get(int id)
            {
                Tenant t;
                byId.TryGetValue(id, out t);
                return t;
            }

            public Tenant Get(string domain)
            {
                if (string.IsNullOrEmpty(domain)) return null;

                Tenant t;
                byDomain.TryGetValue(domain, out t);
                return t;
            }

            public void Insert(Tenant t)
            {
                if (t == null) return;
                Remove(t.TenantId);

                byId[t.TenantId] = t;
                byDomain[t.TenantAlias] = t;
                if (!string.IsNullOrEmpty(t.MappedDomain)) byDomain[t.MappedDomain] = t;
            }

            public void Remove(int id)
            {
                var t = Get(id);
                if (t != null)
                {
                    byId.Remove(id);
                    byDomain.Remove(t.TenantAlias);
                    if (!string.IsNullOrEmpty(t.MappedDomain)) byDomain.Remove(t.MappedDomain);
                }
            }


            public IEnumerator<Tenant> GetEnumerator()
            {
                return byId.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
