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


namespace ASC.Mail.Net.TCP
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Timers;
    using Log;

    #endregion

    /// <summary>
    /// This class implements generic TCP session based server.
    /// </summary>
    public class TCP_Server<T> : IDisposable where T : TCP_ServerSession, new()
    {
        #region Events

        /// <summary>
        /// This event is raised when TCP server has disposed.
        /// </summary>
        public event EventHandler Disposed = null;

        /// <summary>
        /// This event is raised when TCP server has unknown unhandled error.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// This event is raised when TCP server creates new session.
        /// </summary>
        public event EventHandler<TCP_ServerSessionEventArgs<T>> SessionCreated = null;

        /// <summary>
        /// This event is raised when TCP server has started.
        /// </summary>
        public event EventHandler Started = null;

        /// <summary>
        /// This event is raised when TCP server has stopped.
        /// </summary>
        public event EventHandler Stopped = null;

        #endregion

        #region Members

        private readonly List<ListeningPoint> m_pListeningPoints;
        private long m_ConnectionsProcessed;

        private bool m_IsDisposed;
        private bool m_IsRunning;
        private long m_MaxConnections;
        private long m_MaxConnectionsPerIP;
        private IPBindInfo[] m_pBindings = new IPBindInfo[0];
        private Logger m_pLogger;
        private TCP_SessionCollection<T> m_pSessions;
        private TimerEx m_pTimer_IdleTimeout;
        private int m_SessionIdleTimeout = 100;
        private DateTime m_StartTime;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TCP_Server()
        {
            m_pListeningPoints = new List<ListeningPoint>();
            m_pSessions = new TCP_SessionCollection<T>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets TCP server IP bindings.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPBindInfo[] Bindings
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pBindings;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (value == null)
                {
                    value = new IPBindInfo[0];
                }

                //--- See binds has changed --------------
                bool changed = false;
                if (m_pBindings.Length != value.Length)
                {
                    changed = true;
                }
                else
                {
                    for (int i = 0; i < m_pBindings.Length; i++)
                    {
                        if (!m_pBindings[i].Equals(value[i]))
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (changed)
                {
                    m_pBindings = value;

                    if (m_IsRunning)
                    {
                        StartListen();
                    }
                }
            }
        }

        /// <summary>
        /// Gets how many connections this TCP server has processed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP server is not running and this property is accesed.</exception>
        public long ConnectionsProcessed
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }
                if (!m_IsRunning)
                {
                    throw new InvalidOperationException("TCP server is not running.");
                }

                return m_ConnectionsProcessed;
            }
        }

        /// <summary>
        /// Gets if server is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        /// <summary>
        /// Gets if server is running.
        /// </summary>
        public bool IsRunning
        {
            get { return m_IsRunning; }
        }

        /// <summary>
        /// Gets local listening IP end points.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPEndPoint[] LocalEndPoints
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                List<IPEndPoint> retVal = new List<IPEndPoint>();
                foreach (IPBindInfo bind in Bindings)
                {
                    if (bind.IP.Equals(IPAddress.Any))
                    {
                        foreach (IPAddress ip in Dns.GetHostAddresses(""))
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork &&
                                !retVal.Contains(new IPEndPoint(ip, bind.Port)))
                            {
                                retVal.Add(new IPEndPoint(ip, bind.Port));
                            }
                        }
                    }
                    else if (bind.IP.Equals(IPAddress.IPv6Any))
                    {
                        foreach (IPAddress ip in Dns.GetHostAddresses(""))
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetworkV6 &&
                                !retVal.Contains(new IPEndPoint(ip, bind.Port)))
                            {
                                retVal.Add(new IPEndPoint(ip, bind.Port));
                            }
                        }
                    }
                    else
                    {
                        if (!retVal.Contains(bind.EndPoint))
                        {
                            retVal.Add(bind.EndPoint);
                        }
                    }
                }

                return retVal.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets logger. Value null means no logging.
        /// </summary>
        public Logger Logger
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pLogger;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                m_pLogger = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed concurent connections. Value 0 means unlimited.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public long MaxConnections
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }

                return m_MaxConnections;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }
                if (value < 0)
                {
                    throw new ArgumentException("Property 'MaxConnections' value must be >= 0.");
                }

                m_MaxConnections = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed connections for 1 IP address. Value 0 means unlimited.
        /// </summary>
        public long MaxConnectionsPerIP
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }

                return m_MaxConnectionsPerIP;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }
                if (m_MaxConnectionsPerIP < 0)
                {
                    throw new ArgumentException("Property 'MaxConnectionsPerIP' value must be >= 0.");
                }

                m_MaxConnectionsPerIP = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed session idle time in seconds, after what session will be terminated. Value 0 means unlimited,
        /// but this is strongly not recommened.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public int SessionIdleTimeout
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }

                return m_SessionIdleTimeout;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }
                if (m_SessionIdleTimeout < 0)
                {
                    throw new ArgumentException("Property 'SessionIdleTimeout' value must be >= 0.");
                }

                m_SessionIdleTimeout = value;
            }
        }

        /// <summary>
        /// Gets TCP server active sessions.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP server is not running and this property is accesed.</exception>
        public TCP_SessionCollection<T> Sessions
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }
                if (!m_IsRunning)
                {
                    throw new InvalidOperationException("TCP server is not running.");
                }

                return m_pSessions;
            }
        }

        /// <summary>
        /// Gets the time when server was started.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP server is not running and this property is accesed.</exception>
        public DateTime StartTime
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Server");
                }
                if (!m_IsRunning)
                {
                    throw new InvalidOperationException("TCP server is not running.");
                }

                return m_StartTime;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            if (m_IsRunning)
            {
                try
                {
                    Stop();
                }
                catch {}
            }
            m_IsDisposed = true;

            // We must call disposed event before we release events.
            try
            {
                OnDisposed();
            }
            catch
            {
                // We never should get exception here, user should handle it, just skip it.
            }

            m_pSessions = null;

            // Release all events.
            Started = null;
            Stopped = null;
            Disposed = null;
            Error = null;
        }

        /// <summary>
        /// Starts TCP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public void Start()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_Server");
            }
            if (m_IsRunning)
            {
                return;
            }
            m_IsRunning = true;

            m_StartTime = DateTime.Now;
            m_ConnectionsProcessed = 0;

            ThreadPool.QueueUserWorkItem(delegate { StartListen(); });

            m_pTimer_IdleTimeout = new TimerEx(30000, true);
            m_pTimer_IdleTimeout.Elapsed += m_pTimer_IdleTimeout_Elapsed;
            m_pTimer_IdleTimeout.Enabled = true;

            OnStarted();
        }

        /// <summary>
        /// Stops TCP server, all active connections will be terminated.
        /// </summary>
        public void Stop()
        {
            if (!m_IsRunning)
            {
                return;
            }
            m_IsRunning = false;

            // Dispose all old binds.
            foreach (ListeningPoint listeningPoint in m_pListeningPoints.ToArray())
            {
                try
                {
                    listeningPoint.Socket.Close();
                }
                catch (Exception x)
                {
                    OnError(x);
                }
            }
            m_pListeningPoints.Clear();

            m_pTimer_IdleTimeout.Dispose();
            m_pTimer_IdleTimeout = null;

            OnStopped();
        }

        /// <summary>
        /// Restarts TCP server.
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Is called when new incoming session and server maximum allowed connections exceeded.
        /// </summary>
        /// <param name="session">Incoming session.</param>
        /// <remarks>This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected virtual void OnMaxConnectionsExceeded(T session) {}

        /// <summary>
        /// Is called when new incoming session and server maximum allowed connections per connected IP exceeded.
        /// </summary>
        /// <param name="session">Incoming session.</param>
        /// <remarks>This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected virtual void OnMaxConnectionsPerIPExceeded(T session) {}

        #endregion

        #region Utility methods

        /// <summary>
        /// Is called when session idle check timer triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimer_IdleTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (T session in Sessions.ToArray())
                {
                    try
                    {
                        if (DateTime.Now > session.TcpStream.LastActivity.AddSeconds(m_SessionIdleTimeout))
                        {
                            
                            session.OnTimeoutI();
                            // Session didn't dispose itself, so dispose it.
                            if (!session.IsDisposed)
                            {
                                session.Disconnect();
                                session.Dispose();
                            }
                        }
                    }
                    catch {}
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Starts listening incoming connections. NOTE: All active listening points will be disposed.
        /// </summary>
        private void StartListen()
        {
            try
            {
                // Dispose all old binds.
                foreach (ListeningPoint listeningPoint in m_pListeningPoints.ToArray())
                {
                    try
                    {
                        listeningPoint.Socket.Close();
                    }
                    catch (Exception x)
                    {
                        OnError(x);
                    }
                }
                m_pListeningPoints.Clear();

                // Create new listening points and start accepting connections.
                bool ioCompletion_asyncSockets = Net_Utils.IsIoCompletionPortsSupported();
                foreach (IPBindInfo bind in m_pBindings)
                {
                    try
                    {
                        Socket socket = null;
                        if (bind.IP.AddressFamily == AddressFamily.InterNetwork)
                        {
                            socket = new Socket(AddressFamily.InterNetwork,
                                                SocketType.Stream,
                                                ProtocolType.Tcp);
                        }
                        else if (bind.IP.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            socket = new Socket(AddressFamily.InterNetworkV6,
                                                SocketType.Stream,
                                                ProtocolType.Tcp);
                        }
                        else
                        {
                            // Invalid address family, just skip it.
                            continue;
                        }
                        socket.Bind(new IPEndPoint(bind.IP, bind.Port));
                        socket.Listen(100);

                        ListeningPoint listeningPoint = new ListeningPoint(socket, bind);
                        m_pListeningPoints.Add(listeningPoint);

                        // Begin accept.
                        //   We MUST use socket.AcceptAsync method, this consume all threading power in Windows paltform(IO completion ports).
                        //   For other platforms we need to use BeginAccept.

                        #region IO completion ports

                        if (ioCompletion_asyncSockets)
                        {
                            SocketAsyncEventArgs eArgs = new SocketAsyncEventArgs();
                            eArgs.Completed += delegate(object s, SocketAsyncEventArgs e)
                                                   {
                                                       if (e.SocketError == SocketError.Success)
                                                       {
                                                           ProcessConnection(e.AcceptSocket, bind);
                                                       }

                                                       // Start accepting new connection.
                                                       IOCompletionBeginAccept(e, socket, bind);
                                                   };

                            // Move processing to thread-pool, because IOCompletionBeginAccept keeps using calling thread as loang as there is work todo.
                            ThreadPool.QueueUserWorkItem(delegate
                                                             {
                                                                 // Start accepting new connection.
                                                                 IOCompletionBeginAccept(eArgs, socket, bind);
                                                             });
                        }

                            #endregion

                            #region Async sockets

                        else
                        {
                            // Begin accepting connection.
                            socket.BeginAccept(new AsyncCallback(AsynSocketsAcceptCompleted), listeningPoint);
                        }

                        #endregion
                    }
                    catch (Exception x)
                    {
                        // The only exception what we should get there is if socket is in use.
                        OnError(x);
                    }
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Starts accepting connection(s).
        /// </summary>
        /// <param name="socketArgs">AcceptAsync method data.</param>
        /// <param name="listeningSocket">Local listening socket.</param>
        /// <param name="bindInfo">Local listening socket bind info.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>socketArgs</b>,<b>listeningSocket</b> or <b>bindInfo</b> is null reference.</exception>
        private void IOCompletionBeginAccept(SocketAsyncEventArgs socketArgs,
                                             Socket listeningSocket,
                                             IPBindInfo bindInfo)
        {
            if (socketArgs == null)
            {
                throw new ArgumentNullException("socketArgs");
            }
            if (listeningSocket == null)
            {
                throw new ArgumentNullException("listeningSocket");
            }
            if (bindInfo == null)
            {
                throw new ArgumentNullException("bindInfo");
            }

            try
            {
                // We need to clear it, before reuse.
                socketArgs.AcceptSocket = null;

                // Use active worker thread as long as AcceptAsync completes synchronously.
                // (With this approeach we don't have thread context switches while AcceptAsync completes synchronously)
                while (m_IsRunning && !listeningSocket.AcceptAsync(socketArgs))
                {
                    // Operation completed synchronously.

                    if (socketArgs.SocketError == SocketError.Success)
                    {
                        ProcessConnection(socketArgs.AcceptSocket, bindInfo);
                    }

                    // We need to clear it, before reuse.
                    socketArgs.AcceptSocket = null;
                }
            }
            catch (ObjectDisposedException x)
            {
                string dummy = x.Message;
                // Listening socket closed, so skip that error.
            }
        }

        /// <summary>
        /// This method is called when BeginAccept ha completed.
        /// </summary>
        /// <param name="ar">The result of the asynchronous operation.</param>
        private void AsynSocketsAcceptCompleted(IAsyncResult ar)
        {
            ListeningPoint lPoint = (ListeningPoint) ar.AsyncState;
            try
            {
                ProcessConnection(lPoint.Socket.EndAccept(ar), lPoint.BindInfo);
            }
            catch
            {
                // Skip accept errors.
            }

            // Begin accepting connection.
            lPoint.Socket.BeginAccept(new AsyncCallback(AsynSocketsAcceptCompleted), lPoint);
        }

        /// <summary>
        /// Processes specified connection.
        /// </summary>
        /// <param name="socket">Accpeted socket.</param>
        /// <param name="bindInfo">Local bind info what accpeted connection.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> or <b>bindInfo</b> is null reference.</exception>
        private void ProcessConnection(Socket socket, IPBindInfo bindInfo)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (bindInfo == null)
            {
                throw new ArgumentNullException("bindInfo");
            }

            m_ConnectionsProcessed++;

            try
            {
                T session = new T();
                session.Init(this,
                             socket,
                             bindInfo.HostName,
                             bindInfo.SslMode == SslMode.SSL,
                             bindInfo.Certificate);

                // Maximum allowed connections exceeded, reject connection.
                if (m_MaxConnections != 0 && m_pSessions.Count > m_MaxConnections)
                {
                    OnMaxConnectionsExceeded(session);
                    session.Dispose();
                }
                    // Maximum allowed connections per IP exceeded, reject connection.
                else if (m_MaxConnectionsPerIP != 0)
                {
                    OnMaxConnectionsPerIPExceeded(session);
                    session.Dispose();
                }
                    // Start processing new session.
                else
                {
                    session.Disonnected +=
                        delegate(object sender, EventArgs e) { m_pSessions.Remove((T)sender); };
                    m_pSessions.Add(session);

                    OnSessionCreated(session);

                    session.Start();
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Raises <b>SessionCreated</b> event.
        /// </summary>
        /// <param name="session">TCP server session that was created.</param>
        private void OnSessionCreated(T session)
        {
            if (SessionCreated != null)
            {
                SessionCreated(this, new TCP_ServerSessionEventArgs<T>(this, session));
            }
        }

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        protected void OnError(Exception x)
        {
            if (Error != null)
            {
                Error(this, new Error_EventArgs(x, new StackTrace()));
            }
        }

        #endregion

        /// <summary>
        /// Raises <b>Started</b> event.
        /// </summary>
        protected void OnStarted()
        {
            if (Started != null)
            {
                Started(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>Stopped</b> event.
        /// </summary>
        protected void OnStopped()
        {
            if (Stopped != null)
            {
                Stopped(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>Disposed</b> event.
        /// </summary>
        protected void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, new EventArgs());
            }
        }

        #region Nested type: ListeningPoint

        /// <summary>
        /// This class holds listening point info.
        /// </summary>
        private class ListeningPoint
        {
            #region Members

            private readonly IPBindInfo m_pBindInfo;
            private readonly Socket m_pSocket;

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="socket">Listening socket.</param>
            /// <param name="bind">Bind info what acceped socket.</param>
            public ListeningPoint(Socket socket, IPBindInfo bind)
            {
                m_pSocket = socket;
                m_pBindInfo = bind;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets bind info.
            /// </summary>
            public IPBindInfo BindInfo
            {
                get { return m_pBindInfo; }
            }

            /// <summary>
            /// Gets socket.
            /// </summary>
            public Socket Socket
            {
                get { return m_pSocket; }
            }

            #endregion
        }

        #endregion
    }
}