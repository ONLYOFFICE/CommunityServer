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
