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


using System;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

using LumiSoft.Net;
using LumiSoft.Net.AUTH;

namespace LumiSoft.Net.SMTP.Server
{	
	#region Event delegates

	/// <summary>
	/// Represents the method that will handle the AuthUser event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A AuthUser_EventArgs that contains the event data.</param>
	public delegate void AuthUserEventHandler(object sender,AuthUser_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the ValidateMailFrom event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A ValidateSender_EventArgs that contains the event data.</param>
	public delegate void ValidateMailFromHandler(object sender,ValidateSender_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the ValidateMailTo event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A ValidateRecipient_EventArgs that contains the event data.</param>
	public delegate void ValidateMailToHandler(object sender,ValidateRecipient_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the ValidateMailboxSize event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A ValidateMailboxSize_EventArgs that contains the event data.</param>
	public delegate void ValidateMailboxSize(object sender,ValidateMailboxSize_EventArgs e);

    /// <summary>
	/// Represents the method that will handle the GetMessageStoreStream event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A GetMessageStoreStream_eArgs that contains the event data.</param>
	public delegate void GetMessageStoreStreamHandler(object sender,GetMessageStoreStream_eArgs e);

    /// <summary>
	/// Represents the method that will handle the MessageStoringCompleted event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A MessageStoringCompleted_eArgs that contains the event data.</param>
	public delegate void MessageStoringCompletedHandler(object sender,MessageStoringCompleted_eArgs e);

	#endregion
	
	/// <summary>
	/// SMTP server component.
	/// </summary>
	public class SMTP_Server : SocketServer
	{				        
        private int           m_MaxConnectionsPerIP = 0;
		private int           m_MaxMessageSize      = 1000000; 
		private int           m_MaxRecipients       = 100; 
		private SaslAuthTypes m_SupportedAuth       = SaslAuthTypes.All;
		private string        m_GreetingText        = "";
      
		#region Event declarations

		/// <summary>
		/// Occurs when new computer connected to SMTP server.
		/// </summary>
		public event ValidateIPHandler ValidateIPAddress = null;

		/// <summary>
		/// Occurs when connected user tryes to authenticate.
		/// </summary>
		public event AuthUserEventHandler AuthUser = null;

		/// <summary>
		/// Occurs when server needs to validate sender.
		/// </summary>
		public event ValidateMailFromHandler ValidateMailFrom = null;

		/// <summary>
		/// Occurs when server needs to validate recipient.
		/// </summary>
		public event ValidateMailToHandler ValidateMailTo = null;

		/// <summary>
		/// Occurs when server needs to validate recipient mailbox size.
		/// </summary>
		public event ValidateMailboxSize ValidateMailboxSize = null;
                
        /// <summary>
        /// Occurs when server needs to get stream where to store incoming message.
        /// </summary>
        public event GetMessageStoreStreamHandler GetMessageStoreStream = null;

        /// <summary>
        /// Occurs when server has finished message storing.
        /// </summary>
        public event MessageStoringCompletedHandler MessageStoringCompleted = null;

		/// <summary>
		/// Occurs when SMTP session log is available.
		/// </summary>
		public event LogEventHandler SessionLog = null;

		#endregion


		/// <summary>
		/// Defalut constructor.
		/// </summary>
		public SMTP_Server() : base()
		{
			this.BindInfo = new IPBindInfo[]{new IPBindInfo("",IPAddress.Any,25,SslMode.None,null)};
		}


		#region override InitNewSession

		/// <summary>
		/// Initialize and start new session here. Session isn't added to session list automatically, 
		/// session must add itself to server session list by calling AddSession().
		/// </summary>
		/// <param name="socket">Connected client socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
		protected override void InitNewSession(Socket socket,IPBindInfo bindInfo)
		{
            // Check maximum conncurent connections from 1 IP.
            if(m_MaxConnectionsPerIP > 0){
                lock(this.Sessions){
                    int nSessions = 0;
                    foreach(SocketServerSession s in this.Sessions){
                        IPEndPoint ipEndpoint = s.RemoteEndPoint;
                        if(ipEndpoint != null){
                            if(ipEndpoint.Address.Equals(((IPEndPoint)socket.RemoteEndPoint).Address)){
                                nSessions++;
                            }
                        }

                        // Maimum allowed exceeded
                        if(nSessions >= m_MaxConnectionsPerIP){
                            socket.Send(System.Text.Encoding.ASCII.GetBytes("421 Maximum connections from your IP address is exceeded, try again later !\r\n"));
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            return;
                        }
                    }
                }
            }

            string   sessionID = Guid.NewGuid().ToString();
            SocketEx socketEx  = new SocketEx(socket);
            if(LogCommands){
                socketEx.Logger = new SocketLogger(socket,this.SessionLog);
				socketEx.Logger.SessionID = sessionID;
            }
			SMTP_Session session = new SMTP_Session(sessionID,socketEx,bindInfo,this);
		}

		#endregion


		#region Properties Implementaion
		
		/// <summary>
		/// Gets or sets server greeting text.
		/// </summary>
		public string GreetingText
		{
			get{ return m_GreetingText; }

			set{ m_GreetingText = value; }
		}

        /// <summary>
		/// Gets or sets maximum allowed conncurent connections from 1 IP address. Value 0 means unlimited connections.
		/// </summary>
		public int MaxConnectionsPerIP
		{
			get{ return m_MaxConnectionsPerIP; }

			set{ m_MaxConnectionsPerIP = value; }
		}

		/// <summary>
		/// Maximum message size in bytes.
		/// </summary>
		public int MaxMessageSize 
		{
			get{ return m_MaxMessageSize; }

			set{ m_MaxMessageSize = value; }
		}

		/// <summary>
		/// Maximum recipients per message.
		/// </summary>
		public int MaxRecipients
		{
			get{ return m_MaxRecipients; }

			set{ m_MaxRecipients = value; }
		}

		/// <summary>
		/// Gets or sets server supported authentication types.
		/// </summary>
		public SaslAuthTypes SupportedAuthentications
		{
			get{ return m_SupportedAuth; }

			set{ m_SupportedAuth = value; }
		}

        /// <summary>
		/// Gets active sessions.
		/// </summary>
		public new SMTP_Session[] Sessions
		{
			get{
                SocketServerSession[] sessions     = base.Sessions;
                SMTP_Session[]        smtpSessions = new SMTP_Session[sessions.Length];
                sessions.CopyTo(smtpSessions,0);

                return smtpSessions; 
            }
		}
		
		#endregion

		#region Events Implementation

		#region method OnValidate_IpAddress
		
		/// <summary>
		/// Raises event ValidateIP event.
		/// </summary>
		/// <param name="session">Reference to current smtp session.</param>
		internal ValidateIP_EventArgs OnValidate_IpAddress(SMTP_Session session) 
		{	
			ValidateIP_EventArgs oArg = new ValidateIP_EventArgs(session.LocalEndPoint,session.RemoteEndPoint);
			if(this.ValidateIPAddress != null){
				this.ValidateIPAddress(this, oArg);
			}

			session.Tag = oArg.SessionTag;

			return oArg;						
		}

		#endregion

		#region method OnAuthUser

		/// <summary>
		/// Raises event AuthUser.
		/// </summary>
		/// <param name="session">Reference to current smtp session.</param>
		/// <param name="userName">User name.</param>
		/// <param name="passwordData">Password compare data,it depends of authentication type.</param>
		/// <param name="data">For md5 eg. md5 calculation hash.It depends of authentication type.</param>
		/// <param name="authType">Authentication type.</param>
		/// <returns></returns>
		internal AuthUser_EventArgs OnAuthUser(SMTP_Session session,string userName,string passwordData,string data,AuthType authType)
		{
			AuthUser_EventArgs oArgs = new AuthUser_EventArgs(session,userName,passwordData,data,authType);
			if(this.AuthUser != null){
				this.AuthUser(this,oArgs);
			}

			return oArgs;
		}

		#endregion

		#region method OnValidate_MailFrom

		/// <summary>
		/// Raises event ValidateMailFrom.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="reverse_path"></param>
		/// <param name="email"></param>
		/// <returns></returns>
		internal ValidateSender_EventArgs OnValidate_MailFrom(SMTP_Session session,string reverse_path,string email) 
		{	
			ValidateSender_EventArgs oArg = new ValidateSender_EventArgs(session,email);
			if(this.ValidateMailFrom != null){
				this.ValidateMailFrom(this, oArg);
			}

			return oArg;						
		}

		#endregion

		#region method OnValidate_MailTo

		/// <summary>
		/// Raises event ValidateMailTo.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="forward_path"></param>
		/// <param name="email"></param>
		/// <param name="authenticated"></param>
		/// <returns></returns>
		internal ValidateRecipient_EventArgs OnValidate_MailTo(SMTP_Session session,string forward_path,string email,bool authenticated) 
		{	
			ValidateRecipient_EventArgs oArg = new ValidateRecipient_EventArgs(session,email,authenticated);
			if(this.ValidateMailTo != null){
				this.ValidateMailTo(this, oArg);
			}

			return oArg;						
		}

		#endregion

		#region method Validate_MailBoxSize

		/// <summary>
		/// Raises event ValidateMailboxSize.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="eAddress"></param>
		/// <param name="messageSize"></param>
		/// <returns></returns>
		internal bool Validate_MailBoxSize(SMTP_Session session,string eAddress,long messageSize)
		{
			ValidateMailboxSize_EventArgs oArgs = new ValidateMailboxSize_EventArgs(session,eAddress,messageSize);
			if(this.ValidateMailboxSize != null){
				this.ValidateMailboxSize(this,oArgs);
			}

			return oArgs.IsValid;
		}

		#endregion


        #region method OnGetMessageStoreStream

        /// <summary>
        /// Raises event GetMessageStoreStream.
        /// </summary>
        /// <param name="session">Reference to calling SMTP session.</param>
        /// <returns></returns>
        internal GetMessageStoreStream_eArgs OnGetMessageStoreStream(SMTP_Session session)
        {
            GetMessageStoreStream_eArgs eArgs = new GetMessageStoreStream_eArgs(session);
            if(this.GetMessageStoreStream != null){
                this.GetMessageStoreStream(this,eArgs);
            }
            return eArgs;
        }

        #endregion

        #region method OnMessageStoringCompleted

        /// <summary>
        /// Raises event MessageStoringCompleted.
        /// </summary>
        /// <param name="session">Reference to calling SMTP session.</param>
        /// <param name="errorText">Null if no errors, otherwise conatians error text. If errors happened that means that messageStream is incomplete.</param>
        /// <param name="messageStream">Stream where message was stored.</param>
        internal MessageStoringCompleted_eArgs OnMessageStoringCompleted(SMTP_Session session,string errorText,Stream messageStream)
        {            
            MessageStoringCompleted_eArgs eArgs = new MessageStoringCompleted_eArgs(session,errorText,messageStream);
            if(this.MessageStoringCompleted != null){
                this.MessageStoringCompleted(this,eArgs);
            }

            return eArgs;
        }

        #endregion

		#endregion
		
	}
}
