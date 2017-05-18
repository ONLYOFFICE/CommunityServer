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
using System.Globalization;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [Serializable]
    [DataContract]
    public class TariffSettings : ISettings
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        [DataMember(Name = "HideRecommendation")]
        public bool HideBuyRecommendationSetting { get; set; }

        [DataMember(Name = "HidePricingPage")]
        public bool HidePricingPageForUsers { get; set; }

        [DataMember(Name = "LicenseAccept")]
        public string LicenseAcceptSetting { get; set; }

        public ISettings GetDefault()
        {
            return new TariffSettings
                {
                    HideBuyRecommendationSetting = false,
                    HidePricingPageForUsers = false,
                    LicenseAcceptSetting = DateTime.MinValue.ToString(CultureInfo),
                };
        }

        public Guid ID
        {
            get { return new Guid("{07956D46-86F7-433b-A657-226768EF9B0D}"); }
        }

        public static bool HideRecommendation
        {
            get { return SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID).HideBuyRecommendationSetting; }
            set
            {
                var tariffSettings = SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID);
                tariffSettings.HideBuyRecommendationSetting = value;
                SettingsManager.Instance.SaveSettingsFor(tariffSettings, SecurityContext.CurrentAccount.ID);
            }
        }

        public static bool HidePricingPage
        {
            get { return SettingsManager.Instance.LoadSettings<TariffSettings>(TenantProvider.CurrentTenantID).HidePricingPageForUsers; }
            set
            {
                var tariffSettings = SettingsManager.Instance.LoadSettings<TariffSettings>(TenantProvider.CurrentTenantID);
                tariffSettings.HidePricingPageForUsers = value;
                SettingsManager.Instance.SaveSettings(tariffSettings, TenantProvider.CurrentTenantID);
            }
        }

        public static bool LicenseAccept
        {
            get
            {
                return !DateTime.MinValue.ToString(CultureInfo)
                                .Equals(SettingsManager.Instance.LoadSettings<TariffSettings>(Tenant.DEFAULT_TENANT).LicenseAcceptSetting);
            }
            set
            {
                var tariffSettings = SettingsManager.Instance.LoadSettings<TariffSettings>(Tenant.DEFAULT_TENANT);
                if (DateTime.MinValue.ToString(CultureInfo).Equals(tariffSettings.LicenseAcceptSetting))
                {
                    tariffSettings.LicenseAcceptSetting = DateTime.UtcNow.ToString(CultureInfo);
                    SettingsManager.Instance.SaveSettings(tariffSettings, Tenant.DEFAULT_TENANT);
                }
            }
        }
    }
}