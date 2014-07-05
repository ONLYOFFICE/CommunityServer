// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Security.OpenPGP
{
    /// <summary>
    /// Notation Class
    /// </summary>
    public class Notation
    {
        public Notation(byte[] name, byte[] value, bool isHumanReadable)
        {
            this._name = name;
            this._value = value;
            this._isHumanReadable = isHumanReadable;
        }
        public Notation()
        {

        }
        
        private byte[] _name, _value;
        private bool _isHumanReadable;

        public byte[] Name
        {
            get { return this._name; }
            set { this._name = value; }
        }
        public byte[] Value
        {
            get { return this._value; }
            set { this._value = value; }
        }
        public bool IsHumanReadable
        {
            get { return this._isHumanReadable; }
            set { this._isHumanReadable = value; }
        }
    }
}
