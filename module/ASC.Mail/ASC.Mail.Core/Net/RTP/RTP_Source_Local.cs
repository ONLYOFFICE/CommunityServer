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
    using System.Net;

    #endregion

    /// <summary>
    /// This class represents local source what we send.
    /// </summary>
    /// <remarks>Source indicates an entity sending packets, either RTP and/or RTCP.
    /// Sources what send RTP packets are called "active", only RTCP sending ones are "passive".
    /// </remarks>
    public class RTP_Source_Local : RTP_Source
    {
        #region Members

        private RTP_SendStream m_pStream;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true.
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

                return true;
            }
        }

        /// <summary>
        /// Gets local participant. 
        /// </summary>
        public RTP_Participant_Local Participant
        {
            get { return Session.Session.LocalParticipant; }
        }

        /// <summary>
        /// Gets the stream we send. Value null means that source is passive and doesn't send any RTP data.
        /// </summary>
        public RTP_SendStream Stream
        {
            get { return m_pStream; }
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
        /// <param name="rtcpEP">RTCP end point.</param>
        /// <param name="rtpEP">RTP end point.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b>,<b>rtcpEP</b> or <b>rtpEP</b> is null reference.</exception>
        internal RTP_Source_Local(RTP_Session session, uint ssrc, IPEndPoint rtcpEP, IPEndPoint rtpEP)
            : base(session, ssrc)
        {
            if (rtcpEP == null)
            {
                throw new ArgumentNullException("rtcpEP");
            }
            if (rtpEP == null)
            {
                throw new ArgumentNullException("rtpEP");
            }

            SetRtcpEP(rtcpEP);
            SetRtpEP(rtpEP);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends specified application packet to the RTP session target(s).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <param name="packet">Is raised when <b>packet</b> is null reference.</param>
        public void SendApplicationPacket(RTCP_Packet_APP packet)
        {
            if (State == RTP_SourceState.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            packet.Source = SSRC;

            RTCP_CompoundPacket p = new RTCP_CompoundPacket();
            RTCP_Packet_RR rr = new RTCP_Packet_RR();
            rr.SSRC = SSRC;
            p.Packets.Add(packet);

            // Send APP packet.
            Session.SendRtcpPacket(p);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Closes this source, sends BYE to remote party.
        /// </summary>
        /// <param name="closeReason">Stream closing reason text what is reported to the remote party. Value null means not specified.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        internal override void Close(string closeReason)
        {
            if (State == RTP_SourceState.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            RTCP_CompoundPacket packet = new RTCP_CompoundPacket();
            RTCP_Packet_RR rr = new RTCP_Packet_RR();
            rr.SSRC = SSRC;
            packet.Packets.Add(rr);
            RTCP_Packet_BYE bye = new RTCP_Packet_BYE();
            bye.Sources = new[] {SSRC};
            if (!string.IsNullOrEmpty(closeReason))
            {
                bye.LeavingReason = closeReason;
            }
            packet.Packets.Add(bye);

            // Send packet.
            Session.SendRtcpPacket(packet);

            base.Close(closeReason);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Creates RTP send stream for this source.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when this method is called more than 1 times(source already created).</exception>
        internal void CreateStream()
        {
            if (m_pStream != null)
            {
                throw new InvalidOperationException("Stream is already created.");
            }

            m_pStream = new RTP_SendStream(this);
            m_pStream.Disposed += delegate
                                      {
                                          m_pStream = null;
                                          Dispose();
                                      };

            SetState(RTP_SourceState.Active);
        }

        /// <summary>
        /// Sends specified RTP packet to the session remote party.
        /// </summary>
        /// <param name="packet">RTP packet.</param>
        /// <returns>Returns packet size in bytes.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>packet</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when <b>CreateStream</b> method has been not called.</exception>
        internal int SendRtpPacket(RTP_Packet packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }
            if (m_pStream == null)
            {
                throw new InvalidOperationException("RTP stream is not created by CreateStream method.");
            }

            SetLastRtpPacket(DateTime.Now);
            SetState(RTP_SourceState.Active);

            return Session.SendRtpPacket(m_pStream, packet);
        }

        #endregion
    }
}