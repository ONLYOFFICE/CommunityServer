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
    /// <summary>
    /// Provides data for the GetMessgesList event.
    /// </summary>
    public class GetMessagesInfo_EventArgs
    {
        #region Members

        private readonly POP3_MessageCollection m_pPOP3_Messages;
        private readonly POP3_Session m_pSession;
        private readonly string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to pop3 session.
        /// </summary>
        public POP3_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets referance to POP3 messages info.
        /// </summary>
        public POP3_MessageCollection Messages
        {
            get { return m_pPOP3_Messages; }
        }

        /// <summary>
        /// User Name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Mailbox name.
        /// </summary>
        public string Mailbox
        {
            get { return m_UserName; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to pop3 session.</param>
        /// <param name="messages"></param>
        /// <param name="mailbox">Mailbox name.</param>
        public GetMessagesInfo_EventArgs(POP3_Session session, POP3_MessageCollection messages, string mailbox)
        {
            m_pSession = session;
            m_pPOP3_Messages = messages;
            m_UserName = mailbox;
        }

        #endregion
    }
}