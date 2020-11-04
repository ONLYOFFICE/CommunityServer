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

    #endregion

    /// <summary>
    /// This class provided data for <b cref="SMTP_Session.Started">SMTP_Session.Started</b> event.
    /// </summary>
    public class SMTP_e_Started : EventArgs
    {
        #region Members

        private readonly SMTP_Session m_pSession;
        private SMTP_Reply m_pReply;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner SMTP server session.</param>
        /// <param name="reply">SMTP server reply.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> or <b>reply</b> is null reference.</exception>
        public SMTP_e_Started(SMTP_Session session, SMTP_Reply reply)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            if (reply == null)
            {
                throw new ArgumentNullException("reply");
            }

            m_pSession = session;
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

        #endregion
    }
}