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
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "language-tag" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     language-tag = primary-tag *( "-" subtag )
    ///     primary-tag  = 1*8ALPHA
    ///     subtag       = 1*8ALPHA
    /// </code>
    /// </remarks>
    public class SIP_t_LanguageTag : SIP_t_ValueWithParams
    {
        #region Members

        private string m_LanguageTag = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets language tag.
        /// </summary>
        public string LanguageTag
        {
            get { return m_LanguageTag; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property LanguageTag value can't be null or empty !");
                }

                m_LanguageTag = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "language-tag" from specified value.
        /// </summary>
        /// <param name="value">SIP "language-tag" value.</param>
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
        /// Parses "language-tag" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /* 
                Content-Language =  "Content-Language" HCOLON language-tag *(COMMA language-tag)
                language-tag     =  primary-tag *( "-" subtag )
                primary-tag      =  1*8ALPHA
                subtag           =  1*8ALPHA
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse content-coding
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid Content-Language value, language-tag value is missing !");
            }
            m_LanguageTag = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "language-tag" value.
        /// </summary>
        /// <returns>Returns "language-tag" value.</returns>
        public override string ToStringValue()
        {
            /* 
                Content-Language =  "Content-Language" HCOLON language-tag *(COMMA language-tag)
                language-tag     =  primary-tag *( "-" subtag )
                primary-tag      =  1*8ALPHA
                subtag           =  1*8ALPHA
            */

            StringBuilder retVal = new StringBuilder();
            retVal.Append(m_LanguageTag);
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}