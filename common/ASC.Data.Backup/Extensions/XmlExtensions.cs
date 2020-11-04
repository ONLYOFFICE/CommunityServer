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


using System.IO;
using System.Text;
using System.Xml.Linq;

namespace ASC.Data.Backup.Extensions
{
    public static class XmlExtensions
    {
        public static string ValueOrDefault(this XElement el)
        {
            return el != null ? el.Value : null;
        }

        public static string ValueOrDefault(this XAttribute attr)
        {
            return attr != null ? attr.Value : null;
        }

        public static void WriteTo(this XElement el, Stream stream)
        {
            WriteTo(el, stream, Encoding.UTF8);
        }

        public static void WriteTo(this XElement el, Stream stream, Encoding encoding)
        {
            var data = encoding.GetBytes(el.ToString(SaveOptions.None));
            stream.Write(data, 0, data.Length);
        }
    }
}
