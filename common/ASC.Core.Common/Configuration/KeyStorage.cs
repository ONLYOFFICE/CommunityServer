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
using System.Configuration;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Thrdparty.Configuration
{
    public static class KeyStorage
    {
        private static bool? _onlyDefault;

        public static string Get(string name, bool forDefault = false)
        {
            string value = null;

            if (!(_onlyDefault ?? (bool)(_onlyDefault = ConfigurationManager.AppSettings["core.default-consumers"] == "true"))
                && CanSet(name))
            {
                var tenant = CoreContext.Configuration.Standalone || forDefault
                                 ? Tenant.DEFAULT_TENANT
                                 : CoreContext.TenantManager.GetCurrentTenant().TenantId;

                value = CoreContext.Configuration.GetSetting(GetSettingsKey(name), tenant);
            }

            if (string.IsNullOrEmpty(value))
            {
                var section = ConsumerConfigurationSection.GetSection();
                if (section != null)
                {
                    value = section.Keys.GetKeyValue(name);
                }
            }
            return value;
        }

        public static bool CanSet(string name)
        {
            var value = string.Empty;
            var section = ConsumerConfigurationSection.GetSection();
            if (section != null)
            {
                var r = section.Keys.GetKey(name);
                if (r == null || string.IsNullOrEmpty(r.ConsumerName)) return false;
                value = section.Keys.GetKeyValue(name);
            }

            return string.IsNullOrEmpty(value);
        }

        public static void Set(string name, string value)
        {
            if (!CanSet(name))
            {
                throw new NotSupportedException("Key for read only.");
            }

            var tenant = CoreContext.Configuration.Standalone
                             ? Tenant.DEFAULT_TENANT
                             : CoreContext.TenantManager.GetCurrentTenant().TenantId;
            CoreContext.Configuration.SaveSetting(GetSettingsKey(name), value, tenant);
        }

        private static string GetSettingsKey(string name)
        {
            return "AuthKey_" + name;
        }
    }
}