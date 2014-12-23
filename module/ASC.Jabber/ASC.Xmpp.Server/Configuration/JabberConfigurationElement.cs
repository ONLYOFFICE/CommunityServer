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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace ASC.Xmpp.Server.Configuration
{
    public class JabberConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty(Schema.NAME, IsKey = true, IsRequired = true)]
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

		[ConfigurationProperty(Schema.PROPERTIES, IsDefaultCollection = false)]
		public NameValueConfigurationCollection JabberProperties
		{
			get { return (NameValueConfigurationCollection)this[Schema.PROPERTIES]; }
			set { this[Schema.PROPERTIES] = value; }
		}


		public JabberConfigurationElement()
		{

		}

		public JabberConfigurationElement(string name, Type type)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (type == null) throw new ArgumentNullException("type");

			Name = name;
			TypeName = type.FullName;
		}

		public IDictionary<string, string> GetProperties()
		{
			var properties = new Dictionary<string, string>();
			foreach (NameValueConfigurationElement nameValuePair in JabberProperties)
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
                    if (reader.LocalName == "name")
                    {
                        name = reader.Value;
                    }
                    if (reader.LocalName == "value")
                    {
                        value = reader.Value;
                    }
                }
                JabberProperties.Add(new NameValueConfigurationElement(name, value));
                return true;
			}
			return base.OnDeserializeUnrecognizedElement(elementName, reader);
		}
	}
}