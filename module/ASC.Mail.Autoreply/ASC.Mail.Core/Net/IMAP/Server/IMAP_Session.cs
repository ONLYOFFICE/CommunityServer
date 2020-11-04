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


using System.Collections.Generic;
using System.Security.Principal;
using ASC.Mail.Net.IO;

namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using System.Collections;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Timers;
    using AUTH;
    using Mime;
    using StringReader = StringReader;
    using ASC.Mail.Net.TCP;

    #endregion

    /// <summary>
    /// IMAP session.
    /// </summary>
    public class IMAP_Session : TCP_ServerSession
    {
        #region Nested type: Command_IDLE

        /// <summary>
        /// This class implements IDLE command. Defined in RFC 2177.
        /// </summary>
        private class Command_IDLE: IDisposable
        {
            #region Members

            private readonly string m_CmdTag = "";
            private readonly string folder;
            private bool m_IsDisposed;
            private IMAP_Session m_pSession;
            private TimerEx m_pTimer;

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="session">Owner IMAP session.</param>
            /// <param name="cmdTag">IDLE command command-tag.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>session</b> or <b>cmdTag</b> is null reference.</exception>
            /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
            public Command_IDLE(IMAP_Session session, string cmdTag) : this(session, cmdTag, session.SelectedMailbox) { }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="session">Owner IMAP session.</param>
            /// <param name="cmdTag">IDLE command command-tag.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>session</b> or <b>cmdTag</b> is null reference.</exception>
            /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
            public Command_IDLE(IMAP_Session session, string cmdTag, string folder)
            {
                if (session == null)
                {
                    throw new ArgumentNullException("session");
                }
                if (cmdTag == null)
                {
                    throw new ArgumentNullException("cmdTag");
                }
                if (cmdTag == string.Empty)
                {
                    throw new ArgumentException("Argument 'cmdTag' value must be specified.", "cmdTag");
                }

                try
                {
                    m_pSession = session;
                    m_CmdTag = cmdTag;
                    this.folder = folder;
                    Start();
                }
                catch (Exception x)
                {
                    m_pSession.OnError(x);   
                }
            }

            void m_pSession_FolderChanged(string folderName, string username)
            {
                //tada
                if (!m_IsDisposed)
                    m_pSession.ProcessMailboxChanges(folder);
            }

            #endregion

            #region Methods

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            /// 
            public void Dispose()
            {
                if (m_IsDisposed)
                {
                    return;
                }
                m_IsDisposed = true;
                try
                {
                    m_pTimer.Stop();
                    m_pSession.FolderChanged -= m_pSession_FolderChanged;
                    m_pSession.m_pIDLE = null;
                    m_pSession = null;
                    m_pTimer.Dispose();
                    m_pTimer = null;

                }
                catch (Exception)
                {
                    
                }
            }

            #endregion

            #region Event handlers

            private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                // If mailbox changes, report them to connected client.
                if (!m_IsDisposed)
                    m_pSession.ProcessMailboxChanges(folder);
            }

            #endregion

            #region Utility methods

            private void ReadLineCompleted(object sender, EventArgs<SmartStream.ReadLineAsyncOP> eventArgs)
            {
                try
                {
                    // Accoridng RFC, we should get only "DONE" here.
                    if (m_IsDisposed)
                        return;

                    if (eventArgs.Value.IsCompleted && eventArgs.Value.Error == null)
                    {
                        if (eventArgs.Value.LineUtf8 ==
                            "DONE")
                        {
                            // Send "cmd-tag OK IDLE terminated" to connected client.
                            m_pSession.WriteLine(m_CmdTag + " OK IDLE terminated");

                            m_pSession.BeginRecieveCmd();

                            Dispose();
                        }
                        // Connected client send illegal stuff us, end session.
                        else
                        {
                            m_pSession.EndSession();

                            Dispose();
                        }
                    }
                    // Receive errors, probably TCP connection broken.
                    else
                    {
                        m_pSession.EndSession();

                        Dispose();
                    }

                }
                catch (Exception)
                {
                    
                }
            }

            /// <summary>
            /// Starts IDLE command processing.
            /// </summary>
            private void Start()
            {
                /* RFC 2177 IDLE command example.
                    C: A004 IDLE
                    S: * 2 EXPUNGE
                    S: * 3 EXISTS
                    S: + idling
                    ...time passes; another client expunges message 3...
                    S: * 3 EXPUNGE
                    S: * 2 EXISTS
                    ...time passes; new mail arrives...
                    S: * 3 EXISTS
                    C: DONE
                    S: A004 OK IDLE terminated
                */

                // Send status reponse to connected client if any.
                m_pSession.ProcessMailboxChanges(folder);

                // Send "+ idling" to connected client.
                m_pSession.WriteLine("+ idling");


                // Start timer to poll mailbox changes.
                m_pSession.FolderChanged += m_pSession_FolderChanged;

                m_pTimer = new TimerEx(5000, true);
                m_pTimer.Elapsed += m_pTimer_Elapsed;
                m_pTimer.Enabled = true;

                // Start waiting DONE command from connected client.
                m_pSession.ReadAsync(ReadLineCompleted);
            }

            #endregion
        }

        #endregion

        #region Members

        private IMAP_Server ImapServer { get { return Server as IMAP_Server; } }
        private Command_IDLE m_pIDLE;
        private IMAP_SelectedFolder m_pSelectedFolder;
        private IMAP_SelectedFolder m_pAdditionalFolder;
        private string m_StatusedMailbox = "";
        private int m_BadCmdCount;
        private GenericIdentity m_pUser;

        public IMAP_Session()
        {
            SelectedMailbox = "";
        }

        public override void Dispose()
        {
            base.Dispose();
            if (!IsDisposed)
            {
                if (m_pIDLE!=null)
                {
                    m_pIDLE.Dispose();
                }
                if (m_pSelectedFolder!=null)
                {
                    m_pSelectedFolder.Dispose();
                }
                if (m_pAdditionalFolder != null)
                {
                    m_pAdditionalFolder.Dispose();
                }
            }
        }

        internal event IMAP_Server.FolderChangedHandler FolderChanged = null;

        public void OnFolderChanged(string folderName, string username)
        {
            if (FolderChanged != null)
            {
                FolderChanged(folderName, username);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets selected mailbox.
        /// </summary>
        public string SelectedMailbox { get; private set; }

        #endregion

        #region Constructor




        #endregion

        #region Methods


        #endregion

        #region Overrides

        protected override void OnTimeout()
        {
            base.OnTimeout();
            WriteLine("* BYE Session timeout");
            TcpStream.Flush();
        }


        #endregion

        #region Utility methods


        protected internal override void Start()
        {
            base.Start();
            try
            {
                ImapServer.FolderChanged += OnFolderChanged;
                // Check if ip is allowed to connect this computer
                if (ImapServer.OnValidate_IpAddress(LocalEndPoint, RemoteEndPoint))
                {
                    WriteLine(string.Format("* OK {0} IMAP Server ready", LocalHostName));
                    BeginRecieveCmd();
                }
                else
                {
                    EndSession();
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Ends session, closes socket.
        /// </summary>
        private void EndSession()
        {
            Disconnect();
        }

        /// <summary>
        /// Is called when error occures.
        /// </summary>
        /// <param name="x"></param>
		private new void OnError(Exception x)
        {
            // Session disposed, so we don't care about terminating errors any more.
            if (IsDisposed)
            {
                return;
            }
            try
            {
                ImapServer.OnSysError("ending session", x);
                EndSession();
            }
            catch (Exception)
            {
                
            }
        }


        private void AddLog(string message)
        {
            // Log
            if (ImapServer.Logger != null)
            {
                ImapServer.Logger.AddText(ID, message);
            }
        }

        private void AddReadEntry(string cmd)
        {
            // Log
            if (ImapServer.Logger != null)
            {
                ImapServer.Logger.AddRead(ID, AuthenticatedUserIdentity, cmd.Length, cmd, LocalEndPoint, RemoteEndPoint);
            }
        }

        private void AddWriteEntry(string cmd)
        {
            // Log
            if (ImapServer.Logger != null)
            {
                ImapServer.Logger.AddWrite(ID, AuthenticatedUserIdentity, cmd.Length, cmd, LocalEndPoint, RemoteEndPoint);
            }
        }

        /// <summary>
        /// Starts recieveing command.
        /// </summary>
        private void BeginRecieveCmd()
        {
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);

            args.Completed += new EventHandler<EventArgs<SmartStream.ReadLineAsyncOP>>(ReciveCmdComleted);
            TcpStream.ReadLine(args, true);
        }

        internal void ReadAsync(EventHandler<EventArgs<SmartStream.ReadLineAsyncOP>> completed)
        {
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);

            args.Completed += completed;
            TcpStream.ReadLine(args, true);
        }

        protected void WriteLine(string line)
        {
            try
            {
                AddWriteEntry(line);
                TcpStream.WriteLine(line);
            }
            catch (Exception x)
            {
                OnError(x);                
            }
        }


        private void ReciveCmdComleted(object sender, EventArgs<SmartStream.ReadLineAsyncOP> e)
        {
            if (e.Value.IsCompleted && e.Value.Error == null)
            {
                //Call
                bool sessionEnd = false;
                try
                {
                    string cmdLine = e.Value.LineUtf8;
                    AddReadEntry(cmdLine);
                    // Exceute command
                    TcpStream.MemoryBuffer = true;
                    sessionEnd = SwitchCommand(cmdLine);
                    TcpStream.Flush();
                    if (sessionEnd)
                    {
                        // Session end, close session
                        EndSession();
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
            else if (e.Value.Error != null)
            {
                OnError(e.Value.Error);
            }
        }

        private void EndSession(string text)
        {
            Disconnect(text);
        }

        /// <summary>
        /// Executes IMAP command.
        /// </summary>
        /// <param name="IMAP_commandTxt">Original command text.</param>
        /// <returns>Returns true if must end session(command loop).</returns>
        private bool SwitchCommand(string IMAP_commandTxt)
        {
            // Parse commandTag + comand + args
            // eg. C: a100 SELECT INBOX<CRLF>
            // a100   - commandTag
            // SELECT - command
            // INBOX  - arg

            //---- Parse command --------------------------------------------------//
            string[] cmdParts = IMAP_commandTxt.TrimStart().Split(new[] { ' ' });
            // For bad command, just return empty cmdTag and command name
            if (cmdParts.Length < 2)
            {
                cmdParts = new[] { "", "" };
            }
            string commandTag = cmdParts[0].Trim().Trim();
            string command = cmdParts[1].ToUpper().Trim();
            string argsText = Core.GetArgsText(IMAP_commandTxt, cmdParts[0] + " " + cmdParts[1]);
            //---------------------------------------------------------------------//

            bool getNextCmd = true;

            switch (command)
            {
                //--- Non-IsAuthenticated State
                case "STARTTLS":
                    STARTTLS(commandTag, argsText);
                    break;

                case "AUTHENTICATE":
                    Authenticate(commandTag, argsText);
                    break;

                case "LOGIN":
                    LogIn(commandTag, argsText);
                    break;
                //--- End of non-IsAuthenticated

                //--- IsAuthenticated State
                case "SELECT":
                    Select(commandTag, argsText);
                    break;

                case "EXAMINE":
                    Examine(commandTag, argsText);
                    break;

                case "CREATE":
                    Create(commandTag, argsText);
                    break;

                case "DELETE":
                    Delete(commandTag, argsText);
                    break;

                case "RENAME":
                    Rename(commandTag, argsText);
                    break;

                case "SUBSCRIBE":
                    Suscribe(commandTag, argsText);
                    break;

                case "UNSUBSCRIBE":
                    UnSuscribe(commandTag, argsText);
                    break;

                case "LIST":
                    List(commandTag, argsText);
                    break;

                case "LSUB":
                    LSub(commandTag, argsText);
                    break;

                case "STATUS":
                    Status(commandTag, argsText);
                    break;

                case "APPEND":
                    getNextCmd = BeginAppendCmd(commandTag, argsText);
                    break;

                case "NAMESPACE":
                    Namespace(commandTag, argsText);
                    break;

                case "GETACL":
                    GETACL(commandTag, argsText);
                    break;

                case "SETACL":
                    SETACL(commandTag, argsText);
                    break;

                case "DELETEACL":
                    DELETEACL(commandTag, argsText);
                    break;

                case "LISTRIGHTS":
                    LISTRIGHTS(commandTag, argsText);
                    break;

                case "MYRIGHTS":
                    MYRIGHTS(commandTag, argsText);
                    break;

                case "GETQUOTA":
                    GETQUOTA(commandTag, argsText);
                    break;

                case "GETQUOTAROOT":
                    GETQUOTAROOT(commandTag, argsText);
                    break;
                //--- End of IsAuthenticated

                //--- Selected State
                case "CHECK":
                    Check(commandTag);
                    break;

                case "CLOSE":
                    Close(commandTag);
                    break;

                case "EXPUNGE":
                    Expunge(commandTag);
                    break;

                case "SEARCH":
                    Search(commandTag, argsText, false);
                    break;

                case "FETCH":
                    Fetch(commandTag, argsText, false);
                    break;

                case "STORE":
                    Store(commandTag, argsText, false);
                    break;

                case "COPY":
                    Copy(commandTag, argsText, false);
                    break;

                case "UID":
                    Uid(commandTag, argsText);
                    break;

                case "IDLE":
                    getNextCmd = !Idle(commandTag, argsText);
                    break;
                //--- End of Selected 

                //--- Any State
                case "CAPABILITY":
                    Capability(commandTag);
                    break;
                case "NOOP":
                    Noop(commandTag);
                    break;

                case "LOGOUT":
                    LogOut(commandTag);
                    return true;
                //--- End of Any

                default:
                    WriteLine(commandTag + " BAD command unrecognized");

                    //---- Check that maximum bad commands count isn't exceeded ---------------//
                    if (m_BadCmdCount > ImapServer.MaxBadCommands - 1)
                    {
                        WriteLine("* BAD Too many bad commands, closing transmission channel");
                        return true;
                    }
                    m_BadCmdCount++;
                    //-------------------------------------------------------------------------//
                    break;
            }

            if (getNextCmd)
            {
                BeginRecieveCmd();
            }

            return false;
        }




        //--- Non-IsAuthenticated State ------

        private void STARTTLS(string cmdTag, string argsText)
        {
            /* RFC 2595 3. IMAP STARTTLS extension.
                3. IMAP STARTTLS extension

                   When the TLS extension is present in IMAP, "STARTTLS" is listed as a
                   capability in response to the CAPABILITY command.  This extension
                   adds a single command, "STARTTLS" to the IMAP protocol which is used
                   to begin a TLS negotiation.

                3.1. STARTTLS Command

                   Arguments:  none

                   Responses:  no specific responses for this command

                   Result:     OK - begin TLS negotiation
                               BAD - command unknown or arguments invalid

                      A TLS negotiation begins immediately after the CRLF at the end of
                      the tagged OK response from the server.  Once a client issues a
                      STARTTLS command, it MUST NOT issue further commands until a
                      server response is seen and the TLS negotiation is complete.

                      The STARTTLS command is only valid in non-authenticated state.
                      The server remains in non-authenticated state, even if client
                      credentials are supplied during the TLS negotiation.  The SASL
                      [SASL] EXTERNAL mechanism MAY be used to authenticate once TLS
                      client credentials are successfully exchanged, but servers
                      supporting the STARTTLS command are not required to support the
                      EXTERNAL mechanism.

                      Once TLS has been started, the client MUST discard cached
                      information about server capabilities and SHOULD re-issue the
                      CAPABILITY command.  This is necessary to protect against
                      man-in-the-middle attacks which alter the capabilities list prior
                      to STARTTLS.  The server MAY advertise different capabilities
                      after STARTTLS.

                      The formal syntax for IMAP is amended as follows:

                        command_any   =/  "STARTTLS"

                   Example:    C: a001 CAPABILITY
                               S: * CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED
                               S: a001 OK CAPABILITY completed
                               C: a002 STARTTLS
                               S: a002 OK Begin TLS negotiation now
                               <TLS negotiation, further commands are under TLS layer>
                               C: a003 CAPABILITY
                               S: * CAPABILITY IMAP4rev1 AUTH=EXTERNAL
                               S: a003 OK CAPABILITY completed
                               C: a004 LOGIN joe password
                               S: a004 OK LOGIN completed
            */

            if (IsAuthenticated)
            {
                WriteLine(cmdTag +
                                 " NO The STARTTLS command is only valid in non-authenticated state !");
                return;
            }
            if (IsSecureConnection)
            {
                WriteLine(cmdTag + " NO The STARTTLS already started !");
                return;
            }
            if (Certificate == null)
            {
                WriteLine(cmdTag + " NO TLS not available, SSL certificate isn't specified !");
                return;
            }

            WriteLine(cmdTag + " OK Ready to start TLS");

            try
            {
                SwitchToSecure();
                AddLog("TLS negotiation completed successfully.");
            }
            catch (Exception x)
            {
                WriteLine(cmdTag + " NO TLS handshake failed ! " + x.Message);
            }
        }

        private void Authenticate(string cmdTag, string argsText)
        {
            /* Rfc 3501 6.2.2.  AUTHENTICATE Command

				Arguments:  authentication mechanism name

				Responses:  continuation data can be requested

				Result:     OK  - authenticate completed, now in authenticated state
							NO  - authenticate failure: unsupported authentication
								  mechanism, credentials rejected
							BAD - command unknown or arguments invalid,
								  authentication exchange cancelled
			*/
            if (IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO AUTH you are already logged in", cmdTag));
                return;
            }

            string userName = "";
            //	string password = "";
            AuthUser_EventArgs aArgs = null;

            switch (argsText.ToUpper())
            {
                case "CRAM-MD5":

                    #region CRAM-MDD5 authentication

                    /* Cram-M5
					C: A0001 AUTH CRAM-MD5
					S: + <md5_calculation_hash_in_base64>
					C: base64(decoded:username password_hash)
					S: A0001 OK CRAM authentication successful
					*/

                    string md5Hash = string.Format("<{0}>", Guid.NewGuid().ToString().ToLower());
                    WriteLine(string.Format("+ {0}", Convert.ToBase64String(Encoding.ASCII.GetBytes(md5Hash))));

                    string reply = ReadLine();
                    reply = Encoding.Default.GetString(Convert.FromBase64String(reply));
                    string[] replyArgs = reply.Split(' ');
                    userName = replyArgs[0];

                    aArgs = ImapServer.OnAuthUser(this, userName, replyArgs[1], md5Hash, AuthType.CRAM_MD5);

                    // There is custom error, return it
                    if (aArgs.ErrorText != null)
                    {
                        WriteLine(string.Format("{0} NO {1}", cmdTag, aArgs.ErrorText));
                        return;
                    }

                    if (aArgs.Validated)
                    {
                        WriteLine(string.Format("{0} OK Authentication successful.", cmdTag));

                        SetUserName(userName);
                    }
                    else
                    {
                        WriteLine(string.Format("{0} NO Authentication failed", cmdTag));
                    }

                    #endregion

                    break;

                case "DIGEST-MD5":

                    #region DIGEST-MD5 authentication

                    /* RFC 2831 AUTH DIGEST-MD5
					 * 
					 * Example:
					 * 
					 * C: AUTH DIGEST-MD5
					 * S: + base64(realm="elwood.innosoft.com",nonce="OA6MG9tEQGm2hh",qop="auth",algorithm=md5-sess)
					 * C: base64(username="chris",realm="elwood.innosoft.com",nonce="OA6MG9tEQGm2hh",
					 *    nc=00000001,cnonce="OA6MHXh6VqTrRk",digest-uri="imap/elwood.innosoft.com",
                     *    response=d388dad90d4bbd760a152321f2143af7,qop=auth)
					 * S: + base64(rspauth=ea40f60335c427b5527b84dbabcdfffd)
					 * C:
					 * S: A0001 OK Authentication successful.
					*/

                    string realm = LocalHostName;
                    string nonce = AuthHelper.GenerateNonce();

                    WriteLine("+ " +
                                     AuthHelper.Base64en(AuthHelper.Create_Digest_Md5_ServerResponse(realm,
                                                                                                     nonce)));

                    string clientResponse = AuthHelper.Base64de(ReadLine());
                    // Check that realm and nonce in client response are same as we specified
                    if (clientResponse.IndexOf(string.Format("realm=\"{0}\"", realm)) > -1 &&
                        clientResponse.IndexOf(string.Format("nonce=\"{0}\"", nonce)) > -1)
                    {
                        // Parse user name and password compare value
                        //		string userName  = "";
                        string passwData = "";
                        string cnonce = "";
                        foreach (string clntRespParam in clientResponse.Split(','))
                        {
                            if (clntRespParam.StartsWith("username="))
                            {
                                userName = clntRespParam.Split(new[] { '=' }, 2)[1].Replace("\"", "");
                            }
                            else if (clntRespParam.StartsWith("response="))
                            {
                                passwData = clntRespParam.Split(new[] { '=' }, 2)[1];
                            }
                            else if (clntRespParam.StartsWith("cnonce="))
                            {
                                cnonce = clntRespParam.Split(new[] { '=' }, 2)[1].Replace("\"", "");
                            }
                        }

                        aArgs = ImapServer.OnAuthUser(this,
                                                     userName,
                                                     passwData,
                                                     clientResponse,
                                                     AuthType.DIGEST_MD5);

                        // There is custom error, return it
                        if (aArgs.ErrorText != null)
                        {
                            WriteLine(string.Format("{0} NO {1}", cmdTag, aArgs.ErrorText));
                            return;
                        }

                        if (aArgs.Validated)
                        {
                            // Send server computed password hash
                            WriteLine("+ " + AuthHelper.Base64en("rspauth=" + aArgs.ReturnData));

                            // We must got empty line here
                            clientResponse = ReadLine();
                            if (clientResponse == "")
                            {
                                WriteLine(string.Format("{0} OK Authentication successful.", cmdTag));

                                SetUserName(userName);
                            }
                            else
                            {
                                WriteLine(string.Format("{0} NO Authentication failed", cmdTag));
                            }
                        }
                        else
                        {
                            WriteLine(string.Format("{0} NO Authentication failed", cmdTag));
                        }
                    }
                    else
                    {
                        WriteLine(string.Format("{0} NO Authentication failed", cmdTag));
                    }

                    #endregion

                    break;

                default:
                    WriteLine(string.Format("{0} NO unsupported authentication mechanism", cmdTag));
                    break;
            }
        }

        private void LogIn(string cmdTag, string argsText)
        {
            /* RFC 3501 6.2.3 LOGIN Command
			  
				Arguments:  user name
							password

				Responses:  no specific responses for this command

				Result:     OK  - login completed, now in authenticated state
							NO  - login failure: user name or password rejected
							BAD - command unknown or arguments invalid
			   
				The LOGIN command identifies the client to the server and carries
				the plaintext password authenticating this user.
			
				Example: C: a001 LOGIN SMITH SESAME
					     S: a001 OK LOGIN completed
						 
						 //----
						 C: a001 LOGIN "SMITH" "SESAME"
						 S: a001 OK LOGIN completed
			   
			*/
            /*if (IsAuthenticated)
            {
                WriteLine(cmdTag + " NO Reauthentication error, you are already logged in");
                return;
            }*/

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD Invalid arguments, syntax: {{<command-tag> LOGIN \"<user-name>\" \"<password>\"}}", cmdTag));
                return;
            }

            string userName = args[0];
            string password = args[1];

            // Store start time
            long startTime = DateTime.Now.Ticks;

            AuthUser_EventArgs aArgs = ImapServer.OnAuthUser(this, userName, password, "", AuthType.Plain);
            // There is custom error, return it
            if (aArgs.ErrorText != null)
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, aArgs.ErrorText));
                return;
            }

            if (aArgs.Validated)
            {
                WriteLine(string.Format("{0} OK LOGIN Completed in {1} seconds", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2")));

                SetUserName(userName);
            }
            else
            {
                WriteLine(string.Format("{0} NO LOGIN failed", cmdTag));
            }
        }

        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pUser;
            }
        }

        private void SetUserName(string userName)
        {
            UserName = userName;
            m_pUser = new GenericIdentity(UserName);
            
        }

        public string UserName { get; set; }

        //--- End of non-IsAuthenticated State 

        //--- IsAuthenticated State ------

        private void Select(string cmdTag, string argsText)
        {
            /* Rfc 3501 6.3.1 SELECT Command
			 
				Arguments:  mailbox name

				Responses:  REQUIRED untagged responses: FLAGS, EXISTS, RECENT
							REQUIRED OK untagged responses:  UNSEEN,  PERMANENTFLAGS,
							UIDNEXT, UIDVALIDITY

				Result:     OK - select completed, now in selected state
							NO - select failure, now in authenticated state: no
									such mailbox, can't access mailbox
							BAD - command unknown or arguments invalid
							
				The SELECT command selects a mailbox so that messages in the
				mailbox can be accessed.  Before returning an OK to the client,
				the server MUST send the following untagged data to the client.
				Note that earlier versions of this protocol only required the
				FLAGS, EXISTS, and RECENT untagged data; consequently, client
				implementations SHOULD implement default behavior for missing data
				as discussed with the individual item.

					FLAGS       Defined flags in the mailbox.  See the description
								of the FLAGS response for more detail.

					<n> EXISTS  The number of messages in the mailbox.  See the
								description of the EXISTS response for more detail.

					<n> RECENT  The number of messages with the \Recent flag set.
								See the description of the RECENT response for more
								detail.

					OK [UNSEEN <n>]
								The message sequence number of the first unseen
								message in the mailbox.  If this is missing, the
								client can not make any assumptions about the first
								unseen message in the mailbox, and needs to issue a
								SEARCH command if it wants to find it.

					OK [PERMANENTFLAGS (<list of flags>)]
								A list of message flags that the client can change
								permanently.  If this is missing, the client should
								assume that all flags can be changed permanently.

					OK [UIDNEXT <n>]
								The next unique identifier value.  Refer to section
								2.3.1.1 for more information.  If this is missing,
								the client can not make any assumptions about the
								next unique identifier value.
								
					OK [UIDVALIDITY <n>]
                     The unique identifier validity value.  Refer to
                     section 2.3.1.1 for more information.  If this is
                     missing, the server does not support unique
                     identifiers.

				Only one mailbox can be selected at a time in a connection;
				simultaneous access to multiple mailboxes requires multiple
				connections.  The SELECT command automatically deselects any
				currently selected mailbox before attempting the new selection.
				Consequently, if a mailbox is selected and a SELECT command that
				fails is attempted, no mailbox is selected.

				If the client is permitted to modify the mailbox, the server
				SHOULD prefix the text of the tagged OK response with the
				"[READ-WRITE]" response code.

				If the client is not permitted to modify the mailbox but is
				permitted read access, the mailbox is selected as read-only, and
				the server MUST prefix the text of the tagged OK response to
				SELECT with the "[READ-ONLY]" response code.  Read-only access
				through SELECT differs from the EXAMINE command in that certain
				read-only mailboxes MAY permit the change of permanent state on a
				per-user (as opposed to global) basis.  Netnews messages marked in
				a server-based .newsrc file are an example of such per-user
				permanent state that can be modified with read-only mailboxes.

				Example:    C: A142 SELECT INBOX
							S: * 172 EXISTS
							S: * 1 RECENT
							S: * OK [UNSEEN 12] Message 12 is first unseen
							S: * OK [UIDVALIDITY 3857529045] UIDs valid
							S: * OK [UIDNEXT 4392] Predicted next UID
							S: * FLAGS (\Answered \Flagged \Deleted \Seen \Draft)
							S: * OK [PERMANENTFLAGS (\Deleted \Seen \*)] Limited
							S: A142 OK [READ-WRITE] SELECT completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD SELECT invalid arguments. Syntax: {{<command-tag> SELECT \"mailboxName\"}}", cmdTag));
                return;
            }

            // Store start time
            long startTime = DateTime.Now.Ticks;

            IMAP_SelectedFolder selectedFolder = new IMAP_SelectedFolder(Core.Decode_IMAP_UTF7_String(args[0]));
            IMAP_eArgs_GetMessagesInfo eArgs = ImapServer.OnGetMessagesInfo(this, selectedFolder);
            if (eArgs.ErrorText == null)
            {
                m_pSelectedFolder = selectedFolder;
                SelectedMailbox = Core.Decode_IMAP_UTF7_String(args[0]);

                string response = "";
                response += "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n";
                response += string.Format("* {0} EXISTS\r\n", m_pSelectedFolder.Messages.Count);
                response += string.Format("* {0} RECENT\r\n", m_pSelectedFolder.RecentCount);
                response += string.Format("* OK [UNSEEN {0}] Message {1} is first unseen\r\n", m_pSelectedFolder.FirstUnseen, m_pSelectedFolder.FirstUnseen);
                response += string.Format("* OK [UIDVALIDITY {0}] Folder UID\r\n", m_pSelectedFolder.FolderUID);
                response += string.Format("* OK [UIDNEXT {0}] Predicted next message UID\r\n", m_pSelectedFolder.MessageUidNext);
                response +=
                    "* OK [PERMANENTFLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)] Available permanent flags\r\n";
                response += string.Format("{0} OK [{1}] SELECT Completed in {2} seconds\r\n", cmdTag, (m_pSelectedFolder.ReadOnly ? "READ-ONLY" : "READ-WRITE"), ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2"));

                TcpStream.Write(response);
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, eArgs.ErrorText));
            }
        }

        private void Examine(string cmdTag, string argsText)
        {
            /* Rfc 3501 6.3.2 EXAMINE Command
			
				Arguments:  mailbox name

				Responses:  REQUIRED untagged responses: FLAGS, EXISTS, RECENT
							REQUIRED OK untagged responses:  UNSEEN,  PERMANENTFLAGS,
							UIDNEXT, UIDVALIDITY

				Result:     OK - examine completed, now in selected state
							NO - examine failure, now in authenticated state: no
									such mailbox, can't access mailbox
							BAD - command unknown or arguments invalid

				The EXAMINE command is identical to SELECT and returns the same
				output; however, the selected mailbox is identified as read-only.
				No changes to the permanent state of the mailbox, including
				per-user state, are permitted; in particular, EXAMINE MUST NOT
				cause messages to lose the \Recent flag.

				The text of the tagged OK response to the EXAMINE command MUST
				begin with the "[READ-ONLY]" response code.

				Example:    C: A932 EXAMINE blurdybloop
							S: * 17 EXISTS
							S: * 2 RECENT
							S: * OK [UNSEEN 8] Message 8 is first unseen
							S: * OK [UIDVALIDITY 3857529045] UIDs valid
							S: * OK [UIDNEXT 4392] Predicted next UID
							S: * FLAGS (\Answered \Flagged \Deleted \Seen \Draft)
							S: * OK [PERMANENTFLAGS ()] No permanent flags permitted
							S: A932 OK [READ-ONLY] EXAMINE completed
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD EXAMINE invalid arguments. Syntax: {{<command-tag> EXAMINE \"mailboxName\"}}", cmdTag));
                return;
            }

            // Store start time
            long startTime = DateTime.Now.Ticks;

            IMAP_SelectedFolder selectedFolder = new IMAP_SelectedFolder(Core.Decode_IMAP_UTF7_String(args[0]));
            IMAP_eArgs_GetMessagesInfo eArgs = ImapServer.OnGetMessagesInfo(this, selectedFolder);
            if (eArgs.ErrorText == null)
            {
                m_pSelectedFolder = selectedFolder;
                m_pSelectedFolder.ReadOnly = true;
                SelectedMailbox = Core.Decode_IMAP_UTF7_String(args[0]);

                string response = "";
                response += "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n";
                response += string.Format("* {0} EXISTS\r\n", m_pSelectedFolder.Messages.Count);
                response += string.Format("* {0} RECENT\r\n", m_pSelectedFolder.RecentCount);
                response += string.Format("* OK [UNSEEN {0}] Message {1} is first unseen\r\n", m_pSelectedFolder.FirstUnseen, m_pSelectedFolder.FirstUnseen);
                response += string.Format("* OK [UIDVALIDITY {0}] UIDs valid\r\n", m_pSelectedFolder.FolderUID);
                response += string.Format("* OK [UIDNEXT {0}] Predicted next UID\r\n", m_pSelectedFolder.MessageUidNext);
                response +=
                    "* OK [PERMANENTFLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)] Available permanent falgs\r\n";
                response += string.Format("{0} OK [READ-ONLY] EXAMINE  Completed in {1} seconds\r\n", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2"));

                TcpStream.Write(response);
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, eArgs.ErrorText));
            }
        }

        private void Create(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.3
				
				Arguments:  mailbox name

				Responses:  no specific responses for this command

				Result:     OK - create completed
							NO - create failure: can't create mailbox with that name
							BAD - command unknown or arguments invalid
			   
				The CREATE command creates a mailbox with the given name.  An OK
				response is returned only if a new mailbox with that name has been
				created.  It is an error to attempt to create INBOX or a mailbox
				with a name that refers to an extant mailbox.  Any error in
				creation will return a tagged NO response.

				If the mailbox name is suffixed with the server's hierarchy
				separator character (as returned from the server by a LIST
				command), this is a declaration that the client intends to create
				mailbox names under this name in the hierarchy.  Server
				implementations that do not require this declaration MUST ignore
				it.

				If the server's hierarchy separator character appears elsewhere in
				the name, the server SHOULD create any superior hierarchical names
				that are needed for the CREATE command to complete successfully.
				In other words, an attempt to create "foo/bar/zap" on a server in
				which "/" is the hierarchy separator character SHOULD create foo/
				and foo/bar/ if they do not already exist.

				If a new mailbox is created with the same name as a mailbox which
				was deleted, its unique identifiers MUST be greater than any
				unique identifiers used in the previous incarnation of the mailbox
				UNLESS the new incarnation has a different unique identifier
				validity value.  See the description of the UID command for more
				detail.
				
				Example:    C: A003 CREATE owatagusiam/
							S: A003 OK CREATE completed
							C: A004 CREATE owatagusiam/blurdybloop
							S: A004 OK CREATE completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD CREATE invalid arguments. Syntax: {{<command-tag> CREATE \"mailboxName\"}}", cmdTag));
                return;
            }

            // Store start time
            long startTime = DateTime.Now.Ticks;

            string errorText = ImapServer.OnCreateMailbox(this, Core.Decode_IMAP_UTF7_String(args[0]));
            if (errorText == null)
            {
                WriteLine(string.Format("{0} OK CREATE Completed in {1} seconds", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2")));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
            }
        }

        private void Delete(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.4 DELETE Command
			
				Arguments:  mailbox name

				Responses:  no specific responses for this command

				Result:     OK - create completed
							NO - create failure: can't create mailbox with that name
							BAD - command unknown or arguments invalid
			   
				The DELETE command permanently removes the mailbox with the given
				name.  A tagged OK response is returned only if the mailbox has
				been deleted.  It is an error to attempt to delete INBOX or a
				mailbox name that does not exist.

				The DELETE command MUST NOT remove inferior hierarchical names.
				For example, if a mailbox "foo" has an inferior "foo.bar"
				(assuming "." is the hierarchy delimiter character), removing
				"foo" MUST NOT remove "foo.bar".  It is an error to attempt to
				delete a name that has inferior hierarchical names and also has
				the \Noselect mailbox name attribute (see the description of the
				LIST response for more details).

				It is permitted to delete a name that has inferior hierarchical
				names and does not have the \Noselect mailbox name attribute.  In
				this case, all messages in that mailbox are removed, and the name
				will acquire the \Noselect mailbox name attribute.

				The value of the highest-used unique identifier of the deleted
				mailbox MUST be preserved so that a new mailbox created with the
				same name will not reuse the identifiers of the former
				incarnation, UNLESS the new incarnation has a different unique
				identifier validity value.  See the description of the UID command
				for more detail.
				
				Examples:   C: A682 LIST "" *
							S: * LIST () "/" blurdybloop
							S: * LIST (\Noselect) "/" foo
							S: * LIST () "/" foo/bar
							S: A682 OK LIST completed
							C: A683 DELETE blurdybloop
							S: A683 OK DELETE completed
							C: A684 DELETE foo
							S: A684 NO Name "foo" has inferior hierarchical names
							C: A685 DELETE foo/bar
							S: A685 OK DELETE Completed
							C: A686 LIST "" *
							S: * LIST (\Noselect) "/" foo
							S: A686 OK LIST completed
							C: A687 DELETE foo
							S: A687 OK DELETE Completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD DELETE invalid arguments. Syntax: {{<command-tag> DELETE \"mailboxName\"}}", cmdTag));
                return;
            }

            string errorText = ImapServer.OnDeleteMailbox(this, Core.Decode_IMAP_UTF7_String(args[0]));
            if (errorText == null)
            {
                WriteLine(string.Format("{0} OK DELETE Completed", cmdTag));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
            }
        }

        private void Rename(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.5 RENAME Command
			  
				Arguments:  existing mailbox name
							new mailbox name

				Responses:  no specific responses for this command

				Result:     OK - rename completed
							NO - rename failure: can't rename mailbox with that name,
								 can't rename to mailbox with that name
							BAD - command unknown or arguments invalid
			   
				The RENAME command changes the name of a mailbox.  A tagged OK
				response is returned only if the mailbox has been renamed.  It is		  
				an error to attempt to rename from a mailbox name that does not
				exist or to a mailbox name that already exists.  Any error in
				renaming will return a tagged NO response.

				If the name has inferior hierarchical names, then the inferior
				hierarchical names MUST also be renamed.  For example, a rename of
				"foo" to "zap" will rename "foo/bar" (assuming "/" is the
				hierarchy delimiter character) to "zap/bar".

				The value of the highest-used unique identifier of the old mailbox
				name MUST be preserved so that a new mailbox created with the same
				name will not reuse the identifiers of the former incarnation,
				UNLESS the new incarnation has a different unique identifier
				validity value.  See the description of the UID command for more
				detail.

				Renaming INBOX is permitted, and has special behavior.  It moves
				all messages in INBOX to a new mailbox with the given name,
				leaving INBOX empty.  If the server implementation supports
				inferior hierarchical names of INBOX, these are unaffected by a
				rename of INBOX.
				
				Examples:   C: A682 LIST "" *
							S: * LIST () "/" blurdybloop
							S: * LIST (\Noselect) "/" foo
							S: * LIST () "/" foo/bar
							S: A682 OK LIST completed
							C: A683 RENAME blurdybloop sarasoop
							S: A683 OK RENAME completed
							C: A684 RENAME foo zowie
							S: A684 OK RENAME Completed
							C: A685 LIST "" *
							S: * LIST () "/" sarasoop
							S: * LIST (\Noselect) "/" zowie
							S: * LIST () "/" zowie/bar
							S: A685 OK LIST completed
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD RENAME invalid arguments. Syntax: {{<command-tag> RENAME \"mailboxName\" \"newMailboxName\"}}", cmdTag));
                return;
            }

            string mailbox = Core.Decode_IMAP_UTF7_String(args[0]);
            string newMailbox = Core.Decode_IMAP_UTF7_String(args[1]);

            string errorText = ImapServer.OnRenameMailbox(this, mailbox, newMailbox);
            if (errorText == null)
            {
                WriteLine(string.Format("{0} OK RENAME Completed", cmdTag));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
            }
        }

        private void Suscribe(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.6 SUBSCRIBE Commmand
				
				Arguments:  mailbox

				Responses:  no specific responses for this command

				Result:     OK - subscribe completed
							NO - subscribe failure: can't subscribe to that name
							BAD - command unknown or arguments invalid
			   
				The SUBSCRIBE command adds the specified mailbox name to the
				server's set of "active" or "subscribed" mailboxes as returned by
				the LSUB command.  This command returns a tagged OK response only
				if the subscription is successful.

				A server MAY validate the mailbox argument to SUBSCRIBE to verify
				that it exists.  However, it MUST NOT unilaterally remove an
				existing mailbox name from the subscription list even if a mailbox
				by that name no longer exists.

				Note: this requirement is because some server sites may routinely
				remove a mailbox with a well-known name (e.g.  "system-alerts")
				after its contents expire, with the intention of recreating it
				when new contents are appropriate.

				Example:    C: A002 SUBSCRIBE #news.comp.mail.mime
							S: A002 OK SUBSCRIBE completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD SUBSCRIBE invalid arguments. Syntax: {{<command-tag> SUBSCRIBE \"mailboxName\"}}", cmdTag));
                return;
            }
            string errorText = ImapServer.OnSubscribeMailbox(this, Core.Decode_IMAP_UTF7_String(args[0]));
            if (errorText == null)
            {
                AdditionalSelect(Core.Decode_IMAP_UTF7_String(args[0]));
                WriteLine(string.Format("{0} OK SUBSCRIBE completed", cmdTag));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
            }
        }

        

        private void AdditionalSelect(string folder)
        {
            m_StatusedMailbox = folder;
            m_pAdditionalFolder = new IMAP_SelectedFolder(folder);
            IMAP_eArgs_GetMessagesInfo eArgs = ImapServer.OnGetMessagesInfo(this, m_pAdditionalFolder);
        }

        private void UnSuscribe(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.7 UNSUBSCRIBE Command
				
				Arguments:  mailbox

				Responses:  no specific responses for this command

				Result:     OK - subscribe completed
							NO - subscribe failure: can't subscribe to that name
							BAD - command unknown or arguments invalid
			   
				The UNSUBSCRIBE command removes the specified mailbox name from
				the server's set of "active" or "subscribed" mailboxes as returned
				by the LSUB command.  This command returns a tagged OK response
				only if the unsubscription is successful.

				Example:    C: A002 UNSUBSCRIBE #news.comp.mail.mime
							S: A002 OK UNSUBSCRIBE completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD UNSUBSCRIBE invalid arguments. Syntax: {{<command-tag> UNSUBSCRIBE \"mailboxName\"}}", cmdTag));
                return;
            }

            string errorText = ImapServer.OnUnSubscribeMailbox(this, Core.Decode_IMAP_UTF7_String(args[0]));

            if (errorText == null)
            {
                AdditionalSelect(Core.Decode_IMAP_UTF7_String(args[0]));
                WriteLine(string.Format("{0} OK UNSUBSCRIBE completed", cmdTag));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
            }
        }

        private void List(string cmdTag, string argsText)
        {
            /* Rc 3501 6.3.8 LIST Command
			 
				Arguments:  reference name
							mailbox name with possible wildcards

				Responses:  untagged responses: LIST

				Result:     OK - list completed
							NO - list failure: can't list that reference or name
							BAD - command unknown or arguments invalid
							
				The LIST command returns a subset of names from the complete set
				of all names available to the client.  Zero or more untagged LIST
				replies are returned, containing the name attributes, hierarchy
				delimiter, and name; see the description of the LIST reply for
				more detail.
				
				An empty ("" string) reference name argument indicates that the
				mailbox name is interpreted as by SELECT. The returned mailbox
				names MUST match the supplied mailbox name pattern.  A non-empty
				reference name argument is the name of a mailbox or a level of
				mailbox hierarchy, and indicates a context in which the mailbox
				name is interpreted in an implementation-defined manner.
				
				An empty ("" string) mailbox name argument is a special request to
				return the hierarchy delimiter and the root name of the name given
				in the reference.  The value returned as the root MAY be null if
				the reference is non-rooted or is null.  In all cases, the
				hierarchy delimiter is returned.  This permits a client to get the
				hierarchy delimiter even when no mailboxes by that name currently
				exist.
				
				The character "*" is a wildcard, and matches zero or more
				characters at this position.  The character "%" is similar to "*",
				but it does not match a hierarchy delimiter.  If the "%" wildcard
				is the last character of a mailbox name argument, matching levels
				of hierarchy are also returned.
			    
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD Invalid LIST arguments. Syntax: {{<command-tag> LIST \"<reference-name>\" \"<mailbox-name>\"}}", cmdTag));
                return;
            }

            // Store start time
            long startTime = DateTime.Now.Ticks;

            string refName = Core.Decode_IMAP_UTF7_String(args[0]);
            string mailbox = Core.Decode_IMAP_UTF7_String(args[1]);

            string reply = "";

            // Folder separator wanted
            if (mailbox.Length == 0)
            {
                reply += "* LIST (\\Noselect) \"/\" \"\"\r\n";
            }
            // List mailboxes
            else
            {
                IMAP_Folders mailboxes = ImapServer.OnGetMailboxes(this, refName, mailbox);
                foreach (IMAP_Folder mBox in mailboxes.Folders)
                {
                    if (mBox.Selectable)
                    {
                        reply += string.Format("* LIST () \"/\" \"{0}\" \r\n", Core.Encode_IMAP_UTF7_String(mBox.Folder));
                    }
                    else
                    {
                        reply += string.Format("* LIST (\\Noselect) \"/\" \"{0}\" \r\n", Core.Encode_IMAP_UTF7_String(mBox.Folder));
                    }
                }
            }

            reply += string.Format("{0} OK LIST Completed in {1} seconds\r\n", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2"));
            TcpStream.Write(reply);
        }

        private void LSub(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.9 LSUB Command
			 
				Arguments:  reference name
							mailbox name with possible wildcards

				Responses:  untagged responses: LSUB

				Result:     OK - lsub completed
							NO - lsub failure: can't list that reference or name
							BAD - command unknown or arguments invalid
				   
				The LSUB command returns a subset of names from the set of names
				that the user has declared as being "active" or "subscribed".
				Zero or more untagged LSUB replies are returned.  The arguments to
				LSUB are in the same form as those for LIST.

				The returned untagged LSUB response MAY contain different mailbox
				flags from a LIST untagged response.  If this should happen, the
				flags in the untagged LIST are considered more authoritative.

				A special situation occurs when using LSUB with the % wildcard.
				Consider what happens if "foo/bar" (with a hierarchy delimiter of
				"/") is subscribed but "foo" is not.  A "%" wildcard to LSUB must
				return foo, not foo/bar, in the LSUB response, and it MUST be
				flagged with the \Noselect attribute.

				The server MUST NOT unilaterally remove an existing mailbox name
				from the subscription list even if a mailbox by that name no
				longer exists.

				Example:    C: A002 LSUB "#news." "comp.mail.*"
							S: * LSUB () "." #news.comp.mail.mime
							S: * LSUB () "." #news.comp.mail.misc
							S: A002 OK LSUB completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD LSUB invalid arguments", cmdTag));
                return;
            }

            string refName = Core.Decode_IMAP_UTF7_String(args[0]);
            string mailbox = Core.Decode_IMAP_UTF7_String(args[1]);

            string reply = "";

            IMAP_Folders mailboxes = ImapServer.OnGetSubscribedMailboxes(this, refName, mailbox);
            foreach (IMAP_Folder mBox in mailboxes.Folders)
            {
                reply += string.Format("* LSUB () \"/\" \"{0}\" \r\n", Core.Encode_IMAP_UTF7_String(mBox.Folder));
            }

            reply += string.Format("{0} OK LSUB Completed\r\n", cmdTag);
            TcpStream.Write(reply);
        }

        private void Status(string cmdTag, string argsText)
        {
            /* RFC 3501 6.3.10 STATUS Command
			
				Arguments:  mailbox name
							status data item names

				Responses:  untagged responses: STATUS

				Result:     OK - status completed
							NO - status failure: no status for that name
							BAD - command unknown or arguments invalid
			   
				The STATUS command requests the status of the indicated mailbox.
				It does not change the currently selected mailbox, nor does it
				affect the state of any messages in the queried mailbox (in
				particular, STATUS MUST NOT cause messages to lose the \Recent
				flag).

				The STATUS command provides an alternative to opening a second
				IMAP4rev1 connection and doing an EXAMINE command on a mailbox to
				query that mailbox's status without deselecting the current
				mailbox in the first IMAP4rev1 connection.

				Unlike the LIST command, the STATUS command is not guaranteed to
				be fast in its response.  In some implementations, the server is
				obliged to open the mailbox read-only internally to obtain certain
				status information.  Also unlike the LIST command, the STATUS
				command does not accept wildcards.

				The currently defined status data items that can be requested are:

				MESSAGES
					The number of messages in the mailbox.

				RECENT
					The number of messages with the \Recent flag set.

				UIDNEXT
					The next unique identifier value of the mailbox.  Refer to
					section 2.3.1.1 for more information.

				UIDVALIDITY
					The unique identifier validity value of the mailbox.  Refer to
					section 2.3.1.1 for more information.

				UNSEEN
					The number of messages which do not have the \Seen flag set.


				Example:    C: A042 STATUS blurdybloop (UIDNEXT MESSAGES)
							S: * STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)
							S: A042 OK STATUS completed
				  
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = ParseParams(argsText);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD Invalid STATUS arguments. Syntax: {{<command-tag> STATUS \"<mailbox-name>\" \"(status-data-items)\"}}", cmdTag));
                return;
            }

            string folder = Core.Decode_IMAP_UTF7_String(args[0]);
            string wantedItems = args[1].ToUpper();

            // See wanted items are valid.
            if (
                wantedItems.Replace("MESSAGES", "").Replace("RECENT", "").Replace("UIDNEXT", "").Replace(
                    "UIDVALIDITY", "").Replace("UNSEEN", "").Trim().Length > 0)
            {
                WriteLine(string.Format("{0} BAD STATUS invalid arguments", cmdTag));
                return;
            }

            AdditionalSelect(folder);
            IMAP_SelectedFolder selectedFolder = new IMAP_SelectedFolder(folder);
            IMAP_eArgs_GetMessagesInfo eArgs = ImapServer.OnGetMessagesInfo(this, selectedFolder);
            if (eArgs.ErrorText == null)
            {
                string itemsReply = "";
                if (wantedItems.IndexOf("MESSAGES") > -1)
                {
                    itemsReply += string.Format(" MESSAGES {0}", selectedFolder.Messages.Count);
                }
                if (wantedItems.IndexOf("RECENT") > -1)
                {
                    itemsReply += string.Format(" RECENT {0}", selectedFolder.RecentCount);
                }
                if (wantedItems.IndexOf("UNSEEN") > -1)
                {
                    itemsReply += string.Format(" UNSEEN {0}", selectedFolder.UnSeenCount);
                }
                if (wantedItems.IndexOf("UIDVALIDITY") > -1)
                {
                    itemsReply += string.Format(" UIDVALIDITY {0}", selectedFolder.FolderUID);
                }
                if (wantedItems.IndexOf("UIDNEXT") > -1)
                {
                    itemsReply += string.Format(" UIDNEXT {0}", selectedFolder.MessageUidNext);
                }
                itemsReply = itemsReply.Trim();

                WriteLine(string.Format("* STATUS {0} ({1})", args[0], itemsReply));
                WriteLine(string.Format("{0} OK STATUS completed", cmdTag));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, eArgs.ErrorText));
            }
        }

        /// <summary>
        /// Returns true if command ended syncronously.
        /// </summary>
        private bool BeginAppendCmd(string cmdTag, string argsText)
        {
            /* Rfc 3501 6.3.11 APPEND Command
			
				Arguments:  mailbox name
							OPTIONAL flag parenthesized list
							OPTIONAL date/time string
							message literal

				Responses:  no specific responses for this command

				Result:     OK - append completed
							NO - append error: can't append to that mailbox, error
									in flags or date/time or message text
							BAD - command unknown or arguments invalid

				The APPEND command appends the literal argument as a new message
				to the end of the specified destination mailbox.  This argument
				SHOULD be in the format of an [RFC-2822] message.  8-bit
				characters are permitted in the message.  A server implementation
				that is unable to preserve 8-bit data properly MUST be able to
				reversibly convert 8-bit APPEND data to 7-bit using a [MIME-IMB]
				content transfer encoding.
					
				If a flag parenthesized list is specified, the flags SHOULD be set
				in the resulting message; otherwise, the flag list of the
				resulting message is set to empty by default.  In either case, the
				Recent flag is also set.

				If a date-time is specified, the internal date SHOULD be set in
				the resulting message; otherwise, the internal date of the
				resulting message is set to the current date and time by default.

				If the append is unsuccessful for any reason, the mailbox MUST be
				restored to its state before the APPEND attempt; no partial
				appending is permitted.

				If the destination mailbox does not exist, a server MUST return an
				error, and MUST NOT automatically create the mailbox.  Unless it
				is certain that the destination mailbox can not be created, the
				server MUST send the response code "[TRYCREATE]" as the prefix of
				the text of the tagged NO response.  This gives a hint to the
				client that it can attempt a CREATE command and retry the APPEND
				if the CREATE is successful.
					
				Example:    C: A003 APPEND saved-messages (\Seen) {310}
							S: + Ready for literal data
							C: Date: Mon, 7 Feb 1994 21:52:25 -0800 (PST)
							C: From: Fred Foobar <foobar@Blurdybloop.COM>
							C: Subject: afternoon meeting
							C: To: mooch@owatagu.siam.edu
							C: Message-Id: <B27397-0100000@Blurdybloop.COM>
							C: MIME-Version: 1.0
							C: Content-Type: TEXT/PLAIN; CHARSET=US-ASCII
							C:
							C: Hello Joe, do you think we can meet at 3:30 tomorrow?
							C:
							S: A003 OK APPEND completed
					
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return true;
            }

            string[] args = ParseParams(argsText);
            if (args.Length < 2 || args.Length > 4)
            {
                WriteLine(string.Format("{0} BAD APPEND Invalid arguments", cmdTag));
                return true;
            }

            string mailbox = Core.Decode_IMAP_UTF7_String(args[0]);
            IMAP_MessageFlags mFlags = 0;
            DateTime date = DateTime.Now;
            long msgLen = Convert.ToInt64(args[args.Length - 1].Replace("{", "").Replace("}", ""));

            if (args.Length == 4)
            {
                //--- Parse flags, see if valid ----------------
                string flags = args[1].ToUpper();
                if (
                    flags.Replace("\\ANSWERED", "").Replace("\\FLAGGED", "").Replace("\\DELETED", "").Replace(
                        "\\SEEN", "").Replace("\\DRAFT", "").Trim().Length > 0)
                {
                    WriteLine(string.Format("{0} BAD arguments invalid", cmdTag));
                    return false;
                }

                mFlags = IMAP_Utils.ParseMessageFlags(flags);
                date = MimeUtils.ParseDate(args[2]);
            }
            else if (args.Length == 3)
            {
                // See if date or flags specified, try date first
                try
                {
                    date = MimeUtils.ParseDate(args[1]);
                }
                catch
                {
                    //--- Parse flags, see if valid ----------------
                    string flags = args[1].ToUpper();
                    if (
                        flags.Replace("\\ANSWERED", "").Replace("\\FLAGGED", "").Replace("\\DELETED", "").
                            Replace("\\SEEN", "").Replace("\\DRAFT", "").Trim().Length > 0)
                    {
                        WriteLine(string.Format("{0} BAD arguments invalid", cmdTag));
                        return false;
                    }

                    mFlags = IMAP_Utils.ParseMessageFlags(flags);
                }
            }

            // Request data
            WriteLine("+ Ready for literal data");

            MemoryStream strm = new MemoryStream();
            Hashtable param = new Hashtable();
            param.Add("cmdTag", cmdTag);
            param.Add("mailbox", mailbox);
            param.Add("mFlags", mFlags);
            param.Add("date", date);
            param.Add("strm", strm);

            // Begin recieving data                      Why needed msgLen+2 ??? 
            TcpStream.BeginReadFixedCount(strm, (int)msgLen + 2, EndAppendCmd, param);

            return false;
        }

        /// <summary>
        /// Is called when DATA command is finnished.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void EndAppendCmd(IAsyncResult ar)
        {
            try
            {
                TcpStream.EndReadFixedCount(ar);
                if (ar.IsCompleted)
                {

                    Hashtable param = (Hashtable)ar.AsyncState;
                    string cmdTag = (string)param["cmdTag"];
                    string mailbox = (string)param["mailbox"];
                    IMAP_MessageFlags mFlags = (IMAP_MessageFlags)param["mFlags"];
                    DateTime date = (DateTime)param["date"];
                    MemoryStream strm = (MemoryStream)param["strm"];

                    IMAP_Message msg = new IMAP_Message(null, "", 0, date, 0, mFlags);
                    string errotText = ImapServer.OnStoreMessage(this, mailbox, msg, strm.ToArray());
                    if (errotText == null)
                    {
                        WriteLine(string.Format("{0} OK APPEND completed, recieved {1} bytes", cmdTag, strm.Length));
                    }
                    else
                    {
                        WriteLine(string.Format("{0} NO {1}", cmdTag, errotText));
                    }
                }

                // Command completed ok, get next command
                BeginRecieveCmd();
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        private void Namespace(string cmdTag, string argsText)
        {
            /* Rfc 2342 5. NAMESPACE Command.              
             
                Arguments: none

                Response:  an untagged NAMESPACE response that contains the prefix
                             and hierarchy delimiter to the server's Personal
                             Namespace(s), Other Users' Namespace(s), and Shared
                             Namespace(s) that the server wishes to expose. The
                             response will contain a NIL for any namespace class
                             that is not available. Namespace_Response_Extensions
                             MAY be included in the response.
                             Namespace_Response_Extensions which are not on the IETF
                             standards track, MUST be prefixed with an "X-".

                Result:    OK - Command completed
                           NO - Error: Can't complete command
                           BAD - argument invalid
             
				Example: < A server that contains a Personal Namespace and a single Shared
						Namespace. >

						C: A001 NAMESPACE
						S: * NAMESPACE (("" "/")) NIL (("Public Folders/" "/"))
						S: A001 OK NAMESPACE command completed	  
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            SharedRootFolders_EventArgs eArgs = ImapServer.OnGetSharedRootFolders(this);
            string publicRootFolders = "NIL";
            if (eArgs.PublicRootFolders != null && eArgs.PublicRootFolders.Length > 0)
            {
                publicRootFolders = "(";
                foreach (string publicRootFolder in eArgs.PublicRootFolders)
                {
                    publicRootFolders += string.Format("(\"{0}/\" \"/\")", publicRootFolder);
                }
                publicRootFolders += ")";
            }
            string sharedRootFolders = "NIL";
            if (eArgs.SharedRootFolders != null && eArgs.SharedRootFolders.Length > 0)
            {
                sharedRootFolders = "(";
                foreach (string sharedRootFolder in eArgs.SharedRootFolders)
                {
                    sharedRootFolders += string.Format("(\"{0}/\" \"/\")", sharedRootFolder);
                }
                sharedRootFolders += ")";
            }

            string response = string.Format("* NAMESPACE ((\"\" \"/\")) {0} {1}\r\n", sharedRootFolders, publicRootFolders);
            response += string.Format("{0} OK NAMESPACE completed", cmdTag);

            WriteLine(response);
        }

        private void GETACL(string cmdTag, string argsText)
        {
            /* RFC 2086 4.3. GETACL
				Arguments:  mailbox name

				Data:       untagged responses: ACL

				Result:     OK - getacl completed
							NO - getacl failure: can't get acl
							BAD - command unknown or arguments invalid

					The GETACL command returns the access control list for mailbox in
					an untagged ACL reply.

				Example:    C: A002 GETACL INBOX
							S: * ACL INBOX Fred rwipslda
							S: A002 OK Getacl complete
							
							.... Multiple users
							S: * ACL INBOX Fred rwipslda test rwipslda
							
							.... No acl flags for Fred
							S: * ACL INBOX Fred "" test rwipslda
									
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD GETACL invalid arguments. Syntax: GETACL<SP>FolderName<CRLF>", cmdTag));
                return;
            }

            IMAP_GETACL_eArgs eArgs = ImapServer.OnGetFolderACL(this,
                                                               Core.Decode_IMAP_UTF7_String(
                                                                   IMAP_Utils.NormalizeFolder(args[0])));
            if (eArgs.ErrorText.Length > 0)
            {
                WriteLine(string.Format("{0} NO GETACL {1}", cmdTag, eArgs.ErrorText));
            }
            else
            {
                string response = "";
                if (eArgs.ACL.Count > 0)
                {
                    response += string.Format("* ACL \"{0}\"", args[0]);
                    foreach (DictionaryEntry ent in eArgs.ACL)
                    {
                        string aclFalgs = IMAP_Utils.ACL_to_String((IMAP_ACL_Flags)ent.Value);
                        if (aclFalgs.Length == 0)
                        {
                            aclFalgs = "\"\"";
                        }
                        response += string.Format(" \"{0}\" {1}", ent.Key, aclFalgs);
                    }
                    response += "\r\n";
                }
                response += string.Format("{0} OK Getacl complete\r\n", cmdTag);

                TcpStream.Write(response);
            }
        }

        private void SETACL(string cmdTag, string argsText)
        {
            /* RFC 2086 4.1. SETACL
				Arguments:  mailbox name
							authentication identifier
							access right modification

				Data:       no specific data for this command

				Result:     OK - setacl completed
							NO - setacl failure: can't set acl
							BAD - command unknown or arguments invalid

					The SETACL command changes the access control list on the
					specified mailbox so that the specified identifier is granted
					permissions as specified in the third argument.

					The third argument is a string containing an optional plus ("+")
					or minus ("-") prefix, followed by zero or more rights characters.
					If the string starts with a plus, the following rights are added
					to any existing rights for the identifier.  If the string starts
					with a minus, the following rights are removed from any existing
					rights for the identifier.  If the string does not start with a
					plus or minus, the rights replace any existing rights for the
					identifier.
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = ParseParams(argsText);
            if (args.Length != 3)
            {
                WriteLine(string.Format("{0} BAD GETACL invalid arguments. Syntax: SETACL<SP>FolderName<SP>UserName<SP>ACL_Flags<CRLF>", cmdTag));
                return;
            }

            string aclFlags = args[2];
            IMAP_Flags_SetType setType = IMAP_Flags_SetType.Replace;
            if (aclFlags.StartsWith("+"))
            {
                setType = IMAP_Flags_SetType.Add;
            }
            else if (aclFlags.StartsWith("-"))
            {
                setType = IMAP_Flags_SetType.Remove;
            }

            IMAP_SETACL_eArgs eArgs = ImapServer.OnSetFolderACL(this,
                                                               IMAP_Utils.NormalizeFolder(args[0]),
                                                               args[1],
                                                               setType,
                                                               IMAP_Utils.ACL_From_String(aclFlags));
            if (eArgs.ErrorText.Length > 0)
            {
                WriteLine(string.Format("{0} NO SETACL {1}", cmdTag, eArgs.ErrorText));
            }
            else
            {
                WriteLine(string.Format("{0} OK SETACL completed", cmdTag));
            }
        }

        private void DELETEACL(string cmdTag, string argsText)
        {
            /* RFC 2086 4.2. DELETEACL
				Arguments:  mailbox name
							authentication identifier

				Data:       no specific data for this command

				Result:     OK - deleteacl completed
							NO - deleteacl failure: can't delete acl
							BAD - command unknown or arguments invalid

					The DELETEACL command removes any <identifier,rights> pair for the
					specified identifier from the access control list for the specified
					mailbox.
					
				Example:    C: A002 DELETEACL INBOX test
							S: A002 OK DELETEACL completed
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD GETACL invalid arguments. Syntax: DELETEACL<SP>FolderName<SP>UserName<CRLF>", cmdTag));
                return;
            }

            IMAP_DELETEACL_eArgs eArgs = ImapServer.OnDeleteFolderACL(this,
                                                                     IMAP_Utils.NormalizeFolder(args[0]),
                                                                     args[1]);
            if (eArgs.ErrorText.Length > 0)
            {
                WriteLine(string.Format("{0} NO DELETEACL {1}", cmdTag, eArgs.ErrorText));
            }
            else
            {
                WriteLine(string.Format("{0} OK DELETEACL completed", cmdTag));
            }
        }

        private void LISTRIGHTS(string cmdTag, string argsText)
        {
            /* RFC 2086 4.4. LISTRIGHTS
				Arguments:  mailbox name
				authentication identifier

				Data:       untagged responses: LISTRIGHTS

				Result:     OK - listrights completed
							NO - listrights failure: can't get rights list
							BAD - command unknown or arguments invalid

					The LISTRIGHTS command takes a mailbox name and an identifier and
					returns information about what rights may be granted to the identifier
					in the ACL for the mailbox.
					
				Example:    C: a001 LISTRIGHTS ~/Mail/saved smith
							S: * LISTRIGHTS ~/Mail/saved "smith" la r swicd
							S: a001 OK Listrights completed


							C: a005 LISTRIGHTS archive.imap anyone
							S: * LISTRIGHTS archive.imap "anyone" "" l r s w i p c d a
							0 1 2 3 4 5 6 7 8 9
			   
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD GETACL invalid arguments. Syntax: LISTRIGHTS<SP>FolderName<SP>UserName<CRLF>", cmdTag));
                return;
            }

            string response = string.Format("* LISTRIGHTS \"{0}\" \"{1}\" l r s w i p c d a\r\n", args[0], args[1]);
            response += string.Format("{0} OK MYRIGHTS Completed\r\n", cmdTag);

            TcpStream.Write(response);
        }

        private void MYRIGHTS(string cmdTag, string argsText)
        {
            /* RFC 2086 4.5. MYRIGHTS
				Arguments:  mailbox name

				Data:       untagged responses: MYRIGHTS

				Result:     OK - myrights completed
							NO - myrights failure: can't get rights
							BAD - command unknown or arguments invalid

					The MYRIGHTS command returns the set of rights that the user has
					to mailbox in an untagged MYRIGHTS reply.

				Example:    C: A003 MYRIGHTS INBOX
							S: * MYRIGHTS INBOX rwipslda
							S: A003 OK Myrights complete
							
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD GETACL invalid arguments. Syntax: MYRIGHTS<SP>FolderName<CRLF>", cmdTag));
                return;
            }

            IMAP_GetUserACL_eArgs eArgs = ImapServer.OnGetUserACL(this,
                                                                 IMAP_Utils.NormalizeFolder(args[0]),
                                                                 UserName);
            if (eArgs.ErrorText.Length > 0)
            {
                WriteLine(string.Format("{0} NO MYRIGHTS {1}", cmdTag, eArgs.ErrorText));
            }
            else
            {
                string aclFlags = IMAP_Utils.ACL_to_String(eArgs.ACL);
                if (aclFlags.Length == 0)
                {
                    aclFlags = "\"\"";
                }
                string response = string.Format("* MYRIGHTS \"{0}\" {1}\r\n", args[0], aclFlags);
                response += string.Format("{0} OK MYRIGHTS Completed\r\n", cmdTag);

                TcpStream.Write(response);
            }
        }

        private void GETQUOTA(string cmdTag, string argsText)
        {
            /* RFC 2087 4.2. GETQUOTA
				Arguments:  quota root

                Data:       untagged responses: QUOTA

                Result:     OK - getquota completed
                            NO - getquota  error:  no  such  quota  root,  permission denied
                            BAD - command unknown or arguments invalid

                The GETQUOTA command takes the name of a quota root and returns the
                quota root's resource usage and limits in an untagged QUOTA response.

                Example:    C: A003 GETQUOTA ""
                            S: * QUOTA "" (STORAGE 10 512)
                            S: A003 OK Getquota completed
							
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD GETQUOTA invalid arguments. Syntax: GETQUOTA \"quota_root\"<CRLF>", cmdTag));
                return;
            }

            IMAP_eArgs_GetQuota eArgs = ImapServer.OnGetUserQuota(this);
            string reply = string.Format("* QUOTA \"\" (STORAGE {0} {1})\r\n", eArgs.MailboxSize, eArgs.MaxMailboxSize);
            reply += string.Format("{0} OK GETQUOTA completed\r\n", cmdTag);
            TcpStream.Write(reply);
        }

        private void GETQUOTAROOT(string cmdTag, string argsText)
        {
            /* RFC 2087 4.3. GETQUOTAROOT
				Arguments:  mailbox name

                Data:       untagged responses: QUOTAROOT, QUOTA

                Result:     OK - getquota completed
                            NO - getquota error: no such mailbox, permission denied
                            BAD - command unknown or arguments invalid

                The GETQUOTAROOT command takes the name of a mailbox and returns the
                list of quota roots for the mailbox in an untagged QUOTAROOT
                response.  For each listed quota root, it also returns the quota
                root's resource usage and limits in an untagged QUOTA response.

                Example:    C: A003 GETQUOTAROOT INBOX
                            S: * QUOTAROOT INBOX ""
                            S: * QUOTA "" (STORAGE 10 512)
                            S: A003 OK Getquota completed
							
			*/

            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }

            string[] args = TextUtils.SplitQuotedString(argsText, ' ', true);
            if (args.Length != 1)
            {
                WriteLine(string.Format("{0} BAD GETQUOTAROOT invalid arguments. Syntax: GETQUOTAROOT \"folder\"<CRLF>", cmdTag));
                return;
            }

            IMAP_eArgs_GetQuota eArgs = ImapServer.OnGetUserQuota(this);
            string reply = string.Format("* QUOTAROOT \"{0}\" \"\"\r\n", args[0]);
            reply += string.Format("* QUOTA \"\" (STORAGE {0} {1})\r\n", eArgs.MailboxSize, eArgs.MaxMailboxSize);
            reply += string.Format("{0} OK GETQUOTAROOT completed\r\n", cmdTag);
            TcpStream.Write(reply);
        }

        //--- End of IsAuthenticated State 

        //--- Selected State ------

        private void Check(string cmdTag)
        {
            /* RFC 3501 6.4.1 CHECK Command
			
				Arguments:  none

				Responses:  no specific responses for this command

				Result:     OK - check completed
							BAD - command unknown or arguments invalid
			   
				The CHECK command requests a checkpoint of the currently selected
				mailbox.  A checkpoint refers to any implementation-dependent
				housekeeping associated with the mailbox (e.g. resolving the
				server's in-memory state of the mailbox with the state on its
				disk) that is not normally executed as part of each command.  A
				checkpoint MAY take a non-instantaneous amount of real time to
				complete.  If a server implementation has no such housekeeping
				considerations, CHECK is equivalent to NOOP.

				There is no guarantee that an EXISTS untagged response will happen
				as a result of CHECK.  NOOP, not CHECK, SHOULD be used for new
				mail polling.
				
				Example:    C: FXXZ CHECK
							S: FXXZ OK CHECK Completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            WriteLine(string.Format("{0} OK CHECK completed", cmdTag));
        }

        private void Close(string cmdTag)
        {
            /* RFC 3501 6.4.2 CLOSE Command
			
				Arguments:  none

				Responses:  no specific responses for this command

				Result:     OK - close completed, now in authenticated state
							BAD - command unknown or arguments invalid
			   
				The CLOSE command permanently removes from the currently selected
				mailbox all messages that have the \Deleted flag set, and returns
				to authenticated state from selected state.  No untagged EXPUNGE
				responses are sent.

				No messages are removed, and no error is given, if the mailbox is
				selected by an EXAMINE command or is otherwise selected read-only.

				Even if a mailbox is selected, a SELECT, EXAMINE, or LOGOUT
				command MAY be issued without previously issuing a CLOSE command.
				The SELECT, EXAMINE, and LOGOUT commands implicitly close the
				currently selected mailbox without doing an expunge.  However,
				when many messages are deleted, a CLOSE-LOGOUT or CLOSE-SELECT
				sequence is considerably faster than an EXPUNGE-LOGOUT or
				EXPUNGE-SELECT because no untagged EXPUNGE responses (which the
				client would probably ignore) are sent.

				Example:    C: A341 CLOSE
							S: A341 OK CLOSE completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            if (!m_pSelectedFolder.ReadOnly)
            {
                IMAP_Message[] messages = m_pSelectedFolder.Messages.GetWithFlags(IMAP_MessageFlags.Deleted);
                foreach (IMAP_Message msg in messages)
                {
                    ImapServer.OnDeleteMessage(this, msg);
                }
            }

            SelectedMailbox = "";
            m_pSelectedFolder = null;

            WriteLine(string.Format("{0} OK CLOSE completed", cmdTag));
            //EndSession();
        }

        private void Expunge(string cmdTag)
        {
            /* RFC 3501 6.4.3 EXPUNGE Command
			
				Arguments:  none

				Responses:  untagged responses: EXPUNGE

				Result:     OK - expunge completed
							NO - expunge failure: can't expunge (e.g., permission
									denied)
							BAD - command unknown or arguments invalid

					The EXPUNGE command permanently removes all messages that have the
					\Deleted flag set from the currently selected mailbox.  Before
					returning an OK to the client, an untagged EXPUNGE response is
					sent for each message that is removed.
					
				The EXPUNGE response reports that the specified message sequence
			    number has been permanently removed from the mailbox.  The message
				sequence number for each successive message in the mailbox is
				IMMEDIATELY DECREMENTED BY 1, and this decrement is reflected in
				message sequence numbers in subsequent responses (including other
				untagged EXPUNGE responses).


				Example:    C: A202 EXPUNGE
							S: * 3 EXPUNGE
							S: * 3 EXPUNGE
							S: * 5 EXPUNGE
							S: * 8 EXPUNGE
							S: A202 OK EXPUNGE completed

						Note: In this example, messages 3, 4, 7, and 11 had the
						\Deleted flag set.  See the description of the EXPUNGE
						response for further explanation.
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            IMAP_Message[] messages = m_pSelectedFolder.Messages.GetWithFlags(IMAP_MessageFlags.Deleted);
            for (int i = 0; i < messages.Length; i++)
            {
                IMAP_Message msg = messages[i];

                string errorText = ImapServer.OnDeleteMessage(this, msg);
                if (errorText == null)
                {
                    WriteLine(string.Format("* {0} EXPUNGE", msg.SequenceNo));
                    m_pSelectedFolder.Messages.Remove(msg);
                }
                else
                {
                    WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
                    return;
                }
            }

            WriteLine(string.Format("{0} OK EXPUNGE completed", cmdTag));
        }

        private void Search(string cmdTag, string argsText, bool uidSearch)
        {
            /* RFC 3501 6.4.4 SEARCH Command
			
				Arguments:  OPTIONAL [CHARSET] specification
							searching criteria (one or more)

				Responses:  REQUIRED untagged response: SEARCH

				Result:     OK - search completed
							NO - search error: can't search that [CHARSET] or
									criteria
							BAD - command unknown or arguments invalid
							
				The SEARCH command searches the mailbox for messages that match
				the given searching criteria.  Searching criteria consist of one
				or more search keys.  The untagged SEARCH response from the server
				contains a listing of message sequence numbers corresponding to
				those messages that match the searching criteria.
				
				When multiple keys are specified, the result is the intersection
				(AND function) of all the messages that match those keys.  For
				example, the criteria DELETED FROM "SMITH" SINCE 1-Feb-1994 refers
				to all deleted messages from Smith that were placed in the mailbox
				since February 1, 1994.  A search key can also be a parenthesized
				list of one or more search keys (e.g., for use with the OR and NOT
				keys).

				Server implementations MAY exclude [MIME-IMB] body parts with
				terminal content media types other than TEXT and MESSAGE from
				consideration in SEARCH matching.

				The OPTIONAL [CHARSET] specification consists of the word
				"CHARSET" followed by a registered [CHARSET].  It indicates the
				[CHARSET] of the strings that appear in the search criteria.
				[MIME-IMB] content transfer encodings, and [MIME-HDRS] strings in
				[RFC-2822]/[MIME-IMB] headers, MUST be decoded before comparing
				text in a [CHARSET] other than US-ASCII.  US-ASCII MUST be
				supported; other [CHARSET]s MAY be supported.

				If the server does not support the specified [CHARSET], it MUST
				return a tagged NO response (not a BAD).  This response SHOULD
				contain the BADCHARSET response code, which MAY list the
				[CHARSET]s supported by the server.

				In all search keys that use strings, a message matches the key if
				the string is a substring of the field.  The matching is
				case-insensitive.

				The defined search keys are as follows.  Refer to the Formal
				Syntax section for the precise syntactic definitions of the
				arguments.

				<sequence set>
					Messages with message sequence numbers corresponding to the
					specified message sequence number set.

				ALL
					All messages in the mailbox; the default initial key for
					ANDing.

				ANSWERED
					Messages with the \Answered flag set.
					
				BCC <string>
					Messages that contain the specified string in the envelope
					structure's BCC field.

				BEFORE <date>
					Messages whose internal date (disregarding time and timezone)
					is earlier than the specified date.

				BODY <string>
					Messages that contain the specified string in the body of the
					message.

				CC <string>
					Messages that contain the specified string in the envelope
					structure's CC field.

				DELETED
					Messages with the \Deleted flag set.

				DRAFT
					Messages with the \Draft flag set.

				FLAGGED
					Messages with the \Flagged flag set.

				FROM <string>
					Messages that contain the specified string in the envelope
					structure's FROM field.

				HEADER <field-name> <string>
					Messages that have a header with the specified field-name (as
					defined in [RFC-2822]) and that contains the specified string
					in the text of the header (what comes after the colon).  If the
					string to search is zero-length, this matches all messages that
					have a header line with the specified field-name regardless of
					the contents.

				KEYWORD <flag>
					Messages with the specified keyword flag set.

				LARGER <n>
					Messages with an [RFC-2822] size larger than the specified
					number of octets.

				NEW
					Messages that have the \Recent flag set but not the \Seen flag.
					This is functionally equivalent to "(RECENT UNSEEN)".
					
				NOT <search-key>
					Messages that do not match the specified search key.

				OLD
					Messages that do not have the \Recent flag set.  This is
					functionally equivalent to "NOT RECENT" (as opposed to "NOT
					NEW").

				ON <date>
					Messages whose internal date (disregarding time and timezone)
					is within the specified date.

				OR <search-key1> <search-key2>
					Messages that match either search key.

				RECENT
					Messages that have the \Recent flag set.

				SEEN
					Messages that have the \Seen flag set.

				SENTBEFORE <date>
					Messages whose [RFC-2822] Date: header (disregarding time and
					timezone) is earlier than the specified date.

				SENTON <date>
					Messages whose [RFC-2822] Date: header (disregarding time and
					timezone) is within the specified date.

				SENTSINCE <date>
					Messages whose [RFC-2822] Date: header (disregarding time and
					timezone) is within or later than the specified date.

				SINCE <date>
					Messages whose internal date (disregarding time and timezone)
					is within or later than the specified date.

				SMALLER <n>
					Messages with an [RFC-2822] size smaller than the specified
					number of octets.
					
				SUBJECT <string>
					Messages that contain the specified string in the envelope
					structure's SUBJECT field.

				TEXT <string>
					Messages that contain the specified string in the header or
					body of the message.

				TO <string>
					Messages that contain the specified string in the envelope
					structure's TO field.

				UID <sequence set>
					Messages with unique identifiers corresponding to the specified
					unique identifier set.  Sequence set ranges are permitted.

				UNANSWERED
					Messages that do not have the \Answered flag set.

				UNDELETED
					Messages that do not have the \Deleted flag set.

				UNDRAFT
					Messages that do not have the \Draft flag set.

				UNFLAGGED
					Messages that do not have the \Flagged flag set.

				UNKEYWORD <flag>
					Messages that do not have the specified keyword flag set.

				UNSEEN
					Messages that do not have the \Seen flag set.
					
					Example:   
						C: A282 SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith"
						S: * SEARCH 2 84 882
						S: A282 OK SEARCH completed
						C: A283 SEARCH TEXT "string not in mailbox"
						S: * SEARCH
						S: A283 OK SEARCH completed
						C: A284 SEARCH CHARSET UTF-8 TEXT {6}
						S: + Continue                          ### THIS IS UNDOCUMENTED !!!
						C: XXXXXX[arg conitnue]<CRLF>
						S: * SEARCH 43
						S: A284 OK SEARCH completed

				Note: Since this document is restricted to 7-bit ASCII
				text, it is not possible to show actual UTF-8 data.  The
				"XXXXXX" is a placeholder for what would be 6 octets of
				8-bit data in an actual transaction.
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            // Store start time
            long startTime = DateTime.Now.Ticks;

            //--- Get Optional charset, if specified -----------------------------------------------------------------//
            string charset = "ASCII";
            // CHARSET charset
            if (argsText.ToUpper().StartsWith("CHARSET"))
            {
                // Remove CHARSET from argsText
                argsText = argsText.Substring(7).Trim();

                string charsetValueString = IMAP_Utils.ParseQuotedParam(ref argsText);

                try
                {
                    EncodingTools.GetEncodingByCodepageName_Throws(charsetValueString);

                    charset = charsetValueString;
                }
                catch
                {
                    WriteLine(string.Format("{0} NO [BADCHARSET UTF-8] {1} is not supported", cmdTag, charsetValueString));
                    return;
                }
            }
            //---------------------------------------------------------------------------------------------------------//

            /* If multiline command, read all lines
				C: A284 SEARCH CHARSET UTF-8 TEXT {6}
				S: + Continue                             ### THIS IS UNDOCUMENTED !!!
				C: XXXXXX[arg conitnue]<CRLF>
			*/
            argsText = argsText.Trim();
            while (argsText.EndsWith("}") && argsText.IndexOf("{") > -1)
            {
                long dataLength = 0;
                try
                {
                    // Get data length from {xxx}
                    dataLength =
                        Convert.ToInt64(argsText.Substring(argsText.LastIndexOf("{") + 1,
                                                           argsText.Length - argsText.LastIndexOf("{") - 2));
                }
                // There is no valid numeric value between {}, just skip and allow SearchGroup parser to handle this value
                catch
                {
                    break;
                }

                MemoryStream dataStream = new MemoryStream();

                WriteLine("+ Continue");
                ReadSpecifiedLength((int)dataLength, dataStream);
                string argsContinueLine = ReadLine();

                // Append readed data + [args conitnue] line
                argsText += EncodingTools.GetEncodingByCodepageName_Throws(charset).GetString(dataStream.ToArray()) + argsContinueLine;

                // There is no more argumets, stop getting. 
                // We must check this because if length = 0 and no args returned, last line ends with {0},
                // leaves this into endless loop.
                if (argsContinueLine == "")
                {
                    break;
                }
            }

            //--- Parse search criteria ------------------------//
            SearchGroup searchCriteria = new SearchGroup();
            try
            {
                searchCriteria.Parse(new StringReader(argsText));
            }
            catch (Exception x)
            {
                WriteLine(cmdTag + " NO " + x.Message);
                return;
            }
            //--------------------------------------------------//
            /*
			string searchResponse = "* SEARCH";

			// No header and body text needed, can do search on internal messages info data
			if(!searchCriteria.IsHeaderNeeded() && !searchCriteria.IsBodyTextNeeded()){
				// Loop internal messages info, see what messages match
				for(int i=0;i<m_Messages.Count;i++){
					IMAP_Message messageInfo = m_Messages[i];

					// See if message matches
					if(searchCriteria.Match(i,messageInfo.MessageUID,(int)messageInfo.Size,messageInfo.Date,messageInfo.Flags,null,"")){
						// For UID search we must return message UID's
						if(uidSearch){
							searchResponse += " " + messageInfo.MessageUID.ToString();
						}
						// For normal search we must return message index numbers.
						else{
							searchResponse += " " + messageInfo.MessageNo.ToString();							
						}
					}
				}
			}
			// Can't search on iternal messages info, need to do header or body text search, call Search event
			else{
				// Call 'Search' event, get search criteria matching messages
				IMAP_eArgs_Search eArgs = ImapServer.OnSearch(this,Core.Decode_IMAP_UTF7_String(this.SelectedMailbox),new IMAP_SearchMatcher(searchCriteria));
				
				// Constuct matching messages search response
				for(int i=0;i<eArgs.MatchingMessages.Count;i++){
					IMAP_Message messageInfo = eArgs.MatchingMessages[i];

					// For UID search we must return message UID's
					if(uidSearch){
						searchResponse += " " + messageInfo.MessageUID.ToString();
					}
					// For normal search we must return message index numbers.
					else{
						// Map search returnded message to internal message number
						int no = m_Messages.IndexFromUID(messageInfo.MessageUID);
						if(no > -1){
							searchResponse += " " + no.ToString();
						}
					}
				}
			}

			searchResponse += "\r\n";
			searchResponse += cmdTag + " OK SEARCH completed in " + ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2") + " seconds\r\n";
*/
            ProcessMailboxChanges();

            // Just loop messages headers or full messages (depends on search type)
            //	string searchResponse = "* SEARCH";
            TcpStream.Write("* SEARCH");
            string searchResponse = "";

            IMAP_MessageItems_enum messageItems = IMAP_MessageItems_enum.None;
            if (searchCriteria.IsBodyTextNeeded())
            {
                messageItems |= IMAP_MessageItems_enum.Message;
            }
            else if (searchCriteria.IsHeaderNeeded())
            {
                messageItems |= IMAP_MessageItems_enum.Header;
            }
            for (int i = 0; i < m_pSelectedFolder.Messages.Count; i++)
            {
                IMAP_Message msg = m_pSelectedFolder.Messages[i];

                //-- Get message only if matching needs it ------------------------//
                Mime parser = null;
                if ((messageItems & IMAP_MessageItems_enum.Message) != 0 ||
                    (messageItems & IMAP_MessageItems_enum.Header) != 0)
                {
                    // Raise event GetMessageItems, get requested message items.
                    IMAP_eArgs_MessageItems eArgs = ImapServer.OnGetMessageItems(this, msg, messageItems);

                    // Message data is null if no such message available, just skip that message
                    if (!eArgs.MessageExists)
                    {
                        continue;
                    }
                    try
                    {
                        // Ensure that all requested items were provided.
                        eArgs.Validate();
                    }
                    catch (Exception x)
                    {
                        ImapServer.OnSysError(x.Message, x);
                        WriteLine(cmdTag + " NO Internal IMAP server component error: " + x.Message);
                        return;
                    }

                    try
                    {
                        if (eArgs.MessageStream != null)
                        {
                            parser = Mime.Parse(eArgs.MessageStream);
                        }
                        else
                        {
                            parser = Mime.Parse(eArgs.Header);
                        }
                    }
                    // Message parsing failed, bad message. Just make new warning message.
                    catch (Exception x)
                    {
                        parser = Mime.CreateSimple(new AddressList(),
                                                   new AddressList(),
                                                   "[BAD MESSAGE] Bad message, message parsing failed !",
                                                   "NOTE: Bad message, message parsing failed !\r\n\r\n" +
                                                   x.Message,
                                                   "");
                    }
                }
                //-----------------------------------------------------------------//

                string bodyText = "";
                if (searchCriteria.IsBodyTextNeeded())
                {
                    bodyText = parser.BodyText;
                }

                // See if message matches to search criteria
                if (searchCriteria.Match(i,
                                         msg.UID,
                                         (int)msg.Size,
                                         msg.InternalDate,
                                         msg.Flags,
                                         parser,
                                         bodyText))
                {
                    if (uidSearch)
                    {
                        TcpStream.Write(" " + msg.UID);

                        //		searchResponse += " " + msg.MessageUID.ToString();
                    }
                    else
                    {
                        TcpStream.Write(" " + (i + 1));

                        //		searchResponse += " " + i.ToString();
                    }
                }
            }

            searchResponse += "\r\n";
            searchResponse += string.Format("{0} OK SEARCH completed in {1} seconds\r\n", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2"));

            // Send search server response
            TcpStream.Write(searchResponse);
        }

        private void ReadSpecifiedLength(int dataLength, MemoryStream dataStream)
        {
            TcpStream.ReadFixedCount(dataStream, dataLength);
        }

        private string ReadLine()
        {
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            if (TcpStream.ReadLine(args, false))
            {
                return args.LineUtf8;
            }
            return string.Empty;
        }


        private void Fetch(string cmdTag, string argsText, bool uidFetch)
        {
            /* Rfc 3501 6.4.5 FETCH Command
			
                Arguments:  message set
                            message data item names

                Responses:  untagged responses: FETCH

                Result:     OK - fetch completed
                            NO - fetch error: can't fetch that data
                            BAD - command unknown or arguments invalid

                The FETCH command retrieves data associated with a message in the
                mailbox.  The data items to be fetched can be either a single atom
                or a parenthesized list.
				
            Most data items, identified in the formal syntax under the
            msg-att-static rule, are static and MUST NOT change for any
            particular message.  Other data items, identified in the formal
            syntax under the msg-att-dynamic rule, MAY change, either as a
            result of a STORE command or due to external events.

                For example, if a client receives an ENVELOPE for a
                message when it already knows the envelope, it can
                safely ignore the newly transmitted envelope.

            There are three macros which specify commonly-used sets of data
            items, and can be used instead of data items.  A macro must be
            used by itself, and not in conjunction with other macros or data
            items.
			
            ALL
                Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE)

            FAST
                Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE)

            FULL
                Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE
                BODY)

            The currently defined data items that can be fetched are:

            BODY
                Non-extensible form of BODYSTRUCTURE.

            BODY[<section>]<<partial>>
                The text of a particular body section.  The section
                specification is a set of zero or more part specifiers
                delimited by periods.  A part specifier is either a part number
                or one of the following: HEADER, HEADER.FIELDS,
                HEADER.FIELDS.NOT, MIME, and TEXT.  An empty section
                specification refers to the entire message, including the
                header.

                Every message has at least one part number.  Non-[MIME-IMB]
                messages, and non-multipart [MIME-IMB] messages with no
                encapsulated message, only have a part 1.

                Multipart messages are assigned consecutive part numbers, as
                they occur in the message.  If a particular part is of type
                message or multipart, its parts MUST be indicated by a period
                followed by the part number within that nested multipart part.

                A part of type MESSAGE/RFC822 also has nested part numbers,
                referring to parts of the MESSAGE part's body.

                The HEADER, HEADER.FIELDS, HEADER.FIELDS.NOT, and TEXT part
                specifiers can be the sole part specifier or can be prefixed by
                one or more numeric part specifiers, provided that the numeric
                part specifier refers to a part of type MESSAGE/RFC822.  The
                MIME part specifier MUST be prefixed by one or more numeric
                part specifiers.

                The HEADER, HEADER.FIELDS, and HEADER.FIELDS.NOT part
                specifiers refer to the [RFC-2822] header of the message or of
                an encapsulated [MIME-IMT] MESSAGE/RFC822 message.
                HEADER.FIELDS and HEADER.FIELDS.NOT are followed by a list of
                field-name (as defined in [RFC-2822]) names, and return a
                subset of the header.  The subset returned by HEADER.FIELDS
                contains only those header fields with a field-name that
                matches one of the names in the list; similarly, the subset
                returned by HEADER.FIELDS.NOT contains only the header fields
                with a non-matching field-name.  The field-matching is
                case-insensitive but otherwise exact.  Subsetting does not
                exclude the [RFC-2822] delimiting blank line between the header
                and the body; the blank line is included in all header fetches,
                except in the case of a message which has no body and no blank
                line.

                The MIME part specifier refers to the [MIME-IMB] header for
                this part.

                The TEXT part specifier refers to the text body of the message,
                omitting the [RFC-2822] header.

                    Here is an example of a complex message with some of its
                    part specifiers:

                    HEADER     ([RFC-2822] header of the message)
                    TEXT       ([RFC-2822] text body of the message) MULTIPART/MIXED
                    1          TEXT/PLAIN
                    2          APPLICATION/OCTET-STREAM
                    3          MESSAGE/RFC822
                    3.HEADER   ([RFC-2822] header of the message)
                    3.TEXT     ([RFC-2822] text body of the message) MULTIPART/MIXED
                    3.1        TEXT/PLAIN
                    3.2        APPLICATION/OCTET-STREAM
                    4          MULTIPART/MIXED
                    4.1        IMAGE/GIF
                    4.1.MIME   ([MIME-IMB] header for the IMAGE/GIF)
                    4.2        MESSAGE/RFC822
                    4.2.HEADER ([RFC-2822] header of the message)
                    4.2.TEXT   ([RFC-2822] text body of the message) MULTIPART/MIXED
                    4.2.1      TEXT/PLAIN
                    4.2.2      MULTIPART/ALTERNATIVE
                    4.2.2.1    TEXT/PLAIN
                    4.2.2.2    TEXT/RICHTEXT


                It is possible to fetch a substring of the designated text.
                This is done by appending an open angle bracket ("<"), the
                octet position of the first desired octet, a period, the
                maximum number of octets desired, and a close angle bracket
                (">") to the part specifier.  If the starting octet is beyond
                the end of the text, an empty string is returned.
				
                Any partial fetch that attempts to read beyond the end of the
                text is truncated as appropriate.  A partial fetch that starts
                at octet 0 is returned as a partial fetch, even if this
                truncation happened.

                    Note: This means that BODY[]<0.2048> of a 1500-octet message
                    will return BODY[]<0> with a literal of size 1500, not
                    BODY[].

                    Note: A substring fetch of a HEADER.FIELDS or
                    HEADER.FIELDS.NOT part specifier is calculated after
                    subsetting the header.

                The \Seen flag is implicitly set; if this causes the flags to
                change, they SHOULD be included as part of the FETCH responses.

            BODY.PEEK[<section>]<<partial>>
                An alternate form of BODY[<section>] that does not implicitly
                set the \Seen flag.

            BODYSTRUCTURE
                The [MIME-IMB] body structure of the message.  This is computed
                by the server by parsing the [MIME-IMB] header fields in the
                [RFC-2822] header and [MIME-IMB] headers.

            ENVELOPE
                The envelope structure of the message.  This is computed by the
                server by parsing the [RFC-2822] header into the component
                parts, defaulting various fields as necessary.

            FLAGS
                The flags that are set for this message.

            INTERNALDATE
                The internal date of the message.

            RFC822
                Functionally equivalent to BODY[], differing in the syntax of
                the resulting untagged FETCH data (RFC822 is returned).

            RFC822.HEADER
                Functionally equivalent to BODY.PEEK[HEADER], differing in the
                syntax of the resulting untagged FETCH data (RFC822.HEADER is
                returned).

            RFC822.SIZE
                The [RFC-2822] size of the message.
				
            RFC822.TEXT
                Functionally equivalent to BODY[TEXT], differing in the syntax
                of the resulting untagged FETCH data (RFC822.TEXT is returned).

            UID
                The unique identifier for the message.


            Example:    C: A654 FETCH 2:4 (FLAGS BODY[HEADER.FIELDS (DATE FROM)])
                        S: * 2 FETCH ....
                        S: * 3 FETCH ....
                        S: * 4 FETCH ....
                        S: A654 OK FETCH completed
	  
            */
            if (!this.IsAuthenticated)
            {
                TcpStream.WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                this.TcpStream.WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            TcpStream.MemoryBuffer = true;

            // Store start time
            long startTime = DateTime.Now.Ticks;

            IMAP_MessageItems_enum messageItems = IMAP_MessageItems_enum.None;

            #region Parse parameters

            string[] args = ParseParams(argsText);
            if (args.Length != 2)
            {
                this.TcpStream.WriteLine(string.Format("{0} BAD Invalid arguments", cmdTag));
                return;
            }

            IMAP_SequenceSet sequenceSet = new IMAP_SequenceSet();
            // Just try if it can be parsed as sequence-set
            try
            {
                if (uidFetch)
                {
                    if (m_pSelectedFolder.Messages.Count > 0)
                    {
                        sequenceSet.Parse(args[0], m_pSelectedFolder.Messages[m_pSelectedFolder.Messages.Count - 1].UID);
                    }
                }
                else
                {
                    sequenceSet.Parse(args[0], m_pSelectedFolder.Messages.Count);
                }
            }
            // This isn't valid sequnce-set value
            catch
            {
                this.TcpStream.WriteLine(string.Format("{0} BAD Invalid <sequnce-set> value '{1}' Syntax: {{<command-tag> FETCH <sequnce-set> (<fetch-keys>)}}!", cmdTag, args[0]));
                return;
            }

            // Replace macros
            string fetchItems = args[1].ToUpper();
            fetchItems = fetchItems.Replace("ALL", "FLAGS INTERNALDATE RFC822.SIZE ENVELOPE");
            fetchItems = fetchItems.Replace("FAST", "FLAGS INTERNALDATE RFC822.SIZE");
            fetchItems = fetchItems.Replace("FULL", "FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY");

            // If UID FETCH and no UID, we must implicity add it, it's required 
            if (uidFetch && fetchItems.ToUpper().IndexOf("UID") == -1)
            {
                fetchItems += " UID";
            }

            // Start parm parsing from left to end in while loop while params parsed or bad param found
            ArrayList fetchFlags = new ArrayList();
            StringReader argsReader = new StringReader(fetchItems.Trim());
            while (argsReader.Available > 0)
            {
                argsReader.ReadToFirstChar();

                #region BODYSTRUCTURE

                // BODYSTRUCTURE
                if (argsReader.StartsWith("BODYSTRUCTURE"))
                {
                    argsReader.ReadSpecifiedLength("BODYSTRUCTURE".Length);

                    fetchFlags.Add(new object[] { "BODYSTRUCTURE" });
                    messageItems |= IMAP_MessageItems_enum.BodyStructure;
                }

                #endregion

                #region BODY, BODY[<section>]<<partial>>, BODY.PEEK[<section>]<<partial>>

                // BODY, BODY[<section>]<<partial>>, BODY.PEEK[<section>]<<partial>>
                else if (argsReader.StartsWith("BODY"))
                {
                    // Remove BODY
                    argsReader.ReadSpecifiedLength("BODY".Length);

                    bool peek = false;
                    // BODY.PEEK
                    if (argsReader.StartsWith(".PEEK"))
                    {
                        // Remove .PEEK
                        argsReader.ReadSpecifiedLength(".PEEK".Length);

                        peek = true;
                    }

                    // [<section>]<<partial>>
                    if (argsReader.StartsWith("["))
                    {
                        // Read value between []
                        string section = "";
                        try
                        {
                            section = argsReader.ReadParenthesized();
                        }
                        catch
                        {
                            this.TcpStream.WriteLine(cmdTag + " BAD Invalid BODY[], closing ] parenthesize is missing !");
                            return;
                        }

                        string originalSectionValue = section;
                        string mimePartsSpecifier = "";
                        string sectionType = "";
                        string sectionArgs = "";
                        /* Validate <section>
                             Section can be:
                                ""                                                    - entire message								
                                [MimePartsSepcifier.]HEADER                           - message header
                                [MimePartsSepcifier.]HEADER.FIELDS (headerFields)     - message header fields
                                [MimePartsSepcifier.]HEADER.FIELDS.NOT (headerFields) - message header fields except requested
                                [MimePartsSepcifier.]TEXT                             - message text
                                [MimePartsSepcifier.]MIME                             - same as header, different response
                        */
                        if (section.Length > 0)
                        {
                            string[] section_args = section.Split(new char[] { ' ' }, 2);
                            section = section_args[0];
                            if (section_args.Length == 2)
                            {
                                sectionArgs = section_args[1];
                            }

                            if (section.EndsWith("HEADER"))
                            {
                                // Remove HEADER from end
                                section = section.Substring(0, section.Length - "HEADER".Length);

                                sectionType = "HEADER";
                                messageItems |= IMAP_MessageItems_enum.Header;
                            }
                            else if (section.EndsWith("HEADER.FIELDS"))
                            {
                                // Remove HEADER.FIELDS from end
                                section = section.Substring(0, section.Length - "HEADER.FIELDS".Length);

                                sectionType = "HEADER.FIELDS";
                                messageItems |= IMAP_MessageItems_enum.Header;
                            }
                            else if (section.EndsWith("HEADER.FIELDS.NOT"))
                            {
                                // Remove HEADER.FIELDS.NOT from end
                                section = section.Substring(0, section.Length - "HEADER.FIELDS.NOT".Length);

                                sectionType = "HEADER.FIELDS.NOT";
                                messageItems |= IMAP_MessageItems_enum.Header;
                            }
                            else if (section.EndsWith("TEXT"))
                            {
                                // Remove TEXT from end
                                section = section.Substring(0, section.Length - "TEXT".Length);

                                sectionType = "TEXT";
                                messageItems |= IMAP_MessageItems_enum.Message;
                            }
                            else if (section.EndsWith("MIME"))
                            {
                                // Remove MIME from end
                                section = section.Substring(0, section.Length - "MIME".Length);

                                sectionType = "MIME";
                                messageItems = IMAP_MessageItems_enum.Header;
                            }

                            // Remove last ., if there is any
                            if (section.EndsWith("."))
                            {
                                section = section.Substring(0, section.Length - 1);
                            }

                            // MimePartsSepcifier is specified, validate it. It can contain numbers only.
                            if (section.Length > 0)
                            {
                                // Now we certainly need full message, because nested mime parts wanted
                                messageItems |= IMAP_MessageItems_enum.Message;

                                string[] sectionParts = section.Split('.');
                                foreach (string sectionPart in sectionParts)
                                {
                                    if (!Core.IsNumber(sectionPart))
                                    {
                                        this.TcpStream.WriteLine(string.Format("{0} BAD Invalid BODY[<section>] argument. Invalid <section>: {1}", cmdTag, section));
                                        return;
                                    }
                                }

                                mimePartsSpecifier = section;
                            }
                        }
                        else
                        {
                            messageItems |= IMAP_MessageItems_enum.Message;
                        }

                        long startPosition = -1;
                        long length = -1;
                        // See if partial fetch
                        if (argsReader.StartsWith("<"))
                        {
                            /* <partial> syntax:
									startPosition[.endPosition]							  
							*/

                            // Read partial value between <>
                            string partial = "";
                            try
                            {
                                partial = argsReader.ReadParenthesized();
                            }
                            catch
                            {
                                this.TcpStream.WriteLine(string.Format("{0} BAD Invalid BODY[]<start[.length]>, closing > parenthesize is missing !", cmdTag));
                                return;
                            }

                            string[] start_length = partial.Split('.');

                            // Validate <partial>
                            if (start_length.Length == 0 || start_length.Length > 2 || !Core.IsNumber(start_length[0]) || (start_length.Length == 2 && !Core.IsNumber(start_length[1])))
                            {
                                this.TcpStream.WriteLine(string.Format("{0} BAD Invalid BODY[]<partial> argument. Invalid <partial>: {1}", cmdTag, partial));
                                return;
                            }

                            startPosition = Convert.ToInt64(start_length[0]);
                            if (start_length.Length == 2)
                            {
                                length = Convert.ToInt64(start_length[1]);
                            }
                        }

                        // object[] structure for BODY[]
                        //	fetchFlagName
                        //	isPeek
                        //	mimePartsSpecifier
                        //  originalSectionValue
                        //	sectionType
                        //	sectionArgs
                        //	startPosition
                        //	length
                        fetchFlags.Add(new object[] { "BODY[]", peek, mimePartsSpecifier, originalSectionValue, sectionType, sectionArgs, startPosition, length });
                    }
                    // BODY
                    else
                    {
                        fetchFlags.Add(new object[] { "BODY" });
                        messageItems |= IMAP_MessageItems_enum.BodyStructure;
                    }
                }

                #endregion

                #region ENVELOPE

                // ENVELOPE
                else if (argsReader.StartsWith("ENVELOPE"))
                {
                    argsReader.ReadSpecifiedLength("ENVELOPE".Length);

                    fetchFlags.Add(new object[] { "ENVELOPE" });
                    messageItems |= IMAP_MessageItems_enum.Envelope;
                }

                #endregion

                #region FLAGS

                // FLAGS
                //	The flags that are set for this message.
                else if (argsReader.StartsWith("FLAGS"))
                {
                    argsReader.ReadSpecifiedLength("FLAGS".Length);

                    fetchFlags.Add(new object[] { "FLAGS" });
                }

                #endregion

                #region INTERNALDATE

                // INTERNALDATE
                else if (argsReader.StartsWith("INTERNALDATE"))
                {
                    argsReader.ReadSpecifiedLength("INTERNALDATE".Length);

                    fetchFlags.Add(new object[] { "INTERNALDATE" });
                }

                #endregion

                #region RFC822.HEADER

                // RFC822.HEADER
                else if (argsReader.StartsWith("RFC822.HEADER"))
                {
                    argsReader.ReadSpecifiedLength("RFC822.HEADER".Length);

                    fetchFlags.Add(new object[] { "RFC822.HEADER" });
                    messageItems |= IMAP_MessageItems_enum.Header;
                }

                #endregion

                #region RFC822.SIZE

                // RFC822.SIZE
                //	The [RFC-2822] size of the message.
                else if (argsReader.StartsWith("RFC822.SIZE"))
                {
                    argsReader.ReadSpecifiedLength("RFC822.SIZE".Length);

                    fetchFlags.Add(new object[] { "RFC822.SIZE" });
                }

                #endregion

                #region RFC822.TEXT

                // RFC822.TEXT
                else if (argsReader.StartsWith("RFC822.TEXT"))
                {
                    argsReader.ReadSpecifiedLength("RFC822.TEXT".Length);

                    fetchFlags.Add(new object[] { "RFC822.TEXT" });
                    messageItems |= IMAP_MessageItems_enum.Message;
                }

                #endregion

                #region RFC822

                // RFC822 NOTE: RFC822 must be below RFC822.xxx or is parsed wrong !
                else if (argsReader.StartsWith("RFC822"))
                {
                    argsReader.ReadSpecifiedLength("RFC822".Length);

                    fetchFlags.Add(new object[] { "RFC822" });
                    messageItems |= IMAP_MessageItems_enum.Message;
                }

                #endregion

                #region UID

                // UID
                //	The unique identifier for the message.
                else if (argsReader.StartsWith("UID"))
                {
                    argsReader.ReadSpecifiedLength("UID".Length);

                    fetchFlags.Add(new object[] { "UID" });
                }

                #endregion

                // This must be unknown fetch flag
                else
                {
                    this.TcpStream.WriteLine(string.Format("{0} BAD Invalid fetch-items argument. Unkown part starts from: {1}", cmdTag, argsReader.SourceString));
                    return;
                }
            }

            #endregion

            // ToDo: ??? But non of the servers do it ?
            // The server should respond with a tagged BAD response to a command that uses a message
            // sequence number greater than the number of messages in the selected mailbox.  This
            // includes "*" if the selected mailbox is empty.
            //	if(m_Messages.Count == 0 || ){
            //		SendData(cmdTag + " BAD Sequence number greater than the number of messages in the selected mailbox !\r\n");
            //		return;
            //	}

            // Create buffered writer, so we make less network calls.

            for (int i = 0; i < m_pSelectedFolder.Messages.Count; i++)
            {
                IMAP_Message msg = m_pSelectedFolder.Messages[i];

                // For UID FETCH we must compare UIDs and for normal FETCH message numbers. 
                bool sequenceSetContains = false;
                if (uidFetch)
                {
                    sequenceSetContains = sequenceSet.Contains(msg.UID);
                }
                else
                {
                    sequenceSetContains = sequenceSet.Contains(i + 1);
                }

                if (sequenceSetContains)
                {
                    IMAP_eArgs_MessageItems eArgs = null;
                    // Get message items only if they are needed.
                    if (messageItems != IMAP_MessageItems_enum.None)
                    {
                        // Raise event GetMessageItems to get all neccesary message itmes
                        eArgs = ImapServer.OnGetMessageItems(this, msg, messageItems);

                        // Message doesn't exist any more, notify email client.
                        if (!eArgs.MessageExists)
                        {
                            TcpStream.Write("* " + msg.SequenceNo + " EXPUNGE");
                            ImapServer.OnDeleteMessage(this, msg);
                            m_pSelectedFolder.Messages.Remove(msg);
                            i--;
                            continue;
                        }
                        try
                        {
                            // Ensure that all requested items were provided.
                            eArgs.Validate();
                        }
                        catch (Exception x)
                        {
                            ImapServer.OnSysError(x.Message, x);
                            this.TcpStream.WriteLine(string.Format("{0} NO Internal IMAP server component error: {1}", cmdTag, x.Message));
                            return;
                        }
                    }

                    // Write fetch start data "* msgNo FETCH ("
                    TcpStream.Write("* " + (i + 1) + " FETCH (");

                    IMAP_MessageFlags msgFlagsOr = msg.Flags;
                    // Construct reply here, based on requested fetch items
                    int nCount = 0;
                    foreach (object[] fetchFlag in fetchFlags)
                    {
                        string fetchFlagName = (string)fetchFlag[0];

                        #region BODY

                        // BODY
                        if (fetchFlagName == "BODY")
                        {
                            // Sets \seen flag
                            msg.SetFlags(msg.Flags | IMAP_MessageFlags.Seen);

                            // BODY ()
                            TcpStream.Write("BODY " + eArgs.BodyStructure);
                        }

                        #endregion

                        #region BODY[], BODY.PEEK[]

                        // BODY[<section>]<<partial>>, BODY.PEEK[<section>]<<partial>>
                        else if (fetchFlagName == "BODY[]")
                        {
                            // Force to write all buffered data.
                            TcpStream.Flush();

                            // object[] structure for BODY[]
                            //	fetchFlagName
                            //	isPeek
                            //	mimePartsSpecifier
                            //  originalSectionValue
                            //	sectionType
                            //	sectionArgs
                            //	startPosition
                            //	length
                            bool isPeek = (bool)fetchFlag[1];
                            string mimePartsSpecifier = (string)fetchFlag[2];
                            string originalSectionValue = (string)fetchFlag[3];
                            string sectionType = (string)fetchFlag[4];
                            string sectionArgs = (string)fetchFlag[5];
                            long startPosition = (long)fetchFlag[6];
                            long length = (long)fetchFlag[7];

                            // Difference between BODY[] and BODY.PEEK[] is that .PEEK won't set seen flag
                            if (!isPeek)
                            {
                                // Sets \seen flag
                                msg.SetFlags(msg.Flags | IMAP_MessageFlags.Seen);
                            }

                            /* Section value:
                                ""                - entire message								
                                HEADER            - message header
                                HEADER.FIELDS     - message header fields
                                HEADER.FIELDS.NOT - message header fields except requested
                                TEXT              - message text
                                MIME              - same as header, different response
                            */
                            Stream dataStream = null;
                            if (sectionType == "" && mimePartsSpecifier == "")
                            {
                                dataStream = eArgs.MessageStream;
                            }
                            else
                            {
                                Mime parser = null;
                                try
                                {
                                    if (eArgs.MessageStream == null)
                                    {
                                        parser = Mime.Parse(eArgs.Header);
                                    }
                                    else
                                    {
                                        parser = Mime.Parse(eArgs.MessageStream);
                                    }
                                }
                                // Invalid message, parsing failed
                                catch
                                {
                                    parser = Mime.CreateSimple(new AddressList(), new AddressList(), "BAD Message", "This is BAD message, mail server failed to parse it !", "");
                                }
                                MimeEntity currentEntity = parser.MainEntity;
                                // Specific mime entity requested, get it
                                if (mimePartsSpecifier != "")
                                {
                                    currentEntity = FetchHelper.GetMimeEntity(parser, mimePartsSpecifier);
                                }

                                if (currentEntity != null)
                                {
                                    if (sectionType == "HEADER")
                                    {
                                        dataStream = new MemoryStream(FetchHelper.GetMimeEntityHeader(currentEntity));
                                    }
                                    else if (sectionType == "HEADER.FIELDS")
                                    {
                                        dataStream = new MemoryStream(FetchHelper.ParseHeaderFields(sectionArgs, currentEntity));
                                    }
                                    else if (sectionType == "HEADER.FIELDS.NOT")
                                    {
                                        dataStream = new MemoryStream(FetchHelper.ParseHeaderFieldsNot(sectionArgs, currentEntity));
                                    }
                                    else if (sectionType == "TEXT")
                                    {
                                        try
                                        {
                                            if (currentEntity.DataEncoded != null)
                                            {
                                                dataStream = new MemoryStream(currentEntity.DataEncoded);
                                            }
                                        }
                                        catch
                                        { // This probably multipart entity, data isn't available
                                        }
                                    }
                                    else if (sectionType == "MIME")
                                    {
                                        dataStream = new MemoryStream(FetchHelper.GetMimeEntityHeader(currentEntity));
                                    }
                                    else if (sectionType == "")
                                    {
                                        try
                                        {
                                            dataStream = new MemoryStream(currentEntity.DataEncoded);
                                        }
                                        catch
                                        { // This probably multipart entity, data isn't available
                                        }
                                    }
                                }
                            }

                            // Partial fetch. Reports <origin position> in fetch reply.
                            if (startPosition > -1)
                            {
                                if (dataStream == null)
                                {
                                    this.TcpStream.Write("BODY[" + originalSectionValue + "]<" + startPosition.ToString() + "> \"\"\r\n");
                                }
                                else
                                {
                                    long lengthToSend = length;
                                    if (lengthToSend == -1)
                                    {
                                        lengthToSend = (dataStream.Length - dataStream.Position) - startPosition;
                                    }
                                    if ((lengthToSend + startPosition) > (dataStream.Length - dataStream.Position))
                                    {
                                        lengthToSend = (dataStream.Length - dataStream.Position) - startPosition;
                                    }

                                    if (startPosition >= (dataStream.Length - dataStream.Position))
                                    {
                                        this.TcpStream.Write("BODY[" + originalSectionValue + "]<" + startPosition.ToString() + "> \"\"\r\n");
                                    }
                                    else
                                    {
                                        this.TcpStream.Write("BODY[" + originalSectionValue + "]<" + startPosition.ToString() + "> {" + lengthToSend + "}\r\n");
                                        dataStream.Position += startPosition;
                                        this.TcpStream.WriteStream(dataStream, lengthToSend);
                                    }
                                }
                            }
                            // Normal fetch
                            else
                            {
                                if (dataStream == null)
                                {
                                    this.TcpStream.Write("BODY[" + originalSectionValue + "] \"\"\r\n");
                                }
                                else
                                {
                                    this.TcpStream.Write("BODY[" + originalSectionValue + "] {" + (dataStream.Length - dataStream.Position) + "}\r\n");
                                    this.TcpStream.WriteStream(dataStream);
                                }
                            }
                        }

                        #endregion

                        #region BODYSTRUCTURE

                        // BODYSTRUCTURE
                        else if (fetchFlagName == "BODYSTRUCTURE")
                        {
                            TcpStream.Write("BODYSTRUCTURE " + eArgs.BodyStructure);
                        }

                        #endregion

                        #region ENVELOPE

                        // ENVELOPE
                        else if (fetchFlagName == "ENVELOPE")
                        {
                            TcpStream.Write("ENVELOPE " + eArgs.Envelope);
                        }

                        #endregion

                        #region FLAGS

                        // FLAGS
                        else if (fetchFlagName == "FLAGS")
                        {
                            TcpStream.Write("FLAGS (" + msg.FlagsString + ")");
                        }

                        #endregion

                        #region INTERNALDATE

                        // INTERNALDATE
                        else if (fetchFlagName == "INTERNALDATE")
                        {
                            // INTERNALDATE "date"
                            TcpStream.Write("INTERNALDATE \"" + IMAP_Utils.DateTimeToString(msg.InternalDate) + "\"");
                        }

                        #endregion

                        #region RFC822

                        // RFC822
                        else if (fetchFlagName == "RFC822")
                        {
                            // Force to write all buffered data.
                            TcpStream.Flush();

                            // Sets \seen flag
                            msg.SetFlags(msg.Flags | IMAP_MessageFlags.Seen);

                            // RFC822 {size}
                            // msg data
                            this.TcpStream.Write("RFC822 {" + eArgs.MessageSize.ToString() + "}\r\n");
                            this.TcpStream.WriteStream(eArgs.MessageStream);
                        }

                        #endregion

                        #region RFC822.HEADER

                        // RFC822.HEADER
                        else if (fetchFlagName == "RFC822.HEADER")
                        {
                            // Force to write all buffered data.
                            TcpStream.Flush();

                            // RFC822.HEADER {size}
                            // msg header data
                            this.TcpStream.Write("RFC822.HEADER {" + eArgs.Header.Length + "}\r\n");
                            this.TcpStream.Write(eArgs.Header);
                        }

                        #endregion

                        #region RFC822.SIZE

                        // RFC822.SIZE
                        else if (fetchFlagName == "RFC822.SIZE")
                        {
                            // RFC822.SIZE size
                            TcpStream.Write("RFC822.SIZE " + msg.Size);
                        }

                        #endregion

                        #region RFC822.TEXT

                        // RFC822.TEXT
                        else if (fetchFlagName == "RFC822.TEXT")
                        {
                            // Force to write all buffered data.
                            TcpStream.Flush();

                            // Sets \seen flag
                            msg.SetFlags(msg.Flags | IMAP_MessageFlags.Seen);

                            //--- Find body text entity ------------------------------------//
                            Mime parser = Mime.Parse(eArgs.MessageStream);
                            MimeEntity bodyTextEntity = null;
                            if (parser.MainEntity.ContentType == MediaType_enum.NotSpecified)
                            {
                                if (parser.MainEntity.DataEncoded != null)
                                {
                                    bodyTextEntity = parser.MainEntity;
                                }
                            }
                            else
                            {
                                MimeEntity[] entities = parser.MimeEntities;
                                foreach (MimeEntity entity in entities)
                                {
                                    if (entity.ContentType == MediaType_enum.Text_plain)
                                    {
                                        bodyTextEntity = entity;
                                        break;
                                    }
                                }
                            }
                            //----------------------------------------------------------------//

                            // RFC822.TEXT {size}
                            // msg text	
                            byte[] data = null;
                            if (bodyTextEntity != null)
                            {
                                data = bodyTextEntity.DataEncoded;
                            }
                            else
                            {
                                data = System.Text.Encoding.ASCII.GetBytes("");
                            }

                            this.TcpStream.Write("RFC822.TEXT {" + data.Length + "}\r\n");
                            this.TcpStream.Write(data);
                        }

                        #endregion

                        #region UID

                        // UID
                        else if (fetchFlagName == "UID")
                        {
                            TcpStream.Write("UID " + msg.UID);
                        }

                        #endregion

                        nCount++;

                        // Write fetch item separator data " "
                        // We don't write it for last item
                        if (nCount < fetchFlags.Count)
                        {
                            TcpStream.Write(" ");
                        }
                    }

                    // Write fetch end data ")"
                    TcpStream.Write(")\r\n");

                    // Free event args, close message stream, ... .
                    if (eArgs != null)
                    {
                        eArgs.Dispose();
                    }

                    // Set message flags here if required or changed
                    if (((int)IMAP_MessageFlags.Recent & (int)msg.Flags) != 0 || msgFlagsOr != msg.Flags)
                    {
                        msg.SetFlags(msg.Flags & ~IMAP_MessageFlags.Recent);

                        ImapServer.OnStoreMessageFlags(this, msg);
                    }
                }
            }

            // Force to write all buffered data.
            TcpStream.Flush();

            this.TcpStream.WriteLine(string.Format("{0} OK FETCH completed in {1} seconds", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2")));
        }

        

        private void Store(string cmdTag, string argsText, bool uidStore)
        {
            /* Rfc 3501 6.4.6 STORE Command
				
				Arguments:  message set
							message data item name
							value for message data item

				Responses:  untagged responses: FETCH

				Result:     OK - store completed
							NO - store error: can't store that data
							BAD - command unknown or arguments invalid
							
				The STORE command alters data associated with a message in the
				mailbox.  Normally, STORE will return the updated value of the
				data with an untagged FETCH response.  A suffix of ".SILENT" in
				the data item name prevents the untagged FETCH, and the server
				SHOULD assume that the client has determined the updated value
				itself or does not care about the updated value.
				
				Note: regardless of whether or not the ".SILENT" suffix was
					used, the server SHOULD send an untagged FETCH response if a
					change to a message's flags from an external source is
					observed.  The intent is that the status of the flags is
					determinate without a race condition.

				The currently defined data items that can be stored are:

				FLAGS <flag list>
					Replace the flags for the message (other than \Recent) with the
					argument.  The new value of the flags is returned as if a FETCH
					of those flags was done.

				FLAGS.SILENT <flag list>
					Equivalent to FLAGS, but without returning a new value.

				+FLAGS <flag list>
					Add the argument to the flags for the message.  The new value
					of the flags is returned as if a FETCH of those flags was done.

				+FLAGS.SILENT <flag list>
					Equivalent to +FLAGS, but without returning a new value.

				-FLAGS <flag list>
					Remove the argument from the flags for the message.  The new
					value of the flags is returned as if a FETCH of those flags was
					done.

				-FLAGS.SILENT <flag list>
					Equivalent to -FLAGS, but without returning a new value.
		 

				Example:    C: A003 STORE 2:4 +FLAGS (\Deleted)
							S: * 2 FETCH FLAGS (\Deleted \Seen)
							S: * 3 FETCH FLAGS (\Deleted)
							S: * 4 FETCH FLAGS (\Deleted \Flagged \Seen)
							S: A003 OK STORE completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }
            if (m_pSelectedFolder.ReadOnly)
            {
                WriteLine(string.Format("{0} NO Mailbox is read-only", cmdTag));
                return;
            }

            // Store start time
            long startTime = DateTime.Now.Ticks;

            string[] args = ParseParams(argsText);
            if (args.Length != 3)
            {
                WriteLine(string.Format("{0} BAD STORE invalid arguments. Syntax: {{<command-tag> STORE <sequnce-set> <data-item> (<message-flags>)}}", cmdTag));
                return;
            }

            IMAP_SequenceSet sequenceSet = new IMAP_SequenceSet();
            // Just try if it can be parsed as sequence-set
            try
            {
                if (uidStore)
                {
                    if (m_pSelectedFolder.Messages.Count > 0)
                    {
                        sequenceSet.Parse(args[0],
                                          m_pSelectedFolder.Messages[m_pSelectedFolder.Messages.Count - 1].UID);
                    }
                }
                else
                {
                    sequenceSet.Parse(args[0], m_pSelectedFolder.Messages.Count);
                }
            }
            // This isn't vaild sequnce-set value
            catch
            {
                WriteLine(string.Format("{0}BAD Invalid <sequnce-set> value '{1}' Syntax: {{<command-tag> STORE <sequnce-set> <data-item> (<message-flags>)}}!", cmdTag, args[0]));
                return;
            }

            //--- Parse Flags behaviour ---------------//
            string flagsAction = "";
            bool silent = false;
            string flagsType = args[1].ToUpper();
            switch (flagsType)
            {
                case "FLAGS":
                    flagsAction = "REPLACE";
                    break;

                case "FLAGS.SILENT":
                    flagsAction = "REPLACE";
                    silent = true;
                    break;

                case "+FLAGS":
                    flagsAction = "ADD";
                    break;

                case "+FLAGS.SILENT":
                    flagsAction = "ADD";
                    silent = true;
                    break;

                case "-FLAGS":
                    flagsAction = "REMOVE";
                    break;

                case "-FLAGS.SILENT":
                    flagsAction = "REMOVE";
                    silent = true;
                    break;

                default:
                    WriteLine(cmdTag + " BAD arguments invalid");
                    return;
            }
            //-------------------------------------------//

            //--- Parse flags, see if valid ----------------
            string flags = args[2].ToUpper();
            if (
                flags.Replace("\\ANSWERED", "").Replace("\\FLAGGED", "").Replace("\\DELETED", "").Replace(
                    "\\SEEN", "").Replace("\\DRAFT", "").Trim().Length > 0)
            {
                WriteLine(string.Format("{0} BAD arguments invalid", cmdTag));
                return;
            }

            IMAP_MessageFlags mFlags = IMAP_Utils.ParseMessageFlags(flags);

            // Call OnStoreMessageFlags for each message in sequence set
            // Calulate new flags(using old message flags + new flags) for message 
            // and request to store all flags to message, don't specify if add, remove or replace falgs.

            for (int i = 0; i < m_pSelectedFolder.Messages.Count; i++)
            {
                IMAP_Message msg = m_pSelectedFolder.Messages[i];

                // For UID STORE we must compare UIDs and for normal STORE message numbers. 
                bool sequenceSetContains = false;
                if (uidStore)
                {
                    sequenceSetContains = sequenceSet.Contains(msg.UID);
                }
                else
                {
                    sequenceSetContains = sequenceSet.Contains(i + 1);
                }

                if (sequenceSetContains)
                {
                    // Calculate new flags and set to msg
                    switch (flagsAction)
                    {
                        case "REPLACE":
                            msg.SetFlags(mFlags);
                            break;

                        case "ADD":
                            msg.SetFlags(msg.Flags | mFlags);
                            break;

                        case "REMOVE":
                            msg.SetFlags(msg.Flags & ~mFlags);
                            break;
                    }

                    // ToDo: see if flags changed, if not don't call OnStoreMessageFlags

                    string errorText = ImapServer.OnStoreMessageFlags(this, msg);
                    if (errorText == null)
                    {
                        if (!silent)
                        {
                            // Silent doesn't reply untagged lines
                            if (!uidStore)
                            {
                                WriteLine(string.Format("* {0} FETCH FLAGS ({1})", (i + 1), msg.FlagsString));
                            }
                            // Called from UID command, need to add UID response
                            else
                            {
                                WriteLine(string.Format("* {0} FETCH (FLAGS ({1}) UID {2}))", (i + 1), msg.FlagsString, msg.UID));
                            }
                        }
                    }
                    else
                    {
                        WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
                        return;
                    }
                }
            }

            WriteLine(string.Format("{0} OK STORE completed in {1} seconds", cmdTag, ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2")));
        }

        private void Copy(string cmdTag, string argsText, bool uidCopy)
        {
            /* RFC 3501 6.4.7 COPY Command
			
				Arguments:  message set
							mailbox name

				Responses:  no specific responses for this command

				Result:     OK - copy completed
							NO - copy error: can't copy those messages or to that
									name
							BAD - command unknown or arguments invalid
			   
				The COPY command copies the specified message(s) to the end of the
				specified destination mailbox.  The flags and internal date of the
				message(s) SHOULD be preserved in the copy.

				If the destination mailbox does not exist, a server SHOULD return
				an error.  It SHOULD NOT automatically create the mailbox.  Unless
				it is certain that the destination mailbox can not be created, the
				server MUST send the response code "[TRYCREATE]" as the prefix of
				the text of the tagged NO response.  This gives a hint to the
				client that it can attempt a CREATE command and retry the COPY if
				
				If the COPY command is unsuccessful for any reason, server
				implementations MUST restore the destination mailbox to its state
				before the COPY attempt.

				Example:    C: A003 COPY 2:4 MEETING
							S: A003 OK COPY completed
			   
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            string[] args = ParseParams(argsText);
            if (args.Length != 2)
            {
                WriteLine(string.Format("{0} BAD Invalid arguments", cmdTag));
                return;
            }

            IMAP_SequenceSet sequenceSet = new IMAP_SequenceSet();
            // Just try if it can be parsed as sequence-set
            try
            {
                if (uidCopy)
                {
                    if (m_pSelectedFolder.Messages.Count > 0)
                    {
                        sequenceSet.Parse(args[0],
                                          m_pSelectedFolder.Messages[m_pSelectedFolder.Messages.Count - 1].UID);
                    }
                }
                else
                {
                    sequenceSet.Parse(args[0], m_pSelectedFolder.Messages.Count);
                }
            }
            // This isn't vaild sequnce-set value
            catch
            {
                WriteLine(string.Format("{0}BAD Invalid <sequnce-set> value '{1}' Syntax: {{<command-tag> COPY <sequnce-set> \"<mailbox-name>\"}}!", cmdTag, args[0]));
                return;
            }

            string errorText = "";
            for (int i = 0; i < m_pSelectedFolder.Messages.Count; i++)
            {
                IMAP_Message msg = m_pSelectedFolder.Messages[i];

                // For UID COPY we must compare UIDs and for normal COPY message numbers. 
                bool sequenceSetContains = false;
                if (uidCopy)
                {
                    sequenceSetContains = sequenceSet.Contains(msg.UID);
                }
                else
                {
                    sequenceSetContains = sequenceSet.Contains(i + 1);
                }

                if (sequenceSetContains)
                {
                    errorText = ImapServer.OnCopyMessage(this, msg, Core.Decode_IMAP_UTF7_String(args[1]));
                    if (errorText != null)
                    {
                        break; // Errors return error text, don't try to copy other messages
                    }
                }
            }

            if (errorText == null)
            {
                WriteLine(string.Format("{0} OK COPY completed", cmdTag));
            }
            else
            {
                WriteLine(string.Format("{0} NO {1}", cmdTag, errorText));
            }
        }

        private void Uid(string cmdTag, string argsText)
        {
            /* Rfc 3501 6.4.8 UID Command
				
				Arguments:  command name
							command arguments

				Responses:  untagged responses: FETCH, SEARCH

				Result:     OK - UID command completed
							NO - UID command error
							BAD - command unknown or arguments invalid
							
				The UID command has two forms.  In the first form, it takes as its
				arguments a COPY, FETCH, or STORE command with arguments
				appropriate for the associated command.  However, the numbers in
				the message set argument are unique identifiers instead of message
				sequence numbers.

				In the second form, the UID command takes a SEARCH command with
				SEARCH command arguments.  The interpretation of the arguments is
				the same as with SEARCH; however, the numbers returned in a SEARCH
				response for a UID SEARCH command are unique identifiers instead
				of message sequence numbers.  For example, the command UID SEARCH
				1:100 UID 443:557 returns the unique identifiers corresponding to
				the intersection of the message sequence number set 1:100 and the
				UID set 443:557.

				Message set ranges are permitted; however, there is no guarantee
				that unique identifiers be contiguous.  A non-existent unique
				identifier within a message set range is ignored without any error
				message generated.

				The number after the "*" in an untagged FETCH response is always a
				message sequence number, not a unique identifier, even for a UID
				command response.  However, server implementations MUST implicitly
				include the UID message data item as part of any FETCH response
				caused by a UID command, regardless of whether a UID was specified
				as a message data item to the FETCH.
				
				Example:    C: A999 UID FETCH 4827313:4828442 FLAGS
							S: * 23 FETCH (FLAGS (\Seen) UID 4827313)
							S: * 24 FETCH (FLAGS (\Seen) UID 4827943)
							S: * 25 FETCH (FLAGS (\Seen) UID 4828442)
							S: A999 UID FETCH completed
			*/
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return;
            }
            if (SelectedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} NO Select mailbox first !", cmdTag));
                return;
            }

            string[] args = ParseParams(argsText);
            if (args.Length < 2)
            {
                // We must have at least command and message-set or cmd args
                WriteLine(string.Format("{0} BAD Invalid arguments", cmdTag));
                return;
            }

            // Get commands args text, we just remove COMMAND
            string cmdArgs = Core.GetArgsText(argsText, args[0]);

            // See if valid command specified with UID command
            switch (args[0].ToUpper())
            {
                case "COPY":
                    Copy(cmdTag, cmdArgs, true);
                    break;

                case "FETCH":
                    Fetch(cmdTag, cmdArgs, true);
                    break;

                case "STORE":
                    Store(cmdTag, cmdArgs, true);
                    break;

                case "SEARCH":
                    Search(cmdTag, cmdArgs, true);
                    break;

                default:
                    WriteLine(cmdTag + " BAD Invalid arguments");
                    return;
            }
        }

        /// <summary>
        /// Processes IDLE command.
        /// </summary>
        /// <param name="cmdTag">Command tag.</param>
        /// <param name="argsText">Command arguments text.</param>
        /// <returns>Returns true if IDLE comand accepted, otherwise false.</returns>
        private bool Idle(string cmdTag, string argsText)
        {
            if (!IsAuthenticated)
            {
                WriteLine(string.Format("{0} NO Authenticate first !", cmdTag));
                return false;
            }
            if (SelectedMailbox.Length == 0 && m_StatusedMailbox.Length == 0)
            {
                WriteLine(string.Format("{0} BAD Select mailbox first !", cmdTag));
                return false;
            }
            if (m_pIDLE!=null)
            {
                m_pIDLE.Dispose();
            }
            m_pIDLE = new Command_IDLE(this, cmdTag, SelectedMailbox.Length == 0 ? m_StatusedMailbox : SelectedMailbox);

            return true;
        }

        //--- End of Selected State

        //--- Any State ------

        private void Capability(string cmdTag)
        {
            /* RFC 3501 6.1.1
			
				Arguments:  none

				Responses:  REQUIRED untagged response: CAPABILITY

				Result:     OK - capability completed
							BAD - command unknown or arguments invalid
			   
				The CAPABILITY command requests a listing of capabilities that the
				server supports.  The server MUST send a single untagged
				CAPABILITY response with "IMAP4rev1" as one of the listed
				capabilities before the (tagged) OK response.

				A capability name which begins with "AUTH=" indicates that the
				server supports that particular authentication mechanism.
				
				Example:    C: abcd CAPABILITY
							S: * CAPABILITY IMAP4rev1 STARTTLS AUTH=GSSAPI
							LOGINDISABLED
							S: abcd OK CAPABILITY completed
							C: efgh STARTTLS
							S: efgh OK STARTLS completed
							<TLS negotiation, further commands are under [TLS] layer>
							C: ijkl CAPABILITY
							S: * CAPABILITY IMAP4rev1 AUTH=GSSAPI AUTH=PLAIN
							S: ijkl OK CAPABILITY completed
			*/

            string reply = "* CAPABILITY IMAP4rev1";
            if ((ImapServer.SupportedAuthentications & SaslAuthTypes.Digest_md5) != 0)
            {
                reply += " AUTH=DIGEST-MD5";
            }
            if ((ImapServer.SupportedAuthentications & SaslAuthTypes.Cram_md5) != 0)
            {
                reply += " AUTH=CRAM-MD5";
            }
            if (!IsSecureConnection && Certificate != null)
            {
                reply += " STARTTLS";
            }
            reply += " NAMESPACE ACL QUOTA IDLE X-FILTER\r\n";
            reply += cmdTag + " OK CAPABILITY completed\r\n";

            TcpStream.Write(reply);
        }

        private void Noop(string cmdTag)
        {
            /* RFC 3501 6.1.2 NOOP Command
			
				Arguments:  none

				Responses:  no specific responses for this command (but see below)

				Result:     OK - noop completed
							BAD - command unknown or arguments invalid
			   
				The NOOP command always succeeds.  It does nothing.
				Since any command can return a status update as untagged data, the
				NOOP command can be used as a periodic poll for new messages or
				message status updates during a period of inactivity.  The NOOP
				command can also be used to reset any inactivity autologout timer
				on the server.
				
				Example: C: a002 NOOP
						 S: a002 OK NOOP completed
			*/

            // Store start time
            long startTime = DateTime.Now.Ticks;

            // If there is selected mailbox, see if messages status has changed
            if (SelectedMailbox.Length > 0)
            {
                ProcessMailboxChanges();

                WriteLine(cmdTag + " OK NOOP Completed in " +
                                 ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2") +
                                 " seconds\r\n");
            }
            else
            {
                WriteLine(cmdTag + " OK NOOP Completed in " +
                                 ((DateTime.Now.Ticks - startTime) / (decimal)10000000).ToString("f2") +
                                 " seconds\r\n");
            }
        }

        private void LogOut(string cmdTag)
        {
            /* RFC 3501 6.1.3
			
				Arguments:  none

				Responses:  REQUIRED untagged response: BYE

				Result:     OK - logout completed
							BAD - command unknown or arguments invalid
			   
				The LOGOUT command informs the server that the client is done with
				the connection.  The server MUST send a BYE untagged response
				before the (tagged) OK response, and then close the network
				connection.
				
				Example: C: A023 LOGOUT
						S: * BYE IMAP4rev1 Server logging out
						S: A023 OK LOGOUT completed
						(Server and client then close the connection)
			*/

            string reply = "* BYE IMAP4rev1 Server logging out\r\n";
            reply += cmdTag + " OK LOGOUT completed\r\n";

            TcpStream.Write(reply);
            TcpStream.Flush();
        }

        //--- End of Any State

        /// <summary>
        /// Processes changes and sends status responses if there are changes in selected mailbox.
        /// </summary>
        private void ProcessMailboxChanges()
        {
            ProcessMailboxChanges(SelectedMailbox);
        }

        /// <summary>
        /// Processes changes and sends status responses if there are changes in selected mailbox.
        /// </summary>
        private void ProcessMailboxChanges(string mailBox)
        {
            //TODO: Many whelps! Handle it!
            // Get status
            IMAP_SelectedFolder folderInfo = new IMAP_SelectedFolder(mailBox);
            IMAP_eArgs_GetMessagesInfo eArgs = ImapServer.OnGetMessagesInfo(this, folderInfo);

            // Join new info with exisiting
            if (m_pSelectedFolder != null)
            {
                string statusResponse = m_pSelectedFolder.Update(folderInfo);
                if (!string.IsNullOrEmpty(statusResponse))
                {
                    WriteLine(statusResponse);

                    m_pSelectedFolder = folderInfo;
                }
            }
            if (m_pAdditionalFolder != null)
            {
                string statusResponse = m_pAdditionalFolder.Update(folderInfo);
                if (!string.IsNullOrEmpty(statusResponse))
                {
                    WriteLine(statusResponse);

                    m_pAdditionalFolder = folderInfo;
                }
            }
        }

        private string[] ParseParams(string argsText)
        {
            List<string> p = new List<string>();

            try
            {
                while (argsText.Length > 0)
                {
                    // Parameter is between ""
                    if (argsText.StartsWith("\""))
                    {
                        p.Add(argsText.Substring(1, argsText.IndexOf("\"", 1) - 1));
                        // Remove parsed param
                        argsText = argsText.Substring(argsText.IndexOf("\"", 1) + 1).Trim();
                    }
                    else
                    {
                        // Parameter is between ()
                        if (argsText.StartsWith("("))
                        {
                            p.Add(argsText.Substring(1, argsText.LastIndexOf(")") - 1));
                            // Remove parsed param
                            argsText = argsText.Substring(argsText.LastIndexOf(")") + 1).Trim();
                        }
                        else
                        {
                            // Read parameter till " ", probably there is more params
                            // Note: If there is ({ before SP, cosider that it's last parameter.
                            //       For example body[header.fields (from to)]
                            if (argsText.IndexOf(" ") > -1 &&
                                argsText.IndexOfAny(new[] { '(', '[' }, 0, argsText.IndexOf(" ")) == -1)
                            {
                                p.Add(argsText.Substring(0, argsText.IndexOf(" ")));
                                // Remove parsed param
                                argsText = argsText.Substring(argsText.IndexOf(" ") + 1).Trim();
                            }
                            // This is last param
                            else
                            {
                                p.Add(argsText);
                                argsText = "";
                            }
                        }
                    }
                }
            }
            catch { }

            return p.ToArray();
        }

        #endregion
    }
}