using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Textile.States
{
    [FormatterState(@"^\s*<(h[0-9]|p|pre|blockquote)" + Globals.HtmlAttributesPattern + ">")]
    public class PassthroughFormatterState : FormatterState
    {
        public PassthroughFormatterState(TextileFormatter f)
            : base(f)
        {
        }

        public override string Consume(string input, Match m)
        {
            this.Formatter.ChangeState(this);
            return input;
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
            Formatter.Output.WriteLine(input);
        }

        public override bool ShouldExit(string input)
        {
            return true;
        }
    }
}
