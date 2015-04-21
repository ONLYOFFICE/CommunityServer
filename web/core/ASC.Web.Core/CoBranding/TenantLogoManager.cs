/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace ASC.Web.Core.CoBranding
{
    public class TenantLogoManager
    {
        public static string GetFavicon(bool general, bool timeParam)
        {
            var faviconPath = "";
            if (CoBrandingEnabled)
            {
                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);
                faviconPath = _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.Favicon, general);
                if (timeParam) {
                    var now = DateTime.Now;
                    faviconPath = String.Format("{0}?t={1}", faviconPath, now.Ticks);
                }
            }
            else
            {
                faviconPath = TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Favicon, general);
            }
            
            return faviconPath;
        }

        public static string GetTopLogo(bool general)//LogoLightSmall
        {
            if (CoBrandingEnabled)
            {
                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);

                return _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.LightSmall, general);
            }
            return TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.LightSmall, general);
        }

        public static string GetLogoLight(bool general)
        {
            if (CoBrandingEnabled)
            {
                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);

                return _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.Light, general);
            }
            return TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Light, general);
        }

        public static string GetLogoDark(bool general) {
            if (CoBrandingEnabled)
            {
                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);

                var fromSettingsDarkLogoPath = _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.Dark, general);
                var defaultDarkLogoPath = TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Dark, general);
                
                if (!String.Equals(fromSettingsDarkLogoPath, defaultDarkLogoPath, StringComparison.OrdinalIgnoreCase))
                    return fromSettingsDarkLogoPath;
            }

            /*** simple scheme ***/
            var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
            return _tenantInfoSettings.GetAbsoluteCompanyLogoPath();
            /***/
        }

        public static string GetLogoDocsEditor(bool general)
        {
            if (CoBrandingEnabled)
            {
                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);
                return _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.DocsEditor, general);
            }
            return TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.DocsEditor, general);
        }

        public static bool IsRetina(HttpRequest request)
        {
            var isRetina = false;
            if (request != null && request.Cookies != null)
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

        public static bool CoBrandingEnabled {
            get
            {
                var s = WebConfigurationManager.AppSettings["web.hide-settings"] ?? "";
                if (string.IsNullOrEmpty(s)) return true;

                var hideSettings = s.Split(new[] { ',', ';', ' ' });
                return !hideSettings.Contains("WhiteLabel", StringComparer.CurrentCultureIgnoreCase);
            }
        }
    }
}