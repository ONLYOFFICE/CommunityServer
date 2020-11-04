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


using System.Web;
using ASC.Core.Configuration;
using ASC.Core.Tenants;
using System;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;

namespace ASC.Core
{
    public class CoreConfiguration
    {
        private readonly ITenantService tenantService;
        private bool? standalone;
        private bool? personal;
        private bool? customMode;
        private long? personalMaxSpace;
        private string basedomain;


        public CoreConfiguration(ITenantService service)
        {
            tenantService = service;
        }

        public bool Standalone
        {
            get { return standalone ?? (bool)(standalone = ConfigurationManagerExtension.AppSettings["core.base-domain"] == "localhost"); }
        }

        public bool Personal
        {
            get
            {
                //todo: should replace only frotend
                if (CustomMode && HttpContext.Current != null && HttpContext.Current.Request.SailfishApp()) return true;

                return personal ?? (bool)(personal = ConfigurationManagerExtension.AppSettings["core.personal"] == "true");
            }
        }

        public bool CustomMode
        {
            get { return customMode ?? (bool)(customMode = ConfigurationManagerExtension.AppSettings["core.custom-mode"] == "true"); }
        }

        public long PersonalMaxSpace
        {
            get
            {
                var quotaSettings = PersonalQuotaSettings.LoadForCurrentUser();

                if (quotaSettings.MaxSpace != long.MaxValue)
                    return quotaSettings.MaxSpace;
                
                if (personalMaxSpace.HasValue)
                    return personalMaxSpace.Value;

                long value;

                if (!long.TryParse(ConfigurationManagerExtension.AppSettings["core.personal.maxspace"], out value))
                    value = long.MaxValue;

                personalMaxSpace = value;

                return personalMaxSpace.Value;
            }
        }

        public SmtpSettings SmtpSettings
        {
            get
            {
                var isDefaultSettings = false;
                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);

                if (tenant != null)
                {

                    var settingsValue = GetSetting("SmtpSettings", tenant.TenantId);
                    if (string.IsNullOrEmpty(settingsValue))
                    {
                        isDefaultSettings = true;
                        settingsValue = GetSetting("SmtpSettings");
                    }
                    var settings = SmtpSettings.Deserialize(settingsValue);
                    settings.IsDefaultSettings = isDefaultSettings;
                    return settings;
                }
                else
                {
                    var settingsValue = GetSetting("SmtpSettings");

                    var settings = SmtpSettings.Deserialize(settingsValue);
                    settings.IsDefaultSettings = true;
                    return settings;
                }
            }
            set { SaveSetting("SmtpSettings", value != null ? value.Serialize() : null, CoreContext.TenantManager.GetCurrentTenant().TenantId); }
        }

        public string BaseDomain
        {
            get
            {
                if (basedomain == null)
                {
                    basedomain = ConfigurationManagerExtension.AppSettings["core.base-domain"] ?? string.Empty;
                }

                string result;
                if (Standalone || string.IsNullOrEmpty(basedomain))
                {
                    result = GetSetting("BaseDomain") ?? basedomain;
                }
                else
                {
                    result = basedomain;
                }
                return result;
            }
            set
            {
                if (Standalone || string.IsNullOrEmpty(basedomain))
                {
                    SaveSetting("BaseDomain", value);
                }
            }
        }

        #region Methods Get/Save Setting

        public void SaveSetting(string key, string value, int tenant = Tenant.DEFAULT_TENANT)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            byte[] bytes = null;
            if (value != null)
            {
                bytes = Crypto.GetV(Encoding.UTF8.GetBytes(value), 2, true);
            }
            tenantService.SetTenantSettings(tenant, key, bytes);
        }

        public string GetSetting(string key, int tenant = Tenant.DEFAULT_TENANT)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            byte[] bytes = tenantService.GetTenantSettings(tenant, key);

            var result = bytes != null ? Encoding.UTF8.GetString(Crypto.GetV(bytes, 2, false)) : null;

            return result;
        }

        #endregion

        public string GetKey(int tenant)
        {
            if (Standalone)
            {
                var key = GetSetting("PortalId");
                if (string.IsNullOrEmpty(key))
                {
                    lock (tenantService)
                    {
                        // thread safe
                        key = GetSetting("PortalId");
                        if (string.IsNullOrEmpty(key))
                        {
                            SaveSetting("PortalId", key = Guid.NewGuid().ToString());
                        }
                    }
                }
                return key;
            }
            else
            {
                var t = tenantService.GetTenant(tenant);
                if (t != null && !string.IsNullOrWhiteSpace(t.PaymentId))
                    return t.PaymentId;

                return ConfigurationManagerExtension.AppSettings["core.payment-region"] + tenant;
            }
        }

        public string GetAffiliateId(int tenant)
        {
            var t = tenantService.GetTenant(tenant);
            if (t != null && !string.IsNullOrWhiteSpace(t.AffiliateId))
                return t.AffiliateId;

            return null;
        }

        public string GetCampaign(int tenant)
        {
            var t = tenantService.GetTenant(tenant);
            if (t != null && !string.IsNullOrWhiteSpace(t.Campaign))
                return t.Campaign;

            return null;
        }

        #region Methods Get/Set Section

        public T GetSection<T>() where T : class
        {
            return GetSection<T>(typeof(T).Name);
        }

        public T GetSection<T>(int tenantId) where T : class
        {
            return GetSection<T>(tenantId, typeof(T).Name);
        }

        public T GetSection<T>(string sectionName) where T : class
        {
            return GetSection<T>(CoreContext.TenantManager.GetCurrentTenant().TenantId, sectionName);
        }

        public T GetSection<T>(int tenantId, string sectionName) where T : class
        {
            var serializedSection = GetSetting(sectionName, tenantId);
            if (serializedSection == null && tenantId != Tenant.DEFAULT_TENANT)
            {
                serializedSection = GetSetting(sectionName, Tenant.DEFAULT_TENANT);
            }
            return serializedSection != null ? JsonConvert.DeserializeObject<T>(serializedSection) : null;
        }

        public void SaveSection<T>(string sectionName, T section) where T : class
        {
            SaveSection(CoreContext.TenantManager.GetCurrentTenant().TenantId, sectionName, section);
        }

        public void SaveSection<T>(T section) where T : class
        {
            SaveSection(typeof(T).Name, section);
        }

        public void SaveSection<T>(int tenantId, T section) where T : class
        {
            SaveSection(tenantId, typeof(T).Name, section);
        }

        public void SaveSection<T>(int tenantId, string sectionName, T section) where T : class
        {
            var serializedSection = section != null ? JsonConvert.SerializeObject(section) : null;
            SaveSetting(sectionName, serializedSection, tenantId);
        }

        #endregion
    }
}