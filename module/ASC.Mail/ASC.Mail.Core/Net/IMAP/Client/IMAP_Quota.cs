/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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