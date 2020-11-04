/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace ASC.Mail.Net.STUN.Client
{
    /// <summary>
    /// Specifies UDP network type.
    /// </summary>
    public enum STUN_NetType
    {
        /// <summary>
        /// UDP is always blocked.
        /// </summary>
        UdpBlocked,

        /// <summary>
        /// No NAT, public IP, no firewall.
        /// </summary>
        OpenInternet,

        /// <summary>
        /// No NAT, public IP, but symmetric UDP firewall.
        /// </summary>
        SymmetricUdpFirewall,

        /// <summary>
        /// A full cone NAT is one where all requests from the same internal IP address and port are 
        /// mapped to the same external IP address and port. Furthermore, any external host can send 
        /// a packet to the internal host, by sending a packet to the mapped external address.
        /// </summary>
        FullCone,

        /// <summary>
        /// A restricted cone NAT is one where all requests from the same internal IP address and 
        /// port are mapped to the same external IP address and port. Unlike a full cone NAT, an external
        /// host (with IP address X) can send a packet to the internal host only if the internal host 
        /// had previously sent a packet to IP address X.
        /// </summary>
        RestrictedCone,

        /// <summary>
        /// A port restricted cone NAT is like a restricted cone NAT, but the restriction 
        /// includes port numbers. Specifically, an external host can send a packet, with source IP
        /// address X and source port P, to the internal host only if the internal host had previously 
        /// sent a packet to IP address X and port P.
        /// </summary>
        PortRestrictedCone,

        /// <summary>
        /// A symmetric NAT is one where all requests from the same internal IP address and port, 
        /// to a specific destination IP address and port, are mapped to the same external IP address and
        /// port.  If the same host sends a packet with the same source address and port, but to 
        /// a different destination, a different mapping is used. Furthermore, only the external host that
        /// receives a packet can send a UDP packet back to the internal host.
        /// </summary>
        Symmetric
    }
}