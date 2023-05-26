/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
            get
            {
                return CoreContext.Configuration.Standalone || CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).WhiteLabel;
            }
        }

        public static bool IsVisibleWhiteLabelSettings
        {
            get;
            private set;
        }


        static TenantLogoManager()
        {
            var hideSettings = (ConfigurationManagerExtension.AppSettings["web.hide-settings"] ?? "").Split(new[] { ',', ';', ' ' });
            IsVisibleWhiteLabelSettings = !hideSettings.Contains("WhiteLabel", StringComparer.CurrentCultureIgnoreCase);
        }


        public static string GetFavicon(bool general, bool timeParam)
        {
            if (WhiteLabelEnabled)
            {
                var faviconPath = TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, general);
                return timeParam ? string.Format("{0}?t={1}", faviconPath, DateTime.Now.Ticks) : faviconPath;
            }

            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Favicon, general);
        }

        public static string GetTopLogo(bool general)
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.LightSmall, general);
            }

            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.LightSmall, general);
        }

        public static string GetLogoDark(bool general)
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
            }

            if (IsVisibleWhiteLabelSettings)
            {
                return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
            }

            /*** simple scheme ***/
            return TenantInfoSettings.Load().GetAbsoluteCompanyLogoPath(true);
            /***/
        }

        public static string GetLogoLight(bool general)
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Light, general);
            }

            if (IsVisibleWhiteLabelSettings)
            {
                return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Light, general);
            }

            /*** simple scheme ***/
            return TenantInfoSettings.Load().GetAbsoluteCompanyLogoPath(false);
            /***/
        }

        public static string GetLogoAboutDark(bool general)
        {
            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.AboutDark, general);
        }

        public static string GetLogoAboutLight(bool general)
        {
            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.AboutLight, general);
        }

        public static string GetLogoDocsEditor(bool general)
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, general);
            }

            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, general);
        }

        public static string GetLogoDocsEditorEmbed(bool general)
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditorEmbed, general);
            }

            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.DocsEditorEmbed, general);
        }

        public static string GetLogoText()
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().LogoText ?? TenantWhiteLabelSettings.DefaultLogoText;
            }

            return TenantWhiteLabelSettings.DefaultLogoText;
        }

        public static bool IsRetina(HttpRequest request)
        {
            if (request != null)
            {
                var cookie = request.Cookies["is_retina"];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    bool result;
                    if (bool.TryParse(cookie.Value, out result))
                    {
                        return result;
                    }
                }
            }

            return !SecurityContext.IsAuthenticated;
        }

        /// <summary>
        /// Get logo stream or null in case of default logo
        /// </summary>
        public static Stream GetWhitelabelMailLogo()
        {
            if (WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetWhitelabelLogoData(WhiteLabelLogoTypeEnum.Dark, true);
            }

            if (IsVisibleWhiteLabelSettings)
            {
                return TenantWhiteLabelSettings.GetPartnerStorageLogoData(WhiteLabelLogoTypeEnum.Dark, true);
            }

            /*** simple scheme ***/
            return TenantInfoSettings.Load().GetStorageLogoData();
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