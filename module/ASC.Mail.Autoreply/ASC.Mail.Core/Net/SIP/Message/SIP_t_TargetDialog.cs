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
    /// Implements SIP "Target-Dialog" value. Defined in RFC 4538.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4538 Syntax:
    ///     Target-Dialog = callid *(SEMI td-param)    ;callid from RFC 3261
    ///     td-param      = remote-param / local-param / generic-param
    ///     remote-param  = "remote-tag" EQUAL token
    ///     local-param   = "local-tag" EQUAL token
    /// </code>
    /// </remarks>
    public class SIP_t_TargetDialog : SIP_t_ValueWithParams
    {
        #region Members

        private string m_CallID = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets call ID.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid CallID value is passed.</exception>
        public string CallID
        {
            get { return m_CallID; }

            set
            {
                if (m_CallID == null)
                {
                    throw new ArgumentNullException("CallID");
                }
                if (m_CallID == "")
                {
                    throw new ArgumentException("Property 'CallID' may not be '' !");
                }

                m_CallID = value;
            }
        }

        /// <summary>
        /// Gets or sets 'remote-tag' parameter value. Value null means not specified.
        /// </summary>
        public string RemoteTag
        {
            get
            {
                SIP_Parameter parameter = Parameters["remote-tag"];
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
                    Parameters.Remove("remote-tag");
                }
                else
                {
                    Parameters.Set("remote-tag", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'local-tag' parameter value. Value null means not specified.
        /// </summary>
        public string LocalTag
        {
            get
            {
                SIP_Parameter parameter = Parameters["local-tag"];
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
                    Parameters.Remove("local-tag");
                }
                else
                {
                    Parameters.Set("local-tag", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP Target-Dialog value.</param>
        public SIP_t_TargetDialog(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Target-Dialog" from specified value.
        /// </summary>
        /// <param name="value">SIP "Target-Dialog" value.</param>
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
        /// Parses "Target-Dialog" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Target-Dialog = callid *(SEMI td-param)    ;callid from RFC 3261
                td-param      = remote-param / local-param / generic-param
                remote-param  = "remote-tag" EQUAL token
                local-param   = "local-tag" EQUAL token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // callid
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP Target-Dialog 'callid' value is missing !");
            }
            m_CallID = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Target-Dialog" value.
        /// </summary>
        /// <returns>Returns "Target-Dialog" value.</returns>
        public override string ToStringValue()
        {
            /*
                Target-Dialog = callid *(SEMI td-param)    ;callid from RFC 3261
                td-param      = remote-param / local-param / generic-param
                remote-param  = "remote-tag" EQUAL token
                local-param   = "local-tag" EQUAL token
            */

            StringBuilder retVal = new StringBuilder();

            // callid
            retVal.Append(m_CallID);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}