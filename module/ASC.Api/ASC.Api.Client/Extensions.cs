/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
