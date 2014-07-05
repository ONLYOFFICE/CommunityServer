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
    /// RevocationKey Class
    /// </summary>
    public class RevocationKey
    {
        public RevocationKey(RevocationKeyClass clas, HashAlgorithm hashAlgorithm, byte[] fingerPrint)
        {
            this._hashAlg = hashAlgorithm;
            this._fingerPrint = fingerPrint;
            this._class = clas;
        }
        public RevocationKey()
        {

        }

        private RevocationKeyClass _class;
        private HashAlgorithm _hashAlg;
        private byte[] _fingerPrint;

        public RevocationKeyClass Class
        {
            get { return this._class; }
            set { this._class = value; }
        }
        public HashAlgorithm HashAlgorithm
        {
            get { return this._hashAlg; }
            set { this._hashAlg = value; }
        }
        public byte[] FingerPrint
        {
            get { return this._fingerPrint; }
            set { this._fingerPrint = value; }
        }
    }
}
