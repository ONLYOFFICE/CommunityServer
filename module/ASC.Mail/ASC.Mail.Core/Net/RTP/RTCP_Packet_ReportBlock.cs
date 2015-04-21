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

    #endregion

    /// <summary>
    /// This class represents RTCP sender report(SR) or reciver report(RR) packet report block.
    /// </summary>
    public class RTCP_Packet_ReportBlock
    {
        #region Members

        private uint m_CumulativePacketsLost;
        private uint m_DelaySinceLastSR;
        private uint m_ExtHighestSeqNumber;
        private uint m_FractionLost;
        private uint m_Jitter;
        private uint m_LastSR;
        private uint m_SSRC;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the SSRC identifier of the source to which the information in this reception report block pertains.
        /// </summary>
        public uint SSRC
        {
            get { return m_SSRC; }
        }

        /// <summary>
        /// Gets or sets the fraction of RTP data packets from source SSRC lost since the previous SR or 
        /// RR packet was sent.
        /// </summary>
        public uint FractionLost
        {
            get { return m_FractionLost; }

            set { m_FractionLost = value; }
        }

        /// <summary>
        /// Gets or sets total number of RTP data packets from source SSRC that have
        /// been lost since the beginning of reception.
        /// </summary>
        public uint CumulativePacketsLost
        {
            get { return m_CumulativePacketsLost; }

            set { m_CumulativePacketsLost = value; }
        }

        /// <summary>
        /// Gets or sets extended highest sequence number received.
        /// </summary>
        public uint ExtendedHighestSeqNo
        {
            get { return m_ExtHighestSeqNumber; }

            set { m_ExtHighestSeqNumber = value; }
        }

        /// <summary>
        /// Gets or sets an estimate of the statistical variance of the RTP data packet
        /// interarrival time, measured in timestamp units and expressed as an unsigned integer.
        /// </summary>
        public uint Jitter
        {
            get { return m_Jitter; }

            set { m_Jitter = value; }
        }

        /// <summary>
        /// Gets or sets The middle 32 bits out of 64 in the NTP timestamp (as explained in Section 4) received as part of 
        /// the most recent RTCP sender report (SR) packet from source SSRC_n. If no SR has been received yet, the field is set to zero.
        /// </summary>
        public uint LastSR
        {
            get { return m_LastSR; }

            set { m_LastSR = value; }
        }

        /// <summary>
        /// Gets or sets the delay, expressed in units of 1/65536 seconds, between receiving the last SR packet from 
        /// source SSRC_n and sending this reception report block.  If no SR packet has been received yet from SSRC_n, 
        /// the DLSR field is set to zero.
        /// </summary>
        public uint DelaySinceLastSR
        {
            get { return m_DelaySinceLastSR; }

            set { m_DelaySinceLastSR = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ssrc">Source ID.</param>
        internal RTCP_Packet_ReportBlock(uint ssrc)
        {
            m_SSRC = ssrc;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal RTCP_Packet_ReportBlock() {}

        #endregion

        #region Methods

        /// <summary>
        /// Parses RTCP report block (part of SR or RR packet) from specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer from where to read report block.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Parse(byte[] buffer, int offset)
        {
            /* RFC 3550 6.4.1. 
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
            */

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' value must be >= 0.");
            }

            m_SSRC =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_FractionLost = buffer[offset++];
            m_CumulativePacketsLost =
                (uint) (buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_ExtHighestSeqNumber =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_Jitter =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_LastSR =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_DelaySinceLastSR =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
        }

        /// <summary>
        /// Stores report block to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store data.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void ToByte(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.4.1. 
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
            */

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' must be >= 0.");
            }
            if (offset + 24 > buffer.Length)
            {
                throw new ArgumentException("Argument 'buffer' has not enough room to store report block.");
            }

            // SSRC
            buffer[offset++] = (byte) ((m_SSRC >> 24) | 0xFF);
            buffer[offset++] = (byte) ((m_SSRC >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_SSRC >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_SSRC) | 0xFF);
            // fraction lost
            buffer[offset++] = (byte) m_FractionLost;
            // cumulative packets lost
            buffer[offset++] = (byte) ((m_CumulativePacketsLost >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_CumulativePacketsLost >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_CumulativePacketsLost) | 0xFF);
            // extended highest sequence number
            buffer[offset++] = (byte) ((m_ExtHighestSeqNumber >> 24) | 0xFF);
            buffer[offset++] = (byte) ((m_ExtHighestSeqNumber >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_ExtHighestSeqNumber >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_ExtHighestSeqNumber) | 0xFF);
            // jitter
            buffer[offset++] = (byte) ((m_Jitter >> 24) | 0xFF);
            buffer[offset++] = (byte) ((m_Jitter >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_Jitter >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_Jitter) | 0xFF);
            // last SR
            buffer[offset++] = (byte) ((m_LastSR >> 24) | 0xFF);
            buffer[offset++] = (byte) ((m_LastSR >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_LastSR >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_LastSR) | 0xFF);
            // delay since last SR
            buffer[offset++] = (byte) ((m_DelaySinceLastSR >> 24) | 0xFF);
            buffer[offset++] = (byte) ((m_DelaySinceLastSR >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_DelaySinceLastSR >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_DelaySinceLastSR) | 0xFF);
        }

        #endregion
    }
}