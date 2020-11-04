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