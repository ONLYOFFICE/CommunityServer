/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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