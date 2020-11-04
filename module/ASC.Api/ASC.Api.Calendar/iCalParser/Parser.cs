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

using System.Collections;
using System.IO;
using System.Text;

namespace ASC.Api.Calendar.iCalParser
{
    /// <summary>
    /// Parse iCalendar rfc2445 streams and convert to another format based on the emitter used.
    /// </summary>
    /// 
    /// <remarks>
    /// This class is the main entry point for the ICalParser library.  A parser is created
    /// with a TextReader that contains the iCalendar stream to be parsed, and an IEmitter, which 
    /// is used to transform the iCalendar into another format.
    /// 
    /// Each iCalendar format file is in the form:
    /// 
    ///   ID[[;attr1;attr2;attr3;...;attrn]:value]
    ///   
    /// where ID is the main keyword identifying the iCalendar entry, followed optionally by a 
    /// set of attributes and a single value.  The parser works by identifying the specific IDs,
    /// attributes and values, categorizing them based on similar 'behaviour' (as defined in the <code>Token</code>
    /// class) and passing on recognized symbols to the emitter for further processing.  
    /// 
    /// The error recovery policy of the parser is pretty simple.  When an error is detected, it is recorded,
    /// and the rest of the (possibly folded) line is read, and parsing continues.  
    /// </remarks>
    /// 
    /// <example>
    /// The following snippet will read the contents of the file 'myCalendar.ics', which the
    /// parser will expect to contain iCalendar statements, and will write the RdfICalendar
    /// equivalent to standard output.
    /// <code>
    ///	    RDFEmitter emitter = new RDFEmitter( );
    ///	    StreamReader reader = new StreamReader( "myCalendar.ics" );
    ///	    Parser parser = new Parser( reader, emitter );
    ///	    parser.Parse( );
    ///	    Console.WriteLine( emitter.Rdf );
    /// </code>
    /// </example>
    /// 
    public class Parser
    {
        ArrayList errors;
        Scanner scanner;
        Stack stack, attributes;
        IEmitter emitter;
        int linenumber;
        Token id, iprop;  // id is the current ID for the current line

        /// <summary>
        /// Create a new iCalendar parser.
        /// </summary>
        /// <param name="reader">The reader that contains the stream of text iCalendar</param>
        /// <param name="_emitter">The emitter that will transform the iCalendar elements</param>
        public Parser(TextReader reader, IEmitter _emitter)
        {
            scanner = new Scanner(reader);
            emitter = _emitter;
            emitter.VParser = this;
            errors = new ArrayList();
        }

        public ArrayList Errors
        {
            get
            {
                return errors;
            }
        }

        public string ErrorString
        {
            get
            {
                if (!HasErrors)
                {
                    return "";
                }

                StringBuilder rval = new StringBuilder();
                foreach (ParserError error in errors)
                {
                    rval.Append(error.ToString()).Append("\r\n");
                }
                return rval.ToString();
            }
        }

        public bool HasErrors
        {
            get { return errors.Count > 0; }
        }

        /*/// <summary>
        /// Give public access to the parse stack.
        /// </summary>
        private Stack VStack
        {
            get { return stack; }
        }*/

        /// <summary>
        /// Main entry point for starting the Parser.
        /// </summary>
        public void Parse()
        {
            Parse(true);
        }

