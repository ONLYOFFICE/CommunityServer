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


namespace ASC.Mail.Net.IMAP.Server
{
    /// <summary>
    /// Provides data for GetUserQuota event.
    /// </summary>
    public class IMAP_eArgs_GetQuota
    {
        #region Members

        private readonly IMAP_Session m_pSession;

        #endregion

        #region Properties

        /// <summary>
        /// Gets current IMAP session.
        /// </summary>
        public IMAP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets user name.
        /// </summary>
        public string UserName
        {
            get { return m_pSession.UserName; }
        }

        /// <summary>
        /// Gets or sets maximum mailbox size.
        /// </summary>
        public long MaxMailboxSize { get; set; }

        /// <summary>
        /// Gets or sets current mailbox size.
        /// </summary>
        public long MailboxSize { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner IMAP session.</param>
        public IMAP_eArgs_GetQuota(IMAP_Session session)
        {
            m_pSession = session;
        }

        #endregion
    }
}