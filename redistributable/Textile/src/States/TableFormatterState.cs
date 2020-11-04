using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace Textile.States
{
    [FormatterState(@"^\s*(?<tag>table)" +
                   Globals.SpanPattern +
                   Globals.AlignPattern +
                   Globals.BlockModifiersPattern +
                   @"\.\s*$")]
	public class TableFormatterState : FormatterState
	{
        private string m_attsInfo;
        private string m_alignInfo;

		public TableFormatterState(TextileFormatter f)
			: base(f)
		{
		}

        public override string Consume(string input, Match m)
        {
            m_alignInfo = m.Groups["align"].Value;
            m_attsInfo = m.Groups["atts"].Value;

            //TODO: check the state (it could already be a table!)
            this.Formatter.ChangeState(this);

            return string.Empty;
        }

        public override bool ShouldNestState(FormatterState other)
		{
			return false;
		}

		public override void Enter()
		{
            Formatter.Output.WriteLine("<table" + FormattedStylesAndAlignment() + ">");
		}

		public override void Exit()
		{
			Formatter.Output.WriteLine("</table>");
		}

		public override void FormatLine(string input)
		{
			if (input.Length > 0)
				throw new Exception("The TableFormatter state is not supposed to format any lines!");
		}

		public override bool ShouldExit(string input)
		{
			Match m = Regex.Match(input,
                                   @"^\s*" + Globals.AlignPattern + Globals.BlockModifiersPattern +
								   @"(\.\s?)?(?<tag>\|)" +
								   @"(?<content>.*)(?=\|)"
								  );
			return( m.Success == false );
		}

        protected string FormattedStylesAndAlignment()
        {
            return Blocks.BlockAttributesParser.ParseBlockAttributes(m_alignInfo + m_attsInfo);
        }
	}
}
