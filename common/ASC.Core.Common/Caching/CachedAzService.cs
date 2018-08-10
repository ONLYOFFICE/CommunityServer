/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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