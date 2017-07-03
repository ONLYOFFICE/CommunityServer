using System.Text;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/license.html
	*
	* $Id:$
	*/
	
	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	public class DefaultFileLineFormatter : FileLineFormatter
	{
		public override string getFormatString(string fileName, int line, int column)
		{
			StringBuilder buf = new StringBuilder();
			
			if (fileName != null)
				buf.Append(fileName + ":");
			
			if (line != - 1)
			{
				if (fileName == null)
					buf.Append("line ");
				
				buf.Append(line);
				
				if (column != - 1)
					buf.Append(":" + column);
				
				buf.Append(":");
			}
			
			buf.Append(" ");
			
			return buf.ToString();
		}
	}
}