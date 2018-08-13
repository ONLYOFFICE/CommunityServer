/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


namespace ASC.Mail.Net.AUTH
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements "DIGEST-MD5" authenticaiton. Defined in RFC 2831.
    /// </summary>
    public class AUTH_SASL_ServerMechanism_DigestMd5 : AUTH_SASL_ServerMechanism
    {
        #region Events

        /// <summary>
        /// Is called when authentication mechanism needs to get user info to complete atuhentication.
        /// </summary>
        public event EventHandler<AUTH_e_UserInfo> GetUserInfo = null;

        #endregion

        #region Members

        private readonly string m_Nonce = "";

        private readonly bool m_RequireSSL;
        private bool m_IsAuthenticated;
        private bool m_IsCompleted;
        private string m_Realm = "";
        private int m_State;
        private string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if the authentication exchange has completed.
        /// </summary>
        public override bool IsCompleted
        {
            get { return m_IsCompleted; }
        }

        /// <summary>
        /// Gets if user has authenticated sucessfully.
        /// </summary>
        public override bool IsAuthenticated
        {
            get { return m_IsAuthenticated; }
        }

        /// <summary>
        /// Returns always "DIGEST-MD5".
        /// </summary>
        public override string Name
        {
            get { return "DIGEST-MD5"; }
        }

        /// <summary>
        /// Gets if specified SASL mechanism is available only to SSL connection.
        /// </summary>
        public override bool RequireSSL
        {
            get { return m_RequireSSL; }
        }

        /// <summary>
        /// Gets or sets realm value.
        /// </summary>
        /// <remarks>Normally this is host or domain name.</remarks>
        public string Realm
        {
            get { return m_Realm; }

            set
            {
                if (value == null)
                {
                    value = "";
                }
                m_Realm = value;
            }
        }

        /// <summary>
        /// Gets user login name.
        /// </summary>
        public override string UserName
        {
            get { return m_UserName; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="requireSSL">Specifies if this mechanism is available to SSL connections only.</param>
        public AUTH_SASL_ServerMechanism_DigestMd5(bool requireSSL)
        {
            m_RequireSSL = requireSSL;

            m_Nonce = Auth_HttpDigest.CreateNonce();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Continues authentication process.
        /// </summary>
        /// <param name="clientResponse">Client sent SASL response.</param>
        /// <returns>Retunrns challange response what must be sent to client or null if authentication has completed.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>clientResponse</b> is null reference.</exception>
        public override string Continue(string clientResponse)
        {
            if (clientResponse == null)
            {
                throw new ArgumentNullException("clientResponse");
            }

            /* RFC 2831.
                The base64-decoded version of the SASL exchange is:

                S: realm="elwood.innosoft.com",nonce="OA6MG9tEQGm2hh",qop="auth",
                   algorithm=md5-sess,charset=utf-8
                C: charset=utf-8,username="chris",realm="elwood.innosoft.com",
                   nonce="OA6MG9tEQGm2hh",nc=00000001,cnonce="OA6MHXh6VqTrRk",
                   digest-uri="imap/elwood.innosoft.com",
                   response=d388dad90d4bbd760a152321f2143af7,qop=auth
                S: rspauth=ea40f60335c427b5527b84dbabcdfffd
                C: 
                S: ok

                The password in this example was "secret".
            */

            if (m_State == 0)
            {
                m_State++;

                return "realm=\"" + m_Realm + "\",nonce=\"" + m_Nonce +
                       "\",qop=\"auth\",algorithm=md5-sess,charset=utf-8";
            }
            else if (m_State == 1)
            {
                m_State++;

                Auth_HttpDigest auth = new Auth_HttpDigest(clientResponse, "AUTHENTICATE");
                auth.Qop = "auth";
                auth.Algorithm = "md5-sess";

                // Check realm and nonce value.
                if (m_Realm != auth.Realm || m_Nonce != auth.Nonce)
                {
                    return "rspauth=\"\"";
                }

                m_UserName = auth.UserName;
                AUTH_e_UserInfo result = OnGetUserInfo(auth.UserName);
                if (result.UserExists)
                {
                    if (auth.Authenticate(result.UserName, result.Password))
                    {
                        m_IsAuthenticated = true;

                        return "rspauth=" + auth.CalculateRspAuth(result.UserName, result.Password);
                    }
                }

                return "rspauth=\"\"";
            }
            else
            {
                m_IsCompleted = true;
            }

            return null;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>GetUserInfo</b> event.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Returns specified user info.</returns>
        private AUTH_e_UserInfo OnGetUserInfo(string userName)
        {
            AUTH_e_UserInfo retVal = new AUTH_e_UserInfo(userName);

            if (GetUserInfo != null)
            {
                GetUserInfo(this, retVal);
            }

            return retVal;
        }

        #endregion
    }
}