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

    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;

    #endregion

    /// <summary>
    /// Holds IP bind info.
    /// </summary>
    public class IPBindInfo
    {
        #region Members

        private readonly string m_HostName = "";
        private readonly X509Certificate2 m_pCertificate;
        private readonly IPEndPoint m_pEndPoint;
        private readonly BindInfoProtocol m_Protocol = BindInfoProtocol.TCP;
        private readonly SslMode m_SslMode = SslMode.None;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <param name="protocol">Bind protocol.</param>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        public IPBindInfo(string hostName, BindInfoProtocol protocol, IPAddress ip, int port)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            m_HostName = hostName;
            m_Protocol = protocol;
            m_pEndPoint = new IPEndPoint(ip, port);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        /// <param name="sslMode">Specifies SSL mode.</param>
        /// <param name="sslCertificate">Certificate to use for SSL connections.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        public IPBindInfo(string hostName,
                          IPAddress ip,
                          int port,
                          SslMode sslMode,
                          X509Certificate2 sslCertificate)
            : this(hostName, BindInfoProtocol.TCP, ip, port, sslMode, sslCertificate) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <param name="protocol">Bind protocol.</param>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        /// <param name="sslMode">Specifies SSL mode.</param>
        /// <param name="sslCertificate">Certificate to use for SSL connections.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IPBindInfo(string hostName,
                          BindInfoProtocol protocol,
                          IPAddress ip,
                          int port,
                          SslMode sslMode,
                          X509Certificate2 sslCertificate)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            m_HostName = hostName;
            m_Protocol = protocol;
            m_pEndPoint = new IPEndPoint(ip, port);
            m_SslMode = sslMode;
            m_pCertificate = sslCertificate;
            if ((sslMode == SslMode.SSL || sslMode == SslMode.TLS) && sslCertificate == null)
            {
                throw new ArgumentException("SSL requested, but argument 'sslCertificate' is not provided.");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets SSL certificate.
        /// </summary>
        public X509Certificate2 Certificate
        {
            get { return m_pCertificate; }
        }

        /// <summary>
        /// Gets IP end point.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return m_pEndPoint; }
        }

        /// <summary>
        /// Gets host name.
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
            get { return m_pEndPoint.Address; }
        }

        /// <summary>
        /// Gets port.
        /// </summary>
        public int Port
        {
            get { return m_pEndPoint.Port; }
        }

        /// <summary>
        /// Gets protocol.
        /// </summary>
        public BindInfoProtocol Protocol
        {
            get { return m_Protocol; }
        }

        /// <summary>
        /// Gets SSL certificate.
        /// </summary>
        [Obsolete("Use property Certificate instead.")]
        public X509Certificate2 SSL_Certificate
        {
            get { return m_pCertificate; }
        }

        /// <summary>
        /// Gets SSL mode.
        /// </summary>
        public SslMode SslMode
        {
            get { return m_SslMode; }
        }

        /// <summary>
        /// Gets or sets user data. This is used internally don't use it !!!.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>Returns true if two objects are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is IPBindInfo))
            {
                return false;
            }

            IPBindInfo bInfo = (IPBindInfo) obj;
            if (bInfo.HostName != m_HostName)
            {
                return false;
            }
            if (bInfo.Protocol != m_Protocol)
            {
                return false;
            }
            if (!bInfo.EndPoint.Equals(m_pEndPoint))
            {
                return false;
            }
            if (bInfo.SslMode != m_SslMode)
            {
                return false;
            }
            if (!Equals(bInfo.Certificate, m_pCertificate))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>Returns the hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}