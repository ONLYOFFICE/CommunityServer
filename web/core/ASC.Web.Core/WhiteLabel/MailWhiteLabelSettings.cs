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
using System.Runtime.Serialization;

using ASC.Core.Common.Settings;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class MailWhiteLabelSettings : BaseSettings<MailWhiteLabelSettings>
    {
        [DataMember(Name = "FooterEnabled")]
        public bool FooterEnabled { get; set; }

        [DataMember(Name = "FooterSocialEnabled")]
        public bool FooterSocialEnabled { get; set; }

        [DataMember(Name = "SupportUrl")]
        public string SupportUrl { get; set; }

        [DataMember(Name = "SupportEmail")]
        public string SupportEmail { get; set; }

        [DataMember(Name = "SalesEmail")]
        public string SalesEmail { get; set; }

        [DataMember(Name = "DemoUrl")]
        public string DemoUrl { get; set; }

        [DataMember(Name = "SiteUrl")]
        public string SiteUrl { get; set; }

        public bool IsDefault
        {
            get
            {
                var defaultSettings = GetDefault() as MailWhiteLabelSettings;

                if (defaultSettings == null) return false;

                return FooterEnabled == defaultSettings.FooterEnabled &&
                       FooterSocialEnabled == defaultSettings.FooterSocialEnabled &&
                       SupportUrl == defaultSettings.SupportUrl &&
                       SupportEmail == defaultSettings.SupportEmail &&
                       SalesEmail == defaultSettings.SalesEmail &&
                       DemoUrl == defaultSettings.DemoUrl &&
                       SiteUrl == defaultSettings.SiteUrl;
            }
        }

        #region ISettings Members

        public override Guid ID
        {
            get { return new Guid("{C3602052-5BA2-452A-BD2A-ADD0FAF8EB88}"); }
        }

        public override ISettings GetDefault()
        {
            return new MailWhiteLabelSettings
            {
                FooterEnabled = true,
                FooterSocialEnabled = true,
                SupportUrl = DefaultMailSupportUrl,
                SupportEmail = DefaultMailSupportEmail,
                SalesEmail = DefaultMailSalesEmail,
                DemoUrl = DefaultMailDemoUrl,
                SiteUrl = DefaultMailSiteUrl
            };
        }

        #endregion

        #region Default values

        public static string DefaultMailSupportUrl
        {
            get
            {
                var url = CommonLinkUtility.GetRegionalUrl(ConfigurationManagerExtension.AppSettings["web.support-feedback"] ?? String.Empty, null);
                return !string.IsNullOrEmpty(url) ? url : "https://helpdesk.onlyoffice.com";
            }
        }

        public static string DefaultMailSupportEmail
        {
            get
            {
                var email = ConfigurationManagerExtension.AppSettings["web.support.email"];
                return !string.IsNullOrEmpty(email) ? email : "support@onlyoffice.com";
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

        public static string DefaultMailDemoUrl
        {
            get
            {
                var url = CommonLinkUtility.GetRegionalUrl(ConfigurationManagerExtension.AppSettings["web.demo-order"] ?? String.Empty, null);
                return !string.IsNullOrEmpty(url) ? url : "http://www.onlyoffice.com/demo-order.aspx";
            }
        }

        public static string DefaultMailSiteUrl
        {
            get
            {
                var url = ConfigurationManagerExtension.AppSettings["web.teamlab-site"];
                return !string.IsNullOrEmpty(url) ? url : "http://www.onlyoffice.com";
            }
        }

        #endregion

        public static MailWhiteLabelSettings Instance
        {
            get
            {
                return LoadForDefaultTenant();
            }
        }
    }
}
