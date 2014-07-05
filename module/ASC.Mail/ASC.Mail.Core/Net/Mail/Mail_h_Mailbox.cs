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

namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using System.Text;
    using MIME;

    #endregion

    /// <summary>
    /// This class represent generic <b>mailbox</b> header fields. For example: Sender: header.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 5322.
    ///     header = "FiledName:" mailbox CRLF
    /// </code>
    /// </example>
    public class Mail_h_Mailbox : MIME_h
    {
        #region Members

        private readonly string m_Name;
        private readonly Mail_t_Mailbox m_pAddress;
        private string m_ParseValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified
        {
            get { return false; }
        }

        /// <summary>
        /// Gets header field name. For example "Sender".
        /// </summary>
        public override string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets mailbox address.
        /// </summary>
        public Mail_t_Mailbox Address
        {
            get { return m_pAddress; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="fieldName">Header field name. For example: "Sender".</param>
        /// <param name="mailbox">Mailbox value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>filedName</b> or <b>mailbox</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Mail_h_Mailbox(string fieldName, Mail_t_Mailbox mailbox)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }
            if (fieldName == string.Empty)
            {
                throw new ArgumentException("Argument 'fieldName' value must be specified.");
            }
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox");
            }

            m_Name = fieldName;
            m_pAddress = mailbox;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Sender: john.doe@domain.com'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static Mail_h_Mailbox Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string[] name_value = value.Split(new[] {':'}, 2);
            if (name_value.Length != 2)
            {
                throw new ParseException("Invalid header field value '" + value + "'.");
            }

            MIME_Reader r = new MIME_Reader(name_value[1]);

            string word = r.QuotedReadToDelimiter(new[] {',', '<', ':'});
            // Invalid value.
            if (word == null)
            {
                throw new ParseException("Invalid header field value '" + value + "'.");
            }
                // name-addr
            else if (r.Peek(true) == '<')
            {
                Mail_h_Mailbox h = new Mail_h_Mailbox(name_value[0],
                                                      new Mail_t_Mailbox(
                                                          word != null
                                                              ? MIME_Encoding_EncodedWord.DecodeS(
                                                                    TextUtils.UnQuoteString(word))
                                                              : null,
                                                          r.ReadParenthesized()));
                h.m_ParseValue = value;

                return h;
            }
                // addr-spec
            else
            {
                Mail_h_Mailbox h = new Mail_h_Mailbox(name_value[0], new Mail_t_Mailbox(null, word));
                h.m_ParseValue = value;

                return h;
            }
        }

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (m_ParseValue != null)
            {
                return m_ParseValue;
            }
            else
            {
                return m_Name + ": " + m_pAddress.ToString(wordEncoder) + "\r\n";
            }
        }

        #endregion
    }
}