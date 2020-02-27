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
using System.Configuration;

namespace ASC.Web.UserControls.Wiki.Handlers
{
    public class WikiSection : ConfigurationSection
    {
        private static readonly WikiSection defaultSection = new WikiSection();


        public static WikiSection Section
        {
            get
            {
                return (ConfigurationManager.GetSection("wikiSettings") as WikiSection ?? defaultSection);
            }
        }

        [ConfigurationProperty("dbase")]
        public DBElement DB
        {
            get
            {
                return (DBElement)this["dbase"];
            }
            set
            {
                this["dbase"] = value;
            }
        }

        [ConfigurationProperty("fckeditorInfo")]
        public FckeditorInfoElement FckeditorInfo
        {
            get
            {
                return (FckeditorInfoElement)this["fckeditorInfo"];
            }
            set
            {
                this["fckeditorInfo"] = value;
            }
        }

        [ConfigurationProperty("dataStorage")]
        public DataStorageElement DataStorage
        {
            get
            {
                return (DataStorageElement)this["dataStorage"];
            }
            set
            {
                this["dataStorage"] = value;
            }
        }

        [ConfigurationProperty("imageHangler")]
        public ImageHanglerElement ImageHangler
        {
            get
            {
                return (ImageHanglerElement)this["imageHangler"];
            }
            set
            {
                this["imageHangler"] = value;
            }
        }

    }

    public class MainPageElement : ConfigurationElement
    {
        [ConfigurationProperty("url", DefaultValue = "", IsRequired = true)]
        public string Url
        {
            get
            {
                return (String)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        [ConfigurationProperty("wikiView", DefaultValue = "", IsRequired = true)]
        public string WikiView
        {
            get
            {
                return (string)this["wikiView"];
            }
            set
            {
                this["wikiView"] = value;
            }
        }

        [ConfigurationProperty("wikiEdit", DefaultValue = "", IsRequired = true)]
        public string WikiEdit
        {
            get
            {
                return (string)this["wikiEdit"];
            }
            set
            {
                this["wikiEdit"] = value;
            }
        }
    }

    public class DBElement : ConfigurationElement
    {
        [ConfigurationProperty("connectionStringName", DefaultValue = "community", IsRequired = true)]
        public string ConnectionStringName
        {
            get
            {
                return (string)this["connectionStringName"];
            }
            set
            {
                this["connectionStringName"] = value;
            }
        }
    }

    public class FileStorageElement : ConfigurationElement
    {
        [ConfigurationProperty("location", DefaultValue = "", IsRequired = true)]
        public string Location
        {
            get
            {
                return (string)this["location"];
            }
            set
            {
                this["location"] = value;
            }
        }
    }

    public class DataStorageElement : ConfigurationElement
    {
        [ConfigurationProperty("moduleName", DefaultValue = "wiki", IsRequired = true)]
        public string ModuleName
        {
            get
            {
                return (string)this["moduleName"];
            }
            set
            {
                this["moduleName"] = value;
            }
        }

        [ConfigurationProperty("defaultDomain", DefaultValue = "", IsRequired = true)]
        public string DefaultDomain
        {
            get
            {
                return (string)this["defaultDomain"];
            }
            set
            {
                this["defaultDomain"] = value;
            }
        }

        [ConfigurationProperty("tempDomain", DefaultValue = "temp", IsRequired = true)]
        public string TempDomain
        {
            get
            {
                return (string)this["tempDomain"];
            }
            set
            {
                this["tempDomain"] = value;
            }
        }
    }

    public class ImageHanglerElement : ConfigurationElement
    {
        [ConfigurationProperty("urlFormat", DefaultValue = "~/Products/Community/Modules/Wiki/WikiFile.ashx?file={0}", IsRequired = true)]
        public string UrlFormat
        {
            get
            {
                return (string)this["urlFormat"];
            }
            set
            {
                this["urlFormat"] = value;
            }
        }
    }

    public class FckeditorInfoElement : ConfigurationElement
    {
        [ConfigurationProperty("pathFrom", DefaultValue = "../../../../../Products/Community/Modules/Wiki/WikiUC/", IsRequired = true)]
        public string PathFrom
        {
            get
            {
                return (string)this["pathFrom"];
            }
            set
            {
                this["pathFrom"] = value;
            }
        }

        [ConfigurationProperty("baseRelPath", DefaultValue = "~/Products/Community/Modules/Wiki/WikiUC/fckeditor/", IsRequired = true)]
        public string BaseRelPath
        {
            get
            {
                return (string)this["baseRelPath"];
            }
            set
            {
                this["baseRelPath"] = value;
            }
        }

    }



}
