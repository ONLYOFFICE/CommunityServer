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
using System;
using System.Collections.Generic;

namespace ASC.Core.Caching
{
    public class CachedAzService : IAzService
    {
        private readonly IAzService service;
        private readonly ICache cache;
        private readonly ICacheNotify cacheNotify;


        public TimeSpan CacheExpiration { get; set; }


        public CachedAzService(IAzService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            cache = AscCache.Memory;
            CacheExpiration = TimeSpan.FromMinutes(10);

            cacheNotify = AscCache.Notify;
            cacheNotify.Subscribe<AzRecord>((r, a) => UpdateCache(r.Tenant, r, a == CacheNotifyAction.Remove));
        }


        public IEnumerable<AzRecord> GetAces(int tenant, DateTime from)
        {
            var key = GetKey(tenant);
            var aces = cache.Get<AzRecordStore>(key);
            if (aces == null)
            {
                var records = service.GetAces(tenant, default(DateTime));
                cache.Insert(key, aces = new AzRecordStore(records), DateTime.UtcNow.Add(CacheExpiration));
            }
            return aces;
        }

        public AzRecord SaveAce(int tenant, AzRecord r)
        {
            r = service.SaveAce(tenant, r);
            cacheNotify.Publish(r, CacheNotifyAction.InsertOrUpdate);
            return r;
        }

        public void RemoveAce(int tenant, AzRecord r)
        {
            service.RemoveAce(tenant, r);
            cacheNotify.Publish(r, CacheNotifyAction.Remove);
        }


        private string GetKey(int tenant)
        {
            return "acl" + tenant.ToString();
        }

        private void UpdateCache(int tenant, AzRecord r, bool remove)
        {
            var aces = cache.Get<AzRecordStore>(GetKey(r.Tenant));
            if (aces != null)
            {
                lock (aces)
                {
                    if (remove)
                    {
                        aces.Remove(r);
                    }
                    else
                    {
                        aces.Add(r);
                    }
                }
            }
        }
    }
}