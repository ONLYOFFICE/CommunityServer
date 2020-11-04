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


namespace ASC.Mail.Net.FTP.Server
{
    #region usings

    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    #endregion

    #region Event delegates

    /// <summary>
    /// Represents the method that will handle the AuthUser event for FTP_Server.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">A AuthUser_EventArgs that contains the event data.</param>
    public delegate void AuthUserEventHandler(object sender, AuthUser_EventArgs e);

    /// <summary>
    /// Represents the method that will handle the filsystem rerlated events for FTP_Server.
    /// </summary>
    public delegate void FileSysEntryEventHandler(object sender, FileSysEntry_EventArgs e);

    #endregion

    /// <summary>
    /// FTP Server component.
    /// </summary>
    public class FTP_Server : SocketServer
    {
        #region Events

        /// <summary>
        /// Occurs when connected user tryes to authenticate.
        /// </summary>
        public event AuthUserEventHandler AuthUser = null;

        /// <summary>
        /// Occurs when server needs needs to create directory.
        /// </summary>
        public event FileSysEntryEventHandler CreateDir = null;

        /// <summary>
        /// Occurs when server needs needs to delete directory.
        /// </summary>
        public event FileSysEntryEventHandler DeleteDir = null;

        /// <summary>
        /// Occurs when server needs needs to delete file.
        /// </summary>
        public event FileSysEntryEventHandler DeleteFile = null;

        /// <summary>
        /// Occurs when server needs to validatee directory.
        /// </summary>
        public event FileSysEntryEventHandler DirExists = null;

        /// <summary>
        /// Occurs when server needs needs validate file.
        /// </summary>
        public event FileSysEntryEventHandler FileExists = null;

        /// <summary>
        /// Occurs when server needs directory info (directories,files in deirectory).
        /// </summary>
        public event FileSysEntryEventHandler GetDirInfo = null;

        /// <summary>
        /// Occurs when server needs needs to get file.
        /// </summary>
        public event FileSysEntryEventHandler GetFile = null;

        /// <summary>
        /// Occurs when server needs needs to rname directory or file.
        /// </summary>
        public event FileSysEntryEventHandler RenameDirFile = null;

        /// <summary>
        /// Occurs when POP3 session has finished and session log is available.
        /// </summary>
        public event LogEventHandler SessionLog = null;

        /// <summary>
        /// Occurs when server needs needs to store file.
        /// </summary>
        public event FileSysEntryEventHandler StoreFile = null;

        /// <summary>
        /// Occurs when new computer connected to FTP server.
        /// </summary>
        public event ValidateIPHandler ValidateIPAddress = null;

        #endregion

        #region Members

        private int m_PassiveStartPort = 20000;

        #endregion

        #region Properties

        /// <summary>
        /// Gets active sessions.
        /// </summary>
        public new FTP_Session[] Sessions
        {
            get
            {
                SocketServerSession[] sessions = base.Sessions;
                FTP_Session[] ftpSessions = new FTP_Session[sessions.Length];
                sessions.CopyTo(ftpSessions, 0);

                return ftpSessions;
            }
        }

        /// <summary>
        /// Gets or sets passive mode public IP address what is reported to clients. 
        /// This property is manly needed if FTP server is running behind NAT. 
        /// Value null means not spcified.
        /// </summary>
        public IPAddress PassivePublicIP { get; set; }