        /// <summary>
        /// Alternate entry point for starting the parser.
        /// </summary>
        /// <param name="emitHandT">Indicates if the emitter should be told to emit headers
        /// and trailers before and after emitting the iCalendar body</param>
        public void Parse(bool emitHandT)
        {
            stack = new Stack();
            linenumber = 0;
            attributes = new Stack();  // a stack of key-value pairs (implemented as a stack of DitionaryEntry)

            if (emitHandT)
            {
                emitter.doIntro();
            }

            // each time through the loop will get a single (maybe folded) line
            while (true)
            {
                // check for termination condition
                if (scanner.isEOF())
                {
                    // end of file - do cleanup and go
                    break;
                }

                // empty the attribute stack and the iprop value...
                attributes.Clear();
                iprop = null;
                id = null;

                //FIXME: linenumber doesn't really keep track of actual line numbers because
                //       it is not aware of folded lines...
                linenumber++;

                //DEBUG: emit line number
                //emitter.emit( linenumber + ". " );

                if (!parseID())
                {
                    continue;
                }

                // we now have to parse a set of attributes (semi-colon separated) or
                // a value (delimited by a colon)
                Token sep = scanner.GetNextToken(ScannerState.ParseSimple);
                if (sep == null || sep.TokenVal == TokenValue.Error)
                {
                    // some kind of error - skip rest of line and continue
                    reportError(scanner, " expecting : or ; after id - found nothing.");
                    continue;
                }
                else if (sep.TokenVal == TokenValue.SemiColon)
                {
                    if (!parseAttributes(scanner))
                    {
                        continue;
                    }

                    // now we have to parse the value
                    sep = scanner.GetNextToken(ScannerState.ParseSimple);
                    if (!parseValue())
                    {
                        continue;
                    }
                }
                else if (sep.TokenVal == TokenValue.Colon)
                {
                    if (!parseValue())
                    {
                        continue;
                    }
                }
                else
                {
                    reportError(scanner, "expecting : or ; after id - found: " + sep.TokenText);
                    continue;
                }

                // now sploosh out the attributes (if any) and finish the ID tag
                while (attributes.Count > 0)
                {
                    DictionaryEntry entry = (DictionaryEntry)attributes.Pop();
                    Token key = (Token)entry.Key;
                    Token val = (Token)entry.Value;
                    emitter.doAttribute(key, val);
                }

                emitter.doEnd(id);
            }

            if (emitHandT)
            {
                emitter.doOutro();
            }
        }

        protected void reportError(Scanner s, string msg)
        {
            s.ConsumeToEOL();
            errors.Add(new ParserError(linenumber, msg));
        }

        /// <summary>
        /// Parse the first field (ID) of the line.  Returns a boolean on weather or not the
        /// method sucesfully recognized an ID.  If not, the method insures that the scanner
        /// will start at the beginning of a new line.
        /// </summary>
        /// <returns></returns>
        protected virtual bool parseID()
        {
            Token t;  // re-usable token variable
            id = scanner.GetNextToken(ScannerState.ParseID);
            if (id == null || id.TokenVal == TokenValue.Error)
            {
                // some kind of error - skip rest of line and continue
                reportError(scanner, "expecting ID - found nothing.");
                return false;
            }

            switch (id.TokenVal)
            {
                case TokenValue.Tbegin:
                    t = scanner.GetNextToken(ScannerState.ParseSimple);
                    if (t == null || t.isError() || t.TokenVal != TokenValue.Colon)
                    {
                        if (t == null)
                            reportError(scanner, " expecting : - found nothing.");
                        else
                            reportError(scanner, " expecting : - found " + t.TokenText);
                        return false;
                    }

                    t = scanner.GetNextToken(ScannerState.ParseID);
                    if (t == null || t.isError() || (!t.isBeginEndValue() && !t.isResourceProperty()))
                    {
                        if (t == null)
                            reportError(scanner, " expecting a valid beginend value - found nothing.");
                        else
                            reportError(scanner, " expecting a valid beginend value - found " + t.TokenText);
                        return false;
                    }

                    // check for the different types of begin tags
                    if (t.isResourceProperty())
                    {
                        emitter.doResourceBegin(t);
                    }
                    else if (t.isComponent())
                    {
                        emitter.doComponent();
                        emitter.doComponentBegin(t);
                    }
                    else if (t.TokenVal == TokenValue.Tvcalendar)
                    {
                        emitter.doComponentBegin(t);
                    }
                    else
                    {
                        emitter.doBegin(t);
                    }
                    stack.Push(t);  // to match up to the corresponding end value
                    //scanner.ConsumeToEOL();
                    return false;

                case TokenValue.Tend:
                    t = scanner.GetNextToken(ScannerState.ParseSimple);
                    if (t == null || t.isError() || t.TokenVal != TokenValue.Colon)
                    {
                        if (t == null)
                            reportError(scanner, " expecting : - found nothing.");
                        else
                            reportError(scanner, " expecting : - found " + t.TokenText);
                        return false;
                    }

                    t = scanner.GetNextToken(ScannerState.ParseID);
                    if (t == null || t.isError() || (!t.isBeginEndValue() && !t.isResourceProperty()))
                    {
                        if (t == null)
                            reportError(scanner, " expecting a valid beginend value - found nothing.");
                        else
                            reportError(scanner, " expecting a valid beginend value - found " + t.TokenText);
                        return false;
                    }

                    // the end is easier - ignore the last one...  
                    if (stack.Count != 0)
                    {
                        emitter.doEnd(t);
                        if (t.isComponent())
                        {
                            emitter.doEndComponent();
                        }
                        stack.Pop();
                    }
                    else
                    {
                        reportError(scanner, "stack stuff is weird - probably illformed .ics file - parsing " + id.TokenText);
                    }
                    //scanner.ConsumeToEOL();
                    return false;

                case TokenValue.Trrule:
                    emitter.doResourceBegin(id);
                    break;

                default:
                    emitter.doID(id);
                    break;
            }
            return true;
        }

