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
using System.Globalization;
using System.Runtime.Serialization;

using ASC.Core.Common.Settings;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class AdditionalWhiteLabelSettings : BaseSettings<AdditionalWhiteLabelSettings>
    {
        [DataMember(Name = "StartDocsEnabled")]
        public bool StartDocsEnabled { get; set; }

        [DataMember(Name = "HelpCenterEnabled")]
        public bool HelpCenterEnabled { get; set; }

        [DataMember(Name = "FeedbackAndSupportEnabled")]
        public bool FeedbackAndSupportEnabled { get; set; }

        [DataMember(Name = "FeedbackAndSupportUrl")]
        public string FeedbackAndSupportUrl { get; set; }

        [DataMember(Name = "UserForumEnabled")]
        public bool UserForumEnabled { get; set; }

        [DataMember(Name = "UserForumUrl")]
        public string UserForumUrl { get; set; }

        [DataMember(Name = "VideoGuidesEnabled")]
        public bool VideoGuidesEnabled { get; set; }

        [DataMember(Name = "VideoGuidesUrl")]
        public string VideoGuidesUrl { get; set; }

        [DataMember(Name = "SalesEmail")]
        public string SalesEmail { get; set; }

        [DataMember(Name = "BuyUrl")]
        public string BuyUrl { get; set; }

        [DataMember(Name = "LicenseAgreementsEnabled")]
        public bool LicenseAgreementsEnabled { get; set; }

        [DataMember(Name = "LicenseAgreementsUrl")]
        public string LicenseAgreementsUrl { get; set; }

        public bool IsDefault
        {
            get
            {
                var defaultSettings = GetDefault() as AdditionalWhiteLabelSettings;

                if (defaultSettings == null) return false;

                return StartDocsEnabled == defaultSettings.StartDocsEnabled &&
                       HelpCenterEnabled == defaultSettings.HelpCenterEnabled &&
                       FeedbackAndSupportEnabled == defaultSettings.FeedbackAndSupportEnabled &&
                       FeedbackAndSupportUrl == defaultSettings.FeedbackAndSupportUrl &&
                       UserForumEnabled == defaultSettings.UserForumEnabled &&
                       UserForumUrl == defaultSettings.UserForumUrl &&
                       VideoGuidesEnabled == defaultSettings.VideoGuidesEnabled &&
                       VideoGuidesUrl == defaultSettings.VideoGuidesUrl &&
                       SalesEmail == defaultSettings.SalesEmail &&
                       BuyUrl == defaultSettings.BuyUrl &&
                       LicenseAgreementsEnabled == defaultSettings.LicenseAgreementsEnabled &&
                       LicenseAgreementsUrl == defaultSettings.LicenseAgreementsUrl;
            }
        }

        #region ISettings Members

        public override Guid ID
        {
            get { return new Guid("{0108422F-C05D-488E-B271-30C4032494DA}"); }
        }

        public override ISettings GetDefault()
        {
            return new AdditionalWhiteLabelSettings
            {
                StartDocsEnabled = true,
                HelpCenterEnabled = DefaultHelpCenterUrl != null,
                FeedbackAndSupportEnabled = DefaultFeedbackAndSupportUrl != null,
                FeedbackAndSupportUrl = DefaultFeedbackAndSupportUrl,
                UserForumEnabled = DefaultUserForumUrl != null,
                UserForumUrl = DefaultUserForumUrl,
                VideoGuidesEnabled = DefaultVideoGuidesUrl != null,
                VideoGuidesUrl = DefaultVideoGuidesUrl,
                SalesEmail = DefaultMailSalesEmail,
                BuyUrl = DefaultBuyUrl,
                LicenseAgreementsEnabled = true,
                LicenseAgreementsUrl = DefaultLicenseAgreements
            };
        }

        #endregion

        #region Default values

        public static string DefaultHelpCenterUrl
        {
            get
            {
                var url = ConfigurationManagerExtension.AppSettings["web.help-center"];
                return string.IsNullOrEmpty(url) ? null : url;
            }
        }

        public static string DefaultFeedbackAndSupportUrl
        {
            get
            {
                var url = ConfigurationManagerExtension.AppSettings["web.support-feedback"];
                return string.IsNullOrEmpty(url) ? null : url;
            }
        }

        public static string DefaultUserForumUrl
        {
            get
            {
                var url = ConfigurationManagerExtension.AppSettings["web.user-forum"];
                return string.IsNullOrEmpty(url) ? null : url;
            }
        }

        public static string DefaultVideoGuidesUrl
        {
            get
            {
                var url = DefaultHelpCenterUrl;
                return string.IsNullOrEmpty(url) ? null : url + "/video.aspx";
            }
        }

        public static string DefaultMailSalesEmail
        {
            get
            {
                var email = ConfigurationManagerExtension.AppSettings["web.payment.email"];
                return !string.IsNullOrEmpty(email) ? email : "sales@onlyoffice.com";
            }
        }

        public static string DefaultBuyUrl
        {
            get
            {
                var site = ConfigurationManagerExtension.AppSettings["web.teamlab-site"];
                return !string.IsNullOrEmpty(site) ? site + "/post.ashx?type=buyenterprise" : "";
            }
        }

        public static string DefaultLicenseAgreements
        {
            get
            {
                return "https://help.onlyoffice.com/Products/Files/doceditor.aspx?fileid=6795868&doc=RG5GaVN6azdUQW5kLzZQNzBXbHZ4Rm9QWVZuNjZKUmgya0prWnpCd2dGcz0_IjY3OTU4Njgi0";
            }
        }

        #endregion

        public static AdditionalWhiteLabelSettings Instance
        {
            get
            {
                return LoadForDefaultTenant();
            }
        }
    }
}
