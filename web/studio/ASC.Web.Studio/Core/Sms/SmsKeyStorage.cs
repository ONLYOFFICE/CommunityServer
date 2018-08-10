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
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;

namespace ASC.Web.Studio.Core.SMS
{
    public static class SmsKeyStorage
    {
        public static readonly int KeyLength = 6;
        public static readonly TimeSpan TrustInterval = TimeSpan.FromMinutes(10);
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
                var dic = cache.Get<Dictionary<string, DateTime>>(cacheKey) ?? new Dictionary<string, DateTime>();

                var key = random.Next((int)Math.Pow(10, KeyLength - 1), (int)Math.Pow(10, KeyLength)).ToString();
                dic[key] = DateTime.UtcNow;
                cache.Insert(cacheKey, dic, DateTime.UtcNow.Add(TrustInterval));
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
                return cache.Get<Dictionary<string, DateTime>>(BuildCacheKey(phone)) != null;
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
                var dic = cache.Get<Dictionary<string, DateTime>>(cacheKey);
                if (dic == null)
                    throw new TimeoutException(Resource.SmsAuthenticationTimeout);

                if (!dic.ContainsKey(key))
                    return false;

                var result = dic[key].Add(TrustInterval) > DateTime.UtcNow;
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