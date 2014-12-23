/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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