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
    /// IMAP folder.
    /// </summary>
    public class IMAP_Folder
    {
        #region Members

        private readonly string m_Folder = "";
        private bool m_Selectable = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets IMAP folder name. Eg. Inbox, Inbox/myFolder, ... .
        /// </summary>
        public string Folder
        {
            get { return m_Folder; }
        }

        /// <summary>
        /// Gets or sets if folder is selectable (SELECT command can select this folder).
        /// </summary>
        public bool Selectable
        {
            get { return m_Selectable; }

            set { m_Selectable = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder">Full path to folder, path separator = '/'. Eg. Inbox/myFolder .</param>
        /// <param name="selectable">Gets or sets if folder is selectable(SELECT command can select this folder).</param>
        public IMAP_Folder(string folder, bool selectable)
        {
            m_Folder = folder;
            m_Selectable = selectable;
        }

        #endregion
    }
}