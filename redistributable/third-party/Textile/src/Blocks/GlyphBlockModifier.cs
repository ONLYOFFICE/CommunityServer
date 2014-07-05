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
    public class GlyphBlockModifier : BlockModifier
    {
        public override string ModifyLine(string line)
        {
            line = Regex.Replace(line, "\"\\z", "\" ");

            // fix: hackish
            string[,] glyphs = {
                                { @"([^\s[{(>_*])?\'(?(1)|(\s|s\b|" + Globals.PunctuationPattern + @"))", "$1&#8217;$2" },    //  single closing
                                { @"\'", "&#8216;" },                                                   //  single opening
                                { @"([^\s[{(>_*])?""(?(1)|(\s|" + Globals.PunctuationPattern + @"))", "$1&#8221;$2" },        //  double closing
                                { @"""", "&#8220;" },                                                   //  double opening
                                { @"\b( )?\.{3}", "$1&#8230;" },                                        //  ellipsis
                                { @"\b([A-Z][A-Z0-9]{2,})\b(?:[(]([^)]*)[)])", "<acronym title=\"$2\">$1</acronym>" },        //  3+ uppercase acronym
                                { @"(\s)?--(\s)?", "$1&#8212;$2" },                                     //  em dash
                                { @"\s-\s", " &#8211; " },                                              //  en dash
                                { @"(\d+)( )?x( )?(\d+)", "$1$2&#215;$3$4" },                           //  dimension sign
                                { @"\b ?[([](TM|tm)[])]", "&#8482;" },                                  //  trademark
                                { @"\b ?[([](R|r)[])]", "&#174;" },                                     //  registered
                                { @"\b ?[([](C|c)[])]", "&#169;" }                                      //  copyright
                              };

            string output = "";

            if (!Regex.IsMatch(line, "<.*>"))
            {
                // If no HTML, do a simple search & replace.
                for (int i = 0; i < glyphs.GetLength(0); ++i)
                {
                    line = Regex.Replace(line, glyphs[i, 0], glyphs[i, 1]);
                }
                output = line;
            }
            else
            {
                string[] splits = Regex.Split(line, "(<.*?>)");
                string offtags = "code|pre|notextile";
                bool codepre = false;
                foreach (string split in splits)
                {
                    string modifiedSplit = split;
                    if (modifiedSplit.Length == 0)
                        continue;

                    if (Regex.IsMatch(modifiedSplit, @"<(" + offtags + ")>"))
                        codepre = true;
                    if (Regex.IsMatch(modifiedSplit, @"<\/(" + offtags + ")>"))
                        codepre = false;

                    if (!Regex.IsMatch(modifiedSplit, "<.*>") && !codepre)
                    {
                        for (int i = 0; i < glyphs.GetLength(0); ++i)
                        {
                            modifiedSplit = Regex.Replace(modifiedSplit, glyphs[i, 0], glyphs[i, 1]);
                        }
                    }

                    // do htmlspecial if between <code>
                    if (codepre == true)
                    {
                        //TODO: htmlspecialchars(line)
                        //line = Regex.Replace(line, @"&lt;(\/?" + offtags + ")&gt;", "<$1>");
                        //line = line.Replace("&amp;#", "&#");
                    }

                    output += modifiedSplit;
                }
            }

            return output;
        }
    }
}
