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
using System.IO;
using System.Linq;

namespace ASC.Data.Backup.Service
{
    public class BackupConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("tmpFolder", DefaultValue = "..\\Data\\Backup\\")]
        public string TempFolder
        {
            get
            {
                var path = (string)this["tmpFolder"];
                return string.IsNullOrEmpty(path) ? path : path.Replace('\\', Path.DirectorySeparatorChar);
            }
            set { this["tmpFolder"] = value; }
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

        public static BackupConfigurationSection GetSection()
        {
            return (BackupConfigurationSection)ConfigurationManager.GetSection("backup");
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

        [ConfigurationProperty("workerCount", DefaultValue = 1)]
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
