using System;
using System.Text;
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

    public abstract class CharScanner : TokenStream
    {
        internal const char NO_CHAR = (char)(0);
        public static readonly char EOF_CHAR = char.MaxValue;

        // The unique keys for each event that CharScanner [objects] can generate
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

        protected internal StringBuilder text;				// text of current token

        protected bool saveConsumedInput = true;	// does consume() save characters?

        /// <summary>Used for creating Token instances.</summary>
        protected TokenCreator tokenCreator;

        /// <summary>Used for caching lookahead characters.</summary>
        protected char cached_LA1;
        protected char cached_LA2;

        /*Tab chars are handled by tab() according to this value; override
        *  method to do anything weird with tabs.
        */
        protected internal int tabsize = 8;

        protected internal IToken returnToken_ = null; // used to return tokens w/o using return val.

        protected internal LexerSharedInputState inputState;

        /*Used during filter mode to indicate that path is desired.
        *  A subsequent scan error will report an error as usual if
        *  acceptPath=true;
        */
        protected internal bool commitToPath = false;

        /*Used to keep track of indentdepth for traceIn/Out */
        protected internal int traceDepth = 0;

        public CharScanner()
        {
            text = new StringBuilder();
            tokenCreator = new CommonToken.CommonTokenCreator();
        }

        public CharScanner(InputBuffer cb)
            : this()
        {
            inputState = new LexerSharedInputState(cb);
            cached_LA2 = inputState.input.LA(2);
            cached_LA1 = inputState.input.LA(1);
        }

        public CharScanner(LexerSharedInputState sharedState)
            : this()
        {
            inputState = sharedState;
            if (inputState != null)
            {
                cached_LA2 = inputState.input.LA(2);
                cached_LA1 = inputState.input.LA(1);
            }
        }

        // From interface TokenStream
        public virtual IToken nextToken() { return null; }

        public virtual void append(char c)
        {
            if (saveConsumedInput)
            {
                text.Append(c);
            }
        }

        public virtual void append(string s)
        {
            if (saveConsumedInput)
            {
                text.Append(s);
            }
        }

        public virtual void commit()
        {
            inputState.input.commit();
        }

        public virtual void recover(RecognitionException ex, BitSet tokenSet)
        {
            consume();
            consumeUntil(tokenSet);
        }

        public virtual void consume()
        {
            if (inputState.guessing == 0)
            {
                append(cached_LA1);
                if (cached_LA1 == '\t')
                {
                    tab();
                }
                else
                {
                    inputState.column++;
                }
            }
            cached_LA1 = inputState.input.consume();
            cached_LA2 = inputState.input.LA(2);
        }

        /*Consume chars until one matches the given char */
        public virtual void consumeUntil(int c)
        {
            while ((EOF_CHAR != cached_LA1) && (c != cached_LA1))
            {
                consume();
            }
        }

        /*Consume chars until one matches the given set */
        public virtual void consumeUntil(BitSet bset)
        {
            while (cached_LA1 != EOF_CHAR && !bset.member(cached_LA1))
            {
                consume();
            }
        }

        public virtual int getColumn()
        {
            return inputState.column;
        }

        public virtual void setColumn(int c)
        {
            inputState.column = c;
        }

        public virtual bool getCommitToPath()
        {
            return commitToPath;
        }

        public virtual string getFilename()
        {
            return inputState.filename;
        }

        public virtual InputBuffer getInputBuffer()
        {
            return inputState.input;
        }

        public virtual LexerSharedInputState getInputState()
        {
            return inputState;
        }

        public virtual void setInputState(LexerSharedInputState state)
        {
            inputState = state;
        }

        public virtual int getLine()
        {
            return inputState.line;
        }

        /*return a copy of the current text buffer */
        public virtual string getText()
        {
            return text.ToString();
        }

        public virtual IToken getTokenObject()
        {
            return returnToken_;
        }

        public virtual char LA(int i)
        {
            if (i == 1)
            {
                return cached_LA1;
            }
            if (i == 2)
            {
                return cached_LA2;
            }
            return inputState.input.LA(i);
        }

        protected internal virtual IToken makeToken(int t)
        {
            IToken newToken = null;
            bool typeCreated;

            try
            {
                newToken = tokenCreator.Create();
                if (newToken != null)
                {
                    newToken.Type = t;
                    newToken.setColumn(inputState.tokenStartColumn);
                    newToken.setLine(inputState.tokenStartLine);
                    // tracking real start line now: newToken.setLine(inputState.line);
                    newToken.setFilename(inputState.filename);
                }
                typeCreated = true;
            }
            catch
            {
                typeCreated = false;
            }

            if (!typeCreated)
            {
                panic("Can't create Token object '" + tokenCreator.TokenTypeName + "'");
                newToken = Token.badToken;
            }
            return newToken;
        }

        public virtual int mark()
        {
            return inputState.input.mark();
        }

        public virtual void match(char c)
        {
            match((int)c);
        }

        public virtual void match(int c)
        {
            if (cached_LA1 != c)
            {
                throw new MismatchedCharException(cached_LA1, Convert.ToChar(c), false, this);
            }
            consume();
        }

        public virtual void match(BitSet b)
        {
            if (!b.member(cached_LA1))
            {
                throw new MismatchedCharException(cached_LA1, b, false, this);
            }
            consume();
        }

        public virtual void match(string s)
        {
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                if (cached_LA1 != s[i])
                {
                    throw new MismatchedCharException(cached_LA1, s[i], false, this);
                }
                consume();
            }
        }

        public virtual void matchNot(char c)
        {
            matchNot((int)c);
        }

        public virtual void matchNot(int c)
        {
            if (cached_LA1 == c)
            {
                throw new MismatchedCharException(cached_LA1, Convert.ToChar(c), true, this);
            }
            consume();
        }

        public virtual void matchRange(int c1, int c2)
        {
            if (cached_LA1 < c1 || cached_LA1 > c2)
            {
                throw new MismatchedCharException(cached_LA1, Convert.ToChar(c1), Convert.ToChar(c2), false, this);
            }
            consume();
        }

        public virtual void matchRange(char c1, char c2)
        {
            matchRange(c1, (int)c2);
        }

        public virtual void newline()
        {
            inputState.line++;
            inputState.column = 1;
        }

        /*advance the current column number by an appropriate amount
        *  according to tab size. This method is called from consume().
        */
        public virtual void tab()
        {
            int c = getColumn();
            int nc = (((c - 1) / tabsize) + 1) * tabsize + 1; // calculate tab stop
            setColumn(nc);
        }

        public virtual void setTabSize(int size)
        {
            tabsize = size;
        }

        public virtual int getTabSize()
        {
            return tabsize;
        }

        public virtual void panic()
        {
            //Console.Error.WriteLine("CharScanner: panic");
            //Environment.Exit(1);
            panic("");

        }

        /// <summary>
        /// This method is executed by ANTLR internally when it detected an illegal
        /// state that cannot be recovered from.
        /// The previous implementation of this method called <see cref="Environment.Exit"/>
        /// and writes directly to <see cref="Console.Error"/>, which is usually not 
        /// appropriate when a translator is embedded into a larger application.
        /// </summary>
        /// <param name="s">Error message.</param>
        public virtual void panic(string s)
        {
            throw new ANTLRPanicException("CharScanner::panic: " + s);
        }

        /*Parser error-reporting function can be overridden in subclass */
        public virtual void reportError(RecognitionException ex)
        {
            Console.Error.WriteLine(ex);
        }

        /*Parser error-reporting function can be overridden in subclass */
        public virtual void reportError(string s)
        {
            if (getFilename() == null)
            {
                Console.Error.WriteLine("error: " + s);
            }
            else
            {
                Console.Error.WriteLine(getFilename() + ": error: " + s);
            }
        }

        /*Parser warning-reporting function can be overridden in subclass */
        public virtual void reportWarning(string s)
        {
            if (getFilename() == null)
            {
                Console.Error.WriteLine("warning: " + s);
            }
            else
            {
                Console.Error.WriteLine(getFilename() + ": warning: " + s);
            }
        }

        public virtual void refresh()
        {
            cached_LA2 = inputState.input.LA(2);
            cached_LA1 = inputState.input.LA(1);
        }

        public virtual void resetText()
        {
            text.Length = 0;
            inputState.tokenStartColumn = inputState.column;
            inputState.tokenStartLine = inputState.line;
        }

        /*This method is called by YourLexer.nextToken() when the lexer has
        *  hit EOF condition.  EOF is NOT a character.
        *  This method is not called if EOF is reached during
        *  syntactic predicate evaluation or during evaluation
        *  of normal lexical rules, which presumably would be
        *  an IOException.  This traps the "normal" EOF condition.
        *
        *  uponEOF() is called after the complete evaluation of
        *  the previous token and only if your parser asks
        *  for another token beyond that last non-EOF token.
        *
        *  You might want to throw token or char stream exceptions
        *  like: "Heh, premature eof" or a retry stream exception
        *  ("I found the end of this file, go back to referencing file").
        */
        public virtual void uponEOF()
        {
        }
    }
}