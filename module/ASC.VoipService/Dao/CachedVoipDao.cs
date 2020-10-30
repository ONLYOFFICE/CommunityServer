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
using System.Globalization;
using System.Linq;

namespace ASC.VoipService.Dao
{
    public class CachedVoipDao : VoipDao
    {
        private static readonly ICache cache = AscCache.Memory;
        private static readonly ICacheNotify notify = AscCache.Notify;
        private static readonly TimeSpan timeout = TimeSpan.FromDays(1);


        static CachedVoipDao()
        {
            try
            {
                notify.Subscribe<CachedVoipItem>((c, a) => ResetCache(c.Tenant));
            }
            catch (Exception)
            {
            }
        }


        public CachedVoipDao(int tenantID)
            : base(tenantID)
        {
        }

        public override VoipPhone SaveOrUpdateNumber(VoipPhone phone)
        {
            var result = base.SaveOrUpdateNumber(phone);
            notify.Publish(new CachedVoipItem { Tenant = TenantID }, CacheNotifyAction.InsertOrUpdate);
            return result;
        }

        public override void DeleteNumber(string phoneId = "")
        {
            base.DeleteNumber(phoneId);
            notify.Publish(new CachedVoipItem { Tenant = TenantID }, CacheNotifyAction.Remove);
        }

        public override IEnumerable<VoipPhone> GetNumbers(params object[] ids)
        {
            var numbers = cache.Get<List<VoipPhone>>(GetCacheKey(TenantID));
            if (numbers == null)
            {
                numbers = new List<VoipPhone>(base.GetNumbers());
                cache.Insert(GetCacheKey(TenantID), numbers, DateTime.UtcNow.Add(timeout));
            }

            return ids.Any() ? numbers.Where(r => ids.Contains(r.Id) || ids.Contains(r.Number)).ToList() : numbers;
        }


        private static void ResetCache(int tenant)
        {
            cache.Remove(GetCacheKey(tenant));
        }

        private static string GetCacheKey(int tenant)
        {
            return "voip" + tenant.ToString(CultureInfo.InvariantCulture);
        }

        [Serializable]
        class CachedVoipItem
        {
            public int Tenant { get; set; }
        }
    }
}