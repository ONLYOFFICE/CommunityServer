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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core
{
    public static class SetupInfo
    {
        private static string web_autotest_secret_email;
        private static string[] web_display_mobapps_banner;
        private static string[] hideSettings;


        public static string MetaImageURL
        {
            get;
            private set;
        }

        public static string StatisticTrackURL
        {
            get;
            private set;
        }

        public static string UserVoiceURL
        {
            get;
            private set;
        }

        public static string MainLogoURL
        {
            get;
            private set;
        }

        public static string MainLogoMailTmplURL
        {
            get;
            private set;
        }

        public static List<CultureInfo> EnabledCultures
        {
            get;
            private set;
        }

        public static List<CultureInfo> EnabledCulturesPersonal
        {
            get;
            private set;
        }

        public static decimal ExchangeRateRuble
        {
            get;
            private set;
        }

        public static long MaxImageUploadSize
        {
            get;
            private set;
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
            get { return 100L * 1024L * 1024L; }
        }

        /// <summary>
        /// Max possible file size for chunked upload.
        /// </summary>
        public static long MaxChunkedUploadSize
        {
            get
            {
                var diskQuota = TenantExtra.GetTenantQuota();
                if (diskQuota != null)
                {
                    var usedSize = TenantStatisticsProvider.GetUsedSize();
                    var freeSize = Math.Max(diskQuota.MaxTotalSize - usedSize, 0);
                    return Math.Min(freeSize, diskQuota.MaxFileSize);
                }
                return ChunkUploadSize;
            }
        }

        public static string TeamlabSiteRedirect
        {
            get;
            private set;
        }

        public static long ChunkUploadSize
        {
            get;
            private set;
        }

        public static bool ThirdPartyAuthEnabled
        {
            get;
            private set;
        }

        public static string NoTenantRedirectURL
        {
            get;
            private set;
        }

        public static string[] CustomScripts
        {
            get;
            private set;
        }

        public static string NotifyAddress
        {
            get;
            private set;
        }

        public static string TipsAddress
        {
            get;
            private set;
        }

        public static string UserForum
        {
            get;
            private set;
        }

        public static string SupportFeedback
        {
            get;
            private set;
        }

        public static string WebApiBaseUrl
        {
            get { return VirtualPathUtility.ToAbsolute(GetAppSettings("api.url", "~/api/2.0/")); }
        }

        public static TimeSpan ValidEmailKeyInterval
        {
            get;
            private set;
        }

        public static TimeSpan ValidAuthKeyInterval
        {
            get;
            private set;
        }

        public static string SalesEmail
        {
            get;
            private set;
        }

        public static bool IsSecretEmail(string email)
        {
            var s = web_autotest_secret_email;
            //the point is not needed in gmail.com
            email = Regex.Replace(email ?? "", "\\.*(?=\\S*(@gmail.com$))", "");
            return !string.IsNullOrEmpty(s) && s.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(email, StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool DisplayMobappBanner(string product)
        {
            return web_display_mobapps_banner.Contains(product, StringComparer.InvariantCultureIgnoreCase);
        }

        public static bool DisplayPersonalBanners
        {
            get;
            private set;
        }

        public static string ShareTwitterUrl
        {
            get;
            private set;
        }

        public static string ShareFacebookUrl
        {
            get;
            private set;
        }

        public static string ControlPanelUrl
        {
            get;
            private set;
        }

        public static string FontOpenSansUrl
        {
            get;
            private set;
        }

        public static string VoipEnabled
        {
            get;
            private set;
        }


        public static string StartProductList
        {
            get;
            private set;
        }

        public static string SsoSamlLoginUrl
        {
            get;
            private set;
        }

        public static string DownloadForDesktopUrl
        {
            get;
            private set;
        }
        public static string DownloadForIosDocuments
        {
            get;
            private set;
        }
        public static string DownloadForIosProjects
        {
            get;
            private set;
        }
        public static string DownloadForAndroidDocuments
        {
            get;
            private set;
        }

        public static string SsoSamlLogoutUrl
        {
            get;
            private set;
        }


        public static bool SmsTrial
        {
            get;
            private set;
        }

        public static string TfaRegistration
        {
            get;
            private set;
        }

        public static int TfaAppBackupCodeLength
        {
            get;
            private set;
        }

        public static int TfaAppBackupCodeCount
        {
            get;
            private set;
        }

        public static string TfaAppSender
        {
            get;
            private set;
        }

        public static string NotifyAnalyticsUrl
        {
            get;
            private set;
        }

        static SetupInfo()
        {
            Refresh();
        }

        public static void Refresh()
        {
            MetaImageURL = GetAppSettings("web.meta-image-url", "https://download.onlyoffice.com/assets/fb/fb_icon_325x325.jpg");
            StatisticTrackURL = GetAppSettings("web.track-url", string.Empty);
            UserVoiceURL = GetAppSettings("web.uservoice", string.Empty);
            MainLogoURL = GetAppSettings("web.logo.main", string.Empty);
            MainLogoMailTmplURL = GetAppSettings("web.logo.mail.tmpl", string.Empty);
            DownloadForDesktopUrl = GetAppSettings("web.download.for.desktop.url", "https://www.onlyoffice.com/desktop.aspx");
            DownloadForIosDocuments = GetAppSettings("web.download.for.ios.doc", "https://itunes.apple.com/app/onlyoffice-documents/id944896972");
            DownloadForIosProjects = GetAppSettings("web.download.for.ios.proj", "https://itunes.apple.com/app/onlyoffice-projects/id1353395928?mt=8");
            DownloadForAndroidDocuments = GetAppSettings("web.download.for.android.doc", "https://play.google.com/store/apps/details?id=com.onlyoffice.documents");

            EnabledCultures = GetAppSettings("web.cultures", "en-US")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => CultureInfo.GetCultureInfo(l.Trim()))
                .OrderBy(l => l.DisplayName)
                .ToList();
            EnabledCulturesPersonal = GetAppSettings("web.cultures.personal", GetAppSettings("web.cultures", "en-US"))
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => CultureInfo.GetCultureInfo(l.Trim()))
                .OrderBy(l => l.DisplayName)
                .ToList();

            ExchangeRateRuble = GetAppSettings("exchange-rate.ruble", 65);
            MaxImageUploadSize = GetAppSettings<long>("web.max-upload-size", 1024 * 1024);

            TeamlabSiteRedirect = GetAppSettings("web.teamlab-site", string.Empty);
            ChunkUploadSize = GetAppSettings("files.uploader.chunk-size", 5 * 1024 * 1024);
            ThirdPartyAuthEnabled = String.Equals(GetAppSettings("web.thirdparty-auth", "true"), "true");
            NoTenantRedirectURL = GetAppSettings("web.notenant-url", "");
            CustomScripts = GetAppSettings("web.custom-scripts", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            NotifyAddress = GetAppSettings("web.promo-url", string.Empty);
            TipsAddress = GetAppSettings("web.promo-tips-url", string.Empty);
            UserForum = GetAppSettings("web.user-forum", string.Empty);
            SupportFeedback = GetAppSettings("web.support-feedback", string.Empty);

            ValidEmailKeyInterval = GetAppSettings("email.validinterval", TimeSpan.FromDays(7));
            ValidAuthKeyInterval = GetAppSettings("auth.validinterval", TimeSpan.FromHours(1));

            SalesEmail = GetAppSettings("web.payment.email", "sales@onlyoffice.com");
            web_autotest_secret_email = (ConfigurationManager.AppSettings["web.autotest.secret-email"] ?? "").Trim();
            web_display_mobapps_banner = (ConfigurationManager.AppSettings["web.display.mobapps.banner"] ?? "").Trim().Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            DisplayPersonalBanners = GetAppSettings("web.display.personal.banners", false);
            ShareTwitterUrl = GetAppSettings("web.share.twitter", "https://twitter.com/intent/tweet?text={0}");
            ShareFacebookUrl = GetAppSettings("web.share.facebook", "http://www.facebook.com/sharer.php?s=100&p[url]={0}&p[title]={1}&p[images][0]={2}&p[summary]={3}");
            ControlPanelUrl = GetAppSettings("web.controlpanel.url", "");
            FontOpenSansUrl = GetAppSettings("web.font.opensans.url", "");
            VoipEnabled = GetAppSettings("voip.enabled", "false");
            StartProductList = GetAppSettings("web.start.product.list", "");
            SsoSamlLoginUrl = GetAppSettings("web.sso.saml.login.url", "");
            SsoSamlLogoutUrl = GetAppSettings("web.sso.saml.logout.url", "");

            hideSettings = GetAppSettings("web.hide-settings", string.Empty).Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries);

            SmsTrial = GetAppSettings("core.sms.trial", false);

            TfaRegistration = (GetAppSettings("core.tfa.registration", "") ?? "").Trim().ToLower();

            TfaAppBackupCodeLength = GetAppSettings("web.tfaapp.backup.length", 6);
            TfaAppBackupCodeCount = GetAppSettings("web.tfaapp.backup.count", 5);
            TfaAppSender = GetAppSettings("web.tfaapp.backup.title", "ONLYOFFICE");

            NotifyAnalyticsUrl = GetAppSettings("core.notify.analytics.url", "");
        }


        public static bool IsVisibleSettings<TSettings>()
        {
            return IsVisibleSettings(typeof(TSettings).Name);
        }

        public static bool IsVisibleSettings(string settings)
        {
            return hideSettings == null || !hideSettings.Contains(settings, StringComparer.CurrentCultureIgnoreCase);
        }


        private static string GetAppSettings(string key, string defaultValue)
        {
            var result = ConfigurationManager.AppSettings[key] ?? defaultValue;

            if (!String.IsNullOrEmpty(result))
                result = result.Trim();

            return result;

        }

        private static T GetAppSettings<T>(string key, T defaultValue)
        {
            var configSetting = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configSetting))
            {
                configSetting = configSetting.Trim();
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