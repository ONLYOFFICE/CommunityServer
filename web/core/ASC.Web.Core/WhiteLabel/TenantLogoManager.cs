/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.WhiteLabel
{
    public class TenantLogoManager
    {
        private static readonly ICache Cache = AscCache.Default;
        private const string CacheKey = "letterlogodata";

        public static bool WhiteLabelEnabled
        {
            get;
            private set;
        }


        static TenantLogoManager()
        {
            var hideSettings = (WebConfigurationManager.AppSettings["web.hide-settings"] ?? "").Split(new[] { ',', ';', ' ' });
            WhiteLabelEnabled = !hideSettings.Contains("WhiteLabel", StringComparer.CurrentCultureIgnoreCase);
        }


        public static string GetFavicon(bool general, bool timeParam)
        {
            string faviconPath;
            if (WhiteLabelEnabled)
            {
                var tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                faviconPath = tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, general);
                if (timeParam) {
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

        public static string GetLogoDark(bool general) {
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
            var isRetina = false;
            if (request != null)
            {
                var cookie = request.Cookies["is_retina"];
                if (cookie != null && !String.IsNullOrEmpty(cookie.Value))
                {
                    bool result;
                    if (Boolean.TryParse(cookie.Value, out result))
                    {
                        isRetina = result;
                    }
                }
            }
            return isRetina;
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