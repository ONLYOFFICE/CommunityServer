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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.Net;

    #endregion

    /// <summary>
    /// This is base class for SocketServer sessions.
    /// </summary>
    public abstract class SocketServerSession
    {
        #region Members

        private readonly IPBindInfo m_pBindInfo;
        private readonly SocketServer m_pServer;
        private readonly SocketEx m_pSocket;
        private readonly string m_SessionID = "";
        private readonly DateTime m_SessionStartTime;
        private string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets session ID.
        /// </summary>
        public string SessionID
        {
            get { return m_SessionID; }
        }

        /// <summary>
        /// Gets session start time.
        /// </summary>
        public DateTime SessionStartTime
        {
            get { return m_SessionStartTime; }
        }

        /// <summary>
        /// Gets if session is authenticated.
        /// </summary>
        public bool Authenticated
        {
            get
            {
                if (m_UserName.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets authenticated user name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Gets how many seconds has left before timout is triggered.
        /// </summary>
        public int ExpectedTimeout
        {
            get
            {
                return
                    (int)
                    ((m_pServer.SessionIdleTimeOut - ((DateTime.Now.Ticks - SessionLastDataTime.Ticks)/10000))/
                     1000);
            }
        }

        /// <summary>
        /// Gets last data activity time.
        /// </summary>
        public DateTime SessionLastDataTime
        {
            get
            {
                if (m_pSocket == null)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    return m_pSocket.LastActivity;
                }
            }
        }

        /// <summary>
        /// Gets EndPoint which accepted conection.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint) m_pSocket.LocalEndPoint; }
        }

        /// <summary>
        /// Gets connected Host(client) EndPoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                try
                {
                    return (IPEndPoint) m_pSocket.RemoteEndPoint;
                }
                catch
                {
                    // Socket closed/disposed already
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets custom user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets log entries that are currently in log buffer.
        /// </summary>
        public SocketLogger SessionActiveLog
        {
            get { return m_pSocket.Logger; }
        }

        /// <summary>
        /// Gets how many bytes are readed through this session.
        /// </summary>
        public long ReadedCount
        {
            get { return m_pSocket.ReadedCount; }
        }

        /// <summary>
        /// Gets how many bytes are written through this session.
        /// </summary>
        public long WrittenCount
        {
            get { return m_pSocket.WrittenCount; }
        }

        /// <summary>
        /// Gets if the connection is an SSL connection.
        /// </summary>
        public bool IsSecureConnection
        {
            get { return m_pSocket.SSL; }
        }

        /// <summary>
        /// Gets access to SocketEx.
        /// </summary>
        protected SocketEx Socket
        {
            get { return m_pSocket; }
        }

        /// <summary>
        /// Gets access to BindInfo what accepted socket.
        /// </summary>
        protected IPBindInfo BindInfo
        {
            get { return m_pBindInfo; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sessionID">Session ID.</param>
        /// <param name="socket">Server connected socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
        /// <param name="server">Reference to server.</param>
        public SocketServerSession(string sessionID, SocketEx socket, IPBindInfo bindInfo, SocketServer server)
        {
            m_SessionID = sessionID;
            m_pSocket = socket;
            m_pBindInfo = bindInfo;
            m_pServer = server;

            m_SessionStartTime = DateTime.Now;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Kills session.
        /// </summary>
        public virtual void Kill() {}

        #endregion

        #region Virtual methods

        /// <summary>
        /// Times session out.
        /// </summary>
        protected internal virtual void OnSessionTimeout() {}

        #endregion

        /// <summary>
        /// Sets property UserName value.
        /// </summary>
        /// <param name="userName">User name.</param>
        protected void SetUserName(string userName)
        {
            m_UserName = userName;

            if (m_pSocket.Logger != null)
            {
                m_pSocket.Logger.UserName = m_UserName;
            }
        }
    }
}