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

    #endregion

    /// <summary>
    /// Represents a network endpoint as an host(name or IP address) and a port number.
    /// </summary>
    public class HostEndPoint
    {
        #region Members

        private readonly string m_Host = "";
        private readonly int m_Port;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if <b>Host</b> is IP address.
        /// </summary>
        public bool IsIPAddress
        {
            get { return Net_Utils.IsIPAddress(m_Host); }
        }

        /// <summary>
        /// Gets host name or IP address.
        /// </summary>
        public string Host
        {
            get { return m_Host; }
        }

        /// <summary>
        /// Gets the port number of the endpoint. Value -1 means port not specified.
        /// </summary>
        public int Port
        {
            get { return m_Port; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">The port number associated with the host. Value -1 means port not specified.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public HostEndPoint(string host, int port)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (host == "")
            {
                throw new ArgumentException("Argument 'host' value must be specified.");
            }

            m_Host = host;
            m_Port = port;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="endPoint">Host IP end point.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>endPoint</b> is null reference.</exception>
        public HostEndPoint(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }

            m_Host = endPoint.Address.ToString();
            m_Port = endPoint.Port;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses HostEndPoint from the specified string.
        /// </summary>
        /// <param name="value">HostEndPoint value.</param>
        /// <returns>Returns parsed HostEndPoint value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static HostEndPoint Parse(string value)
        {
            return Parse(value, -1);
        }

        /// <summary>
        /// Parses HostEndPoint from the specified string.
        /// </summary>
        /// <param name="value">HostEndPoint value.</param>
        /// <param name="defaultPort">If port isn't specified in value, specified port will be used.</param>
        /// <returns>Returns parsed HostEndPoint value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static HostEndPoint Parse(string value, int defaultPort)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value == "")
            {
                throw new ArgumentException("Argument 'value' value must be specified.");
            }

            // We have IP address without a port.
            try
            {
                IPAddress.Parse(value);

                return new HostEndPoint(value, defaultPort);
            }
            catch
            {
                // We have host name with port.
                if (value.IndexOf(':') > -1)
                {
                    string[] host_port = value.Split(new[] {':'}, 2);

                    try
                    {
                        return new HostEndPoint(host_port[0], Convert.ToInt32(host_port[1]));
                    }
                    catch
                    {
                        throw new ArgumentException("Argument 'value' has invalid value.");
                    }
                }
                    // We have host name without port.
                else
                {
                    return new HostEndPoint(value, defaultPort);
                }
            }
        }

        /// <summary>
        /// Returns HostEndPoint as string.
        /// </summary>
        /// <returns>Returns HostEndPoint as string.</returns>
        public override string ToString()
        {
            if (m_Port == -1)
            {
                return m_Host;
            }
            else
            {
                return m_Host + ":" + m_Port;
            }
        }

        #endregion
    }
}