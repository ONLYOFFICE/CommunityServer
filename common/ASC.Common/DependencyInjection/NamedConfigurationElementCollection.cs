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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace ASC.Common.DependencyInjection
{
    public class NamedConfigurationElementCollection<TElementType> : ConfigurationElementCollection, IEnumerable<TElementType>, IEnumerable where TElementType : ConfigurationElement
    {
        private readonly string elementName;
        private readonly string elementKey;

        protected override string ElementName { get { return elementName; } }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public TElementType this[int index]
        {
            get
            {
                return BaseGet(index) as TElementType;
            }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }


        protected NamedConfigurationElementCollection(string eName, string eKey)
        {
            if (eName == null)
                throw new ArgumentNullException("eName");
            if (eName.Length == 0)
                throw new ArgumentOutOfRangeException(eName);
            if (eKey == null)
                throw new ArgumentNullException("eKey");
            if (eKey.Length == 0)
                throw new ArgumentOutOfRangeException(eKey);
            elementName = eName;
            elementKey = eKey;
        }

        protected override bool IsElementName(string eName)
        {
            if (!string.IsNullOrEmpty(eName))
                return eName == elementName;
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return Activator.CreateInstance<TElementType>();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null || element.ElementInformation.Properties[elementKey] == null)
                throw new ArgumentNullException("element");
            return (string)element.ElementInformation.Properties[elementKey].Value;
        }

        public new IEnumerator<TElementType> GetEnumerator()
        {
            foreach (TElementType elementType in (IEnumerable)this)
                yield return elementType;
        }
    }
}
