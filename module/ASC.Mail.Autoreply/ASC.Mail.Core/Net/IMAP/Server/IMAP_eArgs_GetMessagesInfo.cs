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
    /// Provides data to event GetMessagesInfo.
    /// </summary>
    public class IMAP_eArgs_GetMessagesInfo
    {
        #region Members

        private readonly IMAP_SelectedFolder m_pFolderInfo;
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
        /// Gets folder info.
        /// </summary>
        public IMAP_SelectedFolder FolderInfo
        {
            get { return m_pFolderInfo; }
        }

        /// <summary>
        /// Gets or sets custom error text, which is returned to client. Null value means no error.
        /// </summary>
        public string ErrorText { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_eArgs_GetMessagesInfo(IMAP_Session session, IMAP_SelectedFolder folder)
        {
            m_pSession = session;
            m_pFolderInfo = folder;
        }

        #endregion
    }
}