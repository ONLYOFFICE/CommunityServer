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


namespace ASC.Mail.Net.POP3.Server
{
    public class AuthUser_EventArgsBase {
        protected AuthType m_AuthType;
        protected string m_Data = "";
        protected string m_PasswData = "";
        protected string m_UserName = "";
        private string m_ReturnData = "";
        private bool m_Validated = true;

        /// <summary>
        /// User name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Password data. eg. for AUTH=PLAIN it's password and for AUTH=APOP it's md5HexHash.
        /// </summary>
        public string PasswData
        {
            get { return m_PasswData; }
        }

        /// <summary>
        /// Authentication specific data(as tag).
        /// </summary>
        public string AuthData
        {
            get { return m_Data; }
        }

        /// <summary>
        /// Authentication type.
        /// </summary>
        public AuthType AuthType
        {
            get { return m_AuthType; }
        }

        /// <summary>
        /// Gets or sets if user is valid.
        /// </summary>
        public bool Validated
        {
            get { return m_Validated; }

            set { m_Validated = value; }
        }

        /// <summary>
        /// Gets or sets authentication data what must be returned for connected client.
        /// </summary>
        public string ReturnData
        {
            get { return m_ReturnData; }

            set { m_ReturnData = value; }
        }

        /// <summary>
        /// Gets or sets error text returned to connected client.
        /// </summary>
        public string ErrorText { get; set; }
    }

    /// <summary>
    /// Provides data for the AuthUser event for POP3_Server.
    /// </summary>
    public class AuthUser_EventArgs : AuthUser_EventArgsBase
    {
        #region Members

        private readonly POP3_Session m_pSession;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to pop3 session.
        /// </summary>
        public POP3_Session Session
        {
            get { return m_pSession; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to pop3 session.</param>
        /// <param name="userName">Username.</param>
        /// <param name="passwData">Password data.</param>
        /// <param name="data">Authentication specific data(as tag).</param>
        /// <param name="authType">Authentication type.</param>
        public AuthUser_EventArgs(POP3_Session session,
                                  string userName,
                                  string passwData,
                                  string data,
                                  AuthType authType)
        {
            m_pSession = session;
            m_UserName = userName;
            m_PasswData = passwData;
            m_Data = data;
            m_AuthType = authType;
        }

        #endregion
    }
}