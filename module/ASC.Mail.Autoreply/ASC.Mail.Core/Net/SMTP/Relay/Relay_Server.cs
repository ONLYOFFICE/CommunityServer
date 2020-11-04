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
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Log;
    using TCP;

    #endregion

    #region Delegates

    /// <summary>
    /// Represents the method that will handle the <b>Relay_Server.SessionCompleted</b> event.
    /// </summary>
    /// <param name="e">Event data.</param>
    public delegate void Relay_SessionCompletedEventHandler(Relay_SessionCompletedEventArgs e);

    #endregion

    /// <summary>
    /// This class implements SMTP relay server. Defined in RFC 2821.
    /// </summary>
    public class Relay_Server : IDisposable
    {
        #region Events

        /// <summary>
        /// This event is raised when unhandled exception happens.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// This event is raised when relay session processing completes.
        /// </summary>
        public event Relay_SessionCompletedEventHandler SessionCompleted = null;

        #endregion

        #region Members

        private bool m_HasBindingsChanged;

        private bool m_IsDisposed;
        private bool m_IsRunning;
        private long m_MaxConnections;
        private long m_MaxConnectionsPerIP;
        private IPBindInfo[] m_pBindings = new IPBindInfo[0];
        private Dictionary<IPAddress, long> m_pConnectionsPerIP;
        private CircleCollection<IPBindInfo> m_pLocalEndPoints;
        private List<Relay_Queue> m_pQueues;
        private TCP_SessionCollection<Relay_Session> m_pSessions;
        private CircleCollection<Relay_SmartHost> m_pSmartHosts;
        private Relay_Mode m_RelayMode = Relay_Mode.Dns;
        private int m_SessionIdleTimeout = 30;
        private BalanceMode m_SmartHostsBalanceMode = BalanceMode.LoadBalance;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Relay_Server()
        {
            m_pQueues = new List<Relay_Queue>();
            m_pSmartHosts = new CircleCollection<Relay_SmartHost>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets relay server IP bindings.
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
                    m_HasBindingsChanged = true;
                }
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
        /// Gets or sets relay logger. Value null means no logging.
        /// </summary>
        public Logger Logger { get; set; }

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
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_MaxConnections;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (value < 0)
                {
                    throw new ArgumentException("Property 'MaxConnections' value must be >= 0.");
                }

                m_MaxConnections = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed connections to 1 IP address. Value 0 means unlimited.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public long MaxConnectionsPerIP
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_MaxConnectionsPerIP;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (m_MaxConnectionsPerIP < 0)
                {
                    throw new ArgumentException("Property 'MaxConnectionsPerIP' value must be >= 0.");
                }

                m_MaxConnectionsPerIP = value;
            }
        }

        /// <summary>
        /// Gets relay queues. Queue with lower index number has higher priority.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public List<Relay_Queue> Queues
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pQueues;
            }
        }

        /// <summary>
        /// Gets or sets relay mode.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Relay_Mode RelayMode
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_RelayMode;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                m_RelayMode = value;
            }
        }

        /// <summary>
        /// Gets or sets session idle time in seconds when it will be timed out.  Value 0 means unlimited (strongly not recomended).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public int SessionIdleTimeout
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SessionIdleTimeout;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (m_SessionIdleTimeout < 0)
                {
                    throw new ArgumentException("Property 'SessionIdleTimeout' value must be >= 0.");
                }

                m_SessionIdleTimeout = value;
            }
        }

        /// <summary>
        /// Gets active relay sessions.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and relay server is not running.</exception>
        public TCP_SessionCollection<Relay_Session> Sessions
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!m_IsRunning)
                {
                    throw new InvalidOperationException("Relay server not running.");
                }

                return m_pSessions;
            }
        }

        /// <summary>
        /// Gets or sets smart hosts. Smart hosts must be in priority order, lower index number means higher priority.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public Relay_SmartHost[] SmartHosts
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSmartHosts.ToArray();
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (value == null)
                {
                    throw new ArgumentNullException("SmartHosts");
                }

                m_pSmartHosts.Add(value);
            }
        }

        /// <summary>
        /// Gets or sets how smart hosts will be balanced.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public BalanceMode SmartHostsBalanceMode
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_SmartHostsBalanceMode;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                m_SmartHostsBalanceMode = value;
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
            try
            {
                if (m_IsRunning)
                {
                    Stop();
                }
            }
            catch {}
            m_IsDisposed = true;

            // Release events.
            Error = null;
            SessionCompleted = null;

            m_pQueues = null;
            m_pSmartHosts = null;
        }

        /// <summary>
        /// Starts SMTP relay server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public virtual void Start()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (m_IsRunning)
            {
                return;
            }
            m_IsRunning = true;

            m_pLocalEndPoints = new CircleCollection<IPBindInfo>();
            m_pSessions = new TCP_SessionCollection<Relay_Session>();
            m_pConnectionsPerIP = new Dictionary<IPAddress, long>();

            Thread tr1 = new Thread(Run);
            tr1.Start();

            Thread tr2 = new Thread(Run_CheckTimedOutSessions);
            tr2.Start();
        }

        /// <summary>
        /// Stops SMTP relay server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public virtual void Stop()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!m_IsRunning)
            {
                return;
            }
            m_IsRunning = false;

            // TODO: We need to send notify to all not processed messages, then they can be Disposed as needed.

            // Clean up.            
            m_pLocalEndPoints = null;
            //m_pSessions.Dispose();
            m_pSessions = null;
            m_pConnectionsPerIP = null;
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Raises <b>SessionCompleted</b> event.
        /// </summary>
        /// <param name="session">Session what completed processing.</param>
        /// <param name="exception">Exception happened or null if relay completed successfully.</param>
        protected internal virtual void OnSessionCompleted(Relay_Session session, Exception exception)
        {
            if (SessionCompleted != null)
            {
                SessionCompleted(new Relay_SessionCompletedEventArgs(session, exception));
            }
        }

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="x">Exception happned.</param>
        protected internal virtual void OnError(Exception x)
        {
            if (Error != null)
            {
                Error(this, new Error_EventArgs(x, new StackTrace()));
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Increases specified IP address connactions count if maximum allowed connections to 
        /// the specified IP address isn't exceeded.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        /// <returns>Returns true if specified IP usage increased, false if maximum allowed connections to the specified IP address is exceeded.</returns>
        internal bool TryAddIpUsage(IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            lock (m_pConnectionsPerIP)
            {
                long count = 0;
                // Specified IP entry exists, increase usage.
                if (m_pConnectionsPerIP.TryGetValue(ip, out count))
                {
                    // Maximum allowed connections to the specified IP address is exceeded.
                    if (m_MaxConnectionsPerIP > 0 && count >= m_MaxConnectionsPerIP)
                    {
                        return false;
                    }

                    m_pConnectionsPerIP[ip] = count + 1;
                }
                    // Specified IP entry doesn't exist, create new entry and increase usage.
                else
                {
                    m_pConnectionsPerIP.Add(ip, 1);
                }

                return true;
            }
        }

        /// <summary>
        /// Decreases specified IP address connactions count.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        internal void RemoveIpUsage(IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            lock (m_pConnectionsPerIP)
            {
                long count = 0;
                // Specified IP entry exists, increase usage.
                if (m_pConnectionsPerIP.TryGetValue(ip, out count))
                {
                    // This is last usage to that IP, remove that IP entry.
                    if (count == 1)
                    {
                        m_pConnectionsPerIP.Remove(ip);
                    }
                        // Decrease Ip usage.
                    else
                    {
                        m_pConnectionsPerIP[ip] = count - 1;
                    }
                }
                else
                {
                    // No such entry, just skip it.
                }
            }
        }

        /// <summary>
        /// Gets how many connections to the specified IP address.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <returns>Returns number of connections to the specified IP address.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        internal long GetIpUsage(IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            lock (m_pConnectionsPerIP)
            {
                long count = 0;
                // Specified IP entry exists, return usage.
                if (m_pConnectionsPerIP.TryGetValue(ip, out count))
                {
                    return count;
                }
                    // No usage to specified IP.
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Processes relay queue.
        /// </summary>
        private void Run()
        {
            while (m_IsRunning)
            {
                try
                {
                    // Bind info has changed, create new local end points.
                    if (m_HasBindingsChanged)
                    {
                        m_pLocalEndPoints.Clear();

                        foreach (IPBindInfo binding in m_pBindings)
                        {
                            if (binding.IP == IPAddress.Any)
                            {
                                foreach (IPAddress ip in Dns.GetHostAddresses(""))
                                {
                                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        IPBindInfo b = new IPBindInfo(binding.HostName,
                                                                      binding.Protocol,
                                                                      ip,
                                                                      25);
                                        if (!m_pLocalEndPoints.Contains(b))
                                        {
                                            m_pLocalEndPoints.Add(b);
                                        }
                                    }
                                }
                            }
                            else if (binding.IP == IPAddress.IPv6Any)
                            {
                                foreach (IPAddress ip in Dns.GetHostAddresses(""))
                                {
                                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                                    {
                                        IPBindInfo b = new IPBindInfo(binding.HostName,
                                                                      binding.Protocol,
                                                                      ip,
                                                                      25);
                                        if (!m_pLocalEndPoints.Contains(b))
                                        {
                                            m_pLocalEndPoints.Add(b);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                IPBindInfo b = new IPBindInfo(binding.HostName,
                                                              binding.Protocol,
                                                              binding.IP,
                                                              25);
                                if (!m_pLocalEndPoints.Contains(b))
                                {
                                    m_pLocalEndPoints.Add(b);
                                }
                            }
                        }

                        m_HasBindingsChanged = false;
                    }

                    // There are no local end points specified.
                    if (m_pLocalEndPoints.Count == 0)
                    {
                        Thread.Sleep(10);
                    }
                        // Maximum allowed relay sessions exceeded, skip adding new ones.
                    else if (m_MaxConnections != 0 && m_pSessions.Count >= m_MaxConnections)
                    {
                        Thread.Sleep(10);
                    }
                    else
                    {
                        Relay_QueueItem item = null;

                        // Get next queued message from highest possible priority queue.
                        foreach (Relay_Queue queue in m_pQueues)
                        {
                            item = queue.DequeueMessage();
                            // There is queued message.
                            if (item != null)
                            {
                                break;
                            }
                            // No messages in this queue, see next lower priority queue.
                        }

                        // There are no messages in any queue.
                        if (item == null)
                        {
                            Thread.Sleep(10);
                        }
                            // Create new session for queued relay item.
                        else
                        {
                            // Get round-robin local end point for that session.
                            // This ensures if multiple network connections, all will be load balanced.
                            IPBindInfo localBindInfo = m_pLocalEndPoints.Next();

                            if (m_RelayMode == Relay_Mode.Dns)
                            {
                                Relay_Session session = new Relay_Session(this, localBindInfo, item);
                                m_pSessions.Add(session);
                                ThreadPool.QueueUserWorkItem(session.Start);
                            }
                            else if (m_RelayMode == Relay_Mode.SmartHost)
                            {
                                // Get smart hosts in balance mode order.
                                Relay_SmartHost[] smartHosts = null;
                                if (m_SmartHostsBalanceMode == BalanceMode.FailOver)
                                {
                                    smartHosts = m_pSmartHosts.ToArray();
                                }
                                else
                                {
                                    smartHosts = m_pSmartHosts.ToCurrentOrderArray();
                                }

                                Relay_Session session = new Relay_Session(this,
                                                                          localBindInfo,
                                                                          item,
                                                                          smartHosts);
                                m_pSessions.Add(session);
                                ThreadPool.QueueUserWorkItem(session.Start);
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    OnError(x);
                }
            }
        }

        /// <summary>
        /// This method checks timed out relay sessions while server is running.
        /// </summary>
        private void Run_CheckTimedOutSessions()
        {
            DateTime lastCheck = DateTime.Now;
            while (IsRunning)
            {
                try
                {
                    // Check interval reached.
                    if (m_SessionIdleTimeout > 0 && lastCheck.AddSeconds(30) < DateTime.Now)
                    {
                        foreach (Relay_Session session in Sessions.ToArray())
                        {
                            try
                            {
                                if (session.LastActivity.AddSeconds(m_SessionIdleTimeout) < DateTime.Now)
                                {
                                    session.Dispose(new Exception("Session idle timeout."));
                                }
                            }
                            catch {}
                        }
                        lastCheck = DateTime.Now;
                    }
                        // Not check interval yet.
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception x)
                {
                    OnError(x);
                }
            }
        }

        #endregion
    }
}