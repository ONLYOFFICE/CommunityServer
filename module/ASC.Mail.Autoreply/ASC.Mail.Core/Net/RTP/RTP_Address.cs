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


namespace ASC.Mail.Net.RTP
{
    #region usings

    using System;
    using System.Net;

    #endregion

    /// <summary>
    /// This class implements RTP session address.
    /// </summary>
    public class RTP_Address
    {
        #region Members

        private readonly int m_ControlPort;
        private readonly int m_DataPort;
        private readonly IPAddress m_pIP;
        private readonly IPEndPoint m_pRtcpEP;
        private readonly IPEndPoint m_pRtpEP;
        private readonly int m_TTL;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this is multicast RTP address.
        /// </summary>
        public bool IsMulticast
        {
            get { return Net_Utils.IsMulticastAddress(m_pIP); }
        }

        /// <summary>
        /// Gets IP address.
        /// </summary>
        public IPAddress IP
        {
            get { return m_pIP; }
        }

        /// <summary>
        /// Gets RTP data port.
        /// </summary>
        public int DataPort
        {
            get { return m_DataPort; }
        }

        /// <summary>
        /// Gets RTCP control port.
        /// </summary>
        public int ControlPort
        {
            get { return m_ControlPort; }
        }

        /// <summary>
        /// Gets mulicast TTL(time to live) value.
        /// </summary>
        public int TTL
        {
            get { return m_TTL; }
        }

        /// <summary>
        /// Gets RTP end point.
        /// </summary>
        public IPEndPoint RtpEP
        {
            get { return m_pRtpEP; }
        }

        /// <summary>
        /// Gets RTPCP end point.
        /// </summary>
        public IPEndPoint RtcpEP
        {
            get { return m_pRtcpEP; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Unicast constructor.
        /// </summary>
        /// <param name="ip">Unicast IP address.</param>
        /// <param name="dataPort">RTP data port.</param>
        /// <param name="controlPort">RTP control port. Usualy this is <b>dataPort</b> + 1.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid values.</exception>
        public RTP_Address(IPAddress ip, int dataPort, int controlPort)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }
            if (dataPort < IPEndPoint.MinPort || dataPort > IPEndPoint.MaxPort)
            {
                throw new ArgumentException("Argument 'dataPort' value must be between '" + IPEndPoint.MinPort +
                                            "' and '" + IPEndPoint.MaxPort + "'.");
            }
            if (controlPort < IPEndPoint.MinPort || controlPort > IPEndPoint.MaxPort)
            {
                throw new ArgumentException("Argument 'controlPort' value must be between '" +
                                            IPEndPoint.MinPort + "' and '" + IPEndPoint.MaxPort + "'.");
            }
            if (dataPort == controlPort)
            {
                throw new ArgumentException("Arguments 'dataPort' and 'controlPort' values must be different.");
            }

            m_pIP = ip;
            m_DataPort = dataPort;
            m_ControlPort = controlPort;

            m_pRtpEP = new IPEndPoint(ip, dataPort);
            m_pRtcpEP = new IPEndPoint(ip, controlPort);
        }

        /// <summary>
        /// Multicast constructor.
        /// </summary>
        /// <param name="ip">Multicast IP address.</param>
        /// <param name="dataPort">RTP data port.</param>
        /// <param name="controlPort">RTP control port. Usualy this is <b>dataPort</b> + 1.</param>
        /// <param name="ttl">RTP control port. Usualy this is <b>dataPort</b> + 1.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid values.</exception>
        public RTP_Address(IPAddress ip, int dataPort, int controlPort, int ttl)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }
            if (!Net_Utils.IsMulticastAddress(ip))
            {
                throw new ArgumentException("Argument 'ip' is not multicast ip address.");
            }
            if (dataPort < IPEndPoint.MinPort || dataPort > IPEndPoint.MaxPort)
            {
                throw new ArgumentException("Argument 'dataPort' value must be between '" + IPEndPoint.MinPort +
                                            "' and '" + IPEndPoint.MaxPort + "'.");
            }
            if (controlPort < IPEndPoint.MinPort || controlPort > IPEndPoint.MaxPort)
            {
                throw new ArgumentException("Argument 'controlPort' value must be between '" +
                                            IPEndPoint.MinPort + "' and '" + IPEndPoint.MaxPort + "'.");
            }
            if (dataPort == controlPort)
            {
                throw new ArgumentException("Arguments 'dataPort' and 'controlPort' values must be different.");
            }
            if (ttl < 0 || ttl > 255)
            {
                throw new ArgumentException("Argument 'ttl' value must be between '0' and '255'.");
            }

            m_pIP = ip;
            m_DataPort = dataPort;
            m_ControlPort = controlPort;
            m_TTL = ttl;

            m_pRtpEP = new IPEndPoint(ip, dataPort);
            m_pRtcpEP = new IPEndPoint(ip, controlPort);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object.
        /// </summary>
        /// <param name="obj">The Object to compare with the current Object.</param>
        /// <returns>True if the specified Object is equal to the current Object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is RTP_Address)
            {
                RTP_Address a = (RTP_Address) obj;
                if (!a.IP.Equals(IP))
                {
                    return false;
                }
                if (a.DataPort != DataPort)
                {
                    return false;
                }
                if (a.ControlPort != ControlPort)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets this hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}