/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    /// This class represents RTCP compound packet.
    /// </summary>
    public class RTCP_CompoundPacket
    {
        #region Members

        private readonly List<RTCP_Packet> m_pPackets;

        #endregion

        #region Properties

        /// <summary>
        /// Gets compound packets.
        /// </summary>
        public List<RTCP_Packet> Packets
        {
            get { return m_pPackets; }
        }

        /// <summary>
        /// Gets total packets size in bytes which is needed for this compound packet.
        /// </summary>
        internal int TotalSize
        {
            get
            {
                int size = 0;
                foreach (RTCP_Packet packet in m_pPackets)
                {
                    size += packet.Size;
                }

                return size;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal RTCP_CompoundPacket()
        {
            m_pPackets = new List<RTCP_Packet>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses RTP compound packet.
        /// </summary>
        /// <param name="buffer">Data buffer..</param>
        /// <param name="count">Number of bytes in the <b>buffer</b>.</param>
        /// <returns>Returns parsed RTP packet.</returns>
        public static RTCP_CompoundPacket Parse(byte[] buffer, int count)
        {
            /* Compound packet stucture:
                 Encryption prefix
                    If and only if the compound packet is to be encrypted, it is prefixed by a 
                    random 32-bit quantity redrawn for every compound packet transmitted. 

                 SR or RR
                    The first RTCP packet in the compound packet must always be a report 
                    packet to facilitate header validation as described in Appendix A.2. 
                    This is true even if no data has been sent nor received, in which case an 
                    empty RR is sent, and even if the only other RTCP packet in the compound packet is a BYE. 

                 Additional RRs
                    If the number of sources for which reception statistics are being reported 
                    exceeds 31, the number that will fit into one SR or RR packet, then additional 
                    RR packets should follow the initial report packet. 

                 SDES
                    An SDES packet containing a CNAME item must be included in each compound RTCP packet. 
                    Other source description items may optionally be included if required by a particular 
                    application, subject to bandwidth constraints (see Section 6.2.2). 

                 BYE or APP
                    Other RTCP packet types, including those yet to be defined, may follow in any order, 
                    except that BYE should be the last packet sent with a given SSRC/CSRC. 
                    Packet types may appear more than once. 
            */

            int offset = 0;

            RTCP_CompoundPacket packet = new RTCP_CompoundPacket();
            while (offset < count)
            {
                try
                {
                    packet.m_pPackets.Add(RTCP_Packet.Parse(buffer, ref offset));
                }
                catch
                {
                    // Skip invalid or unknown packets.
                }
            }

            return packet;
        }

        /// <summary>
        /// Gets RTCP compound packet as raw byte data.
        /// </summary>
        /// <returns>Returns compound packet as raw byte data.</returns>
        public byte[] ToByte()
        {
            byte[] retVal = new byte[TotalSize];
            int offset = 0;
            ToByte(retVal, ref offset);

            return retVal;
        }

        /// <summary>
        /// Stores this compund packet to specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store data.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void ToByte(byte[] buffer, ref int offset)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' value must be >= 0.");
            }

            foreach (RTCP_Packet packet in m_pPackets)
            {
                packet.ToByte(buffer, ref offset);
            }
        }

        /// <summary>
        /// Validates RTCP compound packet.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid RTCP compound packet.</exception>
        public void Validate()
        {
            /* RFC 3550 A.2 RTCP Header Validity Checks.
                The following checks should be applied to RTCP packets.

                o  RTP version field must equal 2.

                o  The payload type field of the first RTCP packet in a compound
                   packet must be equal to SR or RR.

                o  The padding bit (P) should be zero for the first packet of a
                   compound RTCP packet because padding should only be applied, if it
                   is needed, to the last packet.

                o  The length fields of the individual RTCP packets must add up to
                   the overall length of the compound RTCP packet as received.  This
                   is a fairly strong check.
            */

            if (m_pPackets.Count == 0)
            {
                throw new ArgumentException("No RTCP packets.");
            }

            // Check version and padding.
            for (int i = 0; i < m_pPackets.Count; i++)
            {
                RTCP_Packet packet = m_pPackets[i];
                if (packet.Version != 2)
                {
                    throw new ArgumentException("RTP version field must equal 2.");
                }
                if (i < (m_pPackets.Count - 1) && packet.IsPadded)
                {
                    throw new ArgumentException("Only the last packet in RTCP compound packet may be padded.");
                }
            }

            // The first RTCP packet in a compound packet must be equal to SR or RR.
            if (m_pPackets[0].Type != RTCP_PacketType.SR || m_pPackets[0].Type != RTCP_PacketType.RR)
            {
                throw new ArgumentException(
                    "The first RTCP packet in a compound packet must be equal to SR or RR.");
            }
        }

        #endregion
    }
}