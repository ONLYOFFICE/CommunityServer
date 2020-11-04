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


namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using System.Net;

    #endregion

    /// <summary>
    /// Represents Received: header "TCP-info" value. Defined in RFC 5321. 4.4.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 5321 4.4.
    ///     TCP-info        = address-literal / ( Domain FWS address-literal )
    ///     address-literal = "[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"
    /// </code>
    /// </remarks>
    public class Mail_t_TcpInfo
    {
        #region Members

        private readonly string m_HostName;
        private readonly IPAddress m_pIP;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <param name="hostName">Host name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public Mail_t_TcpInfo(IPAddress ip, string hostName)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            m_pIP = ip;
            m_HostName = hostName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets host value. Value null means not specified.
        /// </summary>
        public string HostName
        {
            get { return m_HostName; }
        }

        /// <summary>
        /// Gets IP address.
        /// </summary>
        public IPAddress IP
        {
            get { return m_pIP; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(m_HostName))
            {
                return "[" + m_pIP + "]";
            }
            else
            {
                return m_HostName + " [" + m_pIP + "]";
            }
        }

        #endregion
    }
}