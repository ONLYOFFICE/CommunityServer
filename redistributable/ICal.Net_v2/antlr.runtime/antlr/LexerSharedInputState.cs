using System.IO;

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

	/*This object contains the data associated with an
	*  input stream of characters.  Multiple lexers
	*  share a single LexerSharedInputState to lex
	*  the same input stream.
	*/
	public class LexerSharedInputState
	{
		protected internal int column;
		protected internal int line;
		protected internal int tokenStartColumn;
		protected internal int tokenStartLine;
		protected internal InputBuffer input;
		
		/*What file (if known) caused the problem? */
		protected internal string filename;
		
		public int guessing;
		
		public LexerSharedInputState(InputBuffer inbuf)
		{
			initialize();
			input = inbuf;
		}
		
		public LexerSharedInputState(Stream inStream) : this(new ByteBuffer(inStream))
		{
		}
		
		public LexerSharedInputState(TextReader inReader) : this(new CharBuffer(inReader))
		{
		}
		
		private void initialize()
		{
			column = 1;
			line = 1;
			tokenStartColumn = 1;
			tokenStartLine = 1;
			guessing = 0;
			filename = null;
		}
		
		public virtual void reset()
		{
			initialize();
			input.reset();
		}

		public virtual void resetInput(InputBuffer ib)
		{
			reset();
			input = ib;
		}

		public virtual void resetInput(Stream s)
		{
			reset();
			input = new ByteBuffer(s);
		}

		public virtual void resetInput(TextReader tr)
		{
			reset();
			input = new CharBuffer(tr);
		}
	}
}