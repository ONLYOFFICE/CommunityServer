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
    class A6Record : IRecordData
    {
        /// <summary>
        /// Implementation Reference RFC 3363
        /// </summary>
        /// <param name="buffer"></param>
         public A6Record(DataBuffer buffer)
        {
             prefixLength = buffer.ReadByte();
             if(prefixLength == 0) //Only Address Present
             {                 
                 ipAddress = buffer.ReadIPv6Address();
             }
             else if (prefixLength == 128) //Only Domain Name Present
             {
                 domain = buffer.ReadDomainName();
             }
             else //Address and Domain Name Present
             {
                 ipAddress = buffer.ReadIPv6Address();
                 domain = buffer.ReadDomainName();
             }
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Prefix Length:{0} IP Address:{1} Domain:{2}", prefixLength, ipAddress, domain);
        }

        private int prefixLength = -1;
        /// <summary>
        /// Return prefix length
        /// </summary>
        public int PrefixLength         {   get { return prefixLength; }    }
        private IPAddress ipAddress;
        /// <summary>
        /// Return IP Address of the data record
        /// </summary>
        public IPAddress IpAddress      {   get { return ipAddress; }       }
        private string domain;
        /// <summary>
        /// Return Domain name of data record
        /// </summary>
        public string Domain            {   get { return domain; }          }
    }
}
