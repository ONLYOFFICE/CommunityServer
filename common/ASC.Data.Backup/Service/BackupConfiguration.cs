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

using System;
using System.Configuration;
using System.Linq;

namespace ASC.Data.Backup.Service
{
    public class BackupConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("tmpFolder", DefaultValue = "..\\Data\\Backup\\")]
        public string TempFolder
        {
            get { return (string)this["tmpFolder"]; }
            set { this["tmpFolder"] = value; }
        }

        [ConfigurationProperty("expire", DefaultValue = "1.0:0:0")]
        public TimeSpan ExpirePeriod
        {
            get { return (TimeSpan)this["expire"]; }
            set { this["expire"] = value; }
        }

        [ConfigurationProperty("threads", DefaultValue = 4)]
        public int ThreadCount
        {
            get { return (int)this["threads"]; }
            set { this["threads"] = value; }
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

    public class WebConfigCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("current")]
        public string CurrentRegion
        {
            get { return (string)this["current"]; }
            set { this["current"] = value; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WebConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebConfigElement)element).Region;
        }

        public string GetPathForRegion(string region)
        {
            var element = BaseGet(region) as WebConfigElement;
            return element != null ? element.Path : null;
        }

        public string GetCurrentConfig()
        {
            if (Count == 0)
            {
                return "Backup";
            }
            if (Count == 1)
            {
                return this.Cast<WebConfigElement>().First().Path;
            }
            return GetPathForRegion(CurrentRegion);
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
