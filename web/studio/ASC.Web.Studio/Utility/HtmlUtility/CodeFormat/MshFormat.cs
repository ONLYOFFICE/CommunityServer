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


namespace ASC.Web.Studio.Utility.HtmlUtility.CodeFormat
{
    /// <summary>
    /// Generates color-coded HTML 4.01 from MSH (code name Monad) source code.
    /// </summary>
    internal class MshFormat : CodeFormat
    {
        /// <summary>
        /// Regular expression string to match single line comments (#).
        /// </summary>
        protected override string CommentRegEx
        {
            get { return @"#.*?(?=\r|\n)"; }
        }

        /// <summary>
        /// Regular expression string to match string and character literals. 
        /// </summary>
        protected override string StringRegEx
        {
            get { return @"@?""""|@?"".*?(?!\\).""|''|'.*?(?!\\).'"; }
        }

        /// <summary>
        /// The list of MSH keywords.
        /// </summary>
        protected override string Keywords
        {
            get
            {
                return "function filter global script local private if else"
                       + " elseif for foreach in while switch continue break"
                       + " return default param begin process end throw trap";
            }
        }

        /// <summary>
        /// Use preprocessors property to hilight operators.
        /// </summary>
        protected override string Preprocessors
        {
            get
            {
                return "-band -bor -match -notmatch -like -notlike -eq -ne"
                       + " -gt -ge -lt -le -is -imatch -inotmatch -ilike"
                       + " -inotlike -ieq -ine -igt -ige -ilt -ile";
            }
        }
    }
}