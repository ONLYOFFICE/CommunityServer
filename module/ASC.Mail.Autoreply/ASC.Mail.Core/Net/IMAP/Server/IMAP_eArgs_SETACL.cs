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
    /// Provides data for SetFolderACL event.
    /// </summary>
    public class IMAP_SETACL_eArgs
    {
        #region Members

        private readonly IMAP_ACL_Flags m_ACL_Flags = IMAP_ACL_Flags.None;
        private readonly IMAP_Flags_SetType m_FlagsSetType = IMAP_Flags_SetType.Replace;
        private readonly string m_pFolderName = "";
        private readonly IMAP_Session m_pSession;
        private readonly string m_UserName = "";
        private string m_ErrorText = "";

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
        /// Gets folder name which ACL to set.
        /// </summary>
        public string Folder
        {
            get { return m_pFolderName; }
        }

        /// <summary>
        /// Gets user name which ACL to set.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Gets how ACL flags must be stored.
        /// </summary>
        public IMAP_Flags_SetType FlagsSetType
        {
            get { return m_FlagsSetType; }
        }

        /// <summary>
        /// Gets ACL flags. NOTE: See this.FlagsSetType how to store flags.
        /// </summary>
        public IMAP_ACL_Flags ACL
        {
            get { return m_ACL_Flags; }
        }

        /// <summary>
        /// Gets or sets error text returned to connected client.
        /// </summary>
        public string ErrorText
        {
            get { return m_ErrorText; }

            set { m_ErrorText = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner IMAP session.</param>
        /// <param name="folderName">Folder name which ACL to set.</param>
        /// <param name="userName">User name which ACL to set.</param>
        /// <param name="flagsSetType">Specifies how flags must be stored.</param>
        /// <param name="aclFlags">Flags which to store.</param>
        public IMAP_SETACL_eArgs(IMAP_Session session,
                                 string folderName,
                                 string userName,
                                 IMAP_Flags_SetType flagsSetType,
                                 IMAP_ACL_Flags aclFlags)
        {
            m_pSession = session;
            m_pFolderName = folderName;
            m_UserName = userName;
            m_FlagsSetType = flagsSetType;
            m_ACL_Flags = aclFlags;
        }

        #endregion
    }
}