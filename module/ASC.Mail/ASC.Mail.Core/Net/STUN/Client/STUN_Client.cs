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
    #region usings

    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using Message;

    #endregion

    /// <summary>
    /// This class implements STUN client. Defined in RFC 3489.
    /// </summary>
    /// <example>
    /// <code>
    /// // Create new socket for STUN client.
    /// Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
    /// socket.Bind(new IPEndPoint(IPAddress.Any,0));
    /// 
    /// // Query STUN server
    /// STUN_Result result = STUN_Client.Query("stunserver.org",3478,socket);
    /// if(result.NetType != STUN_NetType.UdpBlocked){
    ///     // UDP blocked or !!!! bad STUN server
    /// }
    /// else{
    ///     IPEndPoint publicEP = result.PublicEndPoint;
    ///     // Do your stuff
    /// }
    /// </code>
    /// </example>
    public class STUN_Client
    {
        #region Methods

        /// <summary>
        /// Gets NAT info from STUN server.
        /// </summary>
        /// <param name="host">STUN server name or IP.</param>
        /// <param name="port">STUN server port. Default port is 3478.</param>
        /// <param name="socket">UDP socket to use.</param>
        /// <returns>Returns UDP netwrok info.</returns>
        /// <exception cref="Exception">Throws exception if unexpected error happens.</exception>
        public static STUN_Result Query(string host, int port, Socket socket)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (port < 1)
            {
                throw new ArgumentException("Port value must be >= 1 !");
            }
            if (socket.ProtocolType != ProtocolType.Udp)
            {
                throw new ArgumentException("Socket must be UDP socket !");
            }

            IPEndPoint remoteEndPoint = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);

            socket.ReceiveTimeout = 3000;
            socket.SendTimeout = 3000;

            /*
                In test I, the client sends a STUN Binding Request to a server, without any flags set in the
                CHANGE-REQUEST attribute, and without the RESPONSE-ADDRESS attribute. This causes the server 
                to send the response back to the address and port that the request came from.
            
                In test II, the client sends a Binding Request with both the "change IP" and "change port" flags
                from the CHANGE-REQUEST attribute set.  
              
                In test III, the client sends a Binding Request with only the "change port" flag set.
                          
                                    +--------+
                                    |  Test  |
                                    |   I    |
                                    +--------+
                                         |
                                         |
                                         V
                                        /\              /\
                                     N /  \ Y          /  \ Y             +--------+
                      UDP     <-------/Resp\--------->/ IP \------------->|  Test  |
                      Blocked         \ ?  /          \Same/              |   II   |
                                       \  /            \? /               +--------+
                                        \/              \/                    |
                                                         | N                  |
                                                         |                    V
                                                         V                    /\
                                                     +--------+  Sym.      N /  \
                                                     |  Test  |  UDP    <---/Resp\
                                                     |   II   |  Firewall   \ ?  /
                                                     +--------+              \  /
                                                         |                    \/
                                                         V                     |Y
                              /\                         /\                    |
               Symmetric  N  /  \       +--------+   N  /  \                   V
                  NAT  <--- / IP \<-----|  Test  |<--- /Resp\               Open
                            \Same/      |   I    |     \ ?  /               Internet
                             \? /       +--------+      \  /
                              \/                         \/
                              |                           |Y
                              |                           |
                              |                           V
                              |                           Full
                              |                           Cone
                              V              /\
                          +--------+        /  \ Y
                          |  Test  |------>/Resp\---->Restricted
                          |   III  |       \ ?  /
                          +--------+        \  /
                                             \/
                                              |N
                                              |       Port
                                              +------>Restricted

            */

            // Test I
            STUN_Message test1 = new STUN_Message();
            test1.Type = STUN_MessageType.BindingRequest;
            STUN_Message test1response = DoTransaction(test1, socket, remoteEndPoint);

            // UDP blocked.
            if (test1response == null)
            {
                return new STUN_Result(STUN_NetType.UdpBlocked, null);
            }
            else
            {
                // Test II
                STUN_Message test2 = new STUN_Message();
                test2.Type = STUN_MessageType.BindingRequest;
                test2.ChangeRequest = new STUN_t_ChangeRequest(true, true);

                // No NAT.
                if (socket.LocalEndPoint.Equals(test1response.MappedAddress))
                {
                    STUN_Message test2Response = DoTransaction(test2, socket, remoteEndPoint);
                    // Open Internet.
                    if (test2Response != null)
                    {
                        return new STUN_Result(STUN_NetType.OpenInternet, test1response.MappedAddress);
                    }
                        // Symmetric UDP firewall.
                    else
                    {
                        return new STUN_Result(STUN_NetType.SymmetricUdpFirewall, test1response.MappedAddress);
                    }
                }
                    // NAT
                else
                {
                    STUN_Message test2Response = DoTransaction(test2, socket, remoteEndPoint);
                    // Full cone NAT.
                    if (test2Response != null)
                    {
                        return new STUN_Result(STUN_NetType.FullCone, test1response.MappedAddress);
                    }
                    else
                    {
                        /*
                            If no response is received, it performs test I again, but this time, does so to 
                            the address and port from the CHANGED-ADDRESS attribute from the response to test I.
                        */

                        // Test I(II)
                        STUN_Message test12 = new STUN_Message();
                        test12.Type = STUN_MessageType.BindingRequest;

                        STUN_Message test12Response = DoTransaction(test12,
                                                                    socket,
                                                                    test1response.ChangedAddress);
                        if (test12Response == null)
                        {
                            throw new Exception("STUN Test I(II) dind't get resonse !");
                        }
                        else
                        {
                            // Symmetric NAT
                            if (!test12Response.MappedAddress.Equals(test1response.MappedAddress))
                            {
                                return new STUN_Result(STUN_NetType.Symmetric, test1response.MappedAddress);
                            }
                            else
                            {
                                // Test III
                                STUN_Message test3 = new STUN_Message();
                                test3.Type = STUN_MessageType.BindingRequest;
                                test3.ChangeRequest = new STUN_t_ChangeRequest(false, true);

                                STUN_Message test3Response = DoTransaction(test3,
                                                                           socket,
                                                                           test1response.ChangedAddress);
                                // Restricted
                                if (test3Response != null)
                                {
                                    return new STUN_Result(STUN_NetType.RestrictedCone,
                                                           test1response.MappedAddress);
                                }
                                    // Port restricted
                                else
                                {
                                    return new STUN_Result(STUN_NetType.PortRestrictedCone,
                                                           test1response.MappedAddress);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolves local IP to public IP using STUN.
        /// </summary>
        /// <param name="stunServer">STUN server.</param>
        /// <param name="port">STUN server port. Default port is 3478.</param>
        /// <param name="localIP">Local IP address.</param>
        /// <returns>Returns public IP address.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>stunServer</b> or <b>localIP</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="IOException">Is raised when no connection to STUN server.</exception>
        public static IPAddress GetPublicIP(string stunServer, int port, IPAddress localIP)
        {
            if (stunServer == null)
            {
                throw new ArgumentNullException("stunServer");
            }
            if (stunServer == "")
            {
                throw new ArgumentException("Argument 'stunServer' value must be specified.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Invalid argument 'port' value.");
            }
            if (localIP == null)
            {
                throw new ArgumentNullException("localIP");
            }

            if (!Core.IsPrivateIP(localIP))
            {
                return localIP;
            }

            STUN_Result result = Query(stunServer,
                                       port,
                                       Core.CreateSocket(new IPEndPoint(localIP, 0), ProtocolType.Udp));
            if (result.PublicEndPoint != null)
            {
                return result.PublicEndPoint.Address;
            }
            else
            {
                throw new IOException(
                    "Failed to STUN public IP address. STUN server name is invalid or firewall blocks STUN.");
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Does STUN transaction. Returns transaction response or null if transaction failed.
        /// </summary>
        /// <param name="request">STUN message.</param>
        /// <param name="socket">Socket to use for send/receive.</param>
        /// <param name="remoteEndPoint">Remote end point.</param>
        /// <returns>Returns transaction response or null if transaction failed.</returns>
        private static STUN_Message DoTransaction(STUN_Message request,
                                                  Socket socket,
                                                  IPEndPoint remoteEndPoint)
        {
            byte[] requestBytes = request.ToByteData();
            DateTime startTime = DateTime.Now;
            // We do it only 2 sec and retransmit with 100 ms.
            while (startTime.AddSeconds(2) > DateTime.Now)
            {
                try
                {
                    socket.SendTo(requestBytes, remoteEndPoint);

                    // We got response.
                    if (socket.Poll(100, SelectMode.SelectRead))
                    {
                        byte[] receiveBuffer = new byte[512];
                        socket.Receive(receiveBuffer);

                        // Parse message
                        STUN_Message response = new STUN_Message();
                        response.Parse(receiveBuffer);

                        // Check that transaction ID matches or not response what we want.
                        if (request.TransactionID.Equals(response.TransactionID))
                        {
                            return response;
                        }
                    }
                }
                catch {}
            }

            return null;
        }

        private void GetSharedSecret()
        {
            /*
                *) Open TLS connection to STUN server.
                *) Send Shared Secret request.
            */

            /*
            using(SocketEx socket = new SocketEx()){
                socket.RawSocket.ReceiveTimeout = 5000;
                socket.RawSocket.SendTimeout = 5000;

                socket.Connect(host,port);
                socket.SwitchToSSL_AsClient();                

                // Send Shared Secret request.
                STUN_Message sharedSecretRequest = new STUN_Message();
                sharedSecretRequest.Type = STUN_MessageType.SharedSecretRequest;
                socket.Write(sharedSecretRequest.ToByteData());
                
                // TODO: Parse message

                // We must get  "Shared Secret" or "Shared Secret Error" response.

                byte[] receiveBuffer = new byte[256];
                socket.RawSocket.Receive(receiveBuffer);

                STUN_Message sharedSecretRequestResponse = new STUN_Message();
                if(sharedSecretRequestResponse.Type == STUN_MessageType.SharedSecretResponse){
                }
                // Shared Secret Error or Unknown response, just try again.
                else{
                    // TODO: Unknown response
                }
            }*/
        }

        #endregion
    }
}