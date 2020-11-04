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