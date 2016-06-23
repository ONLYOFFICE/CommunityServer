/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ASC.Web.Core.Security
{
    public class HtmlSanitizer
    {
        private static readonly Regex ScriptReplacer = new Regex(@"<\s*script[^>]*>(.*?)<\s*/\s*script>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);

        //BUG: regex bellow not correctly catch attrs like this onclick="javascript:alert('aaa')" it catches only a part of it: onclick="javascript:alert('. Will fix later
        private static readonly Regex AttrReplacer = new Regex(@"(\S+)=[""']?((?:.(?![""']?\s+(?:\S+)=|[>""']))+.)[""']?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly List<string> BlockedAttrs = new List<string>() 
        { 
            "onload", 
            "onunload", 
            "onclick", 
            "ondblclick", 
            "onmousedown", 
            "onmouseup", 
            "onmouseover", 
            "onmousemove", 
            "onmouseout", 
            "onfocus", 
            "onblur", 
            "onkeypress", 
            "onkeydown", 
            "onkeyup",
            "onsubmit", 
            "onreset", 
            "onselect", 
            "onchange"
        };


        public static string Sanitize(string html)
        {
            return ScriptReplacer.Replace(AttrReplacer.Replace(html, new MatchEvaluator(EvalAttributes)), "");
        }

        private static string EvalAttributes(Match match)
        {
            if (match.Success)
            {
                if (match.Groups[1].Success)
                {
                    if (BlockedAttrs.Contains(match.Groups[1].Value.ToLowerInvariant().Trim()))
                    {
                        return string.Empty;
                    }
                }
                if (match.Groups[2].Success && match.Groups[2].Value.StartsWith("javascript", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }
            }
            return match.Value;
        }
    }
}