        /// <summary>
        /// Parse the list of attributes - separated by ';'s.  Attributes always are in the
        /// form 'id=value' and indicate key/value pairs in the iCalendar attribute format.
        /// </summary>
        /// <returns></returns>
        protected virtual bool parseAttributes(Scanner scan)
        {
            Token key = scan.GetNextToken(ScannerState.ParseKey);
            if (key == null || key.TokenVal == TokenValue.Error)
            {
                // some kind of error - skip rest of line and continue
                if (key == null)
                    reportError(scanner, " expecting ID - found nothing.");
                else
                    reportError(scanner, " expecting ID - found " + key.TokenText);
                return false;
            }

            Token sep = scan.GetNextToken(ScannerState.ParseSimple);
            if (sep == null || sep.TokenVal != TokenValue.Equals)
            {
                // some kind of error - skip rest of line and continue
                if (sep == null)
                    reportError(scanner, " expecting = - found nothing.");
                else
                    reportError(scanner, " expecting = - found " + sep.TokenText);
                return false;
            }

            Token val = scan.GetNextToken(ScannerState.ParseParms);
            if (val == null || val.TokenVal == TokenValue.Error)
            {
                // some kind of error - skip rest of line and continue
                if (val == null)
                    reportError(scanner, " expecting parameter - found nothing.");
                else
                    reportError(scanner, " expecting parameter - found " + val.TokenText);
                return false;
            }

            if (key.TokenVal == TokenValue.Tvalue && scanner == scan)
            {
                // it's an IPROP - don't ask...
                iprop = val;
            }
            else
            {
                attributes.Push(new DictionaryEntry(key, val));
            }

            // do a recursive case to identify all of the attributes
            sep = scan.GetNextToken(ScannerState.ParseSimple);
            if (sep == null || sep.TokenVal == TokenValue.Error)
            {
                // if we are parsing an rrule - this is the line termination
                if (scanner != scan)
                {
                    return true;
                }

                // some kind of error - skip rest of line and continue
                if (sep == null)
                    reportError(scanner, " expecting : or ; - found nothing.");
                else
                    reportError(scanner, " expecting : or ; - found " + sep.TokenText);
                return false;
            }

            if (sep.TokenVal == TokenValue.Colon)
            {
                // termination case
                return true;
            }
            else if (sep.TokenVal == TokenValue.SemiColon)
            {
                // recursive case
                return parseAttributes(scan);
            }
            return true;
        }

        /// <summary>
        /// Parse the value.  The value is the last data item on a iCalendar input line.
        /// </summary>
        /// <returns></returns>
        protected virtual bool parseValue()
        {
            Token val = scanner.GetNextToken(ScannerState.ParseValue);
            if (val == null || val.TokenVal == TokenValue.Error)
            {
                // some kind of error - skip rest of line and continue
                if (val == null)
                    reportError(scanner, " expecting value - found nothing.");
                else
                    reportError(scanner, " expecting value - found " + val.TokenText);
                return false;
            }

            // the emmision of code for the value will depend on the ID for this line
            if (id.isSymbolicProperty())
            {
                emitter.doSymbolic(val);
                return false;  // because this ends the tag
            }
            else if (id.isMailtoProperty())
            {
                emitter.doMailto(val);
            }
            else if (id.isValueProperty())
            {
                if (id.TokenVal == TokenValue.Trrule)
                {
                    // this is a special case - the value will be an attribute list...
                    parseAttributes(new Scanner(new StringReader(val.TokenText)));
                }
                else
                {
                    emitter.doValueProperty(val, iprop);
                }
            }
            else if (iprop != null && id.TokenVal != TokenValue.Xtension)
            {
                if (iprop.TokenText.ToLowerInvariant() == "uri")
                {
                    // special case 
                    emitter.doURIResource(val);
                }
                else
                {
                    emitter.doIprop(val, iprop);
                }
                return false;
            }
            else
            {
                if (id.TokenVal == TokenValue.TrecurrenceId)
                    val.FormatDateTime();  // if this is a recurrence id, then format the date so that it is a legal date type for RDF and RQL
                emitter.doRest(val, id);
                return false;
            }
            return true;
        }

    }
}
