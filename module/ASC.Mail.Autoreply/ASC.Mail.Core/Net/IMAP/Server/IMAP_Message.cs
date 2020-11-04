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

    using System;

    #endregion

    /// <summary>
    /// IMAP message info.
    /// </summary>
    public class IMAP_Message:IDisposable
    {
        #region Members

        private readonly string m_ID = "";
        private readonly DateTime m_InternalDate = DateTime.Now;
        private readonly IMAP_MessageCollection m_pOwner;
        private readonly long m_Size;
        private readonly long m_UID;
        private IMAP_MessageFlags m_Flags = IMAP_MessageFlags.None;

        #endregion

        #region Properties

        /// <summary>
        /// Gets message 1 based sequence number in the collection. This property is slow, use with care, never use in big for loops !
        /// </summary>
        public int SequenceNo
        {
            get { return m_pOwner.IndexOf(this) + 1; }
        }

        /// <summary>
        /// Gets message ID.
        /// </summary>
        public string ID
        {
            get { return m_ID; }
        }

        /// <summary>
        /// Gets message IMAP UID value.
        /// </summary>
        public long UID
        {
            get { return m_UID; }
        }

        /// <summary>
        /// Gets message store date.
        /// </summary>
        public DateTime InternalDate
        {
            get { return m_InternalDate; }
        }

        /// <summary>
        /// Gets message size in bytes.
        /// </summary>
        public long Size
        {
            get { return m_Size; }
        }

        /// <summary>
        /// Gets message flags.
        /// </summary>
        public IMAP_MessageFlags Flags
        {
            get { return m_Flags; }
        }

        /// <summary>
        /// Gets message flags string. For example: "\DELETES \SEEN".
        /// </summary>
        public string FlagsString
        {
            get { return IMAP_Utils.MessageFlagsToString(m_Flags); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="onwer">Owner collection.</param>
        /// <param name="id">Message ID.</param>
        /// <param name="uid">Message IMAP UID value.</param>
        /// <param name="internalDate">Message store date.</param>
        /// <param name="size">Message size in bytes.</param>
        /// <param name="flags">Message flags.</param>
        internal IMAP_Message(IMAP_MessageCollection onwer,
                              string id,
                              long uid,
                              DateTime internalDate,
                              long size,
                              IMAP_MessageFlags flags)
        {
            m_pOwner = onwer;
            m_ID = id;
            m_UID = uid;
            m_InternalDate = internalDate;
            m_Size = size;
            m_Flags = flags;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Sets message flags.
        /// </summary>
        /// <param name="flags">Message flags.</param>
        internal void SetFlags(IMAP_MessageFlags flags)
        {
            m_Flags = flags;
        }

        #endregion

        public void Dispose()
        {
            
        }
    }
}