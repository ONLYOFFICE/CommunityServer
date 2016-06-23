/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using Mime;

    #endregion

    /// <summary>
    /// IMAP SEARCH message matcher. You can use this class to see if message values match to search criteria.
    /// </summary>
    public class IMAP_SearchMatcher
    {
        #region Members

        private readonly SearchGroup m_pSearchCriteria;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if header is needed for matching.
        /// </summary>
        public bool IsHeaderNeeded
        {
            get { return m_pSearchCriteria.IsHeaderNeeded(); }
        }

        /// <summary>
        /// Gets if body text is needed for matching.
        /// </summary>
        public bool IsBodyTextNeeded
        {
            get { return m_pSearchCriteria.IsBodyTextNeeded(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Deault constuctor.
        /// </summary>
        /// <param name="mainSearchGroup">SEARCH command main search group.</param>
        internal IMAP_SearchMatcher(SearchGroup mainSearchGroup)
        {
            m_pSearchCriteria = mainSearchGroup;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets if specified values match search criteria.
        /// </summary>
        /// <param name="no">Message sequence number.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        /// <param name="internalDate">Message INTERNALDATE (dateTime when server stored message).</param>
        /// <param name="flags">Message flags.</param>
        /// <param name="header">Message header. This is only needed if this.IsHeaderNeeded is true.</param>
        /// <param name="bodyText">Message body text (must be decoded unicode text). This is only needed if this.IsBodyTextNeeded is true.</param>
        /// <returns></returns>
        public bool Matches(int no,
                            int uid,
                            int size,
                            DateTime internalDate,
                            IMAP_MessageFlags flags,
                            string header,
                            string bodyText)
        {
            // Parse header only if it's needed
            Mime m = null;
            if (m_pSearchCriteria.IsHeaderNeeded())
            {
                m = new Mime();
                m.MainEntity.Header.Parse(header);
            }

            return m_pSearchCriteria.Match(no, uid, size, internalDate, flags, m, bodyText);
        }

        #endregion
    }
}