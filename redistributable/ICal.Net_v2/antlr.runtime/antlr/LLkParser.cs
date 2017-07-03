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

	/*An LL(k) parser.
	*
	* @see antlr.Token
	* @see antlr.TokenBuffer
	* @see antlr.LL1Parser
	*/
	public class LLkParser : Parser
	{
		internal int k;
		
		public LLkParser(int k_)
		{
			k = k_;
		}
		public LLkParser(ParserSharedInputState state, int k_)
		{
			k = k_;
			inputState = state;
		}
		public LLkParser(TokenBuffer tokenBuf, int k_)
		{
			k = k_;
		    inputState.input = tokenBuf;
		}
		public LLkParser(TokenStream lexer, int k_)
		{
			k = k_;
			TokenBuffer tokenBuf = new TokenBuffer(lexer);
			setTokenBuffer(tokenBuf);
		}
		/*Consume another token from the input stream.  Can only write sequentially!
		* If you need 3 tokens ahead, you must consume() 3 times.
		* <p>
		* Note that it is possible to overwrite tokens that have not been matched.
		* For example, calling consume() 3 times when k=2, means that the first token
		* consumed will be overwritten with the 3rd.
		*/
		public override void  consume()
		{
			inputState.input.consume();
		}
		public override int LA(int i)
		{
			return inputState.input.LA(i);
		}
		public override IToken LT(int i)
		{
			return inputState.input.LT(i);
		}
	}
}