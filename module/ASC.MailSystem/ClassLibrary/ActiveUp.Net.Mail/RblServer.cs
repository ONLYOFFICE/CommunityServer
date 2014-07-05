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
using ActiveUp.Net.Dns;
using System.Net;

namespace ActiveUp.Net.Mail
{
    public class RblServer : Server
    {
        /// <summary>
        /// Gets the RBL status.
        /// </summary>
        /// <param name="rblServer">The RBL server.</param>
        /// <param name="ipString">The ip string.</param>
        /// <returns></returns>
        public RblStatus GetRblStatus(string rblServer, string ipString)
        {
            return GetRblStatus(rblServer, IPAddress.Parse(ipString));
        }

        /// <summary>
        /// Gets the RBL status.
        /// </summary>
        /// <param name="rblServer">The RBL server.</param>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        public RblStatus GetRblStatus(string rblServer, IPAddress ip)
        {
            //DnsQuery query = new DnsQuery();
            IPAddress result = GetRblIp(rblServer, ip);

            if (result == null)
                return RblStatus.NotListed;

            switch (result.ToString())
            {
                case "127.0.0.5": return RblStatus.OpenRelay;
                case "127.0.0.1":
                case "127.0.0.2": return RblStatus.BlackListed;
                case "127.0.0.3": return RblStatus.OpenSocks;
            }

            return RblStatus.NotListed;
        }

        /// <summary>
        /// Gets the RBL ip.
        /// </summary>
        /// <param name="rblServer">The RBL server.</param>
        /// <param name="ipString">The ip string.</param>
        /// <returns></returns>
        public IPAddress GetRblIp(string rblServer, string ipString)
        {
            return GetRblIp(rblServer, IPAddress.Parse(ipString));
        }

        /// <summary>
        /// Gets the RBL ip.
        /// </summary>
        /// <param name="rblServer">The RBL server.</param>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        public IPAddress GetRblIp(string rblServer, IPAddress ip)
        {
            DnsQuery query = new DnsQuery("213.254.203.215");

            byte[] addressBytes = ip.GetAddressBytes();
            string domain = string.Format("{0}.{1}.{2}.{3}.{4}", addressBytes[3].ToString(),
                addressBytes[2].ToString(), addressBytes[1].ToString(), addressBytes[0].ToString(), rblServer);

            query.Domain = domain;
            DnsAnswer answer = query.QueryServer(RecordType.A);
            
            if (answer == null || answer.Answers.Count == 0)
                return null;

            string result = answer.Answers[0].Data.ToString().Replace("IP Address: ", "");

            return IPAddress.Parse(result);
        }
    }
}
