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
    [FormatterState(SimpleBlockFormatterState.PatternBegin + @"bq" + SimpleBlockFormatterState.PatternEnd)]
	public class BlockQuoteFormatterState : SimpleBlockFormatterState
	{
        public BlockQuoteFormatterState(TextileFormatter f)
            : base(f)
        {
        }

		public override void Enter()
		{
            Formatter.Output.Write("<blockquote" + FormattedStylesAndAlignment("blockquote") + "><p>");
		}

		public override void Exit()
		{
			Formatter.Output.WriteLine("</p></blockquote>");
		}

		public override void FormatLine(string input)
		{
			Formatter.Output.Write(input);
		}

        public override bool ShouldExit(string input)
        {
            if (Regex.IsMatch(input, @"^\s*$"))
                return true;
            Formatter.Output.WriteLine("<br />");
            return false;
        }

        public override Type FallbackFormattingState
        {
            get { return null; }
        }
    }
}
