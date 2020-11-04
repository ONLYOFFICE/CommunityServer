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
    /// This class holds receiver report info.
    /// </summary>
    public class RTCP_Report_Receiver
    {
        #region Members

        private readonly uint m_CumulativePacketsLost;
        private readonly uint m_DelaySinceLastSR;
        private readonly uint m_ExtHigestSeqNumber;
        private readonly uint m_FractionLost;
        private readonly uint m_Jitter;
        private readonly uint m_LastSR;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fraction of RTP data packets from source SSRC lost since the previous SR or 
        /// RR packet was sent.
        /// </summary>
        public uint FractionLost
        {
            get { return m_FractionLost; }
        }

        /// <summary>
        /// Gets total number of RTP data packets from source SSRC that have
        /// been lost since the beginning of reception.
        /// </summary>
        public uint CumulativePacketsLost
        {
            get { return m_CumulativePacketsLost; }
        }

        /// <summary>
        /// Gets extended highest sequence number received.
        /// </summary>
        public uint ExtendedSequenceNumber
        {
            get { return m_ExtHigestSeqNumber; }
        }

        /// <summary>
        /// Gets an estimate of the statistical variance of the RTP data packet
        /// interarrival time, measured in timestamp units and expressed as an
        /// unsigned integer.
        /// </summary>
        public uint Jitter
        {
            get { return m_Jitter; }
        }

        /// <summary>
        /// Gets when last sender report(SR) was recieved.
        /// </summary>
        public uint LastSR
        {
            get { return m_LastSR; }
        }

        /// <summary>
        /// Gets delay since last sender report(SR) was received.
        /// </summary>
        public uint DelaySinceLastSR
        {
            get { return m_DelaySinceLastSR; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="rr">RTCP RR report.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>rr</b> is null reference.</exception>
        internal RTCP_Report_Receiver(RTCP_Packet_ReportBlock rr)
        {
            if (rr == null)
            {
                throw new ArgumentNullException("rr");
            }

            m_FractionLost = rr.FractionLost;
            m_CumulativePacketsLost = rr.CumulativePacketsLost;
            m_ExtHigestSeqNumber = rr.ExtendedHighestSeqNo;
            m_Jitter = rr.Jitter;
            m_LastSR = rr.LastSR;
            m_DelaySinceLastSR = rr.DelaySinceLastSR;
        }

        #endregion
    }
}