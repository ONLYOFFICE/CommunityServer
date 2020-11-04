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
