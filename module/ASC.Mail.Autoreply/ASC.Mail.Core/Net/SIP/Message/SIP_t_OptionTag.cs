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
    /// Implements SIP "option-tag" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     option-tag = token
    /// </code>
    /// </remarks>
    public class SIP_t_OptionTag : SIP_t_Value
    {
        #region Members

        private string m_OptionTag = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets option tag.
        /// </summary>
        public string OptionTag
        {
            get { return m_OptionTag; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("property OptionTag value cant be null or empty !");
                }

                m_OptionTag = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "option-tag" from specified value.
        /// </summary>
        /// <param name="value">SIP "option-tag" value.</param>
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
        /// Parses "option-tag" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            // option-tag = token

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get Method
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new ArgumentException("Invalid 'option-tag' value, value is missing !");
            }
            m_OptionTag = word;
        }

        /// <summary>
        /// Converts this to valid "option-tag" value.
        /// </summary>
        /// <returns>Returns "option-tag" value.</returns>
        public override string ToStringValue()
        {
            return m_OptionTag;
        }

        #endregion
    }
}