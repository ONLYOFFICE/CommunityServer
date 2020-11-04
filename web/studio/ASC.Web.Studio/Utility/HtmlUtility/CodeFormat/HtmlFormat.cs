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


#region Copyright � 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */ 
#endregion

using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ASC.Web.Studio.Utility.HtmlUtility.CodeFormat
{
    /// <summary>
    /// Generates color-coded HTML 4.01 from HTML/XML/ASPX source code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation assumes that code inside &lt;script&gt; blocks 
    /// is JavaScript, and code inside &lt;% %&gt; blocks is C#.</para>
    /// <para>
    /// The default tab width is set to 2 characters in this class.</para>
    /// </remarks>
    internal class HtmlFormat : SourceFormat
    {
        private readonly CSharpFormat _csf; //to format embedded C# code
        private readonly JavaScriptFormat _jsf; //to format client-side JavaScript code
        private readonly Regex _attribRegex;

        /// <summary/>
        public HtmlFormat()
        {
            const string regJavaScript = @"(?<=&lt;script(?:\s.*?)?&gt;).+?(?=&lt;/script&gt;)";
            const string regComment = @"&lt;!--.*?--&gt;";
            const string regAspTag = @"&lt;%@.*?%&gt;|&lt;%|%&gt;";
            const string regAspCode = @"(?<=&lt;%).*?(?=%&gt;)";
            const string regTagDelimiter = @"(?:&lt;/?!?\??(?!%)|(?<!%)/?&gt;)+";
            const string regTagName = @"(?<=&lt;/?!?\??(?!%))[\w\.:-]+(?=.*&gt;)";
            const string regAttributes = @"(?<=&lt;(?!%)/?!?\??[\w:-]+).*?(?=(?<!%)/?&gt;)";
            const string regEntity = @"&amp;\w+;";
            const string regAttributeMatch = @"(=?"".*?""|=?'.*?')|([\w:-]+)";

            //the regex object will handle all the replacements in one pass
            const string regAll = "(" + regJavaScript + ")|(" + regComment + ")|("
                                  + regAspTag + ")|(" + regAspCode + ")|("
                                  + regTagDelimiter + ")|(" + regTagName + ")|("
                                  + regAttributes + ")|(" + regEntity + ")";

            CodeRegex = new Regex(regAll, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            _attribRegex = new Regex(regAttributeMatch, RegexOptions.Singleline);

            _csf = new CSharpFormat();
            _jsf = new JavaScriptFormat();
        }

        /// <summary>
        /// Called to evaluate the HTML fragment corresponding to each 
        /// attribute's name/value in the code.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> resulting from a 
        /// single regular expression match.</param>
        /// <returns>A string containing the HTML code fragment.</returns>
        private string AttributeMatchEval(Match match)
        {
            if (match.Groups[1].Success) //attribute value
                return "<span class=\"kwrd\">" + match + "</span>";

            if (match.Groups[2].Success) //attribute name
                return "<span class=\"attr\">" + match + "</span>";

            return match.ToString();
        }

        /// <summary>
        /// Called to evaluate the HTML fragment corresponding to each 
        /// matching token in the code.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> resulting from a 
        /// single regular expression match.</param>
        /// <returns>A string containing the HTML code fragment.</returns>
        protected override string MatchEval(Match match)
        {
            if (match.Groups[1].Success) //JavaScript code
            {
                return _jsf.FormatSubCode(match.ToString());
            }
            if (match.Groups[2].Success) //comment
            {
                var reader = new StringReader(match.ToString());
                string line;
                var sb = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("\n");
                    }
                    sb.Append("<span class=\"rem\">");
                    sb.Append(line);
                    sb.Append("</span>");
                }
                return sb.ToString();
            }
            if (match.Groups[3].Success) //asp tag
            {
                return "<span class=\"asp\">" + match + "</span>";
            }
            if (match.Groups[4].Success) //asp C# code
            {
                return _csf.FormatSubCode(match.ToString());
            }
            if (match.Groups[5].Success) //tag delimiter
            {
                return "<span class=\"kwrd\">" + match + "</span>";
            }
            if (match.Groups[6].Success) //html tagname
            {
                return "<span class=\"html\">" + match + "</span>";
            }
            if (match.Groups[7].Success) //attributes
            {
                return _attribRegex.Replace(match.ToString(), AttributeMatchEval);
            }
            if (match.Groups[8].Success) //entity
            {
                return "<span class=\"attr\">" + match + "</span>";
            }
            return match.ToString();
        }
    }
}