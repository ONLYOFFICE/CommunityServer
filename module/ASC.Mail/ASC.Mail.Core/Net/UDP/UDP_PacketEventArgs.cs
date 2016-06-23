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


namespace ASC.Mail.Net.UDP
{
    #region usings

    using System;
    using System.Net;
    using System.Net.Sockets;

    #endregion

    /// <summary>
    /// This class provides data for <b>UdpServer.PacketReceived</b> event.
    /// </summary>
    public class UDP_PacketEventArgs : EventArgs
    {
        #region Members

        private readonly byte[] m_pData;
        private readonly IPEndPoint m_pRemoteEP;
        private readonly Socket m_pSocket;
        private readonly UDP_Server m_pUdpServer;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">UDP server which received packet.</param>
        /// <param name="socket">Socket which received packet.</param>
        /// <param name="remoteEP">Remote end point which sent data.</param>
        /// <param name="data">UDP data.</param>
        internal UDP_PacketEventArgs(UDP_Server server, Socket socket, IPEndPoint remoteEP, byte[] data)
        {
            m_pUdpServer = server;
            m_pSocket = socket;
            m_pRemoteEP = remoteEP;
            m_pData = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets UDP packet data.
        /// </summary>
        public byte[] Data
        {
            get { return m_pData; }
        }

        /// <summary>
        /// Gets local end point what recieved packet.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint) m_pSocket.LocalEndPoint; }
        }

        /// <summary>
        /// Gets remote end point what sent data.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEP; }
        }

        /// <summary>
        /// Gets UDP server which received packet.
        /// </summary>
        public UDP_Server UdpServer
        {
            get { return m_pUdpServer; }
        }

        /// <summary>
        /// Gets socket which received packet.
        /// </summary>
        internal Socket Socket
        {
            get { return m_pSocket; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends reply to received packet. This method uses same local end point to send packet which
        /// received packet, this ensures right NAT traversal.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">Offset in the buffer.</param>
        /// <param name="count">Number of bytes to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null.</exception>
        public void SendReply(byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            IPEndPoint localEP = null;
            m_pUdpServer.SendPacket(m_pSocket, data, offset, count, m_pRemoteEP, out localEP);
        }

        #endregion
    }
}