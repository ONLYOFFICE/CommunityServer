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


namespace ASC.Mail.Net.STUN.Message
{
    /// <summary>
    /// This class implements STUN ERROR-CODE. Defined in RFC 3489 11.2.9.
    /// </summary>
    public class STUN_t_ErrorCode
    {
        #region Members

        private string m_ReasonText = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="reasonText">Reason text.</param>
        public STUN_t_ErrorCode(int code, string reasonText)
        {
            Code = code;
            m_ReasonText = reasonText;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets reason text.
        /// </summary>
        public string ReasonText
        {
            get { return m_ReasonText; }

            set { m_ReasonText = value; }
        }

        #endregion
    }
}