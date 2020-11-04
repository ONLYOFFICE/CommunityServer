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

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class provides data for SIP_ProxyCore.GetGateways event.
    /// </summary>
    public class SIP_GatewayEventArgs
    {
        #region Members

        private readonly List<SIP_Gateway> m_pGateways;
        private readonly string m_UriScheme = "";
        private readonly string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets URI scheme which gateways to get.
        /// </summary>
        public string UriScheme
        {
            get { return m_UriScheme; }
        }

        /// <summary>
        /// Gets authenticated user name.
        /// </summary>        
        public string UserName
        {
            get { return m_UserName; }
        }

        /*
        /// <summary>
        /// Gets or sets if specified user has 
        /// </summary>
        public bool IsForbidden
        {
            get{ return false; }

            set{ }
        }*/

        /// <summary>
        /// Gets gateways collection.
        /// </summary>
        public List<SIP_Gateway> Gateways
        {
            get { return m_pGateways; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uriScheme">URI scheme which gateways to get.</param>
        /// <param name="userName">Authenticated user name.</param>
        /// <exception cref="ArgumentException">If any argument has invalid value.</exception>
        public SIP_GatewayEventArgs(string uriScheme, string userName)
        {
            if (string.IsNullOrEmpty(uriScheme))
            {
                throw new ArgumentException("Argument 'uriScheme' value can't be null or empty !");
            }

            m_UriScheme = uriScheme;
            m_UserName = userName;
            m_pGateways = new List<SIP_Gateway>();
        }

        #endregion
    }
}