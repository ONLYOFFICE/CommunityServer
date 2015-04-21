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
    /// This class represents Application-Defined RTCP Packet.
    /// </summary>
    public class RTCP_Packet_APP : RTCP_Packet
    {
        #region Members

        private byte[] m_Data;
        private string m_Name = "";
        private uint m_Source;
        private int m_SubType;
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
            get { return RTCP_PacketType.APP; }
        }

        /// <summary>
        /// Gets subtype value.
        /// </summary>
        public int SubType
        {
            get { return m_SubType; }
        }

        /// <summary>
        /// Gets sender synchronization(SSRC) or contributing(CSRC) source identifier.
        /// </summary>
        public uint Source
        {
            get { return m_Source; }

            set { m_Source = value; }
        }

        /// <summary>
        /// Gets 4 ASCII char packet name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets application-dependent data.
        /// </summary>
        public byte[] Data
        {
            get { return m_Data; }
        }

        /// <summary>
        /// Gets number of bytes needed for this packet.
        /// </summary>
        public override int Size
        {
            get { return 12 + m_Data.Length; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal RTCP_Packet_APP()
        {
            m_Name = "xxxx";
            m_Data = new byte[0];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stores APP packet to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store APP packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public override void ToByte(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.7 APP: Application-Defined RTCP Packet.
                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |V=2|P| subtype |   PT=APP=204  |             length            |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                           SSRC/CSRC                           |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                          name (ASCII)                         |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                   application-dependent data                ...
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

            int length = 8 + m_Data.Length;

            // V P subtype
            buffer[offset++] = (byte) (2 << 6 | 0 << 5 | m_SubType & 0x1F);
            // PT=APP=204
            buffer[offset++] = 204;
            // length
            buffer[offset++] = (byte) ((length >> 8) | 0xFF);
            buffer[offset++] = (byte) ((length) | 0xFF);
            // SSRC/CSRC            
            buffer[offset++] = (byte) ((m_Source >> 24) | 0xFF);
            buffer[offset++] = (byte) ((m_Source >> 16) | 0xFF);
            buffer[offset++] = (byte) ((m_Source >> 8) | 0xFF);
            buffer[offset++] = (byte) ((m_Source) | 0xFF);
            // name          
            buffer[offset++] = (byte) m_Name[0];
            buffer[offset++] = (byte) m_Name[1];
            buffer[offset++] = (byte) m_Name[2];
            buffer[offset++] = (byte) m_Name[2];
            // application-dependent data
            Array.Copy(m_Data, 0, buffer, offset, m_Data.Length);
            offset += m_Data.Length;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Parses APP packet from the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer what conatins APP packet.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        protected override void ParseInternal(byte[] buffer, ref int offset)
        {
            /* RFC 3550 6.7 APP: Application-Defined RTCP Packet.
                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |V=2|P| subtype |   PT=APP=204  |             length            |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                           SSRC/CSRC                           |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                          name (ASCII)                         |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                   application-dependent data                ...
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

            m_Version = buffer[offset++] >> 6;
            bool isPadded = Convert.ToBoolean((buffer[offset] >> 5) & 0x1);
            int subType = buffer[offset++] & 0x1F;
            int type = buffer[offset++];
            int length = buffer[offset++] << 8 | buffer[offset++];
            if (isPadded)
            {
                PaddBytesCount = buffer[offset + length];
            }

            m_SubType = subType;
            m_Source =
                (uint)
                (buffer[offset++] << 24 | buffer[offset++] << 16 | buffer[offset++] << 8 | buffer[offset++]);
            m_Name = ((char) buffer[offset++]) + ((char) buffer[offset++]).ToString() +
                     ((char) buffer[offset++]) + ((char) buffer[offset++]);
            m_Data = new byte[length - 8];
            Array.Copy(buffer, offset, m_Data, 0, m_Data.Length);
        }

        #endregion
    }
}