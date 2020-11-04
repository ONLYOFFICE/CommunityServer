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