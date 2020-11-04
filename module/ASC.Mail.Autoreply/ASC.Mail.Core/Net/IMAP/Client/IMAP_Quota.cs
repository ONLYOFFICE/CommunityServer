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


namespace ASC.Mail.Net.IMAP.Client
{
    /// <summary>
    /// IMAP quota entry. Defined in RFC 2087.
    /// </summary>
    public class IMAP_Quota
    {
        #region Members

        private readonly long m_MaxMessages = -1;
        private readonly long m_MaxStorage = -1;
        private readonly long m_Messages = -1;
        private readonly string m_QuotaRootName = "";
        private readonly long m_Storage = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets quota root name.
        /// </summary>
        public string QuotaRootName
        {
            get { return m_QuotaRootName; }
        }

        /// <summary>
        /// Gets current messages count. Returns -1 if messages and maximum messages quota is not defined.
        /// </summary>
        public long Messages
        {
            get { return m_Messages; }
        }

        /// <summary>
        /// Gets maximum allowed messages count. Returns -1 if messages and maximum messages quota is not defined.
        /// </summary>
        public long MaximumMessages
        {
            get { return m_MaxMessages; }
        }

        /// <summary>
        /// Gets current storage in bytes. Returns -1 if storage and maximum storage quota is not defined.
        /// </summary>
        public long Storage
        {
            get { return m_Storage; }
        }

        /// <summary>
        /// Gets maximum allowed storage in bytes. Returns -1 if storage and maximum storage quota is not defined.
        /// </summary>
        public long MaximumStorage
        {
            get { return m_MaxStorage; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="quotaRootName">Quota root name.</param>
        /// <param name="messages">Number of current messages.</param>
        /// <param name="maxMessages">Number of maximum allowed messages.</param>
        /// <param name="storage">Current storage bytes.</param>
        /// <param name="maxStorage">Maximum allowed storage bytes.</param>
        public IMAP_Quota(string quotaRootName, long messages, long maxMessages, long storage, long maxStorage)
        {
            m_QuotaRootName = quotaRootName;
            m_Messages = messages;
            m_MaxMessages = maxMessages;
            m_Storage = storage;
            m_MaxStorage = maxStorage;
        }

        #endregion
    }
}