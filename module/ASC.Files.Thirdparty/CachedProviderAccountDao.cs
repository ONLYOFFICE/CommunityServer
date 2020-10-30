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
using ASC.Files.Core;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace ASC.Files.Thirdparty
{
    internal class CachedProviderAccountDao : ProviderAccountDao
    {
        private static readonly ConcurrentDictionary<string, IProviderInfo> cache = new ConcurrentDictionary<string, IProviderInfo>();
        private static readonly ICacheNotify cacheNotify;

        private readonly string _rootKey;


        static CachedProviderAccountDao()
        {
            cacheNotify = AscCache.Notify;
            cacheNotify.Subscribe<ProviderAccountCacheItem>((i, a) => RemoveFromCache(i.Key));
        }

        public CachedProviderAccountDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
            _rootKey = tenantID.ToString(CultureInfo.InvariantCulture);
        }

        public override IProviderInfo GetProviderInfo(int linkId)
        {
            var key = _rootKey + linkId.ToString(CultureInfo.InvariantCulture);
            IProviderInfo value;
            if (!cache.TryGetValue(key, out value))
            {
                value = base.GetProviderInfo(linkId);
                cache.TryAdd(key, value);
            }
            return value;
        }

        public override void RemoveProviderInfo(int linkId)
        {
            base.RemoveProviderInfo(linkId);

            var key = _rootKey + linkId.ToString(CultureInfo.InvariantCulture);
            cacheNotify.Publish(new ProviderAccountCacheItem { Key = key }, CacheNotifyAction.Remove);
        }

        public override int UpdateProviderInfo(int linkId, string customerTitle, AuthData authData, FolderType folderType, Guid? userId = null)
        {
            var result = base.UpdateProviderInfo(linkId, customerTitle, authData, folderType, userId);

            var key = _rootKey + linkId.ToString(CultureInfo.InvariantCulture);
            cacheNotify.Publish(new ProviderAccountCacheItem { Key = key }, CacheNotifyAction.Update);
            return result;
        }

        public override int UpdateProviderInfo(int linkId, AuthData authData)
        {
            var result = base.UpdateProviderInfo(linkId, authData);

            var key = _rootKey + linkId.ToString(CultureInfo.InvariantCulture);
            cacheNotify.Publish(new ProviderAccountCacheItem { Key = key }, CacheNotifyAction.Update);
            return result;
        }


        private static void RemoveFromCache(string key)
        {
            IProviderInfo value = null;
            cache.TryRemove(key, out value);
        }


        [Serializable]
        class ProviderAccountCacheItem
        {
            public string Key { get; set; }
        }
    }
}