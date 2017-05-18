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
using System.Runtime.Serialization;
using System.Web.Configuration;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class MailWhiteLabelSettings : ISettings
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

        [DataMember(Name = "DemotUrl")]
        public string DemotUrl { get; set; }

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
                       DemotUrl == defaultSettings.DemotUrl &&
                       SiteUrl == defaultSettings.SiteUrl;
            }
        }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{C3602052-5BA2-452A-BD2A-ADD0FAF8EB88}"); }
        }

        public ISettings GetDefault()
        {
            return new MailWhiteLabelSettings
                {
                    FooterEnabled = true,
                    FooterSocialEnabled = true,
                    SupportUrl = DefaultMailSupportUrl,
                    SupportEmail = DefaultMailSupportEmail,
                    SalesEmail = DefaultMailSalesEmail,
                    DemotUrl = DefaultMailDemotUrl,
                    SiteUrl = DefaultMailSiteUrl
                };
        }

        #endregion

        #region Default values

        public static string DefaultMailSupportUrl
        {
            get
            {
                var url = CommonLinkUtility.GetRegionalUrl(WebConfigurationManager.AppSettings["web.support-feedback"] ?? String.Empty, null).TrimEnd('/');
                return !string.IsNullOrEmpty(url) ? url : "http://support.onlyoffice.com";
            }
        }

        public static string DefaultMailSupportEmail
        {
            get
            {
                var email = WebConfigurationManager.AppSettings["web.support.email"];
                return !string.IsNullOrEmpty(email) ? email : "support@onlyoffice.com";
            }
        }

        public static string DefaultMailSalesEmail
        {
            get
            {
                var email = WebConfigurationManager.AppSettings["web.payment.email"];
                return !string.IsNullOrEmpty(email) ? email : "sales@onlyoffice.com";
            }
        }

        public static string DefaultMailDemotUrl
        {
            get
            {
                var url = CommonLinkUtility.GetRegionalUrl(WebConfigurationManager.AppSettings["web.demo-order"] ?? String.Empty, null).TrimEnd('/');
                return !string.IsNullOrEmpty(url) ? url : "http://www.onlyoffice.com/demo-order.aspx";
            }
        }

        public static string DefaultMailSiteUrl
        {
            get
            {
                var url = WebConfigurationManager.AppSettings["web.teamlab-site"];
                return !string.IsNullOrEmpty(url) ? url : "http://www.onlyoffice.com";
            }
        }

        #endregion

        public static MailWhiteLabelSettings Instance
        {
            get
            {
                return SettingsManager.Instance.LoadSettings<MailWhiteLabelSettings>(Tenant.DEFAULT_TENANT);
            }
        }
    }
}
