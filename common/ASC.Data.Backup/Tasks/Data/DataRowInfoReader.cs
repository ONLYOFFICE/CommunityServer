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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ASC.Data.Backup.Extensions;

namespace ASC.Data.Backup.Tasks.Data
{
    internal static class DataRowInfoReader
    {
        private const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

        public static IEnumerable<DataRowInfo> ReadFromStream(Stream stream)
        {
            var readerSettings = new XmlReaderSettings
                {
                    CheckCharacters = false,
                    CloseInput = false
                };

            using (var xmlReader = XmlReader.Create(stream, readerSettings))
            {
                xmlReader.MoveToContent();
                xmlReader.ReadToFollowing("schema", XmlSchemaNamespace);

                var schema = new Dictionary<string, string>();

                var schemaElement = XNode.ReadFrom(xmlReader) as XElement;
                if (schemaElement != null)
                {
                    foreach (var entry in schemaElement.Descendants(XName.Get("sequence", XmlSchemaNamespace)).Single().Elements(XName.Get("element", XmlSchemaNamespace)))
                    {
                        schema.Add(entry.Attribute("name").ValueOrDefault(), entry.Attribute("type").ValueOrDefault());
                    }
                }

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        var el = XNode.ReadFrom(xmlReader) as XElement;
                        if (el != null)
                        {
                            var dataRowInfo = new DataRowInfo(el.Name.LocalName);
                            foreach (var column in schema)
                            {
                                object value = ConvertToType(el.Element(column.Key).ValueOrDefault(), column.Value);
                                dataRowInfo.SetValue(column.Key, value);
                            }

                            yield return dataRowInfo;
                        }
                    }
                }
            }
        }

        private static object ConvertToType(string str, string schemaType)
        {
            if (str == null)
            {
                return null;
            }
            if (schemaType == "xs:boolean")
            {
                return Convert.ToBoolean(str);
            }
            if (schemaType == "xs:base64Binary")
            {
                return Convert.FromBase64String(str);
            }
            return str;
        }
    }
}
