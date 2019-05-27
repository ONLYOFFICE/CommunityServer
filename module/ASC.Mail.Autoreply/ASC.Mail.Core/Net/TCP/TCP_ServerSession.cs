/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


namespace ASC.Mail.Net.TCP
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using IO;

    #endregion

    /// <summary>
    /// This class implements generic TCP server session.
    /// </summary>
    public class TCP_ServerSession : TCP_Session
    {
        #region Events

        /// <summary>
        /// This event is raised when session has disconnected and will be disposed soon.
        /// </summary>
        public event EventHandler Disonnected = null;

        /// <summary>
        /// This event is raised when session has disposed.
        /// </summary>
        public event EventHandler Disposed = null;

        /// <summary>
        /// This event is raised when TCP server session has unknown unhandled error.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// This event is raised when session idle(no activity) timeout reached.
        /// </summary>
        public event EventHandler IdleTimeout = null;

        #endregion

        #region Members

        private DateTime m_ConnectTime;
        private string m_ID = "";

        private bool m_IsDisposed;
        private bool m_IsSecure;
        private bool m_IsTerminated;
        private string m_LocalHostName = "";
        private X509Certificate m_pCertificate;
        private IPEndPoint m_pLocalEP;
        private IPEndPoint m_pRemoteEP;
        private object m_pServer;
        private Dictionary<string, object> m_pTags;
        private SmartStream m_pTcpStream;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TCP_ServerSession()
        {
            m_pTags = new Dictionary<string, object>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets session certificate.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public X509Certificate Certificate
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pCertificate;
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
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_ConnectTime;
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
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_ID;
            }
        }

        /// <summary>
        /// Gets if session is connected.
        /// </summary>
        public override bool IsConnected
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if TCP server session is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        /// <summary>
        /// Gets if this session TCP connection is secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool IsSecureConnection
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_IsSecure;
            }
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
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pTcpStream.LastActivity;
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
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pLocalEP;
            }
        }

        /// <summary>
        /// Gets local host name.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string LocalHostName
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_LocalHostName;
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
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pRemoteEP;
            }
        }

        /// <summary>
        /// Gets owner TCP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public object Server
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pServer;
            }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets user data items collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Dictionary<string, object> Tags
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pTags;
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
                    throw new ObjectDisposedException("TCP_ServerSession");
                }

                return m_pTcpStream;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public override void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            if (!m_IsTerminated)
            {
                try
                {
                    Disconnect();
                }
                catch
                {
                    // Skip disconnect errors.
                }
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

            m_pLocalEP = null;
            m_pRemoteEP = null;
            m_pCertificate = null;

            try
            {
                //_socket.Shutdown(SocketShutdown.Both);//Stop coms
                m_pTcpStream.Dispose();
            }
            catch (Exception)
            {
                                
            }
            
            m_pTcpStream = null;
            m_pTags = null;

            // Release events.
            IdleTimeout = null;
            Disonnected = null;
            Disposed = null;
        }

        /// <summary>
        /// Switches session to secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when connection is already secure or when SSL certificate is not specified.</exception>
        public void SwitchToSecure()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_ServerSession");
            }
            if (m_IsSecure)
            {
                throw new InvalidOperationException("Session is already SSL/TLS.");
            }
            if (m_pCertificate == null)
            {
                throw new InvalidOperationException("There is no certificate specified.");
            }

            // FIX ME: if ssl switching fails, it closes source stream or otherwise if ssl successful, source stream leaks.

            SslStream sslStream = new SslStream(m_pTcpStream.SourceStream);
            sslStream.AuthenticateAsServer(m_pCertificate);

            // Close old stream, but leave source stream open.
            m_pTcpStream.IsOwner = false;
            m_pTcpStream.Dispose();

            m_IsSecure = true;
            m_pTcpStream = new SmartStream(sslStream, true);
        }

        /// <summary>
        /// Disconnects session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override void Disconnect()
        {
            Disconnect(null);
        }

        /// <summary>
        /// Disconnects session.
        /// </summary>
        /// <param name="text">Text what is sent to connected host before disconnecting.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public void Disconnect(string text)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("TCP_ServerSession");
            }
            if (m_IsTerminated)
            {
                return;
            }
            m_IsTerminated = true;

            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    m_pTcpStream.Write(text);
                }
                catch (Exception x)
                {
                    OnError(x);
                }
            }

            try
            {
                OnDisonnected();
            }
            catch (Exception x)
            {
                // We never should get exception here, user should handle it.
                OnError(x);
            }

            Dispose();
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// This method is called from TCP server when session should start processing incoming connection.
        /// </summary>
        protected internal virtual void Start() {}

        /// <summary>
        /// This method is called when specified session times out.
        /// </summary>
        /// <remarks>
        /// This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected virtual void OnTimeout() {}

        /// <summary>
        /// Just calls <b>OnTimeout</b> method.
        /// </summary>
        internal virtual void OnTimeoutI()
        {
            OnTimeout();
        }

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        protected virtual void OnError(Exception x)
        {
            if (Error != null)
            {
                Error(this, new Error_EventArgs(x, new StackTrace()));
            }
        }

        #endregion

        #region Internal methods

        private Socket _socket;
        /// <summary>
        /// Initializes session. This method is called from TCP_Server when new session created.
        /// </summary>
        /// <param name="server">Owner TCP server.</param>
        /// <param name="socket">Connected socket.</param>
        /// <param name="hostName">Local host name.</param>
        /// <param name="ssl">Specifies if session should switch to SSL.</param>
        /// <param name="certificate">SSL certificate.</param>
        internal void Init(object server,
                           Socket socket,
                           string hostName,
                           bool ssl,
                           X509Certificate certificate)
        {
            // NOTE: We may not raise any event here !
            _socket = socket;
            m_pServer = server;
            m_LocalHostName = hostName;
            m_ID = Guid.NewGuid().ToString();
            m_ConnectTime = DateTime.Now;
            m_pLocalEP = (IPEndPoint) socket.LocalEndPoint;
            m_pRemoteEP = (IPEndPoint) socket.RemoteEndPoint;
            m_pCertificate = certificate;

            socket.ReceiveBufferSize = Workaround.Definitions.MaxStreamLineLength;
            socket.SendBufferSize = Workaround.Definitions.MaxStreamLineLength;
            socket.ReceiveTimeout = 30000;
            socket.SendTimeout = 10000;//10 sec

            if (ssl)
            {
                SwitchToSecure();
            }
            else
            {
                m_pTcpStream = new SmartStream(new NetworkStream(socket, true), true);
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>IdleTimeout</b> event.
        /// </summary>
        private void OnIdleTimeout()
        {
            if (IdleTimeout != null)
            {
                IdleTimeout(this, new EventArgs());
            }
        }


        /// <summary>
        /// Raises <b>Disonnected</b> event.
        /// </summary>
        private void OnDisonnected()
        {
            if (Disonnected != null)
            {
                Disonnected(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>Disposed</b> event.
        /// </summary>
        private void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, new EventArgs());
            }
        }

        #endregion
    }
}