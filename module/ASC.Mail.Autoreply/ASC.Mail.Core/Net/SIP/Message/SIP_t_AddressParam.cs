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
    /// Implements SIP_t_NameAddress + parameters value.
    /// </summary>
    public class SIP_t_AddressParam : SIP_t_ValueWithParams
    {
        #region Members

        private SIP_t_NameAddress m_pAddress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets address.
        /// </summary>
        public SIP_t_NameAddress Address
        {
            get { return m_pAddress; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_AddressParam() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP_t_NameAddress + parameters value.</param>
        public SIP_t_AddressParam(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses this from specified value.
        /// </summary>
        /// <param name="value">Address + params value.</param>
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
        /// Parses this from address param string.
        /// </summary>
        /// <param name="reader">Reader what contains address param string.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse address
            SIP_t_NameAddress address = new SIP_t_NameAddress();
            address.Parse(reader);
            m_pAddress = address;

            // Parse parameters.
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid value string.
        /// </summary>
        /// <returns></returns>
        public override string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();

            // Add address
            retVal.Append(m_pAddress.ToStringValue());

            // Add parameters
            foreach (SIP_Parameter parameter in Parameters)
            {
                if (parameter.Value != null)
                {
                    retVal.Append(";" + parameter.Name + "=" + parameter.Value);
                }
                else
                {
                    retVal.Append(";" + parameter.Name);
                }
            }

            return retVal.ToString();
        }

        #endregion
    }
}