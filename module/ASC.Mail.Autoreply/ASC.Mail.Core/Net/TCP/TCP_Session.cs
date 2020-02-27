/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace ASC.Mail.Net.TCP
{
    #region usings

    using System;
    using System.Net;
    using System.Security.Principal;
    using IO;

    #endregion

    /// <summary>
    /// This is base class for TCP_Client and TCP_ServerSession.
    /// </summary>
    public abstract class TCP_Session : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets session authenticated user identity , returns null if not authenticated.
        /// </summary>
        public virtual GenericIdentity AuthenticatedUserIdentity
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        public abstract DateTime ConnectTime { get; }

        /// <summary>
        /// Gets session ID.
        /// </summary>
        public abstract string ID { get; }

        /// <summary>
        /// Gets if this session is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get { return AuthenticatedUserIdentity != null; }
        }

        /// <summary>
        /// Gets if session is connected.
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Gets if this session TCP connection is secure connection.
        /// </summary>
        public virtual bool IsSecureConnection
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        public abstract DateTime LastActivity { get; }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        public abstract IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        public abstract IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        public abstract SmartStream TcpStream { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Disconnects session.
        /// </summary>
        public abstract void Disconnect();

        #endregion
    }
}