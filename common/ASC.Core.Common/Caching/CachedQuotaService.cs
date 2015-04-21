/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Core.Tenants;

namespace ASC.Core.Caching
{
    public class CachedQuotaService : IQuotaService
    {
        private const string KEY_QUOTA = "quota";
        private const string KEY_QUOTA_ROWS = "quotarows";
        private readonly IQuotaService service;
        private readonly ICache cache;
        private readonly TrustInterval interval;
        private int syncQuotaRows;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }


        public CachedQuotaService(IQuotaService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            this.cache = AscCache.Default;
            this.interval = new TrustInterval();
            this.syncQuotaRows = 0;

            CacheExpiration = TimeSpan.FromMinutes(10);
        }


        private Hashtable GetTenantQuotasInernal()
        {
            var key = "quota";
            var store = cache.Get(key) as Hashtable;
            if (store == null)
            {
                store = Hashtable.Synchronized(new Hashtable());
                foreach (var q in service.GetTenantQuotas())
                {
                    store[q.Id] = q;
                }
                cache.Insert(key, store, DateTime.UtcNow.Add(CacheExpiration));
            }
            return store;
        }

        public IEnumerable<TenantQuota> GetTenantQuotas()
        {
            return GetTenantQuotasInernal().Values.Cast<TenantQuota>();
        }

        public TenantQuota GetTenantQuota(int tenant)
        {
            var store = GetTenantQuotasInernal();
            return (TenantQuota)store[tenant];
        }

        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            quota = service.SaveTenantQuota(quota);
            var store = GetTenantQuotasInernal();
            store[quota.Id] = quota;
            return quota;
        }

        public void RemoveTenantQuota(int tenant)
        {
            service.RemoveTenantQuota(tenant);
            var store = GetTenantQuotasInernal();
            store.Remove(tenant);
        }


        public void SetTenantQuotaRow(TenantQuotaRow row, bool exchange)
        {
            service.SetTenantQuotaRow(row, exchange);
            interval.Expire();
        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(TenantQuotaRowQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");

            if (Interlocked.CompareExchange(ref syncQuotaRows, 1, 0) == 0)
            {
                try
                {
                    var rows = cache.Get(KEY_QUOTA_ROWS) as Dictionary<string, List<TenantQuotaRow>>;
                    if (rows == null || interval.Expired)
                    {
                        var date = rows != null ? interval.StartTime : DateTime.MinValue;
                        interval.Start(CacheExpiration);

                        var changes = service.FindTenantQuotaRows(new TenantQuotaRowQuery(Tenant.DEFAULT_TENANT).WithLastModified(date))
                            .GroupBy(r => r.Tenant.ToString())
                            .ToDictionary(g => g.Key, g => g.ToList());

                        // merge changes from db to cache
                        if (rows == null)
                        {
                            rows = changes;
                        }
                        else
                        {
                            foreach (var p in changes)
                            {
                                if (rows.ContainsKey(p.Key))
                                {
                                    var cachedRows = rows[p.Key];
                                    foreach (var r in p.Value)
                                    {
                                        cachedRows.RemoveAll(c => c.Path == r.Path);
                                        cachedRows.Add(r);
                                    }
                                }
                                else
                                {
                                    rows[p.Key] = p.Value;
                                }
                            }
                        }

                        cache.Insert(KEY_QUOTA_ROWS, rows, DateTime.UtcNow.Add(CacheExpiration));
                    }
                }
                finally
                {
                    syncQuotaRows = 0;
                }
            }
            var quotaRows = cache.Get(KEY_QUOTA_ROWS) as IDictionary<string, List<TenantQuotaRow>>;
            if (quotaRows == null) return new TenantQuotaRow[0];

            lock (quotaRows)
            {
                var list = quotaRows.ContainsKey(query.Tenant.ToString()) ?
                    quotaRows[query.Tenant.ToString()] :
                    new List<TenantQuotaRow>();

                if (query != null && !string.IsNullOrEmpty(query.Path))
                {
                    return list.Where(r => query.Path == r.Path);
                }
                return list.ToList();
            }
        }
    }
}
