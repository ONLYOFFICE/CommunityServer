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

using System.Text.RegularExpressions;

#endregion


namespace Textile.Blocks
{
    public class HyperLinkBlockModifier : BlockModifier
    {
        private string m_rel = string.Empty;

        public override string ModifyLine(string line)
        {
            line = Regex.Replace(line,
                                    @"(?<pre>[\s[{(]|" + Globals.PunctuationPattern + @")?" +       // $pre
                                    "\"" +									// start
                                    Globals.BlockModifiersPattern +			// attributes
                                    "(?<text>[\\w\\W]+?)" +					// text
                                    @"\s?" +
									@"(?:\((?<title>[^)]+)\)(?=""))?" +		// title
                                    "\":" +
                                    string.Format(@"""(?<url>\S+[^""]+)""",Regex.Escape(@"a-zA-Z:/.-{}?&_%#+=@")) +						// url
                                    @"(?<slash>\/)?" +						// slash
                                    @"(?<post>[^\w\/;]*)" +					// post
                                    @"(?=\s|$)",
                                   new MatchEvaluator(HyperLinksFormatMatchEvaluator));
            return line;
        }

        private string HyperLinksFormatMatchEvaluator(Match m)
        {
            //TODO: check the URL
            string atts = BlockAttributesParser.ParseBlockAttributes(m.Groups["atts"].Value, "a");
            if (m.Groups["title"].Length > 0)
                atts += " title=\"" + m.Groups["title"].Value + "\"";
            string linkText = m.Groups["text"].Value.Trim(' ');

            string str = m.Groups["pre"].Value + "<a ";
			if (!string.IsNullOrEmpty(m_rel))
				str += "ref=\"" + m_rel + "\" ";
			str += "href=\"" +
				  m.Groups["url"].Value + m.Groups["slash"].Value + "\"" +
				  atts +
				  ">" + linkText + "</a>" + m.Groups["post"].Value;
            return str;
        }
    }
}
