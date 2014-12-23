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
