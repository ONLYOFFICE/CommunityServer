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


namespace ASC.Mail.Net.IMAP.Client
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// IMAP client exception.
    /// </summary>
    public class IMAP_ClientException : Exception
    {
        #region Members

        private readonly string m_ResponseText = "";
        private readonly string m_StatusCode = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets IMAP server error status code.
        /// </summary>
        public string StatusCode
        {
            get { return m_StatusCode; }
        }

        /// <summary>
        /// Gets IMAP server response text after status code.
        /// </summary>
        public string ResponseText
        {
            get { return m_ResponseText; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseLine">IMAP server response line.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseLine</b> is null.</exception>
        public IMAP_ClientException(string responseLine) : base(responseLine)
        {
            if (responseLine == null)
            {
                throw new ArgumentNullException("responseLine");
            }

            // <status-code> SP <response-text>
            string[] code_text = responseLine.Split(new char[] {}, 2);
            m_StatusCode = code_text[0];
            if (code_text.Length == 2)
            {
                m_ResponseText = code_text[1];
            }
        }

        #endregion
    }
}