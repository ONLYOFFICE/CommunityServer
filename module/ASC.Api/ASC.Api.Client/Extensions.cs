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
using System.IO;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace ASC.Api.Client
{
    public static class XmlExtensions
    {
        public static bool ReadToNextElement(this XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            bool readed;
            while ((readed = xmlReader.Read()) && xmlReader.NodeType != XmlNodeType.Element)
            {
            }

            return readed;
        }
    }

    public static class JsonExtensions
    {
        public static string GetInnerJson(this JsonReader jsonReader)
        {
            if (jsonReader == null)
                throw new ArgumentNullException("jsonReader");

            if (jsonReader.TokenType == JsonToken.PropertyName)
                jsonReader.Read();

            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.DateFormatString = "o";
                jsonWriter.WriteToken(jsonReader);
            }

            return sb.ToString();
        }
    }

    public static class StreamExtensions
    {
        public static void WriteString(this Stream stream, string format, params object[] args)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            byte[] bytes = Encoding.UTF8.GetBytes(string.Format(format, args));
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
