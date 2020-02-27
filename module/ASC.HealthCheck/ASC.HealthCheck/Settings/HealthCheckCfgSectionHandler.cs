/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ASC.HealthCheck.Models;

namespace ASC.HealthCheck.Settings
{
    class HealthCheckCfgSectionHandler : ConfigurationSection
    {
        private static HealthCheckCfgSectionHandler instance;
        public static HealthCheckCfgSectionHandler Instance
        {
            get
            {
                return instance ?? (instance = (HealthCheckCfgSectionHandler)ConfigurationManager.GetSection("healthCheck"));
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public KeyValueConfigurationCollection Settings
        {
            get
            {
                return (KeyValueConfigurationCollection)this[new ConfigurationProperty("", typeof(KeyValueConfigurationCollection), null, ConfigurationPropertyOptions.IsDefaultCollection)];
            }
        }

        private string GetValue(string key, string defaultValue = "")
        {
            return Settings.AllKeys.Contains(key) ? Settings[key].Value : defaultValue;
        }

        public TimeSpan MainLoopPeriod
        {
            get { return TimeSpan.FromSeconds(Convert.ToInt32(GetValue("mainLoopPeriod", "60"))); }
        }

        public List<ServiceEnum> ServiceNames
        {
            get
            {
                var configValue = GetValue("service-names").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return configValue.Select(r =>(ServiceEnum)Enum.Parse(typeof(ServiceEnum), r, true)).ToList();
            }
        }

        public int DriveSpaceThreashold
        {
            get { return Convert.ToInt32(GetValue("drive-space-threashold", "104857600")); }
        }

        public string LogFolders
        {
            get { return GetValue("log-folders", "/var/log/onlyoffice/"); }
        }

        public string SMSUSAregex
        {
            get { return GetValue("sms-USAregex"); }
        }

        public string SMSCISregex
        {
            get { return GetValue("sms-CISregex"); }
        }

        public string SupportEmails
        {
            get { return GetValue("support-emails"); }
        }

        public string Url
        {
            get { return GetValue("url", "http://localhost:9810/"); }
        }

        public string PortalsWebConfigPath
        {
            get { return GetValue("portals-web-config-path", "/var/www/onlyoffice/WebStudio{0}/Web.config"); }
        }

        public int WebInstanceCount
        {
            get { return Convert.ToInt32(GetValue("portals-web-count", "2")); }
        }
    }
}
