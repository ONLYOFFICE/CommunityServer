/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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