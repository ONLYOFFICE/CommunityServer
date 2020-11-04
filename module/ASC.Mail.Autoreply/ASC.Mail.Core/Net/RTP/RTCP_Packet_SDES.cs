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