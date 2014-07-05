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
    class SigRecord : IRecordData
    {
        /// <summary>
        /// Implementation Reference RFC 2535
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
         public SigRecord(DataBuffer buffer, int length)
        {
            int pos = buffer.Position;
            coveredType = buffer.ReadShortInt();
            algorithm = buffer.ReadByte();
            numLabels = buffer.ReadByte();
            expiration = buffer.ReadUInt();
            inception = buffer.ReadUInt();
            keyTag = buffer.ReadShortInt();
            signer = buffer.ReadDomainName();
            buffer.Position = pos - length;
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Covered Type:{0} Algorithm:{1} Number of Labels:{2} Expiration:{3} Inception:{4} Key Tag:{5} Signer:{6}",
                coveredType, algorithm, numLabels, expiration, inception, keyTag, signer);
        }

        private short coveredType;
        /// <summary>
        /// Return Covered Type of record
        /// </summary>
        public short CoveredType
        {
            get { return coveredType; }
        }
        private byte algorithm;
        /// <summary>
        /// return Algorithm of record type
        /// </summary>
        public byte Algorithm
        {
            get { return algorithm; }
        }
        private byte numLabels;
        /// <summary>
        /// return Number of Labels of record
        /// </summary>
        public byte NumLabels
        {
            get { return numLabels; }
        }
        private uint expiration;
        /// <summary>
        /// return expiration of record
        /// </summary>
        public uint Expiration
        {
            get { return expiration; }
        }
        private uint inception;
        /// <summary>
        /// return inception of record
        /// </summary>
        public uint Inception
        {
            get { return inception; }
        }
        private short keyTag;
        /// <summary>
        /// return Key Tag of record
        /// </summary>
        public short KeyTag
        {
            get { return keyTag; }
        }
        private string signer;
        /// <summary>
        /// return signer of record
        /// </summary>
        public string Signer
        {
            get { return signer; }
        }
    }
}
