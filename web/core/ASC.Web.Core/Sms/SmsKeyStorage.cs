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
            if (!int.TryParse(ConfigurationManagerExtension.AppSettings["sms.keylength"], out KeyLength))
            {
                KeyLength = 6;
            }

            int store;
            if (!int.TryParse(ConfigurationManagerExtension.AppSettings["sms.keystore"], out store))
            {
                store = 10;
            }
            StoreInterval = TimeSpan.FromMinutes(store);

            if (!int.TryParse(ConfigurationManagerExtension.AppSettings["sms.keycount"], out AttemptCount))
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