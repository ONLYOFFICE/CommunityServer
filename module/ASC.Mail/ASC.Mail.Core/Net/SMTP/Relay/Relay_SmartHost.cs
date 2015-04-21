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


namespace ASC.Mail.Net.SMTP.Relay
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class holds smart host settings.
    /// </summary>
    public class Relay_SmartHost
    {
        #region Members

        private readonly string m_Host = "";
        private readonly string m_Password;
        private readonly int m_Port = 25;
        private readonly SslMode m_SslMode = SslMode.None;
        private readonly string m_UserName;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="host">Smart host name or IP address.</param>
        /// <param name="port">Smart host port.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Relay_SmartHost(string host, int port) : this(host, port, SslMode.None, null, null) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="host">Smart host name or IP address.</param>
        /// <param name="port">Smart host port.</param>
        /// <param name="sslMode">Smart host SSL mode.</param>
        /// <param name="userName">Smart host user name.</param>
        /// <param name="password">Smart host password.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Relay_SmartHost(string host, int port, SslMode sslMode, string userName, string password)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (host == "")
            {
                throw new ArgumentException("Argument 'host' value must be specified.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value is invalid.");
            }

            m_Host = host;
            m_Port = port;
            m_SslMode = sslMode;
            m_UserName = userName;
            m_Password = password;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets smart host name or IP address.
        /// </summary>
        public string Host
        {
            get { return m_Host; }
        }

        /// <summary>
        /// Gets smart host password.
        /// </summary>
        public string Password
        {
            get { return m_Password; }
        }

        /// <summary>
        /// Gets smart host port.
        /// </summary>
        public int Port
        {
            get { return m_Port; }
        }

        /// <summary>
        /// Gets smart host SSL mode.
        /// </summary>
        public SslMode SslMode
        {
            get { return m_SslMode; }
        }

        /// <summary>
        /// Gets smart host user name. Value null means no authentication used.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

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
            if (!(obj is Relay_SmartHost))
            {
                return false;
            }

            Relay_SmartHost smartHost = (Relay_SmartHost) obj;
            if (m_Host != smartHost.Host)
            {
                return false;
            }
            else if (m_Port != smartHost.Port)
            {
                return false;
            }
            else if (m_SslMode != smartHost.SslMode)
            {
                return false;
            }
            else if (m_UserName != smartHost.UserName)
            {
                return false;
            }
            else if (m_Password != smartHost.Password)
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