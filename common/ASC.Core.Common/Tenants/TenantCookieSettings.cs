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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Core.Tenants
{
    [Serializable]
    [DataContract]
    public class TenantCookieSettings : BaseSettings<TenantCookieSettings>
    {
        [DataMember(Name = "Index")]
        public int Index { get; set; }

        [DataMember(Name = "LifeTime")]
        public int LifeTime { get; set; }

        private static readonly bool IsVisibleSettings;

        static TenantCookieSettings()
        {
            IsVisibleSettings = !(ConfigurationManagerExtension.AppSettings["web.hide-settings"] ?? string.Empty)
                        .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Contains("CookieSettings", StringComparer.CurrentCultureIgnoreCase);
        }

        public override ISettings GetDefault()
        {
            return GetInstance();
        }

        public static TenantCookieSettings GetInstance()
        {
            return new TenantCookieSettings();
        }

        public override Guid ID
        {
            get { return new Guid("{16FB8E67-E96D-4B22-B217-C80F25C5DE1B}"); }
        }


        public bool IsDefault()
        {
            var defaultSettings = GetInstance();

            return LifeTime == defaultSettings.LifeTime;
        }


        public static TenantCookieSettings GetForTenant(int tenantId)
        {
            return IsVisibleSettings
                       ? LoadForTenant(tenantId)
                       : GetInstance();
        }

        public static void SetForTenant(int tenantId, TenantCookieSettings settings = null)
        {
            if (!IsVisibleSettings) return;
            (settings ?? GetInstance()).SaveForTenant(tenantId);
        }

        public static TenantCookieSettings GetForUser(Guid userId)
        {
            return IsVisibleSettings
                       ? LoadForUser(userId)
                       : GetInstance();
        }

        public static TenantCookieSettings GetForUser(int tenantId, Guid userId)
        {
            return IsVisibleSettings
                       ? SettingsManager.Instance.LoadSettingsFor<TenantCookieSettings>(tenantId, userId)
                       : GetInstance();
        }

        public static void SetForUser(Guid userId, TenantCookieSettings settings = null)
        {
            if (!IsVisibleSettings) return;
            (settings ?? GetInstance()).SaveForUser(userId);
        }

        public static DateTime GetExpiresTime(int tenantId)
        {
            var settingsTenant = GetForTenant(tenantId);
            var expires = settingsTenant.IsDefault() ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMinutes(settingsTenant.LifeTime);
            return expires;
        }
    }
}