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
    /// Implements SIP "accept-range" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     accept-range  = media-range [ accept-params ] 
    ///     media-range   = ("*//*" / (m-type SLASH "*") / (m-type SLASH m-subtype)) *(SEMI m-parameter)
    ///     accept-params = SEMI "q" EQUAL qvalue *(SEMI generic-param)
    /// </code>
    /// </remarks>
    public class SIP_t_AcceptRange : SIP_t_Value
    {
        #region Members

        private readonly SIP_ParameterCollection m_pMediaParameters;
        private readonly SIP_ParameterCollection m_pParameters;
        private string m_MediaType = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets media type. Value *(STAR) means all values. Syntax: mediaType / mediaSubType.
        /// Examples: */*,video/*,text/html.
        /// </summary>
        public string MediaType
        {
            get { return m_MediaType; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property MediaType value can't be null or empty !");
                }
                if (value.IndexOf('/') == -1)
                {
                    throw new ArgumentException(
                        "Invalid roperty MediaType value, syntax: mediaType / mediaSubType !");
                }

                m_MediaType = value;
            }
        }

        /// <summary>
        /// Gets media parameters collection.
        /// </summary>
        /// <returns></returns>
        public SIP_ParameterCollection MediaParameters
        {
            get { return m_pMediaParameters; }
        }

        /// <summary>
        /// Gets accept value parameters.
        /// </summary>
        public SIP_ParameterCollection Parameters
        {
            get { return m_pParameters; }
        }

        /// <summary>
        /// Gets or sets qvalue parameter. Targets are processed from highest qvalue to lowest. 
        /// This value must be between 0.0 and 1.0. Value -1 means that value not specified.
        /// </summary>
        public double QValue
        {
            get
            {
                SIP_Parameter parameter = Parameters["qvalue"];
                if (parameter != null)
                {
                    return Convert.ToDouble(parameter.Value);
                }
                else
                {
                    return -1;
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

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_AcceptRange()
        {
            m_pMediaParameters = new SIP_ParameterCollection();
            m_pParameters = new SIP_ParameterCollection();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "accept-range" from specified value.
        /// </summary>
        /// <param name="value">SIP "accept-range" value.</param>
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
        /// Parses "accept-range" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                accept-range  = media-range [ accept-params ] 
                media-range   = ("*/
            /*" / (m-type SLASH "*") / (m-type SLASH m-subtype)) *(SEMI m-parameter)
                accept-params = SEMI "q" EQUAL qvalue *(SEMI generic-param)
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse m-type
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'accept-range' value, m-type is missing !");
            }
            MediaType = word;

            // Parse media and accept parameters !!! thats confusing part, RFC invalid.
            bool media_accept = true;
            while (reader.Available > 0)
            {
                reader.ReadToFirstChar();

                // We have 'next' value, so we are done here.
                if (reader.SourceString.StartsWith(","))
                {
                    break;
                }
                    // We have parameter
                else if (reader.SourceString.StartsWith(";"))
                {
                    reader.ReadSpecifiedLength(1);
                    string paramString = reader.QuotedReadToDelimiter(new[] {';', ','}, false);
                    if (paramString != "")
                    {
                        string[] name_value = paramString.Split(new[] {'='}, 2);
                        string name = name_value[0].Trim();
                        string value = "";
                        if (name_value.Length == 2)
                        {
                            value = name_value[1];
                        }

                        // If q, then accept parameters begin
                        if (name.ToLower() == "q")
                        {
                            media_accept = false;
                        }

                        if (media_accept)
                        {
                            MediaParameters.Add(name, value);
                        }
                        else
                        {
                            Parameters.Add(name, value);
                        }
                    }
                }
                    // Unknown data
                else
                {
                    throw new SIP_ParseException("SIP_t_AcceptRange unexpected prarameter value !");
                }
            }
        }

        /// <summary>
        /// Converts this to valid "accept-range" value.
        /// </summary>
        /// <returns>Returns "accept-range" value.</returns>
        public override string ToStringValue()
        {
            /*
                Accept        = "Accept" HCOLON [ accept-range *(COMMA accept-range) ]
                accept-range  = media-range [ accept-params ] 
                media-range   = ("*/
            /*" / (m-type SLASH "*") / (m-type SLASH m-subtype)) *(SEMI m-parameter)
                accept-params = SEMI "q" EQUAL qvalue *(SEMI generic-param)
            */

            StringBuilder retVal = new StringBuilder();
            retVal.Append(m_MediaType);
            foreach (SIP_Parameter parameter in m_pMediaParameters)
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
            foreach (SIP_Parameter parameter in m_pParameters)
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