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


namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using MIME;

    #endregion

    /// <summary>
    /// This class represents "mailbox" address. Defined in RFC 5322 3.4.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 5322 3.4.
    ///     mailbox    = name-addr / addr-spec
    ///     name-addr  = [display-name] angle-addr
    ///     angle-addr = [CFWS] "&lt;" addr-spec "&gt;" [CFWS]
    /// </code>
    /// </example>
    public class Mail_t_Mailbox : Mail_t_Address
    {
        #region Members

        private readonly string m_Address;
        private readonly string m_DisplayName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets display name. Value null means not specified.
        /// </summary>
        public string DisplayName
        {
            get { return m_DisplayName; }
        }

        /// <summary>
        /// Gets address.
        /// </summary>
        public string Address
        {
            get { return m_Address; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="displayName">Display name. Value null means not specified.</param>
        /// <param name="address">Email address.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>address</b> is null reference.</exception>
        public Mail_t_Mailbox(string displayName, string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            m_DisplayName = displayName;
            m_Address = address;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns mailbox as string.
        /// </summary>
        /// <returns>Returns mailbox as string.</returns>
        public override string ToString()
        {
            return ToString(null).Trim();
        }

        /// <summary>
        /// Returns address as string value.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <returns>Returns address as string value.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder)
        {
            if (string.IsNullOrEmpty(m_DisplayName))
            {
                return "<" + m_Address.Trim() + ">";
            }
            else
            {
                if (MIME_Encoding_EncodedWord.MustEncode(m_DisplayName))
                {
                    return wordEncoder == null
                               ? (m_DisplayName + " " + "<" + m_Address.Trim() + ">")
                               : (wordEncoder.Encode(m_DisplayName) + " " + "<" + m_Address.Trim() + ">");
                }
                else
                {
                    return TextUtils.QuoteString(m_DisplayName) + " " + "<" + m_Address.Trim() + ">";
                }
            }
        }

        #endregion
    }
}