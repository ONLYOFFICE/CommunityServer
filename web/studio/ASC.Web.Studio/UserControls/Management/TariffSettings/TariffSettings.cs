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
using System.Globalization;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;

namespace ASC.Web.Studio.UserControls.Management
{
    [Serializable]
    [DataContract]
    public class TariffSettings : BaseSettings<TariffSettings>
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        [DataMember(Name = "HideRecommendation")]
        public bool HideBuyRecommendationSetting { get; set; }

        [DataMember(Name = "HideNotify")]
        public bool HideNotifySetting { get; set; }

        [DataMember(Name = "HidePricingPage")]
        public bool HidePricingPageForUsers { get; set; }

        [DataMember(Name = "LicenseAccept")]
        public string LicenseAcceptSetting { get; set; }

        public override ISettings GetDefault()
        {
            return new TariffSettings
                {
                    HideBuyRecommendationSetting = false,
                    HideNotifySetting = false,
                    HidePricingPageForUsers = false,
                    LicenseAcceptSetting = DateTime.MinValue.ToString(CultureInfo),
                };
        }

        public override Guid ID
        {
            get { return new Guid("{07956D46-86F7-433b-A657-226768EF9B0D}"); }
        }

        public static bool HideRecommendation
        {
            get { return LoadForCurrentUser().HideBuyRecommendationSetting; }
            set
            {
                var tariffSettings = LoadForCurrentUser();
                tariffSettings.HideBuyRecommendationSetting = value;
                tariffSettings.SaveForCurrentUser();
            }
        }

        public static bool HideNotify
        {
            get { return LoadForCurrentUser().HideNotifySetting; }
            set
            {
                var tariffSettings = LoadForCurrentUser();
                tariffSettings.HideNotifySetting = value;
                tariffSettings.SaveForCurrentUser();
            }
        }

        public static bool HidePricingPage
        {
            get { return Load().HidePricingPageForUsers; }
            set
            {
                var tariffSettings = Load();
                tariffSettings.HidePricingPageForUsers = value;
                tariffSettings.Save();
            }
        }

        public static bool LicenseAccept
        {
            get
            {
                return !DateTime.MinValue.ToString(CultureInfo).Equals(LoadForDefaultTenant().LicenseAcceptSetting);
            }
            set
            {
                var tariffSettings = LoadForDefaultTenant();
                if (DateTime.MinValue.ToString(CultureInfo).Equals(tariffSettings.LicenseAcceptSetting))
                {
                    tariffSettings.LicenseAcceptSetting = DateTime.UtcNow.ToString(CultureInfo);
                    tariffSettings.SaveForDefaultTenant();
                }
            }
        }
    }
}