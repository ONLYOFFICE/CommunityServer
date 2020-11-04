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
    /// Implements SIP hop(address,port,transport). Defined in RFC 3261.
    /// </summary>
    public class SIP_Hop
    {
        #region Members

        private readonly IPEndPoint m_pEndPoint;
        private readonly string m_Transport = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets target IP end point.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return m_pEndPoint; }
        }

        /// <summary>
        /// Gets target IP address.
        /// </summary>
        public IPAddress IP
        {
            get { return m_pEndPoint.Address; }
        }

        /// <summary>
        /// Gets target port.
        /// </summary>
        public int Port
        {
            get { return m_pEndPoint.Port; }
        }

        /// <summary>
        /// Gets target SIP transport.
        /// </summary>
        public string Transport
        {
            get { return m_Transport; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ep">IP end point.</param>
        /// <param name="transport">SIP transport to use.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ep</b> or <b>transport</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_Hop(IPEndPoint ep, string transport)
        {
            if (ep == null)
            {
                throw new ArgumentNullException("ep");
            }
            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }
            if (transport == "")
            {
                throw new ArgumentException("Argument 'transport' value must be specified.");
            }

            m_pEndPoint = ep;
            m_Transport = transport;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <param name="port">Destination port.</param>
        /// <param name="transport">SIP transport to use.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> or <b>transport</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_Hop(IPAddress ip, int port, string transport)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value must be >= 1.");
            }
            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }
            if (transport == "")
            {
                throw new ArgumentException("Argument 'transport' value must be specified.");
            }

            m_pEndPoint = new IPEndPoint(ip, port);
            m_Transport = transport;
        }

        #endregion
    }
}