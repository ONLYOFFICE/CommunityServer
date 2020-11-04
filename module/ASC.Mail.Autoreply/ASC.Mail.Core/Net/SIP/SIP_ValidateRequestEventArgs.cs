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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;
    using System.Net;

    #endregion

    /// <summary>
    /// This class provides data for SIP_Stack.ValidateRequest event.
    /// </summary>
    public class SIP_ValidateRequestEventArgs : EventArgs
    {
        #region Members

        private readonly IPEndPoint m_pRemoteEndPoint;
        private readonly SIP_Request m_pRequest;

        #endregion

        #region Properties

        /// <summary>
        /// Gets incoming SIP request.
        /// </summary>
        public SIP_Request Request
        {
            get { return m_pRequest; }
        }

        /// <summary>
        /// Gets IP end point what made request.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets response code. Value null means SIP stack will handle it.
        /// </summary>
        public string ResponseCode { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="request">Incoming SIP request.</param>
        /// <param name="remoteEndpoint">IP end point what made request.</param>
        public SIP_ValidateRequestEventArgs(SIP_Request request, IPEndPoint remoteEndpoint)
        {
            m_pRequest = request;
            m_pRemoteEndPoint = remoteEndpoint;
        }

        #endregion
    }
}