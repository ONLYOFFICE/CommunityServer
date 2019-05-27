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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System;
    using Stack;

    #endregion

    /// <summary>
    /// This class represents SIP gateway to other system.
    /// </summary>
    public class SIP_Gateway
    {
        #region Members

        private string m_Host = "";
        private string m_Password = "";
        private int m_Port = 5060;
        private string m_Realm = "";
        private string m_Transport = SIP_Transport.UDP;
        private string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets transport.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public string Transport
        {
            get { return m_Transport; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Value cant be null or empty !");
                }

                m_Transport = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway host name or IP address.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public string Host
        {
            get { return m_Host; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Value cant be null or empty !");
                }

                m_Host = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway port.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public int Port
        {
            get { return m_Port; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Value must be >= 1 !");
                }

                m_Port = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway realm(domain).
        /// </summary>
        public string Realm
        {
            get { return m_Realm; }

            set
            {
                if (value == null)
                {
                    m_Realm = "";
                }

                m_Realm = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway user name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }

            set
            {
                if (value == null)
                {
                    m_UserName = "";
                }

                m_UserName = value;
            }
        }

        /// <summary>
        /// Gets or sets remote gateway password.
        /// </summary>
        public string Password
        {
            get { return m_Password; }

            set
            {
                if (value == null)
                {
                    m_Password = "";
                }

                m_Password = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transport">Transport to use.</param>
        /// <param name="host">Remote gateway host name or IP address.</param>
        /// <param name="port">Remote gateway port.</param>
        public SIP_Gateway(string transport, string host, int port) : this(transport, host, port, "", "", "") {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transport">Transport to use.</param>
        /// <param name="host">Remote gateway host name or IP address.</param>
        /// <param name="port">Remote gateway port.</param>
        /// <param name="realm">Remote gateway realm.</param>
        /// <param name="userName">Remote gateway user name.</param>
        /// <param name="password">Remote gateway password.</param>
        public SIP_Gateway(string transport,
                           string host,
                           int port,
                           string realm,
                           string userName,
                           string password)
        {
            Transport = transport;
            Host = host;
            Port = port;
            Realm = realm;
            UserName = userName;
            Password = password;
        }

        #endregion
    }
}