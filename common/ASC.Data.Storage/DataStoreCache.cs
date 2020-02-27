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


using ASC.Common.Caching;
using System;

namespace ASC.Data.Storage
{
    static class DataStoreCache
    {
        private static readonly ICache Cache = AscCache.Memory;
        
 
        public static void Put(IDataStore store, string tenantId, string module)
        {
            Cache.Insert(DataStoreCacheItem.Create(tenantId, module).MakeCacheKey(), store, DateTime.MaxValue);
        }

        public static IDataStore Get(string tenantId, string module)
        {
            return Cache.Get<IDataStore>(DataStoreCacheItem.Create(tenantId, module).MakeCacheKey());
        }

        public static void Remove(string tenantId, string module)
        {
            Cache.Remove(DataStoreCacheItem.Create(tenantId, module).MakeCacheKey());
        }
        

    }

    public class DataStoreCacheItem
    {
        public string TenantId { get; set; }
        public string Module { get; set; }

        public static DataStoreCacheItem Create(string tenantId, string module)
        {
            return new DataStoreCacheItem
            {
                TenantId = tenantId,
                Module = module
            };
        }

        internal string MakeCacheKey()
        {
            return string.Format("{0}:\\{1}", TenantId, Module);
        }
    }
}