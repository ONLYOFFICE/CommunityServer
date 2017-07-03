using antlr.collections;

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

	public abstract class Parser
	{
	    // The unique keys for each event that Parser [objects] can generate
	    internal static readonly object EnterRuleEventKey = new object();
	    internal static readonly object ExitRuleEventKey = new object();
	    internal static readonly object DoneEventKey = new object();
	    internal static readonly object ReportErrorEventKey = new object();
	    internal static readonly object ReportWarningEventKey = new object();
	    internal static readonly object NewLineEventKey = new object();
	    internal static readonly object MatchEventKey = new object();
	    internal static readonly object MatchNotEventKey = new object();
	    internal static readonly object MisMatchEventKey = new object();
	    internal static readonly object MisMatchNotEventKey = new object();
	    internal static readonly object ConsumeEventKey = new object();
	    internal static readonly object LAEventKey = new object();
	    internal static readonly object SemPredEvaluatedEventKey = new object();
	    internal static readonly object SynPredStartedEventKey = new object();
	    internal static readonly object SynPredFailedEventKey = new object();
	    internal static readonly object SynPredSucceededEventKey = new object();

		protected internal ParserSharedInputState inputState = new ParserSharedInputState();

        /*Table of token type to token names */
        protected internal string[] tokenNames;
		
		/*Get another token object from the token stream */
		public abstract void  consume();
		public virtual string getFilename()
		{
			return inputState.filename;
		}
		
		/*Return the token type of the ith token of lookahead where i=1
		* is the current token being examined by the parser (i.e., it
		* has not been matched yet).
		*/
		public abstract int LA(int i);

		/*Return the ith token of lookahead */
		public abstract IToken LT(int i);

		/*Make sure current lookahead symbol matches token type <tt>t</tt>.
		* Throw an exception upon mismatch, which is catch by either the
		* error handler or by the syntactic predicate.
		*/
		public virtual void  match(int t)
		{
		    if (LA(1) != t)
				throw new MismatchedTokenException(tokenNames, LT(1), t, false, getFilename());
		    consume();
		}

	    /*Make sure current lookahead symbol matches the given set
		* Throw an exception upon mismatch, which is catch by either the
		* error handler or by the syntactic predicate.
		*/
		public virtual void  match(BitSet b)
		{
		    if (!b.member(LA(1)))
				throw new MismatchedTokenException(tokenNames, LT(1), b, false, getFilename());
		    consume();
		}

		/*Set or change the input token buffer */
		public virtual void  setTokenBuffer(TokenBuffer t)
		{
			inputState.input = t;
		}
	}
}
