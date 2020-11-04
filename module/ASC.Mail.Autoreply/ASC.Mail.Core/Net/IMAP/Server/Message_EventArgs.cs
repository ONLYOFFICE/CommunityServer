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
    /// Provides data for message related events.
    /// </summary>
    public class Message_EventArgs
    {
        #region Members

        private readonly string m_CopyLocation = "";
        private readonly string m_Folder = "";
        private readonly bool m_HeadersOnly;
        private readonly IMAP_Message m_pMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets IMAP folder.
        /// </summary>
        public string Folder
        {
            get { return m_Folder; }
        }

        /// <summary>
        /// Gets IMAP message info.
        /// </summary>
        public IMAP_Message Message
        {
            get { return m_pMessage; }
        }

        /// <summary>
        /// Gets message new location. NOTE: this is available for copy command only.
        /// </summary>
        public string CopyLocation
        {
            get { return m_CopyLocation; }
        }

        /// <summary>
        /// Gets or sets message data. NOTE: this is available for GetMessage and StoreMessage event only.
        /// </summary>
        public byte[] MessageData { get; set; }

        /// <summary>
        /// Gets if message headers or full message wanted. NOTE: this is available for GetMessage event only.
        /// </summary>
        public bool HeadersOnly
        {
            get { return m_HeadersOnly; }
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
        /// <param name="folder">IMAP folder which message is.</param>
        /// <param name="msg"></param>
        public Message_EventArgs(string folder, IMAP_Message msg)
        {
            m_Folder = folder;
            m_pMessage = msg;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="folder">IMAP folder which message is.</param>
        /// <param name="msg"></param>
        /// <param name="copyLocation"></param>
        public Message_EventArgs(string folder, IMAP_Message msg, string copyLocation)
        {
            m_Folder = folder;
            m_pMessage = msg;
            m_CopyLocation = copyLocation;
        }

        /// <summary>
        /// GetMessage constructor.
        /// </summary>
        /// <param name="folder">IMAP folder which message is.</param>
        /// <param name="msg"></param>
        /// <param name="headersOnly">Specifies if messages headers or full message is needed.</param>
        public Message_EventArgs(string folder, IMAP_Message msg, bool headersOnly)
        {
            m_Folder = folder;
            m_pMessage = msg;
            m_HeadersOnly = headersOnly;
        }

        #endregion
    }
}