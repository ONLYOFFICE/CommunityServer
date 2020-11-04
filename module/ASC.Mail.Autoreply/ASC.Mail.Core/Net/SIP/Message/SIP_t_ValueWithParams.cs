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

    using System.Text;

    #endregion

    /// <summary>
    /// This base class for SIP data types what has parameters support.
    /// </summary>
    public abstract class SIP_t_ValueWithParams : SIP_t_Value
    {
        #region Members

        private readonly SIP_ParameterCollection m_pParameters;

        #endregion

        #region Properties

        /// <summary>
        /// Gets via parameters.
        /// </summary>
        public SIP_ParameterCollection Parameters
        {
            get { return m_pParameters; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ValueWithParams()
        {
            m_pParameters = new SIP_ParameterCollection();
        }

        #endregion

        /// <summary>
        /// Parses parameters from specified reader. Reader position must be where parameters begin.
        /// </summary>
        /// <param name="reader">Reader from where to read parameters.</param>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        protected void ParseParameters(StringReader reader)
        {
            // Remove all old parameters.
            m_pParameters.Clear();

            // Parse parameters
            while (reader.Available > 0)
            {
                reader.ReadToFirstChar();

                // We have parameter
                if (reader.SourceString.StartsWith(";"))
                {
                    reader.ReadSpecifiedLength(1);
                    string paramString = reader.QuotedReadToDelimiter(new[] {';', ','}, false);
                    if (paramString != "")
                    {
                        string[] name_value = paramString.Split(new[] {'='}, 2);
                        if (name_value.Length == 2)
                        {
                            Parameters.Add(name_value[0], name_value[1]);
                        }
                        else
                        {
                            Parameters.Add(name_value[0], null);
                        }
                    }
                }
                    // Next value
                else if (reader.SourceString.StartsWith(","))
                {
                    break;
                }
                    // Unknown data
                else
                {
                    throw new SIP_ParseException("Unexpected value '" + reader.SourceString + "' !");
                }
            }
        }

        /// <summary>
        /// Convert parameters to valid parameters string.
        /// </summary>
        /// <returns>Returns parameters string.</returns>
        protected string ParametersToString()
        {
            StringBuilder retVal = new StringBuilder();
            foreach (SIP_Parameter parameter in m_pParameters)
            {
                if (!string.IsNullOrEmpty(parameter.Value))
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
    }
}