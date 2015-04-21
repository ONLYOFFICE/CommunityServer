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
    public class CodeBlockModifier : BlockModifier
    {
        public override string ModifyLine(string line)
        {
            // Replace "@...@" zones with "<code>" tags.
            MatchEvaluator me = new MatchEvaluator(CodeFormatMatchEvaluator);
            line = Regex.Replace(line,
                                  @"(?<before>^|([\s\([{]))" +    // before
                                   "@" +
                                  @"(\|(?<lang>\w+)\|)?" +        // lang
                                   "(?<code>[^@]+)" +              // code
                                   "@" +
                                  @"(?<after>$|([\]}])|(?=" + Globals.PunctuationPattern + @"{1,2}|\s|$))",  // after
                                me);
            // Encode the contents of the "<code>" tags so that we don't
            // generate formatting out of it.
            line = NoTextileEncoder.EncodeNoTextileZones(line,
                                  @"(?<=(^|\s)<code(" + Globals.HtmlAttributesPattern + @")>)",
                                  @"(?=</code>)");
            return line;
        }

        public override string Conclude(string line)
        {
            // Recode everything except "<" and ">";
            line = NoTextileEncoder.DecodeNoTextileZones(line,
                                    @"(?<=(^|\s)<code(" + Globals.HtmlAttributesPattern + @")>)",
                                    @"(?=</code>)",
                                    new string[] { "<", ">" });
            return line;
        }

        static public string CodeFormatMatchEvaluator(Match m)
        {
            string res = m.Groups["before"].Value + "<code";
            if (m.Groups["lang"].Length > 0)
                res += " language=\"" + m.Groups["lang"].Value + "\"";
            res += ">" + m.Groups["code"].Value + "</code>" + m.Groups["after"].Value;
            return res;
        }
    }
}
