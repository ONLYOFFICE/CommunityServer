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

namespace ASC.Mail.Autoreply.Utility.Html
{
    public class Text2HtmlConverter
    {
        public static String Convert(String text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            text = text.Replace("  ", " &nbsp;");

            var sr = new StringReader(text);
            var sw = new StringWriter();

            while (sr.Peek() > -1)
            {
                sw.Write(sr.ReadLine() + "<br>");
            }

            return sw.ToString();
        }
    }
}
