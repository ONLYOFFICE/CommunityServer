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
using System.Linq;

using ASC.Common.Caching;
using ASC.Core.Tenants;

namespace ASC.Core.Caching
{
    public class CachedQuotaService : IQuotaService
    {
        private const string KEY_QUOTA = "quota";
        private const string KEY_QUOTA_ROWS = "quotarows";
        private readonly IQuotaService service;
        private readonly ICache cache;
        private readonly ICacheNotify cacheNotify;


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
            CacheExpiration = TimeSpan.FromMinutes(10);

            cacheNotify = AscCache.Notify;
            cacheNotify.Subscribe<QuotaCacheItem>((i, a) =>
            {
                if (i.Key == KEY_QUOTA)
                {
                    cache.Remove(KEY_QUOTA);
                }
                else
                {
                    cache.Remove(i.Key);
                }
            });
        }


        public IEnumerable<TenantQuota> GetTenantQuotas(bool useCache = true)
        {
            var quotas = useCache ? cache.Get<IEnumerable<TenantQuota>>(KEY_QUOTA) : null;
            if (quotas == null)
            {
                cache.Insert(KEY_QUOTA, quotas = service.GetTenantQuotas(useCache), DateTime.UtcNow.Add(CacheExpiration));
            }
            return quotas;
        }
        public TenantQuota GetTenantQuota(int tenant, bool useCache = true)
        {
            return GetTenantQuotas(useCache).SingleOrDefault(q => q.Id == tenant);
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
            cacheNotify.Publish(new QuotaCacheItem { Key = GetKey(row.Tenant) }, CacheNotifyAction.InsertOrUpdate);

            if (row.UserId != Guid.Empty)
            {
                cacheNotify.Publish(new QuotaCacheItem { Key = GetKey(row.Tenant, row.UserId) }, CacheNotifyAction.InsertOrUpdate);
            }

        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(int tenantId)
        {
            var key = GetKey(tenantId);
            var result = cache.Get<IEnumerable<TenantQuotaRow>>(key);

            if (result == null)
            {
                result = service.FindTenantQuotaRows(tenantId);
                cache.Insert(key, result, DateTime.UtcNow.Add(CacheExpiration));
            }

            return result;
        }

        public IEnumerable<TenantQuotaRow> FindUserQuotaRows(int tenantId, Guid userId, bool useCache)
        {
            var key = GetKey(tenantId, userId);
            var result = cache.Get<IEnumerable<TenantQuotaRow>>(key);

            if (result == null || !useCache)
            {
                result = service.FindUserQuotaRows(tenantId, userId, useCache);
                cache.Insert(key, result, DateTime.UtcNow.Add(CacheExpiration));
            }

            return result;
        }

        public TenantQuotaRow FindUserQuotaRow(int tenantId, Guid userId, Guid tag)
        {
            var key = GetKey(tenantId, userId, tag);
            var result = cache.Get<TenantQuotaRow>(key);

            if (result == null)
            {
                result = service.FindUserQuotaRow(tenantId, userId, tag);
                if(result != null) cache.Insert(key, result, DateTime.UtcNow.Add(CacheExpiration));
            }

            return result;
        }

        public string GetKey(int tenant)
        {
            return KEY_QUOTA_ROWS + tenant;
        }

        public string GetKey(int tenant, Guid userId)
        {
            return KEY_QUOTA_ROWS + tenant + userId;
        }
        public string GetKey(int tenant, Guid userId, Guid tag)
        {
            return KEY_QUOTA_ROWS + tenant + userId + tag;
        }


        [Serializable]
        class QuotaCacheItem
        {
            public string Key { get; set; }
        }
    }
}
