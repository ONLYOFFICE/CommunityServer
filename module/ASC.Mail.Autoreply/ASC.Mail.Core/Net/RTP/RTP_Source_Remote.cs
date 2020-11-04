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
    /// This class represents RTP remote source what we receive.
    /// </summary>
    /// <remarks>Source indicates an entity sending packets, either RTP and/or RTCP.
    /// Sources what send RTP packets are called "active", only RTCP sending ones are "passive".
    /// </remarks>
    public class RTP_Source_Remote : RTP_Source
    {
        #region Events

        /// <summary>
        /// Is raised when source sends RTCP APP packet.
        /// </summary>
        public event EventHandler<EventArgs<RTCP_Packet_APP>> ApplicationPacket = null;

        #endregion

        #region Members

        private RTP_Participant_Remote m_pParticipant;
        private RTP_ReceiveStream m_pStream;

        #endregion

        #region Properties

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public override bool IsLocal
        {
            get
            {
                if (State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets remote participant. 
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public RTP_Participant_Remote Participant
        {
            get
            {
                if (State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pParticipant;
            }
        }

        /// <summary>
        /// Gets the stream we receive. Value null means that source is passive and doesn't send any RTP data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public RTP_ReceiveStream Stream
        {
            get
            {
                if (State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pStream;
            }
        }

        /// <summary>
        /// Gets source CNAME. Value null means that source not binded to participant.
        /// </summary>
        internal override string CName
        {
            get
            {
                if (Participant != null)
                {
                    return null;
                }
                else
                {
                    return Participant.CNAME;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner RTP session.</param>
        /// <param name="ssrc">Synchronization source ID.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null reference.</exception>
        internal RTP_Source_Remote(RTP_Session session, uint ssrc) : base(session, ssrc) {}

        #endregion

        #region Overrides

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        internal override void Dispose()
        {
            m_pParticipant = null;
            if (m_pStream != null)
            {
                m_pStream.Dispose();
            }

            ApplicationPacket = null;

            base.Dispose();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>ApplicationPacket</b> event.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>packet</b> is null reference.</exception>
        private void OnApplicationPacket(RTCP_Packet_APP packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            if (ApplicationPacket != null)
            {
                ApplicationPacket(this, new EventArgs<RTCP_Packet_APP>(packet));
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Sets source owner participant.
        /// </summary>
        /// <param name="participant">RTP participant.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>participant</b> is null reference.</exception>
        internal void SetParticipant(RTP_Participant_Remote participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException("participant");
            }

            m_pParticipant = participant;
        }

        /// <summary>
        /// Is called when RTP session receives new RTP packet.
        /// </summary>
        /// <param name="packet">RTP packet.</param>
        /// <param name="size">Packet size in bytes.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>packet</b> is null reference.</exception>
        internal void OnRtpPacketReceived(RTP_Packet packet, int size)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            SetLastRtpPacket(DateTime.Now);

            // Passive source and first RTP packet.
            if (m_pStream == null)
            {
                m_pStream = new RTP_ReceiveStream(Session, this, packet.SeqNo);

                SetState(RTP_SourceState.Active);
            }

            m_pStream.Process(packet, size);
        }

        /// <summary>
        /// This method is called when this source got sender report.
        /// </summary>
        /// <param name="report">Sender report.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>report</b> is null reference.</exception>
        internal void OnSenderReport(RTCP_Report_Sender report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            if (m_pStream != null)
            {
                m_pStream.SetSR(report);
            }
        }

        /// <summary>
        /// This method is called when this source got RTCP APP apcket.
        /// </summary>
        /// <param name="packet">RTCP APP packet.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>packet</b> is null reference.</exception>
        internal void OnAppPacket(RTCP_Packet_APP packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            OnApplicationPacket(packet);
        }

        #endregion
    }
}