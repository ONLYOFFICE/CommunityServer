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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using Newtonsoft.Json;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class CompanyWhiteLabelSettings : BaseSettings<CompanyWhiteLabelSettings>
    {
        [DataMember(Name = "CompanyName")]
        public string CompanyName { get; set; }

        [DataMember(Name = "Site")]
        public string Site { get; set; }

        [DataMember(Name = "Email")]
        public string Email { get; set; }

        [DataMember(Name = "Address")]
        public string Address { get; set; }

        [DataMember(Name = "Phone")]
        public string Phone { get; set; }

        [DataMember(Name = "IsLicensor")]
        public bool IsLicensorSetting { get; set; }

        public bool IsLicensor
        {
            get
            {
                return IsLicensorSetting
                    && (IsDefault || CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).Branding);
            }
            set { IsLicensorSetting = value; }
        }

        public bool IsDefault
        {
            get
            {
                var defaultSettings = GetDefault() as CompanyWhiteLabelSettings;

                if (defaultSettings == null) return false;

                return CompanyName == defaultSettings.CompanyName &&
                       Site == defaultSettings.Site &&
                       Email == defaultSettings.Email &&
                       Address == defaultSettings.Address &&
                       Phone == defaultSettings.Phone &&
                       IsLicensorSetting == defaultSettings.IsLicensorSetting;
            }
        }

        #region ISettings Members

        public override Guid ID
        {
            get { return new Guid("{C3C5A846-01A3-476D-A962-1CFD78C04ADB}"); }
        }

        private static CompanyWhiteLabelSettings _default;

        public override ISettings GetDefault()
        {
            if (_default != null) return _default;

            var settings = CoreContext.Configuration.GetSetting("CompanyWhiteLabelSettings");

            _default = string.IsNullOrEmpty(settings) ? new CompanyWhiteLabelSettings() : JsonConvert.DeserializeObject<CompanyWhiteLabelSettings>(settings);

            return _default;
        }

        #endregion

        public static CompanyWhiteLabelSettings Instance
        {
            get
            {
                return LoadForDefaultTenant();
            }
        }
    }
}
