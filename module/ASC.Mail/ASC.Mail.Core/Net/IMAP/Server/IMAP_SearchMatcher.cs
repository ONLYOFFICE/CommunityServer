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