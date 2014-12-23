/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
    /// Represents "Return-Path:" header. Defined in RFC 5322 3.6.7.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 5322 3.6.7.
    ///     return     = "Return-Path:" path CRLF
    ///     path       = angle-addr / ([CFWS] "&lt;" [CFWS] "&gt;" [CFWS])
    ///     angle-addr = [CFWS] "&lt;" addr-spec "&gt;" [CFWS]
    /// </code>
    /// </example>
    public class Mail_h_ReturnPath : MIME_h
    {
        #region Members

        private string m_Address;
        private bool m_IsModified;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified
        {
            get { return m_IsModified; }
        }

        /// <summary>
        /// Gets header field name. For example "Sender".
        /// </summary>
        public override string Name
        {
            get { return "Return-Path"; }
        }

        /// <summary>
        /// Gets mailbox address. Value null means null-path.
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
        /// <param name="address">Address. Value null means null-path.</param>
        public Mail_h_ReturnPath(string address)
        {
            m_Address = address;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Return-Path: &lt;jhon.doe@domain.com&gt;'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static Mail_h_ReturnPath Parse(string value)
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

            Mail_h_ReturnPath retVal = new Mail_h_ReturnPath(null);

            MIME_Reader r = new MIME_Reader(name_value[1]);
            r.ToFirstChar();
            // Return-Path missing <>, some server won't be honor RFC.
            if (!r.StartsWith("<"))
            {
                retVal.m_Address = r.ToEnd();
            }
            else
            {
                retVal.m_Address = r.ReadParenthesized();
            }

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
            if (string.IsNullOrEmpty(m_Address))
            {
                return "Return-Path: <>\r\n";
            }
            else
            {
                return "Return-Path: <" + m_Address + ">\r\n";
            }
        }

        #endregion
    }
}