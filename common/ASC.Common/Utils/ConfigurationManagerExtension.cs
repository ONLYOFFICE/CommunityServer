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


using System.Collections.Concurrent;
using System.Collections.Specialized;

namespace System.Configuration
{
    //mono:hack
    public static class ConfigurationManagerExtension
    {
        private static ConcurrentDictionary<string, object> Sections { get; set; }

        static ConfigurationManagerExtension()
        {
            Sections = new ConcurrentDictionary<string, object>();
        }

        public static NameValueCollection AppSettings
        {
            get
            {
                var section = GetSection("appSettings");
                if (section == null || !(section is NameValueCollection))
                {
                    throw new ConfigurationErrorsException("config exception");
                }

                return (NameValueCollection)section;
            }
        }

        public static object GetSection(string sectionName)
        {
            return Sections.GetOrAdd(sectionName, ConfigurationManager.GetSection(sectionName));
        }

        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                var section = GetSection("connectionStrings");
                if (section == null || section.GetType() != typeof(ConnectionStringsSection))
                {
                    throw new ConfigurationErrorsException("Config_connectionstrings_declaration_invalid");
                }

                var connectionStringsSection = (ConnectionStringsSection)section;
                return connectionStringsSection.ConnectionStrings;
            }
        }
    }
}
