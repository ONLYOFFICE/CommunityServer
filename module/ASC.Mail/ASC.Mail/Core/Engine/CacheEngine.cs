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
