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


namespace ASC.Mail.Net.SMTP
{
    #region usings

    using System;
    using MIME;

    #endregion

    /// <summary>
    /// This class represents SMTP "Mailbox" address. Defined in RFC 5321 4.1.2.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 5321 4.1.2.
    ///     Mailbox    = Local-part "@" ( Domain / address-literal )
    ///     Local-part = Dot-string / Quoted-string
    ///                  ; MAY be case-sensitive
    /// </code>
    /// </example>
    public class SMTP_t_Mailbox
    {
        #region Members

        private readonly string m_Domain;
        private readonly string m_LocalPart;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="localPart">Local part of mailbox.</param>
        /// <param name="domain">Domain of mailbox.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>localPart</b> or <b>domain</b> is null reference.</exception>
        public SMTP_t_Mailbox(string localPart, string domain)
        {
            if (localPart == null)
            {
                throw new ArgumentNullException("localPart");
            }
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }

            m_LocalPart = localPart;
            m_Domain = domain;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets domain of mailbox.
        /// </summary>
        /// <remarks>If domain is <b>address-literal</b>, surrounding bracets will be removed.</remarks>
        public string Domain
        {
            get { return m_Domain; }
        }

        /// <summary>
        /// Gets local-part of mailbox.
        /// </summary>
        /// <remarks>If local-part is <b>Quoted-string</b>, quotes will not returned.</remarks>
        public string LocalPart
        {
            get { return m_LocalPart; }
        }

        #endregion

        /*
        /// <summary>
        /// Parses SMTP mailbox from the specified string.
        /// </summary>
        /// <param name="value">Mailbox string.</param>
        /// <returns>Returns parsed SMTP mailbox.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static SMTP_t_Mailbox Parse(string value)
        {
            if(value == null){
                throw new ArgumentNullException("value");
            }

            return Parse(new ABNF_Reader(value));
        }

        /// <summary>
        /// Parses SMTP mailbox from the specified reader.
        /// </summary>
        /// <param name="reader">Source reader.</param>
        /// <returns>Returns parsed SMTP mailbox.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>reader</b> is null reference.</exception>
        public static SMTP_t_Mailbox Parse(ABNF_Reader reader)
        {
            if(reader == null){
                throw new ArgumentNullException("reader");
            }

            // TODO:

            return null;
        }
        */

        #region Methods

        /// <summary>
        /// Returns mailbox as string.
        /// </summary>
        /// <returns>Returns mailbox as string.</returns>
        public override string ToString()
        {
            if (MIME_Reader.IsDotAtom(m_LocalPart))
            {
                return m_LocalPart + "@" + (Net_Utils.IsIPAddress(m_Domain) ? "[" + m_Domain + "]" : m_Domain);
            }
            else
            {
                return TextUtils.QuoteString(m_LocalPart) + "@" +
                       (Net_Utils.IsIPAddress(m_Domain) ? "[" + m_Domain + "]" : m_Domain);
            }
        }

        #endregion
    }
}