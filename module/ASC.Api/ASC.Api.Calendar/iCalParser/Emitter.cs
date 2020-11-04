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
    /// Defines all of the basic operations that must be implemented by parser emitters.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface IEmitter
    {
        void doIntro();
        void doOutro();
        void doEnd(Token t);
        void doResourceBegin(Token t);
        void doBegin(Token t);
        void doComponentBegin(Token t);
        void doComponent();
        void doEndComponent();
        void doID(Token t);
        void doSymbolic(Token t);
        void doResource(Token t);
        void doURIResource(Token t);
        void doMailto(Token t);
        void doValueProperty(Token t, Token iprop);
        void doIprop(Token t, Token iprop);
        void doRest(Token t, Token id);
        void doAttribute(Token t1, Token t2);
        Parser VParser { get; set; }
        void emit(string val);
    }   
}
