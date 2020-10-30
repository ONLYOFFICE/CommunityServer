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
using System.Text.RegularExpressions;
using ASC.Common.Caching;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Core.Engine
{
    public static class CacheEngine
    {
        private static readonly ICache Cache;
        private static readonly ICacheNotify CacheNotify;
        private static readonly TimeSpan CacheExpiration;
        private static readonly Regex AllReg = new Regex(".*", RegexOptions.Compiled);

        static CacheEngine()
        {
            Cache = AscCache.Memory;

            CacheExpiration = TimeSpan.FromMinutes(20);

            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<AccountCacheItem>((u, a) =>
            {
                if (string.IsNullOrEmpty(u.Key))
                {
                    Cache.Remove(AllReg);
                }
                else
                {
                    Cache.Remove(u.Key);
                }
            });
        }

        public static List<AccountInfo> Get(string username)
        {
            return Cache.Get<List<AccountInfo>>(username);
        }

        public static void Set(string username, List<AccountInfo> accounts)
        {
            Cache.Insert(username, accounts, CacheExpiration);
        }

        public static void Clear(string username)
        {
            CacheNotify.Publish(new AccountCacheItem { Key = username }, CacheNotifyAction.Remove);
        }

        public static void ClearAll()
        {
            CacheNotify.Publish(new AccountCacheItem(), CacheNotifyAction.Remove);
        }

        [Serializable]
        private class AccountCacheItem
        {
            public string Key { get; set; }
        }
    }
}
