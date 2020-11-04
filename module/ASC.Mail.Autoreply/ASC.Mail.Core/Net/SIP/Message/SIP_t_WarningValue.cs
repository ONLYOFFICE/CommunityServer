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
    /// Implements SIP "warning-value" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     warning-value = warn-code SP warn-agent SP warn-text
    ///     warn-code     = 3DIGIT
    ///     warn-agent    = hostport / pseudonym
    ///                      ;  the name or pseudonym of the server adding
    ///                      ;  the Warning header, for use in debugging
    ///     warn-text     = quoted-string
    ///     pseudonym     = token
    /// </code>
    /// </remarks>
    public class SIP_t_WarningValue : SIP_t_Value
    {
        #region Members

        private string m_Agent = "";
        private int m_Code;
        private string m_Text = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets warning code.
        /// </summary>
        public int Code
        {
            get { return m_Code; }

            set
            {
                if (value < 100 || value > 999)
                {
                    throw new ArgumentException("Property Code value must be 3 digit !");
                }

                m_Code = value;
            }
        }

        /// <summary>
        /// Gets or sets name or pseudonym of the server.
        /// </summary>
        public string Agent
        {
            get { return m_Agent; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Agent value may not be null or empty !");
                }

                m_Agent = value;
            }
        }

        /// <summary>
        /// Gets or sets warning text.
        /// </summary>
        public string Text
        {
            get { return m_Text; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Text value may not be null or empty !");
                }

                m_Text = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "warning-value" from specified value.
        /// </summary>
        /// <param name="value">SIP "warning-value" value.</param>
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
        /// Parses "warning-value" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*                
                warning-value  =  warn-code SP warn-agent SP warn-text
                warn-code      =  3DIGIT
                warn-agent     =  hostport / pseudonym
                                  ;  the name or pseudonym of the server adding
                                  ;  the Warning header, for use in debugging
                warn-text      =  quoted-string
                pseudonym      =  token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'warning-value' value, warn-code is missing !");
            }
            try
            {
                Code = Convert.ToInt32(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid 'warning-value' warn-code value, warn-code is missing !");
            }

            word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'warning-value' value, warn-agent is missing !");
            }
            Agent = word;

            word = reader.ReadToEnd();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'warning-value' value, warn-text is missing !");
            }
            Agent = TextUtils.UnQuoteString(word);
        }

        /// <summary>
        /// Converts this to valid "warning-value" value.
        /// </summary>
        /// <returns>Returns "warning-value" value.</returns>
        public override string ToStringValue()
        {
            return m_Code + " " + m_Agent + " " + TextUtils.QuoteString(m_Text);
        }

        #endregion
    }
}