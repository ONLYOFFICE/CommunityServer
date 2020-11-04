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