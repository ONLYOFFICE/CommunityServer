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

using ASC.Core.Caching;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;

namespace ASC.Web.Studio.Core.SMS
{
    public static class SmsKeyStorage
    {
        public static readonly int KeyLength = 6;
        private static readonly TimeSpan trustInterval = TimeSpan.FromMinutes(10);
        private static readonly object locker = new object();
        private static readonly ICache cache = AscCache.Default;
        private static readonly Random random = new Random();


        public static string GenerateKey(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                throw new ArgumentNullException("phone");
            }

            lock (locker)
            {
                var cacheKey = BuildCacheKey(phone);
                var dic = cache.Get(cacheKey) as IDictionary<string, DateTime> ?? new Dictionary<string, DateTime>();

                var key = random.Next((int)Math.Pow(10, KeyLength - 1), (int)Math.Pow(10, KeyLength)).ToString();
                dic[key] = DateTime.UtcNow;
                cache.Insert(cacheKey, dic, DateTime.UtcNow.Add(trustInterval));
                return key;
            }
        }

        public static bool ExistsKey(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }

            lock (locker)
            {
                return cache.Get(BuildCacheKey(phone)) != null;
            }
        }

        public static bool ValidateKey(string phone, string key)
        {
            key = (key ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(key))
            {
                return false;
            }

            lock (locker)
            {
                var cacheKey = BuildCacheKey(phone);
                var dic = cache.Get(cacheKey) as IDictionary<string, DateTime>;
                if (dic == null)
                    throw new TimeoutException(Resource.SmsAuthenticationTimeout);

                if (!dic.ContainsKey(key))
                    return false;

                var result = dic[key].Add(trustInterval) > DateTime.UtcNow;
                cache.Remove(cacheKey);
                return result;
            }
        }

        private static string BuildCacheKey(string phone)
        {
            return phone + TenantProvider.CurrentTenantID;
        }
    }
}