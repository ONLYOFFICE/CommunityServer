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


namespace ASC.Mail.Net.SMTP.Relay
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Principal;
    using Client;
    using Dns.Client;
    using IO;
    using Log;
    using TCP;

    #endregion

    /// <summary>
    /// This class implements SMTP relay server session.
    /// </summary>
    public class Relay_Session : TCP_Session
    {
        #region Members

        private readonly Relay_Mode m_RelayMode = Relay_Mode.Dns;
        private readonly DateTime m_SessionCreateTime;
        private readonly string m_SessionID = "";

        private bool m_IsDisposed;
        private Relay_Target m_pActiveTarget;
        private IPBindInfo m_pLocalBindInfo;
        private Relay_QueueItem m_pRelayItem;
        private Relay_Server m_pServer;
        private Relay_SmartHost[] m_pSmartHosts;
        private SMTP_Client m_pSmtpClient;
        private List<Relay_Target> m_pTargets;

        #endregion

        #region Constructor

        /// <summary>
        /// Dns relay session constructor.
        /// </summary>
        /// <param name="server">Owner relay server.</param>
        /// <param name="localBindInfo">Local bind info.</param>
        /// <param name="realyItem">Relay item.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>server</b>,<b>localBindInfo</b> or <b>realyItem</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        internal Relay_Session(Relay_Server server, IPBindInfo localBindInfo, Relay_QueueItem realyItem)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (localBindInfo == null)
            {
                throw new ArgumentNullException("localBindInfo");
            }
            if (realyItem == null)
            {
                throw new ArgumentNullException("realyItem");
            }

            m_pServer = server;
            m_pLocalBindInfo = localBindInfo;
            m_pRelayItem = realyItem;

            m_SessionID = Guid.NewGuid().ToString();
            m_SessionCreateTime = DateTime.Now;
            m_pTargets = new List<Relay_Target>();
        }

        /// <summary>
        /// Smart host relay session constructor.
        /// </summary>
        /// <param name="server">Owner relay server.</param>
        /// <param name="localBindInfo">Local bind info.</param>
        /// <param name="realyItem">Relay item.</param>
        /// <param name="smartHosts">Smart hosts.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>server</b>,<b>localBindInfo</b>,<b>realyItem</b> or <b>smartHosts</b>is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        internal Relay_Session(Relay_Server server,
                               IPBindInfo localBindInfo,
                               Relay_QueueItem realyItem,
                               Relay_SmartHost[] smartHosts)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (localBindInfo == null)
            {
                throw new ArgumentNullException("localBindInfo");
            }
            if (realyItem == null)
            {
                throw new ArgumentNullException("realyItem");
            }
            if (smartHosts == null)
            {
                throw new ArgumentNullException("smartHosts");
            }

            m_pServer = server;
            m_pLocalBindInfo = localBindInfo;
            m_pRelayItem = realyItem;
            m_pSmartHosts = smartHosts;

            m_RelayMode = Relay_Mode.SmartHost;
            m_SessionID = Guid.NewGuid().ToString();
            m_SessionCreateTime = DateTime.Now;
            m_pTargets = new List<Relay_Target>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets session authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and relay session is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!m_pSmtpClient.IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pSmtpClient.AuthenticatedUserIdentity;
            }
        }

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override DateTime ConnectTime
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmtpClient.ConnectTime;
            }
        }

        /// <summary>
        /// Gets how many seconds has left before timout is triggered.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int ExpectedTimeout
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return
                    (int)
                    (m_pServer.SessionIdleTimeout -
                     ((DateTime.Now.Ticks - TcpStream.LastActivity.Ticks)/10000));
            }
        }

        /// <summary>
        /// Gets from address.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string From
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRelayItem.From;
            }
        }

        /// <summary>
        /// Gets session ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override string ID
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SessionID;
            }
        }

        /// <summary>
        /// Gets if session is connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool IsConnected
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmtpClient.IsConnected;
            }
        }

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override DateTime LastActivity
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmtpClient.LastActivity;
            }
        }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override IPEndPoint LocalEndPoint
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmtpClient.LocalEndPoint;
            }
        }

        /// <summary>
        /// Gets message ID which is being relayed now.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string MessageID
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRelayItem.MessageID;
            }
        }

        /// <summary>
        /// Gets message what is being relayed now.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Stream MessageStream
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRelayItem.MessageStream;
            }
        }

        /// <summary>
        /// Gets relay queue which session it is.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Relay_Queue Queue
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRelayItem.Queue;
            }
        }

        /// <summary>
        /// Gets user data what was procided to Relay_Queue.QueueMessage method.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public object QueueTag
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRelayItem.Tag;
            }
        }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmtpClient.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Gets time when relay session created.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public DateTime SessionCreateTime
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SessionCreateTime;
            }
        }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override SmartStream TcpStream
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmtpClient.TcpStream;
            }
        }

        /// <summary>
        /// Gets target recipient.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string To
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRelayItem.To;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Completes relay session and does clean up. This method is thread-safe.
        /// </summary>
        public override void Dispose()
        {
            Dispose(new ObjectDisposedException(GetType().Name));
        }

        /// <summary>
        /// Completes relay session and does clean up. This method is thread-safe.
        /// </summary>
        /// <param name="exception">Exception happened or null if relay completed successfully.</param>
        public void Dispose(Exception exception)
        {
            try
            {
                lock (this)
                {
                    if (m_IsDisposed)
                    {
                        return;
                    }
                    try
                    {
                        m_pServer.OnSessionCompleted(this, exception);
                    }
                    catch {}
                    m_pServer.Sessions.Remove(this);
                    m_IsDisposed = true;

                    m_pLocalBindInfo = null;
                    m_pRelayItem = null;
                    m_pSmartHosts = null;
                    if (m_pSmtpClient != null)
                    {
                        m_pSmtpClient.Dispose();
                        m_pSmtpClient = null;
                    }
                    m_pTargets = null;
                    if (m_pActiveTarget != null)
                    {
                        m_pServer.RemoveIpUsage(m_pActiveTarget.Target.Address);
                        m_pActiveTarget = null;
                    }
                    m_pServer = null;
                }
            }
            catch (Exception x)
            {
                if (m_pServer != null)
                {
                    m_pServer.OnError(x);
                }
            }
        }

        /// <summary>
        /// Closes relay connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public override void Disconnect()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                return;
            }

            m_pSmtpClient.Disconnect();
        }

        /// <summary>
        /// Closes relay connection.
        /// </summary>
        /// <param name="text">Text to send to the connected host.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public void Disconnect(string text)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                return;
            }

            m_pSmtpClient.TcpStream.WriteLine(text);
            Disconnect();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Start processing relay message.
        /// </summary>
        /// <param name="state">User data.</param>
        internal void Start(object state)
        {
            try
            {
                m_pSmtpClient = new SMTP_Client();
                m_pSmtpClient.LocalHostName = m_pLocalBindInfo.HostName;
                if (m_pServer.Logger != null)
                {
                    m_pSmtpClient.Logger = new Logger();
                    m_pSmtpClient.Logger.WriteLog += SmtpClient_WriteLog;
                }

                LogText("Starting to relay message '" + m_pRelayItem.MessageID + "' from '" +
                        m_pRelayItem.From + "' to '" + m_pRelayItem.To + "'.");

                // Get all possible target hosts for active recipient.
                List<string> targetHosts = new List<string>();
                if (m_RelayMode == Relay_Mode.Dns)
                {
                    foreach (string host in SMTP_Client.GetDomainHosts(m_pRelayItem.To))
                    {
                        try
                        {
                            foreach (IPAddress ip in Dns_Client.Resolve(host))
                            {
                                m_pTargets.Add(new Relay_Target(new IPEndPoint(ip, 25)));
                            }
                        }
                        catch
                        {
                            // Failed to resolve host name.

                            LogText("Failed to resolve host '" + host + "' name.");
                        }
                    }
                }
                else if (m_RelayMode == Relay_Mode.SmartHost)
                {
                    foreach (Relay_SmartHost smartHost in m_pSmartHosts)
                    {
                        try
                        {
                            m_pTargets.Add(
                                new Relay_Target(
                                    new IPEndPoint(Dns_Client.Resolve(smartHost.Host)[0], smartHost.Port),
                                    smartHost.SslMode,
                                    smartHost.UserName,
                                    smartHost.Password));
                        }
                        catch
                        {
                            // Failed to resolve smart host name.

                            LogText("Failed to resolve smart host '" + smartHost.Host + "' name.");
                        }
                    }
                }

                BeginConnect();
            }
            catch (Exception x)
            {
                Dispose(x);
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Thsi method is called when SMTP client has new log entry available.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void SmtpClient_WriteLog(object sender, WriteLogEventArgs e)
        {
            try
            {
                if (m_pServer.Logger == null) {}
                else if (e.LogEntry.EntryType == LogEntryType.Read)
                {
                    m_pServer.Logger.AddRead(m_SessionID,
                                             e.LogEntry.UserIdentity,
                                             e.LogEntry.Size,
                                             e.LogEntry.Text,
                                             e.LogEntry.LocalEndPoint,
                                             e.LogEntry.RemoteEndPoint);
                }
                else if (e.LogEntry.EntryType == LogEntryType.Text)
                {
                    m_pServer.Logger.AddText(m_SessionID,
                                             e.LogEntry.UserIdentity,
                                             e.LogEntry.Text,
                                             e.LogEntry.LocalEndPoint,
                                             e.LogEntry.RemoteEndPoint);
                }
                else if (e.LogEntry.EntryType == LogEntryType.Write)
                {
                    m_pServer.Logger.AddWrite(m_SessionID,
                                              e.LogEntry.UserIdentity,
                                              e.LogEntry.Size,
                                              e.LogEntry.Text,
                                              e.LogEntry.LocalEndPoint,
                                              e.LogEntry.RemoteEndPoint);
                }
            }
            catch {}
        }

        /// <summary>
        /// Starts connecting to best target. 
        /// </summary>
        private void BeginConnect()
        {
            // No tagets, abort relay.
            if (m_pTargets.Count == 0)
            {
                LogText("No relay target(s) for '" + m_pRelayItem.To + "', aborting.");
                Dispose(new Exception("No relay target(s) for '" + m_pRelayItem.To + "', aborting."));
                return;
            }

            // If maximum connections to specified target exceeded and there are more targets, try to get limit free target.            
            if (m_pServer.MaxConnectionsPerIP > 0)
            {
                // For DNS or load-balnced smart host relay, search free target if any.
                if (m_pServer.RelayMode == Relay_Mode.Dns ||
                    m_pServer.SmartHostsBalanceMode == BalanceMode.LoadBalance)
                {
                    foreach (Relay_Target t in m_pTargets)
                    {
                        // We found free target, stop searching.
                        if (m_pServer.TryAddIpUsage(m_pTargets[0].Target.Address))
                        {
                            m_pActiveTarget = t;
                            m_pTargets.Remove(t);
                            break;
                        }
                    }
                }
                    // Smart host fail-over mode, just check if it's free.
                else
                {
                    // Smart host IP limit not reached.
                    if (m_pServer.TryAddIpUsage(m_pTargets[0].Target.Address))
                    {
                        m_pActiveTarget = m_pTargets[0];
                        m_pTargets.RemoveAt(0);
                    }
                }
            }
                // Just get first target.
            else
            {
                m_pActiveTarget = m_pTargets[0];
                m_pTargets.RemoveAt(0);
            }

            // If all targets has exeeded maximum allowed connection per IP address, end relay session, 
            // next relay cycle will try to relay again.
            if (m_pActiveTarget == null)
            {
                LogText("All targets has exeeded maximum allowed connection per IP address, skip relay.");
                Dispose(
                    new Exception(
                        "All targets has exeeded maximum allowed connection per IP address, skip relay."));
                return;
            }

            m_pSmtpClient.BeginConnect(new IPEndPoint(m_pLocalBindInfo.IP, 0),
                                       m_pActiveTarget.Target,
                                       false,
                                       ConnectCallback,
                                       null);
        }

        /// <summary>
        /// This method is called when asynchronous Connect method completes.
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                m_pSmtpClient.EndConnect(ar);

                // Start TLS requested, start switching to SSL.
                if (m_pActiveTarget.SslMode == SslMode.TLS)
                {
                    m_pSmtpClient.BeginStartTLS(StartTlsCallback, null);
                }
                    // Authentication requested, start authenticating.
                else if (!string.IsNullOrEmpty(m_pActiveTarget.UserName))
                {
                    m_pSmtpClient.BeginAuthenticate(m_pActiveTarget.UserName,
                                                    m_pActiveTarget.Password,
                                                    AuthenticateCallback,
                                                    null);
                }
                else
                {
                    long messageSize = -1;
                    try
                    {
                        messageSize = m_pRelayItem.MessageStream.Length - m_pRelayItem.MessageStream.Position;
                    }
                    catch
                    {
                        // Stream doesn't support seeking.
                    }

                    m_pSmtpClient.BeginMailFrom(From, messageSize, MailFromCallback, null);
                }
            }
            catch (Exception x)
            {
                try
                {
                    // Release IP usage.
                    m_pServer.RemoveIpUsage(m_pActiveTarget.Target.Address);
                    m_pActiveTarget = null;

                    // Connect failed, if there are more target IPs, try next one.
                    if (!IsDisposed && !IsConnected && m_pTargets.Count > 0)
                    {
                        BeginConnect();
                    }
                    else
                    {
                        Dispose(x);
                    }
                }
                catch (Exception xx)
                {
                    Dispose(xx);
                }
            }
        }

        /// <summary>
        /// This method is called when asynchronous <b>StartTLS</b> method completes.
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        private void StartTlsCallback(IAsyncResult ar)
        {
            try
            {
                m_pSmtpClient.EndStartTLS(ar);

                // Authentication requested, start authenticating.
                if (!string.IsNullOrEmpty(m_pActiveTarget.UserName))
                {
                    m_pSmtpClient.BeginAuthenticate(m_pActiveTarget.UserName,
                                                    m_pActiveTarget.Password,
                                                    AuthenticateCallback,
                                                    null);
                }
                else
                {
                    long messageSize = -1;
                    try
                    {
                        messageSize = m_pRelayItem.MessageStream.Length - m_pRelayItem.MessageStream.Position;
                    }
                    catch
                    {
                        // Stream doesn't support seeking.
                    }

                    m_pSmtpClient.BeginMailFrom(From, messageSize, MailFromCallback, null);
                }
            }
            catch (Exception x)
            {
                Dispose(x);
            }
        }

        /// <summary>
        /// This method is called when asynchronous <b>Authenticate</b> method completes.
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        private void AuthenticateCallback(IAsyncResult ar)
        {
            try
            {
                m_pSmtpClient.EndAuthenticate(ar);

                long messageSize = -1;
                try
                {
                    messageSize = m_pRelayItem.MessageStream.Length - m_pRelayItem.MessageStream.Position;
                }
                catch
                {
                    // Stream doesn't support seeking.
                }

                m_pSmtpClient.BeginMailFrom(From, messageSize, MailFromCallback, null);
            }
            catch (Exception x)
            {
                Dispose(x);
            }
        }

        /// <summary>
        /// This method is called when asynchronous MailFrom method completes.
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        private void MailFromCallback(IAsyncResult ar)
        {
            try
            {
                m_pSmtpClient.EndMailFrom(ar);

                m_pSmtpClient.BeginRcptTo(To, RcptToCallback, null);
            }
            catch (Exception x)
            {
                Dispose(x);
            }
        }

        /// <summary>
        /// This method is called when asynchronous RcptTo method completes.
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        private void RcptToCallback(IAsyncResult ar)
        {
            try
            {
                m_pSmtpClient.EndRcptTo(ar);

                m_pSmtpClient.BeginSendMessage(m_pRelayItem.MessageStream, SendMessageCallback, null);
            }
            catch (Exception x)
            {
                Dispose(x);
            }
        }

        /// <summary>
        /// This method is called when asynchronous SendMessage method completes.
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        private void SendMessageCallback(IAsyncResult ar)
        {
            try
            {
                m_pSmtpClient.EndSendMessage(ar);

                // Message relayed successfully.
                Dispose(null);
            }
            catch (Exception x)
            {
                Dispose(x);
            }
        }

        /// <summary>
        /// Logs specified text if logging enabled.
        /// </summary>
        /// <param name="text">Text to log.</param>
        private void LogText(string text)
        {
            if (m_pServer.Logger != null)
            {
                GenericIdentity identity = null;
                try
                {
                    identity = AuthenticatedUserIdentity;
                }
                catch {}
                IPEndPoint localEP = null;
                IPEndPoint remoteEP = null;
                try
                {
                    localEP = m_pSmtpClient.LocalEndPoint;
                    remoteEP = m_pSmtpClient.RemoteEndPoint;
                }
                catch {}
                m_pServer.Logger.AddText(m_SessionID, identity, text, localEP, remoteEP);
            }
        }

        #endregion

        #region Nested type: Relay_Target

        /// <summary>
        /// This class holds relay target information.
        /// </summary>
        private class Relay_Target
        {
            #region Members

            private readonly string m_Password;
            private readonly IPEndPoint m_pTarget;
            private readonly SslMode m_SslMode = SslMode.None;
            private readonly string m_UserName;

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="target">Target host IP end point.</param>
            public Relay_Target(IPEndPoint target)
            {
                m_pTarget = target;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="target">Target host IP end point.</param>
            /// <param name="sslMode">SSL mode.</param>
            /// <param name="userName">Target host user name.</param>
            /// <param name="password">Target host password.</param>
            public Relay_Target(IPEndPoint target, SslMode sslMode, string userName, string password)
            {
                m_pTarget = target;
                m_SslMode = sslMode;
                m_UserName = userName;
                m_Password = password;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets target server password.
            /// </summary>
            public string Password
            {
                get { return m_Password; }
            }

            /// <summary>
            /// Gets target SSL mode.
            /// </summary>
            public SslMode SslMode
            {
                get { return m_SslMode; }
            }

            /// <summary>
            /// Gets specified target IP end point.
            /// </summary>
            public IPEndPoint Target
            {
                get { return m_pTarget; }
            }

            /// <summary>
            /// Gets target server user name.
            /// </summary>
            public string UserName
            {
                get { return m_UserName; }
            }

            #endregion
        }

        #endregion
    }
}