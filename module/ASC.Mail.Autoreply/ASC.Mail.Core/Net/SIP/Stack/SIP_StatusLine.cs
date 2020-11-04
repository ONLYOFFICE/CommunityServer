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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SIP Status-Line. Defined in RFC 3261.
    /// </summary>
    public class SIP_StatusLine
    {
        #region Members

        private string m_Reason = "";
        private int m_StatusCode;
        private string m_Version = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="reason">Reason text.</param>
        /// <exception cref="ArgumentException">Is raised when </exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>reason</b> is null reference.</exception>
        public SIP_StatusLine(int statusCode, string reason)
        {
            if (statusCode < 100 || statusCode > 699)
            {
                throw new ArgumentException("Argument 'statusCode' value must be >= 100 and <= 699.");
            }
            if (reason == null)
            {
                throw new ArgumentNullException("reason");
            }

            m_Version = "SIP/2.0";
            m_StatusCode = statusCode;
            m_Reason = reason;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets reason phrase.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public string Reason
        {
            get { return m_Reason; }

            set
            {
                if (Reason == null)
                {
                    throw new ArgumentNullException("Reason");
                }

                m_Reason = value;
            }
        }

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid value.</exception>
        public int StatusCode
        {
            get { return m_StatusCode; }

            set
            {
                if (value < 100 || value > 699)
                {
                    throw new ArgumentException("Argument 'statusCode' value must be >= 100 and <= 699.");
                }

                m_StatusCode = value;
            }
        }

        /// <summary>
        /// Gets or sets SIP version.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid value.</exception>
        public string Version
        {
            get { return m_Version; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Version");
                }
                if (value == "")
                {
                    throw new ArgumentException("Property 'Version' value must be specified.");
                }

                m_Version = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns Status-Line string.
        /// </summary>
        /// <returns>Returns Status-Line string.</returns>
        public override string ToString()
        {
            // RFC 3261 25. 
            //  Status-Line = SIP-Version SP Status-Code SP Reason-Phrase CRLF

            return m_Version + " " + m_StatusCode + " " + m_Reason + "\r\n";
        }

        #endregion
    }
}