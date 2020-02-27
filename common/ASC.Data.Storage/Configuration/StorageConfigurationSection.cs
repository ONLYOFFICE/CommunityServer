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
using System.IO;
using System.Linq;
using System.Xml;

namespace ASC.Data.Storage.Configuration
{
    public class StorageConfigurationSection : ConfigurationSection
    {
        private string sourceFile;


        [ConfigurationProperty(Schema.FILE_PATH)]
        public string ConfigFile
        {
            get { return (string)this[Schema.FILE_PATH]; }
            set { this[Schema.FILE_PATH] = value; }
        }

        [ConfigurationProperty(Schema.MODULES)]
        public ModuleConfigurationCollection Modules
        {
            get { return (ModuleConfigurationCollection)base[Schema.MODULES]; }
        }

        [ConfigurationProperty(Schema.HANDLERS)]
        public HandlersConfigurationCollection Handlers
        {
            get { return (HandlersConfigurationCollection)base[Schema.HANDLERS]; }
        }

        [ConfigurationProperty(Schema.APPENDERS)]
        public AppenderConfigurationCollection Appenders
        {
            get { return (AppenderConfigurationCollection)base[Schema.APPENDERS]; }
        }


        public void SetSourceFile(string sourceFile)
        {
            this.sourceFile = sourceFile;
            foreach (HandlerConfigurationElement h in Handlers)
            {
                h.HandlerProperties.Remove(Constants.CONFIG_DIR);
                h.HandlerProperties.Add(new NameValueConfigurationElement(Constants.CONFIG_DIR, Path.GetDirectoryName(sourceFile)));
            }
        }


        protected override void DeserializeSection(XmlReader reader)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                sourceFile = ElementInformation.Source;
            }
            if (string.IsNullOrEmpty(sourceFile))
            {
                sourceFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }

            base.DeserializeSection(reader);

            if (!string.IsNullOrWhiteSpace(ConfigFile))
            {
                sourceFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile), ConfigFile));
                if (File.Exists(sourceFile))
                {
                    using (var xmlReader = XmlReader.Create(sourceFile))
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.LocalName == Schema.SECTION_NAME)
                            {
                                base.DeserializeElement(xmlReader, false);
                            }
                        }
                    }
                }
            }

            SetSourceFile(sourceFile);
        }

        protected override void PostDeserialize()
        {
            // mono hack:
            //   PostDeserialize not execute in Microsoft CLR
            //   PostDeserialize execute in mono
            //   Handlers property available at DeserializeSection in Microsoft CLR
            //   Handlers property not available at DeserializeSection in mono
            SetSourceFile(sourceFile);
        }
    }
}