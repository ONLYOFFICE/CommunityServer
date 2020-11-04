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


namespace ASC.Mail.Net.POP3.Server
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// Provides data to POP3 server event GetMessageStream.
    /// </summary>
    public class POP3_eArgs_GetMessageStream
    {
        #region Members

        private readonly POP3_Message m_pMessageInfo;
        private readonly POP3_Session m_pSession;
        private bool m_CloseMessageStream = true;
        private bool m_MessageExists = true;
        private long m_MessageStartOffset;
        private Stream m_MessageStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to current POP3 session.
        /// </summary>
        public POP3_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets message info what message stream to get.
        /// </summary>
        public POP3_Message MessageInfo
        {
            get { return m_pMessageInfo; }
        }

        /// <summary>
        /// Gets or sets if message stream is closed automatically if all actions on it are completed.
        /// Default value is true.
        /// </summary>
        public bool CloseMessageStream
        {
            get { return m_CloseMessageStream; }

            set { m_CloseMessageStream = value; }
        }

        /// <summary>
        /// Gets or sets message stream. When setting this property Stream position must be where message begins.
        /// </summary>
        public Stream MessageStream
        {
            get
            {
                if (m_MessageStream != null)
                {
                    m_MessageStream.Position = m_MessageStartOffset;
                }
                return m_MessageStream;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Property MessageStream value can't be null !");
                }
                if (!value.CanSeek)
                {
                    throw new Exception("Stream must support seeking !");
                }

                m_MessageStream = value;
                m_MessageStartOffset = m_MessageStream.Position;
            }
        }

        /// <summary>
        /// Gets message size in bytes.
        /// </summary>
        public long MessageSize
        {
            get
            {
                if (m_MessageStream == null)
                {
                    throw new Exception("You must set MessageStream property first to use this property !");
                }
                else
                {
                    return m_MessageStream.Length - m_MessageStream.Position;
                }
            }
        }

        /// <summary>
        /// Gets or sets if message exists. Set this false, if message actually doesn't exist any more.
        /// </summary>
        public bool MessageExists
        {
            get { return m_MessageExists; }

            set { m_MessageExists = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to current POP3 session.</param>
        /// <param name="messageInfo">Message info what message items to get.</param>
        public POP3_eArgs_GetMessageStream(POP3_Session session, POP3_Message messageInfo)
        {
            m_pSession = session;
            m_pMessageInfo = messageInfo;
        }

        #endregion
    }
}