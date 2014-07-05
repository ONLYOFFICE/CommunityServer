#region License Statement
// Copyright (c) L.A.B.Soft.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Textile.Blocks
{
    public class ImageBlockModifier : BlockModifier
    {
        public override string ModifyLine(string line)
        {
            line = Regex.Replace(line,
                                    @"\!" +                   // opening !
                                    @"(?<algn>\<|\=|\>)?" +   // optional alignment atts
                                    Globals.BlockModifiersPattern + // optional style, public class atts
                                    @"(?:\. )?" +             // optional dot-space
                                    @"(?<url>[^\s(!]+)" +     // presume this is the src
                                    @"\s?" +                  // optional space
                                    @"(?:\((?<title>([^\)]+))\))?" +// optional title
                                    @"\!" +                   // closing
                                    @"(?::(?<href>(\S+)))?" +     // optional href
                                    @"(?=\s|\.|,|;|\)|\||$)",               // lookahead: space or simple punctuation or end of string
                                new MatchEvaluator(ImageFormatMatchEvaluator)
                                );
            return line;
        }

        static string ImageFormatMatchEvaluator(Match m)
        {
            string atts = BlockAttributesParser.ParseBlockAttributes(m.Groups["atts"].Value, "img");
            if (m.Groups["algn"].Length > 0)
                atts += " align=\"" + Globals.ImageAlign[m.Groups["algn"].Value] + "\"";
            if (m.Groups["title"].Length > 0)
            {
                atts += " title=\"" + m.Groups["title"].Value + "\"";
                atts += " alt=\"" + m.Groups["title"].Value + "\"";
            }
            else
            {
                atts += " alt=\"\"";
            }
            // Get Image Size?

            string res = "<img src=\"" + m.Groups["url"].Value + "\"" + atts + " />";

            if (m.Groups["href"].Length > 0)
            {
                string href = m.Groups["href"].Value;
                string end = string.Empty;
                Match endMatch = Regex.Match(href, @"(.*)(?<end>\.|,|;|\))$");
                if (m.Success && !string.IsNullOrEmpty(endMatch.Groups["end"].Value))
                {
                    href = href.Substring(0, href.Length - 1);
                    end = endMatch.Groups["end"].Value;
                }
                res = "<a href=\"" + Globals.EncodeHTMLLink(href) + "\">" + res + "</a>" + end;
            }

            return res;
        }
    }
}
