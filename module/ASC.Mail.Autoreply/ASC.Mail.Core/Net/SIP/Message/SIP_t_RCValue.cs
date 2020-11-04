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
    /// Implements SIP "rc-value" value. Defined in RFC 3841.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3841 Syntax:
    ///     rc-value  =  "*" *(SEMI rc-params)
    ///     rc-params =  feature-param / generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_RCValue : SIP_t_ValueWithParams
    {
        #region Methods

        /// <summary>
        /// Parses "rc-value" from specified value.
        /// </summary>
        /// <param name="value">SIP "rc-value" value.</param>
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
        /// Parses "rc-value" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                rc-value  =  "*" *(SEMI rc-params)
                rc-params =  feature-param / generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'rc-value', '*' is missing !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "rc-value" value.
        /// </summary>
        /// <returns>Returns "rc-value" value.</returns>
        public override string ToStringValue()
        {
            /*
                rc-value  =  "*" *(SEMI rc-params)
                rc-params =  feature-param / generic-param
            */

            StringBuilder retVal = new StringBuilder();

            // *
            retVal.Append("*");

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}