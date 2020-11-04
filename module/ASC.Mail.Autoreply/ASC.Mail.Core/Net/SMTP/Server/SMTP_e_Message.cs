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
    /// This class provided data for <b cref="SMTP_Session.GetMessageStream">SMTP_Session.GetMessageStream</b> event.
    /// </summary>
    public class SMTP_e_Message : EventArgs
    {
        #region Members

        private readonly SMTP_Session m_pSession;
        private Stream m_pStream;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner SMTP server session.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null reference.</exception>
        public SMTP_e_Message(SMTP_Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            m_pSession = session;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets owner SMTP session.
        /// </summary>
        public SMTP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets or stes stream where to store incoming message.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference is passed.</exception>
        public Stream Stream
        {
            get { return m_pStream; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Stream");
                }

                m_pStream = value;
            }
        }

        #endregion
    }
}