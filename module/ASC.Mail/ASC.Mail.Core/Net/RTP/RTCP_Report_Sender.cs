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
    /// This class holds sender report info.
    /// </summary>
    public class RTCP_Report_Sender
    {
        #region Members

        private readonly ulong m_NtpTimestamp;
        private readonly uint m_RtpTimestamp;
        private readonly uint m_SenderOctetCount;
        private readonly uint m_SenderPacketCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the wallclock time (see Section 4) when this report was sent.
        /// </summary>
        public ulong NtpTimestamp
        {
            get { return m_NtpTimestamp; }
        }

        /// <summary>
        /// Gets RTP timestamp.
        /// </summary>
        public uint RtpTimestamp
        {
            get { return m_RtpTimestamp; }
        }

        /// <summary>
        /// Gets how many packets sender has sent.
        /// </summary>
        public uint SenderPacketCount
        {
            get { return m_SenderPacketCount; }
        }

        /// <summary>
        /// Gets how many bytes sender has sent.
        /// </summary>
        public uint SenderOctetCount
        {
            get { return m_SenderOctetCount; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sr">RTCP SR report.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>sr</b> is null reference.</exception>
        internal RTCP_Report_Sender(RTCP_Packet_SR sr)
        {
            if (sr == null)
            {
                throw new ArgumentNullException("sr");
            }

            m_NtpTimestamp = sr.NtpTimestamp;
            m_RtpTimestamp = sr.RtpTimestamp;
            m_SenderPacketCount = sr.SenderPacketCount;
            m_SenderOctetCount = sr.SenderOctetCount;
        }

        #endregion
    }
}