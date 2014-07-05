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
    /// <summary>
    /// Implementation References RFC 2845
    /// </summary>
    class TSigRecord : IRecordData
    {
         public TSigRecord(DataBuffer buffer)
        {
            algorithm = buffer.ReadDomainName();
            timeSigned = buffer.ReadLongInt();
            fudge = buffer.ReadShortUInt();
            macSize = buffer.ReadShortUInt();
            mac = buffer.ReadBytes(macSize);
            originalId = buffer.ReadShortUInt();
            error = buffer.ReadShortUInt();
            otherLen = buffer.ReadShortUInt();
            otherData = buffer.ReadBytes(otherLen);
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Algorithm:{0} Signed Time:{1} Fudge Factor:{2} Mac:{3} Original ID:{4} Error:{5}\nOther Data:{6}",
                algorithm, timeSigned, fudge, mac, originalId, error, otherData);
        }

        private string algorithm;
        /// <summary>
        /// return Algorithm of record
        /// </summary>
        public string Algorithm
        {
            get { return algorithm; }
        }
        private long timeSigned;
        /// <summary>
        /// return signature time
        /// </summary>
        public long TimeSigned
        {
            get { return timeSigned; }
        }
        private ushort fudge;
        /// <summary>
        /// return fudge factor of record
        /// </summary>
        public ushort Fudge
        {
            get { return fudge; }
        }
        private ushort macSize;
        private byte[] mac;
        /// <summary>
        /// return MAC Address
        /// </summary>
        public byte[] Mac
        {
            get { return mac; }
        }
        private ushort originalId;
        /// <summary>
        /// return Original ID of record
        /// </summary>
        public ushort OriginalId
        {
            get { return originalId; }
        }
        private ushort error;
        /// <summary>
        /// return error of record
        /// </summary>
        public ushort Error
        {
            get { return error; }
        }
        private ushort otherLen;
        private byte[] otherData;
        /// <summary>
        /// rfeturn Other Data of record
        /// </summary>
        public byte[] OtherData
        {
            get { return otherData; }
        }
    }
}
