using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Textile.States
{
    [FormatterState(@"^\s*<notextile>\s*$")]
    public class NoTextileFormatterState : FormatterState
    {
        bool m_shouldExitNextTime = false;

        public NoTextileFormatterState(TextileFormatter f)
            : base(f)
        {
        }

        public override string Consume(string input, Match m)
        {
            this.Formatter.ChangeState(this);
            return string.Empty;
        }

        public override bool ShouldNestState(FormatterState other)
        {
            return false;
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
        }

        public override void FormatLine(string input)
        {
            if (!m_shouldExitNextTime)
                Formatter.Output.WriteLine(input);
        }

        public override bool ShouldExit(string input)
        {
            if (m_shouldExitNextTime)
                return true;
            m_shouldExitNextTime = Regex.IsMatch(input, @"^\s*</notextile>\s*$");
            return false;
        }

        public override bool ShouldFormatBlocks(string input)
        {
            return false;
        }

        public override bool ShouldParseForNewFormatterState(string input)
        {
            return false;
        }

        public override Type FallbackFormattingState
        {
            get
            {
                return null;
            }
        }
    }
}
