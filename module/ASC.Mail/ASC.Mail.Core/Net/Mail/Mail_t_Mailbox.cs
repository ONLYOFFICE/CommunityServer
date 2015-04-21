/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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