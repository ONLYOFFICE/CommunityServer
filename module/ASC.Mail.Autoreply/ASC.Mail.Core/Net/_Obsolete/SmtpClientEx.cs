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


namespace ASC.Mail.Net.SMTP.Client
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using Dns.Client;
    using Mime;

    #endregion

    /// <summary>
    /// Is called when asynchronous command had completed.
    /// </summary>
    public delegate void CommadCompleted(SocketCallBackResult result, Exception exception);

    /// <summary>
    /// SMTP client.
    /// </summary>
    [Obsolete("Use SMTP_Client instead !")]
    public class SmtpClientEx : IDisposable
    {
        #region Nested type: Auth_state_data

        /// <summary>
        /// Provides state date for BeginAuthenticate method.
        /// </summary>
        private class Auth_state_data
        {
            #region Members

            private readonly string m_Password = "";
            private readonly CommadCompleted m_pCallback;
            private readonly string m_UserName = "";

            #endregion

            #region Properties

            /// <summary>
            /// Gets user name.
            /// </summary>
            public string UserName
            {
                get { return m_UserName; }
            }

            /// <summary>
            /// Gets user password.
            /// </summary>
            public string Password
            {
                get { return m_Password; }
            }

            /// <summary>
            /// Gets callback what must be called when aynchrounous execution completes.
            /// </summary>
            public CommadCompleted Callback
            {
                get { return m_pCallback; }
            }

            /// <summary>
            /// Gets or sets user data.
            /// </summary>
            public object Tag { get; set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="userName">User name.</param>
            /// <param name="password">Password.</param>
            /// <param name="callback">Callback what must be called when aynchrounous execution completes.</param>
            public Auth_state_data(string userName, string password, CommadCompleted callback)
            {
                m_UserName = userName;
                m_Password = password;
                m_pCallback = callback;
            }

            #endregion
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when SMTP session has finished and session log is available.
        /// </summary>
        public event LogEventHandler SessionLog = null;

        #endregion

        #region Members

        private bool m_Authenticated;
        private bool m_Connected;
        private string[] m_pDnsServers;
        private SocketLogger m_pLogger;
        private SocketEx m_pSocket;
        private bool m_Supports_Bdat;
        private bool m_Supports_CramMd5;
        private bool m_Supports_Login;
        private bool m_Supports_Size;

        #endregion

        #region Properties

        /// <summary>
        /// Gets local endpoint. Returns null if smtp client isn't connected.
        /// </summary>
        public EndPoint LocalEndpoint
        {
            get
            {
                if (m_pSocket != null)
                {
                    return m_pSocket.LocalEndPoint;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets remote endpoint. Returns null if smtp client isn't connected.
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (m_pSocket != null)
                {
                    return m_pSocket.RemoteEndPoint;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets dns servers.
        /// </summary>
        public string[] DnsServers
        {
            get { return m_pDnsServers; }

            set { m_pDnsServers = value; }
        }

        /// <summary>
        /// Gets if smtp client is connected.
        /// </summary>
        public bool Connected
        {
            get { return m_Connected; }
        }

        /// <summary>
        /// Gets if pop3 client is authenticated.
        /// </summary>
        public bool Authenticated
        {
            get { return m_Authenticated; }
        }

        /// <summary>
        /// Gets when was last activity.
        /// </summary>
        public DateTime LastDataTime
        {
            get { return m_pSocket.LastActivity; }
        }

        /// <summary>
        /// Gets log entries that are currently in log buffer. Returns null if socket not connected or no logging enabled.
        /// </summary>
        public SocketLogger SessionActiveLog
        {
            get
            {
                if (m_pSocket == null)
                {
                    return null;
                }
                else
                {
                    return m_pSocket.Logger;
                }
            }
        }

        /// <summary>
        /// Gets how many bytes are readed through smtp client.
        /// </summary>
        public long ReadedCount
        {
            get
            {
                if (!m_Connected)
                {
                    throw new Exception("You must connect first");
                }

                return m_pSocket.ReadedCount;
            }
        }

        /// <summary>
        /// Gets how many bytes are written through smtp client.
        /// </summary>
        public long WrittenCount
        {
            get
            {
                if (!m_Connected)
                {
                    throw new Exception("You must connect first");
                }

                return m_pSocket.WrittenCount;
            }
        }

        /// <summary>
        /// Gets if the connection is an SSL connection.
        /// </summary>
        public bool IsSecureConnection
        {
            get
            {
                if (!m_Connected)
                {
                    throw new Exception("You must connect first");
                }

                return m_pSocket.SSL;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends specified message to specified smart host.
        /// </summary>
        /// <param name="smartHost">Smarthost name or IP.</param>
        /// <param name="port">SMTP port number. Normally this is 25.</param>
        /// <param name="hostName">Host name reported to SMTP server.</param>
        /// <param name="message">Mime message to send.</param>
        public static void QuickSendSmartHost(string smartHost, int port, string hostName, Mime message)
        {
            QuickSendSmartHost(smartHost, port, hostName, "", "", message);
        }

        /// <summary>
        /// Sends specified message to specified smart host.
        /// </summary>
        /// <param name="smartHost">Smarthost name or IP.</param>
        /// <param name="port">SMTP port number. Normally this is 25.</param>
        /// <param name="hostName">Host name reported to SMTP server.</param>
        /// <param name="userName">SMTP user name. Note: Pass empty string if no authentication wanted.</param>
        /// <param name="password">SMTP password.</param>
        /// <param name="message">Mime message to send.</param>
        public static void QuickSendSmartHost(string smartHost,
                                              int port,
                                              string hostName,
                                              string userName,
                                              string password,
                                              Mime message)
        {
            QuickSendSmartHost(smartHost, port, false, hostName, userName, password, message);
        }

        /// <summary>
        /// Sends specified message to specified smart host.
        /// </summary>
        /// <param name="smartHost">Smarthost name or IP.</param>
        /// <param name="port">SMTP port number. Default SMTP port is 25 and SSL port is 465.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        /// <param name="hostName">Host name reported to SMTP server.</param>
        /// <param name="userName">SMTP user name. Note: Pass empty string if no authentication wanted.</param>
        /// <param name="password">SMTP password.</param>
        /// <param name="message">Mime message to send.</param>
        public static void QuickSendSmartHost(string smartHost,
                                              int port,
                                              bool ssl,
                                              string hostName,
                                              string userName,
                                              string password,
                                              Mime message)
        {
            string from = "";
            if (message.MainEntity.From != null)
            {
                MailboxAddress[] addresses = message.MainEntity.From.Mailboxes;
                if (addresses.Length > 0)
                {
                    from = addresses[0].EmailAddress;
                }
            }

            ArrayList recipients = new ArrayList();
            if (message.MainEntity.To != null)
            {
                MailboxAddress[] addresses = message.MainEntity.To.Mailboxes;
                foreach (MailboxAddress address in addresses)
                {
                    recipients.Add(address.EmailAddress);
                }
            }
            if (message.MainEntity.Cc != null)
            {
                MailboxAddress[] addresses = message.MainEntity.Cc.Mailboxes;
                foreach (MailboxAddress address in addresses)
                {
                    recipients.Add(address.EmailAddress);
                }
            }
            if (message.MainEntity.Bcc != null)
            {
                MailboxAddress[] addresses = message.MainEntity.Bcc.Mailboxes;
                foreach (MailboxAddress address in addresses)
                {
                    recipients.Add(address.EmailAddress);
                }
            }
            string[] to = new string[recipients.Count];
            recipients.CopyTo(to);

            MemoryStream messageStream = new MemoryStream();
            message.ToStream(messageStream);
            messageStream.Position = 0;

            // messageStream
            QuickSendSmartHost(smartHost, port, ssl, hostName, userName, password, from, to, messageStream);
        }

        /// <summary>
        /// Sends specified message to specified smart host. NOTE: Message sending starts from message stream current posision.
        /// </summary>
        /// <param name="smartHost">Smarthost name or IP.</param>
        /// <param name="port">SMTP port number. Normally this is 25.</param>
        /// <param name="hostName">Host name reported to SMTP server.</param>
        /// <param name="from">From address reported to SMTP server.</param>
        /// <param name="to">Message recipients.</param>
        /// <param name="messageStream">Message stream. NOTE: Message sending starts from message stream current posision.</param>
        public static void QuickSendSmartHost(string smartHost,
                                              int port,
                                              string hostName,
                                              string from,
                                              string[] to,
                                              Stream messageStream)
        {
            QuickSendSmartHost(smartHost, port, false, hostName, "", "", from, to, messageStream);
        }

        /// <summary>
        /// Sends specified message to specified smart host. NOTE: Message sending starts from message stream current posision.
        /// </summary>
        /// <param name="smartHost">Smarthost name or IP.</param>
        /// <param name="port">SMTP port number. Normally this is 25.</param>
        /// <param name="hostName">Host name reported to SMTP server.</param>
        /// <param name="userName">SMTP user name. Note: Pass empty string if no authentication wanted.</param>
        /// <param name="password">SMTP password.</param>
        /// <param name="from">From address reported to SMTP server.</param>
        /// <param name="to">Message recipients.</param>
        /// <param name="messageStream">Message stream. NOTE: Message sending starts from message stream current posision.</param>
        public static void QuickSendSmartHost(string smartHost,
                                              int port,
                                              string hostName,
                                              string userName,
                                              string password,
                                              string from,
                                              string[] to,
                                              Stream messageStream)
        {
            QuickSendSmartHost(smartHost, port, false, hostName, userName, password, from, to, messageStream);
        }

        /// <summary>
        /// Sends specified message to specified smart host. NOTE: Message sending starts from message stream current posision.
        /// </summary>
        /// <param name="smartHost">Smarthost name or IP.</param>
        /// <param name="port">SMTP port number. Default SMTP port is 25 and SSL port is 465.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        /// <param name="hostName">Host name reported to SMTP server.</param>
        /// <param name="userName">SMTP user name. Note: Pass empty string if no authentication wanted.</param>
        /// <param name="password">SMTP password.</param>
        /// <param name="from">From address reported to SMTP server.</param>
        /// <param name="to">Message recipients.</param>
        /// <param name="messageStream">Message stream. NOTE: Message sending starts from message stream current posision.</param>
        public static void QuickSendSmartHost(string smartHost,
                                              int port,
                                              bool ssl,
                                              string hostName,
                                              string userName,
                                              string password,
                                              string from,
                                              string[] to,
                                              Stream messageStream)
        {
            using (SmtpClientEx smtp = new SmtpClientEx())
            {
                smtp.Connect(smartHost, port, ssl);
                smtp.Ehlo(hostName);
                if (userName.Length > 0)
                {
                    smtp.Authenticate(userName, password);
                }
                smtp.SetSender(MailboxAddress.Parse(from).EmailAddress,
                               messageStream.Length - messageStream.Position);
                foreach (string t in to)
                {
                    smtp.AddRecipient(MailboxAddress.Parse(t).EmailAddress);
                }

                smtp.SendMessage(messageStream);
            }
        }

        /// <summary>
        /// Cleasns up resources and disconnect smtp client if open.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Disconnect();
            }
            catch {}
        }

        /// <summary>
        /// Connects to sepcified host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        public void Connect(string host, int port)
        {
            Connect(null, host, port, false);
        }

        /// <summary>
        /// Connects to sepcified host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect. Default SMTP port is 25 and SSL port is 465.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        public void Connect(string host, int port, bool ssl)
        {
            Connect(null, host, port, ssl);
        }

        /// <summary>
        /// Connects to sepcified host.
        /// </summary>
        /// <param name="localEndpoint">Sets local endpoint. Pass null, to use default.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        public void Connect(IPEndPoint localEndpoint, string host, int port)
        {
            Connect(localEndpoint, host, port, false);
        }

        /// <summary>
        /// Connects to sepcified host.
        /// </summary>
        /// <param name="localEndpoint">Sets local endpoint. Pass null, to use default.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        /// <param name="ssl">Specifies if to connected via SSL. Default SMTP port is 25 and SSL port is 465.</param>
        public void Connect(IPEndPoint localEndpoint, string host, int port, bool ssl)
        {
            m_pSocket = new SocketEx();
            if (localEndpoint != null)
            {
                m_pSocket.Bind(localEndpoint);
            }

            // Create logger
            if (SessionLog != null)
            {
                m_pLogger = new SocketLogger(m_pSocket.RawSocket, SessionLog);
                m_pLogger.SessionID = Guid.NewGuid().ToString();
                m_pSocket.Logger = m_pLogger;
            }

            if (host.IndexOf("@") == -1)
            {
                m_pSocket.Connect(host, port, ssl);
            }
            else
            {
                //---- Parse e-domain -------------------------------//
                string domain = host;

                // eg. Ivx <ivx@lumisoft.ee>
                if (domain.IndexOf("<") > -1 && domain.IndexOf(">") > -1)
                {
                    domain = domain.Substring(domain.IndexOf("<") + 1,
                                              domain.IndexOf(">") - domain.IndexOf("<") - 1);
                }

                if (domain.IndexOf("@") > -1)
                {
                    domain = domain.Substring(domain.LastIndexOf("@") + 1);
                }

                if (domain.Trim().Length == 0)
                {
                    if (m_pLogger != null)
                    {
                        m_pLogger.AddTextEntry("Destination address '" + host + "' is invalid, aborting !");
                    }
                    throw new Exception("Destination address '" + host + "' is invalid, aborting !");
                }

                //--- Get MX record -------------------------------------------//
                Dns_Client dns = new Dns_Client();
                Dns_Client.DnsServers = m_pDnsServers;
                DnsServerResponse dnsResponse = dns.Query(domain, QTYPE.MX);

                bool connected = false;
                switch (dnsResponse.ResponseCode)
                {
                    case RCODE.NO_ERROR:
                        DNS_rr_MX[] mxRecords = dnsResponse.GetMXRecords();

                        // Try all available hosts by MX preference order, if can't connect specified host.
                        foreach (DNS_rr_MX mx in mxRecords)
                        {
                            try
                            {
                                if (m_pLogger != null)
                                {
                                    m_pLogger.AddTextEntry("Connecting with mx record to: " + mx.Host);
                                }
                                m_pSocket.Connect(mx.Host, port, ssl);
                                connected = true;
                                break;
                            }
                            catch (Exception x)
                            {
                                // Just skip and let for to try next host.									
                                if (m_pLogger != null)
                                {
                                    m_pLogger.AddTextEntry("Failed connect to: " + mx.Host + " error:" +
                                                           x.Message);
                                }
                            }
                        }

                        // None of MX didn't connect
                        if (mxRecords.Length > 0 && !connected)
                        {
                            throw new Exception("Destination email server is down");
                        }

                        /* Rfc 2821 5
						 If no MX records are found, but an A RR is found, the A RR is treated as
						 if it was associated with an implicit MX RR, with a preference of 0,
						 pointing to that host.
						*/
                        if (!connected)
                        {
                            // Try to connect with A record
                            IPAddress[] ipEntry = null;
                            try
                            {
                                if (m_pLogger != null)
                                {
                                    m_pLogger.AddTextEntry("No mx record, trying to get A record for: " +
                                                           domain);
                                }
                                ipEntry = Dns_Client.Resolve(domain);
                            }
                            catch
                            {
                                if (m_pLogger != null)
                                {
                                    m_pLogger.AddTextEntry("Invalid domain,no MX or A record: " + domain);
                                }
                                throw new Exception("Invalid domain,no MX or A record: " + domain);
                            }

                            try
                            {
                                if (m_pLogger != null)
                                {
                                    m_pLogger.AddTextEntry("Connecting with A record to:" + domain);
                                }
                                m_pSocket.Connect(domain, port, ssl);
                            }
                            catch
                            {
                                if (m_pLogger != null)
                                {
                                    m_pLogger.AddTextEntry("Failed connect to:" + domain);
                                }
                                throw new Exception("Destination email server is down");
                            }
                        }
                        break;

                    case RCODE.NAME_ERROR:
                        if (m_pLogger != null)
                        {
                            m_pLogger.AddTextEntry("Invalid domain,no MX or A record: " + domain);
                        }
                        throw new Exception("Invalid domain,no MX or A record: " + domain);

                    case RCODE.SERVER_FAILURE:
                        if (m_pLogger != null)
                        {
                            m_pLogger.AddTextEntry("Dns server unvailable.");
                        }
                        throw new Exception("Dns server unvailable.");
                }
            }

            /*
			 * Notes: Greeting may be single or multiline response.
			 *		
			 * Examples:
			 *		220<SP>SMTP server ready<CRLF> 
			 * 
			 *		220-SMTP server ready<CRLF>
			 *		220-Addtitional text<CRLF>
			 *		220<SP>final row<CRLF>
			 * 
			*/

            // Read server response
            string responseLine = m_pSocket.ReadLine(1000);
            while (!responseLine.StartsWith("220 "))
            {
                // If lisne won't start with 220, then its error response
                if (!responseLine.StartsWith("220"))
                {
                    throw new Exception(responseLine);
                }

                responseLine = m_pSocket.ReadLine(1000);
            }

            m_Connected = true;
        }

        /// <summary>
        /// Starts connection to specified host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        /// <param name="callback">Callback to be called if connect ends.</param>
        public void BeginConnect(string host, int port, CommadCompleted callback)
        {
            BeginConnect(null, host, port, false, callback);
        }

        /// <summary>
        /// Starts connection to specified host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        /// <param name="callback">Callback to be called if connect ends.</param>
        public void BeginConnect(string host, int port, bool ssl, CommadCompleted callback)
        {
            BeginConnect(null, host, port, ssl, callback);
        }

        /// <summary>
        /// Starts connection to specified host.
        /// </summary>
        /// <param name="localEndpoint">Sets local endpoint. Pass null, to use default.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        /// <param name="callback">Callback to be called if connect ends.</param>
        public void BeginConnect(IPEndPoint localEndpoint, string host, int port, CommadCompleted callback)
        {
            BeginConnect(localEndpoint, host, port, false, callback);
        }

        /// <summary>
        /// Starts connection to specified host.
        /// </summary>
        /// <param name="localEndpoint">Sets local endpoint. Pass null, to use default.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port where to connect.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        /// <param name="callback">Callback to be called if connect ends.</param>
        public void BeginConnect(IPEndPoint localEndpoint,
                                 string host,
                                 int port,
                                 bool ssl,
                                 CommadCompleted callback)
        {
            ThreadPool.QueueUserWorkItem(BeginConnect_workerThread,
                                         new object[] {localEndpoint, host, port, ssl, callback});
        }

        /*		/// <summary>
		/// Starts disconnecting SMTP client.
		/// </summary>
		public void BeginDisconnect()
		{
			if(!m_Connected){
				throw new Exception("You must connect first");
			}
		}*/

        /// <summary>
        /// Disconnects smtp client from server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (m_pSocket != null && m_pSocket.Connected)
                {
                    m_pSocket.WriteLine("QUIT");

                    m_pSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch {}

            m_pSocket = null;
            m_Connected = false;
            m_Supports_Size = false;
            m_Supports_Bdat = false;
            m_Supports_Login = false;
            m_Supports_CramMd5 = false;

            if (m_pLogger != null)
            {
                m_pLogger.Flush();
                m_pLogger = null;
            }
        }

        /// <summary>
        /// Switches SMTP connection to SSL.
        /// </summary>
        public void StartTLS()
        {
            /* RFC 2487 STARTTLS 5. STARTTLS Command.
                STARTTLS with no parameters.
              
                After the client gives the STARTTLS command, the server responds with
                one of the following reply codes:

                220 Ready to start TLS
                501 Syntax error (no parameters allowed)
                454 TLS not available due to temporary reason
            */

            if (!m_Connected)
            {
                throw new Exception("You must connect first !");
            }
            if (m_Authenticated)
            {
                throw new Exception("The STLS command is only valid in non-authenticated state !");
            }
            if (m_pSocket.SSL)
            {
                throw new Exception("Connection is already secure !");
            }

            m_pSocket.WriteLine("STARTTLS");

            string reply = m_pSocket.ReadLine();
            if (!reply.ToUpper().StartsWith("220"))
            {
                throw new Exception("Server returned:" + reply);
            }

            m_pSocket.SwitchToSSL_AsClient();
        }

        /// <summary>
        /// Start TLS(SSL) negotiation asynchronously.
        /// </summary>
        /// <param name="callback">The method to be called when the asynchronous StartTLS operation is completed.</param>
        public void BeginStartTLS(CommadCompleted callback)
        {
            ThreadPool.QueueUserWorkItem(BeginStartTLS_workerThread, callback);
        }

        /// <summary>
        /// Does EHLO command. If server don't support EHLO, tries HELO.
        /// </summary>
        /// <param name="hostName">Host name which is reported to SMTP server.</param>
        public void Ehlo(string hostName)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* Rfc 2821 4.1.1.1 EHLO
			 * Syntax: "EHLO" SP Domain CRLF
			*/

            if (hostName.Length == 0)
            {
                hostName = Dns.GetHostName();
            }

            // Send EHLO command to server
            m_pSocket.WriteLine("EHLO " + hostName);

            string responseLine = m_pSocket.ReadLine();
            // Response line must start with 250 or otherwise it's error response,
            // try HELO
            if (!responseLine.StartsWith("250"))
            {
                // Send HELO command to server
                m_pSocket.WriteLine("HELO " + hostName);

                responseLine = m_pSocket.ReadLine();
                // HELO failed, return error
                if (!responseLine.StartsWith("250"))
                {
                    throw new Exception(responseLine);
                }
            }

            /* RFC 2821 4.1.1.1 EHLO
				*	Examples:
				*		250-domain<SP>free_text<CRLF>
				*       250-EHLO_keyword<CRLF>
				*		250<SP>EHLO_keyword<CRLF>
				* 
				*		250<SP> specifies that last EHLO response line.
			*/

            while (!responseLine.StartsWith("250 "))
            {
                //---- Store supported ESMTP features --------------------//
                if (responseLine.ToLower().IndexOf("size") > -1)
                {
                    m_Supports_Size = true;
                }
                else if (responseLine.ToLower().IndexOf("chunking") > -1)
                {
                    m_Supports_Bdat = true;
                }
                else if (responseLine.ToLower().IndexOf("cram-md5") > -1)
                {
                    m_Supports_CramMd5 = true;
                }
                else if (responseLine.ToLower().IndexOf("login") > -1)
                {
                    m_Supports_Login = true;
                }
                //--------------------------------------------------------//

                // Read next EHLO response line
                responseLine = m_pSocket.ReadLine();
            }
        }

        /// <summary>
        /// Begins EHLO command.
        /// </summary>
        /// <param name="hostName">Host name which is reported to SMTP server.</param>
        /// <param name="callback">Callback to be called if command ends.</param>
        public void BeginEhlo(string hostName, CommadCompleted callback)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* Rfc 2821 4.1.1.1 EHLO
			 * Syntax: "EHLO" SP Domain CRLF
			*/

            if (hostName.Length == 0)
            {
                hostName = Dns.GetHostName();
            }

            // Start sending EHLO command to server
            m_pSocket.BeginWriteLine("EHLO " + hostName, new object[] {hostName, callback}, OnEhloSendFinished);
        }

        /// <summary>
        /// Does AUTH command.
        /// </summary>
        /// <param name="userName">Uesr name.</param>
        /// <param name="password">Password.</param>
        public void Authenticate(string userName, string password)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first !");
            }
            if (!(m_Supports_CramMd5 || m_Supports_Login))
            {
                throw new Exception("Authentication isn't supported.");
            }

            /* LOGIN
			 * Example:
			 *	C: AUTH<SP>LOGIN<CRLF>
			 *	S: 334<SP>base64(USERNAME)<CRLF>   // USERNAME is string constant
			 *	C: base64(username)<CRLF>
			 *  S: 334<SP>base64(PASSWORD)<CRLF>   // PASSWORD is string constant
			 *  C: base64(password)<CRLF>
			 *	S: 235 Ok<CRLF>
			*/

            /* Cram-M5
			   Example:
					C: AUTH<SP>CRAM-MD5<CRLF>
					S: 334<SP>base64(md5_calculation_hash)<CRLF>
					C: base64(username<SP>password_hash)<CRLF>
					S: 235 Ok<CRLF>
			*/

            if (m_Supports_CramMd5)
            {
                m_pSocket.WriteLine("AUTH CRAM-MD5");

                string responseLine = m_pSocket.ReadLine();
                // Response line must start with 334 or otherwise it's error response
                if (!responseLine.StartsWith("334"))
                {
                    throw new Exception(responseLine);
                }

                string md5HashKey =
                    Encoding.ASCII.GetString(Convert.FromBase64String(responseLine.Split(' ')[1]));

                HMACMD5 kMd5 = new HMACMD5(Encoding.ASCII.GetBytes(password));
                byte[] md5HashByte = kMd5.ComputeHash(Encoding.ASCII.GetBytes(md5HashKey));
                string hashedPwd = BitConverter.ToString(md5HashByte).ToLower().Replace("-", "");

                m_pSocket.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + " " + hashedPwd)));

                responseLine = m_pSocket.ReadLine();
                // Response line must start with 235 or otherwise it's error response
                if (!responseLine.StartsWith("235"))
                {
                    throw new Exception(responseLine);
                }

                m_Authenticated = true;
            }
            else if (m_Supports_Login)
            {
                m_pSocket.WriteLine("AUTH LOGIN");

                string responseLine = m_pSocket.ReadLine();
                // Response line must start with 334 or otherwise it's error response
                if (!responseLine.StartsWith("334"))
                {
                    throw new Exception(responseLine);
                }

                // Send user name to server
                m_pSocket.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(userName)));

                responseLine = m_pSocket.ReadLine();
                // Response line must start with 334 or otherwise it's error response
                if (!responseLine.StartsWith("334"))
                {
                    throw new Exception(responseLine);
                }

                // Send password to server
                m_pSocket.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(password)));

                responseLine = m_pSocket.ReadLine();
                // Response line must start with 235 or otherwise it's error response
                if (!responseLine.StartsWith("235"))
                {
                    throw new Exception(responseLine);
                }

                m_Authenticated = true;
            }

            if (m_Authenticated && m_pSocket.Logger != null)
            {
                m_pSocket.Logger.UserName = userName;
            }
        }

        /// <summary>
        /// Begins authenticate.
        /// </summary>
        /// <param name="userName">Uesr name.</param>
        /// <param name="password">Password.</param> 
        /// <param name="callback">Callback to be called if command ends.</param>
        public void BeginAuthenticate(string userName, string password, CommadCompleted callback)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first !");
            }
            if (!(m_Supports_CramMd5 || m_Supports_Login))
            {
                throw new Exception("Authentication isn't supported.");
            }

            /* LOGIN
			 * Example:
			 *	C: AUTH<SP>LOGIN<CRLF>
			 *	S: 334<SP>base64(USERNAME)<CRLF>   // USERNAME is string constant
			 *	C: base64(username)<CRLF>
			 *  S: 334<SP>base64(PASSWORD)<CRLF>   // PASSWORD is string constant
			 *  C: base64(password)<CRLF>
			 *	S: 235 Ok<CRLF>
			*/

            /* Cram-M5
			   Example:
					C: AUTH<SP>CRAM-MD5<CRLF>
					S: 334<SP>base64(md5_calculation_hash)<CRLF>
					C: base64(username<SP>password_hash)<CRLF>
					S: 235 Ok<CRLF>
			*/

            if (m_Supports_CramMd5)
            {
                m_pSocket.BeginWriteLine("AUTH CRAM-MD5",
                                         new Auth_state_data(userName, password, callback),
                                         OnAuthCramMd5SendFinished);
            }
            else if (m_Supports_Login)
            {
                m_pSocket.BeginWriteLine("AUTH LOGIN",
                                         new Auth_state_data(userName, password, callback),
                                         OnAuthLoginSendFinished);
            }
        }

        /// <summary>
        /// Does MAIL FROM: command.
        /// </summary>
        /// <param name="senderEmail">Sender email address what is reported to smtp server</param>
        /// <param name="messageSize">Message size in bytes or -1 if message size isn't known.</param>
        public void SetSender(string senderEmail, long messageSize)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* RFC 2821 4.1.1.2 MAIL
			 *  Examples:
			 *		MAIL FROM:<ivx@lumisoft.ee>
			 * 
			 * RFC 1870 adds optional SIZE keyword support.
			 * SIZE keyword may only be used if it's reported in EHLO command response.
			 *	Examples:
			 *		MAIL FROM:<ivx@lumisoft.ee> SIZE=1000
			*/

            if (m_Supports_Size && messageSize > -1)
            {
                m_pSocket.WriteLine("MAIL FROM:<" + senderEmail + "> SIZE=" + messageSize);
            }
            else
            {
                m_pSocket.WriteLine("MAIL FROM:<" + senderEmail + ">");
            }

            string responseLine = m_pSocket.ReadLine();
            // Response line must start with 250 or otherwise it's error response
            if (!responseLine.StartsWith("250"))
            {
                throw new Exception(responseLine);
            }
        }

        /// <summary>
        /// Begin setting sender.
        /// </summary>
        /// <param name="senderEmail">Sender email address what is reported to smtp server.</param>
        /// <param name="messageSize">Message size in bytes or -1 if message size isn't known.</param>
        /// <param name="callback">Callback to be called if command ends.</param>
        public void BeginSetSender(string senderEmail, long messageSize, CommadCompleted callback)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* RFC 2821 4.1.1.2 MAIL
			 *  Examples:
			 *		MAIL FROM:<ivx@lumisoft.ee>
			 * 
			 * RFC 1870 adds optional SIZE keyword support.
			 * SIZE keyword may only be used if it's reported in EHLO command response.
			 *	Examples:
			 *		MAIL FROM:<ivx@lumisoft.ee> SIZE=1000
			*/

            if (m_Supports_Size && messageSize > -1)
            {
                m_pSocket.BeginWriteLine("MAIL FROM:<" + senderEmail + "> SIZE=" + messageSize,
                                         callback,
                                         OnMailSendFinished);
            }
            else
            {
                m_pSocket.BeginWriteLine("MAIL FROM:<" + senderEmail + ">", callback, OnMailSendFinished);
            }
        }

        /// <summary>
        /// Does RCPT TO: command.
        /// </summary>
        /// <param name="recipientEmail">Recipient email address.</param>
        public void AddRecipient(string recipientEmail)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* RFC 2821 4.1.1.2 RCPT
			 *  Examples:
			 *		RCPT TO:<ivx@lumisoft.ee>
			*/

            m_pSocket.WriteLine("RCPT TO:<" + recipientEmail + ">");

            string responseLine = m_pSocket.ReadLine();
            // Response line must start with 250 or otherwise it's error response
            if (!responseLine.StartsWith("250"))
            {
                throw new Exception(responseLine);
            }
        }

        /// <summary>
        /// Begin adding recipient.
        /// </summary>
        /// <param name="recipientEmail">Recipient email address.</param>
        /// <param name="callback">Callback to be called if command ends.</param>
        public void BeginAddRecipient(string recipientEmail, CommadCompleted callback)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* RFC 2821 4.1.1.2 RCPT
			 *  Examples:
			 *		RCPT TO:<ivx@lumisoft.ee>
			*/

            m_pSocket.BeginWriteLine("RCPT TO:<" + recipientEmail + ">", callback, OnRcptSendFinished);
        }

        /// <summary>
        /// Sends message to server. NOTE: Message sending starts from message stream current posision.
        /// </summary>
        /// <param name="message">Message what will be sent to server. NOTE: Message sending starts from message stream current posision.</param>
        public void SendMessage(Stream message)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* RFC 2821 4.1.1.4 DATA
			 * Notes:
			 *		Message must be period handled for DATA command. This meas if message line starts with .,
			 *		additional .(period) must be added.
			 *		Message send is ended with <CRLF>.<CRLF>.
			 *	Examples:
			 *		C: DATA<CRLF>
			 *		S: 354 Start sending message, end with <crlf>.<crlf><CRLF>
			 *		C: send_message
			 *		C: <CRLF>.<CRLF>
			*/

            /* RFC 3030 BDAT
			 *	Syntax:BDAT<SP>message_size_in_bytes<SP>LAST<CRLF>
			 *	
			 *	Exapmle:
			 *		C: BDAT 1000 LAST<CRLF>
			 *		C: send_1000_byte_message
			 *		S: 250 OK<CRLF>
			 * 
			*/

            if (m_Supports_Bdat)
            {
                m_pSocket.WriteLine("BDAT " + (message.Length - message.Position) + " LAST");

                m_pSocket.Write(message);

                string responseLine = m_pSocket.ReadLine();
                // Response line must start with 250 or otherwise it's error response
                if (!responseLine.StartsWith("250"))
                {
                    throw new Exception(responseLine);
                }
            }
            else
            {
                m_pSocket.WriteLine("DATA");

                string responseLine = m_pSocket.ReadLine();
                // Response line must start with 334 or otherwise it's error response
                if (!responseLine.StartsWith("354"))
                {
                    throw new Exception(responseLine);
                }

                m_pSocket.WritePeriodTerminated(message);

                responseLine = m_pSocket.ReadLine();
                // Response line must start with 250 or otherwise it's error response
                if (!responseLine.StartsWith("250"))
                {
                    throw new Exception(responseLine);
                }
            }
        }

        /// <summary>
        /// Starts sending message.
        /// </summary>
        /// <param name="message">Message what will be sent to server. NOTE: Message sending starts from message stream current posision.</param>
        /// <param name="callback">Callback to be called if command ends.</param>
        public void BeginSendMessage(Stream message, CommadCompleted callback)
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            /* RFC 2821 4.1.1.4 DATA
			 * Notes:
			 *		Message must be period handled for DATA command. This meas if message line starts with .,
			 *		additional .(period) must be added.
			 *		Message send is ended with <CRLF>.<CRLF>.
			 *	Examples:
			 *		C: DATA<CRLF>
			 *		S: 354 Start sending message, end with <crlf>.<crlf><CRLF>
			 *		C: send_message
			 *		C: <CRLF>.<CRLF>
			*/

            /* RFC 3030 BDAT
			 *	Syntax:BDAT<SP>message_size_in_bytes<SP>LAST<CRLF>
			 *	
			 *	Exapmle:
			 *		C: BDAT 1000 LAST<CRLF>
			 *		C: send_1000_byte_message
			 *		S: 250 OK<CRLF>
			 * 
			*/

            if (m_Supports_Bdat)
            {
                m_pSocket.BeginWriteLine("BDAT " + (message.Length - message.Position) + " LAST",
                                         new object[] {message, callback},
                                         OnBdatSendFinished);
            }
            else
            {
                m_pSocket.BeginWriteLine("DATA", new object[] {message, callback}, OnDataSendFinished);
            }
        }

        /// <summary>
        /// Send RSET command to SMTP server, resets SMTP session.
        /// </summary>
        public void Reset()
        {
            if (!m_Connected)
            {
                throw new Exception("You must connect first");
            }

            m_pSocket.WriteLine("RSET");

            string responseLine = m_pSocket.ReadLine();
            if (!responseLine.StartsWith("250"))
            {
                throw new Exception(responseLine);
            }
        }

        /// <summary>
        /// Gets specified email domain possible connect points. Values are in priority descending order.
        /// </summary>
        /// <param name="domain">Email address or domain name.</param>
        /// <returns>Returns possible email server connection points.</returns>
        public IPAddress[] GetDestinations(string domain)
        {
            /* 
                1) Add MX records
                2) Add A records
            */

            // We have email address, just get domain from it.
            if (domain.IndexOf('@') > -1)
            {
                domain = domain.Substring(domain.IndexOf('@') + 1);
            }

            List<IPAddress> retVal = new List<IPAddress>();
            Dns_Client dns = new Dns_Client();
            Dns_Client.DnsServers = DnsServers;
            DnsServerResponse response = dns.Query(domain, QTYPE.MX);
            // Add MX            
            foreach (DNS_rr_MX mx in response.GetMXRecords())
            {
                try
                {
                    IPAddress[] ips = Dns.GetHostAddresses(mx.Host);
                    foreach (IPAddress ip in ips)
                    {
                        if (!retVal.Contains(ip))
                        {
                            retVal.Add(ip);
                        }
                    }
                }
                catch
                {
                    // Probably wrong MX record, no A reocrd for it, so we don't get IP. Just skip it.
                }
            }
            // Add A records only if no MX records.
            if (retVal.Count == 0)
            {
                response = dns.Query(domain, QTYPE.A);
                foreach (DNS_rr_A a in response.GetARecords())
                {
                    IPAddress ip = a.IP;
                    if (!retVal.Contains(ip))
                    {
                        retVal.Add(ip);
                    }
                }
            }

            return retVal.ToArray();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Is called from ThreadPool Thread. This method just call synchrounous Connect.
        /// </summary>
        /// <param name="tag"></param>
        private void BeginConnect_workerThread(object tag)
        {
            CommadCompleted callback = (CommadCompleted) ((object[]) tag)[4];

            try
            {
                IPEndPoint localEndpoint = (IPEndPoint) ((object[]) tag)[0];
                string host = (string) ((object[]) tag)[1];
                int port = (int) ((object[]) tag)[2];
                bool ssl = (bool) ((object[]) tag)[3];

                Connect(localEndpoint, host, port, ssl);

                // Connect completed susscessfully, call callback method.
                callback(SocketCallBackResult.Ok, null);
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called from ThreadPool Thread. This method just call synchrounous StartTLS.
        /// </summary>
        /// <param name="tag">User data.</param>
        private void BeginStartTLS_workerThread(object tag)
        {
            CommadCompleted callback = (CommadCompleted) tag;

            try
            {
                StartTLS();

                // Connect completed susscessfully, call callback method.
                callback(SocketCallBackResult.Ok, null);
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished EHLO command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnEhloSendFinished(SocketCallBackResult result,
                                        long count,
                                        Exception exception,
                                        object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[1]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    // Begin reading server EHLO command response
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new[] {((object[]) tag)[0], callback, ms},
                                            OnEhloReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading EHLO command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnEhloReadServerResponseFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[1]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[2])).ToArray());

                    /* RFC 2821 4.1.1.1 EHLO
					*	Examples:
					*		250-domain<SP>free_text<CRLF>
					*       250-EHLO_keyword<CRLF>
					*		250<SP>EHLO_keyword<CRLF>
					* 
					* 250<SP> specifies that last EHLO response line.
					*/

                    // Response line must start with 250 or otherwise it's error response
                    if (!responseLine.StartsWith("250"))
                    {
                        // Server isn't required to support EHLO, try HELO
                        string hostName = (string) (((object[]) tag)[0]);
                        m_pSocket.BeginWriteLine("HELO " + hostName, callback, OnHeloSendFinished);
                    }
                    else
                    {
                        //---- Store supported ESMTP features --------------------//
                        if (responseLine.ToLower().IndexOf("size") > -1)
                        {
                            m_Supports_Size = true;
                        }
                        else if (responseLine.ToLower().IndexOf("chunking") > -1)
                        {
                            m_Supports_Bdat = true;
                        }
                        else if (responseLine.ToLower().IndexOf("cram-md5") > -1)
                        {
                            m_Supports_CramMd5 = true;
                        }
                        else if (responseLine.ToLower().IndexOf("login") > -1)
                        {
                            m_Supports_Login = true;
                        }
                        //--------------------------------------------------------//

                        // This isn't last EHLO response line
                        if (!responseLine.StartsWith("250 "))
                        {
                            MemoryStream ms = new MemoryStream();
                            m_pSocket.BeginReadLine(ms,
                                                    1000,
                                                    new[] {(((object[]) tag)[0]), callback, ms},
                                                    OnEhloReadServerResponseFinished);
                        }
                        else
                        {
                            // EHLO completed susscessfully, call callback method.
                            callback(SocketCallBackResult.Ok, null);
                        }
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished HELO command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnHeloSendFinished(SocketCallBackResult result,
                                        long count,
                                        Exception exception,
                                        object tag)
        {
            CommadCompleted callback = (CommadCompleted) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    // Begin reading server HELO command response
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new object[] {callback, ms},
                                            OnHeloReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading EHLO command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnHeloReadServerResponseFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[0]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[1])).ToArray());

                    /* RFC 2821 4.1.1.1 HELO
					*	Examples:
					*		250<SP>domain<SP>free_text<CRLF>
					* 
					*/

                    // Response line must start with 250 or otherwise it's error response
                    if (!responseLine.StartsWith("250"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // EHLO completed susscessfully, call callback method.
                        callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished AUTH CRAM-MD5 command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnAuthCramMd5SendFinished(SocketCallBackResult result,
                                               long count,
                                               Exception exception,
                                               object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    stateData.Tag = ms;
                    m_pSocket.BeginReadLine(ms, 1000, stateData, OnAuthCramMd5ReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading AUTH CRAM-MD% server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param> 
        private void OnAuthCramMd5ReadServerResponseFinished(SocketCallBackResult result,
                                                             long count,
                                                             Exception exception,
                                                             object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine = Encoding.ASCII.GetString(((MemoryStream) stateData.Tag).ToArray());

                    // Response line must start with 334 or otherwise it's error response
                    if (!responseLine.StartsWith("334"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        string md5HashKey =
                            Encoding.ASCII.GetString(Convert.FromBase64String(responseLine.Split(' ')[1]));

                        HMACMD5 kMd5 = new HMACMD5(Encoding.ASCII.GetBytes(stateData.Password));
                        byte[] md5HashByte = kMd5.ComputeHash(Encoding.ASCII.GetBytes(md5HashKey));
                        string hashedPwd = BitConverter.ToString(md5HashByte).ToLower().Replace("-", "");

                        // Start sending user name to server
                        m_pSocket.BeginWriteLine(
                            Convert.ToBase64String(
                                Encoding.ASCII.GetBytes(stateData.UserName + " " + hashedPwd)),
                            stateData,
                            OnAuthCramMd5UserPwdSendFinished);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished sending username and password to smtp server.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnAuthCramMd5UserPwdSendFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    stateData.Tag = ms;
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            stateData,
                                            OnAuthCramMd5UserPwdReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading user name and password send server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param> 
        private void OnAuthCramMd5UserPwdReadServerResponseFinished(SocketCallBackResult result,
                                                                    long count,
                                                                    Exception exception,
                                                                    object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine = Encoding.ASCII.GetString(((MemoryStream) stateData.Tag).ToArray());

                    // Response line must start with 235 or otherwise it's error response
                    if (!responseLine.StartsWith("235"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        m_Authenticated = true;

                        if (m_Authenticated && m_pSocket.Logger != null)
                        {
                            m_pSocket.Logger.UserName = stateData.UserName;
                        }

                        // AUTH CRAM-MD5 completed susscessfully, call callback method.
                        stateData.Callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished AUTH LOGIN command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnAuthLoginSendFinished(SocketCallBackResult result,
                                             long count,
                                             Exception exception,
                                             object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    stateData.Tag = ms;
                    m_pSocket.BeginReadLine(ms, 1000, stateData, OnAuthLoginReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading MAIL FROM: command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param> 
        private void OnAuthLoginReadServerResponseFinished(SocketCallBackResult result,
                                                           long count,
                                                           Exception exception,
                                                           object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine = Encoding.ASCII.GetString(((MemoryStream) (stateData.Tag)).ToArray());

                    // Response line must start with 334 or otherwise it's error response
                    if (!responseLine.StartsWith("334"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // Start sending user name to server
                        m_pSocket.BeginWriteLine(
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(stateData.UserName)),
                            stateData,
                            OnAuthLoginUserSendFinished);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished sending user name to SMTP server.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnAuthLoginUserSendFinished(SocketCallBackResult result,
                                                 long count,
                                                 Exception exception,
                                                 object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    stateData.Tag = ms;
                    m_pSocket.BeginReadLine(ms, 1000, stateData, OnAuthLoginUserReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading AUTH LOGIN user send server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param> 
        private void OnAuthLoginUserReadServerResponseFinished(SocketCallBackResult result,
                                                               long count,
                                                               Exception exception,
                                                               object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine = Encoding.ASCII.GetString(((MemoryStream) stateData.Tag).ToArray());

                    // Response line must start with 334 or otherwise it's error response
                    if (!responseLine.StartsWith("334"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // Start sending password to server
                        m_pSocket.BeginWriteLine(
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(stateData.Password)),
                            stateData,
                            OnAuthLoginPasswordSendFinished);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished sending password to SMTP server.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnAuthLoginPasswordSendFinished(SocketCallBackResult result,
                                                     long count,
                                                     Exception exception,
                                                     object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    stateData.Tag = ms;
                    m_pSocket.BeginReadLine(ms, 1000, stateData, OnAuthLoginPwdReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading AUTH LOGIN password send server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param> 
        private void OnAuthLoginPwdReadServerResponseFinished(SocketCallBackResult result,
                                                              long count,
                                                              Exception exception,
                                                              object tag)
        {
            Auth_state_data stateData = (Auth_state_data) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine = Encoding.ASCII.GetString(((MemoryStream) stateData.Tag).ToArray());

                    // Response line must start with 235 or otherwise it's error response
                    if (!responseLine.StartsWith("235"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        m_Authenticated = true;

                        if (m_Authenticated && m_pSocket.Logger != null)
                        {
                            m_pSocket.Logger.UserName = stateData.UserName;
                        }

                        // AUTH LOGIN completed susscessfully, call callback method.
                        stateData.Callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                stateData.Callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished MAIL FROM: command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnMailSendFinished(SocketCallBackResult result,
                                        long count,
                                        Exception exception,
                                        object tag)
        {
            CommadCompleted callback = (CommadCompleted) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new object[] {callback, ms},
                                            OnMailReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading MAIL FROM: command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param> 
        private void OnMailReadServerResponseFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[0]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[1])).ToArray());

                    // Response line must start with 250 or otherwise it's error response
                    if (!responseLine.StartsWith("250"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // MAIL FROM: completed susscessfully, call callback method.
                        callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished RCPT TO: command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnRcptSendFinished(SocketCallBackResult result,
                                        long count,
                                        Exception exception,
                                        object tag)
        {
            CommadCompleted callback = (CommadCompleted) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new object[] {callback, ms},
                                            OnRcptReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading RCPT TO: command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnRcptReadServerResponseFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[0]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[1])).ToArray());

                    // Response line must start with 250 or otherwise it's error response
                    if (!responseLine.StartsWith("250"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // RCPT TO: completed susscessfully, call callback method.
                        callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished BDAT command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnBdatSendFinished(SocketCallBackResult result,
                                        long count,
                                        Exception exception,
                                        object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[1]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    // BDAT command successfully sent to SMTP server, start sending DATA.
                    m_pSocket.BeginWrite((Stream) (((object[]) tag)[0]), callback, OnBdatDataSendFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished sending BDAT message data to smtp server.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnBdatDataSendFinished(SocketCallBackResult result,
                                            long count,
                                            Exception exception,
                                            object tag)
        {
            CommadCompleted callback = (CommadCompleted) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    // BDAT message data successfully sent to SMTP server, start reading server response
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new object[] {callback, ms},
                                            OnBdatReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading BDAT: command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnBdatReadServerResponseFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[0]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[1])).ToArray());

                    // Response line must start with 250 or otherwise it's error response
                    if (!responseLine.StartsWith("250"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // BDAT: completed susscessfully, call callback method.
                        callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished DATA command sending.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnDataSendFinished(SocketCallBackResult result,
                                        long count,
                                        Exception exception,
                                        object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[1]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    // DATA command has sent to SMTP server, start reading server response.
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new object[] {(((object[]) tag)[0]), callback, ms},
                                            OnDataReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading DATA command server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnDataReadServerResponseFinished(SocketCallBackResult result,
                                                      long count,
                                                      Exception exception,
                                                      object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[1]);

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[2])).ToArray());

                    // Response line must start with 334 or otherwise it's error response
                    if (!responseLine.StartsWith("354"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        Stream message = (Stream) (((object[]) tag)[0]);

                        // Start sending message to smtp server
                        m_pSocket.BeginWritePeriodTerminated(message, callback, OnDataMessageSendFinished);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has sending MESSAGE to smtp server.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnDataMessageSendFinished(SocketCallBackResult result,
                                               long count,
                                               Exception exception,
                                               object tag)
        {
            CommadCompleted callback = (CommadCompleted) tag;

            try
            {
                if (result == SocketCallBackResult.Ok)
                {
                    // Message has successfully sent to smtp server, start reading server response
                    MemoryStream ms = new MemoryStream();
                    m_pSocket.BeginReadLine(ms,
                                            1000,
                                            new object[] {callback, ms},
                                            OnDataMessageSendReadServerResponseFinished);
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Is called when smtp client has finished reading MESSAGE send smtp server response line.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="count"></param>
        /// <param name="exception"></param>
        /// <param name="tag"></param>
        private void OnDataMessageSendReadServerResponseFinished(SocketCallBackResult result,
                                                                 long count,
                                                                 Exception exception,
                                                                 object tag)
        {
            CommadCompleted callback = (CommadCompleted) (((object[]) tag)[0]);

            try
            {
                // TODO: some servers close connection after DATA command, hanndle Socket closed.

                if (result == SocketCallBackResult.Ok)
                {
                    string responseLine =
                        Encoding.ASCII.GetString(((MemoryStream) (((object[]) tag)[1])).ToArray());

                    // Response line must start with 250 or otherwise it's error response
                    if (!responseLine.StartsWith("250"))
                    {
                        throw new Exception(responseLine);
                    }
                    else
                    {
                        // DATA: completed susscessfully, call callback method.
                        callback(SocketCallBackResult.Ok, null);
                    }
                }
                else
                {
                    HandleSocketError(result, exception);
                }
            }
            catch (Exception x)
            {
                // Pass exception to callback method
                callback(SocketCallBackResult.Exception, x);
            }
        }

        /// <summary>
        /// Handles socket errors.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="x"></param>
        private void HandleSocketError(SocketCallBackResult result, Exception x)
        {
            // Log socket errors to log
            if (m_pSocket.Logger != null)
            {
                if (result == SocketCallBackResult.SocketClosed)
                {
                    m_pSocket.Logger.AddTextEntry("Server closed socket !");
                }
                else if (x != null && x is SocketException)
                {
                    SocketException socketException = (SocketException) x;
                    // Server disconnected or aborted connection
                    if (socketException.ErrorCode == 10054 || socketException.ErrorCode == 10053)
                    {
                        m_pSocket.Logger.AddTextEntry("Server closed socket or aborted connection !");
                    }
                }
                else
                {
                    m_pSocket.Logger.AddTextEntry("Unknown error !");
                }
            }

            if (result == SocketCallBackResult.Exception)
            {
                throw x;
            }
            else
            {
                throw new Exception(result.ToString());
            }
        }

        #endregion
    }
}