/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Net;
using System.Net.Sockets;

namespace ASC.IPSecurity
{
    class IPAddressRange
    {
        private readonly AddressFamily addressFamily;
        private readonly byte[] lowerBytes;
        private readonly byte[] upperBytes;

        public IPAddressRange(IPAddress lower, IPAddress upper)
        {
            addressFamily = lower.AddressFamily;
            lowerBytes = lower.GetAddressBytes();
            upperBytes = upper.GetAddressBytes();
        }

        public bool IsInRange(IPAddress address)
        {
            if (address.AddressFamily != addressFamily)
            {
                return false;
            }

            var addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (var i = 0; i < lowerBytes.Length &&
                            (lowerBoundary || upperBoundary); i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) || (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }

        public static bool IsInRange(string ipAddress, string CIDRmask)
        {
            string[] parts = CIDRmask.Split('/');

            var requestIP = IPAddress.Parse(ipAddress);
            var restrictionIP = IPAddress.Parse(parts[0]);

            if (requestIP.AddressFamily != restrictionIP.AddressFamily)
            {
                return false;
            }

            int IP_addr = BitConverter.ToInt32(requestIP.GetAddressBytes(), 0);
            int CIDR_addr = BitConverter.ToInt32(restrictionIP.GetAddressBytes(), 0);
            int CIDR_mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

            return (IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask);
        }
    }
}