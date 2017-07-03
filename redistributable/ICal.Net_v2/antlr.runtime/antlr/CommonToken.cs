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

	public class CommonToken : Token
	{
		public static readonly CommonTokenCreator Creator = new CommonTokenCreator();

		// most tokens will want line and text information
		protected internal int line;
		protected internal string text;
		protected internal int col;
		
		public CommonToken()
		{
		}
		
		public CommonToken(int t, string txt)
		{
			type_ = t;
		    text = txt;
		}
		
		public CommonToken(string s)
		{
			text = s;
		}
		
		public override int getLine()
		{
			return line;
		}
		
		public override string getText()
		{
			return text;
		}
		
		public override void  setLine(int l)
		{
			line = l;
		}
		
		public override void  setText(string s)
		{
			text = s;
		}
		
		public override string ToString()
		{
			return "[\"" + getText() + "\",<" + type_ + ">,line=" + line + ",col=" + col + "]";
		}
		
		/*Return token's start column */
		public override int getColumn()
		{
			return col;
		}
		
		public override void  setColumn(int c)
		{
			col = c;
		}

		public class CommonTokenCreator : TokenCreator
		{

			/// <summary>
			/// Returns the fully qualified name of the Token type that this
			/// class creates.
			/// </summary>
			public override string TokenTypeName => typeof(CommonToken).FullName;

		    /// <summary>
			/// Constructs a <see cref="Token"/> instance.
			/// </summary>
			public override IToken Create() => new CommonToken();
		}
	}
}