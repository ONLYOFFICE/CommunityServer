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
using System.Text.RegularExpressions;

namespace ActiveUp.Net.Security
{
    public class PublicKeyRecord
    {
        public static PublicKeyRecord Parse(string input)
        {
            PublicKeyRecord record = new PublicKeyRecord();
            MatchCollection matches = Regex.Matches(input, @"[a-zA-Z]+=[^;]+(?=(;|\Z))");
            foreach (Match m in matches)
            {
                string tag = m.Value.Substring(0, m.Value.IndexOf('='));
                string value = m.Value.Substring(m.Value.IndexOf('=') + 1);
                if (tag.Equals("n")) record._n = value;
                else if (tag.Equals("p"))
                {
                    value = value.Trim('\r', '\n').Replace(" ", "");
                    while ((value.Length % 4) != 0) value += "=";
                    record._p64 = value;
                    record._p = Convert.FromBase64String(record._p64);
                }
                else if (tag.Equals("k"))
                {
                    if (value.Equals("rsa")) record._k = KeyType.Rsa;
                }
                else if (tag.Equals("g")) record._g = value;
                else if (tag.Equals("t")) record._t = value.Equals("y");
            }
            return record;
        }

        private string _n, _p64, _g;
        private byte[] _p;
        private bool _t;
        private KeyType _k;

        public string Granularity
        {
            get { return this._g; }
            set {this._g = value; }
        }
        public KeyType KeyType
        {
            get { return this._k; }
            set { this._k = value; }
        }
        public string Notes
        {
            get { return this._n; }
            set {this._n = value; }
        }
        public string KeyDataBase64
        {
            get { return this._p64; }
            set {this._p64 = value; }
        }
        public byte[] KeyData
        {
            get { return this._p; }
            set {this._p = value; }
        }
        public bool InTestMode
        {
            get { return this._t; }
            set {this._t = value; }
        }
    }
}
