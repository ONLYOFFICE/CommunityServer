/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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