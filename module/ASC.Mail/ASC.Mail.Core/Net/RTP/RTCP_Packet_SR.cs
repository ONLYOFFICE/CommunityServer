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
    /// This class represents SR: Sender Report RTCP Packet.
    /// </summary>
    public class RTCP_Packet_SR : RTCP_Packet
    {
        #region Members

        private readonly List<RTCP_Packet_ReportBlock> m_pReportBlocks;
        private ulong m_NtpTimestamp;
        private uint m_RtpTimestamp;
        private uint m_SenderOctetCount;
        private uint m_SenderPacketCount;
        private uint m_SSRC;
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
            get { return RTCP_PacketType.SR; }
        }

        /// <summary>
        /// Gets sender synchronization source identifier.
        /// </summary>
        public uint SSRC
        {
            get { return m_SSRC; }
        }

        /// <summary>
        /// Gets or sets the wallclock time (see Section 4) when this report was sent.
        /// </summary>
        public ulong NtpTimestamp
        {
            get { return m_NtpTimestamp; }

            set { m_NtpTimestamp = value; }
        }

        /// <summary>
        /// Gets RTP timestamp.
        /// </summary>
        public uint RtpTimestamp
        {
            get { return m_RtpTimestamp; }

            set { m_RtpTimestamp = value; }
        }

        /// <summary>
        /// Gets how many packets sender has sent.
        /// </summary>
        public uint SenderPacketCount
        {
            get { return m_SenderPacketCount; }

            set { m_SenderPacketCount = value; }
        }

        /// <summary>
        /// Gets how many bytes sender has sent.
        /// </summary>
        public uint SenderOctetCount
        {
            get { return m_SenderOctetCount; }

            set { m_SenderOctetCount = value; }
        }

        /// <summary>
        /// Gets reports blocks.
        /// </summary>
        public List<RTCP_Packet_ReportBlock> ReportBlocks
        {
            get { return m_pReportBlocks; }
        }

        /// <summary>
        /// Gets number of bytes needed for this packet.
        /// </summary>
        public override int Size
        {
            get { return 28 + (24*m_pReportBlocks.Count); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ssrc">Source(sender) ID.</param>
        internal RTCP_Packet_SR(uint ssrc)
        {
            m_SSRC = ssrc;

            m_pReportBlocks = new List<RTCP_Packet_ReportBlock>();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal RTCP_Packet_SR()
        {
            m_pReportBlocks = new List<RTCP_Packet_ReportBlock>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stores sender report(SR) packet to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store SR packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public override void ToByte(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.4.1 SR: Sender Report RTCP Packet.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            header |V=2|P|    RC   |   PT=SR=200   |             length            |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                         SSRC of sender                        |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            sender |              NTP timestamp, most significant word             |
            info   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |             NTP timestamp, least significant word             |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                         RTP timestamp                         |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                     sender's packet count                     |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                      sender's octet count                     |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            report |                 SSRC_1 (SSRC of first source)                 |
            block  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
              1    | fraction lost |       cumulative number of packets lost       |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |           extended highest sequence number received           |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                      interarrival jitter                      |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                         last SR (LSR)                         |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                   delay since last SR (DLSR)                  |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            report |                 SSRC_2 (SSRC of second source)                |
            block  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
              2    :                               ...                             :
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
                   |                  profile-specific extensions                  |
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

            // NOTE: Size in 32-bit boundary, header not included.
            int length = (24 + (m_pReportBlocks.Count*24))/4;

            // V P RC
            buffer[offset++] = (byte) (2 << 6 | 0 << 5 | (m_pReportBlocks.Count & 0x1F));
            // PT=SR=200
            buffer[offset++] = 200;
            // length
            buffer[offset++] = (byte) ((length >> 8) & 0xFF);
            buffer[offset++] = (byte) ((length) & 0xFF);
            // SSRC
            buffer[offset++] = (byte) ((m_SSRC >> 24) & 0xFF);
            buffer[offset++] = (byte) ((m_SSRC >> 16) & 0xFF);
            buffer[offset++] = (byte) ((m_SSRC >> 8) & 0xFF);
            buffer[offset++] = (byte) ((m_SSRC) & 0xFF);
            // NTP timestamp
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 56) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 48) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 40) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 32) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 24) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 16) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp >> 8) & 0xFF);
            buffer[offset++] = (byte) ((m_NtpTimestamp) & 0xFF);
            // RTP timestamp
            buffer[offset++] = (byte) ((m_RtpTimestamp >> 24) & 0xFF);
            buffer[offset++] = (byte) ((m_RtpTimestamp >> 16) & 0xFF);
            buffer[offset++] = (byte) ((m_RtpTimestamp >> 8) & 0xFF);
            buffer[offset++] = (byte) ((m_RtpTimestamp) & 0xFF);
            // sender's packet count
            buffer[offset++] = (byte) ((m_SenderPacketCount >> 24) & 0xFF);
            buffer[offset++] = (byte) ((m_SenderPacketCount >> 16) & 0xFF);
            buffer[offset++] = (byte) ((m_SenderPacketCount >> 8) & 0xFF);
            buffer[offset++] = (byte) ((m_SenderPacketCount) & 0xFF);
            // sender's octet count
            buffer[offset++] = (byte) ((m_SenderOctetCount >> 24) & 0xFF);
            buffer[offset++] = (byte) ((m_SenderOctetCount >> 16) & 0xFF);
            buffer[offset++] = (byte) ((m_SenderOctetCount >> 8) & 0xFF);
            buffer[offset++] = (byte) ((m_SenderOctetCount) & 0xFF);
            // Report blocks
            foreach (RTCP_Packet_ReportBlock block in m_pReportBlocks)
            {
                block.ToByte(buffer, ref offset);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Parses RTCP sender report(SR) from specified data buffer.
        /// </summary>
        /// <param name="buffer">Buffer which contains sender report.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        protected override void ParseInternal(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.4.1 SR: Sender Report RTCP Packet.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            header |V=2|P|    RC   |   PT=SR=200   |             length            |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                         SSRC of sender                        |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            sender |              NTP timestamp, most significant word             |
            info   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |             NTP timestamp, least significant word             |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                         RTP timestamp                         |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                     sender's packet count                     |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                      sender's octet count                     |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            report |                 SSRC_1 (SSRC of first source)                 |
            block  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
              1    | fraction lost |       cumulative number of packets lost       |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |           extended highest sequence number received           |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                      interarrival jitter                      |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                         last SR (LSR)                         |
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |                   delay since last SR (DLSR)                  |
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
            report |                 SSRC_2 (SSRC of second source)                |
            block  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
              2    :                               ...                             :
                   +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
                   |                  profile-specific extensions                  |
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
            int reportBlockCount = buffer[offset++] & 0x1F;
            int type = buffer[offset++];
            int length = buffer[offset++] << 8 | buffer[offset++];
            if (isPadded)
            {
                PaddBytesCount = buffer[offset + length];
            }

            m_SSRC =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_NtpTimestamp =
                (ulong)
                (buffer[offset++] << 56 | buffer[offset++] << 48 | buffer[offset++] << 40 |
                 buffer[offset++] << 32 | buffer[offset++] << 24 | buffer[offset++] << 16 |
                 buffer[offset++] << 8 | buffer[offset++]);
            m_RtpTimestamp =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_SenderPacketCount =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_SenderOctetCount =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);

            for (int i = 0; i < reportBlockCount; i++)
            {
                RTCP_Packet_ReportBlock reportBlock = new RTCP_Packet_ReportBlock();
                reportBlock.Parse(buffer, offset);
                m_pReportBlocks.Add(reportBlock);
                offset += 24;
            }
        }

        #endregion
    }
}