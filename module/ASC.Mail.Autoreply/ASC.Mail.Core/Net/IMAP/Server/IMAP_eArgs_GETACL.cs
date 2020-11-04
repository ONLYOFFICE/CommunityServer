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
    #region usings

    using System.Collections;

    #endregion

    /// <summary>
    /// Provides data for GetFolderACL event.
    /// </summary>
    public class IMAP_GETACL_eArgs
    {
        #region Members

        private readonly Hashtable m_ACLs;
        private readonly string m_pFolderName = "";
        private readonly IMAP_Session m_pSession;
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
        /// Gets folder name which ACL to get.
        /// </summary>
        public string Folder
        {
            get { return m_pFolderName; }
        }

        /// <summary>
        /// Gets ACL collection. Key = userName, Value = IMAP_ACL_Flags.
        /// </summary>
        public Hashtable ACL
        {
            get { return m_ACLs; }
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
        /// <param name="folderName">Folder name which ACL to get.</param>
        public IMAP_GETACL_eArgs(IMAP_Session session, string folderName)
        {
            m_pSession = session;
            m_pFolderName = folderName;

            m_ACLs = new Hashtable();
        }

        #endregion
    }
}