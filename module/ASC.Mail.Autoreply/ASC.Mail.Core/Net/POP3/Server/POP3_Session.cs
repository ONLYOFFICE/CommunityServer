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


using System.Security.Principal;
using ASC.Mail.Net.IO;
using ASC.Mail.Net.TCP;

namespace ASC.Mail.Net.POP3.Server
{
    #region usings

    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using AUTH;

    #endregion

    /// <summary>
    /// POP3 Session.
    /// </summary>
    public class POP3_Session : TCP_ServerSession
    {
        #region Members

        private readonly POP3_MessageCollection m_POP3_Messages;
        private readonly POP3_Server m_pServer;
        private int m_BadCmdCount; // Holds number of bad commands.
        private string m_MD5_prefix = ""; // Session MD5 prefix for APOP command
        private GenericIdentity m_pUser;

        #endregion

        #region Constructor


        public string UserName  
        {
            get
            {
                if (AuthenticatedUserIdentity!=null)
                {
                    return AuthenticatedUserIdentity.Name;
                }
                return string.Empty;
            }
        }

        #endregion

        #region Methods


        #endregion

        #region Overrides

        protected override void OnTimeout()
        {
            base.OnTimeout();
            WriteLine("-ERR Session timeout, closing transmission channel");
        }

        private POP3_Server Pop3Server { get { return Server as POP3_Server; } }

        private void AddWriteEntry(string cmd)
        {
            // Log
            if (Pop3Server.Logger != null)
            {
                Pop3Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, cmd.Length, cmd, LocalEndPoint, RemoteEndPoint);
            }
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

        #endregion

        #region Utility methods

