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
using System.Net;

namespace ActiveUp.Net.Dns
{
    class WksRecord : IRecordData
    {
        /// <summary>
        /// WKS Record Type Consructor
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
         public WksRecord(DataBuffer buffer, int length)
        {
             ipAddress = buffer.ReadIPAddress();
             protocol = buffer.ReadByte();
             services = new Byte[length - 5];
             for(int i = 0; i < (length - 5); i++)
                 services[i] = buffer.ReadByte();
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("IP Address:{0} Protocol:{1} Services:{2}", ipAddress, protocol, services);
        }

        IPAddress ipAddress;
        /// <summary>
        /// IP Address of record
        /// </summary>
        public IPAddress IpAddress      {   get { return ipAddress; }   }
        byte protocol;
        /// <summary>
        /// return Protocol of record
        /// </summary>
        public byte Protocol            {   get { return protocol; }    }
        byte[] services;
        /// <summary>
        /// return Services of record
        /// </summary>
        public byte[] Services          {   get { return services; }    }
    }
}
