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
