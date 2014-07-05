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
    class TKeyRecord : IRecordData
    {
        /// <summary>
        /// Implementation References RFC 2930
        /// </summary>
        /// <param name="buffer"></param>
         public TKeyRecord(DataBuffer buffer)
        {
            algorithm = buffer.ReadDomainName();
            inception = buffer.ReadUInt();
            expiration = buffer.ReadUInt();
            mode = buffer.ReadShortUInt();
            error = buffer.ReadShortUInt();
            keySize = buffer.ReadShortUInt();
            keyData = buffer.ReadBytes(keySize);
            otherSize = buffer.ReadShortUInt();
            otherData = buffer.ReadBytes(otherSize);
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Algorithm:{0} Inception:{1} Expiration:{2} Mode:{3} Error:{4} \nKey Data:{5} \nOther Data:{6} ",
                algorithm, inception, expiration, mode, error, keyData, otherData);
        }


        private string algorithm;
        /// <summary>
        /// return algorithm of record
        /// </summary>
        public string Algorithm
        {
            get { return algorithm; }
        }
        private uint inception;
        /// <summary>
        /// return inception time of record
        /// </summary>
        public uint Inception
        {
            get { return inception; }
        }
        private uint expiration;
        /// <summary>
        /// return expiration time of record
        /// </summary>
        public uint Expiration
        {
            get { return expiration; }
        }
        private ushort mode;
        /// <summary>
        /// return mode of record
        /// </summary>
        public ushort Mode
        {
            get { return mode; }
        }
        private ushort error;
        /// <summary>
        /// rfeturn error of record
        /// </summary>
        public ushort Error
        {
            get { return error; }
        }
        private ushort keySize;            
        private byte[] keyData;
        /// <summary>
        /// return Key Data of record
        /// </summary>
        public byte[] KeyData
        {
            get { return keyData; }
        }
        private ushort otherSize;
        private byte[] otherData;
        /// <summary>
        /// return Other Data of record
        /// </summary>
        public byte[] OtherData
        {
            get { return otherData; }
        }
    }
}
