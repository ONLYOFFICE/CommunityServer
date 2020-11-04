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
    /// IMAP search key (RFC 3501 6.4.4 SEARCH Command).
    /// </summary>
    internal class SearchKey
    {
        #region Members

        private readonly string m_SearchKeyName = "";
        private readonly object m_SearchKeyValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SearchKey(string searchKeyName, object value)
        {
            m_SearchKeyName = searchKeyName;
            m_SearchKeyValue = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses one search key from current position. Returns null if there isn't any search key left.
        /// </summary>
        /// <param name="reader"></param>
        public static SearchKey Parse(StringReader reader)
        {
            string searchKeyName = "";
            object searchKeyValue = null;

            //Remove spaces from string start
            reader.ReadToFirstChar();

            // Search keyname is always 1 word
            string word = reader.ReadWord();
            if (word == null)
            {
                return null;
            }
            word = word.ToUpper().Trim();

            //Remove spaces from string start
            reader.ReadToFirstChar();

            #region ALL

            // ALL
            //		All messages in the mailbox; the default initial key for ANDing.
            if (word == "ALL")
            {
                searchKeyName = "ALL";
            }

                #endregion

                #region ANSWERED

                // ANSWERED
                //		Messages with the \Answered flag set.
            else if (word == "ANSWERED")
            {
                // We internally use KEYWORD ANSWERED
                searchKeyName = "KEYWORD";
                searchKeyValue = "ANSWERED";
            }

                #endregion

                #region BCC

                // BCC <string>
                //		Messages that contain the specified string in the envelope structure's BCC field.
            else if (word == "BCC")
            {
                // We internally use HEADER "BCC:" "value"
                searchKeyName = "HEADER";

                // Read <string>
                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = new[] {"BCC:", TextUtils.UnQuoteString(val)};
                }
                else
                {
                    throw new Exception("BCC <string> value is missing !");
                }
            }

                #endregion

                #region BEFORE

                //	BEFORE <date>
                //		Messages whose internal date (disregarding time and timezone) is earlier than the specified date.
            else if (word == "BEFORE")
            {
                searchKeyName = "BEFORE";

                // Read <date>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    // Parse date
                    try
                    {
                        searchKeyValue = IMAP_Utils.ParseDate(TextUtils.UnQuoteString(val));
                    }
                        // Invalid date
                    catch
                    {
                        throw new Exception("Invalid BEFORE <date> value '" + val +
                                            "', valid date syntax: {dd-MMM-yyyy} !");
                    }
                }
                else
                {
                    throw new Exception("BEFORE <date> value is missing !");
                }
            }

                #endregion

                #region BODY

                //	BODY <string>
                //		Messages that contain the specified string in the body of the message.
            else if (word == "BODY")
            {
                searchKeyName = "BODY";

                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = val;
                }
                else
                {
                    throw new Exception("BODY <string> value is missing !");
                }
            }

                #endregion

                #region CC

                //	CC <string>
                //		Messages that contain the specified string in the envelope structure's CC field.
            else if (word == "CC")
            {
                // We internally use HEADER "CC:" "value"
                searchKeyName = "HEADER";

                // Read <string>
                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = new[] {"CC:", TextUtils.UnQuoteString(val)};
                }
                else
                {
                    throw new Exception("CC <string> value is missing !");
                }
            }

                #endregion

                #region DELETED

                // DELETED
                //		Messages with the \Deleted flag set.
            else if (word == "DELETED")
            {
                // We internally use KEYWORD DELETED
                searchKeyName = "KEYWORD";
                searchKeyValue = "DELETED";
            }

                #endregion

                #region DRAFT

                //	DRAFT
                //		Messages with the \Draft flag set.
            else if (word == "DRAFT")
            {
                // We internally use KEYWORD DRAFT
                searchKeyName = "KEYWORD";
                searchKeyValue = "DRAFT";
            }

                #endregion

                #region FLAGGED

                //	FLAGGED
                //		Messages with the \Flagged flag set.
            else if (word == "FLAGGED")
            {
                // We internally use KEYWORD FLAGGED
                searchKeyName = "KEYWORD";
                searchKeyValue = "FLAGGED";
            }

                #endregion

                #region FROM

                //	FROM <string>
                //		Messages that contain the specified string in the envelope structure's FROM field.
            else if (word == "FROM")
            {
                // We internally use HEADER "FROM:" "value"
                searchKeyName = "HEADER";

                // Read <string>
                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = new[] {"FROM:", TextUtils.UnQuoteString(val)};
                }
                else
                {
                    throw new Exception("FROM <string> value is missing !");
                }
            }

                #endregion

                #region HEADER

                //	HEADER <field-name> <string>
                //		Messages that have a header with the specified field-name (as
                //		defined in [RFC-2822]) and that contains the specified string
                //		in the text of the header (what comes after the colon).  If the
                //		string to search is zero-length, this matches all messages that
                //		have a header line with the specified field-name regardless of
                //		the contents.
            else if (word == "HEADER")
            {
                searchKeyName = "HEADER";

                // Read <field-name>
                string fieldName = ReadString(reader);
                if (fieldName != null)
                {
                    fieldName = TextUtils.UnQuoteString(fieldName);
                }
                else
                {
                    throw new Exception("HEADER <field-name> value is missing !");
                }

                // Read <string>
                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = new[] {fieldName, TextUtils.UnQuoteString(val)};
                }
                else
                {
                    throw new Exception("(HEADER <field-name>) <string> value is missing !");
                }
            }

                #endregion

                #region KEYWORD

                //	KEYWORD <flag>
                //		Messages with the specified keyword flag set.
            else if (word == "KEYWORD")
            {
                searchKeyName = "KEYWORD";

                // Read <flag>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    searchKeyValue = TextUtils.UnQuoteString(val);
                }
                else
                {
                    throw new Exception("KEYWORD <flag> value is missing !");
                }
            }

                #endregion

                #region LARGER

                //	LARGER <n>
                //		Messages with an [RFC-2822] size larger than the specified number of octets.
            else if (word == "LARGER")
            {
                searchKeyName = "LARGER";

                // Read <n>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    // Parse <n> - must be integer value
                    try
                    {
                        searchKeyValue = Convert.ToInt64(TextUtils.UnQuoteString(val));
                    }
                        // Invalid <n>
                    catch
                    {
                        throw new Exception("Invalid LARGER <n> value '" + val +
                                            "', it must be numeric value !");
                    }
                }
                else
                {
                    throw new Exception("LARGER <n> value is missing !");
                }
            }

                #endregion

                #region NEW

                //	NEW
                //		Messages that have the \Recent flag set but not the \Seen flag.
                //		This is functionally equivalent to "(RECENT UNSEEN)".
            else if (word == "NEW")
            {
                // We internally use KEYWORD RECENT
                searchKeyName = "KEYWORD";
                searchKeyValue = "RECENT";
            }

                #endregion

                #region NOT

                //	NOT <search-key> or (<search-key> <search-key> ...)(SearchGroup)
                //		Messages that do not match the specified search key.
            else if (word == "NOT")
            {
                searchKeyName = "NOT";

                object searchItem = SearchGroup.ParseSearchKey(reader);
                if (searchItem != null)
                {
                    searchKeyValue = searchItem;
                }
                else
                {
                    throw new Exception("Required NOT <search-key> isn't specified !");
                }
            }

                #endregion

                #region OLD

                //	OLD
                //		Messages that do not have the \Recent flag set.  This is
                //		functionally equivalent to "NOT RECENT" (as opposed to "NOT	NEW").
            else if (word == "OLD")
            {
                // We internally use UNKEYWORD RECENT
                searchKeyName = "UNKEYWORD";
                searchKeyValue = "RECENT";
            }

                #endregion

                #region ON

                //	ON <date>
                //		Messages whose internal date (disregarding time and timezone) is within the specified date.
            else if (word == "ON")
            {
                searchKeyName = "ON";

                // Read <date>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    // Parse date
                    try
                    {
                        searchKeyValue = IMAP_Utils.ParseDate(TextUtils.UnQuoteString(val));
                    }
                        // Invalid date
                    catch
                    {
                        throw new Exception("Invalid ON <date> value '" + val +
                                            "', valid date syntax: {dd-MMM-yyyy} !");
                    }
                }
                else
                {
                    throw new Exception("ON <date> value is missing !");
                }
            }

                #endregion

                #region OR

                //	OR <search-key1> <search-key2> - SearckKey can be parenthesis list of keys !
                //		Messages that match either search key.
            else if (word == "OR")
            {
                searchKeyName = "OR";

                //--- <search-key1> ----------------------------------------------------//
                object searchKey1 = SearchGroup.ParseSearchKey(reader);
                if (searchKey1 == null)
                {
                    throw new Exception("Required OR <search-key1> isn't specified !");
                }
                //----------------------------------------------------------------------//

                //--- <search-key2> ----------------------------------------------------//
                object searchKey2 = SearchGroup.ParseSearchKey(reader);
                if (searchKey2 == null)
                {
                    throw new Exception("Required (OR <search-key1>) <search-key2> isn't specified !");
                }
                //-----------------------------------------------------------------------//

                searchKeyValue = new[] {searchKey1, searchKey2};
            }

                #endregion

                #region RECENT

                //	RECENT
                //		Messages that have the \Recent flag set.
            else if (word == "RECENT")
            {
                // We internally use KEYWORD RECENT
                searchKeyName = "KEYWORD";
                searchKeyValue = "RECENT";
            }

                #endregion

                #region SEEN

                //	SEEN
                //		Messages that have the \Seen flag set.
            else if (word == "SEEN")
            {
                // We internally use KEYWORD SEEN
                searchKeyName = "KEYWORD";
                searchKeyValue = "SEEN";
            }

                #endregion

                #region SENTBEFORE

                //	SENTBEFORE <date>
                //		Messages whose [RFC-2822] Date: header (disregarding time and
                //		timezone) is earlier than the specified date.
            else if (word == "SENTBEFORE")
            {
                searchKeyName = "SENTBEFORE";

                // Read <date>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    // Parse date
                    try
                    {
                        searchKeyValue = IMAP_Utils.ParseDate(TextUtils.UnQuoteString(val));
                    }
                        // Invalid date
                    catch
                    {
                        throw new Exception("Invalid SENTBEFORE <date> value '" + val +
                                            "', valid date syntax: {dd-MMM-yyyy} !");
                    }
                }
                else
                {
                    throw new Exception("SENTBEFORE <date> value is missing !");
                }
            }

                #endregion

                #region SENTON

                //	SENTON <date>
                //		Messages whose [RFC-2822] Date: header (disregarding time and
                //		timezone) is within the specified date.
            else if (word == "SENTON")
            {
                searchKeyName = "SENTON";

                // Read <date>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    // Parse date
                    try
                    {
                        searchKeyValue = IMAP_Utils.ParseDate(TextUtils.UnQuoteString(val));
                    }
                        // Invalid date
                    catch
                    {
                        throw new Exception("Invalid SENTON <date> value '" + val +
                                            "', valid date syntax: {dd-MMM-yyyy} !");
                    }
                }
                else
                {
                    throw new Exception("SENTON <date> value is missing !");
                }
            }

                #endregion

                #region SENTSINCE

                //	SENTSINCE <date>
                //		Messages whose [RFC-2822] Date: header (disregarding time and
                //		timezone) is within or later than the specified date.
            else if (word == "SENTSINCE")
            {
                searchKeyName = "SENTSINCE";

                // Read <date>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    // Parse date
                    try
                    {
                        searchKeyValue = IMAP_Utils.ParseDate(TextUtils.UnQuoteString(val));
                    }
                        // Invalid date
                    catch
                    {
                        throw new Exception("Invalid SENTSINCE <date> value '" + val +
                                            "', valid date syntax: {dd-MMM-yyyy} !");
                    }
                }
                else
                {
                    throw new Exception("SENTSINCE <date> value is missing !");
                }
            }

                #endregion

                #region SINCE

                //	SINCE <date>
                //		Messages whose internal date (disregarding time and timezone)
                //		is within or later than the specified date.
            else if (word == "SINCE")
            {
                searchKeyName = "SINCE";

                // Read <date>
                string val = reader.ReadWord();
                if (val != null)
                {
                    // Parse date
                    try
                    {
                        searchKeyValue = IMAP_Utils.ParseDate(TextUtils.UnQuoteString(val));
                    }
                        // Invalid date
                    catch
                    {
                        throw new Exception("Invalid SINCE <date> value '" + val +
                                            "', valid date syntax: {dd-MMM-yyyy} !");
                    }
                }
                else
                {
                    throw new Exception("SINCE <date> value is missing !");
                }
            }

                #endregion

                #region SMALLER

                //	SMALLER <n>
                //		Messages with an [RFC-2822] size smaller than the specified number of octets.
            else if (word == "SMALLER")
            {
                searchKeyName = "SMALLER";

                // Read <n>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    val = TextUtils.UnQuoteString(val);

                    // Parse <n> - must be integer value
                    try
                    {
                        searchKeyValue = Convert.ToInt64(val);
                    }
                        // Invalid <n>
                    catch
                    {
                        throw new Exception("Invalid SMALLER <n> value '" + val +
                                            "', it must be numeric value !");
                    }
                }
                else
                {
                    throw new Exception("SMALLER <n> value is missing !");
                }
            }

                #endregion

                #region SUBJECT

                //	SUBJECT <string>
                //		Messages that contain the specified string in the envelope structure's SUBJECT field.
            else if (word == "SUBJECT")
            {
                // We internally use HEADER "SUBJECT:" "value"
                searchKeyName = "HEADER";

                // Read <string>
                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = new[] {"SUBJECT:", TextUtils.UnQuoteString(val)};
                }
                else
                {
                    throw new Exception("SUBJECT <string> value is missing !");
                }
            }

                #endregion

                #region TEXT

                //	TEXT <string>
                //		Messages that contain the specified string in the header or body of the message.
            else if (word == "TEXT")
            {
                searchKeyName = "TEXT";

                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = val;
                }
                else
                {
                    throw new Exception("TEXT <string> value is missing !");
                }
            }

                #endregion

                #region TO

                //	TO <string>
                //		Messages that contain the specified string in the envelope structure's TO field.
            else if (word == "TO")
            {
                // We internally use HEADER "TO:" "value"
                searchKeyName = "HEADER";

                // Read <string>
                string val = ReadString(reader);
                if (val != null)
                {
                    searchKeyValue = new[] {"TO:", TextUtils.UnQuoteString(val)};
                }
                else
                {
                    throw new Exception("TO <string> value is missing !");
                }
            }

                #endregion

                #region UID

                //	UID <sequence set>
                //		Messages with unique identifiers corresponding to the specified
                //		unique identifier set.  Sequence set ranges are permitted.
            else if (word == "UID")
            {
                searchKeyName = "UID";

                // Read <sequence set>
                string val = reader.QuotedReadToDelimiter(' ');

                if (val != null)
                {
                    try
                    {
                        IMAP_SequenceSet sequenceSet = new IMAP_SequenceSet();
                        sequenceSet.Parse(TextUtils.UnQuoteString(val), long.MaxValue);

                        searchKeyValue = sequenceSet;
                    }
                    catch
                    {
                        throw new Exception("Invalid UID <sequence-set> value '" + val + "' !");
                    }
                }
                else
                {
                    throw new Exception("UID <sequence-set> value is missing !");
                }
            }

                #endregion

                #region UNANSWERED

                //	UNANSWERED
                //		Messages that do not have the \Answered flag set.
            else if (word == "UNANSWERED")
            {
                // We internally use UNKEYWORD SEEN
                searchKeyName = "UNKEYWORD";
                searchKeyValue = "ANSWERED";
            }

                #endregion

                #region UNDELETED

                //	UNDELETED
                //		Messages that do not have the \Deleted flag set.
            else if (word == "UNDELETED")
            {
                // We internally use UNKEYWORD UNDELETED
                searchKeyName = "UNKEYWORD";
                searchKeyValue = "DELETED";
            }

                #endregion

                #region UNDRAFT

                //	UNDRAFT
                //		Messages that do not have the \Draft flag set.
            else if (word == "UNDRAFT")
            {
                // We internally use UNKEYWORD UNDRAFT
                searchKeyName = "UNKEYWORD";
                searchKeyValue = "DRAFT";
            }

                #endregion

                #region UNFLAGGED

                //	UNFLAGGED
                //		Messages that do not have the \Flagged flag set.
            else if (word == "UNFLAGGED")
            {
                // We internally use UNKEYWORD UNFLAGGED
                searchKeyName = "UNKEYWORD";
                searchKeyValue = "FLAGGED";
            }

                #endregion

                #region UNKEYWORD

                //	UNKEYWORD <flag>
                //		Messages that do not have the specified keyword flag set.
            else if (word == "UNKEYWORD")
            {
                searchKeyName = "UNKEYWORD";

                // Read <flag>
                string val = reader.QuotedReadToDelimiter(' ');
                if (val != null)
                {
                    searchKeyValue = TextUtils.UnQuoteString(val);
                }
                else
                {
                    throw new Exception("UNKEYWORD <flag> value is missing !");
                }
            }

                #endregion

                #region UNSEEN

                //	UNSEEN
                //		Messages that do not have the \Seen flag set.
            else if (word == "UNSEEN")
            {
                // We internally use UNKEYWORD UNSEEN
                searchKeyName = "UNKEYWORD";
                searchKeyValue = "SEEN";
            }

                #endregion

                #region Unknown or SEQUENCESET

                // Unkown keyword or <sequence set>
            else
            {
                // DUMMY palce(bad design) in IMAP.
                // Active keyword can be <sequence set> or bad keyword, there is now way to distinguish what is meant.
                // Why they don't key work SEQUENCESET <sequence set> ?  

                // <sequence set>
                //		Messages with message sequence numbers corresponding to the
                //		specified message sequence number set.

                // Just try if it can be parsed as sequence-set
                try
                {
                    IMAP_SequenceSet sequenceSet = new IMAP_SequenceSet();
                    sequenceSet.Parse(word, long.MaxValue);

                    searchKeyName = "SEQUENCESET";
                    searchKeyValue = sequenceSet;
                }
                    // This isn't vaild sequnce-set value
                catch
                {
                    throw new Exception("Invalid search key or <sequnce-set> value '" + word + "' !");
                }
            }

            #endregion

            // REMOVE ME:
            //	Console.WriteLine(searchKeyName + " : " + Convert.ToString(searchKeyValue));

            return new SearchKey(searchKeyName, searchKeyValue);
        }

        // TODO: We have envelope, see if Header is needed or can use envelope for it

        /// <summary>
        /// Gets if message Header is needed for matching.
        /// </summary>
        /// <returns></returns>
        public bool IsHeaderNeeded()
        {
            if (m_SearchKeyName == "HEADER")
            {
                return true;
            }
            else if (m_SearchKeyName == "NOT")
            {
                return SearchGroup.IsHeaderNeededForKey(m_SearchKeyValue);
            }
            else if (m_SearchKeyName == "OR")
            {
                object serachKey1 = ((object[]) m_SearchKeyValue)[0];
                object serachKey2 = ((object[]) m_SearchKeyValue)[1];

                if (SearchGroup.IsHeaderNeededForKey(serachKey1) ||
                    SearchGroup.IsHeaderNeededForKey(serachKey2))
                {
                    return true;
                }
            }
            else if (m_SearchKeyName == "SENTBEFORE")
            {
                return true;
            }
            else if (m_SearchKeyName == "SENTON")
            {
                return true;
            }
            else if (m_SearchKeyName == "SENTSINCE")
            {
                return true;
            }
            else if (m_SearchKeyName == "TEXT")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets if message body text is needed for matching.
        /// </summary>
        /// <returns></returns>
        public bool IsBodyTextNeeded()
        {
            if (m_SearchKeyName == "BODY")
            {
                return true;
            }
            else if (m_SearchKeyName == "NOT")
            {
                return SearchGroup.IsBodyTextNeededForKey(m_SearchKeyValue);
            }
            else if (m_SearchKeyName == "OR")
            {
                object serachKey1 = ((object[]) m_SearchKeyValue)[0];
                object serachKey2 = ((object[]) m_SearchKeyValue)[1];

                if (SearchGroup.IsBodyTextNeededForKey(serachKey1) ||
                    SearchGroup.IsBodyTextNeededForKey(serachKey2))
                {
                    return true;
                }
            }
            else if (m_SearchKeyName == "TEXT")
            {
                return true;
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
            #region ALL

            // ALL
            //		All messages in the mailbox; the default initial key for ANDing.
            if (m_SearchKeyName == "ALL")
            {
                return true;
            }

                #endregion

                #region BEFORE

                // BEFORE <date>
                //	Messages whose internal date (disregarding time and timezone)
                //	is earlier than the specified date.
            else if (m_SearchKeyName == "BEFORE")
            {
                if (internalDate.Date < (DateTime) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region BODY

                // BODY <string>
                //	Messages that contain the specified string in the body of the message.
                //
                //	NOTE: Compare must be done on decoded header and decoded body of message.
                //		  In all search keys that use strings, a message matches the key if
                //		  the string is a substring of the field.  The matching is case-insensitive.
            else if (m_SearchKeyName == "BODY")
            {
                string val = bodyText;
                if (val != null && val.ToLower().IndexOf(((string) m_SearchKeyValue).ToLower()) > -1)
                {
                    return true;
                }
            }

                #endregion

                #region HEADER

                // HEADER <field-name> <string>
                //	Messages that have a header with the specified field-name (as
                //	defined in [RFC-2822]) and that contains the specified string
                //	in the text of the header (what comes after the colon).  If the
                //	string to search is zero-length, this matches all messages that
                //	have a header line with the specified field-name regardless of
                //	the contents.
                //
                //	NOTE: Compare must be done on decoded header field value.
                //		  In all search keys that use strings, a message matches the key if
                //		  the string is a substring of the field.  The matching is case-insensitive.
            else if (m_SearchKeyName == "HEADER")
            {
                string[] headerField_value = (string[]) m_SearchKeyValue;

                // If header field name won't end with :, add it
                if (!headerField_value[0].EndsWith(":"))
                {
                    headerField_value[0] = headerField_value[0] + ":";
                }

                if (mime.MainEntity.Header.Contains(headerField_value[0]))
                {
                    if (headerField_value[1].Length == 0)
                    {
                        return true;
                    }
                    else if (
                        mime.MainEntity.Header.GetFirst(headerField_value[0]).Value.ToLower().IndexOf(
                            headerField_value[1].ToLower()) > -1)
                    {
                        return true;
                    }
                }
            }

                #endregion

                #region KEYWORD

                // KEYWORD <flag>
                //	Messages with the specified keyword flag set.
            else if (m_SearchKeyName == "KEYWORD")
            {
                if ((flags & IMAP_Utils.ParseMessageFlags((string) m_SearchKeyValue)) != 0)
                {
                    return true;
                }
            }

                #endregion
				
                #region LARGER
	
                // LARGER <n>
                //	Messages with an [RFC-2822] size larger than the specified number of octets.
            else if (m_SearchKeyName == "LARGER")
            {
                if (size > (long) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region NOT

                //	NOT <search-key> or (<search-key> <search-key> ...)(SearchGroup)
                //		Messages that do not match the specified search key.
            else if (m_SearchKeyName == "NOT")
            {
                return
                    !SearchGroup.Match_Key_Value(m_SearchKeyValue,
                                                 no,
                                                 uid,
                                                 size,
                                                 internalDate,
                                                 flags,
                                                 mime,
                                                 bodyText);
            }

                #endregion

                #region ON

                // ON <date>
                //	Messages whose internal date (disregarding time and timezone)
                //	is within the specified date.
            else if (m_SearchKeyName == "ON")
            {
                if (internalDate.Date == (DateTime) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region OR

                //	OR <search-key1> <search-key2> - SearckKey can be parenthesis list of keys !
                //		Messages that match either search key.
            else if (m_SearchKeyName == "OR")
            {
                object serachKey1 = ((object[]) m_SearchKeyValue)[0];
                object serachKey2 = ((object[]) m_SearchKeyValue)[1];

                if (
                    SearchGroup.Match_Key_Value(serachKey1, no, uid, size, internalDate, flags, mime, bodyText) ||
                    SearchGroup.Match_Key_Value(serachKey2, no, uid, size, internalDate, flags, mime, bodyText))
                {
                    return true;
                }
            }

                #endregion

                #region SENTBEFORE

                // SENTBEFORE <date>
                //	Messages whose [RFC-2822] Date: header (disregarding time and
                //	timezone) is earlier than the specified date.
            else if (m_SearchKeyName == "SENTBEFORE")
            {
                if (mime.MainEntity.Date.Date < (DateTime) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region SENTON

                // SENTON <date>
                //	Messages whose [RFC-2822] Date: header (disregarding time and
                //	timezone) is within the specified date.
            else if (m_SearchKeyName == "SENTON")
            {
                if (mime.MainEntity.Date.Date == (DateTime) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region SENTSINCE

                // SENTSINCE <date>
                //	Messages whose [RFC-2822] Date: header (disregarding time and
                //	timezone) is within or later than the specified date.
            else if (m_SearchKeyName == "SENTSINCE")
            {
                if (mime.MainEntity.Date.Date >= (DateTime) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region SINCE

                // SINCE <date>
                //	Messages whose internal date (disregarding time and timezone)
                //	is within or later than the specified date.	
            else if (m_SearchKeyName == "SINCE")
            {
                if (internalDate.Date >= (DateTime) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region SMALLER

                // SMALLER <n>
                //	Messages with an [RFC-2822] size smaller than the specified	number of octets.
            else if (m_SearchKeyName == "SMALLER")
            {
                if (size < (long) m_SearchKeyValue)
                {
                    return true;
                }
            }

                #endregion

                #region TEXT

                // TEXT <string>
                //	Messages that contain the specified string in the header or	body of the message.
                //
                //  NOTE: Compare must be done on decoded header and decoded body of message.
                //		  In all search keys that use strings, a message matches the key if
                //		  the string is a substring of the field.  The matching is case-insensitive.
            else if (m_SearchKeyName == "TEXT")
            {
                // See body first
                string val = bodyText;
                if (val != null && val.ToLower().IndexOf(((string) m_SearchKeyValue).ToLower()) > -1)
                {
                    return true;
                }

                // If we reach so far, that means body won't contain specified text and we need to check header.
                foreach (HeaderField headerField in mime.MainEntity.Header)
                {
                    if (headerField.Value.ToLower().IndexOf(((string) m_SearchKeyValue).ToLower()) > -1)
                    {
                        return true;
                    }
                }
            }

                #endregion

                #region UID

                // UID <sequence set>
                //	Messages with unique identifiers corresponding to the specified
                //	unique identifier set.  Sequence set ranges are permitted.
            else if (m_SearchKeyName == "UID")
            {
                return ((IMAP_SequenceSet) m_SearchKeyValue).Contains(uid);
            }

                #endregion

                #region UNKEYWORD

                // UNKEYWORD <flag>
                //	Messages that do not have the specified keyword flag set.
            else if (m_SearchKeyName == "UNKEYWORD")
            {
                if ((flags & IMAP_Utils.ParseMessageFlags((string) m_SearchKeyValue)) == 0)
                {
                    return true;
                }
            }

                #endregion

                #region SEQUENCESET

                // <sequence set>
                //		Messages with message sequence numbers corresponding to the
                //		specified message sequence number set.
            else if (m_SearchKeyName == "SEQUENCESET")
            {
                return ((IMAP_SequenceSet) m_SearchKeyValue).Contains(no);
            }

            #endregion

            return false;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Reads search-key &lt;string&gt; value.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static string ReadString(StringReader reader)
        {
            //Remove spaces from string start
            reader.ReadToFirstChar();

            // We must support:
            //	word
            //  "text"
            //	{string_length}data(string_length)

            // {string_length}data(string_length)
            if (reader.StartsWith("{"))
            {
                // Remove {
                reader.ReadSpecifiedLength("{".Length);

                int dataLength = Convert.ToInt32(reader.QuotedReadToDelimiter('}'));
                return reader.ReadSpecifiedLength(dataLength);
            }

            return TextUtils.UnQuoteString(reader.QuotedReadToDelimiter(' '));
        }

        #endregion
    }
}