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
    /// Implements SIP "challenge" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     challenge = ("Digest" LWS digest-cln *(COMMA digest-cln)) / other-challenge
    /// </code>
    /// </remarks>
    public class SIP_t_Challenge : SIP_t_Value
    {
        #region Members

        private string m_AuthData = "";
        private string m_Method = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets authentication method. Normally this value is always 'Digest'.
        /// </summary>
        public string Method
        {
            get { return m_Method; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Method value cant be null or mepty !");
                }

                m_Method = value;
            }
        }

        /// <summary>
        /// Gets or sets authentication data. That value depends on authentication type.
        /// </summary>
        public string AuthData
        {
            get { return m_AuthData; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property AuthData value cant be null or mepty !");
                }

                m_AuthData = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP challenge value.</param>
        public SIP_t_Challenge(string value)
        {
            Parse(new StringReader(value));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "challenge" from specified value.
        /// </summary>
        /// <param name="value">SIP "challenge" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "challenge" from specified reader.
        /// </summary>
        /// <param name="reader">Reader what contains challenge value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            // challenge = ("Digest" LWS digest-cln *(COMMA digest-cln)) / other-challenge

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get authentication method
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException(
                    "Invalid WWW-Authenticate: value, authentication method is missing !");
            }
            m_Method = word;

            // Get authentication data
            word = reader.ReadToEnd();
            if (word == null)
            {
                throw new SIP_ParseException(
                    "Invalid WWW-Authenticate: value, authentication parameters are missing !");
            }
            m_AuthData = word.Trim();
        }

        /// <summary>
        /// Converts this to valid "challenge" value.
        /// </summary>
        /// <returns>Returns "challenge" value.</returns>
        public override string ToStringValue()
        {
            return m_Method + " " + m_AuthData;
        }

        #endregion
    }
}