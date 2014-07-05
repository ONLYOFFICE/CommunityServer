/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.ICMP
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;

    #endregion

    /// <summary>
    /// ICMP type.
    /// </summary>
    public enum ICMP_Type
    {
        /// <summary>
        /// Echo rely.
        /// </summary>
        EchoReply = 0,

        /// <summary>
        /// Time to live exceeded reply.
        /// </summary>
        TimeExceeded = 11,

        /// <summary>
        /// Echo.
        /// </summary>
        Echo = 8,
    }

    #region class EchoMessage

    /// <summary>
    /// Echo reply message.
    /// </summary>
    public class EchoMessage
    {
        #region Members

        private readonly IPAddress m_pIP;
        private readonly int m_Time;
        private readonly int m_TTL;

        #endregion

        #region Properties

        /// <summary>
        /// Gets IP address what sent echo message.
        /// </summary>
        public IPAddress IPAddress
        {
            get { return m_pIP; }
        }

        /*
        /// <summary>
        /// Gets time to live in milli seconds.
        /// </summary>
        public int TTL
        {
            get{ return m_TTL; }
        }*/

        /// <summary>
        /// Gets time in milliseconds what toke to get reply.
        /// </summary>
        public int ReplyTime
        {
            get { return m_Time; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ip">IP address what sent echo message.</param>
        /// <param name="ttl">Time to live in milli seconds.</param>
        /// <param name="time">Time what elapsed before getting echo response.</param>
        internal EchoMessage(IPAddress ip, int ttl, int time)
        {
            m_pIP = ip;
            m_TTL = ttl;
            m_Time = time;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        [Obsolete("Will be removed !")]
        public static string ToStringEx(EchoMessage[] messages)
        {
            string retVal = "";

            foreach (EchoMessage m in messages)
            {
                retVal += m.ToStringEx() + "\r\n";
            }

            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete("Will be removed !")]
        public string ToStringEx()
        {
            return "TTL=" + m_TTL + "\tTime=" + m_Time + "ms" + "\tIP=" + m_pIP;
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// Icmp utils.
    /// </summary>
    public class Icmp
    {
        #region Methods

        /// <summary>
        /// Traces specified ip.
        /// </summary>
        /// <param name="destIP">Destination IP address.</param>
        /// <returns></returns>
        public static EchoMessage[] Trace(string destIP)
        {
            return Trace(IPAddress.Parse(destIP), 2000);
        }

        /// <summary>
        /// Traces specified ip.
        /// </summary>
        /// <param name="ip">IP address to tracce.</param>
        /// <param name="timeout">Send recieve timeout in milli seconds.</param>
        /// <returns></returns>
        public static EchoMessage[] Trace(IPAddress ip, int timeout)
        {
            List<EchoMessage> retVal = new List<EchoMessage>();

            //Create Raw ICMP Socket 
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);

            IPEndPoint ipdest = new IPEndPoint(ip, 80);
            EndPoint endpoint = (new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 80));

            ushort id = (ushort) DateTime.Now.Millisecond;
            byte[] sendPacket = CreatePacket(id);

            int continuesNoReply = 0;
            //send requests with increasing number of TTL
            for (int ittl = 1; ittl <= 30; ittl++)
            {
                byte[] buffer = new byte[1024];

                try
                {
                    //Socket options to set TTL and Timeouts 
                    s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ittl);
                    s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
                    s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);

                    //Get current time
                    DateTime startTime = DateTime.Now;

                    //Send Request
                    s.SendTo(sendPacket, sendPacket.Length, SocketFlags.None, ipdest);

                    //Receive				
                    s.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref endpoint);

                    //Calculate time required
                    TimeSpan ts = DateTime.Now - startTime;
                    retVal.Add(new EchoMessage(((IPEndPoint) endpoint).Address, ittl, ts.Milliseconds));

                    // Endpoint reached
                    if (buffer[20] == (byte) ICMP_Type.EchoReply)
                    {
                        break;
                    }

                    // Un wanted reply
                    if (buffer[20] != (byte) ICMP_Type.TimeExceeded)
                    {
                        throw new Exception("UnKnown error !");
                    }

                    continuesNoReply = 0;
                }
                catch
                {
                    //ToDo: Handle recive/send timeouts
                    continuesNoReply++;
                }

                // If there is 3 continues no reply, consider that destination host won't accept ping.
                if (continuesNoReply >= 3)
                {
                    break;
                }
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Pings specified destination host.
        /// </summary>
        /// <param name="ip">IP address to ping.</param>
        /// <param name="timeout">Send recieve timeout in milli seconds.</param>
        /// <returns></returns>
        public static EchoMessage Ping(IPAddress ip, int timeout)
        {
            //Create Raw ICMP Socket 
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);

            IPEndPoint ipdest = new IPEndPoint(ip, 80);
            EndPoint endpoint = (new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 80));

            ushort id = (ushort) DateTime.Now.Millisecond;
            byte[] sendPacket = CreatePacket(id);

            //Socket options to set TTL and Timeouts 
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 30);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);

            //Get current time
            DateTime startTime = DateTime.Now;

            //Send Request
            s.SendTo(sendPacket, sendPacket.Length, SocketFlags.None, ipdest);

            //Receive
            byte[] buffer = new byte[1024];
            s.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref endpoint);

            // Endpoint reached
            if (buffer[20] == (byte) ICMP_Type.EchoReply) {}					
                // Un wanted reply
            else if (buffer[20] != (byte) ICMP_Type.TimeExceeded)
            {
                throw new Exception("UnKnown error !");
            }

            //Calculate time elapsed
            TimeSpan ts = DateTime.Now - startTime;
            return new EchoMessage(((IPEndPoint) endpoint).Address, 0, ts.Milliseconds);
        }

        #endregion

        #region Utility methods

        private static byte[] CreatePacket(ushort id)
        {
            /*Rfc 792  Echo or Echo Reply Message
			  0               8              16              24
			 +---------------+---------------+---------------+---------------+
			 |     Type      |     Code      |           Checksum            |
			 +---------------+---------------+---------------+---------------+
			 |           ID Number           |            Sequence Number    |
			 +---------------+---------------+---------------+---------------+
			 |     Data...        
			 +---------------+---------------+---------------+---------------+
			*/

            byte[] packet = new byte[8 + 2];
            packet[0] = (byte) ICMP_Type.Echo; // Type
            packet[1] = 0; // Code
            packet[2] = 0; // Checksum
            packet[3] = 0; // Checksum
            packet[4] = 0; // ID
            packet[5] = 0; // ID
            packet[6] = 0; // Sequence
            packet[7] = 0; // Sequence

            // Set id
            Array.Copy(BitConverter.GetBytes(id), 0, packet, 4, 2);

            // Fill data 2 byte data
            for (int i = 0; i < 2; i++)
            {
                packet[i + 8] = (byte) 'x'; // Data
            }

            // calculate checksum
            int checkSum = 0;
            for (int i = 0; i < packet.Length; i += 2)
            {
                checkSum += Convert.ToInt32(BitConverter.ToUInt16(packet, i));
            }

            // The checksum is the 16-bit ones's complement of the one's
            // complement sum of the ICMP message starting with the ICMP Type.
            checkSum = (checkSum & 0xffff);
            Array.Copy(BitConverter.GetBytes((ushort) ~checkSum), 0, packet, 2, 2);

            return packet;
        }

        #endregion
    }
}