        protected internal override void Start()
        {
            base.Start();
            try
            {
                // Check if ip is allowed to connect this computer
                if (m_pServer.OnValidate_IpAddress(LocalEndPoint, RemoteEndPoint))
                {
                    // Notify that server is ready
                    m_MD5_prefix = "<" + Guid.NewGuid().ToString().ToLower() + ">";
                    if (m_pServer.GreetingText == "")
                    {
                        WriteLine(string.Format("+OK {0} POP3 Server ready {1}", LocalHostName, m_MD5_prefix));
                    }
                    else
                    {
                        WriteLine(string.Format("+OK {0} {1}", m_pServer.GreetingText, m_MD5_prefix));
                    }

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
        /// Starts session.
        /// </summary>
        //private void StartSession()
        //{
        //    // Add session to session list
        //    m_pServer.AddSession(this);

        //    try
        //    {
        //        // Check if ip is allowed to connect this computer
        //        if (m_pServer.OnValidate_IpAddress(LocalEndPoint, RemoteEndPoint))
        //        {
        //            //--- Dedicated SSL connection, switch to SSL -----------------------------------//
        //            if (BindInfo.SslMode == SslMode.SSL)
        //            {
        //                try
        //                {
        //                    Socket.SwitchToSSL(BindInfo.Certificate);

        //                    if (Socket.Logger != null)
        //                    {
        //                        Socket.Logger.AddTextEntry("SSL negotiation completed successfully.");
        //                    }
        //                }
        //                catch (Exception x)
        //                {
        //                    if (Socket.Logger != null)
        //                    {
        //                        Socket.Logger.AddTextEntry("SSL handshake failed ! " + x.Message);

        //                        EndSession();
        //                        return;
        //                    }
        //                }
        //            }
        //            //-------------------------------------------------------------------------------//

        //            // Notify that server is ready
        //            m_MD5_prefix = "<" + Guid.NewGuid().ToString().ToLower() + ">";
        //            if (m_pServer.GreetingText == "")
        //            {
        //                Socket.WriteLine("+OK " + Net_Utils.GetLocalHostName(BindInfo.HostName) +
        //                                 " POP3 Server ready " + m_MD5_prefix);
        //            }
        //            else
        //            {
        //                Socket.WriteLine("+OK " + m_pServer.GreetingText + " " + m_MD5_prefix);
        //            }

        //            BeginRecieveCmd();
        //        }
        //        else
        //        {
        //            EndSession();
        //        }
        //    }
        //    catch (Exception x)
        //    {
        //        OnError(x);
        //    }
        //}

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
            try
            {
                // We must see InnerException too, SocketException may be as inner exception.
                SocketException socketException = null;
                if (x is SocketException)
                {
                    socketException = (SocketException) x;
                }
                else if (x.InnerException != null && x.InnerException is SocketException)
                {
                    socketException = (SocketException) x.InnerException;
                }

                if (socketException != null)
                {
                    // Client disconnected without shutting down
                    if (socketException.ErrorCode == 10054 || socketException.ErrorCode == 10053)
                    {
                        EndSession();
                        // Exception handled, return
                        return;
                    }
                }

                m_pServer.OnSysError("", x);
            }
            catch (Exception ex)
            {
                m_pServer.OnSysError("", ex);
            }
        }

        /// <summary>
        /// Starts recieveing command.
        /// </summary>
        private void BeginRecieveCmd()
        {
            ReadAsync(ReciveCmdComleted);
        }

        private void AddReadEntry(string cmd)
        {
            // Log
            if (Pop3Server.Logger != null)
            {
                Pop3Server.Logger.AddRead(ID, AuthenticatedUserIdentity, cmd.Length, cmd, LocalEndPoint, RemoteEndPoint);
            }
        }

        internal void BeginWriteLine(string line)
        {
            if (!line.EndsWith("\r\n"))
            {
                line += "\r\n";
            }
            byte[] buffer = Encoding.Default.GetBytes(line);
            TcpStream.BeginWrite(buffer, 0, buffer.Length, EndSend, null);
        }

        private void EndSend(IAsyncResult ar)
        {
            try
            {
                TcpStream.EndWrite(ar);
                BeginRecieveCmd();
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        internal void ReadAsync(EventHandler<EventArgs<SmartStream.ReadLineAsyncOP>> completed)
        {
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);

            args.Completed += completed;
            TcpStream.ReadLine(args, true);
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
                    sessionEnd = SwitchCommand(cmdLine);
                    if (sessionEnd)
                    {
                        // Session end, close session
                        EndSession();
                    }
                }
                catch (Exception ex)
                {
                    WriteLine("-ERR " + ex.Message);
                    if (!sessionEnd)
                    {
                        BeginRecieveCmd();
                    }
                }
            }
            else if (e.Value.Error != null)
            {
                OnError(e.Value.Error);
            }
        }

        /// <summary>
        /// Parses and executes POP3 commmand.
        /// </summary>
        /// <param name="POP3_commandTxt">POP3 command text.</param>
        /// <returns>Returns true,if session must be terminated.</returns>
        private bool SwitchCommand(string POP3_commandTxt)
        {
            //---- Parse command --------------------------------------------------//
            string[] cmdParts = POP3_commandTxt.TrimStart().Split(new[] {' '});
            string POP3_command = cmdParts[0].ToUpper().Trim();
            string argsText = Core.GetArgsText(POP3_commandTxt, POP3_command);
            //---------------------------------------------------------------------//

            bool getNextCmd = true;

            switch (POP3_command)
            {
                case "USER":
                    USER(argsText);
                    getNextCmd = false;
                    break;

                case "PASS":
                    PASS(argsText);
                    getNextCmd = false;
                    break;

                case "STAT":
                    STAT();
                    getNextCmd = false;
                    break;

                case "LIST":
                    LIST(argsText);
                    getNextCmd = false;
                    break;

                case "RETR":
                    RETR(argsText);
                    getNextCmd = false;
                    break;

                case "DELE":
                    DELE(argsText);
                    getNextCmd = false;
                    break;

                case "NOOP":
                    NOOP();
                    getNextCmd = false;
                    break;

                case "RSET":
                    RSET();
                    getNextCmd = false;
                    break;

                case "QUIT":
                    QUIT();
                    getNextCmd = false;
                    return true;

                    //----- Optional commands ----- //
                case "UIDL":
                    UIDL(argsText);
                    getNextCmd = false;
                    break;

                case "APOP":
                    APOP(argsText);
                    getNextCmd = false;
                    break;

                case "TOP":
                    TOP(argsText);
                    getNextCmd = false;
                    break;

                case "AUTH":
                    AUTH(argsText);
                    getNextCmd = false;
                    break;

                case "CAPA":
                    CAPA(argsText);
                    getNextCmd = false;
                    break;

                case "STLS":
                    STLS(argsText);
                    getNextCmd = false;
                    break;

                default:
                    WriteLine("-ERR Invalid command");

                    //---- Check that maximum bad commands count isn't exceeded ---------------//
                    if (m_BadCmdCount > m_pServer.MaxBadCommands - 1)
                    {
                        WriteLine("-ERR Too many bad commands, closing transmission channel");
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
            m_pUser = new GenericIdentity(UserName);

        }

        private void USER(string argsText)
        {
            /* RFC 1939 7. USER
			Arguments:
				a string identifying a mailbox (required), which is of
				significance ONLY to the server
				
			NOTE:
				If the POP3 server responds with a positive
				status indicator ("+OK"), then the client may issue
				either the PASS command to complete the authentication,
				or the QUIT command to terminate the POP3 session.
			 
			*/

            if (IsAuthenticated)
            {
                BeginWriteLine("-ERR You are already authenticated");
                return;
            }
            if (UserName.Length > 0)
            {
                BeginWriteLine("-ERR username is already specified, please specify password");
                return;
            }
            if ((m_pServer.SupportedAuthentications & SaslAuthTypes.Plain) == 0)
            {
                BeginWriteLine("-ERR USER/PASS command disabled");
                return;
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // There must be only one parameter - userName
            if (argsText.Length > 0 && param.Length == 1)
            {
                string userName = param[0];

                // Check if user isn't logged in already
                if (!m_pServer.IsUserLoggedIn(userName))
                {
                    SetUserName(userName);

                    // Send this line last, because it issues a new command and any assignments done
                    // after this method may not become wisible to next command.
                    BeginWriteLine("+OK User:'" + userName + "' ok");
                }
                else
                {
                    BeginWriteLine("-ERR User:'" + userName + "' already logged in");
                }
            }
            else
            {
                BeginWriteLine("-ERR Syntax error. Syntax:{USER username}");
            }
        }

        private void PASS(string argsText)
        {
            /* RFC 7. PASS
			Arguments:
				a server/mailbox-specific password (required)
				
			Restrictions:
				may only be given in the AUTHORIZATION state immediately
				after a successful USER command
				
			NOTE:
				When the client issues the PASS command, the POP3 server
				uses the argument pair from the USER and PASS commands to
				determine if the client should be given access to the
				appropriate maildrop.
				
			Possible Responses:
				+OK maildrop locked and ready
				-ERR invalid password
				-ERR unable to lock maildrop
						
			*/

            if (IsAuthenticated)
            {
                BeginWriteLine("-ERR You are already authenticated");
                return;
            }
            if (UserName.Length == 0)
            {
                BeginWriteLine("-ERR please specify username first");
                return;
            }
            if ((m_pServer.SupportedAuthentications & SaslAuthTypes.Plain) == 0)
            {
                BeginWriteLine("-ERR USER/PASS command disabled");
                return;
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // There may be only one parameter - password
            if (param.Length == 1)
            {
                string password = param[0];

                // Authenticate user
                AuthUser_EventArgs aArgs = m_pServer.OnAuthUser(this, UserName, password, "", AuthType.Plain);

                // There is custom error, return it
                if (aArgs.ErrorText != null)
                {
                    BeginWriteLine("-ERR " + aArgs.ErrorText);
                    return;
                }

                if (aArgs.Validated)
                {
                    SetUserName(UserName);

                    // Get user messages info.
                    m_pServer.OnGetMessagesInfo(this, m_POP3_Messages);

                    BeginWriteLine("+OK Password ok");
                }
                else
                {
                    BeginWriteLine("-ERR UserName or Password is incorrect");
                    SetUserName(""); // Reset userName !!!
                }
            }
            else
            {
                BeginWriteLine("-ERR Syntax error. Syntax:{PASS userName}");
            }
        }

        private void STAT()
        {
            /* RFC 1939 5. STAT
			NOTE:
				The positive response consists of "+OK" followed by a single
				space, the number of messages in the maildrop, a single
				space, and the size of the maildrop in octets.
				
				Note that messages marked as deleted are not counted in
				either total.
			 
			Example:
				C: STAT
				S: +OK 2 320
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
                return;
            }

            BeginWriteLine(
                string.Format("+OK {0} {1}", m_POP3_Messages.Count, m_POP3_Messages.GetTotalMessagesSize()));
        }

        private void LIST(string argsText)
        {
            /* RFC 1939 5. LIST
			Arguments:
				a message-number (optional), which, if present, may NOT
				refer to a message marked as deleted
			 
			NOTE:
				If an argument was given and the POP3 server issues a
				positive response with a line containing information for
				that message.

				If no argument was given and the POP3 server issues a
				positive response, then the response given is multi-line.
				
				Note that messages marked as deleted are not listed.
			
			Examples:
				C: LIST
				S: +OK 2 messages (320 octets)
				S: 1 120				
				S: 2 200
				S: .
				...
				C: LIST 2
				S: +OK 2 200
				...
				C: LIST 3
				S: -ERR no such message, only 2 messages in maildrop
			 
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
                return;
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // Argument isn't specified, multiline response.
            if (argsText.Length == 0)
            {
                StringBuilder reply = new StringBuilder();
                reply.Append("+OK " + m_POP3_Messages.Count + " messages\r\n");

                // Send message number and size for each message
                for (int i = 0; i < m_POP3_Messages.Count; i++)
                {
                    POP3_Message message = m_POP3_Messages[i];
                    if (!message.MarkedForDelete)
                    {
                        reply.Append((i + 1) + " " + message.Size + "\r\n");
                    }
                }

                // ".<CRLF>" - means end of list
                reply.Append(".\r\n");

                BeginWriteLine(reply.ToString());
            }
            else
            {
                // If parameters specified,there may be only one parameter - messageNr
                if (param.Length == 1)
                {
                    // Check if messageNr is valid
                    if (Core.IsNumber(param[0]))
                    {
                        int messageNr = Convert.ToInt32(param[0]);
                        if (m_POP3_Messages.MessageExists(messageNr))
                        {
                            POP3_Message msg = m_POP3_Messages[messageNr - 1];

                            BeginWriteLine(string.Format("+OK {0} {1}", messageNr, msg.Size));
                        }
                        else
                        {
                            BeginWriteLine("-ERR no such message, or marked for deletion");
                        }
                    }
                    else
                    {
                        BeginWriteLine("-ERR message-number is invalid");
                    }
                }
                else
                {
                    BeginWriteLine("-ERR Syntax error. Syntax:{LIST [messageNr]}");
                }
            }
        }

        private void RETR(string argsText)
        {
            /* RFC 1939 5. RETR
			Arguments:
				a message-number (required) which may NOT refer to a
				message marked as deleted
			 
			NOTE:
				If the POP3 server issues a positive response, then the
				response given is multi-line.  After the initial +OK, the
				POP3 server sends the message corresponding to the given
				message-number, being careful to byte-stuff the termination
				character (as with all multi-line responses).
				
			Example:
				C: RETR 1
				S: +OK 120 octets
				S: <the POP3 server sends the entire message here>
				S: .
			
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // There must be only one parameter - messageNr
            if (argsText.Length > 0 && param.Length == 1)
            {
                // Check if messageNr is valid
                if (Core.IsNumber(param[0]))
                {
                    int messageNr = Convert.ToInt32(param[0]);
                    if (m_POP3_Messages.MessageExists(messageNr))
                    {
                        POP3_Message msg = m_POP3_Messages[messageNr - 1];

                        // Raise Event, request message
                        POP3_eArgs_GetMessageStream eArgs = m_pServer.OnGetMessageStream(this, msg);
                        if (eArgs.MessageExists && eArgs.MessageStream != null)
                        {
                            WriteLine("+OK " + eArgs.MessageSize + " octets");

                            // Send message asynchronously to client
                            TcpStream.WritePeriodTerminated(eArgs.MessageStream);
                            BeginRecieveCmd();
                        }
                        else
                        {
                            BeginWriteLine("-ERR no such message");
                        }
                    }
                    else
                    {
                        BeginWriteLine("-ERR no such message");
                    }
                }
                else
                {
                    BeginWriteLine("-ERR message-number is invalid");
                }
            }
            else
            {
                BeginWriteLine("-ERR Syntax error. Syntax:{RETR messageNr}");
            }
        }

        private void DELE(string argsText)
        {
            /* RFC 1939 5. DELE
			Arguments:
				a message-number (required) which may NOT refer to a
				message marked as deleted
			 
			NOTE:
				The POP3 server marks the message as deleted.  Any future
				reference to the message-number associated with the message
				in a POP3 command generates an error.  The POP3 server does
				not actually delete the message until the POP3 session
				enters the UPDATE state.
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
                return;
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // There must be only one parameter - messageNr
            if (argsText.Length > 0 && param.Length == 1)
            {
                // Check if messageNr is valid
                if (Core.IsNumber(param[0]))
                {
                    int nr = Convert.ToInt32(param[0]);
                    if (m_POP3_Messages.MessageExists(nr))
                    {
                        POP3_Message msg = m_POP3_Messages[nr - 1];
                        msg.MarkedForDelete = true;

                        BeginWriteLine("+OK marked for delete");
                    }
                    else
                    {
                        BeginWriteLine("-ERR no such message");
                    }
                }
                else
                {
                    BeginWriteLine("-ERR message-number is invalid");
                }
            }
            else
            {
                BeginWriteLine("-ERR Syntax error. Syntax:{DELE messageNr}");
            }
        }

        private void NOOP()
        {
            /* RFC 1939 5. NOOP
			NOTE:
				The POP3 server does nothing, it merely replies with a
				positive response.
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
                return;
            }

            BeginWriteLine("+OK");
        }

        private void RSET()
        {
            /* RFC 1939 5. RSET
			Discussion:
				If any messages have been marked as deleted by the POP3
				server, they are unmarked.  The POP3 server then replies
				with a positive response.
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
                return;
            }

            Reset();

            // Raise SessionResetted event
            m_pServer.OnSessionResetted(this);

            BeginWriteLine("+OK");
        }

        private void QUIT()
        {
            /* RFC 1939 6. QUIT
			NOTE:
				The POP3 server removes all messages marked as deleted
				from the maildrop and replies as to the status of this
				operation.  If there is an error, such as a resource
				shortage, encountered while removing messages, the
				maildrop may result in having some or none of the messages
				marked as deleted be removed.  In no case may the server
				remove any messages not marked as deleted.

				Whether the removal was successful or not, the server
				then releases any exclusive-access lock on the maildrop
				and closes the TCP connection.
			*/
            Update();

            WriteLine("+OK POP3 server signing off");
        }

        //--- Optional commands

        private void TOP(string argsText)
        {
            /* RFC 1939 7. TOP
			Arguments:
				a message-number (required) which may NOT refer to to a
				message marked as deleted, and a non-negative number
				of lines (required)
		
			NOTE:
				If the POP3 server issues a positive response, then the
				response given is multi-line.  After the initial +OK, the
				POP3 server sends the headers of the message, the blank
				line separating the headers from the body, and then the
				number of lines of the indicated message's body, being
				careful to byte-stuff the termination character (as with
				all multi-line responses).
			
			Examples:
				C: TOP 1 10
				S: +OK
				S: <the POP3 server sends the headers of the
					message, a blank line, and the first 10 lines
					of the body of the message>
				S: .
                ...
				C: TOP 100 3
				S: -ERR no such message
			 
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // There must be at two parameters - messageNr and nrLines
            if (param.Length == 2)
            {
                // Check if messageNr and nrLines is valid
                if (Core.IsNumber(param[0]) && Core.IsNumber(param[1]))
                {
                    int messageNr = Convert.ToInt32(param[0]);
                    if (m_POP3_Messages.MessageExists(messageNr))
                    {
                        POP3_Message msg = m_POP3_Messages[messageNr - 1];

                        byte[] lines = m_pServer.OnGetTopLines(this, msg, Convert.ToInt32(param[1]));
                        if (lines != null)
                        {
                            WriteLine("+OK " + lines.Length + " octets");

                            // Send message asynchronously to client
                            TcpStream.WritePeriodTerminated(new MemoryStream(lines));
                            BeginRecieveCmd();
                        }
                        else
                        {
                            BeginWriteLine("-ERR no such message");
                        }
                    }
                    else
                    {
                        BeginWriteLine("-ERR no such message");
                    }
                }
                else
                {
                    BeginWriteLine("-ERR message-number or number of lines is invalid");
                }
            }
            else
            {
                BeginWriteLine("-ERR Syntax error. Syntax:{TOP messageNr nrLines}");
            }
        }

        private void UIDL(string argsText)
        {
            /* RFC 1939 UIDL [msg]
			Arguments:
			    a message-number (optional), which, if present, may NOT
				refer to a message marked as deleted
				
			NOTE:
				If an argument was given and the POP3 server issues a positive
				response with a line containing information for that message.

				If no argument was given and the POP3 server issues a positive
				response, then the response given is multi-line.  After the
				initial +OK, for each message in the maildrop, the POP3 server
				responds with a line containing information for that message.	
				
			Examples:
				C: UIDL
				S: +OK
				S: 1 whqtswO00WBw418f9t5JxYwZ
				S: 2 QhdPYR:00WBw1Ph7x7
				S: .
				...
				C: UIDL 2
				S: +OK 2 QhdPYR:00WBw1Ph7x7
				...
				C: UIDL 3
				S: -ERR no such message
			*/

            if (!IsAuthenticated)
            {
                BeginWriteLine("-ERR You must authenticate first");
                return;
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // Argument isn't specified, multiline response.
            if (argsText.Length == 0)
            {
                StringBuilder reply = new StringBuilder();
                reply.Append("+OK\r\n");

                // Send message number and size for each message
                for (int i = 0; i < m_POP3_Messages.Count; i++)
                {
                    POP3_Message message = m_POP3_Messages[i];
                    if (!message.MarkedForDelete)
                    {
                        reply.Append((i + 1) + " " + message.UID + "\r\n");
                    }
                }

                // ".<CRLF>" - means end of list
                reply.Append(".\r\n");

                BeginWriteLine(reply.ToString());
            }
            else
            {
                // If parameters specified,there may be only one parameter - messageID
                if (param.Length == 1)
                {
                    // Check if messageNr is valid
                    if (Core.IsNumber(param[0]))
                    {
                        int messageNr = Convert.ToInt32(param[0]);
                        if (m_POP3_Messages.MessageExists(messageNr))
                        {
                            POP3_Message msg = m_POP3_Messages[messageNr - 1];

                            BeginWriteLine(string.Format("+OK {0} {1}", messageNr, msg.UID));
                        }
                        else
                        {
                            BeginWriteLine("-ERR no such message");
                        }
                    }
                    else
                    {
                        BeginWriteLine("-ERR message-number is invalid");
                    }
                }
                else
                {
                    BeginWriteLine("-ERR Syntax error. Syntax:{UIDL [messageNr]}");
                }
            }
        }

        private void APOP(string argsText)
        {
            /* RFC 1939 7. APOP
			Arguments:
				a string identifying a mailbox and a MD5 digest string
				(both required)
				
			NOTE:
				A POP3 server which implements the APOP command will
				include a timestamp in its banner greeting.  The syntax of
				the timestamp corresponds to the `msg-id' in [RFC822], and
				MUST be different each time the POP3 server issues a banner
				greeting.
				
			Examples:
				S: +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
				C: APOP mrose c4c9334bac560ecc979e58001b3e22fb
				S: +OK maildrop has 1 message (369 octets)

				In this example, the shared  secret  is  the  string  `tan-
				staaf'.  Hence, the MD5 algorithm is applied to the string

				<1896.697170952@dbc.mtview.ca.us>tanstaaf
				 
				which produces a digest value of
		            c4c9334bac560ecc979e58001b3e22fb
			 
			*/

            if (IsAuthenticated)
            {
                BeginWriteLine("-ERR You are already authenticated");
                return;
            }

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);

            // There must be two params
            if (param.Length == 2)
            {
                string userName = param[0];
                string md5HexHash = param[1];

                // Check if user isn't logged in already
                if (m_pServer.IsUserLoggedIn(userName))
                {
                    BeginWriteLine(string.Format("-ERR User:'{0}' already logged in", userName));
                    return;
                }

                // Authenticate user
                AuthUser_EventArgs aArgs = m_pServer.OnAuthUser(this,
                                                                userName,
                                                                md5HexHash,
                                                                m_MD5_prefix,
                                                                AuthType.APOP);
                if (aArgs.Validated)
                {
                    SetUserName(userName);

                    // Get user messages info.
                    m_pServer.OnGetMessagesInfo(this, m_POP3_Messages);

                    BeginWriteLine("+OK authentication was successful");
                }
                else
                {
                    BeginWriteLine("-ERR authentication failed");
                }
            }
            else
            {
                BeginWriteLine("-ERR syntax error. Syntax:{APOP userName md5HexHash}");
            }
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

        private void AUTH(string argsText)
        {
            /* Rfc 1734
				
				AUTH mechanism

					Arguments:
						a string identifying an IMAP4 authentication mechanism,
						such as defined by [IMAP4-AUTH].  Any use of the string
						"imap" used in a server authentication identity in the
						definition of an authentication mechanism is replaced with
						the string "pop".
						
					Possible Responses:
						+OK maildrop locked and ready
						-ERR authentication exchange failed

					Restrictions:
						may only be given in the AUTHORIZATION state

					Discussion:
						The AUTH command indicates an authentication mechanism to
						the server.  If the server supports the requested
						authentication mechanism, it performs an authentication
						protocol exchange to authenticate and identify the user.
						Optionally, it also negotiates a protection mechanism for
						subsequent protocol interactions.  If the requested
						authentication mechanism is not supported, the server						
						should reject the AUTH command by sending a negative
						response.

						The authentication protocol exchange consists of a series
						of server challenges and client answers that are specific
						to the authentication mechanism.  A server challenge,
						otherwise known as a ready response, is a line consisting
						of a "+" character followed by a single space and a BASE64
						encoded string.  The client answer consists of a line
						containing a BASE64 encoded string.  If the client wishes
						to cancel an authentication exchange, it should issue a
						line with a single "*".  If the server receives such an
						answer, it must reject the AUTH command by sending a
						negative response.

						A protection mechanism provides integrity and privacy
						protection to the protocol session.  If a protection
						mechanism is negotiated, it is applied to all subsequent
						data sent over the connection.  The protection mechanism
						takes effect immediately following the CRLF that concludes
						the authentication exchange for the client, and the CRLF of
						the positive response for the server.  Once the protection
						mechanism is in effect, the stream of command and response
						octets is processed into buffers of ciphertext.  Each
						buffer is transferred over the connection as a stream of
						octets prepended with a four octet field in network byte
						order that represents the length of the following data.
						The maximum ciphertext buffer length is defined by the
						protection mechanism.

						The server is not required to support any particular
						authentication mechanism, nor are authentication mechanisms
						required to support any protection mechanisms.  If an AUTH
						command fails with a negative response, the session remains
						in the AUTHORIZATION state and client may try another
						authentication mechanism by issuing another AUTH command,
						or may attempt to authenticate by using the USER/PASS or
						APOP commands.  In other words, the client may request
						authentication types in decreasing order of preference,
						with the USER/PASS or APOP command as a last resort.

						Should the client successfully complete the authentication
						exchange, the POP3 server issues a positive response and
						the POP3 session enters the TRANSACTION state.
						
				Examples:
							S: +OK POP3 server ready
							C: AUTH KERBEROS_V4
							S: + AmFYig==
							C: BAcAQU5EUkVXLkNNVS5FRFUAOCAsho84kLN3/IJmrMG+25a4DT
								+nZImJjnTNHJUtxAA+o0KPKfHEcAFs9a3CL5Oebe/ydHJUwYFd
								WwuQ1MWiy6IesKvjL5rL9WjXUb9MwT9bpObYLGOKi1Qh
							S: + or//EoAADZI=
							C: DiAF5A4gA+oOIALuBkAAmw==
							S: +OK Kerberos V4 authentication successful
								...
							C: AUTH FOOBAR
							S: -ERR Unrecognized authentication type
			 
			*/
            if (IsAuthenticated)
            {
                BeginWriteLine("-ERR already authenticated");
                return;
            }

            //------ Parse parameters -------------------------------------//
            string userName = "";
            string password = "";
            AuthUser_EventArgs aArgs = null;

            string[] param = TextUtils.SplitQuotedString(argsText, ' ', true);
            switch (param[0].ToUpper())
            {
                case "PLAIN":
                    BeginWriteLine("-ERR Unrecognized authentication type.");
                    break;

                case "LOGIN":

                    #region LOGIN authentication

                    //---- AUTH = LOGIN ------------------------------
                    /* Login
					C: AUTH LOGIN-MD5
					S: + VXNlcm5hbWU6
					C: username_in_base64
					S: + UGFzc3dvcmQ6
					C: password_in_base64
					
					   VXNlcm5hbWU6 base64_decoded= USERNAME
					   UGFzc3dvcmQ6 base64_decoded= PASSWORD
					*/
                    // Note: all strings are base64 strings eg. VXNlcm5hbWU6 = UserName.

                    // Query UserName
                    WriteLine("+ VXNlcm5hbWU6");

                    string userNameLine = ReadLine();
                    // Encode username from base64
                    if (userNameLine.Length > 0)
                    {
                        userName = Encoding.Default.GetString(Convert.FromBase64String(userNameLine));
                    }

                    // Query Password
                    WriteLine("+ UGFzc3dvcmQ6");

                    string passwordLine = ReadLine();
                    // Encode password from base64
                    if (passwordLine.Length > 0)
                    {
                        password = Encoding.Default.GetString(Convert.FromBase64String(passwordLine));
                    }

                    aArgs = m_pServer.OnAuthUser(this, userName, password, "", AuthType.Plain);

                    // There is custom error, return it
                    if (aArgs.ErrorText != null)
                    {
                        BeginWriteLine("-ERR " + aArgs.ErrorText);
                        return;
                    }

                    if (aArgs.Validated)
                    {

                        SetUserName(userName);

                        // Get user messages info.
                        m_pServer.OnGetMessagesInfo(this, m_POP3_Messages);
                        BeginWriteLine("+OK Authentication successful.");
                    }
                    else
                    {
                        BeginWriteLine("-ERR Authentication failed");
                    }

                    #endregion

                    break;

                case "CRAM-MD5":

                    #region CRAM-MD5 authentication

                    /* Cram-M5
					C: AUTH CRAM-MD5
					S: + <md5_calculation_hash_in_base64>
					C: base64(decoded:username password_hash)
					*/

                    string md5Hash = "<" + Guid.NewGuid().ToString().ToLower() + ">";
                    WriteLine("+ " + Convert.ToBase64String(Encoding.ASCII.GetBytes(md5Hash)));

                    string reply = ReadLine();

                    reply = Encoding.Default.GetString(Convert.FromBase64String(reply));
                    string[] replyArgs = reply.Split(' ');
                    userName = replyArgs[0];

                    aArgs = m_pServer.OnAuthUser(this, userName, replyArgs[1], md5Hash, AuthType.CRAM_MD5);

                    // There is custom error, return it
                    if (aArgs.ErrorText != null)
                    {
                        BeginWriteLine("-ERR " + aArgs.ErrorText);
                        return;
                    }

                    if (aArgs.Validated)
                    {

                        SetUserName(userName);

                        // Get user messages info.
                        m_pServer.OnGetMessagesInfo(this, m_POP3_Messages);
                        BeginWriteLine("+OK Authentication successful.");
                    }
                    else
                    {
                        BeginWriteLine("-ERR Authentication failed");
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
					 * S: +OK Authentication successful.
					*/

                    string realm = LocalHostName;
                    string nonce = AuthHelper.GenerateNonce();

                    WriteLine("+ " +
                                     AuthHelper.Base64en(AuthHelper.Create_Digest_Md5_ServerResponse(realm,
                                                                                                     nonce)));

                    string clientResponse = AuthHelper.Base64de(ReadLine());
                    // Check that realm and nonce in client response are same as we specified
                    if (clientResponse.IndexOf("realm=\"" + realm + "\"") > - 1 &&
                        clientResponse.IndexOf("nonce=\"" + nonce + "\"") > - 1)
                    {
                        // Parse user name and password compare value
                        //		string userName  = "";
                        string passwData = "";
                        string cnonce = "";
                        foreach (string clntRespParam in clientResponse.Split(','))
                        {
                            if (clntRespParam.StartsWith("username="))
                            {
                                userName = clntRespParam.Split(new[] {'='}, 2)[1].Replace("\"", "");
                            }
                            else if (clntRespParam.StartsWith("response="))
                            {
                                passwData = clntRespParam.Split(new[] {'='}, 2)[1];
                            }
                            else if (clntRespParam.StartsWith("cnonce="))
                            {
                                cnonce = clntRespParam.Split(new[] {'='}, 2)[1].Replace("\"", "");
                            }
                        }

                        aArgs = m_pServer.OnAuthUser(this,
                                                     userName,
                                                     passwData,
                                                     clientResponse,
                                                     AuthType.DIGEST_MD5);

                        // There is custom error, return it
                        if (aArgs.ErrorText != null)
                        {
                            BeginWriteLine("-ERR " + aArgs.ErrorText);
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

                                SetUserName(userName);
                                m_pServer.OnGetMessagesInfo(this, m_POP3_Messages);
                                BeginWriteLine("+OK Authentication successful.");
                            }
                            else
                            {
                                BeginWriteLine("-ERR Authentication failed");
                            }
                        }
                        else
                        {
                            BeginWriteLine("-ERR Authentication failed");
                        }
                    }
                    else
                    {
                        BeginWriteLine("-ERR Authentication failed");
                    }

                    #endregion

                    break;

                default:
                    BeginWriteLine("-ERR Unrecognized authentication type.");
                    break;
            }
            //-----------------------------------------------------------------//
        }

        private void CAPA(string argsText)
        {
            /* Rfc 2449 5.  The CAPA Command
			
				The POP3 CAPA command returns a list of capabilities supported by the
				POP3 server.  It is available in both the AUTHORIZATION and
				TRANSACTION states.

				A capability description MUST document in which states the capability
				is announced, and in which states the commands are valid.

				Capabilities available in the AUTHORIZATION state MUST be announced
				in both states.

				If a capability is announced in both states, but the argument might
				differ after authentication, this possibility MUST be stated in the
				capability description.

				(These requirements allow a client to issue only one CAPA command if
				it does not use any TRANSACTION-only capabilities, or any
				capabilities whose values may differ after authentication.)

				If the authentication step negotiates an integrity protection layer,
				the client SHOULD reissue the CAPA command after authenticating, to
				check for active down-negotiation attacks.

				Each capability may enable additional protocol commands, additional
				parameters and responses for existing commands, or describe an aspect
				of server behavior.  These details are specified in the description
				of the capability.
				
				Section 3 describes the CAPA response using [ABNF].  When a
				capability response describes an optional command, the <capa-tag>
				SHOULD be identical to the command keyword.  CAPA response tags are
				case-insensitive.

				CAPA

				Arguments:
					none

				Restrictions:
					none

				Discussion:
					An -ERR response indicates the capability command is not
					implemented and the client will have to probe for
					capabilities as before.

					An +OK response is followed by a list of capabilities, one
					per line.  Each capability name MAY be followed by a single
					space and a space-separated list of parameters.  Each
					capability line is limited to 512 octets (including the
					CRLF).  The capability list is terminated by a line
					containing a termination octet (".") and a CRLF pair.

				Possible Responses:
					+OK -ERR

					Examples:
						C: CAPA
						S: +OK Capability list follows
						S: TOP
						S: USER
						S: SASL CRAM-MD5 KERBEROS_V4
						S: RESP-CODES
						S: LOGIN-DELAY 900
						S: PIPELINING
						S: EXPIRE 60
						S: UIDL
						S: IMPLEMENTATION Shlemazle-Plotz-v302
						S: .
			*/

            string capaResponse = "";
            capaResponse += "+OK Capability list follows\r\n";
            capaResponse += "PIPELINING\r\n";
            capaResponse += "UIDL\r\n";
            capaResponse += "TOP\r\n";
            if ((m_pServer.SupportedAuthentications & SaslAuthTypes.Plain) != 0)
            {
                capaResponse += "USER\r\n";
            }
            capaResponse += "SASL";
            if ((m_pServer.SupportedAuthentications & SaslAuthTypes.Cram_md5) != 0)
            {
                capaResponse += " CRAM-MD5";
            }
            if ((m_pServer.SupportedAuthentications & SaslAuthTypes.Digest_md5) != 0)
            {
                capaResponse += " DIGEST-MD5";
            }
            if ((m_pServer.SupportedAuthentications & SaslAuthTypes.Login) != 0)
            {
                capaResponse += " LOGIN";
            }
            capaResponse += "\r\n";
            if (Certificate != null)
            {
                capaResponse += "STLS\r\n";
            }
            capaResponse += ".\r\n";

            BeginWriteLine(capaResponse);
        }

        private void STLS(string argsText)
        {
            /* RFC 2595 4. POP3 STARTTLS extension.
                 Arguments: none

                 Restrictions:
                     Only permitted in AUTHORIZATION state.

                 Discussion:
                     A TLS negotiation begins immediately after the CRLF at the
                     end of the +OK response from the server.  A -ERR response
                     MAY result if a security layer is already active.  Once a
                     client issues a STLS command, it MUST NOT issue further
                     commands until a server response is seen and the TLS
                     negotiation is complete.

                     The STLS command is only permitted in AUTHORIZATION state
                     and the server remains in AUTHORIZATION state, even if
                     client credentials are supplied during the TLS negotiation.
                     The AUTH command [POP-AUTH] with the EXTERNAL mechanism
                     [SASL] MAY be used to authenticate once TLS client
                     credentials are successfully exchanged, but servers
                     supporting the STLS command are not required to support the
                     EXTERNAL mechanism.

                     Once TLS has been started, the client MUST discard cached
                     information about server capabilities and SHOULD re-issue
                     the CAPA command.  This is necessary to protect against
                     man-in-the-middle attacks which alter the capabilities list
                     prior to STLS.  The server MAY advertise different
                     capabilities after STLS.

                 Possible Responses:
                     +OK -ERR

                 Examples:
                     C: STLS
                     S: +OK Begin TLS negotiation
                     <TLS negotiation, further commands are under TLS layer>
                       ...
                     C: STLS
                     S: -ERR Command not permitted when TLS active
            */

            if (IsAuthenticated)
            {
                WriteLine("-ERR STLS command is only permitted in AUTHORIZATION state !");
                return;
            }

            if (Certificate == null)
            {
                WriteLine("-ERR TLS not available, SSL certificate isn't specified !");
                return;
            }

            WriteLine("+OK Ready to start TLS");

            try
            {
                SwitchToSecure();
            }
            catch (Exception x)
            {
                WriteLine("-ERR TLS handshake failed ! " + x.Message);
            }

            Reset();

            BeginRecieveCmd();
        }

        private void Reset()
        {
            /* RFC 1939 5. RSET
			Discussion:
				If any messages have been marked as deleted by the POP3
				server, they are unmarked.
			*/
            m_POP3_Messages.ResetDeletedFlag();
        }

        private void Update()
        {
            /* RFC 1939 6.
			NOTE:
				When the client issues the QUIT command from the TRANSACTION state,
				the POP3 session enters the UPDATE state.  (Note that if the client
				issues the QUIT command from the AUTHORIZATION state, the POP3
				session terminates but does NOT enter the UPDATE state.)

				If a session terminates for some reason other than a client-issued
				QUIT command, the POP3 session does NOT enter the UPDATE state and
				MUST not remove any messages from the maildrop.
			*/

            if (IsAuthenticated)
            {
                lock (m_POP3_Messages)
                {
                    // Delete all message which are marked for deletion ---//
                    foreach (POP3_Message msg in m_POP3_Messages)
                    {
                        if (msg.MarkedForDelete)
                        {
                            m_pServer.OnDeleteMessage(this, msg);
                        }
                    }
                    //-----------------------------------------------------//
                }
            }
        }

        /// <summary>
        /// Is called when asynchronous send completes.
        /// </summary>
        /// <param name="result">If true, then send was successfull.</param>
        /// <param name="count">Count sended.</param>
        /// <param name="exception">Exception happend on send. NOTE: available only is result=false.</param>
        /// <param name="tag">User data.</param>
        

        #endregion
    }
}