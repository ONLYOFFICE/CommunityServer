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


namespace ASC.Mail.Net
{
    #region usings

    using System.Net;

    #endregion

    /// <summary>
    /// Provides data for the ValidateIPAddress event for servers.
    /// </summary>
    public class ValidateIP_EventArgs
    {
        #region Members

        private readonly IPEndPoint m_pLocalEndPoint;
        private readonly IPEndPoint m_pRemoteEndPoint;
        private string m_ErrorText = "";
        private bool m_Validated = true;

        #endregion

        #region Properties

        /// <summary>
        /// IP address of computer, which is sending mail to here.
        /// </summary>
        public string ConnectedIP
        {
            get { return m_pRemoteEndPoint.Address.ToString(); }
        }

        /// <summary>
        /// Gets local endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return m_pLocalEndPoint; }
        }

        /// <summary>
        /// Gets remote endpoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets if IP is allowed access.
        /// </summary>
        public bool Validated
        {
            get { return m_Validated; }

            set { m_Validated = value; }
        }

        /// <summary>
        /// Gets or sets user data what is stored to session.Tag property.
        /// </summary>
        public object SessionTag { get; set; }

        /// <summary>
        /// Gets or sets error text what is sent to connected socket. NOTE: This is only used if Validated = false.
        /// </summary>
        public string ErrorText
        {
            get { return m_ErrorText; }

            set { m_ErrorText = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="localEndPoint">Server IP.</param>
        /// <param name="remoteEndPoint">Connected client IP.</param>
        public ValidateIP_EventArgs(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            m_pLocalEndPoint = localEndPoint;
            m_pRemoteEndPoint = remoteEndPoint;
        }

        #endregion
    }
}