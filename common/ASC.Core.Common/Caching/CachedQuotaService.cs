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
using System.Collections.Generic;
using System.Linq;

namespace ASC.Core.Caching
{
    public class CachedQuotaService : IQuotaService
    {
        private const string KEY_QUOTA = "quota";
        private const string KEY_QUOTA_ROWS = "quotarows";
        private readonly IQuotaService service;
        private readonly ICache cache;
        private readonly ICacheNotify cacheNotify;
        private readonly TrustInterval interval;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }


        public CachedQuotaService(IQuotaService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            cache = AscCache.Memory;
            interval = new TrustInterval();
            CacheExpiration = TimeSpan.FromMinutes(10);

            cacheNotify = AscCache.Notify;
            cacheNotify.Subscribe<QuotaCacheItem>((i, a) =>
            {
                if (i.Key == KEY_QUOTA_ROWS)
                {
                    interval.Expire();
                }
                else if (i.Key == KEY_QUOTA)
                {
                    cache.Remove(KEY_QUOTA);
                }
            });
        }


        public IEnumerable<TenantQuota> GetTenantQuotas()
        {
            var quotas = cache.Get<IEnumerable<TenantQuota>>(KEY_QUOTA);
            if (quotas == null)
            {
                cache.Insert(KEY_QUOTA, quotas = service.GetTenantQuotas(), DateTime.UtcNow.Add(CacheExpiration));
            }
            return quotas;
        }

        public TenantQuota GetTenantQuota(int tenant)
        {
            return GetTenantQuotas().SingleOrDefault(q => q.Id == tenant);
        }

        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            var q = service.SaveTenantQuota(quota);
            cacheNotify.Publish(new QuotaCacheItem { Key = KEY_QUOTA }, CacheNotifyAction.Remove);
            return q;
        }

        public void RemoveTenantQuota(int tenant)
        {
            throw new NotImplementedException();
        }


        public void SetTenantQuotaRow(TenantQuotaRow row, bool exchange)
        {
            service.SetTenantQuotaRow(row, exchange);
            cacheNotify.Publish(new QuotaCacheItem { Key = KEY_QUOTA_ROWS }, CacheNotifyAction.InsertOrUpdate);
        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(TenantQuotaRowQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");

            var rows = cache.Get<Dictionary<string, List<TenantQuotaRow>>>(KEY_QUOTA_ROWS);
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
                    lock (rows)
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
                }

                cache.Insert(KEY_QUOTA_ROWS, rows, DateTime.UtcNow.Add(CacheExpiration));
            }

            var quotaRows = cache.Get<Dictionary<string, List<TenantQuotaRow>>>(KEY_QUOTA_ROWS);
            if (quotaRows == null)
            {
                return Enumerable.Empty<TenantQuotaRow>();
            }

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


        [Serializable]
        class QuotaCacheItem
        {
            public string Key { get; set; }
        }
    }
}
