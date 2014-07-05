/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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