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


namespace ASC.Mail.Net.IMAP
{
    #region usings

    using System;
    using System.Collections;
    using System.Text;
    using Mime;
    using MIME;

    #endregion

    /// <summary>
    /// IMAP ENVELOPE STRUCTURE (date, subject, from, sender, reply-to, to, cc, bcc, in-reply-to, and message-id).
    ///  Defined in RFC 3501 7.4.2.
    /// </summary>
    public class IMAP_Envelope
    {
        #region Members

        private MailboxAddress[] m_Bcc;
        private MailboxAddress[] m_Cc;
        private DateTime m_Date = DateTime.MinValue;
        private MailboxAddress[] m_From;
        private string m_InReplyTo;
        private string m_MessageID;
        private MailboxAddress[] m_ReplyTo;
        private MailboxAddress m_Sender;
        private string m_Subject;
        private MailboxAddress[] m_To;

        #endregion

        #region Properties

        /// <summary>
        /// Gets header field "<b>Date:</b>" value. Returns DateTime.MinValue if no date or date parsing fails.
        /// </summary>
        public DateTime Date
        {
            get { return m_Date; }
        }

        /// <summary>
        /// Gets header field "<b>Subject:</b>" value. Returns null if value isn't set.
        /// </summary>
        public string Subject
        {
            get { return m_Subject; }
        }

        /// <summary>
        /// Gets header field "<b>From:</b>" value. Returns null if value isn't set.
        /// </summary>
        public MailboxAddress[] From
        {
            get { return m_From; }
        }

        /// <summary>
        /// Gets header field "<b>Sender:</b>" value. Returns null if value isn't set.
        /// </summary>
        public MailboxAddress Sender
        {
            get { return m_Sender; }
        }

        /// <summary>
        /// Gets header field "<b>Reply-To:</b>" value. Returns null if value isn't set.
        /// </summary>
        public MailboxAddress[] ReplyTo
        {
            get { return m_ReplyTo; }
        }

        /// <summary>
        /// Gets header field "<b>To:</b>" value. Returns null if value isn't set.
        /// </summary>
        public MailboxAddress[] To
        {
            get { return m_To; }
        }

        /// <summary>
        /// Gets header field "<b>Cc:</b>" value. Returns null if value isn't set.
        /// </summary>
        public MailboxAddress[] Cc
        {
            get { return m_Cc; }
        }

        /// <summary>
        /// Gets header field "<b>Bcc:</b>" value. Returns null if value isn't set.
        /// </summary>
        public MailboxAddress[] Bcc
        {
            get { return m_Bcc; }
        }

        /// <summary>
        /// Gets header field "<b>In-Reply-To:</b>" value. Returns null if value isn't set.
        /// </summary>
        public string InReplyTo
        {
            get { return m_InReplyTo; }
        }

