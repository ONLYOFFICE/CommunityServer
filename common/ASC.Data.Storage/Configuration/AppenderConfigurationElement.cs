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


using System.Configuration;

namespace ASC.Data.Storage.Configuration
{
    public class AppenderConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this[Schema.NAME]; }
            set { this[Schema.NAME] = value; }
        }

        [ConfigurationProperty(Schema.APPEND, IsRequired = true)]
        public string Append
        {
            get { return (string) this[Schema.APPEND]; }
            set { this[Schema.APPEND] = value; }
        }

        [ConfigurationProperty(Schema.APPENDSECURE)]
        public string AppendSecure
        {
            get { return (string) this[Schema.APPENDSECURE]; }
            set { this[Schema.APPENDSECURE] = value; }
        }

        [ConfigurationProperty(Schema.EXTs)]
        public string Extensions
        {
            get { return (string)this[Schema.EXTs]; }
            set { this[Schema.EXTs] = value; }
        }
    }
}