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
    /// Implementation Reference RFC 1035
    /// </summary>
    class SoaRecord : IRecordData
    {
        public SoaRecord(DataBuffer buffer)
        {
            primaryNameServer = buffer.ReadDomainName();
            responsibleMailAddress = buffer.ReadDomainName();
            serial = buffer.ReadInt();
            refresh = buffer.ReadInt();
            retry = buffer.ReadInt();
            expire = buffer.ReadInt();
            defaultTtl = buffer.ReadInt();
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Primary Name Server:{0} Responsible Name Address:{1} Serial:{2} Refresh:{3} Retry:{4} Expire:{5} Default TTL:{6}",
                primaryNameServer, responsibleMailAddress, serial, refresh, retry, expire, defaultTtl);
        }

        private string primaryNameServer;
        /// <summary>
        /// Primary Name  Server of record
        /// </summary>
        public string PrimaryNameServer
        {
            get { return primaryNameServer; }
        }
        private string responsibleMailAddress;
        /// <summary>
        /// Responsible Person Mail Address 
        /// </summary>
        public string ResponsibleMailAddress
        {
            get { return responsibleMailAddress; }
        }
        private int serial;
        /// <summary>
        /// Serial of record
        /// </summary>
        public int Serial
        {
            get { return serial; }
        }
        private int refresh;
        /// <summary>
        /// Refresh of record
        /// </summary>
        public int Refresh
        {
            get { return refresh; }
        }
        private int retry;
        /// <summary>
        /// return retry of record
        /// </summary>
        public int Retry
        {
            get { return retry; }
        }
        private int expire;
        /// <summary>
        /// return expiration of record
        /// </summary>
        public int Expire
        {
            get { return expire; }
        }
        private int defaultTtl;
        /// <summary>
        /// return default ttl of record
        /// </summary>
        public int DefaultTtl
        {
            get { return defaultTtl; }
        }
    }
}
