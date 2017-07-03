// $ANTLR 2.7.6 (20061021): "iCal.g" -> "iCalLexer.cs"$

using System.IO;
using antlr;
using antlr.collections;

namespace Ical.Net.Serialization.iCalendar
{
    // Generate header specific to lexer CSharp file

    public class iCalLexer : CharScanner
    {
        public const int EOF = 1;
        public const int NULL_TREE_LOOKAHEAD = 3;
        public const int CRLF = 4;
        public const int BEGIN = 5;
        public const int COLON = 6;
        public const int VCALENDAR = 7;
        public const int END = 8;
        public const int IANA_TOKEN = 9;
        public const int X_NAME = 10;
        public const int SEMICOLON = 11;
        public const int EQUAL = 12;
        public const int COMMA = 13;
        public const int DQUOTE = 14;
        public const int CTL = 15;
        public const int BACKSLASH = 16;
        public const int NUMBER = 17;
        public const int DOT = 18;
        public const int CR = 19;
        public const int LF = 20;
        public const int ALPHA = 21;
        public const int DIGIT = 22;
        public const int DASH = 23;
        public const int UNDERSCORE = 24;
        public const int UNICODE = 25;
        public const int SPECIAL = 26;
        public const int SPACE = 27;
        public const int HTAB = 28;
        public const int SLASH = 29;
        public const int ESCAPED_CHAR = 30;
        public const int LINEFOLDER = 31;

        public iCalLexer(Stream ins) : this(new ByteBuffer(ins)) {}

        public iCalLexer(TextReader r) : this(new CharBuffer(r)) {}

        public iCalLexer(InputBuffer ib) : this(new LexerSharedInputState(ib)) {}

        public iCalLexer(LexerSharedInputState state) : base(state)
        {
        }

        public override IToken nextToken() //throws TokenStreamException
        {
            tryAgain:
            for (;;)
            {
                resetText();
                try // for char stream error handling
                {
                    try // for lexical error handling
                    {
                        switch (cached_LA1)
                        {
                            case '\n':
                            {
                                mLF(true);
                                break;
                            }
                            case ' ':
                            {
                                mSPACE(true);
                                break;
                            }
                            case '\t':
                            {
                                mHTAB(true);
                                break;
                            }
                            case ':':
                            {
                                mCOLON(true);
                                break;
                            }
                            case ';':
                            {
                                mSEMICOLON(true);
                                break;
                            }
                            case ',':
                            {
                                mCOMMA(true);
                                break;
                            }
                            case '.':
                            {
                                mDOT(true);
                                break;
                            }
                            case '=':
                            {
                                mEQUAL(true);
                                break;
                            }
                            case '/':
                            {
                                mSLASH(true);
                                break;
                            }
                            case '"':
                            {
                                mDQUOTE(true);
                                break;
                            }
                            default:
                                if ((cached_LA1 == '\r') && (cached_LA2 == '\n') && (LA(3) == '\t' || LA(3) == ' '))
                                {
                                    mLINEFOLDER(true);
                                }
                                else if ((cached_LA1 == '\r') && (cached_LA2 == '\n'))
                                {
                                    mCRLF(true);
                                }
                                else if ((cached_LA1 == '\\') && tokenSet_0_.member(cached_LA2))
                                {
                                    mESCAPED_CHAR(true);
                                }
                                else if (cached_LA1 == '\\')
                                {
                                    mBACKSLASH(true);
                                }
                                else if (tokenSet_1_.member(cached_LA1))
                                {
                                    mCTL(true);
                                }
                                else if (tokenSet_2_.member(cached_LA1))
                                {
                                    mIANA_TOKEN(true);
                                }
                                else
                                {
                                    if (cached_LA1 == EOF_CHAR)
                                    {
                                        uponEOF();
                                        returnToken_ = makeToken(Token.EOF_TYPE);
                                    }
                                    else
                                    {
                                        throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                                    }
                                }
                                break;
                        }
                        if (null == returnToken_)
                        {
                            goto tryAgain; // found SKIP token
                        }
                        return returnToken_;
                    }
                    catch (RecognitionException e)
                    {
                        throw new TokenStreamRecognitionException(e);
                    }
                }
                catch (CharStreamException cse)
                {
                    if (cse is CharStreamIOException)
                    {
                        throw new TokenStreamIOException(((CharStreamIOException) cse).io);
                    }
                    throw new TokenStreamException(cse.Message);
                }
            }
        }

