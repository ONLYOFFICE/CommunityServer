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

namespace ASC.Mail.Net.POP3.Server
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
    /// Represents the method that will handle the AuthUser event for POP3_Server.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">A AuthUser_EventArgs that contains the event data.</param>
    public delegate void AuthUserEventHandler(object sender, AuthUser_EventArgs e);

    /// <summary>
    /// Represents the method that will handle the GetMessgesList event for POP3_Server.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">A GetMessagesInfo_EventArgs that contains the event data.</param>
    public delegate void GetMessagesInfoHandler(object sender, GetMessagesInfo_EventArgs e);

    /// <summary>
    /// Represents the method that will handle the GetMessage,DeleteMessage,GetTopLines event for POP3_Server.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">A GetMessage_EventArgs that contains the event data.</param>
    public delegate void MessageHandler(object sender, POP3_Message_EventArgs e);

    /// <summary>
    /// Represents the method that will handle the GetMessageStream event for POP3_Server.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">Event data.</param>
    public delegate void GetMessageStreamHandler(object sender, POP3_eArgs_GetMessageStream e);

    #endregion

    /// <summary>
    /// POP3 server component.
    /// </summary>
    public class POP3_Server : TCP_Server<POP3_Session>
    {
        #region Events

        /// <summary>
        /// Occurs when connected user tryes to authenticate.
        /// </summary>
        public event AuthUserEventHandler AuthUser = null;

        /// <summary>
        /// Occurs when user requests delete message.
        /// </summary>		
        public event MessageHandler DeleteMessage = null;

        /// <summary>
        /// Occurs when user requests to get specified message.
        /// </summary>
        public event GetMessageStreamHandler GetMessageStream = null;

        /// <summary>
        /// Occurs when server needs to know logged in user's maibox messages.
        /// </summary>
        public event GetMessagesInfoHandler GetMessgesList = null;

        /// <summary>
        /// Occurs when user requests specified message TOP lines.
        /// </summary>
        public event MessageHandler GetTopLines = null;

        /// <summary>
        /// Occurs user session ends. This is place for clean up.
        /// </summary>
        public event EventHandler SessionEnd = null;

        /// <summary>
        /// Occurs when POP3 session has finished and session log is available.
        /// </summary>
        public event LogEventHandler SessionLog = null;

        /// <summary>
        /// Occurs user session resetted. Messages marked for deletion are unmarked.
        /// </summary>
        public event EventHandler SessionResetted = null;

        /// <summary>
        /// Occurs when new computer connected to POP3 server.
        /// </summary>
        public event ValidateIPHandler ValidateIPAddress = null;

        #endregion

        #region Members

        private string m_GreetingText = "";

        private int m_MaxConnectionsPerIP;
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

        public int MaxBadCommands { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Defalut constructor.
        /// </summary>
        public POP3_Server()
        {
            
        }

        #endregion

        #region Overrides


        #endregion

        #region Virtual methods


        protected override void OnMaxConnectionsPerIPExceeded(POP3_Session session)
        {
            base.OnMaxConnectionsPerIPExceeded(session);
            session.TcpStream.WriteLine("-ERR Maximum connections from your IP address is exceeded, try again later!");
        }

        protected override void OnMaxConnectionsExceeded(POP3_Session session)
        {
            session.TcpStream.WriteLine("-ERR Maximum connections exceeded, try again later!");
        }
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
        internal virtual AuthUser_EventArgs OnAuthUser(POP3_Session session,
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

            return oArg;
        }

        /// <summary>
        /// Gest pop3 messages info.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="messages"></param>
        internal virtual void OnGetMessagesInfo(POP3_Session session, POP3_MessageCollection messages)
        {
            GetMessagesInfo_EventArgs oArg = new GetMessagesInfo_EventArgs(session, messages, session.UserName);
            if (GetMessgesList != null)
            {
                GetMessgesList(this, oArg);
            }
        }

        /// <summary>
        /// Raises delete message event.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message">Message which to delete.</param>
        /// <returns></returns>
        internal virtual bool OnDeleteMessage(POP3_Session session, POP3_Message message)
        {
            POP3_Message_EventArgs oArg = new POP3_Message_EventArgs(session, message, null);
            if (DeleteMessage != null)
            {
                DeleteMessage(this, oArg);
            }

            return true;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Checks if user is logged in.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns></returns>
        internal bool IsUserLoggedIn(string userName)
        {
            lock (Sessions)
            {
                foreach (POP3_Session sess in Sessions)
                {
                    if (sess.AuthenticatedUserIdentity!=null && sess.AuthenticatedUserIdentity.Name.ToLower() == userName.ToLower())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Raises event 'GetMessageStream'.
        /// </summary>
        /// <param name="session">Reference to POP3 session.</param>
        /// <param name="messageInfo">Message info what message stream to get.</param>
        /// <returns></returns>
        internal POP3_eArgs_GetMessageStream OnGetMessageStream(POP3_Session session, POP3_Message messageInfo)
        {
            POP3_eArgs_GetMessageStream eArgs = new POP3_eArgs_GetMessageStream(session, messageInfo);
            if (GetMessageStream != null)
            {
                GetMessageStream(this, eArgs);
            }
            return eArgs;
        }

        /// <summary>
        /// Raises event GetTopLines.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message">Message wich top lines to get.</param>
        /// <param name="nLines">Header + number of body lines to get.</param>
        /// <returns></returns>
        internal byte[] OnGetTopLines(POP3_Session session, POP3_Message message, int nLines)
        {
            POP3_Message_EventArgs oArgs = new POP3_Message_EventArgs(session, message, null, nLines);
            if (GetTopLines != null)
            {
                GetTopLines(this, oArgs);
            }
            return oArgs.MessageData;
        }

        /// <summary>
        /// Raises SessionEnd event.
        /// </summary>
        /// <param name="session">Session which is ended.</param>
        internal void OnSessionEnd(object session)
        {
            if (SessionEnd != null)
            {
                SessionEnd(session, new EventArgs());
            }
        }

        /// <summary>
        /// Raises SessionResetted event.
        /// </summary>
        /// <param name="session">Session which is resetted.</param>
        internal void OnSessionResetted(object session)
        {
            if (SessionResetted != null)
            {
                SessionResetted(session, new EventArgs());
            }
        }

        #endregion

        public void OnSysError(string s, Exception exception)
        {
            OnError(exception);
        }
    }
}