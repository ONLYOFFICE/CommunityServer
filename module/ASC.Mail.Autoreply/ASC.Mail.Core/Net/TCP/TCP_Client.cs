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
    using System.Diagnostics;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using IO;
    using Log;

    #endregion

    /// <summary>
    /// This class implements generic TCP client.
    /// </summary>
    public class TCP_Client : TCP_Session
    {
        #region Delegates

        /// <summary>
        /// Internal helper method for asynchronous Connect method.
        /// </summary>
        /// <param name="localEP">Local IP end point to use for connect.</param>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        private delegate void BeginConnectEPDelegate(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl);

        /// <summary>
        /// Internal helper method for asynchronous Connect method.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        private delegate void BeginConnectHostDelegate(string host, int port, bool ssl);

        /// <summary>
        /// Internal helper method for asynchronous Disconnect method.
        /// </summary>
        private delegate void DisconnectDelegate();

        #endregion

        #region Members

        private static readonly Regex IpRegexp =
            new Regex(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private DateTime m_ConnectTime;
        private string m_ID = "";
        private bool m_IsConnected;
        private bool m_IsDisposed;
        private bool m_IsSecure;
        private RemoteCertificateValidationCallback m_pCertificateCallback;
        private IPEndPoint m_pLocalEP;
        private Logger m_pLogger;
        private IPEndPoint m_pRemoteEP;
        private SmartStream m_pTcpStream;
        private Socket socket;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override DateTime ConnectTime
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_ConnectTime;
            }
        }

        /// <summary>
        /// Gets session ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override string ID
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_ID;
            }
        }

        /// <summary>
        /// Gets if TCP client is connected.
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (Socket != null)
                {
                    return Socket.Connected;
                }
                return m_IsConnected;
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
        /// Gets if this session TCP connection is secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override bool IsSecureConnection
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_IsSecure;
            }
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override DateTime LastActivity
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_pTcpStream.LastActivity;
            }
        }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override IPEndPoint LocalEndPoint
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_pLocalEP;
            }
        }

        /// <summary>
        /// Gets or sets TCP client logger. Value null means no logging.
        /// </summary>
        public Logger Logger
        {
            get { return m_pLogger; }

            set { m_pLogger = value; }
        }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_pRemoteEP;
            }
        }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override SmartStream TcpStream
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_Client");
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }

                return m_pTcpStream;
            }
        }

        /// <summary>
        /// Gets or stes remote callback which is called when remote server certificate needs to be validated.
        /// Value null means not sepcified.
        /// </summary>
        public RemoteCertificateValidationCallback ValidateCertificateCallback
        {
            get { return m_pCertificateCallback; }

            set { m_pCertificateCallback = value; }
        }

        public Socket Socket
        {
            get { return socket; }
            set { socket = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used. This method is thread-safe.
        /// </summary>
        public override void Dispose()
        {
            lock (this)
            {
                Debug.Print("TCP client disposed");
                if (m_IsDisposed)
                {
                    return;
                }
                try
                {
                    if (IsConnected)
                    {
                        Disconnect();
                    }
                }
                catch {}
                m_IsDisposed = true;
            }
        }

        /// <summary>
        /// Starts connection to the specified host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="callback">Callback to call when the connect operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous connection.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback callback, object state)
        {
            return BeginConnect(host, port, false, callback, state);
        }

        /// <summary>
        /// Starts connection to the specified host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <param name="callback">Callback to call when the connect operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous connection.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IAsyncResult BeginConnect(string host, int port, bool ssl, AsyncCallback callback, object state)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsConnected)
            {
                throw new InvalidOperationException("TCP client is already connected.");
            }
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Argument 'host' value may not be null or empty.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value must be >= 1.");
            }

            BeginConnectHostDelegate asyncMethod = Connect;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(host,
                                                              port,
                                                              ssl,
                                                              asyncState.CompletedCallback,
                                                              null));

            return asyncState;
        }

        /// <summary>
        /// Starts connection to the specified remote end point.
        /// </summary>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <param name="callback">Callback to call when the connect operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous connection.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IAsyncResult BeginConnect(IPEndPoint remoteEP, bool ssl, AsyncCallback callback, object state)
        {
            return BeginConnect(null, remoteEP, ssl, callback, state);
        }

        /// <summary>
        /// Starts connection to the specified remote end point.
        /// </summary>
        /// <param name="localEP">Local IP end point to use for connect.</param>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <param name="callback">Callback to call when the connect operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous connection.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IAsyncResult BeginConnect(IPEndPoint localEP,
                                         IPEndPoint remoteEP,
                                         bool ssl,
                                         AsyncCallback callback,
                                         object state)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsConnected)
            {
                throw new InvalidOperationException("TCP client is already connected.");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            BeginConnectEPDelegate asyncMethod = Connect;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(localEP,
                                                              remoteEP,
                                                              ssl,
                                                              asyncState.CompletedCallback,
                                                              null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous connection request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when argument <b>asyncResult</b> was not returned by a call to the <b>BeginConnect</b> method.</exception>
        /// <exception cref="InvalidOperationException">Is raised when <b>EndConnect</b> was previously called for the asynchronous connection.</exception>
        public void EndConnect(IAsyncResult asyncResult)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginConnect method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "EndConnect was previously called for the asynchronous operation.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is BeginConnectHostDelegate)
            {
                ((BeginConnectHostDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(
                    castedAsyncResult.AsyncResult);
            }
            else if (castedAsyncResult.AsyncDelegate is BeginConnectEPDelegate)
            {
                ((BeginConnectEPDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(
                    castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginConnect method.");
            }
        }

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(string host, int port)
        {
            Connect(host, port, false);
        }

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(string host, int port, bool ssl)
        {
            Connect(host, port, ssl, 30000);
        }

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(string host, int port, bool ssl, int timeout)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_Client");
            }
            if (IsConnected)
            {
                throw new InvalidOperationException("TCP client is already connected.");
            }
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Argument 'host' value may not be null or empty.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value must be >= 1.");
            }
            //Check ip

            IPAddress[] ips = Dns.GetHostAddresses(host);

            for (int i = 0; i < ips.Length; i++)
            {
                try
                {
                    Connect(null, new IPEndPoint(ips[i], port), ssl, timeout);
                    break;
                }
                catch (Exception x)
                {
                    if (IsConnected)
                    {
                        throw x;
                    }
                        // Connect failed for specified IP address, if there are some more IPs left, try next, otherwise forward exception.
                    else if (i == (ips.Length - 1))
                    {
                        throw x;
                    }
                }
            }
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(IPEndPoint remoteEP, bool ssl)
        {
            Connect(null, remoteEP, ssl);
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="localEP">Local IP end point to use for connet.</param>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl)
        {
            Connect(localEP, remoteEP, ssl, 30000);
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="localEP">Local IP end point to use for connet.</param>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl, int timeout)
        {
            Debug.Print("TCP client connect");
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_Client");
            }
            if (IsConnected)
            {
                throw new InvalidOperationException("TCP client is already connected.");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            if (Socket != null && Socket.Connected)
            {
                Socket.Disconnect(false);
                Socket.Shutdown(SocketShutdown.Both);
            }

            Socket = null;
            if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else if (remoteEP.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                throw new ArgumentException("Remote end point has invalid AddressFamily.");
            }

            try
            {
                Socket.SendTimeout = timeout;
                Socket.ReceiveTimeout = timeout;

                if (localEP != null)
                {
                    Socket.Bind(localEP);
                }

                LogAddText("Connecting to " + remoteEP + ".");
                Socket.Connect(remoteEP);

                m_IsConnected = true;
                m_ID = Guid.NewGuid().ToString();
                m_ConnectTime = DateTime.Now;
                m_pLocalEP = (IPEndPoint) Socket.LocalEndPoint;
                m_pRemoteEP = (IPEndPoint) Socket.RemoteEndPoint;
                m_pTcpStream = new SmartStream(new NetworkStream(Socket, true), true);

                LogAddText("Connected, localEP='" + m_pLocalEP + "'; remoteEP='" + remoteEP + "'.");

                if (ssl)
                {
                    SwitchToSecure();
                }
            }
            catch (Exception x)
            {
                LogAddException("Exception: " + x.Message, x);

                // Switching to secure failed.
                if (IsConnected)
                {
                    Disconnect();
                }
                    // Bind or connect failed.
                else
                {
                    Socket.Close();
                }

                OnError(x);

                throw x;
            }

            OnConnected();
        }

        /// <summary>
        /// Disconnects connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override void Disconnect()
        {
            Debug.Print("TCP client disconect");

            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_Client");
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("TCP client is not connected.");
            }

            try
            {
                m_pLocalEP = null;
                m_pRemoteEP = null;
                m_pTcpStream.Dispose();
                LogAddText("Socket disconnecting");
                m_IsSecure = false;

            }
            catch (Exception)
            {
                
            }
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {

                Socket.EndDisconnect(ar);
                Socket.Close();
                m_IsConnected = false;
                LogAddText("Disconnected.");
                
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Starts disconnecting connection.
        /// </summary>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous disconnect.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("TCP client is not connected.");
            }

            DisconnectDelegate asyncMethod = Disconnect;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous disconnect request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when argument <b>asyncResult</b> was not returned by a call to the <b>BeginDisconnect</b> method.</exception>
        /// <exception cref="InvalidOperationException">Is raised when <b>EndDisconnect</b> was previously called for the asynchronous connection.</exception>
        public void EndDisconnect(IAsyncResult asyncResult)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginDisconnect method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "EndDisconnect was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is DisconnectDelegate)
            {
                ((DisconnectDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginDisconnect method.");
            }
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// This method is called after TCP client has sucessfully connected.
        /// </summary>
        protected virtual void OnConnected() {}

        #endregion

        #region Utility methods

        private bool RemoteCertificateValidationCallback(object sender,
                                                         X509Certificate certificate,
                                                         X509Chain chain,
                                                         SslPolicyErrors sslPolicyErrors)
        {
            // User will handle it.
            if (m_pCertificateCallback != null)
            {
                return m_pCertificateCallback(sender, certificate, chain, sslPolicyErrors);
            }
            else
            {
                if (sslPolicyErrors == SslPolicyErrors.None ||
                    sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
                {
                    return true;
                }

                // Do not allow this client to communicate with unauthenticated servers.
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Switches session to secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected or is already secure.</exception>
        protected void SwitchToSecure()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_Client");
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("TCP client is not connected.");
            }
            if (m_IsSecure)
            {
                throw new InvalidOperationException("TCP client is already secure.");
            }

            LogAddText("Switching to SSL.");

            // FIX ME: if ssl switching fails, it closes source stream or otherwise if ssl successful, source stream leaks.

            SslStream sslStream = new SslStream(m_pTcpStream.SourceStream,
                                                true,
                                                RemoteCertificateValidationCallback);
            sslStream.AuthenticateAsClient("");

            // Close old stream, but leave source stream open.
            m_pTcpStream.IsOwner = false;
            m_pTcpStream.Dispose();

            m_IsSecure = true;
            m_pTcpStream = new SmartStream(sslStream, true);
        }

        // Do we need it ?

        /// <summary>
        /// This must be called when unexpected error happens. When inheriting <b>TCP_Client</b> class, be sure that you call <b>OnError</b>
        /// method for each unexpected error.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        protected void OnError(Exception x)
        {
            try
            {
                if (m_pLogger != null)
                {
                    //m_pLogger.AddException(x);
                }
            }
            catch {}
        }

        /// <summary>
        /// Reads and logs specified line from connected host.
        /// </summary>
        /// <returns>Returns readed line.</returns>
        protected string ReadLine()
        {
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            string line = args.LineUtf8;
            if (args.BytesInBuffer > 0)
            {
                LogAddRead(args.BytesInBuffer, line);
            }
            else
            {
                LogAddText("Remote host closed connection.");
            }

            return line;
        }

        /// <summary>
        /// Sends and logs specified line to connected host.
        /// </summary>
        /// <param name="line">Line to send.</param>
        protected void WriteLine(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);
        }

        /// <summary>
        /// Logs read operation.
        /// </summary>
        /// <param name="size">Number of bytes readed.</param>
        /// <param name="text">Log text.</param>
        protected void LogAddRead(long size, string text)
        {
            try
            {
                if (m_pLogger != null)
                {
                    m_pLogger.AddRead(ID, AuthenticatedUserIdentity, size, text, LocalEndPoint, RemoteEndPoint);
                }
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        /// <summary>
        /// Logs write operation.
        /// </summary>
        /// <param name="size">Number of bytes written.</param>
        /// <param name="text">Log text.</param>
        protected void LogAddWrite(long size, string text)
        {
            try
            {
                if (m_pLogger != null)
                {
                    m_pLogger.AddWrite(ID,
                                       AuthenticatedUserIdentity,
                                       size,
                                       text,
                                       LocalEndPoint,
                                       RemoteEndPoint);
                }
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        /// <summary>
        /// Logs free text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        protected void LogAddText(string text)
        {
            try
            {
                if (m_pLogger != null)
                {
                    m_pLogger.AddText(IsConnected ? ID : "",
                                      IsConnected ? AuthenticatedUserIdentity : null,
                                      text,
                                      IsConnected ? LocalEndPoint : null,
                                      IsConnected ? RemoteEndPoint : null);
                }
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="x">Exception happened.</param>
        protected void LogAddException(string text, Exception x)
        {
            try
            {
                if (m_pLogger != null)
                {
                    m_pLogger.AddException(IsConnected ? ID : "",
                                           IsConnected ? AuthenticatedUserIdentity : null,
                                           text,
                                           IsConnected ? LocalEndPoint : null,
                                           IsConnected ? RemoteEndPoint : null,
                                           x);
                }
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }
    }
}