        protected void mCR(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = CR;

            match('\u000d');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mLF(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;

            match('\u000a');
            var _ttype = Token.SKIP;
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        protected void mALPHA(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = ALPHA;

            switch (cached_LA1)
            {
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                {
                    matchRange('\u0041', '\u005a');
                    break;
                }
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                {
                    matchRange('\u0061', '\u007a');
                    break;
                }
                default:
                {
                    throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                }
            }
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        protected void mDIGIT(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = DIGIT;

            matchRange('\u0030', '\u0039');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        protected void mDASH(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = DASH;

            match('\u002d');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        protected void mUNDERSCORE(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = UNDERSCORE;

            match('\u005F');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        protected void mUNICODE(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = UNICODE;

            matchRange('\u0100', '\uFFFE');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        protected void mSPECIAL(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = SPECIAL;

            switch (cached_LA1)
            {
                case '!':
                {
                    match('\u0021');
                    break;
                }
                case '#':
                case '$':
                case '%':
                case '&':
                case '\'':
                case '(':
                case ')':
                case '*':
                case '+':
                {
                    matchRange('\u0023', '\u002b');
                    break;
                }
                case '<':
                {
                    match('\u003c');
                    break;
                }
                case '>':
                case '?':
                case '@':
                {
                    matchRange('\u003e', '\u0040');
                    break;
                }
                case '[':
                {
                    match('\u005b');
                    break;
                }
                case ']':
                case '^':
                {
                    matchRange('\u005d', '\u005e');
                    break;
                }
                case '`':
                {
                    match('\u0060');
                    break;
                }
                case '{':
                case '|':
                case '}':
                case '~':
                {
                    matchRange('\u007b', '\u007e');
                    break;
                }
                default:
                    if (cached_LA1 >= '\u0080' && cached_LA1 <= '\u00ff')
                    {
                        matchRange('\u0080', '\u00ff');
                    }
                    else
                    {
                        throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                    }
                    break;
            }
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mSPACE(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = SPACE;

            match('\u0020');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mHTAB(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = HTAB;

            match('\u0009');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mCOLON(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = COLON;

            match('\u003a');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mSEMICOLON(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = SEMICOLON;

            match('\u003b');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mCOMMA(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = COMMA;

            match('\u002c');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mDOT(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = DOT;

            match('\u002e');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mEQUAL(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = EQUAL;

            match('\u003d');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mBACKSLASH(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = BACKSLASH;

            match('\u005c');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mSLASH(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = SLASH;

            match('\u002f');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mDQUOTE(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = DQUOTE;

            match('\u0022');
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mCRLF(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = CRLF;

            mCR(false);
            mLF(false);
            newline();
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mCTL(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = CTL;

            switch (cached_LA1)
            {
                case '\u0000':
                case '\u0001':
                case '\u0002':
                case '\u0003':
                case '\u0004':
                case '\u0005':
                case '\u0006':
                case '\u0007':
                case '\u0008':
                {
                    matchRange('\u0000', '\u0008');
                    break;
                }
                case '\u000b':
                case '\u000c':
                case '\r':
                case '\u000e':
                case '\u000f':
                case '\u0010':
                case '\u0011':
                case '\u0012':
                case '\u0013':
                case '\u0014':
                case '\u0015':
                case '\u0016':
                case '\u0017':
                case '\u0018':
                case '\u0019':
                case '\u001a':
                case '\u001b':
                case '\u001c':
                case '\u001d':
                case '\u001e':
                case '\u001f':
                {
                    matchRange('\u000b', '\u001F');
                    break;
                }
                case '\u007f':
                {
                    match('\u007F');
                    break;
                }
                default:
                {
                    throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                }
            }
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mESCAPED_CHAR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = ESCAPED_CHAR;

            mBACKSLASH(false);
            {
                switch (cached_LA1)
                {
                    case '\\':
                    {
                        mBACKSLASH(false);
                        break;
                    }
                    case '"':
                    {
                        mDQUOTE(false);
                        break;
                    }
                    case ';':
                    {
                        mSEMICOLON(false);
                        break;
                    }
                    case ',':
                    {
                        mCOMMA(false);
                        break;
                    }
                    case 'N':
                    {
                        match("N");
                        break;
                    }
                    case 'n':
                    {
                        match("n");
                        break;
                    }
                    default:
                    {
                        throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                    }
                }
            }
            if (_createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mIANA_TOKEN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;
            var _ttype = IANA_TOKEN;

            { // ( ... )+
                var _cnt82 = 0;
                for (;;)
                {
                    switch (cached_LA1)
                    {
                        case 'A':
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'E':
                        case 'F':
                        case 'G':
                        case 'H':
                        case 'I':
                        case 'J':
                        case 'K':
                        case 'L':
                        case 'M':
                        case 'N':
                        case 'O':
                        case 'P':
                        case 'Q':
                        case 'R':
                        case 'S':
                        case 'T':
                        case 'U':
                        case 'V':
                        case 'W':
                        case 'X':
                        case 'Y':
                        case 'Z':
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                        case 'g':
                        case 'h':
                        case 'i':
                        case 'j':
                        case 'k':
                        case 'l':
                        case 'm':
                        case 'n':
                        case 'o':
                        case 'p':
                        case 'q':
                        case 'r':
                        case 's':
                        case 't':
                        case 'u':
                        case 'v':
                        case 'w':
                        case 'x':
                        case 'y':
                        case 'z':
                        {
                            mALPHA(false);
                            break;
                        }
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        {
                            mDIGIT(false);
                            break;
                        }
                        case '-':
                        {
                            mDASH(false);
                            break;
                        }
                        case '_':
                        {
                            mUNDERSCORE(false);
                            break;
                        }
                        default:
                            if (tokenSet_3_.member(cached_LA1))
                            {
                                mSPECIAL(false);
                            }
                            else if (cached_LA1 >= '\u0100' && cached_LA1 <= '\ufffe')
                            {
                                mUNICODE(false);
                            }
                            else
                            {
                                if (_cnt82 >= 1)
                                {
                                    goto _loop82_breakloop;
                                }
                                throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                            }
                            break;
                    }
                    _cnt82++;
                }
                _loop82_breakloop:
                ;
            } // ( ... )+

            var s = text.ToString(_begin, text.Length - _begin);
            int val;
            if (int.TryParse(s, out val))
            {
                _ttype = NUMBER;
            }
            else
            {
                switch (s.ToUpper())
                {
                    case "BEGIN":
                        _ttype = BEGIN;
                        break;
                    case "END":
                        _ttype = END;
                        break;
                    case "VCALENDAR":
                        _ttype = VCALENDAR;
                        break;
                    default:
                        if (s.Length > 2 && s.Substring(0, 2).Equals("X-"))
                        {
                            _ttype = X_NAME;
                        }
                        break;
                }
            }

            if (_createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }

        public void mLINEFOLDER(bool createToken) //throws RecognitionException, CharStreamException, TokenStreamException
        {
            IToken _token = null;
            var _begin = text.Length;

            mCRLF(false);
            {
                switch (cached_LA1)
                {
                    case ' ':
                    {
                        mSPACE(false);
                        break;
                    }
                    case '\t':
                    {
                        mHTAB(false);
                        break;
                    }
                    default:
                    {
                        throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
                    }
                }
            }
            var _ttype = Token.SKIP;
            if (createToken && (_ttype != Token.SKIP))
            {
                _token = makeToken(_ttype);
                _token.setText(text.ToString(_begin, text.Length - _begin));
            }
            returnToken_ = _token;
        }


        private static long[] mk_tokenSet_0_()
        {
            var data = new long[1025];
            data[0] = 576478361669337088L;
            data[1] = 70369012629504L;
            for (var i = 2; i <= 1024; i++)
            {
                data[i] = 0L;
            }
            return data;
        }

        public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());

        private static long[] mk_tokenSet_1_()
        {
            var data = new long[1025];
            data[0] = 4294965759L;
            data[1] = -9223372036854775808L;
            for (var i = 2; i <= 1024; i++)
            {
                data[i] = 0L;
            }
            return data;
        }

        public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());

        private static long[] mk_tokenSet_2_()
        {
            var data = new long[2560];
            data[0] = -3170762861857210368L;
            data[1] = 9223372036586340351L;
            for (var i = 2; i <= 1022; i++)
            {
                data[i] = -1L;
            }
            data[1023] = 9223372036854775807L;
            for (var i = 1024; i <= 2559; i++)
            {
                data[i] = 0L;
            }
            return data;
        }

        public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());

        private static long[] mk_tokenSet_3_()
        {
            var data = new long[1025];
            data[0] = -3458746947404300288L;
            data[1] = 8646911290591150081L;
            for (var i = 2; i <= 3; i++)
            {
                data[i] = -1L;
            }
            for (var i = 4; i <= 1024; i++)
            {
                data[i] = 0L;
            }
            return data;
        }

        public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
    }
}