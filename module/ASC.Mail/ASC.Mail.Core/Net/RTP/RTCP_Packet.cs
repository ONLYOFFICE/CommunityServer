/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.RTP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This is base class for RTCP packets.
    /// </summary>
    public abstract class RTCP_Packet
    {
        #region Members

        private int m_PaddBytesCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets RTCP version.
        /// </summary>
        public abstract int Version { get; }

        /// <summary>
        /// Gets if packet is padded to some bytes boundary.
        /// </summary>
        public bool IsPadded
        {
            get
            {
                if (m_PaddBytesCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets RTCP packet type.
        /// </summary>
        public abstract int Type { get; }

        /// <summary>
        /// Gets or sets number empty bytes to add at the end of packet.
        /// </summary>
        public int PaddBytesCount
        {
            get { return m_PaddBytesCount; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Property 'PaddBytesCount' value must be >= 0.");
                }

                m_PaddBytesCount = value;
            }
        }

        /// <summary>
        /// Gets number of bytes needed for this packet.
        /// </summary>
        public abstract int Size { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Parses 1 RTCP packet from the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer which contains RTCP packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <returns>Returns parsed RTCP packet.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static RTCP_Packet Parse(byte[] buffer, ref int offset)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' value must be >= 0.");
            }

            /* RFC 3550 RTCP header.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            header |V=2|P|    XX   |   type        |             length            |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */

            int type = buffer[offset + 1];

            // SR
            if (type == RTCP_PacketType.SR)
            {
                RTCP_Packet_SR packet = new RTCP_Packet_SR();
                packet.ParseInternal(buffer, ref offset);

                return packet;
            }
                // RR
            else if (type == RTCP_PacketType.RR)
            {
                RTCP_Packet_RR packet = new RTCP_Packet_RR();
                packet.ParseInternal(buffer, ref offset);

                return packet;
            }
                // SDES
            else if (type == RTCP_PacketType.SDES)
            {
                RTCP_Packet_SDES packet = new RTCP_Packet_SDES();
                packet.ParseInternal(buffer, ref offset);

                return packet;
            }
                // BYE
            else if (type == RTCP_PacketType.BYE)
            {
                RTCP_Packet_BYE packet = new RTCP_Packet_BYE();
                packet.ParseInternal(buffer, ref offset);

                return packet;
            }
                // APP
            else if (type == RTCP_PacketType.APP)
            {
                RTCP_Packet_APP packet = new RTCP_Packet_APP();
                packet.ParseInternal(buffer, ref offset);

                return packet;
            }
            else
            {
                // We need to move offset.
                offset += 2;
                int length = buffer[offset++] << 8 | buffer[offset++];
                offset += length;

                throw new ArgumentException("Unknown RTCP packet type '" + type + "'.");
            }
        }

        /// <summary>
        /// Stores this packet to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        public abstract void ToByte(byte[] buffer, ref int offset);

        #endregion

        #region Abstract methods

        /// <summary>
        /// Parses RTCP packet from the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer which contains packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        protected abstract void ParseInternal(byte[] buffer, ref int offset);

        #endregion
    }
}