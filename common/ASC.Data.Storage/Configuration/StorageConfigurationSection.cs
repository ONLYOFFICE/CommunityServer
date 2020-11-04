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