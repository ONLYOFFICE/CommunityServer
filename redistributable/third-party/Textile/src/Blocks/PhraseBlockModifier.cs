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
    public abstract class PhraseBlockModifier : BlockModifier
    {
        protected PhraseBlockModifier()
        {
        }

        protected string PhraseModifierFormat(string input, string modifier, string tag)
        {
            // All phrase modifiers are one character, or a double character. Sometimes,
            // there's an additional escape character for the regex ('\').
            string compressedModifier = modifier;
            if (modifier.Length == 4)
            {
                compressedModifier = modifier.Substring(0, 2);
            }
            else if (modifier.Length == 2)
            {
                if (modifier[0] != '\\')
                    compressedModifier = modifier.Substring(0, 1);
                //else: compressedModifier = modifier;
            }
            //else: compressedModifier = modifier;

            // We try to remove the Textile tag used for the formatting from
            // the punctuation pattern, so that we match the end of the formatted
            // zone correctly.
            string punctuationPattern = Globals.PunctuationPattern.Replace(compressedModifier, "");

            // Now we can do the replacement.
            PhraseModifierMatchEvaluator pmme = new PhraseModifierMatchEvaluator(tag);
            string res = Regex.Replace(input,
                                            @"(?<=\s|" + punctuationPattern + @"|[{\(\[]|^)" +
                                            modifier +
                                            Globals.BlockModifiersPattern +
                                            @"(:(?<cite>(\S+)))?" +
                                            @"(?<content>[^" + compressedModifier + "]*)" +
                                            @"(?<end>" + punctuationPattern + @"*)" +
                                            modifier +
                                            @"(?=[\]\)}]|" + punctuationPattern + @"+|\s|$)",
                                       new MatchEvaluator(pmme.MatchEvaluator)
                                      );
            return res;
        }

        private class PhraseModifierMatchEvaluator
        {
            string m_tag;

            public PhraseModifierMatchEvaluator(string tag)
            {
                m_tag = tag;
            }

            public string MatchEvaluator(Match m)
            {
                if (m.Groups["content"].Length == 0)
                {
                    // It's possible that the "atts" match groups eats the contents
                    // when the user didn't want to give block attributes, but the content
                    // happens to match the syntax. For example: "*(blah)*".
                    if (m.Groups["atts"].Length == 0)
                        return m.ToString();
                    return "<" + m_tag + ">" + m.Groups["atts"].Value + m.Groups["end"].Value + "</" + m_tag + ">";
                }

                string atts = BlockAttributesParser.ParseBlockAttributes(m.Groups["atts"].Value, m_tag);
                if (m.Groups["cite"].Length > 0)
                    atts += " cite=\"" + m.Groups["cite"] + "\"";

                string res = "<" + m_tag + atts + ">" +
                             m.Groups["content"].Value + m.Groups["end"].Value +
                             "</" + m_tag + ">";
                return res;
            }
        }
    }
}
