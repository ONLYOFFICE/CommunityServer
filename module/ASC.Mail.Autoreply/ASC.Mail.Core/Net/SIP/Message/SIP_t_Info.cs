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
    /// Implements SIP "info" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     info       = LAQUOT absoluteURI RAQUOT *( SEMI info-param)
    ///     info-param = ( "purpose" EQUAL ( "icon" / "info" / "card" / token ) ) / generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_Info : SIP_t_ValueWithParams
    {
        #region Members

        private string m_Uri = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets 'purpose' parameter value. Value null means not specified. 
        /// Known values: "icon","info","card".
        /// </summary>
        public string Purpose
        {
            get
            {
                SIP_Parameter parameter = Parameters["purpose"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Parameters.Remove("purpose");
                }
                else
                {
                    Parameters.Set("purpose", value);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "info" from specified value.
        /// </summary>
        /// <param name="value">SIP "info" value.</param>
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
        /// Parses "info" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Call-Info  = "Call-Info" HCOLON info *(COMMA info)
                info       = LAQUOT absoluteURI RAQUOT *( SEMI info-param)
                info-param = ( "purpose" EQUAL ( "icon" / "info" / "card" / token ) ) / generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse uri
            // Read to LAQUOT
            reader.QuotedReadToDelimiter('<');
            if (!reader.StartsWith("<"))
            {
                throw new SIP_ParseException("Invalid Alert-Info value, Uri not between <> !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "info" value.
        /// </summary>
        /// <returns>Returns "info" value.</returns>
        public override string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append("<" + m_Uri + ">");
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}