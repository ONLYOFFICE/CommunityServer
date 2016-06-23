/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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