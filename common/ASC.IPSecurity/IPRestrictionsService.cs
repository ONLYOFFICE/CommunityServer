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
using System.Collections.Concurrent;
using System.Collections.Generic;
using ASC.Core.Caching;
using System.Linq;

namespace ASC.IPSecurity
{
    public class IPRestrictionsService
    {
        private const string cacheKey = "/iprestrictions";
        private static readonly ICache cache;

        private static readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(5);

        private static readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();

        static IPRestrictionsService()
        {
            cache = AscCache.Default;
        }

        public static IEnumerable<IPRestriction> Get(int tenant)
        {
            var key = GetCacheKey(tenant);
            var restrictions = cache.Get(key) as List<IPRestriction>;
            if (restrictions == null)
            {
                var lck = locks.GetOrAdd(key, k => new object());
                lock (lck)
                {
                    restrictions = cache.Get(key) as List<IPRestriction>;
                    if (restrictions == null)
                    {
                        restrictions = IPRestrictionsRepository.Get(tenant).ToList();
                        cache.Insert(key, restrictions, cacheExpiration);
                    }

                    object temp;
                    if (locks.TryGetValue(key, out temp) && (temp == lck))
                    {
                        locks.TryRemove(key, out temp);
                    }
                }
            }
            return restrictions;
        }

        public static IEnumerable<string> Save(IEnumerable<string> ips, int tenant)
        {
            var restrictions = IPRestrictionsRepository.Save(ips, tenant);
            cache.Insert(GetCacheKey(tenant), IPRestrictionsRepository.Get(tenant), cacheExpiration);

            return restrictions;
        }

        private static string GetCacheKey(int tenant)
        {
            return cacheKey + tenant;
        }
    }
}