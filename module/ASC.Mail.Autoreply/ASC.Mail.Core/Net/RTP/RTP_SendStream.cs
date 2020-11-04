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
    /// Implements RTP session send stream.
    /// </summary>
    public class RTP_SendStream
    {
        #region Events

        /// <summary>
        /// Is raised when stream is closed.
        /// </summary>
        public event EventHandler Closed = null;

        /// <summary>
        /// Is raised when stream has disposed.
        /// </summary>
        public event EventHandler Disposed = null;

        #endregion

        #region Members

        private bool m_IsDisposed;
        private uint m_LastPacketRtpTimestamp;
        private DateTime m_LastPacketTime;
        private RTP_Source_Local m_pSource;
        private int m_RtcpCyclesSinceWeSent = 9999;
        private long m_RtpBytesSent;
        private long m_RtpDataBytesSent;
        private long m_RtpPacketsSent;
        private int m_SeqNo;
        private int m_SeqNoWrapCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        /// <summary>
        /// Gets stream owner RTP session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public RTP_Session Session
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSource.Session;
            }
        }

        /// <summary>
        /// Gets stream owner source.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public RTP_Source Source
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSource;
            }
        }

        /// <summary>
        /// Gets number of times <b>SeqNo</b> has wrapped around.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public int SeqNoWrapCount
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SeqNoWrapCount;
            }
        }

        /// <summary>
        /// Gets next packet sequence number.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public int SeqNo
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SeqNo;
            }
        }

        /// <summary>
        /// Gets last packet send time.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime LastPacketTime
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LastPacketTime;
            }
        }

        /// <summary>
        /// Gets last sent RTP packet RTP timestamp header value.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public uint LastPacketRtpTimestamp
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LastPacketRtpTimestamp;
            }
        }

        /// <summary>
        /// Gets how many RTP packets has sent by this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public long RtpPacketsSent
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_RtpPacketsSent;
            }
        }

        /// <summary>
        /// Gets how many RTP bytes has sent by this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public long RtpBytesSent
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_RtpBytesSent;
            }
        }

        /// <summary>
        /// Gets how many RTP data(no RTP header included) bytes has sent by this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public long RtpDataBytesSent
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_RtpDataBytesSent;
            }
        }

        /// <summary>
        /// Gets how many RTCP cycles has passed since we sent data.
        /// </summary>
        internal int RtcpCyclesSinceWeSent
        {
            get { return m_RtcpCyclesSinceWeSent; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="source">Owner RTP source.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>source</b> is null reference.</exception>
        internal RTP_SendStream(RTP_Source_Local source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            m_pSource = source;

            /* RFC 3550 4.
                The initial value of the sequence number SHOULD be random (unpredictable) to make known-plaintext 
                attacks on encryption more difficult.
            */
            m_SeqNo = new Random().Next(1, Workaround.Definitions.MaxStreamLineLength);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes this sending stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        public void Close()
        {
            Close(null);
        }

        /// <summary>
        /// Closes this sending stream.
        /// </summary>
        /// <param name="closeReason">Stream closing reason text what is reported to the remote party. Value null means not specified.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        public void Close(string closeReason)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            m_pSource.Close(closeReason);

            OnClosed();
            Dispose();
        }

        /// <summary>
        /// Sends specified packet to the RTP session remote party.
        /// </summary>
        /// <param name="packet">RTP packet.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>packet</b> is null reference.</exception>
        /// <remarks>Properties <b>packet.SSRC</b>,<b>packet.SeqNo</b>,<b>packet.PayloadType</b> filled by this method automatically.</remarks>
        public void Send(RTP_Packet packet)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            // RTP was designed around the concept of Application Level Framing (ALF), 
            // because of it we only allow to send packets and don't deal with breaking frames into packets.

            packet.SSRC = Source.SSRC;
            packet.SeqNo = NextSeqNo();
            packet.PayloadType = Session.Payload;

            // Send RTP packet.
            m_RtpBytesSent += m_pSource.SendRtpPacket(packet);

            m_RtpPacketsSent++;
            m_RtpDataBytesSent += packet.Data.Length;
            m_LastPacketTime = DateTime.Now;
            m_LastPacketRtpTimestamp = packet.Timestamp;
            m_RtcpCyclesSinceWeSent = 0;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        private void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            m_IsDisposed = true;

            m_pSource = null;

            OnDisposed();

            Disposed = null;
            Closed = null;
        }

        /// <summary>
        /// Gets next packet sequence number.
        /// </summary>
        /// <returns>Returns next packet sequence number.</returns>
        private ushort NextSeqNo()
        {
            // Wrap around sequence number.
            if (m_SeqNo >= ushort.MaxValue)
            {
                m_SeqNo = 0;
                m_SeqNoWrapCount++;
            }

            return (ushort) m_SeqNo++;
        }

        /// <summary>
        /// Raises <b>Disposed</b> event.
        /// </summary>
        private void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, new EventArgs());
            }
        }

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

        #endregion

        #region Internal methods

        /// <summary>
        /// Is called by RTP session if RTCP cycle compled.
        /// </summary>
        internal void RtcpCycle()
        {
            m_RtcpCyclesSinceWeSent++;
        }

        #endregion
    }
}