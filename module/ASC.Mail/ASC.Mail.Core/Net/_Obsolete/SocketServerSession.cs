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