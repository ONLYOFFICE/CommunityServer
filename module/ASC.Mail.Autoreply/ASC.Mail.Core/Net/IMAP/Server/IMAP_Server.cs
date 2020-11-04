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


using ASC.Mail.Net.TCP;

namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using AUTH;

    #endregion

    #region Event delegates

    /// <summary>
    /// Represents the method that will handle the AuthUser event for SMTP_Server.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">A AuthUser_EventArgs that contains the event data.</param>
    public delegate void AuthUserEventHandler(object sender, AuthUser_EventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void FolderEventHandler(object sender, Mailbox_EventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void FoldersEventHandler(object sender, IMAP_Folders e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void MessagesEventHandler(object sender, IMAP_eArgs_GetMessagesInfo e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void MessagesItemsEventHandler(object sender, IMAP_eArgs_MessageItems e);

    public delegate byte[] FiletGetDelegate(IMAP_Session session);
    public delegate void FiletSetDelegate(IMAP_Session session, byte[] buffer);
    /// <summary>
    /// 
    /// </summary>
    public delegate void MessageEventHandler(object sender, Message_EventArgs e);

    /*
	/// <summary>
	/// 
	/// </summary>
	public delegate void SearchEventHandler(object sender,IMAP_eArgs_Search e);*/

    /// <summary>
    /// 
    /// </summary>
    public delegate void SharedRootFoldersEventHandler(object sender, SharedRootFolders_EventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void GetFolderACLEventHandler(object sender, IMAP_GETACL_eArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void DeleteFolderACLEventHandler(object sender, IMAP_DELETEACL_eArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void SetFolderACLEventHandler(object sender, IMAP_SETACL_eArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void GetUserACLEventHandler(object sender, IMAP_GetUserACL_eArgs e);

    /// <summary>
    /// 
    /// </summary>
    public delegate void GetUserQuotaHandler(object sender, IMAP_eArgs_GetQuota e);

    #endregion

    /// <summary>
    /// IMAP server componet.
    /// </summary>
    public class IMAP_Server : TCP_Server<IMAP_Session>
    {
        #region Events

        /// <summary>
        /// Occurs when connected user tryes to authenticate.
        /// </summary>
        public event AuthUserEventHandler AuthUser = null;

        /// <summary>
        /// Occurs when server requests to copy message to new location.
        /// </summary>
        public event MessageEventHandler CopyMessage = null;

        /// <summary>
        /// Occurs when server requests to create folder.
        /// </summary>
        public event FolderEventHandler CreateFolder = null;

        /// <summary>
        /// Occurs when server requests to delete folder.
        /// </summary>
        public event FolderEventHandler DeleteFolder = null;

        /// <summary>
        /// Occurs when IMAP server requests to delete folder ACL.
        /// </summary>
        public event DeleteFolderACLEventHandler DeleteFolderACL = null;

        /// <summary>
        /// Occurs when server requests to delete message.
        /// </summary>
        public event MessageEventHandler DeleteMessage = null;

        /// <summary>
        /// Occurs when IMAP server requests folder ACL.
        /// </summary>
        public event GetFolderACLEventHandler GetFolderACL = null;

        /// <summary>
        /// Occurs when server requests all available folders.
        /// </summary>
        public event FoldersEventHandler GetFolders = null;

        /// <summary>
        /// Occurs when server requests to get message items.
        /// </summary>
        public event MessagesItemsEventHandler GetMessageItems = null;

        /// <summary>
        /// Occurs when server requests to folder messages info.
        /// </summary>
        public event MessagesEventHandler GetMessagesInfo = null;

        /// <summary>
        /// Occurs when IMAP server requests shared root folders info.
        /// </summary>
        public event SharedRootFoldersEventHandler GetSharedRootFolders = null;

        /// <summary>
        /// Occurs when server requests subscribed folders.
        /// </summary>
        public event FoldersEventHandler GetSubscribedFolders = null;

        /// <summary>
        /// Occurs when IMAP server requests to get user ACL for specified folder.
        /// </summary>
        public event GetUserACLEventHandler GetUserACL = null;

        /// <summary>
        /// Occurs when IMAP server requests to get user quota.
        /// </summary>
        public event GetUserQuotaHandler GetUserQuota = null;

        /// <summary>
        /// Occurs when server requests to rename folder.
        /// </summary>
        public event FolderEventHandler RenameFolder = null;

        /// <summary>
        /// Occurs when IMAP session has finished and session log is available.
        /// </summary>
        public event LogEventHandler SessionLog = null;

        /// <summary>
        /// Occurs when IMAP server requests to set folder ACL.
        /// </summary>
        public event SetFolderACLEventHandler SetFolderACL = null;

        /// <summary>
        /// Occurs when server requests to store message.
        /// </summary>
        public event MessageEventHandler StoreMessage = null;

        /*
		/// <summary>
		/// Occurs when server requests to search specified folder messages.
		/// </summary>
		public event SearchEventHandler Search = null;*/

        /// <summary>
        /// Occurs when server requests to store message flags.
        /// </summary>
        public event MessageEventHandler StoreMessageFlags = null;

        /// <summary>
        /// Occurs when server requests to subscribe folder.
        /// </summary>
        public event FolderEventHandler SubscribeFolder = null;

        /// <summary>
        /// Occurs when server requests to unsubscribe folder.
        /// </summary>
        public event FolderEventHandler UnSubscribeFolder = null;

        /// <summary>
        /// Occurs when new computer connected to IMAP server.
        /// </summary>
        public event ValidateIPHandler ValidateIPAddress = null;

        internal delegate void FolderChangedHandler(string folderName, string username);

        internal event FolderChangedHandler FolderChanged = null;


        public void OnFolderChanged(string folderName, string username)
        {
            if (FolderChanged!=null)
            {
                FolderChanged(folderName, username);
            }
        }

        #endregion

        #region Members

        private string m_GreetingText = "";
        private int m_MaxConnectionsPerIP;
        private int m_MaxMessageSize = 1000000;
        private SaslAuthTypes m_SupportedAuth = SaslAuthTypes.All;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets server supported authentication types.
        /// </summary>
        public SaslAuthTypes SupportedAuthentications
        {
            get { return m_SupportedAuth; }

            set { m_SupportedAuth = value; }
        }

        /// <summary>
        /// Gets or sets server greeting text.
        /// </summary>
        public string GreetingText
        {
            get { return m_GreetingText; }

            set { m_GreetingText = value; }
        }

        /// <summary>
        /// Gets or sets maximum allowed conncurent connections from 1 IP address. Value 0 means unlimited connections.
        /// </summary>
		public new int MaxConnectionsPerIP
        {
            get { return m_MaxConnectionsPerIP; }

            set { m_MaxConnectionsPerIP = value; }
        }

        /// <summary>
        /// Maximum message size.
        /// </summary>
        public int MaxMessageSize
        {
            get { return m_MaxMessageSize; }

            set { m_MaxMessageSize = value; }
        }


        public int MaxBadCommands { get; set; }

        #endregion

        #region Constructor


        #endregion

        #region Overrides

        protected override void OnMaxConnectionsPerIPExceeded(IMAP_Session session)
        {
            base.OnMaxConnectionsPerIPExceeded(session);
            session.TcpStream.WriteLine("* NO Maximum connections from your IP address is exceeded, try again later !\r\n");
        }

        protected override void OnMaxConnectionsExceeded(IMAP_Session session)
        {
            session.TcpStream.WriteLine("* NO Maximum connections is exceeded");
        }

        /// <summary>
        /// Initialize and start new session here. Session isn't added to session list automatically, 
        /// session must add itself to server session list by calling AddSession().
        /// </summary>
        /// <param name="socket">Connected client socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
        //protected override void InitNewSession(Socket socket, IPBindInfo bindInfo)
        //{
        //    // Check maximum conncurent connections from 1 IP.
        //    if (m_MaxConnectionsPerIP > 0)
        //    {
        //        lock (Sessions)
        //        {
        //            int nSessions = 0;
        //            foreach (SocketServerSession s in Sessions)
        //            {
        //                IPEndPoint ipEndpoint = s.RemoteEndPoint;
        //                if (ipEndpoint != null)
        //                {
        //                    if (ipEndpoint.Address.Equals(((IPEndPoint) socket.RemoteEndPoint).Address))
        //                    {
        //                        nSessions++;
        //                    }
        //                }

        //                // Maimum allowed exceeded
        //                if (nSessions >= m_MaxConnectionsPerIP)
        //                {
        //                    socket.Send(
        //                        Encoding.ASCII.GetBytes(
        //                            "* NO Maximum connections from your IP address is exceeded, try again later !\r\n"));
        //                    socket.Shutdown(SocketShutdown.Both);
        //                    socket.Close();
        //                    return;
        //                }
        //            }
        //        }
        //    }

        //    string sessionID = Guid.NewGuid().ToString();
        //    SocketEx socketEx = new SocketEx(socket);
        //    if (LogCommands)
        //    {
        //        socketEx.Logger = new SocketLogger(socket, SessionLog);
        //        socketEx.Logger.SessionID = sessionID;
        //    }
        //    IMAP_Session session = new IMAP_Session(sessionID, socketEx, bindInfo, this);
        //    FolderChanged += session.OnFolderChanged;
        //}

        #endregion

        #region Internal methods

        /// <summary>
        /// Raises event ValidateIP event.
        /// </summary>
        /// <param name="localEndPoint">Server IP.</param>
        /// <param name="remoteEndPoint">Connected client IP.</param>
        /// <returns>Returns true if connection allowed.</returns>
        internal bool OnValidate_IpAddress(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            ValidateIP_EventArgs oArg = new ValidateIP_EventArgs(localEndPoint, remoteEndPoint);
            if (ValidateIPAddress != null)
            {
                ValidateIPAddress(this, oArg);
            }

            return oArg.Validated;
        }

        /// <summary>
        /// Raises event AuthUser.
        /// </summary>
        /// <param name="session">Reference to current IMAP session.</param>
        /// <param name="userName">User name.</param>
        /// <param name="passwordData">Password compare data,it depends of authentication type.</param>
        /// <param name="data">For md5 eg. md5 calculation hash.It depends of authentication type.</param>
        /// <param name="authType">Authentication type.</param>
        /// <returns>Returns true if user is authenticated ok.</returns>
        internal AuthUser_EventArgs OnAuthUser(IMAP_Session session,
                                               string userName,
                                               string passwordData,
                                               string data,
                                               AuthType authType)
        {
            AuthUser_EventArgs oArgs = new AuthUser_EventArgs(session, userName, passwordData, data, authType);
            if (AuthUser != null)
            {
                AuthUser(this, oArgs);
            }

            return oArgs;
        }

        /// <summary>
        /// Raises event 'SubscribeMailbox'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="mailbox">Mailbox which to subscribe.</param>
        /// <returns></returns>
        internal string OnSubscribeMailbox(IMAP_Session session, string mailbox)
        {
            if (SubscribeFolder != null)
            {
                Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
                SubscribeFolder(session, eArgs);

                return eArgs.ErrorText;
            }

            return null;
        }

        /// <summary>
        /// Raises event 'UnSubscribeMailbox'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="mailbox">Mailbox which to unsubscribe.</param>
        /// <returns></returns>
        internal string OnUnSubscribeMailbox(IMAP_Session session, string mailbox)
        {
            if (UnSubscribeFolder != null)
            {
                Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
                UnSubscribeFolder(session, eArgs);

                return eArgs.ErrorText;
            }

            return null;
        }

        /// <summary>
        /// Raises event 'GetSubscribedMailboxes'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="referenceName">Mailbox reference.</param>
        /// <param name="mailBox">Mailbox search pattern or mailbox.</param>
        /// <returns></returns>
        internal IMAP_Folders OnGetSubscribedMailboxes(IMAP_Session session,
                                                       string referenceName,
                                                       string mailBox)
        {
            IMAP_Folders retVal = new IMAP_Folders(session, referenceName, mailBox);
            if (GetSubscribedFolders != null)
            {
                GetSubscribedFolders(session, retVal);
            }

            return retVal;
        }

        /// <summary>
        /// Raises event 'GetMailboxes'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="referenceName">Mailbox reference.</param>
        /// <param name="mailBox">Mailbox search pattern or mailbox.</param>
        /// <returns></returns>
        internal IMAP_Folders OnGetMailboxes(IMAP_Session session, string referenceName, string mailBox)
        {
            IMAP_Folders retVal = new IMAP_Folders(session, referenceName, mailBox);
            if (GetFolders != null)
            {
                GetFolders(session, retVal);
            }

            return retVal;
        }

        /// <summary>
        /// Raises event 'CreateMailbox'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="mailbox">Mailbox to create.</param>
        /// <returns></returns>
        internal string OnCreateMailbox(IMAP_Session session, string mailbox)
        {
            if (CreateFolder != null)
            {
                Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
                CreateFolder(session, eArgs);

                return eArgs.ErrorText;
            }

            return null;
        }

        /// <summary>
        /// Raises event 'DeleteMailbox'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="mailbox">Mailbox which to delete.</param>
        /// <returns></returns>
        internal string OnDeleteMailbox(IMAP_Session session, string mailbox)
        {
            if (DeleteFolder != null)
            {
                Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
                DeleteFolder(session, eArgs);

                return eArgs.ErrorText;
            }

            return null;
        }

        /// <summary>
        /// Raises event 'RenameMailbox'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="mailbox">Mailbox which to rename.</param>
        /// <param name="newMailboxName">New mailbox name.</param>
        /// <returns></returns>
        internal string OnRenameMailbox(IMAP_Session session, string mailbox, string newMailboxName)
        {
            if (RenameFolder != null)
            {
                Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox, newMailboxName);
                RenameFolder(session, eArgs);

                return eArgs.ErrorText;
            }

            return null;
        }

        /// <summary>
        /// Raises event 'GetMessagesInfo'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="folder">Folder which messages info to get.</param>
        /// <returns></returns>
        internal IMAP_eArgs_GetMessagesInfo OnGetMessagesInfo(IMAP_Session session, IMAP_SelectedFolder folder)
        {
            IMAP_eArgs_GetMessagesInfo eArgs = new IMAP_eArgs_GetMessagesInfo(session, folder);
            if (GetMessagesInfo != null)
            {
                GetMessagesInfo(session, eArgs);
            }

            return eArgs;
        }

        /// <summary>
        /// Raises event 'DeleteMessage'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="message">Message which to delete.</param>
        /// <returns></returns>
        internal string OnDeleteMessage(IMAP_Session session, IMAP_Message message)
        {
            Message_EventArgs eArgs =
                new Message_EventArgs(Core.Decode_IMAP_UTF7_String(session.SelectedMailbox), message);
            if (DeleteMessage != null)
            {
                DeleteMessage(session, eArgs);
            }

            return eArgs.ErrorText;
        }

        /// <summary>
        /// Raises event 'CopyMessage'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="msg">Message which to copy.</param>
        /// <param name="location">New message location.</param>
        /// <returns></returns>
        internal string OnCopyMessage(IMAP_Session session, IMAP_Message msg, string location)
        {
            Message_EventArgs eArgs =
                new Message_EventArgs(Core.Decode_IMAP_UTF7_String(session.SelectedMailbox), msg, location);
            if (CopyMessage != null)
            {
                CopyMessage(session, eArgs);
            }

            return eArgs.ErrorText;
        }

        /// <summary>
        /// Raises event 'StoreMessage'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="folder">Folder where to store.</param>
        /// <param name="msg">Message which to store.</param>
        /// <param name="messageData">Message data which to store.</param>
        /// <returns></returns>
        internal string OnStoreMessage(IMAP_Session session,
                                       string folder,
                                       IMAP_Message msg,
                                       byte[] messageData)
        {
            Message_EventArgs eArgs = new Message_EventArgs(folder, msg);
            eArgs.MessageData = messageData;
            if (StoreMessage != null)
            {
                StoreMessage(session, eArgs);
            }

            return eArgs.ErrorText;
        }

        /*
		/// <summary>
		/// Raises event 'Search'.
		/// </summary>
		/// <param name="session">IMAP session what calls this search.</param>
		/// <param name="folder">Folder what messages to search.</param>
		/// <param name="matcher">Matcher what must be used to check if message matches searching criterial.</param>
		/// <returns></returns>
		internal IMAP_eArgs_Search OnSearch(IMAP_Session session,string folder,IMAP_SearchMatcher matcher)
		{
			IMAP_eArgs_Search eArgs = new IMAP_eArgs_Search(session,folder,matcher);
			if(this.Search != null){
				this.Search(session,eArgs);
			}

			return eArgs;
		}*/

        /// <summary>
        /// Raises event 'StoreMessageFlags'.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="msg">Message which flags to store.</param>
        /// <returns></returns>
        internal string OnStoreMessageFlags(IMAP_Session session, IMAP_Message msg)
        {
            Message_EventArgs eArgs =
                new Message_EventArgs(Core.Decode_IMAP_UTF7_String(session.SelectedMailbox), msg);
            if (StoreMessageFlags != null)
            {
                StoreMessageFlags(session, eArgs);
            }

            return eArgs.ErrorText;
        }

        internal SharedRootFolders_EventArgs OnGetSharedRootFolders(IMAP_Session session)
        {
            SharedRootFolders_EventArgs eArgs = new SharedRootFolders_EventArgs(session);
            if (GetSharedRootFolders != null)
            {
                GetSharedRootFolders(session, eArgs);
            }

            return eArgs;
        }

        internal IMAP_GETACL_eArgs OnGetFolderACL(IMAP_Session session, string folderName)
        {
            IMAP_GETACL_eArgs eArgs = new IMAP_GETACL_eArgs(session, folderName);
            if (GetFolderACL != null)
            {
                GetFolderACL(session, eArgs);
            }

            return eArgs;
        }

        internal IMAP_SETACL_eArgs OnSetFolderACL(IMAP_Session session,
                                                  string folderName,
                                                  string userName,
                                                  IMAP_Flags_SetType flagsSetType,
                                                  IMAP_ACL_Flags aclFlags)
        {
            IMAP_SETACL_eArgs eArgs = new IMAP_SETACL_eArgs(session,
                                                            folderName,
                                                            userName,
                                                            flagsSetType,
                                                            aclFlags);
            if (SetFolderACL != null)
            {
                SetFolderACL(session, eArgs);
            }

            return eArgs;
        }

        internal IMAP_DELETEACL_eArgs OnDeleteFolderACL(IMAP_Session session,
                                                        string folderName,
                                                        string userName)
        {
            IMAP_DELETEACL_eArgs eArgs = new IMAP_DELETEACL_eArgs(session, folderName, userName);
            if (DeleteFolderACL != null)
            {
                DeleteFolderACL(session, eArgs);
            }

            return eArgs;
        }

        internal IMAP_GetUserACL_eArgs OnGetUserACL(IMAP_Session session, string folderName, string userName)
        {
            IMAP_GetUserACL_eArgs eArgs = new IMAP_GetUserACL_eArgs(session, folderName, userName);
            if (GetUserACL != null)
            {
                GetUserACL(session, eArgs);
            }

            return eArgs;
        }

        internal IMAP_eArgs_GetQuota OnGetUserQuota(IMAP_Session session)
        {
            IMAP_eArgs_GetQuota eArgs = new IMAP_eArgs_GetQuota(session);
            if (GetUserQuota != null)
            {
                GetUserQuota(session, eArgs);
            }

            return eArgs;
        }

        #endregion

        /// <summary>
        /// Raises event GetMessageItems.
        /// </summary>
        /// <param name="session">Reference to IMAP session.</param>
        /// <param name="messageInfo">Message info what message items to get.</param>
        /// <param name="messageItems">Specifies message items what must be filled.</param>
        /// <returns></returns>
        protected internal IMAP_eArgs_MessageItems OnGetMessageItems(IMAP_Session session,
                                                                     IMAP_Message messageInfo,
                                                                     IMAP_MessageItems_enum messageItems)
        {
            IMAP_eArgs_MessageItems eArgs = new IMAP_eArgs_MessageItems(session, messageInfo, messageItems);
            if (GetMessageItems != null)
            {
                GetMessageItems(session, eArgs);
            }
            return eArgs;
        }

        public event FiletSetDelegate SetFilterSet = null;
        public event FiletGetDelegate GetFilterSet = null;

        public byte[] GetFilter(IMAP_Session session)
        {
            if (GetFilterSet != null)
            {
                return GetFilterSet(session);
            }
            return null;
        }

        public void SetFilter(IMAP_Session session, byte[] filter)
        {
            if (SetFilterSet != null)
            {
                SetFilterSet(session, filter);
            }
        }

        public void OnSysError(string text, Exception exception)
        {
           OnError(exception);
        }
    }
}