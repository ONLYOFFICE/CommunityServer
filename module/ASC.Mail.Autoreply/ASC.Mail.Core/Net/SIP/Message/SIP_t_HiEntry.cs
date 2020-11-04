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
    /// Implements SIP "hi-entry" value. Defined in RFC 4244.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4244 Syntax:
    ///     hi-entry = hi-targeted-to-uri *( SEMI hi-param )
    ///     hi-targeted-to-uri= name-addr
    ///     hi-param = hi-index / hi-extension
    ///     hi-index = "index" EQUAL 1*DIGIT *(DOT 1*DIGIT)
    ///     hi-extension = generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_HiEntry : SIP_t_ValueWithParams
    {
        #region Members

        private SIP_t_NameAddress m_pAddress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets address.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public SIP_t_NameAddress Address
        {
            get { return m_pAddress; }

            set
            {
                if (m_pAddress == null)
                {
                    throw new ArgumentNullException("m_pAddress");
                }

                m_pAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets 'index' parameter value. Value -1 means not specified.
        /// </summary>
        public double Index
        {
            get
            {
                SIP_Parameter parameter = Parameters["index"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("index");
                }
                else
                {
                    Parameters.Set("index", value.ToString());
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "hi-entry" from specified value.
        /// </summary>
        /// <param name="value">SIP "hi-entry" value.</param>
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
        /// Parses "hi-entry" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                hi-entry = hi-targeted-to-uri *( SEMI hi-param )
                hi-targeted-to-uri= name-addr
                hi-param = hi-index / hi-extension
                hi-index = "index" EQUAL 1*DIGIT *(DOT 1*DIGIT)
                hi-extension = generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // name-addr
            m_pAddress = new SIP_t_NameAddress();
            m_pAddress.Parse(reader);

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "hi-entry" value.
        /// </summary>
        /// <returns>Returns "hi-entry" value.</returns>
        public override string ToStringValue()
        {
            /*
                hi-entry = hi-targeted-to-uri *( SEMI hi-param )
                hi-targeted-to-uri= name-addr
                hi-param = hi-index / hi-extension
                hi-index = "index" EQUAL 1*DIGIT *(DOT 1*DIGIT)
                hi-extension = generic-param
            */

            StringBuilder retVal = new StringBuilder();

            // name-addr
            retVal.Append(m_pAddress.ToStringValue());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}