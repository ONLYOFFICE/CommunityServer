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
using System.Text.RegularExpressions;

namespace ASC.Web.Studio.Utility.BBCodeParser
{
    public class RegularExpressionTemplate
    {
        public Regex RegExpression { get; private set; }
        public string Replacement { get; private set; }

        public RegularExpressionTemplate(Regex regExpression, string replacement)
        {
            RegExpression = regExpression;
            Replacement = replacement;
        }

        public string Parse(string text)
        {
            const int start = 0;
            var m = RegExpression.Match(text, start);
            while (m.Success)
            {
                text = text.Remove(m.Index, m.Length);
                var insertion = String.Format(Replacement, m.Value);
                text = text.Insert(m.Index, insertion);
                m = m.NextMatch();
            }

            return text;
        }

        public static RegularExpressionTemplate HTMLReferenceExpression
        {
            get
            {
                return new RegularExpressionTemplate(
                    new Regex("((http|ftp|https|gopher|mailto|news|nntp|telnet)://){1}([0-9a-zA-Z]+[0-9a-zA-Z]+[0-9a-zA-Z\\-_]*\\.{0,1}[0-9a-zA-Z]+[0-9a-zA-Z/\\.{0,1}\\-_:]*){1}(\\?[0-9a-zA-Z;/?@&=+$\\.\\-_!~*'#()%]*)?",
                              RegexOptions.IgnoreCase | RegexOptions.Compiled),
                    "<a href=\"{0}\" target=_blank>{0}</a>");
            }
        }
    }
}