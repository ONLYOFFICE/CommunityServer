/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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