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
    /// Summary description for SharedRootFolders_EventArgs.
    /// </summary>
    public class SharedRootFolders_EventArgs
    {
        #region Members

        private readonly IMAP_Session m_pSession;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to smtp session.
        /// </summary>
        public IMAP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets or sets users shared root folders. Ususaly there is only one root folder 'Shared Folders'.
        /// </summary>
        public string[] SharedRootFolders { get; set; }

        /// <summary>
        /// Gets or sets public root folders. Ususaly there is only one root folder 'Public Folders'.
        /// </summary>
        public string[] PublicRootFolders { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SharedRootFolders_EventArgs(IMAP_Session session)
        {
            m_pSession = session;
        }

        #endregion
    }
}