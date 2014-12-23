/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ASC.Web.Studio.Core
{
    public class SetupInfo
    {
        public static string StatisticTrackURL
        {
            get { return GetAppSettings("web.track-url", string.Empty); }
        }

        public static string UserVoiceURL
        {
            get { return GetAppSettings("web.uservoice", string.Empty); }
        }

        public static string MainLogoURL
        {
            get { return GetAppSettings("web.logo.main", string.Empty); }
        }

        public static string MainLogoMailTmplURL
        {
            get { return GetAppSettings("web.logo.mail.tmpl", string.Empty); }
        }

        public static List<CultureInfo> EnabledCultures
        {
            get
            {
                return GetAppSettings("web.cultures", "en-US")
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => CultureInfo.GetCultureInfo(l.Trim()))
                    .OrderBy(l => l.Name)
                    .ToList();
            }
        }

        public static double ExchangeRateRuble
        {
            get { return GetAppSettings("exchange-rate.ruble", 40.0); }
        }

        public static long MaxImageUploadSize
        {
            get { return 1024*1024; }
        }

        /// <summary>
        /// Max possible file size for not chunked upload. Less or equal than 100 mb.
        /// </summary>
        public static long MaxUploadSize
        {
            get { return Math.Min(AvailableFileSize, MaxChunkedUploadSize); }
        }

        public static long AvailableFileSize
        {
            get { return 100L*1024L*1024L; }
        }

        /// <summary>
        /// Max possible file size for chunked upload.
        /// </summary>
        public static long MaxChunkedUploadSize
        {
            get
            {
                var diskQuota = TenantExtra.GetTenantQuota();
                var usedSize = UserControls.Statistics.TenantStatisticsProvider.GetUsedSize();

                if (diskQuota != null)
                {
                    var freeSize = diskQuota.MaxTotalSize - usedSize;
                    if (freeSize < diskQuota.MaxFileSize)
                        return freeSize < 0 ? 0 : freeSize;

                    return diskQuota.MaxFileSize;
                }

                return 5*1024*1024;
            }
        }

        public static string TeamlabSiteRedirect
        {
            get
            {
                return GetAppSettings("web.teamlab-site", string.Empty);
                ;
            }
        }

        public static long ChunkUploadSize
        {
            get { return GetAppSettings("files.uploader.chunk-size", 5*1024*1024); }
        }

        public static bool ThirdPartyAuthEnabled
        {
            get { return String.Equals(GetAppSettings("web.thirdparty-auth", "true"), "true"); }
        }

        public static string NoTenantRedirectURL
        {
            get { return GetAppSettings("web.notenant-url", "http://www.onlyoffice.com/wrongportalname.aspx"); }
        }

        public static string[] CustomScripts
        {
            get { return GetAppSettings("web.custom-scripts", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public static string NotifyAddress
        {
            get { return GetAppSettings("web.promo-url", string.Empty); }
        }

        public static string TipsAddress
        {
            get { return GetAppSettings("web.promo-tips-url", string.Empty); }
        }

        public static string UserForum
        {
            get { return GetAppSettings("web.user-forum", string.Empty); }
        }

        public static string GetImportServiceUrl()
        {
            var url = GetAppSettings("web.import-contacts-url", string.Empty);
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            var urlSeparatorChar = "?";
            if (url.Contains(urlSeparatorChar))
            {
                urlSeparatorChar = "&";
            }
            var cultureName = HttpUtility.HtmlEncode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            return UrlSwitcher.SelectCurrentUriScheme(string.Format("{0}{2}culture={1}&mobile={3}", url, cultureName, urlSeparatorChar, MobileDetector.IsMobile));
        }

        public static string BaseDomain
        {
            get { return GetAppSettings("core.base-domain", string.Empty); }
        }

        public static string WebApiBaseUrl
        {
            get { return VirtualPathUtility.ToAbsolute(GetAppSettings("api.url", "~/api/2.0/")); }
        }

        public static TimeSpan ValidEamilKeyInterval
        {
            get { return GetAppSettings("email.validinterval", TimeSpan.FromDays(7)); }
        }

        public static bool IsSecretEmail(string email)
        {
            var s = (ConfigurationManager.AppSettings["web.autotest.secret-email"] ?? "").Trim();

            //the point is not needed in gmail.com
            email = Regex.Replace(email ?? "", "\\.*(?=\\S*(@gmail.com$))", "");

            return !string.IsNullOrEmpty(s) &&
                   s.Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries).Contains(email, StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsVisibleSettings<TSettings>()
        {
            return IsVisibleSettings(typeof(TSettings).Name);
        }

        public static bool IsVisibleSettings(string settings)
        {
            var s = GetAppSettings("web.hide-settings", null);
            if (string.IsNullOrEmpty(s)) return true;

            var hideSettings = s.Split(new[] { ',', ';', ' ' });
            return !hideSettings.Contains(settings, StringComparer.CurrentCultureIgnoreCase);
        }

        private static string GetAppSettings(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        private static T GetAppSettings<T>(string key, T defaultValue)
        {
            var configSetting = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configSetting))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    return (T)converter.ConvertFromString(configSetting);
                }
            }
            return defaultValue;
        }
    }
}