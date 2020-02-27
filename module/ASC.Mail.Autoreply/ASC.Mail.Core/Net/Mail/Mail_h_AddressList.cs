/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    /// This class represent generic <b>address-list</b> header fields. For example: To header.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 5322.
    ///     header       = "FiledName:" address-list CRLF
    ///     address-list = (address *("," address))
    ///     address      = mailbox / group
    /// </code>
    /// </example>
    public class Mail_h_AddressList : MIME_h
    {
        #region Members

        private readonly string m_Name;
        private readonly Mail_t_AddressList m_pAddresses;
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
            get { return m_pAddresses.IsModified; }
        }

        /// <summary>
        /// Gets header field name. For example "To".
        /// </summary>
        public override string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets addresses collection.
        /// </summary>
        public Mail_t_AddressList Addresses
        {
            get { return m_pAddresses; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="fieldName">Header field name. For example: "To".</param>
        /// <param name="values">Addresses collection.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>filedName</b> or <b>values</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Mail_h_AddressList(string fieldName, Mail_t_AddressList values)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }
            if (fieldName == string.Empty)
            {
                throw new ArgumentException("Argument 'fieldName' value must be specified.");
            }
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            m_Name = fieldName;
            m_pAddresses = values;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Content-Type: text/plain'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static Mail_h_AddressList Parse(string value)
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

            MIME_Reader r = new MIME_Reader(MIME_Utils.UnfoldHeader(name_value.Length == 2 ? name_value[1].TrimStart() : ""));
            
            Mail_h_AddressList retVal = new Mail_h_AddressList(name_value[0], new Mail_t_AddressList());


            while (true)
            {
                string word = r.QuotedReadToDelimiter(new[] { ',', '<', ':' });
                // We processed all data.
                if (word == null && r.Available == 0)
                {
                    break;
                }
                // name-addr
                else if (r.Peek(true) == '<')
                {
                    retVal.m_pAddresses.Add(
                        new Mail_t_Mailbox(
                            word != null
                                ? MIME_Encoding_EncodedWord.DecodeAll(TextUtils.UnQuoteString(word))
                                : null,
                            r.ReadParenthesized()));
                }
                // addr-spec
                else
                {
                    retVal.m_pAddresses.Add(new Mail_t_Mailbox(null, word));
                }

                // We have more addresses.
                if (r.Peek(true) == ',')
                {
                    r.Char(false);
                }
            }


            retVal.m_ParseValue = value;
            retVal.m_pAddresses.AcceptChanges();
            return retVal;
        }

        

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (IsModified)
            {
                StringBuilder retVal = new StringBuilder();
                retVal.Append(Name + ": ");
                for (int i = 0; i < m_pAddresses.Count; i++)
                {
                    if (i > 0)
                    {
                        retVal.Append("\t");
                    }

                    // Don't add ',' for last item.
                    if (i == (m_pAddresses.Count - 1))
                    {
                        retVal.Append(m_pAddresses[i].ToString(wordEncoder) + "\r\n");
                    }
                    else
                    {
                        retVal.Append(m_pAddresses[i].ToString(wordEncoder) + ",\r\n");
                    }
                }

                return retVal.ToString();
            }
            else
            {
                return m_ParseValue;
            }
        }

        #endregion
    }
}