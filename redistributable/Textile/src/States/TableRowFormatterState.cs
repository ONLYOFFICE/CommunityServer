using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Textile.States
{
    [FormatterState(@"^\s*(" + Globals.AlignPattern + Globals.BlockModifiersPattern + @"\.\s?)?" +
                                   @"\|(?<content>.*)\|\s*$")]
    public class TableRowFormatterState : FormatterState
    {
        private string m_attsInfo;
        private string m_alignInfo;

        public TableRowFormatterState(TextileFormatter f)
            : base(f)
        {
        }

        public override string Consume(string input, Match m)
        {
            m_alignInfo = m.Groups["align"].Value;
            m_attsInfo = m.Groups["atts"].Value;
            input = string.Format("|{0}|", m.Groups["content"].Value);

            if (!(this.Formatter.CurrentState is TableFormatterState))
            {
                TableFormatterState s = new TableFormatterState(this.Formatter);
                this.Formatter.ChangeState(s);
            }

            this.Formatter.ChangeState(this);

            return input;
        }

        public override bool ShouldNestState(FormatterState other)
        {
            return false;
        }

        public override void Enter()
        {
            Formatter.Output.WriteLine("<tr" + FormattedStylesAndAlignment() + ">");
        }

        public override void Exit()
        {
            Formatter.Output.WriteLine("</tr>");
        }

        public override void FormatLine(string input)
        {
            // can get: Align & Classes

            string formattedLine = "";

            string[] cellsInput = input.Split('|');
            for (int i = 1; i < cellsInput.Length - 1; i++)
            {
                string cellInput = cellsInput[i];
                TableCellParser tcp = new TableCellParser(cellInput);
                formattedLine += tcp.GetLineFragmentFormatting();
            }

            Formatter.Output.WriteLine(formattedLine);
        }

        public override bool ShouldExit(string input)
        {
            return true;
        }

        protected string FormattedStylesAndAlignment()
        {
            return Blocks.BlockAttributesParser.ParseBlockAttributes(m_alignInfo + m_attsInfo);
        }
    }
}
