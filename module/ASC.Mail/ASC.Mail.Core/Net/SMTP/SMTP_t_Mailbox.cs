/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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