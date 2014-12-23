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
    using System.Text;

    #endregion

    /// <summary>
    /// This class represents BYE: Goodbye RTCP Packet.
    /// </summary>
    public class RTCP_Packet_BYE : RTCP_Packet
    {
        #region Members

        private string m_LeavingReason = "";
        private uint[] m_Sources;
        private int m_Version = 2;

        #endregion

        #region Properties

        /// <summary>
        /// Gets RTCP version.
        /// </summary>
        public override int Version
        {
            get { return m_Version; }
        }

        /// <summary>
        /// Gets RTCP packet type.
        /// </summary>
        public override int Type
        {
            get { return RTCP_PacketType.BYE; }
        }

        /// <summary>
        /// Gets or sets SSRC/CSRC identifiers included in this BYE packet. 
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public uint[] Sources
        {
            get { return m_Sources; }

            set
            {
                if (value.Length > 31)
                {
                    throw new ArgumentException("Property 'Sources' can accomodate only 31 entries.");
                }

                m_Sources = value;
            }
        }

        /// <summary>
        /// Gets leaving reason.
        /// </summary>
        public string LeavingReason
        {
            get { return m_LeavingReason; }

            set { m_LeavingReason = value; }
        }

        /// <summary>
        /// Gets number of bytes needed for this packet.
        /// </summary>
        public override int Size
        {
            get
            {
                int size = 4;
                if (m_Sources != null)
                {
                    size += 4*m_Sources.Length;
                }
                if (!string.IsNullOrEmpty(m_LeavingReason))
                {
                    size += Encoding.UTF8.GetByteCount(m_LeavingReason);
                }

                return size;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal RTCP_Packet_BYE() {}

        #endregion

        #region Methods

        /// <summary>
        /// Stores BYE packet to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store BYE packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public override void ToByte(byte[] buffer, ref int offset)
        {
            /* RFC 3550.6.6 BYE: Goodbye RTCP Packet.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |V=2|P|    SC   |   PT=BYE=203  |             length            |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                           SSRC/CSRC                           |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   :                              ...                              :
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
             (opt) |     length    |               reason for leaving            ...
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' value must be >= 0.");
            }

            // Calculate packet body size in bytes.
            int length = 0;
            length += m_Sources.Length*4;
            if (!string.IsNullOrEmpty(m_LeavingReason))
            {
                length += Encoding.UTF8.GetByteCount(m_LeavingReason);
            }

            // V=2 P SC
            buffer[offset++] = (byte) (2 << 6 | 0 << 5 | m_Sources.Length & 0x1F);
            // PT=BYE=203
            buffer[offset++] = 203;
            // length
            buffer[offset++] = (byte) ((length >> 8) & 0xFF);
            buffer[offset++] = (byte) (length & 0xFF);
            // SSRC/CSRC's
            foreach (int source in m_Sources)
            {
                buffer[offset++] = (byte) ((source & 0xFF000000) >> 24);
                buffer[offset++] = (byte) ((source & 0x00FF0000) >> 16);
                buffer[offset++] = (byte) ((source & 0x0000FF00) >> 8);
                buffer[offset++] = (byte) ((source & 0x000000FF));
            }
            // reason for leaving
            if (!string.IsNullOrEmpty(m_LeavingReason))
            {
                byte[] reasonBytes = Encoding.UTF8.GetBytes(m_LeavingReason);
                buffer[offset++] = (byte) reasonBytes.Length;
                Array.Copy(reasonBytes, 0, buffer, offset, reasonBytes.Length);
                offset += reasonBytes.Length;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Parses BYE packet from raw byte[] bye packet.
        /// </summary>
        /// <param name="buffer">Buffer what contains BYE packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        protected override void ParseInternal(byte[] buffer, ref int offset)
        {
            /* RFC 3550.6.6 BYE: Goodbye RTCP Packet.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |V=2|P|    SC   |   PT=BYE=203  |             length            |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                           SSRC/CSRC                           |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   :                              ...                              :
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
             (opt) |     length    |               reason for leaving            ...
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' value must be >= 0.");
            }

            m_Version = buffer[offset] >> 6;
            bool isPadded = Convert.ToBoolean((buffer[offset] >> 5) & 0x1);
            int sourceCount = buffer[offset++] & 0x1F;
            int type = buffer[offset++];
            int length = buffer[offset++] << 8 | buffer[offset++];
            if (isPadded)
            {
                PaddBytesCount = buffer[offset + length];
            }

            m_Sources = new uint[sourceCount];
            for (int i = 0; i < sourceCount; i++)
            {
                m_Sources[i] =
                    (uint)
                    (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 |
                     buffer[offset++]);
            }

            // See if we have optional reason text.
            if (length > m_Sources.Length*4)
            {
                int reasonLength = buffer[offset++];
                m_LeavingReason = Encoding.UTF8.GetString(buffer, offset, reasonLength);
                offset += reasonLength;
            }
        }

        #endregion
    }
}