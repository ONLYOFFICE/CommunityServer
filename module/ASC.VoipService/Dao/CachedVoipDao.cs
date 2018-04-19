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