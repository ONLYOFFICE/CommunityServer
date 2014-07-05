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

namespace ActiveUp.Net.Security.OpenPGP.Packets
{
    /// <summary>
    /// Packet Class
    /// </summary>
    public class Packet
    {
        private byte[] _rawData;
        private PacketType _type;
        private PacketFormat _format;
        private int _bodyLength, _totalLength, _tempPosition;

        public byte[] RawData
        {
            get { return this._rawData; }
            set { this._rawData = value; }
        }
        public PacketType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }
        public PacketFormat Format
        {
            get { return this._format; }
            set { this._format = value; }
        }
        public int BodyLength
        {
            get { return this._bodyLength; }
            set { this._bodyLength = value; }
        }
        public int TotalLength
        {
            get { return this._totalLength; }
            set { this._totalLength = value; }
        }
        internal int TempPosition
        {
            get { return this._tempPosition; }
            set { this._tempPosition = value; }
        }
    }
}
