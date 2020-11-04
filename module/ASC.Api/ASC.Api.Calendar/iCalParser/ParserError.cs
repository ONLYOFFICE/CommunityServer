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

namespace ASC.Api.Calendar.iCalParser
{
    /// <summary>
    /// Basic error handling mechanism for the Parser
    /// </summary>
    public class ParserError
    {
        private int linenumber;
        private string message;

        public ParserError(int _linenumber, string _message)
        {
            linenumber = _linenumber;
            message = _message;
        }

        public int Linenumber
        {
            get { return linenumber; }
        }

        public string Message
        {
            get { return message; }
        }

        public override string ToString()
        {
            return "Error on line: " + linenumber + ".  " + message;
        }
    }
}
