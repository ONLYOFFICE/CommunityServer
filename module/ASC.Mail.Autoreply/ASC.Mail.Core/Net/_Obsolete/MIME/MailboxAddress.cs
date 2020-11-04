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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;
    using MIME;

    #endregion

    /// <summary>
    /// RFC 2822 3.4. (Address Specification) Mailbox address. 
    /// <p/>
    /// Syntax: ["display-name"&lt;SP&gt;]&lt;local-part@domain&gt;.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class MailboxAddress : Address
    {
        #region Members

        private string m_DisplayName = "";
        private string m_EmailAddress = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MailboxAddress() : base(false) {}

        /// <summary>
        /// Creates new mailbox from specified email address.
        /// </summary>
        /// <param name="emailAddress">Email address.</param>
        public MailboxAddress(string emailAddress) : base(false)
        {
            m_EmailAddress = emailAddress;
        }

        /// <summary>
        /// Creates new mailbox from specified name and email address.
        /// </summary>
        /// <param name="displayName">Display name.</param>
        /// <param name="emailAddress">Email address.</param>
        public MailboxAddress(string displayName, string emailAddress) : base(false)
        {
            m_DisplayName = displayName;
            m_EmailAddress = emailAddress;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets display name. 
        /// </summary>
        public string DisplayName
        {
            get { return m_DisplayName; }

            set
            {
                m_DisplayName = value;

                OnChanged();
            }
        }

        /// <summary>
        /// Gets domain from email address. For example domain is "lumisoft.ee" from "ivar@lumisoft.ee".
        /// </summary>
        public string Domain
        {
            get
            {
                if (EmailAddress.IndexOf("@") != -1)
                {
                    return EmailAddress.Substring(EmailAddress.IndexOf("@") + 1);
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Gets or sets email address. For example ivar@lumisoft.ee.
        /// </summary>
        public string EmailAddress
        {
            get { return m_EmailAddress; }

            set
            {
                // Email address can contain only ASCII chars.
                if (!Core.IsAscii(value))
                {
                    throw new Exception("Email address can contain ASCII chars only !");
                }

                m_EmailAddress = value;

                OnChanged();
            }
        }

        /// <summary>
        /// Gets local-part from email address. For example mailbox is "ivar" from "ivar@lumisoft.ee".
        /// </summary>
        public string LocalPart
        {
            get
            {
                if (EmailAddress.IndexOf("@") > -1)
                {
                    return EmailAddress.Substring(0, EmailAddress.IndexOf("@"));
                }
                else
                {
                    return EmailAddress;
                }
            }
        }

        /// <summary>
        /// Gets Mailbox as RFC 2822(3.4. Address Specification) string. Format: ["display-name"&lt;SP&gt;]&lt;local-part@domain&gt;.
        /// For example, "Ivar Lumi" &lt;ivar@lumisoft.ee&gt;.
        /// </summary>
        [Obsolete("Use ToMailboxAddressString instead !")]
        public string MailboxString
        {
            get
            {
                string retVal = "";
                if (DisplayName != "")
                {
                    retVal += TextUtils.QuoteString(DisplayName) + " ";
                }
                retVal += "<" + EmailAddress + ">";

                return retVal;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses mailbox from mailbox address string.
        /// </summary>
        /// <param name="mailbox">Mailbox string. Format: ["diplay-name"&lt;SP&gt;]&lt;local-part@domain&gt;.</param>
        /// <returns></returns>
        public static MailboxAddress Parse(string mailbox)
        {
            mailbox = mailbox.Trim();

            /* We must parse following situations:
				"Ivar Lumi" <ivar@lumisoft.ee>
				"Ivar Lumi" ivar@lumisoft.ee
				<ivar@lumisoft.ee>
				ivar@lumisoft.ee				
				Ivar Lumi <ivar@lumisoft.ee>
			*/

            string name = "";
            string emailAddress = mailbox;

            // Email address is between <> and remaining left part is display name
            if (mailbox.IndexOf("<") > -1 && mailbox.IndexOf(">") > -1)
            {
                name =
                    MIME_Encoding_EncodedWord.DecodeS(
                        TextUtils.UnQuoteString(mailbox.Substring(0, mailbox.LastIndexOf("<"))));
                emailAddress =
                    mailbox.Substring(mailbox.LastIndexOf("<") + 1,
                                      mailbox.Length - mailbox.LastIndexOf("<") - 2).Trim();
            }
            else
            {
                // There is name included, parse it
                if (mailbox.StartsWith("\""))
                {
                    int startIndex = mailbox.IndexOf("\"");
                    if (startIndex > -1 && mailbox.LastIndexOf("\"") > startIndex)
                    {
                        name =
                            MIME_Encoding_EncodedWord.DecodeS(
                                mailbox.Substring(startIndex + 1, mailbox.LastIndexOf("\"") - startIndex - 1).
                                    Trim());
                    }

                    emailAddress = mailbox.Substring(mailbox.LastIndexOf("\"") + 1).Trim();
                }

                // Right part must be email address
                emailAddress = emailAddress.Replace("<", "").Replace(">", "").Trim();
            }

            return new MailboxAddress(name, emailAddress);
        }

        /// <summary>
        /// Converts this to valid mailbox address string.
        /// Defined in RFC 2822(3.4. Address Specification) string. Format: ["display-name"&lt;SP&gt;]&lt;local-part@domain&gt;.
        /// For example, "Ivar Lumi" &lt;ivar@lumisoft.ee&gt;.
        /// If display name contains unicode chrs, display name will be encoded with canonical encoding in utf-8 charset.
        /// </summary>
        /// <returns></returns>
        public string ToMailboxAddressString()
        {
            string retVal = "";
            if (m_DisplayName.Length > 0)
            {
                if (Core.IsAscii(m_DisplayName))
                {
                    retVal = TextUtils.QuoteString(m_DisplayName) + " ";
                }
                else
                {
                    // Encoded word must be treated as unquoted and unescaped word.
                    retVal = MimeUtils.EncodeWord(m_DisplayName) + " ";
                }
            }
            retVal += "<" + EmailAddress + ">";

            return retVal;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// This called when mailox address has changed.
        /// </summary>
        internal void OnChanged()
        {
            if (Owner != null)
            {
                if (Owner is AddressList)
                {
                    ((AddressList) Owner).OnCollectionChanged();
                }
                else if (Owner is MailboxAddressCollection)
                {
                    ((MailboxAddressCollection) Owner).OnCollectionChanged();
                }
            }
        }

        #endregion
    }
}