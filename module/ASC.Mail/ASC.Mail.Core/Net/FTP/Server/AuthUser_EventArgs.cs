/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.FTP.Server
{
    /// <summary>
    /// Provides data for the AuthUser event for FTP_Server.
    /// </summary>
    public class AuthUser_EventArgs
    {
        #region Members

        private readonly AuthType m_AuthType;
        private readonly string m_Data = "";
        private readonly string m_PasswData = "";
        private readonly FTP_Session m_pSession;
        private readonly string m_UserName = "";
        private bool m_Validated = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to pop3 session.
        /// </summary>
        public FTP_Session Session
        {
            get { return m_pSession; }
        }

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
        public AuthUser_EventArgs(FTP_Session session,
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