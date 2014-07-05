/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.Core.Configuration;
using ASC.Core.Tenants;
using System;
using System.Configuration;
using System.Text;

namespace ASC.Core
{
    class ClientConfiguration : IConfigurationClient
    {
        private readonly ITenantService tenantService;

        public bool Standalone
        {
            get { return ConfigurationManager.AppSettings["core.base-domain"] == "localhost"; }
        }

        public bool Personal
        {
            get { return ConfigurationManager.AppSettings["core.personal"] == "true"; }
        }

        public bool PartnerHosted
        {
            get { return ConfigurationManager.AppSettings["core.payment-partners-hosted"] == "true"; }
        }

        public SmtpSettings SmtpSettings
        {
            get
            {
                bool isDefaultSettings = false;
                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);

                if (tenant != null)
                {

                    string settingsValue = GetSetting(tenant.TenantId, "SmtpSettings");
                    if (string.IsNullOrEmpty(settingsValue))
                    {
                        isDefaultSettings = true;
                        settingsValue = GetSetting(Tenant.DEFAULT_TENANT, "SmtpSettings");
                    }
                    var settings = SmtpSettings.Deserialize(settingsValue);
                    settings.IsDefaultSettings = isDefaultSettings;
                    return settings;
                }
                else
                {
                    string settingsValue = GetSetting(Tenant.DEFAULT_TENANT, "SmtpSettings");

                    var settings = SmtpSettings.Deserialize(settingsValue);
                    settings.IsDefaultSettings = true;
                    return settings;
                }
            }
            set { SaveSetting(CoreContext.TenantManager.GetCurrentTenant().TenantId, "SmtpSettings", value != null ? value.Serialize() : null); }
        }

        public string SKey
        {
            get
            {
                return GetSetting("DocKey") ?? ConfigurationManager.AppSettings["files.docservice.key"];
            }
            set
            {
                if (Standalone)
                {
                    SaveSetting("DocKey", value);
                }
            }
        }


        public ClientConfiguration(ITenantService service)
        {
            tenantService = service;
        }


        public void SaveSetting(string key, string value)
        {
            SaveSetting(Tenant.DEFAULT_TENANT, key, value);
        }

        private void SaveSetting(int tenant, string key, string value)
        {
            if (string.IsNullOrEmpty("key"))
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

        public string GetSetting(string key)
        {
            return GetSetting(Tenant.DEFAULT_TENANT, key);
        }

        private string GetSetting(int tenant, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            byte[] bytes = tenantService.GetTenantSettings(tenant, key);
            return bytes != null ? Encoding.UTF8.GetString(Crypto.GetV(bytes, 2, false)) : null;
        }

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

                var prefix = string.Empty;
                if (t != null && !string.IsNullOrEmpty(t.PartnerId) &&
                    PartnerHosted)
                {
                    prefix = t.PartnerId + "h";
                }
                return prefix + ConfigurationManager.AppSettings["core.payment-region"] + tenant;
            }
        }
    }
}