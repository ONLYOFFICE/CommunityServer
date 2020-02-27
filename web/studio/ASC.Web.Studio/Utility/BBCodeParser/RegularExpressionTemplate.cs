/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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