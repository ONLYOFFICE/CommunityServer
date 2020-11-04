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

namespace ASC.Web.UserControls.Wiki.Handlers
{
    public class WikiSection : ConfigurationSection
    {
        private static readonly WikiSection defaultSection = new WikiSection();


        public static WikiSection Section
        {
            get
            {
                return (ConfigurationManagerExtension.GetSection("wikiSettings") as WikiSection ?? defaultSection);
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
