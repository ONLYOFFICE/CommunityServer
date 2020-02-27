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