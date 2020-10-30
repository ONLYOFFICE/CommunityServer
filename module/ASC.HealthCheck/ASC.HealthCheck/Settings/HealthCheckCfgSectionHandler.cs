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
