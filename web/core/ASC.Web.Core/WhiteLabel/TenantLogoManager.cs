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


using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace ASC.Web.Core.WhiteLabel
{
    public class TenantLogoManager
    {
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
            var faviconPath = "";
            if (WhiteLabelEnabled)
            {
                var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);
                faviconPath = _tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, general);
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
                var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);

                return _tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.LightSmall, general);
            }
            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.LightSmall, general);
        }

        public static string GetLogoDark(bool general) {
            if (WhiteLabelEnabled)
            {
                var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);
                return _tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
            }

            /*** simple scheme ***/
            var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
            return _tenantInfoSettings.GetAbsoluteCompanyLogoPath();
            /***/
        }

        public static string GetLogoDocsEditor(bool general)
        {
            if (WhiteLabelEnabled)
            {
                var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);
                return _tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, general);
            }
            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, general);
        }

        public static string GetLogoText()
        {
            if (WhiteLabelEnabled)
            {
                var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);

                return _tenantWhiteLabelSettings.LogoText != null ? _tenantWhiteLabelSettings.LogoText : TenantWhiteLabelSettings.DefaultLogoText;
            }
            return TenantWhiteLabelSettings.DefaultLogoText;
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

         public static bool WhiteLabelPaid
        {
            get
            {
                return CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).WhiteLabel;
            }
        }
    }
}