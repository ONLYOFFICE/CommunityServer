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

#if !PocketPC
using System.Net.NetworkInformation;
#endif
using System.Net;
using System;

namespace ActiveUp.Net.Dns
{
    class Utils
    {
#if !PocketPC
        public static string[] GetNetworkInterfaces()
        {
          NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
          if (nics == null || nics.Length < 1) {
            throw new Exception("No network interfaces found");
          }

          String[] s = new string[nics.Length];
          int i = 0;
          foreach (NetworkInterface adapter in nics) {
            s[i] = adapter.Description;
            i++;
          }
          return s;
        }
#endif

#if !PocketPC
        private static string[] GetAdapterIpAdresses(NetworkInterface adapter){
          if (adapter == null) {
            throw new Exception("No network interfaces found");
          }
          IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
          string[] s = null;
          IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
          if (dnsServers != null) {
            s = new string[dnsServers.Count];
            int i = 0;
            foreach (IPAddress dns in dnsServers) {
              s[i] = dns.ToString();
              i++;
            }
          }
          return s;
        }
#endif
    }
}
