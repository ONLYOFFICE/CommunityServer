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
    using System.Collections;
    using Mime;

    #endregion

    /// <summary>
    /// IMAP search command grouped(parenthesized) search-key collection.
    /// </summary>
    internal class SearchGroup
    {
        #region Members

        private readonly ArrayList m_pSearchKeys;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SearchGroup()
        {
            m_pSearchKeys = new ArrayList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses search key from current position.
        /// </summary>
        /// <param name="reader"></param>
        public void Parse(StringReader reader)
        {
            //Remove spaces from string start
            reader.ReadToFirstChar();

            if (reader.StartsWith("("))
            {
                reader = new StringReader(reader.ReadParenthesized().Trim());
            }

            //--- Start parsing search keys --------------//
            while (reader.Available > 0)
            {
                object searchKey = ParseSearchKey(reader);
                if (searchKey != null)
                {
                    m_pSearchKeys.Add(searchKey);
                }
            }
            //--------------------------------------------//			
        }

        /// <summary>
        /// Gets if message Header is needed for matching.
        /// </summary>
        /// <returns></returns>
        public bool IsHeaderNeeded()
        {
            foreach (object searchKey in m_pSearchKeys)
            {
                if (IsHeaderNeededForKey(searchKey))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if message body text is needed for matching.
        /// </summary>
        /// <returns></returns>
        public bool IsBodyTextNeeded()
        {
            foreach (object searchKey in m_pSearchKeys)
            {
                if (IsBodyTextNeededForKey(searchKey))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if specified message matches with this class search-key.
        /// </summary>
        /// <param name="no">IMAP message sequence number.</param>
        /// <param name="uid">IMAP message UID.</param>
        /// <param name="size">IMAP message size in bytes.</param>
        /// <param name="internalDate">IMAP message INTERNALDATE (dateTime when server stored message).</param>
        /// <param name="flags">IMAP message flags.</param>
        /// <param name="mime">Mime message main header only.</param>
        /// <param name="bodyText">Message body text.</param>
        /// <returns></returns>
        public bool Match(long no,
                          long uid,
                          long size,
                          DateTime internalDate,
                          IMAP_MessageFlags flags,
                          Mime mime,
                          string bodyText)
        {
            // We must match all keys, if one fails, no need to check others

            foreach (object searckKey in m_pSearchKeys)
            {
                if (!Match_Key_Value(searckKey, no, uid, size, internalDate, flags, mime, bodyText))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses SearchGroup or SearchItem from reader. If reader starts with (, then parses searchGroup, otherwise SearchItem.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static object ParseSearchKey(StringReader reader)
        {
            //Remove spaces from string start
            reader.ReadToFirstChar();

            // SearchGroup
            if (reader.StartsWith("("))
            {
                SearchGroup searchGroup = new SearchGroup();
                searchGroup.Parse(reader);

                return searchGroup;
            }
                // SearchItem
            else
            {
                return SearchKey.Parse(reader);
            }
        }

        /// <summary>
        /// Gets if specified message matches to specified search key.
        /// </summary>
        /// <param name="searchKey">SearchKey or SearchGroup.</param>
        /// <param name="no">IMAP message sequence number.</param>
        /// <param name="uid">IMAP message UID.</param>
        /// <param name="size">IMAP message size in bytes.</param>
        /// <param name="internalDate">IMAP message INTERNALDATE (dateTime when server stored message).</param>
        /// <param name="flags">IMAP message flags.</param>
        /// <param name="mime">Mime message main header only.</param>
        /// <param name="bodyText">Message body text.</param>
        /// <returns></returns>
        internal static bool Match_Key_Value(object searchKey,
                                             long no,
                                             long uid,
                                             long size,
                                             DateTime internalDate,
                                             IMAP_MessageFlags flags,
                                             Mime mime,
                                             string bodyText)
        {
            if (searchKey.GetType() == typeof (SearchKey))
            {
                if (!((SearchKey) searchKey).Match(no, uid, size, internalDate, flags, mime, bodyText))
                {
                    return false;
                }
            }
            else if (searchKey.GetType() == typeof (SearchGroup))
            {
                if (!((SearchGroup) searchKey).Match(no, uid, size, internalDate, flags, mime, bodyText))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets if message header is needed for matching.
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        internal static bool IsHeaderNeededForKey(object searchKey)
        {
            if (searchKey.GetType() == typeof (SearchKey))
            {
                if (((SearchKey) searchKey).IsHeaderNeeded())
                {
                    return true;
                }
            }
            else if (searchKey.GetType() == typeof (SearchGroup))
            {
                if (((SearchGroup) searchKey).IsHeaderNeeded())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if message body text is needed for matching.
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        internal static bool IsBodyTextNeededForKey(object searchKey)
        {
            if (searchKey.GetType() == typeof (SearchKey))
            {
                if (((SearchKey) searchKey).IsBodyTextNeeded())
                {
                    return true;
                }
            }
            else if (searchKey.GetType() == typeof (SearchGroup))
            {
                if (((SearchGroup) searchKey).IsBodyTextNeeded())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}