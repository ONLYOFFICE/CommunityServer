/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


namespace ASC.Mail.Net.SMTP.Server
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Security.Principal;
    using System.Text;
    using AUTH;
    using IO;
    using Mail;
    using TCP;

    #endregion

    /// <summary>
    /// This class implements SMTP session. Defined RFC 5321.
    /// </summary>
    public class SMTP_Session : TCP_ServerSession
    {
        #region Events

        /// <summary>
        /// Is raised when EHLO command received.
        /// </summary>
        public event EventHandler<SMTP_e_Ehlo> Ehlo = null;

        /// <summary>
        /// Is raised when SMTP server needs to get stream where to store incoming message.
        /// </summary>
        public event EventHandler<SMTP_e_Message> GetMessageStream = null;

        /// <summary>
        /// Is raised when MAIL FROM: command received.
        /// </summary>
        public event EventHandler<SMTP_e_MailFrom> MailFrom = null;

        /// <summary>
        /// Is raised when SMTP server has canceled message storing.
        /// </summary>
        /// <remarks>This can happen on 2 cases: on session timeout and if between BDAT chunks RSET issued.</remarks>
        public event EventHandler<SMTP_e_MessageStored> MessageStoringCanceled = null;

        /// <summary>
        /// Is raised when SMTP server has completed message storing.
        /// </summary>
        public event EventHandler<SMTP_e_MessageStored> MessageStoringCompleted = null;

        /// <summary>
        /// Is raised when RCPT TO: command received.
        /// </summary>
        public event EventHandler<SMTP_e_RcptTo> RcptTo = null;

        /// <summary>
        /// Is raised when session has started processing and needs to send 220 greeting or 554 error resposne to the connected client.
        /// </summary>
        public event EventHandler<SMTP_e_Started> Started = null;

        #endregion

        #region Members

        private int m_BadCommands;
        private int m_BDatReadedCount;
        private string m_EhloHost;
        private Dictionary<string, AUTH_SASL_ServerMechanism> m_pAuthentications;
        private SMTP_MailFrom m_pFrom;
        private Stream m_pMessageStream;
        private Dictionary<string, SMTP_RcptTo> m_pTo;
        private GenericIdentity m_pUser;
        private bool m_SessionRejected;
        private int m_Transactions;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SMTP_Session()
        {
            m_pAuthentications =
                new Dictionary<string, AUTH_SASL_ServerMechanism>(StringComparer.CurrentCultureIgnoreCase);
            m_pTo = new Dictionary<string, SMTP_RcptTo>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets authenticated user identity or null if user has not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
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

        /// <summary>
        /// Gets supported authentications collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Dictionary<string, AUTH_SASL_ServerMechanism> Authentications
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pAuthentications;
            }
        }

        /// <summary>
        /// Gets number of bad commands happened on SMTP session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int BadCommands
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_BadCommands;
            }
        }

        /// <summary>
        /// Gets client reported EHLO host name. Returns null if EHLO/HELO is not issued yet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string EhloHost
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_EhloHost;
            }
        }

        /// <summary>
        /// Gets MAIL FROM: value. Returns null if MAIL FROM: is not issued yet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public SMTP_MailFrom From
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pFrom;
            }
        }

        /// <summary>
        /// Gets session owner SMTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public new SMTP_Server Server
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return (SMTP_Server) base.Server;
            }
        }

        /// <summary>
        /// Gets RCPT TO: values. Returns null if RCPT TO: is not issued yet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public SMTP_RcptTo[] To
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                lock (m_pTo)
                {
                    SMTP_RcptTo[] retVal = new SMTP_RcptTo[m_pTo.Count];
                    m_pTo.Values.CopyTo(retVal, 0);

                    return retVal;
                }
            }
        }

        /// <summary>
        /// Gets number of mail transactions processed by this SMTP session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int Transactions
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_Transactions;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resource being used.
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();

            m_pAuthentications = null;
            m_EhloHost = null;
            m_pUser = null;
            m_pFrom = null;
            m_pTo = null;
            m_pMessageStream = null;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Starts session processing.
        /// </summary>
        protected internal override void Start()
        {
            base.Start();

            /* RFC 5321 3.1.
                The SMTP protocol allows a server to formally reject a mail session
                while still allowing the initial connection as follows: a 554
                response MAY be given in the initial connection opening message
                instead of the 220.  A server taking this approach MUST still wait
                for the client to send a QUIT (see Section 4.1.1.10) before closing
                the connection and SHOULD respond to any intervening commands with
                "503 bad sequence of commands".  Since an attempt to make an SMTP
                connection to such a system is probably in error, a server returning
                a 554 response on connection opening SHOULD provide enough
                information in the reply text to facilitate debugging of the sending
                system.
            */

            try
            {
                SMTP_Reply reply = null;
                if (string.IsNullOrEmpty(Server.GreetingText))
                {
                    reply = new SMTP_Reply(220,
                                           "<" + Net_Utils.GetLocalHostName(LocalHostName) +
                                           "> Simple Mail Transfer Service Ready.");
                }
                else
                {
                    reply = new SMTP_Reply(220, Server.GreetingText);
                }

                reply = OnStarted(reply);

                WriteLine(reply.ToString());

                // Setup rejected flag, so we respond "503 bad sequence of commands" any command except QUIT.
                if (reply.ReplyCode >= 300)
                {
                    m_SessionRejected = true;
                }

                BeginReadCmd();
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Is called when session has processing error.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        protected override void OnError(Exception x)
        {
            if (IsDisposed)
            {
                return;
            }
            if (x == null)
            {
                return;
            }

            /* Error handling:
                IO and Socket exceptions are permanent, so we must end session.
            */

            try
            {
                LogAddText("Exception: " + x.Message);

                // Permanent error.
                if (x is IOException || x is SocketException)
                {
                    Dispose();
                }
                    // xx error, may be temporary.
                else
                {
                    // Raise SMTP_Server.Error event.
                    base.OnError(x);

                    // Try to send "500 Internal server error."
                    try
                    {
                        WriteLine("500 Internal server error.");
                    }
                    catch
                    {
                        // Error is permanent.
                        Dispose();
                    }
                }
            }
            catch {}
        }

        /// <summary>
        /// This method is called when specified session times out.
        /// </summary>
        /// <remarks>
        /// This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected override void OnTimeout()
        {
            try
            {
                if (m_pMessageStream != null)
                {
                    OnMessageStoringCanceled();
                }

                WriteLine("421 Idle timeout, closing connection.");
            }
            catch
            {
                // Skip errors.
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Starts reading incoming command from the connected client.
        /// </summary>
        private void BeginReadCmd()
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                                         SizeExceededAction.
                                                                                             JunkAndThrowException);
                // This event is raised only if read period-terminated opeartion completes asynchronously.
                readLineOP.Completed += delegate
                                            {
                                                if (ProcessCmd(readLineOP))
                                                {
                                                    BeginReadCmd();
                                                }
                                            };
                // Process incoming commands while, command reading completes synchronously.
                while (TcpStream.ReadLine(readLineOP, true))
                {
                    if (!ProcessCmd(readLineOP))
                    {
                        break;
                    }
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Completes command reading operation.
        /// </summary>
        /// <param name="op">Operation.</param>
        /// <returns>Returns true if server should start reading next command.</returns>
        private bool ProcessCmd(SmartStream.ReadLineAsyncOP op)
        {
            bool readNextCommand = true;

            // Check errors.
            if (op.Error != null)
            {
                OnError(op.Error);
            }

            try
            {
                if (IsDisposed)
                {
                    return false;
                }

                // Log.
                if (Server.Logger != null)
                {
                    Server.Logger.AddRead(ID,
                                          AuthenticatedUserIdentity,
                                          op.BytesInBuffer,
                                          op.LineUtf8,
                                          LocalEndPoint,
                                          RemoteEndPoint);
                }

                string[] cmd_args =
                    Encoding.UTF8.GetString(op.Buffer, 0, op.LineBytesInBuffer).Split(new[] {' '}, 2);
                string cmd = cmd_args[0].ToUpperInvariant();
                string args = cmd_args.Length == 2 ? cmd_args[1] : "";

                if (cmd == "EHLO")
                {
                    EHLO(args);
                }
                else if (cmd == "HELO")
                {
                    HELO(args);
                }
                else if (cmd == "STARTTLS")
                {
                    STARTTLS(args);
                }
                else if (cmd == "AUTH")
                {
                    AUTH(args);
                }
                else if (cmd == "MAIL")
                {
                    MAIL(args);
                }
                else if (cmd == "RCPT")
                {
                    RCPT(args);
                }
                else if (cmd == "DATA")
                {
                    readNextCommand = DATA(args);
                }
                else if (cmd == "BDAT")
                {
                    readNextCommand = BDAT(args);
                }
                else if (cmd == "RSET")
                {
                    RSET(args);
                }
                else if (cmd == "NOOP")
                {
                    NOOP(args);
                }
                else if (cmd == "QUIT")
                {
                    QUIT(args);
                    readNextCommand = false;
                }
                else
                {
                    m_BadCommands++;

                    // Maximum allowed bad commands exceeded.
                    if (Server.MaxBadCommands != 0 && m_BadCommands > Server.MaxBadCommands)
                    {
                        WriteLine("421 Too many bad commands, closing transmission channel.");
                        Disconnect();
                        return false;
                    }

                    WriteLine("502 Error: command '" + cmd + "' not recognized.");
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }

            return readNextCommand;
        }

        private void EHLO(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return;
            }

            /* RFC 5321 4.1.1.1.
                ehlo           = "EHLO" SP ( Domain / address-literal ) CRLF

                ehlo-ok-rsp    = ( "250" SP Domain [ SP ehlo-greet ] CRLF )
                                 / ( "250-" Domain [ SP ehlo-greet ] CRLF
                                 *( "250-" ehlo-line CRLF )
                                 "250" SP ehlo-line CRLF )

                ehlo-greet     = 1*(%d0-9 / %d11-12 / %d14-127)
                                 ; string of any characters other than CR or LF

                ehlo-line      = ehlo-keyword *( SP ehlo-param )

                ehlo-keyword   = (ALPHA / DIGIT) *(ALPHA / DIGIT / "-")
                                ; additional syntax of ehlo-params depends on ehlo-keyword

                ehlo-param     = 1*(%d33-126)
                                ; any CHAR excluding <SP> and all control characters (US-ASCII 0-31 and 127 inclusive)
            */
            if (string.IsNullOrEmpty(cmdText) || cmdText.Split(' ').Length != 1)
            {
                WriteLine("501 Syntax error, syntax: \"EHLO\" SP hostname CRLF");
                return;
            }

            List<string> ehloLines = new List<string>();
            ehloLines.Add(Net_Utils.GetLocalHostName(LocalHostName));
            if (Server.Extentions.Contains(SMTP_ServiceExtensions.PIPELINING))
            {
                ehloLines.Add(SMTP_ServiceExtensions.PIPELINING);
            }
            if (Server.Extentions.Contains(SMTP_ServiceExtensions.SIZE))
            {
                ehloLines.Add(SMTP_ServiceExtensions.SIZE + " " + Server.MaxMessageSize);
            }
            if (Server.Extentions.Contains(SMTP_ServiceExtensions.STARTTLS) && !IsSecureConnection &&
                Certificate != null)
            {
                ehloLines.Add(SMTP_ServiceExtensions.STARTTLS);
            }
            if (Server.Extentions.Contains(SMTP_ServiceExtensions._8BITMIME))
            {
                ehloLines.Add(SMTP_ServiceExtensions._8BITMIME);
            }
            if (Server.Extentions.Contains(SMTP_ServiceExtensions.BINARYMIME))
            {
                ehloLines.Add(SMTP_ServiceExtensions.BINARYMIME);
            }
            if (Server.Extentions.Contains(SMTP_ServiceExtensions.CHUNKING))
            {
                ehloLines.Add(SMTP_ServiceExtensions.CHUNKING);
            }
            if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN))
            {
                ehloLines.Add(SMTP_ServiceExtensions.DSN);
            }

            StringBuilder sasl = new StringBuilder();
            foreach (AUTH_SASL_ServerMechanism authMechanism in Authentications.Values)
            {
                if (!authMechanism.RequireSSL || (authMechanism.RequireSSL && IsSecureConnection))
                {
                    sasl.Append(authMechanism.Name + " ");
                }
            }
            if (sasl.Length > 0)
            {
                ehloLines.Add(SMTP_ServiceExtensions.AUTH + " " + sasl.ToString().Trim());
            }

            SMTP_Reply reply = new SMTP_Reply(250, ehloLines.ToArray());

            reply = OnEhlo(cmdText, reply);

            // EHLO accepted.
            if (reply.ReplyCode < 300)
            {
                m_EhloHost = cmdText;

                /* RFC 5321 4.1.4.
                    An EHLO command MAY be issued by a client later in the session.  If
                    it is issued after the session begins and the EHLO command is
                    acceptable to the SMTP server, the SMTP server MUST clear all buffers
                    and reset the state exactly as if a RSET command had been issued.  In
                    other words, the sequence of RSET followed immediately by EHLO is
                    redundant, but not harmful other than in the performance cost of
                    executing unnecessary commands.
                */
                Reset();
            }

            WriteLine(reply.ToString());
        }

        private void HELO(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return;
            }

            /* RFC 5321 4.1.1.1.
                helo     = "HELO" SP Domain CRLF
            
                response = "250" SP Domain [ SP ehlo-greet ] CRLF
            */
            if (string.IsNullOrEmpty(cmdText) || cmdText.Split(' ').Length != 1)
            {
                WriteLine("501 Syntax error, syntax: \"HELO\" SP hostname CRLF");
                return;
            }

            SMTP_Reply reply = new SMTP_Reply(250, Net_Utils.GetLocalHostName(LocalHostName));

            reply = OnEhlo(cmdText, reply);

            // HELO accepted.
            if (reply.ReplyCode < 300)
            {
                m_EhloHost = cmdText;

                /* RFC 5321 4.1.4.
                    An EHLO command MAY be issued by a client later in the session.  If
                    it is issued after the session begins and the EHLO command is
                    acceptable to the SMTP server, the SMTP server MUST clear all buffers
                    and reset the state exactly as if a RSET command had been issued.  In
                    other words, the sequence of RSET followed immediately by EHLO is
                    redundant, but not harmful other than in the performance cost of
                    executing unnecessary commands.
                */
                Reset();
            }

            WriteLine(reply.ToString());
        }

        private void STARTTLS(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 Bad sequence of commands: Session rejected.");
                return;
            }

            /* RFC 3207 STARTTLS 4.
                The format for the STARTTLS command is:

                STARTTLS

                with no parameters.

                After the client gives the STARTTLS command, the server responds with
                one of the following reply codes:

                220 Ready to start TLS
                501 Syntax error (no parameters allowed)
                454 TLS not available due to temporary reason
             
               4.2 Result of the STARTTLS Command
                Upon completion of the TLS handshake, the SMTP protocol is reset to
                the initial state (the state in SMTP after a server issues a 220
                service ready greeting).  The server MUST discard any knowledge
                obtained from the client, such as the argument to the EHLO command,
                which was not obtained from the TLS negotiation itself.  The client
                MUST discard any knowledge obtained from the server, such as the list
                of SMTP service extensions, which was not obtained from the TLS
                negotiation itself.  The client SHOULD send an EHLO command as the
                first command after a successful TLS negotiation.
            
                Both the client and the server MUST know if there is a TLS session
                active.  A client MUST NOT attempt to start a TLS session if a TLS
                session is already active.  A server MUST NOT return the STARTTLS
                extension in response to an EHLO command received after a TLS
                handshake has completed.
              
             
               RFC 2246 7.2.2. Error alerts.
                Error handling in the TLS Handshake protocol is very simple. When an
                error is detected, the detecting party sends a message to the other
                party. Upon transmission or receipt of an fatal alert message, both
                parties immediately close the connection.  <...>
            */

            if (!string.IsNullOrEmpty(cmdText))
            {
                WriteLine("501 Syntax error: No parameters allowed.");
                return;
            }
            if (IsSecureConnection)
            {
                WriteLine("503 Bad sequence of commands: Connection is already secure.");
                return;
            }
            if (Certificate == null)
            {
                WriteLine("454 TLS not available: Server has no SSL certificate.");
                return;
            }

            WriteLine("220 Ready to start TLS.");

            try
            {
                SwitchToSecure();

                // Log
                LogAddText("TLS negotiation completed successfully.");

                m_EhloHost = null;
                Reset();
            }
            catch (Exception x)
            {
                // Log
                LogAddText("TLS negotiation failed: " + x.Message + ".");

                Disconnect();
            }
        }

        private void AUTH(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 Bad sequence of commands: Session rejected.");
                return;
            }

            /* RFC 4954 
			    AUTH mechanism [initial-response]

                Arguments:
                    mechanism: A string identifying a [SASL] authentication mechanism.

                    initial-response: An optional initial client response.  If
                    present, this response MUST be encoded as described in Section
                    4 of [BASE64] or contain a single character "=".

                Restrictions:
                    After an AUTH command has been successfully completed, no more
                    AUTH commands may be issued in the same session.  After a
                    successful AUTH command completes, a server MUST reject any
                    further AUTH commands with a 503 reply.

                    The AUTH command is not permitted during a mail transaction.
                    An AUTH command issued during a mail transaction MUST be
                    rejected with a 503 reply.
             
                A server challenge is sent as a 334 reply with the text part
                containing the [BASE64] encoded string supplied by the SASL
                mechanism.  This challenge MUST NOT contain any text other
                than the BASE64 encoded challenge.
             
                In SMTP, a server challenge that contains no data is defined 
                as a 334 reply with no text part. Note that there is still a space 
                following the reply code, so the complete response line is "334 ".
             
                If the client wishes to cancel the authentication exchange, 
                it issues a line with a single "*". If the server receives 
                such a response, it MUST reject the AUTH command by sending a 501 reply.
			*/

            if (IsAuthenticated)
            {
                WriteLine("503 Bad sequence of commands: you are already authenticated.");
                return;
            }
            if (m_pFrom != null)
            {
                WriteLine(
                    "503 Bad sequence of commands: The AUTH command is not permitted during a mail transaction.");
                return;
            }

            #region Parse parameters

            string[] arguments = cmdText.Split(' ');
            if (arguments.Length > 2)
            {
                WriteLine("501 Syntax error, syntax: AUTH SP mechanism [SP initial-response] CRLF");
                return;
            }
            string initialClientResponse = "";
            if (arguments.Length == 2)
            {
                if (arguments[1] == "=")
                {
                    // Skip.
                }
                else
                {
                    try
                    {
                        initialClientResponse = Encoding.UTF8.GetString(Convert.FromBase64String(arguments[1]));
                    }
                    catch
                    {
                        WriteLine(
                            "501 Syntax error: Parameter 'initial-response' value must be BASE64 or contain a single character '='.");
                        return;
                    }
                }
            }
            string mechanism = arguments[0];

            #endregion

            if (!Authentications.ContainsKey(mechanism))
            {
                WriteLine("501 Not supported authentication mechanism.");
                return;
            }

            string clientResponse = initialClientResponse;
            AUTH_SASL_ServerMechanism auth = Authentications[mechanism];
            while (true)
            {
                string serverResponse = auth.Continue(clientResponse);
                // Authentication completed.
                if (auth.IsCompleted)
                {
                    if (auth.IsAuthenticated)
                    {
                        m_pUser = new GenericIdentity(auth.UserName, "SASL-" + auth.Name);

                        WriteLine("235 2.7.0 Authentication succeeded.");
                    }
                    else
                    {
                        WriteLine("535 5.7.8 Authentication credentials invalid.");
                    }
                    break;
                }
                    // Authentication continues.
                else
                {
                    // Send server challange.
                    if (string.IsNullOrEmpty(serverResponse))
                    {
                        WriteLine("334 ");
                    }
                    else
                    {
                        WriteLine("334 " + Convert.ToBase64String(Encoding.UTF8.GetBytes(serverResponse)));
                    }

                    // Read client response. 
                    SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                                             SizeExceededAction
                                                                                                 .
                                                                                                 JunkAndThrowException);
                    TcpStream.ReadLine(readLineOP, false);
                    if (readLineOP.Error != null)
                    {
                        throw readLineOP.Error;
                    }
                    clientResponse = readLineOP.LineUtf8;
                    // Log
                    if (Server.Logger != null)
                    {
                        Server.Logger.AddRead(ID,
                                              AuthenticatedUserIdentity,
                                              readLineOP.BytesInBuffer,
                                              "base64 auth-data",
                                              LocalEndPoint,
                                              RemoteEndPoint);
                    }

                    // Client canceled authentication.
                    if (clientResponse == "*")
                    {
                        WriteLine("501 Authentication canceled.");
                        return;
                    }
                        // We have base64 client response, decode it.
                    else
                    {
                        try
                        {
                            clientResponse = Encoding.UTF8.GetString(Convert.FromBase64String(clientResponse));
                        }
                        catch
                        {
                            WriteLine("501 Invalid client response '" + clientResponse + "'.");
                            return;
                        }
                    }
                }
            }
        }

        private void MAIL(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return;
            }
            // RFC 5321 4.1.4.
            if (string.IsNullOrEmpty(m_EhloHost))
            {
                WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
                return;
            }
            // RFC 5321 4.1.4.
            if (m_pFrom != null)
            {
                WriteLine("503 Bad sequence of commands: nested MAIL command.");
                return;
            }
            // RFC 3030 BDAT.
            if (m_pMessageStream != null)
            {
                WriteLine("503 Bad sequence of commands: BDAT command is pending.");
                return;
            }
            if (Server.MaxTransactions != 0 && m_Transactions >= Server.MaxTransactions)
            {
                WriteLine("503 Bad sequence of commands: Maximum allowed mail transactions exceeded.");
                return;
            }

            /* RFC 5321 4.1.1.2.
                mail            = "MAIL FROM:" Reverse-path [SP Mail-parameters] CRLF
              
                Mail-parameters = esmtp-param *(SP esmtp-param)

                esmtp-param     = esmtp-keyword ["=" esmtp-value]

                esmtp-keyword   = (ALPHA / DIGIT) *(ALPHA / DIGIT / "-")

                esmtp-value     = 1*(%d33-60 / %d62-126)
                                  ; any CHAR excluding "=", SP, and control
                                  ; characters.  If this string is an email address,
                                  ; i.e., a Mailbox, then the "xtext" syntax [32] SHOULD be used.
              
                Reverse-path   = Path / "<>"
                Path           = "<" [ A-d-l ":" ] Mailbox ">"
              
               4.1.1.11.
                If the server SMTP does not recognize or cannot implement one or more
                of the parameters associated with a particular MAIL FROM or RCPT TO
                command, it will return code 555.
            */

            if (cmdText.ToUpper().StartsWith("FROM:"))
            {
                // Remove FROM: from command text.
                cmdText = cmdText.Substring(5).Trim();
            }
            else
            {
                WriteLine(
                    "501 Syntax error, syntax: \"MAIL FROM:\" \"<\" address \">\" / \"<>\" [SP Mail-parameters] CRLF");
                return;
            }

            string address = "";
            int size = -1;
            string body = null;
            string ret = null;
            string envID = null;

            // Mailbox not between <>.
            if (!cmdText.StartsWith("<") || cmdText.IndexOf('>') == -1)
            {
                WriteLine(
                    "501 Syntax error, syntax: \"MAIL FROM:\" \"<\" address \">\" / \"<>\" [SP Mail-parameters] CRLF");
                return;
            }
                // Parse mailbox.
            else
            {
                address = cmdText.Substring(1, cmdText.IndexOf('>') - 1).Trim();
                cmdText = cmdText.Substring(cmdText.IndexOf('>') + 1).Trim();
            }

            #region Parse parameters

            string[] parameters = string.IsNullOrEmpty(cmdText) ? new string[0] : cmdText.Split(' ');
            foreach (string parameter in parameters)
            {
                string[] name_value = parameter.Split(new[] {'='}, 2);

                // SIZE
                if (Server.Extentions.Contains(SMTP_ServiceExtensions.SIZE) &&
                    name_value[0].ToUpper() == "SIZE")
                {
                    // RFC 1870.
                    //  size-value ::= 1*20DIGIT
                    if (name_value.Length == 1)
                    {
                        WriteLine("501 Syntax error: SIZE parameter value must be specified.");
                        return;
                    }
                    if (!int.TryParse(name_value[1], out size))
                    {
                        WriteLine("501 Syntax error: SIZE parameter value must be integer.");
                        return;
                    }

                    // Message size exceeds maximum allowed message size.
                    if (size > Server.MaxMessageSize)
                    {
                        WriteLine("552 Message exceeds fixed maximum message size.");
                        return;
                    }
                }
                    // BODY
                else if (Server.Extentions.Contains(SMTP_ServiceExtensions._8BITMIME) &&
                         name_value[0].ToUpper() == "BODY")
                {
                    // RFC 1652.
                    //  body-value ::= "7BIT" / "8BITMIME" / "BINARYMIME"
                    //
                    // BINARYMIME - defined in RFC 3030.
                    if (name_value.Length == 1)
                    {
                        WriteLine("501 Syntax error: BODY parameter value must be specified.");
                        return;
                    }
                    if (name_value[1].ToUpper() != "7BIT" && name_value[1].ToUpper() != "8BITMIME" &&
                        name_value[1].ToUpper() != "BINARYMIME")
                    {
                        WriteLine(
                            "501 Syntax error: BODY parameter value must be \"7BIT\",\"8BITMIME\" or \"BINARYMIME\".");
                        return;
                    }
                    body = name_value[1].ToUpper();
                }
                    // RET
                else if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) &&
                         name_value[0].ToUpper() == "RET")
                {
                    // RFC 1891 5.3.
                    //  ret-value = "FULL" / "HDRS"
                    if (name_value.Length == 1)
                    {
                        WriteLine("501 Syntax error: RET parameter value must be specified.");
                        return;
                    }
                    if (name_value[1].ToUpper() != "FULL" && name_value[1].ToUpper() != "HDRS")
                    {
                        WriteLine(
                            "501 Syntax error: RET parameter value must be \"FULL\" or \"HDRS\".");
                        return;
                    }
                    ret = name_value[1].ToUpper();
                }
                    // ENVID
                else if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) &&
                         name_value[0].ToUpper() == "ENVID")
                {
                    if (name_value.Length == 1)
                    {
                        WriteLine("501 Syntax error: ENVID parameter value must be specified.");
                        return;
                    }

                    envID = name_value[1].ToUpper();
                }
                    // Unsupported parameter.
                else
                {
                    WriteLine("555 Unsupported parameter: " + parameter);
                    return;
                }
            }

            #endregion

            SMTP_MailFrom from = new SMTP_MailFrom(address, size, body, ret, envID);
            SMTP_Reply reply = new SMTP_Reply(250, "OK.");

            reply = OnMailFrom(from, reply);

            // MAIL accepted.
            if (reply.ReplyCode < 300)
            {
                m_pFrom = from;
                m_Transactions++;
            }

            WriteLine(reply.ToString());
        }

        private void RCPT(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return;
            }
            // RFC 5321 4.1.4.
            if (string.IsNullOrEmpty(m_EhloHost))
            {
                WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
                return;
            }
            // RFC 5321 4.1.4.
            if (m_pFrom == null)
            {
                WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
                return;
            }
            // RFC 3030 BDAT.
            if (m_pMessageStream != null)
            {
                WriteLine("503 Bad sequence of commands: BDAT command is pending.");
                return;
            }

            /* RFC 5321 4.1.1.3.
                rcpt = "RCPT TO:" ( "<Postmaster@" Domain ">" / "<Postmaster>" /  Forward-path ) [SP Rcpt-parameters] CRLF
              
                Rcpt-parameters = esmtp-param *(SP esmtp-param)

                esmtp-param     = esmtp-keyword ["=" esmtp-value]

                esmtp-keyword   = (ALPHA / DIGIT) *(ALPHA / DIGIT / "-")

                esmtp-value     = 1*(%d33-60 / %d62-126)
                                  ; any CHAR excluding "=", SP, and control
                                  ; characters.  If this string is an email address,
                                  ; i.e., a Mailbox, then the "xtext" syntax [32] SHOULD be used.

                    Note that, in a departure from the usual rules for local-parts, the "Postmaster" string shown above is
                    treated as case-insensitive.
             
                Forward-path   = Path
                Path           = "<" [ A-d-l ":" ] Mailbox ">"
              
               4.1.1.11.
                If the server SMTP does not recognize or cannot implement one or more
                of the parameters associated with a particular MAIL FROM or RCPT TO
                command, it will return code 555.
            */

            if (cmdText.ToUpper().StartsWith("TO:"))
            {
                // Remove TO: from command text.
                cmdText = cmdText.Substring(3).Trim();
            }
            else
            {
                WriteLine(
                    "501 Syntax error, syntax: \"RCPT TO:\" \"<\" address \">\" [SP Rcpt-parameters] CRLF");
                return;
            }

            string address = "";
            SMTP_Notify notify = SMTP_Notify.NotSpecified;
            string orcpt = null;

            // Mailbox not between <>.
            if (!cmdText.StartsWith("<") || cmdText.IndexOf('>') == -1)
            {
                WriteLine(
                    "501 Syntax error, syntax: \"RCPT TO:\" \"<\" address \">\" [SP Rcpt-parameters] CRLF");
                return;
            }
                // Parse mailbox.
            else
            {
                address = cmdText.Substring(1, cmdText.IndexOf('>') - 1).Trim();
                cmdText = cmdText.Substring(cmdText.IndexOf('>') + 1).Trim();
            }
            if (address == string.Empty)
            {
                WriteLine(
                    "501 Syntax error('address' value must be specified), syntax: \"RCPT TO:\" \"<\" address \">\" [SP Rcpt-parameters] CRLF");
                return;
            }

            #region Parse parameters

            string[] parameters = string.IsNullOrEmpty(cmdText) ? new string[0] : cmdText.Split(' ');
            foreach (string parameter in parameters)
            {
                string[] name_value = parameter.Split(new[] {'='}, 2);

                // NOTIFY
                if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) &&
                    name_value[0].ToUpper() == "NOTIFY")
                {
                    /* RFC 1891 5.1.
                        notify-esmtp-value  = "NEVER" / 1#notify-list-element
                        notify-list-element = "SUCCESS" / "FAILURE" / "DELAY"
                      
                        a. Multiple notify-list-elements, separated by commas, MAY appear in a
                           NOTIFY parameter; however, the NEVER keyword MUST appear by itself.
                    */
                    if (name_value.Length == 1)
                    {
                        WriteLine("501 Syntax error: NOTIFY parameter value must be specified.");
                        return;
                    }
                    string[] notifyItems = name_value[1].ToUpper().Split(',');
                    foreach (string notifyItem in notifyItems)
                    {
                        if (notifyItem.Trim().ToUpper() == "NEVER")
                        {
                            notify |= SMTP_Notify.Never;
                        }
                        else if (notifyItem.Trim().ToUpper() == "SUCCESS")
                        {
                            notify |= SMTP_Notify.Success;
                        }
                        else if (notifyItem.Trim().ToUpper() == "FAILURE")
                        {
                            notify |= SMTP_Notify.Failure;
                        }
                        else if (notifyItem.Trim().ToUpper() == "DELAY")
                        {
                            notify |= SMTP_Notify.Delay;
                        }
                            // Invalid or not supported notify item.
                        else
                        {
                            WriteLine("501 Syntax error: Not supported NOTIFY parameter value '" + notifyItem +
                                      "'.");
                            return;
                        }
                    }
                }
                    // ORCPT
                else if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) &&
                         name_value[0].ToUpper() == "ORCPT")
                {
                    if (name_value.Length == 1)
                    {
                        WriteLine("501 Syntax error: ORCPT parameter value must be specified.");
                        return;
                    }
                    orcpt = name_value[1].ToUpper();
                }
                    // Unsupported parameter.
                else
                {
                    WriteLine("555 Unsupported parameter: " + parameter);
                }
            }

            #endregion

            // Maximum allowed recipients exceeded.
            if (m_pTo.Count >= Server.MaxRecipients)
            {
                WriteLine("452 Too many recipients");
                return;
            }

            SMTP_RcptTo to = new SMTP_RcptTo(address, notify, orcpt);
            SMTP_Reply reply = new SMTP_Reply(250, "OK.");

            reply = OnRcptTo(to, reply);

            // RCPT accepted.
            if (reply.ReplyCode < 300)
            {
                if (!m_pTo.ContainsKey(address.ToLower()))
                {
                    m_pTo.Add(address.ToLower(), to);
                }
            }

            WriteLine(reply.ToString());
        }

        private bool DATA(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return true;
            }
            // RFC 5321 4.1.4.
            if (string.IsNullOrEmpty(m_EhloHost))
            {
                WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
                return true;
            }
            // RFC 5321 4.1.4.
            if (m_pFrom == null)
            {
                WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
                return true;
            }
            // RFC 5321 4.1.4.
            if (m_pTo.Count == 0)
            {
                WriteLine("503 Bad sequence of commands: send 'RCPT TO:' first.");
                return true;
            }
            // RFC 3030 BDAT.
            if (m_pMessageStream != null)
            {
                WriteLine(
                    "503 Bad sequence of commands: DATA and BDAT commands cannot be used in the same transaction.");
                return true;
            }

            /* RFC 5321 4.1.1.4.
                The receiver normally sends a 354 response to DATA, and then treats
                the lines (strings ending in <CRLF> sequences, as described in
                Section 2.3.7) following the command as mail data from the sender.
                This command causes the mail data to be appended to the mail data
                buffer.  The mail data may contain any of the 128 ASCII character
                codes, although experience has indicated that use of control
                characters other than SP, HT, CR, and LF may cause problems and
                SHOULD be avoided when possible.
             
                The custom of accepting lines ending only in <LF>, as a concession to
                non-conforming behavior on the part of some UNIX systems, has proven
                to cause more interoperability problems than it solves, and SMTP
                server systems MUST NOT do this, even in the name of improved
                robustness.  In particular, the sequence "<LF>.<LF>" (bare line
                feeds, without carriage returns) MUST NOT be treated as equivalent to
                <CRLF>.<CRLF> as the end of mail data indication.
             
                Receipt of the end of mail data indication requires the server to
                process the stored mail transaction information.  This processing
                consumes the information in the reverse-path buffer, the forward-path
                buffer, and the mail data buffer, and on the completion of this
                command these buffers are cleared.  If the processing is successful,
                the receiver MUST send an OK reply.  If the processing fails, the
                receiver MUST send a failure reply.  The SMTP model does not allow
                for partial failures at this point: either the message is accepted by
                the server for delivery and a positive response is returned or it is
                not accepted and a failure reply is returned.  In sending a positive
                "250 OK" completion reply to the end of data indication, the receiver
                takes full responsibility for the message (see Section 6.1).  Errors
                that are diagnosed subsequently MUST be reported in a mail message,
                as discussed in Section 4.4.

                When the SMTP server accepts a message either for relaying or for
                final delivery, it inserts a trace record (also referred to
                interchangeably as a "time stamp line" or "Received" line) at the top
                of the mail data.  This trace record indicates the identity of the
                host that sent the message, the identity of the host that received
                the message (and is inserting this time stamp), and the date and time
                the message was received.  Relayed messages will have multiple time
                stamp lines.  Details for formation of these lines, including their
                syntax, is specified in Section 4.4.
            */

            DateTime startTime = DateTime.Now;

            m_pMessageStream = OnGetMessageStream();
            if (m_pMessageStream == null)
            {
                m_pMessageStream = new MemoryStream();
            }
            // RFC 5321.4.4 trace info.
            byte[] recevived = CreateReceivedHeader();
            m_pMessageStream.Write(recevived, 0, recevived.Length);

            WriteLine("354 Start mail input; end with <CRLF>.<CRLF>");

            // Create asynchronous read period-terminated opeartion.
            SmartStream.ReadPeriodTerminatedAsyncOP readPeriodTermOP =
                new SmartStream.ReadPeriodTerminatedAsyncOP(m_pMessageStream,
                                                            Server.MaxMessageSize,
                                                            SizeExceededAction.JunkAndThrowException);
            // This event is raised only if read period-terminated opeartion completes asynchronously.
            readPeriodTermOP.Completed += delegate { DATA_End(startTime, readPeriodTermOP); };
            // Read period-terminated completed synchronously.
            if (TcpStream.ReadPeriodTerminated(readPeriodTermOP, true))
            {
                DATA_End(startTime, readPeriodTermOP);

                return true;
            }
            // Read period-terminated completed asynchronously, Completed event will be raised once operation completes.
            // else{

            return false;
        }

        /// <summary>
        /// Completes DATA command.
        /// </summary>
        /// <param name="startTime">Time DATA command started.</param>
        /// <param name="op">Read period-terminated opeartion.</param>
        private void DATA_End(DateTime startTime, SmartStream.ReadPeriodTerminatedAsyncOP op)
        {
            try
            {
                if (op.Error != null)
                {
                    if (op.Error is LineSizeExceededException)
                    {
                        WriteLine("500 Line too long.");
                    }
                    else if (op.Error is DataSizeExceededException)
                    {
                        WriteLine("552 Too much mail data.");
                    }
                    else
                    {
                        OnError(op.Error);
                    }

                    OnMessageStoringCanceled();
                }
                else
                {
                    SMTP_Reply reply = new SMTP_Reply(250,
                                                      "DATA completed in " +
                                                      (DateTime.Now - startTime).TotalSeconds.ToString("f2") +
                                                      " seconds.");

                    reply = OnMessageStoringCompleted(reply);

                    WriteLine(reply.ToString());
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }

            Reset();
            BeginReadCmd();
        }

        private bool BDAT(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return true;
            }
            // RFC 5321 4.1.4.
            if (string.IsNullOrEmpty(m_EhloHost))
            {
                WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
                return true;
            }
            // RFC 5321 4.1.4.
            if (m_pFrom == null)
            {
                WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
                return true;
            }
            // RFC 5321 4.1.4.
            if (m_pTo.Count == 0)
            {
                WriteLine("503 Bad sequence of commands: send 'RCPT TO:' first.");
                return true;
            }

            /* RFC 3030 2
				The BDAT verb takes two arguments.The first argument indicates the length, 
                in octets, of the binary data chunk. The second optional argument indicates 
                that the data chunk	is the last.
				
				The message data is sent immediately after the trailing <CR>
				<LF> of the BDAT command line.  Once the receiver-SMTP receives the
				specified number of octets, it will return a 250 reply code.

				The optional LAST parameter on the BDAT command indicates that this
				is the last chunk of message data to be sent.  The last BDAT command
				MAY have a byte-count of zero indicating there is no additional data
				to be sent.  Any BDAT command sent after the BDAT LAST is illegal and
				MUST be replied to with a 503 "Bad sequence of commands" reply code.
				The state resulting from this error is indeterminate.  A RSET command
				MUST be sent to clear the transaction before continuing.
				
				A 250 response MUST be sent to each successful BDAT data block within
				a mail transaction.

				bdat-cmd   ::= "BDAT" SP chunk-size [ SP end-marker ] CR LF
				chunk-size ::= 1*DIGIT
				end-marker ::= "LAST"
			*/

            DateTime startTime = DateTime.Now;

            int chunkSize = 0;
            bool last = false;
            string[] args = cmdText.Split(' ');
            if (cmdText == string.Empty || args.Length > 2)
            {
                WriteLine("501 Syntax error, syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
                return true;
            }
            if (!int.TryParse(args[0], out chunkSize))
            {
                WriteLine(
                    "501 Syntax error(chunk-size must be integer), syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
                return true;
            }
            if (args.Length == 2)
            {
                if (args[1].ToUpperInvariant() != "LAST")
                {
                    WriteLine("501 Syntax error, syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
                    return true;
                }
                last = true;
            }

            // First BDAT block in transaction.
            if (m_pMessageStream == null)
            {
                m_pMessageStream = OnGetMessageStream();
                if (m_pMessageStream == null)
                {
                    m_pMessageStream = new MemoryStream();
                }
                // RFC 5321.4.4 trace info.
                byte[] recevived = CreateReceivedHeader();
                m_pMessageStream.Write(recevived, 0, recevived.Length);
            }

            Stream storeStream = m_pMessageStream;
            // Maximum allowed message size exceeded.
            if ((m_BDatReadedCount + chunkSize) > Server.MaxMessageSize)
            {
                storeStream = new JunkingStream();
            }

            // Read data block.
            TcpStream.BeginReadFixedCount(storeStream,
                                          chunkSize,
                                          delegate(IAsyncResult ar)
                                              {
                                                  try
                                                  {
                                                      TcpStream.EndReadFixedCount(ar);

                                                      m_BDatReadedCount += chunkSize;

                                                      // Maximum allowed message size exceeded.
                                                      if (m_BDatReadedCount > Server.MaxMessageSize)
                                                      {
                                                          WriteLine("552 Too much mail data.");

                                                          OnMessageStoringCanceled();
                                                      }
                                                      else
                                                      {
                                                          SMTP_Reply reply = new SMTP_Reply(250,
                                                                                            chunkSize +
                                                                                            " bytes received in " +
                                                                                            (DateTime.Now -
                                                                                             startTime).
                                                                                                TotalSeconds.
                                                                                                ToString("f2") +
                                                                                            " seconds.");

                                                          if (last)
                                                          {
                                                              reply = OnMessageStoringCompleted(reply);
                                                          }

                                                          WriteLine(reply.ToString());
                                                      }

                                                      if (last)
                                                      {
                                                          // Accoring RFC 3030, client should send RSET and we must wait it and reject transaction commands.
                                                          // If we reset internally, then all works as specified. 
                                                          Reset();
                                                      }
                                                  }
                                                  catch (Exception x)
                                                  {
                                                      OnError(x);
                                                  }

                                                  BeginReadCmd();
                                              },
                                          null);

            return false;
        }

        private void RSET(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return;
            }

            /* RFC 5321 4.1.1.5.
                This command specifies that the current mail transaction will be
                aborted.  Any stored sender, recipients, and mail data MUST be
                discarded, and all buffers and state tables cleared.  The receiver
                MUST send a "250 OK" reply to a RSET command with no arguments.  A
                reset command may be issued by the client at any time.  It is
                effectively equivalent to a NOOP (i.e., it has no effect) if issued
                immediately after EHLO, before EHLO is issued in the session, after
                an end of data indicator has been sent and acknowledged, or
                immediately before a QUIT.  An SMTP server MUST NOT close the
                connection as the result of receiving a RSET; that action is reserved
                for QUIT (see Section 4.1.1.10).
            */

            if (m_pMessageStream != null)
            {
                OnMessageStoringCanceled();
            }

            Reset();

            WriteLine("250 OK.");
        }

        private void NOOP(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_SessionRejected)
            {
                WriteLine("503 bad sequence of commands: Session rejected.");
                return;
            }

            /* RFC 5321 4.1.1.9.
                This command does not affect any parameters or previously entered
                commands.  It specifies no action other than that the receiver send a
                "250 OK" reply.

                This command has no effect on the reverse-path buffer, the forward-
                path buffer, or the mail data buffer, and it may be issued at any
                time.  If a parameter string is specified, servers SHOULD ignore it.

                Syntax:
                    noop = "NOOP" [ SP String ] CRLF
            */

            WriteLine("250 OK.");
        }

        private void QUIT(string cmdText)
        {
            /* RFC 5321 4.1.1.10.
                This command specifies that the receiver MUST send a "221 OK" reply,
                and then close the transmission channel.
              
                The QUIT command may be issued at any time.  Any current uncompleted
                mail transaction will be aborted.
            
                quit = "QUIT" CRLF
            */

            try
            {
                WriteLine("221 <" + Net_Utils.GetLocalHostName(LocalHostName) +
                          "> Service closing transmission channel.");
            }
            catch {}
            Disconnect();
            Dispose();
        }

        /// <summary>
        /// Does reset as specified in RFC 5321.
        /// </summary>
        private void Reset()
        {
            if (IsDisposed)
            {
                return;
            }

            m_pFrom = null;
            m_pTo.Clear();
            m_pMessageStream = null;
            m_BDatReadedCount = 0;
        }

        /// <summary>
        /// Creates "Received:" header field. For more info see RFC 5321.4.4.
        /// </summary>
        /// <returns>Returns "Received:" header field.</returns>
        private byte[] CreateReceivedHeader()
        {
            /* 5321 4.4. Trace Information.
                When an SMTP server receives a message for delivery or further
                processing, it MUST insert trace ("time stamp" or "Received")
                information at the beginning of the message content, as discussed in
                Section 4.1.1.4.

               RFC 4954.7. Additional Requirements on Servers.
                As described in Section 4.4 of [SMTP], an SMTP server that receives a
                message for delivery or further processing MUST insert the
                "Received:" header field at the beginning of the message content.
                This document places additional requirements on the content of a
                generated "Received:" header field.  Upon successful authentication,
                a server SHOULD use the "ESMTPA" or the "ESMTPSA" [SMTP-TT] (when
                appropriate) keyword in the "with" clause of the Received header
                field.
               
               http://www.iana.org/assignments/mail-parameters
                ESMTP                SMTP with Service Extensions               [RFC5321]
                ESMTPA               ESMTP with SMTP AUTH                       [RFC3848]
                ESMTPS               ESMTP with STARTTLS                        [RFC3848]
                ESMTPSA              ESMTP with both STARTTLS and SMTP AUTH     [RFC3848]
            */

            Mail_h_Received received = new Mail_h_Received(EhloHost,
                                                           Net_Utils.GetLocalHostName(LocalHostName),
                                                           DateTime.Now);
            received.From_TcpInfo = new Mail_t_TcpInfo(RemoteEndPoint.Address, null);
            received.Via = "TCP";
            if (!IsAuthenticated && !IsSecureConnection)
            {
                received.With = "ESMTP";
            }
            else if (IsAuthenticated && !IsSecureConnection)
            {
                received.With = "ESMTPA";
            }
            else if (!IsAuthenticated && IsSecureConnection)
            {
                received.With = "ESMTPS";
            }
            else if (IsAuthenticated && IsSecureConnection)
            {
                received.With = "ESMTPSA";
            }

            return Encoding.UTF8.GetBytes(received.ToString());
        }

        /// <summary>
        /// Sends and logs specified line to connected host.
        /// </summary>
        /// <param name="line">Line to send.</param>
        private void WriteLine(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            int countWritten = TcpStream.WriteLine(line);

            // Log.
            if (Server.Logger != null)
            {
                Server.Logger.AddWrite(ID,
                                       AuthenticatedUserIdentity,
                                       countWritten,
                                       line,
                                       LocalEndPoint,
                                       RemoteEndPoint);
            }
        }

        /// <summary>
        /// Logs specified text.
        /// </summary>
        /// <param name="text">text to log.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
        private void LogAddText(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            // Log
            if (Server.Logger != null)
            {
                Server.Logger.AddText(ID, text);
            }
        }

        /// <summary>
        /// Raises <b>Started</b> event.
        /// </summary>
        /// <param name="reply">Default SMTP server reply.</param>
        /// <returns>Returns SMTP server reply what must be sent to the connected client.</returns>
        private SMTP_Reply OnStarted(SMTP_Reply reply)
        {
            if (Started != null)
            {
                SMTP_e_Started eArgs = new SMTP_e_Started(this, reply);
                Started(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        /// <summary>
        /// Raises <b>Ehlo</b> event.
        /// </summary>
        /// <param name="domain">Ehlo/Helo domain.</param>
        /// <param name="reply">Default SMTP server reply.</param>
        /// <returns>Returns SMTP server reply what must be sent to the connected client.</returns>
        private SMTP_Reply OnEhlo(string domain, SMTP_Reply reply)
        {
            if (Ehlo != null)
            {
                SMTP_e_Ehlo eArgs = new SMTP_e_Ehlo(this, domain, reply);
                Ehlo(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        /// <summary>
        /// Raises <b>MailFrom</b> event.
        /// </summary>
        /// <param name="from">MAIL FROM: value.</param>
        /// <param name="reply">Default SMTP server reply.</param>
        /// <returns>Returns SMTP server reply what must be sent to the connected client.</returns>
        private SMTP_Reply OnMailFrom(SMTP_MailFrom from, SMTP_Reply reply)
        {
            if (MailFrom != null)
            {
                SMTP_e_MailFrom eArgs = new SMTP_e_MailFrom(this, from, reply);
                MailFrom(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        /// <summary>
        /// Raises <b>RcptTo</b> event.
        /// </summary>
        /// <param name="to">RCPT TO: value.</param>
        /// <param name="reply">Default SMTP server reply.</param>
        /// <returns>Returns SMTP server reply what must be sent to the connected client.</returns>
        private SMTP_Reply OnRcptTo(SMTP_RcptTo to, SMTP_Reply reply)
        {
            if (RcptTo != null)
            {
                SMTP_e_RcptTo eArgs = new SMTP_e_RcptTo(this, to, reply);
                RcptTo(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        /// <summary>
        /// Raises <b>GetMessageStream</b> event.
        /// </summary>
        /// <returns>Returns message store stream.</returns>
        private Stream OnGetMessageStream()
        {
            if (GetMessageStream != null)
            {
                SMTP_e_Message eArgs = new SMTP_e_Message(this);
                GetMessageStream(this, eArgs);

                return eArgs.Stream;
            }

            return null;
        }

        /// <summary>
        /// Raises <b>MessageStoringCanceled</b> event.
        /// </summary>
        private void OnMessageStoringCanceled()
        {
            if (MessageStoringCanceled != null)
            {
                MessageStoringCanceled(this, new SMTP_e_MessageStored(this,m_pMessageStream, null));
            }
        }

        /// <summary>
        /// Raises <b>MessageStoringCompleted</b> event.
        /// </summary>
        /// <param name="reply">Default SMTP server reply.</param>
        /// <returns>Returns SMTP server reply what must be sent to the connected client.</returns>
        private SMTP_Reply OnMessageStoringCompleted(SMTP_Reply reply)
        {
            if (MessageStoringCompleted != null)
            {
                SMTP_e_MessageStored eArgs = new SMTP_e_MessageStored(this, m_pMessageStream, reply);
                MessageStoringCompleted(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        #endregion
    }
}