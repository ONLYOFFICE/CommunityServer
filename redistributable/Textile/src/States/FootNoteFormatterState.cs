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
    [FormatterState(SimpleBlockFormatterState.PatternBegin + @"fn[0-9]+" + SimpleBlockFormatterState.PatternEnd)]
    public class FootNoteFormatterState : SimpleBlockFormatterState
    {
        int m_noteID = 0;

        public FootNoteFormatterState(TextileFormatter f)
            : base(f)
        {
        }

        public override void Enter()
        {
            Formatter.Output.Write(
                string.Format("<p id=\"fn{0}\"{1}><sup>{2}</sup> ",
                    m_noteID,
                    FormattedStylesAndAlignment("p"),
                    m_noteID));
        }

        public override void Exit()
        {
            Formatter.Output.WriteLine("</p>");
        }

        public override void FormatLine(string input)
        {
            Formatter.Output.Write(input);
        }

        public override bool ShouldExit(string input)
        {
            return true;
        }
        protected override void OnContextAcquired()
        {
            Match m = Regex.Match(Tag, @"^fn(?<id>[0-9]+)");
            m_noteID = Int32.Parse(m.Groups["id"].Value);
        }

        public override bool ShouldNestState(FormatterState other)
        {
            return false;
        }
    }
}
