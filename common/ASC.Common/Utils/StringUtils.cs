/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Linq;
using System.Text;
using System.Xml;

namespace ASC.Common.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// Removes control characters and other non-UTF-8 characters
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <return>A string with no control characters or entities above 0x00FD</return>
        public static string NormalizeStringForMySql(string inString)
        {
            if (string.IsNullOrEmpty(inString))
                return inString;

            var newString = new StringBuilder(inString.Length);

            foreach (var ch in inString.Where(XmlConvert.IsXmlChar))
                newString.Append(ch);

            return newString.ToString();
        }
    }
}
