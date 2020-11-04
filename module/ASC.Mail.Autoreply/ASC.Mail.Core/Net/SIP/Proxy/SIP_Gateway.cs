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
    using Stack;

    #endregion

    /// <summary>
    /// This class represents SIP gateway to other system.
    /// </summary>
    public class SIP_Gateway
    {
        #region Members

        private string m_Host = "";
        private string m_Password = "";
        private int m_Port = 5060;
        private string m_Realm = "";
        private string m_Transport = SIP_Transport.UDP;
        private string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets transport.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public string Transport
        {
            get { return m_Transport; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Value cant be null or empty !");
                }

                m_Transport = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway host name or IP address.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public string Host
        {
            get { return m_Host; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Value cant be null or empty !");
                }

                m_Host = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway port.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public int Port
        {
            get { return m_Port; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Value must be >= 1 !");
                }

                m_Port = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway realm(domain).
        /// </summary>
        public string Realm
        {
            get { return m_Realm; }

            set
            {
                if (value == null)
                {
                    m_Realm = "";
                }

                m_Realm = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway user name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }

            set
            {
                if (value == null)
                {
                    m_UserName = "";
                }

                m_UserName = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway password.
        /// </summary>
        public string Password
        {
            get { return m_Password; }

            set
            {
                if (value == null)
                {
                    m_Password = "";
                }

                m_Password = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transport">Transport to use.</param>
        /// <param name="host">Remote gateway host name or IP address.</param>
        /// <param name="port">Remote gateway port.</param>
        public SIP_Gateway(string transport, string host, int port) : this(transport, host, port, "", "", "") {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transport">Transport to use.</param>
        /// <param name="host">Remote gateway host name or IP address.</param>
        /// <param name="port">Remote gateway port.</param>
        /// <param name="realm">Remote gateway realm.</param>
        /// <param name="userName">Remote gateway user name.</param>
        /// <param name="password">Remote gateway password.</param>
        public SIP_Gateway(string transport,
                           string host,
                           int port,
                           string realm,
                           string userName,
                           string password)
        {
            Transport = transport;
            Host = host;
            Port = port;
            Realm = realm;
            UserName = userName;
            Password = password;
        }

        #endregion
    }
}