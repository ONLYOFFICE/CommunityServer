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


namespace ASC.Mail.Net.SMTP.Server
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// This class provided data for <b cref="SMTP_Session.MessageStoringCompleted">SMTP_Session.MessageStoringCompleted</b> event.
    /// </summary>
    public class SMTP_e_MessageStored : EventArgs
    {
        #region Members

        private readonly SMTP_Session m_pSession;
        private readonly Stream m_pStream;
        private SMTP_Reply m_pReply;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner SMTP server session.</param>
        /// <param name="stream">Message stream.</param>
        /// <param name="reply">SMTP server reply.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b>, <b>stream</b> or <b>reply</b> is null reference.</exception>
        public SMTP_e_MessageStored(SMTP_Session session, Stream stream, SMTP_Reply reply)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (reply == null)
            {
                throw new ArgumentNullException("reply");
            }

            m_pSession = session;
            m_pStream = stream;
            m_pReply = reply;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets SMTP server reply.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference passed.</exception>
        public SMTP_Reply Reply
        {
            get { return m_pReply; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Reply");
                }

                m_pReply = value;
            }
        }

        /// <summary>
        /// Gets owner SMTP session.
        /// </summary>
        public SMTP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets message stream where message has stored.
        /// </summary>
        public Stream Stream
        {
            get { return m_pStream; }
        }

        #endregion
    }
}