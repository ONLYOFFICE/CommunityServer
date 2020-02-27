/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Web.Core.Sms
{
    public static class SmsKeyStorage
    {
        public static readonly int KeyLength;
        public static readonly TimeSpan StoreInterval;
        public static readonly int AttemptCount;
        private static readonly object KeyLocker = new object();
        private static readonly ICache KeyCache = AscCache.Default;
        private static readonly ICache CheckCache = AscCache.Default;

        static SmsKeyStorage()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["sms.keylength"], out KeyLength))
            {
                KeyLength = 6;
            }

            int store;
            if (!int.TryParse(ConfigurationManager.AppSettings["sms.keystore"], out store))
            {
                store = 10;
            }
            StoreInterval = TimeSpan.FromMinutes(store);

            if (!int.TryParse(ConfigurationManager.AppSettings["sms.keycount"], out AttemptCount))
            {
                AttemptCount = 5;
            }
        }

        private static string BuildCacheKey(string phone)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            var tenantCache = tenant == null ? Tenant.DEFAULT_TENANT : tenant.TenantId;
            return "smskey" + phone + tenantCache;
        }


        public static bool GenerateKey(string phone, out string key)
        {
            if (string.IsNullOrEmpty(phone))
            {
                throw new ArgumentNullException("phone");
            }

            lock (KeyLocker)
            {
                var cacheKey = BuildCacheKey(phone);
                var phoneKeys = KeyCache.Get<Dictionary<string, DateTime>>(cacheKey) ?? new Dictionary<string, DateTime>();
                if (phoneKeys.Count > AttemptCount)
                {
                    key = null;
                    return false;
                }

                key = new Random().Next((int)Math.Pow(10, KeyLength - 1), (int)Math.Pow(10, KeyLength)).ToString(CultureInfo.InvariantCulture);
                phoneKeys[key] = DateTime.UtcNow;

                KeyCache.Insert(cacheKey, phoneKeys, DateTime.UtcNow.Add(StoreInterval));
                return true;
            }
        }

        public static bool ExistsKey(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }

            lock (KeyLocker)
            {
                var cacheKey = BuildCacheKey(phone);
                var phoneKeys = KeyCache.Get<Dictionary<string, DateTime>>(cacheKey);
                return phoneKeys != null;
            }
        }


        public static Result ValidateKey(string phone, string key)
        {
            key = (key ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(key))
            {
                return Result.Empty;
            }

            var cacheCheck = BuildCacheKey("check" + phone);
            int counter;
            int.TryParse(CheckCache.Get<String>(cacheCheck), out counter);
            if (++counter > AttemptCount)
                return Result.TooMuch;
            CheckCache.Insert(cacheCheck, counter.ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(StoreInterval));

            lock (KeyLocker)
            {
                var cacheKey = BuildCacheKey(phone);
                var phoneKeys = KeyCache.Get<Dictionary<string, DateTime>>(cacheKey);
                if (phoneKeys == null)
                    return Result.Timeout;

                if (!phoneKeys.ContainsKey(key))
                    return Result.Invalide;

                var createDate = phoneKeys[key];
                KeyCache.Remove(cacheKey);
                if (createDate.Add(StoreInterval) < DateTime.UtcNow)
                    return Result.Timeout;

                CheckCache.Insert(cacheCheck, (--counter).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(StoreInterval));
                return Result.Ok;
            }
        }

        public enum Result
        {
            Ok,
            Invalide,
            Empty,
            TooMuch,
            Timeout,
        }
    }
}