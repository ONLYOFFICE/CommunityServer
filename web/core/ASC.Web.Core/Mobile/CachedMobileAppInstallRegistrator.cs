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
using ASC.Core.Common.Notify.Push;
using System;

namespace ASC.Web.Core.Mobile
{
    public class CachedMobileAppInstallRegistrator : IMobileAppInstallRegistrator
    {
        private readonly ICache cache = AscCache.Memory;
        private readonly TimeSpan cacheExpiration;
        private readonly IMobileAppInstallRegistrator registrator;

        public CachedMobileAppInstallRegistrator(IMobileAppInstallRegistrator registrator)
            : this(registrator, TimeSpan.FromMinutes(30))
        {
        }

        public CachedMobileAppInstallRegistrator(IMobileAppInstallRegistrator registrator, TimeSpan cacheExpiration)
        {
            if (registrator == null)
            {
                throw new ArgumentNullException("registrator");
            }
            this.registrator = registrator;
            this.cacheExpiration = cacheExpiration;
        }

        public void RegisterInstall(string userEmail, MobileAppType appType)
        {
            if (string.IsNullOrEmpty(userEmail)) return;
            registrator.RegisterInstall(userEmail, appType);
            cache.Insert(GetCacheKey(userEmail, null), true, cacheExpiration);
            cache.Insert(GetCacheKey(userEmail, appType), true, cacheExpiration);
        }

        public bool IsInstallRegistered(string userEmail, MobileAppType? appType)
        {
            if (string.IsNullOrEmpty(userEmail)) return false;

            var fromCache = cache.Get<String>(GetCacheKey(userEmail, appType));

            bool cachedValue;

            if (bool.TryParse(fromCache, out cachedValue))
            {
                return cachedValue;
            }

            var isRegistered = registrator.IsInstallRegistered(userEmail, appType);
            cache.Insert(GetCacheKey(userEmail, appType), isRegistered.ToString(), cacheExpiration);
            return isRegistered;
        }

        private static string GetCacheKey(string userEmail, MobileAppType? appType)
        {
            var cacheKey = appType.HasValue ? userEmail + "/" + appType.ToString() : userEmail;
            
            return String.Format("{0}:mobile:{1}", ASC.Core.CoreContext.TenantManager.GetCurrentTenant().TenantId, cacheKey);
        }
    }
}
