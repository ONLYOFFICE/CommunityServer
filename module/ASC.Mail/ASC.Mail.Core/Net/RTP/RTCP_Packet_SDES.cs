/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.RTP
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class represents SDES: Source Description RTCP Packet.
    /// </summary>
    public class RTCP_Packet_SDES : RTCP_Packet
    {
        #region Members

        private readonly List<RTCP_Packet_SDES_Chunk> m_pChunks;
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
            get { return RTCP_PacketType.SDES; }
        }

        /// <summary>
        /// Gets session description(SDES) chunks.
        /// </summary>
        public List<RTCP_Packet_SDES_Chunk> Chunks
        {
            get { return m_pChunks; }
        }

        /// <summary>
        /// Gets number of bytes needed for this packet.
        /// </summary>
        public override int Size
        {
            get
            {
                int size = 4;
                foreach (RTCP_Packet_SDES_Chunk chunk in m_pChunks)
                {
                    size += chunk.Size;
                }

                return size;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal RTCP_Packet_SDES()
        {
            m_pChunks = new List<RTCP_Packet_SDES_Chunk>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stores SDES packet to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store SDES packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        public override void ToByte(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.5 SDES: Source Description RTCP Packet.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            header |V=2|P|    SC   |  PT=SDES=202  |             length            |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            chunk  |                          SSRC/CSRC_1                          |
              1    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                           SDES items                          |
                   |                              ...                              |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            chunk  |                          SSRC/CSRC_2                          |
              2    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                           SDES items                          |
                   |                              ...                              |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            */

            // V=2 P SC
            buffer[offset++] = (byte) (2 << 6 | 0 << 5 | m_pChunks.Count & 0x1F);
            // PT=SDES=202
            buffer[offset++] = 202;
            // length
            int lengthOffset = offset;
            buffer[offset++] = 0; // We fill it at last, when length is known.
            buffer[offset++] = 0; // We fill it at last, when length is known.

            int chunksStartOffset = offset;

            // Add chunks.            
            foreach (RTCP_Packet_SDES_Chunk chunk in m_pChunks)
            {
                chunk.ToByte(buffer, ref offset);
            }

            // NOTE: Size in 32-bit boundary, header not included.
            int length = (offset - chunksStartOffset)/4;
            // length
            buffer[lengthOffset] = (byte) ((length >> 8) & 0xFF);
            buffer[lengthOffset + 1] = (byte) ((length) & 0xFF);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Parses Source Description(SDES) packet from data buffer.
        /// </summary>
        /// <param name="buffer">Buffer what contains SDES packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        protected override void ParseInternal(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.5 SDES: Source Description RTCP Packet.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            header |V=2|P|    SC   |  PT=SDES=202  |             length            |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            chunk  |                          SSRC/CSRC_1                          |
              1    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                           SDES items                          |
                   |                              ...                              |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            chunk  |                          SSRC/CSRC_2                          |
              2    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                           SDES items                          |
                   |                              ...                              |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            */

            m_Version = buffer[offset] >> 6;
            bool isPadded = Convert.ToBoolean((buffer[offset] >> 5) & 0x1);
            int sourceCount = buffer[offset++] & 0x1F;
            int type = buffer[offset++];
            int length = buffer[offset++] << 8 | buffer[offset++];
            if (isPadded)
            {
                PaddBytesCount = buffer[offset + length];
            }

            // Read chunks
            for (int i = 0; i < sourceCount; i++)
            {
                RTCP_Packet_SDES_Chunk chunk = new RTCP_Packet_SDES_Chunk();
                chunk.Parse(buffer, ref offset);
                m_pChunks.Add(chunk);
            }
        }

        #endregion
    }
}