        /// <summary>
        /// Gets header field "<b>Message-ID:</b>" value. Returns null if value isn't set.
        /// </summary>
        public string MessageID
        {
            get { return m_MessageID; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Construct secified mime entity ENVELOPE string.
        /// </summary>
        /// <param name="entity">Mime entity.</param>
        /// <returns></returns>
        public static string ConstructEnvelope(MimeEntity entity)
        {
            /* RFC 3501 7.4.2
				ENVELOPE
					A parenthesized list that describes the envelope structure of a
					message.  This is computed by the server by parsing the
					[RFC-2822] header into the component parts, defaulting various
					fields as necessary.

					The fields of the envelope structure are in the following
					order: date, subject, from, sender, reply-to, to, cc, bcc,
					in-reply-to, and message-id.  The date, subject, in-reply-to,
					and message-id fields are strings.  The from, sender, reply-to,
					to, cc, and bcc fields are parenthesized lists of address
					structures.

					An address structure is a parenthesized list that describes an
					electronic mail address.  The fields of an address structure
					are in the following order: personal name, [SMTP]
					at-domain-list (source route), mailbox name, and host name.

					[RFC-2822] group syntax is indicated by a special form of
					address structure in which the host name field is NIL.  If the
					mailbox name field is also NIL, this is an end of group marker
					(semi-colon in RFC 822 syntax).  If the mailbox name field is
					non-NIL, this is a start of group marker, and the mailbox name
					field holds the group name phrase.

					If the Date, Subject, In-Reply-To, and Message-ID header lines
					are absent in the [RFC-2822] header, the corresponding member
					of the envelope is NIL; if these header lines are present but
					empty the corresponding member of the envelope is the empty
					string.
					
						Note: some servers may return a NIL envelope member in the
						"present but empty" case.  Clients SHOULD treat NIL and
						empty string as identical.

						Note: [RFC-2822] requires that all messages have a valid
						Date header.  Therefore, the date member in the envelope can
						not be NIL or the empty string.

						Note: [RFC-2822] requires that the In-Reply-To and
						Message-ID headers, if present, have non-empty content.
						Therefore, the in-reply-to and message-id members in the
						envelope can not be the empty string.

					If the From, To, cc, and bcc header lines are absent in the
					[RFC-2822] header, or are present but empty, the corresponding
					member of the envelope is NIL.

					If the Sender or Reply-To lines are absent in the [RFC-2822]
					header, or are present but empty, the server sets the
					corresponding member of the envelope to be the same value as
					the from member (the client is not expected to know to do
					this).

						Note: [RFC-2822] requires that all messages have a valid
						From header.  Therefore, the from, sender, and reply-to
						members in the envelope can not be NIL.
		 
					ENVELOPE ("date" "subject" from sender reply-to to cc bcc "in-reply-to" "messageID")
			*/

            // NOTE: all header fields and parameters must in ENCODED form !!!

            StringBuilder retVal = new StringBuilder();
            retVal.Append("(");

            // date
            if (entity.Header.Contains("Date:"))
            {
                retVal.Append(TextUtils.QuoteString(MimeUtils.DateTimeToRfc2822(entity.Date)));
            }
            else
            {
                retVal.Append("NIL");
            }

            // subject
            if (entity.Subject != null)
            {
                retVal.Append(" " + TextUtils.QuoteString(MimeUtils.EncodeHeaderField(entity.Subject)));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // from
            if (entity.From != null && entity.From.Count > 0)
            {
                retVal.Append(" " + ConstructAddresses(entity.From));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // sender	
            //	NOTE: There is confusing part, according rfc 2822 Sender: is MailboxAddress and not AddressList.
            if (entity.Sender != null)
            {
                retVal.Append(" (");

                retVal.Append(ConstructAddress(entity.Sender));

                retVal.Append(")");
            }
            else if (entity.From != null)
            {
                retVal.Append(" " + ConstructAddresses(entity.From));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // reply-to
            if (entity.ReplyTo != null)
            {
                retVal.Append(" " + ConstructAddresses(entity.ReplyTo));
            }
            else if (entity.From != null)
            {
                retVal.Append(" " + ConstructAddresses(entity.From));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // to
            if (entity.To != null && entity.To.Count > 0)
            {
                retVal.Append(" " + ConstructAddresses(entity.To));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // cc
            if (entity.Cc != null && entity.Cc.Count > 0)
            {
                retVal.Append(" " + ConstructAddresses(entity.Cc));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // bcc
            if (entity.Bcc != null && entity.Bcc.Count > 0)
            {
                retVal.Append(" " + ConstructAddresses(entity.Bcc));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // in-reply-to			
            if (entity.InReplyTo != null)
            {
                retVal.Append(" " + TextUtils.QuoteString(MimeUtils.EncodeHeaderField(entity.InReplyTo)));
            }
            else
            {
                retVal.Append(" NIL");
            }

            // message-id
            if (entity.MessageID != null)
            {
                retVal.Append(" " + TextUtils.QuoteString(MimeUtils.EncodeHeaderField(entity.MessageID)));
            }
            else
            {
                retVal.Append(" NIL");
            }

            retVal.Append(")");

            return retVal.ToString();
        }

        /// <summary>
        /// Parses ENVELOPE from IMAP envelope string.
        /// </summary>
        /// <param name="envelopeString">Envelope string.</param>
        public void Parse(string envelopeString)
        {
            if (envelopeString.StartsWith("("))
            {
                envelopeString = envelopeString.Substring(1);
            }
            if (envelopeString.EndsWith(")"))
            {
                envelopeString = envelopeString.Substring(0, envelopeString.Length - 1);
            }

            string word = "";
            StringReader r = new StringReader(envelopeString);

            #region Date

            // Date
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("Invalid IMAP ENVELOPE structure !");
            }
            if (word.ToUpper() == "NIL")
            {
                m_Date = DateTime.MinValue;
            }
            else
            {
                try
                {
                    m_Date = MimeUtils.ParseDate(word);
                }
                catch
                {
                    // Failed to parse date, return minimum.
                    m_Date = DateTime.MinValue;
                }
            }

            #endregion

            #region Subject

            // Subject
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("Invalid IMAP ENVELOPE structure !");
            }
            if (word.ToUpper() == "NIL")
            {
                m_Subject = null;
            }
            else
            {
                m_Subject = MIME_Encoding_EncodedWord.DecodeS(word);
            }

            #endregion

            #region From

            // From
            m_From = ParseAddresses(r);

            #endregion

            #region Sender

            // Sender
            //	NOTE: There is confusing part, according rfc 2822 Sender: is MailboxAddress and not AddressList.
            MailboxAddress[] sender = ParseAddresses(r);
            if (sender != null && sender.Length > 0)
            {
                m_Sender = sender[0];
            }
            else
            {
                m_Sender = null;
            }

            #endregion

            #region ReplyTo

            // ReplyTo
            m_ReplyTo = ParseAddresses(r);

            #endregion

            #region To

            // To
            m_To = ParseAddresses(r);

            #endregion

            #region Cc

            // Cc
            m_Cc = ParseAddresses(r);

            #endregion

            #region Bcc

            // Bcc
            m_Bcc = ParseAddresses(r);

            #endregion

            #region InReplyTo

            // InReplyTo
            r.ReadToFirstChar();
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("Invalid IMAP ENVELOPE structure !");
            }
            if (word.ToUpper() == "NIL")
            {
                m_InReplyTo = null;
            }
            else
            {
                m_InReplyTo = word;
            }

            #endregion

            #region MessageID

            // MessageID
            r.ReadToFirstChar();
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("Invalid IMAP ENVELOPE structure !");
            }
            if (word.ToUpper() == "NIL")
            {
                m_MessageID = null;
            }
            else
            {
                m_MessageID = word;
            }

            #endregion
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Constructs ENVELOPE addresses structure.
        /// </summary>
        /// <param name="addressList">Address list.</param>
        /// <returns></returns>
        private static string ConstructAddresses(AddressList addressList)
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append("(");

            foreach (MailboxAddress address in addressList.Mailboxes)
            {
                retVal.Append(ConstructAddress(address));
            }

            retVal.Append(")");

            return retVal.ToString();
        }

        /// <summary>
        /// Constructs ENVELOPE address structure.
        /// </summary>
        /// <param name="address">Mailbox address.</param>
        /// <returns></returns>
        private static string ConstructAddress(MailboxAddress address)
        {
            /* An address structure is a parenthesized list that describes an
			   electronic mail address.  The fields of an address structure
			   are in the following order: personal name, [SMTP]
			   at-domain-list (source route), mailbox name, and host name.
			*/

            // NOTE: all header fields and parameters must in ENCODED form !!!

            StringBuilder retVal = new StringBuilder();
            retVal.Append("(");

            // personal name
            retVal.Append(TextUtils.QuoteString(MimeUtils.EncodeHeaderField(address.DisplayName)));

            // source route, always NIL (not used nowdays)
            retVal.Append(" NIL");

            // mailbox name
            retVal.Append(" " + TextUtils.QuoteString(MimeUtils.EncodeHeaderField(address.LocalPart)));

            // host name
            retVal.Append(" " + TextUtils.QuoteString(MimeUtils.EncodeHeaderField(address.Domain)));

            retVal.Append(")");

            return retVal.ToString();
        }

        /// <summary>
        /// Parses addresses from IMAP ENVELOPE addresses structure.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private MailboxAddress[] ParseAddresses(StringReader r)
        {
            r.ReadToFirstChar();
            if (r.StartsWith("NIL", false))
            {
                // Remove NIL
                r.ReadSpecifiedLength("NIL".Length);

                return null;
            }
            else
            {
                r.ReadToFirstChar();

                // This must be ((address)[*(address)])
                if (!r.StartsWith("("))
                {
                    throw new Exception("Invalid IMAP ENVELOPE structure !");
                }
                else
                {
                    // Read addresses
                    string addressesString = r.ReadParenthesized();

                    ArrayList addresses = new ArrayList();
                    StringReader rAddresses = new StringReader(addressesString.Trim());
                    // Now we have (address)[*(address)], read addresses
                    while (rAddresses.StartsWith("("))
                    {
                        addresses.Add(ParseAddress(rAddresses.ReadParenthesized()));

                        rAddresses.ReadToFirstChar();
                    }

                    MailboxAddress[] retVal = new MailboxAddress[addresses.Count];
                    addresses.CopyTo(retVal);

                    return retVal;
                }
            }
        }

        /// <summary>
        /// Parses address from IMAP ENVELOPE address structure.
        /// </summary>
        /// <param name="addressString">Address structure string.</param>
        /// <returns></returns>
        private MailboxAddress ParseAddress(string addressString)
        {
            /* RFC 3501 7.4.2 ENVELOPE
				An address structure is a parenthesized list that describes an
				electronic mail address.  The fields of an address structure
				are in the following order: personal name, [SMTP]
				at-domain-list (source route), mailbox name, and host name.
			*/

            StringReader r = new StringReader(addressString.Trim());
            string personalName = "";
            string emailAddress = "";

            // personal name
            if (r.StartsWith("NIL", false))
            {
                // Remove NIL
                r.ReadSpecifiedLength("NIL".Length);
            }
            else
            {
                personalName = MIME_Encoding_EncodedWord.DecodeS(r.ReadWord());
            }

            // source route, always NIL (not used nowdays)
            r.ReadWord();

            // mailbox name
            if (r.StartsWith("NIL", false))
            {
                // Remove NIL
                r.ReadSpecifiedLength("NIL".Length);
            }
            else
            {
                emailAddress = r.ReadWord() + "@";
            }

            // host name
            if (r.StartsWith("NIL", false))
            {
                // Remove NIL
                r.ReadSpecifiedLength("NIL".Length);
            }
            else
            {
                emailAddress += r.ReadWord();
            }

            return new MailboxAddress(personalName, emailAddress);
        }

        #endregion
    }
}