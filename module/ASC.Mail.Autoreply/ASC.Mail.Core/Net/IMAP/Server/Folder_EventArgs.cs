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
    /// Provides data for IMAP events.
    /// </summary>
    public class Mailbox_EventArgs
    {
        #region Members

        private readonly string m_Folder = "";
        private readonly string m_NewFolder = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets folder.
        /// </summary>
        public string Folder
        {
            get { return m_Folder; }
        }

        /// <summary>
        /// Gets new folder name, this is available for rename only.
        /// </summary>
        public string NewFolder
        {
            get { return m_NewFolder; }
        }

        /// <summary>
        /// Gets or sets custom error text, which is returned to client.
        /// </summary>
        public string ErrorText { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder"></param>
        public Mailbox_EventArgs(string folder)
        {
            m_Folder = folder;
        }

        /// <summary>
        /// Folder rename constructor.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="newFolder"></param>
        public Mailbox_EventArgs(string folder, string newFolder)
        {
            m_Folder = folder;
            m_NewFolder = newFolder;
        }

        #endregion
    }
}