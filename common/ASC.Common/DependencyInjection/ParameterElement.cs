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


namespace ASC.Common.DependencyInjection
{
    public class ParameterElement : ConfigurationElement
    {
        private const string NameAttributeName = "name";
        private const string ValueAttributeName = "value";
        private const string TypeAttributeName = "type";
        private const string ListElementName = "list";
        private const string DictionaryElementName = "dictionary";
        internal const string Key = "name";

        [ConfigurationProperty(NameAttributeName, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this[NameAttributeName];
            }
        }

        [ConfigurationProperty(ValueAttributeName, IsRequired = false)]
        public string Value
        {
            get
            {
                return (string)this[ValueAttributeName];
            }
        }

        [ConfigurationProperty(TypeAttributeName, IsRequired = false)]
        public string Type
        {
            get
            {
                return (string)this[TypeAttributeName];
            }
        }

        [ConfigurationProperty(ListElementName, DefaultValue = null, IsRequired = false)]
        public ListElementCollection List
        {
            get
            {
                return this[ListElementName] as ListElementCollection;
            }
        }

        [ConfigurationProperty(DictionaryElementName, DefaultValue = null, IsRequired = false)]
        public DictionaryElementCollection Dictionary
        {
            get
            {
                return this[DictionaryElementName] as DictionaryElementCollection;
            }
        }

        public object CoerceValue()
        {
            if (List.ElementInformation.IsPresent)
                return List;
            if (Dictionary.ElementInformation.IsPresent)
                return Dictionary;
            return Value;
        }
    }
}
