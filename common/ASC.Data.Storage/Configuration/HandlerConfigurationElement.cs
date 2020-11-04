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
using System.Xml;

namespace ASC.Data.Storage.Configuration
{
    public class HandlerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this[Schema.NAME]; }
            set { this[Schema.NAME] = value; }
        }

        [ConfigurationProperty(Schema.TYPE, IsRequired = true)]
        public string TypeName
        {
            get { return (string)this[Schema.TYPE]; }
            set { this[Schema.TYPE] = value; }
        }

        [ConfigurationProperty(Schema.PROPERTIES)]
        public NameValueConfigurationCollection HandlerProperties
        {
            get { return (NameValueConfigurationCollection)this[Schema.PROPERTIES]; }
            set { this[Schema.PROPERTIES] = value; }
        }

        public Type Type
        {
            get { return Type.GetType(TypeName, true); }
        }

        public IDictionary<string, string> GetProperties()
        {
            var properties = new Dictionary<string, string>();
            foreach (NameValueConfigurationElement nameValuePair in HandlerProperties)
            {
                properties.Add(nameValuePair.Name, nameValuePair.Value);
            }
            return properties;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            if (elementName == Schema.PROPERTY)
            {
                var name = string.Empty;
                var value = string.Empty;
                while (reader.MoveToNextAttribute())
                {
                    if (reader.LocalName == Schema.NAME)
                    {
                        name = reader.Value;
                    }
                    else if (reader.LocalName == Schema.VALUE)
                    {
                        value = reader.Value;
                    }
                }
                HandlerProperties.Add(new NameValueConfigurationElement(name, value));
                return true;
            }
            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }
    }
}