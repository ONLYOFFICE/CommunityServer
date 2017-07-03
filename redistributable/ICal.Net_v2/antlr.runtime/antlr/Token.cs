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

	/*A token is minimally a token type.  Subclasses can add the text matched
	*  for the token and line info. 
	*/

	public class Token : IToken //, ICloneable
	{
		// constants
		public const int MIN_USER_TYPE = 4;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int INVALID_TYPE = 0;
		public const int EOF_TYPE = 1;
		public static readonly int SKIP = - 1;
		
		// each Token has at least a token type
		protected int type_;
		
		// the illegal token object
		public static Token badToken = new Token(INVALID_TYPE, "<no text>");
		
		public Token()
		{
			type_ = INVALID_TYPE;
		}
		public Token(int t)
		{
			type_ = t;
		}
		public Token(int t, string txt)
		{
			type_ = t;
			setText(txt);
		}
		public virtual int getColumn()
		{
			return 0;
		}
		public virtual int getLine()
		{
			return 0;
		}
		public virtual string getFilename() 
		{
			return null;
		}

		public virtual void setFilename(string name) 
		{
		}

		public virtual string getText()
		{
			return "<no text>";
		}

		public int Type
		{
			get { return type_;  }
			set { type_ = value; }
		}

		public virtual void setType(int newType)	{ Type = newType; }

		public virtual void  setColumn(int c)
		{
		}
		public virtual void  setLine(int l)
		{
		}
		public virtual void  setText(string t)
		{
		}
		public override string ToString()
		{
			return "[\"" + getText() + "\",<" + type_ + ">]";
		}
	}
}