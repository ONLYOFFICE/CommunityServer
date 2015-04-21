/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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