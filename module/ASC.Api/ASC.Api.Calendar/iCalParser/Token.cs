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


using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Globalization;

namespace ASC.Api.Calendar.iCalParser
{
    // TODO:  remove the reserved keywords from the Token Base Class - this will allow
    // generic parsing/scanning.

    // note that order is important in this enum type simple tokens are '<= Equals' and 
    // reserved words are '>= Tcalscale' - furthermore the reserved words are broken down into
    // a set of ranges that indicate similar 'types' of tokens (ie. the parser will treat the
    // objects in the range similarily) - this means that if new values are inserted into the
    // ranges, they should occur between the two tokens on each end.  For example to add a new
    // keyword to the symbolic property range, it must be lexically after 'Tcalscale' and lexically
    // before 'Tcutype', otherwise it will not be recognized by the 'isSymbolicProperty()' method.
    //
    // note that there is convenience methods on the token class for these classifications
    // (ie. isResourceProperty() isMailtoProperty() etc)
    public enum TokenValue
    {
        SemiColon = 0, Colon = 1, Comma = 2, Hyphen = 3, CRLF = 4, Equals = 5, // simple tokens
        QuotedString = 6, Value = 7, Xtension = 8, ID = 11, Parm = 12, Error = 13,  // general tokens
        // reserved words	
        Tcalscale, Taction, Tclass, Ttransp, Tstart, Tpartstat, Trsvp, Trole, Tcutype,  // this range is the Symbolic Properties
        Tstandard, Tdaylight, Tvalarm, Ttrigger, // this range is the resource properties
        Tattendee, Torganizer, // this range is the 'mailto:' type properties
        Tbegin, Tend, Tvalue, TrecurrenceId,
        Tvcalendar, // this range is the BEGINEND keyword properties (up to Tvtimezone)
        Tvevent, Tvtodo, Tvjournal, Tvfreebusy, Tvtimezone,  // these are a subproperty of BEGINEND which are COMPONENTs
        Tdtstart, Tdtstamp, Tdtend, Trrule, Texdate, //  this range are the ValueProperties
    };

    /// <summary>
    /// Represents the individual tokens returned from the scanner to the parser.  Note that the
    /// Token creation process is sensitive to the ScannerState.  This state is defined by what context
    /// the scanner currently is in - Parsing IDs, Parmeters, or values:
    ///   e.g.  the iCalendar grammar defines the following possible states
    ///   id;id=parm:value
    /// each string parsed out of the value has to be treated differently (eg. quoted strings are
    /// allowed in 'parm' but not in 'id')
    /// 
    /// </summary>
    public class Token
    {
        private string tokenText;
        private TokenValue tokenVal;
        private ScannerState state;
        private string errorMessage;

        private static Hashtable reservedWords;

        static Token()
        {
            // static initialization for reserved words - for quick parsing
            reservedWords = new Hashtable();
            //reservedWords[ "calscale" ] = TokenValue.Tcalscale;
            reservedWords["action"] = TokenValue.Taction;
            reservedWords["class"] = TokenValue.Tclass;
            reservedWords["transp"] = TokenValue.Ttransp;
            reservedWords["start"] = TokenValue.Tstart;
            reservedWords["partstat"] = TokenValue.Tpartstat;
            reservedWords["rsvp"] = TokenValue.Trsvp;
            reservedWords["role"] = TokenValue.Trole;
            reservedWords["cutype"] = TokenValue.Tcutype;
            reservedWords["standard"] = TokenValue.Tstandard;
            reservedWords["daylight"] = TokenValue.Tdaylight;
            reservedWords["valarm"] = TokenValue.Tvalarm;
            reservedWords["trigger"] = TokenValue.Ttrigger;
            reservedWords["attendee"] = TokenValue.Tattendee;
            reservedWords["organizer"] = TokenValue.Torganizer;
            reservedWords["begin"] = TokenValue.Tbegin;
            reservedWords["end"] = TokenValue.Tend;
            reservedWords["vevent"] = TokenValue.Tvevent;
            reservedWords["vtodo"] = TokenValue.Tvtodo;
            reservedWords["vjournal"] = TokenValue.Tvjournal;
            reservedWords["vfreebusy"] = TokenValue.Tvfreebusy;
            reservedWords["vtimezone"] = TokenValue.Tvtimezone;
            reservedWords["vcalendar"] = TokenValue.Tvcalendar;
            reservedWords["dtstart"] = TokenValue.Tdtstart;
            reservedWords["dtend"] = TokenValue.Tdtend;
            reservedWords["rrule"] = TokenValue.Trrule;
            reservedWords["exdate"] = TokenValue.Texdate;
            reservedWords["value"] = TokenValue.Tvalue;
            reservedWords["dtstamp"] = TokenValue.Tdtstamp;
            reservedWords["recurrence-id"] = TokenValue.TrecurrenceId;
        }

        public static bool isID(string str)
        {
            return Regex.IsMatch(str, @"[a-zA-Z][a-zA-Z0-9_]*");
        }

        public static string CapsCamelCase(string str)
        {
            if (str.Length > 0)
            {
                return CamelCase(str.Substring(0, 1).ToUpper() + str.Substring(1));
            }
            else
            {
                return CamelCase(str);
            }
        }

