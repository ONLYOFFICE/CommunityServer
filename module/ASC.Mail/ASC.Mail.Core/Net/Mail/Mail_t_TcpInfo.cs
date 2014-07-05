/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using System.Net;

    #endregion

    /// <summary>
    /// Represents Received: header "TCP-info" value. Defined in RFC 5321. 4.4.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 5321 4.4.
    ///     TCP-info        = address-literal / ( Domain FWS address-literal )
    ///     address-literal = "[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"
    /// </code>
    /// </remarks>
    public class Mail_t_TcpInfo
    {
        #region Members

        private readonly string m_HostName;
        private readonly IPAddress m_pIP;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <param name="hostName">Host name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public Mail_t_TcpInfo(IPAddress ip, string hostName)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            m_pIP = ip;
            m_HostName = hostName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets host value. Value null means not specified.
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
            get { return m_pIP; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(m_HostName))
            {
                return "[" + m_pIP + "]";
            }
            else
            {
                return m_HostName + " [" + m_pIP + "]";
            }
        }

        #endregion
    }
}