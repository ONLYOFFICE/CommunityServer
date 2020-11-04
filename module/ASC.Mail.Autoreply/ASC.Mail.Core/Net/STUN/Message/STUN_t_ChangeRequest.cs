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
    /// This class implements STUN CHANGE-REQUEST attribute. Defined in RFC 3489 11.2.4.
    /// </summary>
    public class STUN_t_ChangeRequest
    {
        #region Members

        private bool m_ChangeIP = true;
        private bool m_ChangePort = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public STUN_t_ChangeRequest() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="changeIP">Specifies if STUN server must send response to different IP than request was received.</param>
        /// <param name="changePort">Specifies if STUN server must send response to different port than request was received.</param>
        public STUN_t_ChangeRequest(bool changeIP, bool changePort)
        {
            m_ChangeIP = changeIP;
            m_ChangePort = changePort;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if STUN server must send response to different IP than request was received.
        /// </summary>
        public bool ChangeIP
        {
            get { return m_ChangeIP; }

            set { m_ChangeIP = value; }
        }

        /// <summary>
        /// Gets or sets if STUN server must send response to different port than request was received.
        /// </summary>
        public bool ChangePort
        {
            get { return m_ChangePort; }

            set { m_ChangePort = value; }
        }

        #endregion
    }
}