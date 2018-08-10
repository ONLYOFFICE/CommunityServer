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


using System;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Notify.Patterns;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;

namespace ASC.Notify.Textile
{
    public class JabberStyler : IPatternStyler
    {
        static readonly Regex VelocityArguments = new Regex(NVelocityPatternFormatter.NoStylePreffix + "(?<arg>.*?)" + NVelocityPatternFormatter.NoStyleSuffix, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex LinkReplacer = new Regex(@"""(?<text>[\w\W]+?)"":""(?<link>[^""]+)""", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex TextileReplacer = new Regex(@"(h1\.|h2\.|\*|h3\.|\^)", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex BrReplacer = new Regex(@"<br\s*\/*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex ClosedTagsReplacer = new Regex(@"</(p|div)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex TagReplacer = new Regex(@"<(.|\n)*?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex MultiLineBreaksReplacer = new Regex(@"(?:\r\n|\r(?!\n)|(?!<\r)\n){3,}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public void ApplyFormating(NoticeMessage message)
        {
            var body = string.Empty;
            if (!string.IsNullOrEmpty(message.Subject))
            {
                body += VelocityArguments.Replace(message.Subject, ArgMatchReplace) + Environment.NewLine;
                message.Subject = string.Empty;
            }
            if (string.IsNullOrEmpty(message.Body)) return;
            var lines = message.Body.Split(new[] {Environment.NewLine, "\n"}, StringSplitOptions.None);
            for (var i = 0; i < lines.Length - 1; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) { body += Environment.NewLine; continue; }
                lines[i] = VelocityArguments.Replace(lines[i], ArgMatchReplace);
                body += LinkReplacer.Replace(lines[i], EvalLink) + Environment.NewLine;
            }
            lines[lines.Length - 1] = VelocityArguments.Replace(lines[lines.Length - 1], ArgMatchReplace);
            body += LinkReplacer.Replace(lines[lines.Length - 1], EvalLink);
            body = TextileReplacer.Replace(HttpUtility.HtmlDecode(body), ""); //Kill textile markup
            body = BrReplacer.Replace(body, Environment.NewLine);
            body = ClosedTagsReplacer.Replace(body, Environment.NewLine);
            body = TagReplacer.Replace(body, "");
            body = MultiLineBreaksReplacer.Replace(body, Environment.NewLine);
            message.Body = body;
        }

        private static string EvalLink(Match match)
        {
            if (match.Success)
            {
                if (match.Groups["text"].Success && match.Groups["link"].Success)
                {
                    if (match.Groups["text"].Value.Equals(match.Groups["link"].Value,StringComparison.OrdinalIgnoreCase))
                    {
                        return " " + match.Groups["text"].Value + " ";
                    }
                    return match.Groups["text"].Value + string.Format(" ( {0} )", match.Groups["link"].Value);
                }
            }
            return match.Value;
        }

        private static string ArgMatchReplace(Match match)
        {
            return match.Result("${arg}");
        }
    }
}
