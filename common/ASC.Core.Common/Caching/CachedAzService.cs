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
using System.Collections.Generic;
using ASC.Core.Common.Caching;

namespace ASC.Core.Caching
{
    public class CachedAzService : IAzService
    {
        private readonly IAzService service;
        private readonly ICache cache;

        public TimeSpan CacheExpiration { get; set; }

        public CachedAzService(IAzService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            this.cache = AscCache.Default;

            CacheExpiration = TimeSpan.FromMinutes(10);
        }

        public IEnumerable<AzRecord> GetAces(int tenant, DateTime from)
        {
            var key = GetKey(tenant);
            var aces = cache.Get(key) as AzRecordStore;
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
            var aces = cache.Get(GetKey(tenant)) as AzRecordStore;
            if (aces != null)
            {
                lock (aces)
                {
                    aces.Add(r);
                }
            }
            return r;
        }

        public void RemoveAce(int tenant, AzRecord r)
        {
            service.RemoveAce(tenant, r);
            var aces = cache.Get(GetKey(tenant)) as AzRecordStore;
            if (aces != null)
            {
                lock (aces)
                {
                    aces.Remove(r);
                }
            }
        }


        private string GetKey(int tenant)
        {
            return "acl" + tenant.ToString();
        }
    }
}