/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
