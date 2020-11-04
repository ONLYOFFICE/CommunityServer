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


namespace ASC.Mail.Net.FTP.Client
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// FTP client exception.
    /// </summary>
    public class FTP_ClientException : Exception
    {
        #region Members

        private readonly string m_ResponseText = "";
        private readonly int m_StatusCode = 500;

        #endregion

        #region Properties

        /// <summary>
        /// Gets FTP status code.
        /// </summary>
        public int StatusCode
        {
            get { return m_StatusCode; }
        }

        /// <summary>
        /// Gets FTP server response text after status code.
        /// </summary>
        public string ResponseText
        {
            get { return m_ResponseText; }
        }

        /// <summary>
        /// Gets if it is permanent FTP(5xx) error.
        /// </summary>
        public bool IsPermanentError
        {
            get
            {
                if (m_StatusCode >= 500 && m_StatusCode <= 599)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseLine">FTP server response line.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseLine</b> is null.</exception>
        public FTP_ClientException(string responseLine) : base(responseLine)
        {
            if (responseLine == null)
            {
                throw new ArgumentNullException("responseLine");
            }

            string[] code_text = responseLine.Split(new[] {' '}, 2);
            try
            {
                m_StatusCode = Convert.ToInt32(code_text[0]);
            }
            catch {}
            if (code_text.Length == 2)
            {
                m_ResponseText = code_text[1];
            }
        }

        #endregion
    }
}