/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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