        /// <summary>
        /// Gets or sets passive mode start port form which server starts using ports.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when ivalid value is passed.</exception>
        public int PassiveStartPort
        {
            get { return m_PassiveStartPort; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Valu must be > 0 !");
                }

                m_PassiveStartPort = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Defalut constructor.
        /// </summary>
        public FTP_Server()
        {
            BindInfo = new[] {new IPBindInfo("", IPAddress.Any, 21, SslMode.None, null)};
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Initialize and start new session here. Session isn't added to session list automatically, 
        /// session must add itself to server session list by calling AddSession().
        /// </summary>
        /// <param name="socket">Connected client socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
        protected override void InitNewSession(Socket socket, IPBindInfo bindInfo)
        {
            string sessionID = Guid.NewGuid().ToString();
            SocketEx socketEx = new SocketEx(socket);
            if (LogCommands)
            {
                socketEx.Logger = new SocketLogger(socket, SessionLog);
                socketEx.Logger.SessionID = sessionID;
            }
            FTP_Session session = new FTP_Session(sessionID, socketEx, bindInfo, this);
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Raises event ValidateIP event.
        /// </summary>
        /// <param name="localEndPoint">Server IP.</param>
        /// <param name="remoteEndPoint">Connected client IP.</param>
        /// <returns>Returns true if connection allowed.</returns>
        internal virtual bool OnValidate_IpAddress(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            ValidateIP_EventArgs oArg = new ValidateIP_EventArgs(localEndPoint, remoteEndPoint);
            if (ValidateIPAddress != null)
            {
                ValidateIPAddress(this, oArg);
            }

            return oArg.Validated;
        }

        /// <summary>
        /// Authenticates user.
        /// </summary>
        /// <param name="session">Reference to current pop3 session.</param>
        /// <param name="userName">User name.</param>
        /// <param name="passwData"></param>
        /// <param name="data"></param>
        /// <param name="authType"></param>
        /// <returns></returns>
        internal virtual bool OnAuthUser(FTP_Session session,
                                         string userName,
                                         string passwData,
                                         string data,
                                         AuthType authType)
        {
            AuthUser_EventArgs oArg = new AuthUser_EventArgs(session, userName, passwData, data, authType);
            if (AuthUser != null)
            {
                AuthUser(this, oArg);
            }

            return oArg.Validated;
        }

        #endregion

        #region Internal methods

        internal FileSysEntry_EventArgs OnGetDirInfo(FTP_Session session, string dir)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, dir, "");
            if (GetDirInfo != null)
            {
                GetDirInfo(this, oArg);
            }
            return oArg;
        }

        internal bool OnDirExists(FTP_Session session, string dir)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, dir, "");
            if (DirExists != null)
            {
                DirExists(this, oArg);
            }

            return oArg.Validated;
        }

        internal bool OnCreateDir(FTP_Session session, string dir)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, dir, "");
            if (CreateDir != null)
            {
                CreateDir(this, oArg);
            }

            return oArg.Validated;
        }

        internal bool OnDeleteDir(FTP_Session session, string dir)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, dir, "");
            if (DeleteDir != null)
            {
                DeleteDir(this, oArg);
            }

            return oArg.Validated;
        }

        internal bool OnRenameDirFile(FTP_Session session, string from, string to)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, from, to);
            if (RenameDirFile != null)
            {
                RenameDirFile(this, oArg);
            }

            return oArg.Validated;
        }

        internal bool OnFileExists(FTP_Session session, string file)
        {
            // Remove last /
            file = file.Substring(0, file.Length - 1);

            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, file, "");
            if (FileExists != null)
            {
                FileExists(this, oArg);
            }

            return oArg.Validated;
        }

        internal Stream OnGetFile(FTP_Session session, string file)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, file, "");
            if (GetFile != null)
            {
                GetFile(this, oArg);
            }

            return oArg.FileStream;
        }

        internal Stream OnStoreFile(FTP_Session session, string file)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, file, "");
            if (StoreFile != null)
            {
                StoreFile(this, oArg);
            }

            return oArg.FileStream;
        }

        internal bool OnDeleteFile(FTP_Session session, string file)
        {
            FileSysEntry_EventArgs oArg = new FileSysEntry_EventArgs(session, file, "");
            if (DeleteFile != null)
            {
                DeleteFile(this, oArg);
            }

            return oArg.Validated;
        }

        #endregion
    }
}