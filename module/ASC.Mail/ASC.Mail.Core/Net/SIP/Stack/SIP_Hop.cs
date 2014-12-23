/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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