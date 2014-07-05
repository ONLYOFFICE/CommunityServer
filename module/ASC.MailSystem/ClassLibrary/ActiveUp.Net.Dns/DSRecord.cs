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

namespace ActiveUp.Net.Dns
{
    class DSRecord : IRecordData
    {
        /// <summary>
        /// Implementation Reference RFC 3658
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
         public DSRecord(DataBuffer buffer, int length)
        {
             key = buffer.ReadShortInt();
             algorithm = buffer.ReadByte();
             digestType = buffer.ReadByte();
             digest = buffer.ReadBytes(length - 4);
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Key:{0} Algorithm:{1} DigestType:{2} Digest:{3}", key, algorithm, digestType, digest);
        }

        short key;
        /// <summary>
        /// Return Record Key
        /// </summary>
        public short Key
        {
            get { return key; }
        }
        byte algorithm;
        /// <summary>
        /// Return Record Algorithm
        /// </summary>
        public byte Algorithm
        {
            get { return algorithm; }
        }
        byte digestType;
        /// <summary>
        /// return record Digest Type
        /// </summary>
        public byte DigestType
        {
            get { return digestType; }
        }
        byte[] digest;
        /// <summary>
        /// Retuirn Record Digest
        /// </summary>
        public byte[] Digest
        {
            get { return digest; }
        }
    }
}
