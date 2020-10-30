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
                    HideNotifySetting = false,
                    HidePricingPageForUsers = false,
                    LicenseAcceptSetting = DateTime.MinValue.ToString(CultureInfo),
                };
        }

        public override Guid ID
        {
            get { return new Guid("{07956D46-86F7-433b-A657-226768EF9B0D}"); }
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