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
    using System.Net;

    #endregion

    /// <summary>
    /// This class represents RTP source.
    /// </summary>
    /// <remarks>Source indicates an entity sending packets, either RTP and/or RTCP.
    /// Sources what send RTP packets are called "active", only RTCP sending ones are "passive".
    /// Source can be local(we send RTP and/or RTCP remote party) or remote(remote party sends RTP and/or RTCP to us).
    /// </remarks>
    public abstract class RTP_Source
    {
        #region Events

        /// <summary>
        /// Is raised when source is closed (by BYE).
        /// </summary>
        public event EventHandler Closed = null;

        /// <summary>
        /// Is raised when source is disposing.
        /// </summary>
        public event EventHandler Disposing = null;

        /// <summary>
        /// Is raised when source state has changed.
        /// </summary>
        public event EventHandler StateChanged = null;

        #endregion

        #region Members

        private readonly DateTime m_LastRRTime = DateTime.MinValue;
        private string m_CloseReason;
        private DateTime m_LastActivity = DateTime.Now;
        private DateTime m_LastRtcpPacket = DateTime.MinValue;
        private DateTime m_LastRtpPacket = DateTime.MinValue;
        private IPEndPoint m_pRtcpEP;
        private IPEndPoint m_pRtpEP;
        private RTP_Session m_pSession;
        private uint m_SSRC;
        private RTP_SourceState m_State = RTP_SourceState.Passive;

        #endregion

        #region Properties

        /// <summary>
        /// Gets source state.
        /// </summary>
        public RTP_SourceState State
        {
            get { return m_State; }
        }

        /// <summary>
        /// Gets owner RTP session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public RTP_Session Session
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSession;
            }
        }

        /// <summary>
        /// Gets synchronization source ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public uint SSRC
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SSRC;
            }
        }

        /// <summary>
        /// Gets source RTCP end point. Value null means source haven't sent any RTCP packet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public IPEndPoint RtcpEP
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRtcpEP;
            }
        }

        /// <summary>
        /// Gets source RTP end point. Value null means source haven't sent any RTCP packet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public IPEndPoint RtpEP
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRtpEP;
            }
        }

        /// <summary>
        /// Gets if source is local or remote source.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public abstract bool IsLocal { get; }

        /// <summary>
        /// Gets last time when source sent RTP or RCTP packet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime LastActivity
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LastActivity;
            }
        }

        /// <summary>
        /// Gets last time when source sent RTCP packet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime LastRtcpPacket
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LastRtcpPacket;
            }
        }

        /// <summary>
        /// Gets last time when source sent RTP packet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime LastRtpPacket
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LastRtpPacket;
            }
        }

        /// <summary>
        /// Gets last time when source sent RTCP RR report.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime LastRRTime
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LastRRTime;
            }
        }

        /// <summary>
        /// Gets source closing reason. Value null means not specified.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string CloseReason
        {
            get
            {
                if (m_State == RTP_SourceState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_CloseReason;
            }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets source CNAME. Value null means that source not binded to participant.
        /// </summary>
        internal abstract string CName { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner RTP session.</param>
        /// <param name="ssrc">Synchronization source ID.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null reference.</exception>
        internal RTP_Source(RTP_Session session, uint ssrc)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            m_pSession = session;
            m_SSRC = ssrc;
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        internal virtual void Dispose()
        {
            if (m_State == RTP_SourceState.Disposed)
            {
                return;
            }
            OnDisposing();
            SetState(RTP_SourceState.Disposed);

            m_pSession = null;
            m_pRtcpEP = null;
            m_pRtpEP = null;

            Closed = null;
            Disposing = null;
            StateChanged = null;
        }

        /// <summary>
        /// Closes specified source.
        /// </summary>
        /// <param name="closeReason">Closing reason. Value null means not specified.</param>
        internal virtual void Close(string closeReason)
        {
            m_CloseReason = closeReason;

            OnClosed();
            Dispose();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>Closed</b> event.
        /// </summary>
        private void OnClosed()
        {
            if (Closed != null)
            {
                Closed(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>Disposing</b> event.
        /// </summary>
        private void OnDisposing()
        {
            if (Disposing != null)
            {
                Disposing(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>StateChanged</b> event.
        /// </summary>
        private void OnStateChaged()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Sets property <b>RtcpEP</b> value.
        /// </summary>
        /// <param name="ep">IP end point.</param>
        internal void SetRtcpEP(IPEndPoint ep)
        {
            m_pRtcpEP = ep;
        }

        /// <summary>
        /// Sets property <b>RtpEP</b> value.
        /// </summary>
        /// <param name="ep">IP end point.</param>
        internal void SetRtpEP(IPEndPoint ep)
        {
            m_pRtpEP = ep;
        }

        /// <summary>
        /// Sets source active/passive state.
        /// </summary>
        /// <param name="active">If true, source switches to active, otherwise to passive.</param>
        internal void SetActivePassive(bool active)
        {
            if (active) {}
            else {}

            // TODO:
        }

        /// <summary>
        /// Sets <b>LastRtcpPacket</b> property value.
        /// </summary>
        /// <param name="time">Time.</param>
        internal void SetLastRtcpPacket(DateTime time)
        {
            m_LastRtcpPacket = time;
            m_LastActivity = time;
        }

        /// <summary>
        /// Sets <b>LastRtpPacket</b> property value.
        /// </summary>
        /// <param name="time">Time.</param>
        internal void SetLastRtpPacket(DateTime time)
        {
            m_LastRtpPacket = time;
            m_LastActivity = time;
        }

        /// <summary>
        /// Sets property LastRR value.
        /// </summary>
        /// <param name="rr">RTCP RR report.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>rr</b> is null reference.</exception>
        internal void SetRR(RTCP_Packet_ReportBlock rr)
        {
            if (rr == null)
            {
                throw new ArgumentNullException("rr");
            }
        }

        /// <summary>
        /// Generates new SSRC value. This must be called only if SSRC collision of local source.
        /// </summary>
        internal void GenerateNewSSRC()
        {
            m_SSRC = RTP_Utils.GenerateSSRC();
        }

        #endregion

        /// <summary>
        /// Sets source state.
        /// </summary>
        /// <param name="state">New source state.</param>
        protected void SetState(RTP_SourceState state)
        {
            if (m_State == RTP_SourceState.Disposed)
            {
                return;
            }

            if (m_State != state)
            {
                m_State = state;

                OnStateChaged();
            }
        }
    }
}