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
using System.Collections.Specialized;
using System.Text;
using ActiveUp.Net.Security.OpenPGP.Packets;

namespace ActiveUp.Net.Security.OpenPGP
{
    /// <summary>
    /// ASCIIArmoredMessage Class
    /// </summary>
    public class ASCIIArmoredMessage
    {
        private List<Packet> _packets = new List<Packet>();
        private ASCIIArmoredMessageType _type;
        private NameValueCollection _headers = new NameValueCollection();
        private string _crcbase64;

        public string CRCBase64
        {
            get
            {
                return this._crcbase64;
            }
            set
            {
                this._crcbase64 = value;
            }
        }
        public string Version
        {
            get
            {
                if (this.Headers["version"] == null) return string.Empty;
                else return this.Headers["version"];
            }
            set 
            {
                if (this.Headers["version"] == null) this.Headers.Add("version", value);
                else this.Headers["version"] = value;
            }
        }
        public string Comment
        {
            get
            {
                if (this.Headers["comment"] == null) return string.Empty;
                else return this.Headers["comment"];
            }
            set
            {
                if (this.Headers["comment"] == null) this.Headers.Add("comment", value);
                else this.Headers["comment"] = value;
            }
        }
        public string MessageID
        {
            get
            {
                if (this.Headers["messageid"] == null) return string.Empty;
                else return this.Headers["messageid"];
            }
            set
            {
                if (this.Headers["messageid"] == null) this.Headers.Add("messageid", value);
                else this.Headers["messageid"] = value;
            }
        }
        public string Charset
        {
            get
            {
                if (this.Headers["charset"] == null) return string.Empty;
                else return this.Headers["charset"];
            }
            set
            {
                if (this.Headers["charset"] == null) this.Headers.Add("charset", value);
                else this.Headers["charset"] = value;
            }
        }
        public List<Packet> Packets
        {
            get { return this._packets; }
            set { this._packets = value; }
        }
        public ASCIIArmoredMessageType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }
        public NameValueCollection Headers
        {
            get { return this._headers; }
            set { this._headers = value; }
        }
    }
}
