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
	
	/*A Stream of Token objects fed to the parser from a Tokenizer that can
	* be rewound via mark()/rewind() methods.
	* <p>
	* A dynamic array is used to buffer up all the input tokens.  Normally,
	* "k" tokens are stored in the buffer.  More tokens may be stored during
	* guess mode (testing syntactic predicate), or when LT(i>k) is referenced.
	* Consumption of tokens is deferred.  In other words, reading the next
	* token is not done by conume(), but deferred until needed by LA or LT.
	* <p>
	*
	* @see antlr.Token
	* @see antlr.Tokenizer
	* @see antlr.TokenQueue
	*/
	
	public class TokenBuffer
	{
		// Token source
		protected internal TokenStream input;
		
		// Number of active markers
		protected internal int nMarkers;
		
		// Additional offset used when markers are active
		protected internal int markerOffset;
		
		// Number of calls to consume() since last LA() or LT() call
		protected internal int numToConsume;
		
		// Circular queue
		internal TokenQueue queue;
		
		/*Create a token buffer */
		public TokenBuffer(TokenStream input_)
		{
			input = input_;
			queue = new TokenQueue(1);
		}
		
		/*Mark another token for deferred consumption */
		public virtual void  consume()
		{
			numToConsume++;
		}
		
		/*Ensure that the token buffer is sufficiently full */
		protected virtual void  fill(int amount)
		{
			syncConsume();
			// Fill the buffer sufficiently to hold needed tokens
			while (queue.nbrEntries < (amount + markerOffset))
			{
				// Append the next token
				queue.append(input.nextToken());
			}
		}
		
		/*Get a lookahead token value */
		public virtual int LA(int i)
		{
			fill(i);
			return queue.elementAt(markerOffset + i - 1).Type;
		}
		
		/*Get a lookahead token */
		public virtual IToken LT(int i)
		{
			fill(i);
			return queue.elementAt(markerOffset + i - 1);
		}
		
		/*Sync up deferred consumption */
		protected virtual void  syncConsume()
		{
			while (numToConsume > 0)
			{
				if (nMarkers > 0)
				{
					// guess mode -- leave leading tokens and bump offset.
					markerOffset++;
				}
				else
				{
					// normal mode -- remove first token
					queue.removeFirst();
				}
				numToConsume--;
			}
		}
	}
}