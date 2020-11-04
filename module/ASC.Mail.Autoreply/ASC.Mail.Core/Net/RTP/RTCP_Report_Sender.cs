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