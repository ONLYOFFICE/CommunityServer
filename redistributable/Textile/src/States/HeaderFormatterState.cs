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
    [FormatterState(SimpleBlockFormatterState.PatternBegin + @"pad[0-9]+" + SimpleBlockFormatterState.PatternEnd)]
    public class PaddingFormatterState : SimpleBlockFormatterState
    {
        public PaddingFormatterState(TextileFormatter formatter)
            : base(formatter)
        {
        }

        int m_headerLevel = 0;
        public int HeaderLevel
        {
            get { return m_headerLevel; }
        }


        public override void Enter()
        {
            for (int i = 0; i < HeaderLevel; i++)
            {
                Formatter.Output.Write(string.Format("<br {0}/>", FormattedStylesAndAlignment("br")));
            }
        }

        public override void Exit()
        {
        }

        protected override void OnContextAcquired()
        {
            Match m = Regex.Match(Tag, @"^pad(?<lvl>[0-9]+)");
            m_headerLevel = Int32.Parse(m.Groups["lvl"].Value);
        }

        public override void FormatLine(string input)
        {
            Formatter.Output.Write(input);
        }

        public override bool ShouldExit(string intput)
        {
            return true;
        }

        public override bool ShouldNestState(FormatterState other)
        {
            return false;
        }
    }

    /// <summary>
    /// Formatting state for headers and titles.
    /// </summary>
    [FormatterState(SimpleBlockFormatterState.PatternBegin + @"h[0-9]+" + SimpleBlockFormatterState.PatternEnd)]
    public class HeaderFormatterState : SimpleBlockFormatterState
    {
        int m_headerLevel = 0;
        public int HeaderLevel
        {
            get { return m_headerLevel; }
        }
        
        public HeaderFormatterState(TextileFormatter f)
            : base(f)
        {
        }

        public override void Enter()
        {
            Formatter.Output.Write(string.Format("<h{0}{1}>", HeaderLevel, FormattedStylesAndAlignment(string.Format("h{0}", HeaderLevel))));
        }

        public override void Exit()
        {
            Formatter.Output.WriteLine(string.Format("</h{0}>", HeaderLevel.ToString()));
        }

        protected override void OnContextAcquired()
        {
            Match m = Regex.Match(Tag, @"^h(?<lvl>[0-9]+)");
            m_headerLevel = Int32.Parse(m.Groups["lvl"].Value);
        }

        public override void FormatLine(string input)
        {
            Formatter.Output.Write(input);
        }

        public override bool ShouldExit(string intput)
        {
            return true;
        }

        public override bool ShouldNestState(FormatterState other)
        {
            return false;
        }
    }
}
