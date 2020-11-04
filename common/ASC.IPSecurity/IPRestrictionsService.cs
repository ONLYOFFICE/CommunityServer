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

namespace ASC.IPSecurity
{
    public class IPRestrictionsService
    {
        private const string cacheKey = "iprestrictions";
        private static readonly ICache cache = AscCache.Memory;
        private static readonly ICacheNotify notify = AscCache.Notify;
        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(5);


        static IPRestrictionsService()
        {
            notify.Subscribe<IPRestriction>((r, a) => cache.Remove(GetCacheKey(r.TenantId)));
        }


        public static IEnumerable<IPRestriction> Get(int tenant)
        {
            var key = GetCacheKey(tenant);
            var restrictions = cache.Get<List<IPRestriction>>(key);
            if (restrictions == null)
            {
                cache.Insert(key, restrictions = IPRestrictionsRepository.Get(tenant), timeout);
            }
            return restrictions;
        }

        public static IEnumerable<string> Save(IEnumerable<string> ips, int tenant)
        {
            var restrictions = IPRestrictionsRepository.Save(ips, tenant);
            notify.Publish(new IPRestriction { TenantId = tenant }, CacheNotifyAction.InsertOrUpdate);
            return restrictions;
        }

        private static string GetCacheKey(int tenant)
        {
            return cacheKey + tenant;
        }
    }
}