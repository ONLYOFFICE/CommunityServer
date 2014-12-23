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
    /// Implements SIP "name-addr" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     name-addr = [ display-name ] LAQUOT addr-spec RAQUOT
    ///     addr-spec = SIP-URI / SIPS-URI / absoluteURI
    /// </code>
    /// </remarks>
    public class SIP_t_NameAddress
    {
        #region Members

        private string m_DisplayName = "";
        private AbsoluteUri m_pUri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string DisplayName
        {
            get { return m_DisplayName; }

            set
            {
                if (value == null)
                {
                    value = "";
                }

                m_DisplayName = value;
            }
        }

        /// <summary>
        /// Gets or sets URI. This can be SIP-URI / SIPS-URI / absoluteURI.
        /// Examples: sip:ivar@lumisoft.ee,sips:ivar@lumisoft.ee,mailto:ivar@lumisoft.ee, .... .
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference passed.</exception>
        public AbsoluteUri Uri
        {
            get { return m_pUri; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_pUri = value;
            }
        }

        /// <summary>
        /// Gets if current URI is sip or sips URI.
        /// </summary>
        public bool IsSipOrSipsUri
        {
            get { return IsSipUri || IsSecureSipUri; }
        }

        /// <summary>
        /// Gets if current URI is SIP uri.
        /// </summary>
        public bool IsSipUri
        {
            get
            {
                if (m_pUri.Scheme == UriSchemes.sip)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets if current URI is SIPS uri.
        /// </summary>
        public bool IsSecureSipUri
        {
            get
            {
                if (m_pUri.Scheme == UriSchemes.sips)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets if current URI is MAILTO uri.
        /// </summary>
        public bool IsMailToUri
        {
            get
            {
                if (m_pUri.Scheme == UriSchemes.mailto)
                {
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_NameAddress() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP <b>name-addr</b> value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public SIP_t_NameAddress(string value)
        {
            Parse(value);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="displayName">Display name.</param>
        /// <param name="uri">Uri.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>uri</b> is null reference.</exception>
        public SIP_t_NameAddress(string displayName, AbsoluteUri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            DisplayName = displayName;
            Uri = uri;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "name-addr" or "addr-spec" from specified value.
        /// </summary>
        /// <param name="value">SIP "name-addr" or "addr-spec" value.</param>
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
        /// Parses "name-addr" or "addr-spec" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(StringReader reader)
        {
            /* RFC 3261.
                name-addr =  [ display-name ] LAQUOT addr-spec RAQUOT
                addr-spec =  SIP-URI / SIPS-URI / absoluteURI
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            reader.ReadToFirstChar();

            // LAQUOT addr-spec RAQUOT
            if (reader.StartsWith("<"))
            {
                m_pUri = AbsoluteUri.Parse(reader.ReadParenthesized());
            }
            else
            {
                string word = reader.ReadWord();
                if (word == null)
                {
                    throw new SIP_ParseException("Invalid 'name-addr' or 'addr-spec' value !");
                }

                reader.ReadToFirstChar();

                // name-addr
                if (reader.StartsWith("<"))
                {
                    m_DisplayName = word;
                    m_pUri = AbsoluteUri.Parse(reader.ReadParenthesized());
                }
                    // addr-spec
                else
                {
                    m_pUri = AbsoluteUri.Parse(word);
                }
            }
        }

        /// <summary>
        /// Converts this to valid name-addr or addr-spec string as needed.
        /// </summary>
        /// <returns>Returns name-addr or addr-spec string.</returns>
        public string ToStringValue()
        {
            /* RFC 3261.
                name-addr =  [ display-name ] LAQUOT addr-spec RAQUOT
                addr-spec =  SIP-URI / SIPS-URI / absoluteURI
            */

            // addr-spec
            if (string.IsNullOrEmpty(m_DisplayName))
            {
                return "<" + m_pUri + ">";
            }
                // name-addr
            else
            {
                return TextUtils.QuoteString(m_DisplayName) + " <" + m_pUri + ">";
            }
        }

        #endregion
    }
}