        public static string CamelCase(string str)
        {
            bool upper = false;
            char[] lstr = str.ToCharArray();
            StringBuilder buff = new StringBuilder();

            for (int i = 0; i < lstr.Length; ++i)
            {
                if (lstr[i] == '-')
                {
                    upper = true;
                }
                else
                {
                    if (upper)
                    {
                        buff.Append(Char.ToUpper(lstr[i]));
                        upper = false;
                    }
                    else
                    {
                        buff.Append(lstr[i]);
                    }
                }
            }
            return buff.ToString();
        }

        public static DateTime ParseDateTime(string icalDate, out bool isDate, out bool isUTC)
        {
            var _dateTimeFormats = new[]
                                                       {
                                                           "o",
                                                           "yyyyMMdd'T'HHmmssK",                                                            
                                                           "yyyyMMdd",
                                                           "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'.'fffffffK", 
                                                           "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'.'fffK", 
                                                           "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
                                                           "yyyy'-'MM'-'dd'T'HH'-'mm'-'ssK",
                                                           "yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
                                                           "yyyy'-'MM'-'dd"
                                                       };




            
            
            isUTC= icalDate.ToLowerInvariant().EndsWith("z");
            isDate = !icalDate.ToLowerInvariant().Contains("t");


            DateTime dateTime ;
            if(DateTime.TryParseExact(icalDate.ToUpper(), _dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dateTime))
                return dateTime;

            return DateTime.MaxValue;
        }

        public static DateTime ParseDate(string icalDate)
        {
            bool isDate, isUTC;
            return ParseDateTime(icalDate, out isDate, out isUTC);
        }

        public Token(string _tokenText, ScannerState _state)
            : this(_tokenText, _state, false)
        {
        }

        public Token(string _tokenText)
            : this(_tokenText, ScannerState.ParseValue, false)
        {
        }

        public Token(string _tokenText, ScannerState _state, bool quoteFlag)
        {
            state = _state;

            if (_tokenText == null)//|| _tokenText.Length == 0 )
            {
                //tokenVal = TokenValue.Error;
                //errorMessage = "Bad Token String - 0 length";
                _tokenText = "";
            }
            //else
            //{
            switch (state)
            {
                case ScannerState.ParseID:
                    tokenText = _tokenText.ToLowerInvariant();
                    if (reservedWords.Contains(tokenText))
                    {
                        tokenVal = (TokenValue)reservedWords[tokenText];
                    }
                    else if (tokenText.StartsWith("x-"))
                    {
                        tokenVal = TokenValue.Xtension;
                        tokenText = "x:" + tokenText.Substring(2);
                    }
                    else if (isID(tokenText))  // this check may be unnecessary by virute of the scanner....
                    {
                        tokenVal = TokenValue.ID;
                    }
                    else
                    {
                        tokenVal = TokenValue.Error;
                        errorMessage = "Illegal value for ID";
                    }
                    if (isBeginEndValue())
                    {
                        tokenText = CapsCamelCase(tokenText);
                    }
                    else
                    {
                        tokenText = CamelCase(tokenText);
                    }
                    break;

                case ScannerState.ParseParms:
                    tokenText = _tokenText;
                    if (quoteFlag)
                    {
                        tokenVal = TokenValue.QuotedString;
                    }
                    else
                    {
                        tokenVal = TokenValue.Parm;
                    }
                    break;

                case ScannerState.ParseValue:
                    tokenText = _tokenText;
                    tokenVal = TokenValue.Value;
                    break;

                case ScannerState.ParseSimple:
                    tokenVal = TokenValue.Error;
                    errorMessage = "Bad constructor call - ParseSimple and text...";
                    break;
            }
            //}
        }

        public Token(TokenValue _tokenVal)
        {
            tokenText = null;
            if (_tokenVal <= TokenValue.Equals)
            {
                tokenVal = _tokenVal;
            }
            else
            {
                tokenVal = TokenValue.Error;
            }
        }

        public bool isError()
        {
            return tokenVal == TokenValue.Error;
        }

        public bool isSymbolicProperty()
        {
            return tokenVal >= TokenValue.Tcalscale && tokenVal <= TokenValue.Tcutype;
        }

        public bool isResourceProperty()
        {
            return tokenVal >= TokenValue.Tstandard && tokenVal <= TokenValue.Ttrigger;
        }

        public bool isMailtoProperty()
        {
            return tokenVal >= TokenValue.Tattendee && tokenVal <= TokenValue.Torganizer;
        }

        public bool isBeginEndValue()
        {
            return (tokenVal >= TokenValue.Tvcalendar && tokenVal <= TokenValue.Tvtimezone) || tokenVal == TokenValue.Tvalarm;
        }

        public bool isComponent()
        {
            return tokenVal >= TokenValue.Tvevent && tokenVal <= TokenValue.Tvtimezone;
        }

        public bool isValueProperty()
        {
            return (tokenVal >= TokenValue.Tdtstart && tokenVal <= TokenValue.Texdate) || tokenVal == TokenValue.Ttrigger;
        }

        public TokenValue TokenVal
        {
            get { return tokenVal; }
        }

        public string TokenText
        {
            get { return tokenText; }
        }

        public string Error
        {
            get { return errorMessage; }
        }

        public void FormatDateTime()
        {
        }
    }
}
