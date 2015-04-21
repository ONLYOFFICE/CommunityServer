/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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