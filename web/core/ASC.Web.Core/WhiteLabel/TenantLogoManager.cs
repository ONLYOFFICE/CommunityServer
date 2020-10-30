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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

using ASC.Common.Caching;
using ASC.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.WhiteLabel
{
    public class TenantLogoManager
    {
        private static readonly ICache Cache = AscCache.Default;

        private static string CacheKey
        {
            get { return "letterlogodata" + TenantProvider.CurrentTenantID; }
        }

        public static bool WhiteLabelEnabled
        {
            get;
            private set;
        }


        static TenantLogoManager()
        {
            var hideSettings = (ConfigurationManagerExtension.AppSettings["web.hide-settings"] ?? "").Split(new[] { ',', ';', ' ' });
            WhiteLabelEnabled = !hideSettings.Contains("WhiteLabel", StringComparer.CurrentCultureIgnoreCase);
        }


        public static string GetFavicon(bool general, bool timeParam)
        {
            string faviconPath;
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                faviconPath = tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, general);
                if (timeParam)
                {
                    var now = DateTime.Now;
                    faviconPath = String.Format("{0}?t={1}", faviconPath, now.Ticks);
                }
            }
            else
            {
                faviconPath = TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Favicon, general);
            }

            return faviconPath;
        }

        public static string GetTopLogo(bool general)//LogoLightSmall
        {
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();

                return tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.LightSmall, general);
            }
            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.LightSmall, general);
        }

        public static string GetLogoDark(bool general)
        {
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                return tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
            }

            /*** simple scheme ***/
            var tenantInfoSettings = TenantInfoSettings.Load();
            return tenantInfoSettings.GetAbsoluteCompanyLogoPath();
            /***/
        }

        public static string GetLogoDocsEditor(bool general)
        {
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                return tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, general);
            }
            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, general);
        }

        public static string GetLogoText()
        {
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                return tenantWhiteLabelSettings.LogoText ?? TenantWhiteLabelSettings.DefaultLogoText;
            }
            return TenantWhiteLabelSettings.DefaultLogoText;
        }

        public static bool IsRetina(HttpRequest request)
        {
            if (request != null)
            {
                var cookie = request.Cookies["is_retina"];
                if (cookie != null && !String.IsNullOrEmpty(cookie.Value))
                {
                    bool result;
                    if (Boolean.TryParse(cookie.Value, out result))
                    {
                        return result;
                    }
                }
            }

            return !SecurityContext.IsAuthenticated;
        }

        public static bool WhiteLabelPaid
        {
            get
            {
                return CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).WhiteLabel;
            }
        }

        /// <summary>
        /// Get logo stream or null in case of default logo
        /// </summary>
        public static Stream GetWhitelabelMailLogo()
        {
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                return tenantWhiteLabelSettings.GetWhitelabelLogoData(WhiteLabelLogoTypeEnum.Dark, true);
            }

            /*** simple scheme ***/
            var tenantInfoSettings = TenantInfoSettings.Load();
            return tenantInfoSettings.GetStorageLogoData();
            /***/
        }


        public static byte[] GetMailLogoDataFromCache()
        {
            return Cache.Get<byte[]>(CacheKey);
        }

        public static void InsertMailLogoDataToCache(byte[] data)
        {
            Cache.Insert(CacheKey, data, DateTime.UtcNow.Add(TimeSpan.FromDays(1)));
        }

        public static void RemoveMailLogoDataFromCache()
        {
            Cache.Remove(CacheKey);
        }
    }
}