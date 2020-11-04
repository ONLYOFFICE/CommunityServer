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


using System.Collections.Generic;

namespace ASC.Web.Studio.Utility.BBCodeParser
{
    public class TagConfiguration
    {
        public List<TagParamOption> ParamOptions { get; set; }
        public bool IsSingleTag { get; private set; }
        public string Tag { get; set; }
        public string Replacement { get; set; }
        public string AlternativeReplacement { get; set; }
        public bool IsParseContent { get; set; }
        public bool IsParseTextReplacement { get; set; }
        public bool IsParseTextReqularExpressions { get; set; }

        public TagConfiguration(string tag, string replacement) : this(tag, replacement, null, true)
        {
        }

        public TagConfiguration(string tag, string replacement, string alternativeReplacement) : this(tag, replacement, alternativeReplacement, true)
        {
        }

        public TagConfiguration(string tag, string replacement, bool isParseContent) : this(tag, replacement, null, isParseContent)
        {
        }

        public TagConfiguration(string tag, string replacement, string alternativeReplacement, bool isParseContent)
        {
            IsParseTextReplacement = true;
            IsParseTextReqularExpressions = true;

            Tag = tag;
            Replacement = replacement;
            AlternativeReplacement = alternativeReplacement;
            IsParseContent = isParseContent;
            ParamOptions = new List<TagParamOption>(0);

            // If there is a '{0}' in the replacement string the tag cannot be a single tag.
            IsSingleTag = !replacement.Contains("{0}");
        }
    }
}