/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    using System.Globalization;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "contact-param" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     contact-param     = (name-addr / addr-spec) *(SEMI contact-params)
    ///     contact-params    = c-p-q / c-p-expires / contact-extension
    ///     c-p-q             = "q" EQUAL qvalue
    ///     c-p-expires       = "expires" EQUAL delta-seconds
    ///     contact-extension = generic-param
    ///     delta-seconds     = 1*DIGIT
    /// </code>
    /// </remarks>
    public class SIP_t_ContactParam : SIP_t_ValueWithParams
    {
        #region Members

        private SIP_t_NameAddress m_pAddress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets is this SIP contact is special STAR contact.
        /// </summary>
        public bool IsStarContact
        {
            get
            {
                if (m_pAddress.Uri.Value.StartsWith("*"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets contact address.
        /// </summary>
        public SIP_t_NameAddress Address
        {
            get { return m_pAddress; }
        }

        /// <summary>
        /// Gets or sets qvalue parameter. Targets are processed from highest qvalue to lowest. 
        /// This value must be between 0.0 and 1.0. Value -1 means that value not specified.
        /// </summary>
        public double QValue
        {
            get
            {
                if (!Parameters.Contains("qvalue"))
                {
                    return -1;
                }
                else
                {
                    return double.Parse(Parameters["qvalue"].Value, NumberStyles.Any);
                }
            }

            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentException("Property QValue value must be between 0.0 and 1.0 !");
                }

                if (value < 0)
                {
                    Parameters.Remove("qvalue");
                }
                else
                {
                    Parameters.Set("qvalue", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets expire parameter (time in seconds when contact expires). Value -1 means not specified.
        /// </summary>
        public int Expires
        {
            get
            {
                SIP_Parameter parameter = Parameters["expires"];
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
                if (value < 0)
                {
                    Parameters.Remove("expires");
                }
                else
                {
                    Parameters.Set("expires", value.ToString());
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ContactParam()
        {
            m_pAddress = new SIP_t_NameAddress();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "contact-param" from specified value.
        /// </summary>
        /// <param name="value">SIP "contact-param" value.</param>
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
        /// Parses "contact-param" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Contact        =  ("Contact" / "m" ) HCOLON
                                  ( STAR / (contact-param *(COMMA contact-param)))
                contact-param  =  (name-addr / addr-spec) *(SEMI contact-params)
                name-addr      =  [ display-name ] LAQUOT addr-spec RAQUOT
                addr-spec      =  SIP-URI / SIPS-URI / absoluteURI
                display-name   =  *(token LWS)/ quoted-string
                
                contact-params     =  c-p-q / c-p-expires / contact-extension
                c-p-q              =  "q" EQUAL qvalue
                c-p-expires        =  "expires" EQUAL delta-seconds
                contact-extension  =  generic-param
                delta-seconds      =  1*DIGIT
                
                When the header field value contains a display name, the URI including all URI 
                parameters is enclosed in "<" and ">".  If no "<" and ">" are present, all 
                parameters after the URI are header parameters, not URI parameters.
                
                Even if the "display-name" is empty, the "name-addr" form MUST be
                used if the "addr-spec" contains a comma, semicolon, or question
                mark. There may or may not be LWS between the display-name and the "<".            
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse address
            SIP_t_NameAddress address = new SIP_t_NameAddress();
            address.Parse(reader);
            m_pAddress = address;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "contact-param" value.
        /// </summary>
        /// <returns>Returns "contact-param" value.</returns>
        public override string ToStringValue()
        {
            /*
                Contact        =  ("Contact" / "m" ) HCOLON
                                  ( STAR / (contact-param *(COMMA contact-param)))
                contact-param  =  (name-addr / addr-spec) *(SEMI contact-params)
                name-addr      =  [ display-name ] LAQUOT addr-spec RAQUOT
                addr-spec      =  SIP-URI / SIPS-URI / absoluteURI
                display-name   =  *(token LWS)/ quoted-string
                
                contact-params     =  c-p-q / c-p-expires / contact-extension
                c-p-q              =  "q" EQUAL qvalue
                c-p-expires        =  "expires" EQUAL delta-seconds
                contact-extension  =  generic-param
                delta-seconds      =  1*DIGIT
            */

            StringBuilder retVal = new StringBuilder();

            // Add address
            retVal.Append(m_pAddress.ToStringValue());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}