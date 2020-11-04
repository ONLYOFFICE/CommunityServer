/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


/*
 *   ICalParser is a general purpose .Net parser for iCalendar format files (RFC 2445)
 * 
 *   Copyright (C) 2004  J. Tim Spurway
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program; if not, write to the Free Software
 *   Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace ASC.Api.Calendar.iCalParser
{
    public enum ScannerState { ParseSimple, ParseID, ParseParms, ParseValue, ParseKey };

    /// <summary>
    /// The scanner is responsible for tokenizing iCalendar (RFC2445) files for use by the
    /// parser.  
    /// </summary>
    public class Scanner : IEnumerable
    {
        private string fileName = null;
        private Stream stream = null;
        private TextReader rdr = null;
        private int lineNumber = -1;
        private bool newlineFound = false;

        private class Enumerator : IEnumerator
        {
            Scanner scanner;
            Token current;

            internal Enumerator(Scanner _scanner)
            {
                scanner = _scanner;
                current = null;
            }

            public bool MoveNext()
            {
                current = scanner.GetNextToken();
                return current != null;
            }

            public object Current
            {
                get { return current; }
            }

            public void Reset()
            {
                scanner.Close();
            }
        }

        public Scanner(string _fileName)
        {
            fileName = _fileName;
        }

        public Scanner(Stream _stream)
        {
            stream = _stream;
        }

        public Scanner(TextReader _reader)
        {
            rdr = _reader;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public Token GetNextToken()
        {
            return GetNextToken(ScannerState.ParseValue);
        }

        /// <summary>
        /// Returns the next token in the file.  Returns null on EOF.
        /// </summary>
        /// <returns></returns>
        public Token GetNextToken(ScannerState state)
        {

            switch (state)
            {
                case ScannerState.ParseKey:
                    eatWhitespace();
                    return GetNextID();
                case ScannerState.ParseID:
                    return GetNextID();
                case ScannerState.ParseParms:
                    return GetNextParms();
                case ScannerState.ParseSimple:
                    eatWhitespace();
                    return GetNextSimple();
                case ScannerState.ParseValue:
                    Token rval = GetNextValue();
                    //ConsumeToEOL();
                    return rval;
            }
            return null;
        }

        public Token GetNextID()
        {
            StringBuilder buff = new StringBuilder();
            char c;
            int i;

            if ((i = Peek()) == -1)
                return null;
            else
                c = (char)i;

            if (Char.IsLetter(c))
            {
                buff.Append(c);
                Read();
                goto getID;
            }
            return null;  // IDs need to start with a letter

            getID:
            if ((i = Peek()) == -1)
                return new Token(buff.ToString(), ScannerState.ParseID);
            else
                c = (char)i;
            if (Char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                // and may be composed of letter, digit, _, - characters
                buff.Append(c);
                Read();
                goto getID;
            }
            else if (c == '\n')
            {
                Read(); // consume it and go
            }
            return new Token(buff.ToString(), ScannerState.ParseID);
        }

        public Token GetNextParms()
        {
            StringBuilder buff = new StringBuilder();
            char c;
            int i;

            if ((i = Peek()) == -1)
                return null;

        start:
            if ((i = Peek()) == -1)
                return new Token(buff.ToString(), ScannerState.ParseParms);
            else
                c = (char)i;

            if (c == ';' || c == ':')
            {
                return new Token(buff.ToString(), ScannerState.ParseParms);
            }
            else if (c == '\n')
            {
                // parameters must end in a semi-colon or colon
                return null;
            }
            else if (c == '"')
            {
                buff.Append(c);
                Read();
                goto quote;
            }
            else
            {
                buff.Append(c);
                Read();
                goto start;
            }

        quote:
            if ((i = Peek()) == -1)
                return null;
            else
                c = (char)i;

            if (c == '\\')
            {
                buff.Append(c);
                Read();
                goto quoteChar;
            }
            else if (c == '\n')
            {
                return null;
            }
            else if (c == '"')
            {
                buff.Append(c);
                Read();
                return new Token(buff.ToString(), ScannerState.ParseParms, true);
            }
            else
            {
                buff.Append(c);
                Read();
                goto quote;
            }

        quoteChar:
            if ((i = Read()) == -1)
                return null;
            else
                c = (char)i;

            if (c == '\n')
            {
                // can't backslash quote a newline - have to use continuation char
                return null;
            }

            buff.Append(c);
            goto quote;
        }

        public Token GetNextSimple()
        {
            char c;
            int i;

            if ((i = Peek()) == -1)
                return null;
            else
                c = (char)i;
            if (c == ';')
            {
                Read();
                return new Token(TokenValue.SemiColon);
            }
            else if (c == ':')
            {
                Read();
                return new Token(TokenValue.Colon);
            }
            else if (c == '=')
            {
                Read();
                return new Token(TokenValue.Equals);
            }
            else if (c == ',')
            {
                Read();
                return new Token(TokenValue.Comma);
            }
            else if (c == '-')
            {
                Read();
                return new Token(TokenValue.Hyphen);
            }
            else
            {
                return null;
            }
        }

        public Token GetNextValue()
        {
            StringBuilder buff = new StringBuilder();
            char c;
            int i;

            if ((i = Peek()) == -1)
                return null;

        start:
            if ((i = Peek()) == -1)
                return new Token(buff.ToString(), ScannerState.ParseValue);
            else
                c = (char)i;

            if (c == '\n')
            {
                // values always end in a newline
                Read();
                return new Token(buff.ToString(), ScannerState.ParseValue);
            }
            else if (c == '\\')
            {
                Read();  // consume the backslash - it's a quote character unused in RDF/Xml
                goto quoteChar;
            }
            else
            {
                //if( c == '\'' || c == '\\' || c == '\"' )
                //buff.Append( '\\' );
                buff.Append(c);
                Read();
                goto start;
            }

        quoteChar:
            // handle newlines and returns by putting the actual control characters
            // in the output - other quoted character (such as commas) are just dumped out
            if ((i = Read()) == -1)
                return new Token(buff.ToString(), ScannerState.ParseValue);
            else
                c = (char)i;

            if (c == '\n')
            {
                // can't backslash quote a newline - have to use continuation char
                return null;
            }
            else if (c == 'n')
            {
                buff.Append('\n');
            }
            else if (c == 'r')
            {
                buff.Append('\r');
            }
            else
            {
                buff.Append(c);
            }

            goto start;
        }

        private void eatWhitespace()
        {
            int i;
            char c;

            while (true)
            {
                if ((i = Peek()) == -1)
                    return;
                else
                    c = (char)i;

                //if( Char.IsWhiteSpace( c ))
                if (c == ' ' || c == '\t') // only eat spaces and tabs
                {
                    Read();
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// This method is used for error recovery, get the rest of the line, including
        /// folded lines, and position on a fresh line or EOF
        /// </summary>
        public void ConsumeToEOL()
        {
            int i;
            char c;

            while (true)
            {
                if ((i = Peek()) == -1)
                    return;
                else
                    c = (char)i;

                if (c == '\n')
                {
                    Read();
                    return;
                }
                Read();
            }
        }

        public bool isEOF()
        {
            return Peek() == -1;
        }

        private TextReader reader
        {
            get
            {
                if (rdr == null)
                {
                    if (stream == null)
                    {
                        // open file
                        stream = File.OpenRead(fileName);
                    }
                    // create reader
                    rdr = new StreamReader(stream, Encoding.UTF8);
                    lineNumber = 0;
                }
                return rdr;
            }
        }

        public void Close()
        {
            if (reader != null)
            {
                reader.Close();
                rdr = null;
                stream = null;
            }
        }

        public int LineNumber
        {
            get { return lineNumber; }
        }

        private void consumeFolding()
        {
            if ((char)HardPeek() == '\n')
            {
                HardRead();  // read to EOL
                newlineFound = true;

                if ((char)HardPeek() == ' ')
                {
                    // continuation char, consume it
                    newlineFound = false;
                    HardRead();
                }
            }
        }

        private int Read()
        {
            consumeFolding();
            if (newlineFound)
            {
                newlineFound = false;
                return (int)'\n';
            }
            return HardRead();
        }

        private int Peek()
        {
            consumeFolding();
            if (newlineFound)
            {
                //newlineFound = false;
                return (int)'\n';
            }
            return HardPeek();
        }

        private int HardPeek()
        {
            //while( (char)reader.Peek() == '\r' )
            while (IsControlChar(reader.Peek()))
            {
                reader.Read();
            }
            return reader.Peek();
        }

        private int HardRead()
        {
            //while( (char)reader.Peek() == '\r' )
            while (IsControlChar(reader.Peek()))
            {
                reader.Read();
            }
            return reader.Read();
        }

        private bool IsControlChar(int theChar)
        {
            // don't filtre out newlines or tabs - depends on ASCII 
            char c = (char)theChar;
            return char.IsControl(c) && c != '\n' && c != '\t';
        }
    }

}
