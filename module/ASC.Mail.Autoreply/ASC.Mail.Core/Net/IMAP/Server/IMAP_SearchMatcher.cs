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