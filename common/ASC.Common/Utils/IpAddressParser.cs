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

namespace ASC.Common.Utils
{
    public class IpAddressParser
    {
        /// <summary>
        /// Parse the address into an array
        /// </summary>
        /// <param name="value">Comma-separated string value (e.g. X-Forwarded-For request header)</param>
        /// <returns>Address array</returns>
        public static string[] ParseAddress(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                              ? new string[] { }
                              : value.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns IP address without port
        /// </summary>
        /// <param name="ip">IPv4 or IPv6 address with port (e.g. [::1]:80, 127.0.0.1:80)</param>
        /// <returns>IP address without port</returns>
        public static string GetIpWithoutPort(string ip)
        {
            var s = ip.AsSpan();
            var lastColonPos = s.LastIndexOf(':');

            // If it's IPv4 and there's no port then return the entire string
            if (lastColonPos > 0)
            {
                // Look to see if this is an IPv6 address with a port.
                if (s[lastColonPos - 1] == ']')
                {
                    return s.Slice(0, lastColonPos).ToString();
                }
                // Look to see if this is IPv4 with a port (IPv6 will have another colon)
                else if (s.Slice(0, lastColonPos).LastIndexOf(':') == -1)
                {
                    return s.Slice(0, lastColonPos).ToString();
                }
            }

            return ip;
        }
    }
}
