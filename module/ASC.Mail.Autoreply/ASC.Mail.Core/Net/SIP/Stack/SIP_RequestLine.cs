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
    /// Implements SIP Request-Line. Defined in RFC 3261.
    /// </summary>
    public class SIP_RequestLine
    {
        #region Members

        private string m_Method = "";
        private AbsoluteUri m_pUri;
        private string m_Version = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="method">SIP method.</param>
        /// <param name="uri">Request URI.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>method</b> or <b>uri</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_RequestLine(string method, AbsoluteUri uri)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!SIP_Utils.IsToken(method))
            {
                throw new ArgumentException("Argument 'method' value must be token.");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            m_Method = method.ToUpper();
            m_pUri = uri;
            m_Version = "SIP/2.0";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets request method. This value is always in upper-case.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid value.</exception>
        public string Method
        {
            get { return m_Method; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Method");
                }
                if (!SIP_Utils.IsToken(value))
                {
                    throw new ArgumentException("Property 'Method' value must be token.");
                }

                m_Method = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets request URI.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public AbsoluteUri Uri
        {
            get { return m_pUri; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Uri");
                }

                m_pUri = value;
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
        /// Returns Request-Line string.
        /// </summary>
        /// <returns>Returns Request-Line string.</returns>
        public override string ToString()
        {
            // RFC 3261 25. 
            //  Request-Line = Method SP Request-URI SP SIP-Version CRLF

            return m_Method + " " + m_pUri + " " + m_Version + "\r\n";
        }

        #endregion
    }
}