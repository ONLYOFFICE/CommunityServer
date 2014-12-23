/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System.Configuration;

namespace ASC.Thrdparty.Configuration
{
    public class ConsumerConfigurationSection : ConfigurationSection
    {
        public const string SectionName = "consumers";

        [ConfigurationProperty("keys")]
        public KeyElementCollection Keys
        {
            get { return (KeyElementCollection)base["keys"]; }
            set { base["keys"] = value; }
        }

        [ConfigurationProperty("connectionstring")]
        public string ConnectionString
        {
            get { return (string)base["connectionstring"]; }
            set { base["connectionstring"] = value; }
        }

        public static ConsumerConfigurationSection GetSection()
        {
            return (ConsumerConfigurationSection)ConfigurationManager.GetSection(SectionName);
        }
    }

    public class KeyElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new KeyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((KeyElement)element).Name;
        }

        public KeyElement GetKey(string name)
        {
            return BaseGet(name) as KeyElement;
        }

        public string GetKeyValue(string name)
        {
            var keyElement = GetKey(name);
            return keyElement != null ? keyElement.Value : string.Empty;
        }
    }

    public class KeyElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("value", DefaultValue = "")]
        public string Value
        {
            get { return (string)base["value"]; }
            set { base["value"] = value; }
        }

        [ConfigurationProperty("consumer")]
        public string ConsumerName
        {
            get { return (string)base["consumer"]; }
            set { base["consumer"] = value; }
        }

        [ConfigurationProperty("type", DefaultValue = KeyType.Default)]
        public KeyType Type
        {
            get { return (KeyType)base["type"]; }
            set { base["type"] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public enum KeyType
        {
            Default,
            Key,
            Secret
        }
    }
}