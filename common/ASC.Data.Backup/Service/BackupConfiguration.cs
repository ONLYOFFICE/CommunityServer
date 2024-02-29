/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.IO;
using System.Linq;

namespace ASC.Data.Backup.Service
{
    public class BackupConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("upgradesPath", DefaultValue = "../../Sql")]
        public string UpgradesPath
        {
            get
            {
                return (string)this["upgradesPath"];
            }
            set { this["upgradesPath"] = value; }
        }

        [ConfigurationProperty("limit", DefaultValue = 100000)]
        public int Limit
        {
            get
            {
                return Convert.ToInt32(this["limit"]);
            }
            set { this["limit"] = value; }
        }

        [ConfigurationProperty("service")]
        public ServiceConfigurationElement Service
        {
            get { return (ServiceConfigurationElement)this["service"]; }
            set { this["service"] = value; }
        }

        [ConfigurationProperty("scheduler")]
        public SchedulerConfigurationElement Scheduler
        {
            get { return (SchedulerConfigurationElement)this["scheduler"]; }
            set { this["scheduler"] = value; }
        }

        [ConfigurationProperty("cleaner")]
        public CleanerConfigurationElement Cleaner
        {
            get { return (CleanerConfigurationElement)this["cleaner"]; }
            set { this["cleaner"] = value; }
        }

        [ConfigurationProperty("webConfigs")]
        public WebConfigCollection WebConfigs
        {
            get { return (WebConfigCollection)this["webConfigs"]; }
            set { this["webConfigs"] = value; }
        }

        [ConfigurationProperty("mailServer")]
        public MailServerConfigurationElement MailServer
        {
            get { return (MailServerConfigurationElement)this["mailServer"]; }
            set { this["mailServer"] = value; }
        }

        public static BackupConfigurationSection GetSection()
        {
            return (BackupConfigurationSection)ConfigurationManagerExtension.GetSection("backup");
        }
    }

    public class SchedulerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("period", DefaultValue = "0:15:0")]
        public TimeSpan Period
        {
            get { return (TimeSpan)this["period"]; }
            set { this["period"] = value; }
        }

        [ConfigurationProperty("workerCount", DefaultValue = 2)]
        public int WorkerCount
        {
            get { return (int)this["workerCount"]; }
            set { this["workerCount"] = value; }
        }
    }

    public class CleanerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("period", DefaultValue = "0:15:0")]
        public TimeSpan Period
        {
            get { return (TimeSpan)this["period"]; }
            set { this["period"] = value; }
        }
    }

    public class ServiceConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("workerCount", DefaultValue = 4)]
        public int WorkerCount
        {
            get { return (int)this["workerCount"]; }
            set { this["workerCount"] = value; }
        }
    }

    public class MailServerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("pathToMailFilesOnHost", DefaultValue = "")]
        public string PathToMailFilesOnHost
        {
            get { return (string)this["pathToMailFilesOnHost"]; }
            set { this["pathToMailFilesOnHost"] = value; }
        }

        [ConfigurationProperty("hostAddress", DefaultValue = "")]
        public string HostAddress
        {
            get { return (string)this["hostAddress"]; }
            set { this["hostAddress"] = value; }
        }

        [ConfigurationProperty("hostUsername", DefaultValue = "")]
        public string HostUsername
        {
            get { return (string)this["hostUsername"]; }
            set { this["hostUsername"] = value; }
        }

        [ConfigurationProperty("hostPassword", DefaultValue = "")]
        public string HostPassword
        {
            get { return (string)this["hostPassword"]; }
            set { this["hostPassword"] = value; }
        }

        [ConfigurationProperty("hostKey", DefaultValue = "")]
        public string HostKey
        {
            get { return (string)this["hostKey"]; }
            set { this["hostKey"] = value; }
        }
    }

    public class WebConfigCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("current", DefaultValue = "localhost")]
        public string CurrentRegion
        {
            get { return (string)this["current"]; }
            set { this["current"] = value; }
        }

        public string CurrentPath
        {
            get
            {
                if (Count == 0)
                {
                    return Path.Combine("..", "..", "WebStudio");
                }
                if (Count == 1)
                {
                    return this.Cast<WebConfigElement>().First().Path;
                }
                return GetPath(CurrentRegion);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WebConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebConfigElement)element).Region;
        }

        public string GetPath(string region)
        {
            var element = BaseGet(region) as WebConfigElement;
            return element != null ? element.Path : null;
        }
    }

    public class WebConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("region", IsRequired = true, IsKey = true)]
        public string Region
        {
            get { return (string)this["region"]; }
        }

        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
        }
    }
}
