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
    /// Holds POP3_Message info (ID,Size,...).
    /// </summary>
    public class POP3_Message
    {
        #region Members

        private string m_ID = ""; // Holds message ID.
        private POP3_MessageCollection m_pOwner;
        private string m_UID = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets message ID.
        /// </summary>
        public string ID
        {
            get { return m_ID; }

            set { m_ID = value; }
        }

        /// <summary>
        /// Gets or sets message UID. This UID is reported in UIDL command.
        /// </summary>
        public string UID
        {
            get { return m_UID; }

            set { m_UID = value; }
        }

        /// <summary>
        /// Gets or sets message size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets message state flag.
        /// </summary>
        public bool MarkedForDelete { get; set; }

        /// <summary>
        /// Gets or sets user data for message.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="onwer">Owner collection.</param>
        /// <param name="id">Message ID.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        internal POP3_Message(POP3_MessageCollection onwer, string id, string uid, long size)
        {
            m_pOwner = onwer;
            m_ID = id;
            m_UID = uid;
            Size = size;
        }

        #endregion
    }
}