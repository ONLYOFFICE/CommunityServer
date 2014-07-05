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


namespace Textile
{
    public partial class TextileFormatter
    {
        private readonly Regex VelocityArguments =
            new Regex("nostyle(?<arg>.*?)/nostyle", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private string argMatchReplace(Match match)
        {
            return match.Result("${arg}");
        }

        #region Formatting Methods

        /// <summary>
        /// Formats the given text.
        /// </summary>
        /// <param name="input">The text to format.</param>
        public void Format(string input)
        {
            m_output.Begin();

            // Clean the text...
            string str = PrepareInputForFormatting(input);
            // ...and format each line.
            foreach (string line in str.Split('\n'))
            {
                string tmp = line;

                // Let's see if the current state(s) is(are) finished...
                while (CurrentState != null && CurrentState.ShouldExit(tmp))
                    PopState();

                if (!Regex.IsMatch(tmp, @"^\s*$"))
                {
                    // Figure out the new state for this text line, if possible.
                    if (CurrentState == null || CurrentState.ShouldParseForNewFormatterState(tmp))
                    {
                        tmp = HandleFormattingState(tmp);
                    }
                    // else, the current state doesn't want to be superceded by
                    // a new one. We'll leave him be.

                    // Modify the line with our block modifiers.
                    if (CurrentState == null || CurrentState.ShouldFormatBlocks(tmp))
                    {    
                        foreach (BlockModifier blockModifier in s_blockModifiers)
                        {
                            //TODO: if not disabled...
                            tmp = blockModifier.ModifyLine(tmp);
                        }

                        for (int i = s_blockModifiers.Count - 1; i >= 0; i--)
                        {
                            BlockModifier blockModifier = s_blockModifiers[i];
                            tmp = blockModifier.Conclude(tmp);
                        }
                    }

                    tmp = VelocityArguments.Replace(tmp, argMatchReplace);

                    // Format the current line.
                    CurrentState.FormatLine(tmp);
                }
            }
            // We're done. There might be a few states still on
            // the stack (for example if the text ends with a nested
            // list), so we must pop them all so that they have
            // their "Exit" method called correctly.
            while (m_stackOfStates.Count > 0)
                PopState();

            m_output.End();
        }

        #endregion

        #region Preparation Methods

        /// <summary>
        /// Cleans up a text before formatting.
        /// </summary>
        /// <param name="input">The text to clean up.</param>
        /// <returns>The clean text.</returns>
        /// This method cleans stuff like line endings, so that
        /// we don't have to bother with it while formatting.
        private string PrepareInputForFormatting(string input)
        {
            input = CleanWhiteSpace(input);
            return input;
        }

        private string CleanWhiteSpace(string text)
        {
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\t", "");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            text = Regex.Replace(text, @"\n *\n", "\n\n");
            text = Regex.Replace(text, "\"$", "\" ");
            return text;
        }

        #endregion
    }
}
