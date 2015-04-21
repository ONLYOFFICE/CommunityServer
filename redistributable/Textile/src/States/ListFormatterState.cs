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


namespace Textile.States
{
    /// <summary>
    /// Base formatting state for all lists.
    /// </summary>
    abstract public class ListFormatterState : FormatterState
    {
        internal const string PatternBegin = @"^\s*(?<tag>";
        internal const string PatternEnd = @")" + Globals.BlockModifiersPattern + @"(?:\s+)? (?<content>.*)$";

        private bool m_firstItem = true;
        private bool m_firstItemLine = true;
        private string m_tag;
        private string m_attsInfo;
        private string m_alignInfo;

        protected int NestingDepth
        {
            get { return m_tag.Length; }
        }

        public ListFormatterState(TextileFormatter formatter)
            : base(formatter)
        {
        }

        public override string Consume(string input, Match m)
        {
            m_tag = m.Groups["tag"].Value;
            m_alignInfo = m.Groups["align"].Value;
            m_attsInfo = m.Groups["atts"].Value;
            input = m.Groups["content"].Value;

            this.Formatter.ChangeState(this);

            return input;
        }

        public sealed override void Enter()
        {
            m_firstItem = true;
            m_firstItemLine = true;
            WriteIndent();
        }

        public sealed override void Exit()
        {
            Formatter.Output.WriteLine("</li>");
            WriteOutdent();
        }

        public sealed override void FormatLine(string input)
        {
            if (m_firstItemLine)
            {
                if (!m_firstItem)
                    Formatter.Output.WriteLine("</li>");
                Formatter.Output.Write(string.Format("<li {0}>",FormattedStylesAndAlignment("li")));
                m_firstItemLine = false;
            }
            else
            {
                Formatter.Output.WriteLine("<br />");
            }
            Formatter.Output.Write(input);
            m_firstItem = false;
        }

        public sealed override bool ShouldNestState(FormatterState other)
        {
            ListFormatterState listState = (ListFormatterState)other;
            return (listState.NestingDepth > NestingDepth);
        }

        public sealed override bool ShouldExit(string input)
        {
            // If we have an empty line, we can exit.
            if (string.IsNullOrEmpty(input))
                return true;

            // We exit this list if the next
            // list item is of the same type but less
            // deep as us, or of the other type of
            // list and as deep or less.
            if (NestingDepth > 1)
            {
                if (IsMatchForMe(input, 1, NestingDepth - 1))
                    return true;
            }
            if (IsMatchForOthers(input, 1, NestingDepth))
                return true;

            // As it seems we're going to continue taking
            // care of this line, we take the opportunity
            // to check whether it's the same list item as
            // previously (no "**" or "##" tags), or if it's
            // a new list item.
            if (IsMatchForMe(input, NestingDepth, NestingDepth))
                m_firstItemLine = true;

            return false;
        }

        public sealed override bool ShouldParseForNewFormatterState(string input)
        {
            // We don't let anyone but ourselves mess with our stuff.
            if (IsMatchForMe(input, 1, 100))
                return true;
            if (IsMatchForOthers(input, 1, 100))
                return true;
            return false;
        }

        protected abstract void WriteIndent();
        protected abstract void WriteOutdent();
        protected abstract bool IsMatchForMe(string input, int minNestingDepth, int maxNestingDepth);
        protected abstract bool IsMatchForOthers(string input, int minNestingDepth, int maxNestingDepth);

        protected string FormattedStylesAndAlignment(string element)
        {
            return Blocks.BlockAttributesParser.ParseBlockAttributes(m_alignInfo + m_attsInfo,element);
        }
    }
}
