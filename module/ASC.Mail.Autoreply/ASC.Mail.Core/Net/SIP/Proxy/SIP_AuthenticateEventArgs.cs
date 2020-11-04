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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using AUTH;

    #endregion

    /// <summary>
    /// This class provides data for SIP_ProxyCore.Authenticate event.
    /// </summary>
    public class SIP_AuthenticateEventArgs
    {
        #region Members

        private readonly Auth_HttpDigest m_pAuth;

        #endregion

        #region Properties

        /// <summary>
        /// Gets authentication context.
        /// </summary>
        public Auth_HttpDigest AuthContext
        {
            get { return m_pAuth; }
        }

        /// <summary>
        /// Gets or sets if specified request is authenticated.
        /// </summary>
        public bool Authenticated { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="auth">Authentication context.</param>
        public SIP_AuthenticateEventArgs(Auth_HttpDigest auth)
        {
            m_pAuth = auth;
        }

        #endregion
    }
}