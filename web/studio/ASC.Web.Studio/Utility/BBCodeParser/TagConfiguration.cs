/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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