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