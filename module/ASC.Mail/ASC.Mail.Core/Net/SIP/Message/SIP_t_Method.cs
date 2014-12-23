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

namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SIP "Method" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     Method           = INVITEm / ACKm / OPTIONSm / BYEm / CANCELm / REGISTERm / extension-method
    ///     extension-method = token
    /// </code>
    /// </remarks>
    public class SIP_t_Method : SIP_t_Value
    {
        #region Members

        private string m_Method = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets SIP method what is allowed.
        /// </summary>
        public string Method
        {
            get { return m_Method; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Method value can't be null or empty !");
                }
                if (TextUtils.IsToken(value))
                {
                    throw new ArgumentException("Property Method value must be 'token' !");
                }

                m_Method = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Method" from specified value.
        /// </summary>
        /// <param name="value">SIP "Method" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("reader");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "Method" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /* Allow            = "Allow" HCOLON [Method *(COMMA Method)]
               Method           = INVITEm / ACKm / OPTIONSm / BYEm / CANCELm / REGISTERm / extension-method
               extension-method = token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get Method
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'Method' value, value is missing !");
            }
            m_Method = word;
        }

        /// <summary>
        /// Converts this to valid "Method" value.
        /// </summary>
        /// <returns></returns>
        public override string ToStringValue()
        {
            return m_Method;
        }

        #endregion
    }
}