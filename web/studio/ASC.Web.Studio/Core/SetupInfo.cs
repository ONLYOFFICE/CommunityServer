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
            get;
            private set;
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

        public static bool ThirdPartyBannerEnabled
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
            //the point is not needed in gmail.com
            email = Regex.Replace(email ?? "", "\\.*(?=\\S*(@gmail.com$))", "").ToLower();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(web_autotest_secret_email))
                return false;

            var regex = new Regex(web_autotest_secret_email, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            return regex.IsMatch(email);
        }

        public static bool DisplayMobappBanner(string product)
        {
            return web_display_mobapps_banner.Contains(product, StringComparer.InvariantCultureIgnoreCase);
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

        public static bool VoipEnabled
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

        public static string RecaptchaPublicKey
        {
            get;
            private set;
        }

        public static string RecaptchaPrivateKey
        {
            get;
            private set;
        }

        public static string RecaptchaVerifyUrl
        {
            get;
            private set;
        }

        public static int LoginThreshold
        {
            get;
            private set;
        }
        
        public static string AmiMetaUrl
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
            AvailableFileSize = GetAppSettings("web.available-file-size", 100L * 1024L * 1024L);

            TeamlabSiteRedirect = GetAppSettings("web.teamlab-site", string.Empty);
            ChunkUploadSize = GetAppSettings("files.uploader.chunk-size", 5 * 1024 * 1024);
            ThirdPartyAuthEnabled = string.Equals(GetAppSettings("web.thirdparty-auth", "true"), "true");
            ThirdPartyBannerEnabled = string.Equals(GetAppSettings("web.thirdparty-banner", "false"), "true");
            NoTenantRedirectURL = GetAppSettings("web.notenant-url", "");
            CustomScripts = GetAppSettings("web.custom-scripts", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            NotifyAddress = GetAppSettings("web.promo-url", string.Empty);
            TipsAddress = GetAppSettings("web.promo-tips-url", string.Empty);
            UserForum = GetAppSettings("web.user-forum", string.Empty);
            SupportFeedback = GetAppSettings("web.support-feedback", string.Empty);

            ValidEmailKeyInterval = GetAppSettings("email.validinterval", TimeSpan.FromDays(7));
            ValidAuthKeyInterval = GetAppSettings("auth.validinterval", TimeSpan.FromHours(1));

            SalesEmail = GetAppSettings("web.payment.email", "sales@onlyoffice.com");
            web_autotest_secret_email = (ConfigurationManagerExtension.AppSettings["web.autotest.secret-email"] ?? "").Trim();

            RecaptchaPublicKey = GetAppSettings("web.recaptcha.public-key", "");
            RecaptchaPrivateKey = GetAppSettings("web.recaptcha.private-key", "");
            RecaptchaVerifyUrl = GetAppSettings("web.recaptcha.verify-url", "https://www.google.com/recaptcha/api/siteverify");
            LoginThreshold = Convert.ToInt32(GetAppSettings("web.login.threshold", "0"));
            if (LoginThreshold < 1) LoginThreshold = 5;

            web_display_mobapps_banner = (ConfigurationManagerExtension.AppSettings["web.display.mobapps.banner"] ?? "").Trim().Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ShareTwitterUrl = GetAppSettings("web.share.twitter", "https://twitter.com/intent/tweet?text={0}");
            ShareFacebookUrl = GetAppSettings("web.share.facebook", "");
            ControlPanelUrl = GetAppSettings("web.controlpanel.url", "");
            FontOpenSansUrl = GetAppSettings("web.font.opensans.url", "");
            VoipEnabled = GetAppSettings("voip.enabled", true);
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

            AmiMetaUrl = GetAppSettings("web.ami.meta", "");
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
            var result = ConfigurationManagerExtension.AppSettings[key] ?? defaultValue;

            if (!string.IsNullOrEmpty(result))
                result = result.Trim();

            return result;

        }

        private static T GetAppSettings<T>(string key, T defaultValue)
        {
            var configSetting = ConfigurationManagerExtension.AppSettings[key];
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