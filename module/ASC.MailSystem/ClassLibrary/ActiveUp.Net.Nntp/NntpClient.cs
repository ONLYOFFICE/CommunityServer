// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using ActiveUp.Net.Security;
using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Mail
{

    #region NntpClient Object version 2
    
    /// <summary>
    /// NNTP Client extending a System.Net.Sockets.TcpClient to send/receive NNTP command/responses.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class NntpClient : System.Net.Sockets.TcpClient
    {

        #region Constructors

        public NntpClient()
        {

        }

        #endregion

        #region Private fields

        bool _postingAllowed = false;
#if !PocketPC
        System.Net.Security.SslStream _sslStream;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// If true, posting is allowed.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// if(!nntp.PostingAllowed) throw new NntpException("Posting not allowed");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com")
        /// If Not nntp.PostingAllowed Then
        /// Throw New NntpException("Posting not allowed");
        /// End If
        /// nntp.Dicsonnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com"); 
        /// if(!nntp.PostingAllowed) throw new NntpException("Posting not allowed");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public bool PostingAllowed
        {
            get
            {
                return this._postingAllowed;
            }
            set
            {
                this._postingAllowed = value;
            }
        }

        #endregion

        #region Events

        #region Event definitions

        /// <summary>
        /// Event fired when a command is being written to the server.
        /// </summary>
        public event ActiveUp.Net.Mail.TcpWritingEventHandler TcpWriting;
        /// <summary>
        /// Event fired when a command has been written to the server.
        /// </summary>
        public event ActiveUp.Net.Mail.TcpWrittenEventHandler TcpWritten;
        /// <summary>
        /// Event fired when a response is being read from the server.
        /// </summary>
        public event ActiveUp.Net.Mail.TcpReadingEventHandler TcpReading;
        /// <summary>
        /// Event fired when a response has been read from the server.
        /// </summary>
        public event ActiveUp.Net.Mail.TcpReadEventHandler TcpRead;
        /// <summary>
        /// Event fired when attempting to connect to the remote server using the specified host.
        /// </summary>
        public event ActiveUp.Net.Mail.ConnectingEventHandler Connecting;
        /// <summary>
        /// Event fired when the object is connected to the remote server or when connection failed.
        /// </summary>
        public new event ActiveUp.Net.Mail.ConnectedEventHandler Connected;
        /// <summary>
        /// Event fired when attempting to disconnect from the remote server.
        /// </summary>
        public event ActiveUp.Net.Mail.DisconnectingEventHandler Disconnecting;
        /// <summary>
        /// Event fired when the object disconnected from the remote server.
        /// </summary>
        public event ActiveUp.Net.Mail.DisconnectedEventHandler Disconnected;
        /// <summary>
        /// Event fired when a message is being requested using the RetrieveMessage() method.
        /// </summary>
        public event ActiveUp.Net.Mail.MessageRetrievingEventHandler MessageRetrieving;
        /// <summary>
        /// Event fired when a message is being retrieved using the RetrieveMessage() method.
        /// </summary>
        public event ActiveUp.Net.Mail.MessageRetrievedEventHandler MessageRetrieved;
        /// <summary>
        /// Event fired when a message Header is being requested using the RetrieveHeader() method.
        /// </summary>
        public event ActiveUp.Net.Mail.HeaderRetrievingEventHandler HeaderRetrieving;
        /// <summary>
        /// Event fired when a message Header has been retrieved using the RetrieveHeader() method.
        /// </summary>
        public event ActiveUp.Net.Mail.HeaderRetrievedEventHandler HeaderRetrieved;
        /// <summary>
        /// Event fired when authentication starts.
        /// </summary>
        public event ActiveUp.Net.Mail.AuthenticatingEventHandler Authenticating;
        /// <summary>
        /// Event fired when authentication completed.
        /// </summary>
        public event ActiveUp.Net.Mail.AuthenticatedEventHandler Authenticated;

        #endregion

        #region Event triggers and logging

        internal void OnTcpWriting(ActiveUp.Net.Mail.TcpWritingEventArgs e)
        {
            if(TcpWriting!=null) TcpWriting(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Sending "+e.Command+"...",1);
        }
        internal void OnTcpWritten(ActiveUp.Net.Mail.TcpWrittenEventArgs e)
        {
            if(TcpWritten!=null) TcpWritten(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Sent "+e.Command+".",1);
        }
        internal void OnTcpReading()
        {
            if(TcpReading!=null) TcpReading(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Reading...",1);
        }
        internal void OnTcpRead(ActiveUp.Net.Mail.TcpReadEventArgs e)
        {
            if(TcpRead!=null) TcpRead(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Read "+e.Response+".",1);
        }
        internal void OnDisconnecting()
        {
            if(Disconnecting!=null) Disconnecting(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Disconnecting...",2);
        }
        internal void OnDisconnected(ActiveUp.Net.Mail.DisconnectedEventArgs e)
        {
            if(Disconnected!=null) Disconnected(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Disconnected.",2);
        }
        internal void OnConnecting()
        {
            if(Connecting!=null) Connecting(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Connecting...",2);
        }
        internal void OnConnected(ActiveUp.Net.Mail.ConnectedEventArgs e)
        {
            if(Connected!=null) Connected(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Connected. Server replied : "+e.ServerResponse+"...",2);
        }
        internal void OnMessageRetrieving(ActiveUp.Net.Mail.MessageRetrievingEventArgs e)
        {
            if(MessageRetrieving!=null) MessageRetrieving(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieving message at index "+e.MessageIndex+" out of "+e.TotalCount+"...",2);
        }
        internal void OnMessageRetrieved(ActiveUp.Net.Mail.MessageRetrievedEventArgs e)
        {
            if(MessageRetrieved!=null) MessageRetrieved(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieved message at index "+e.MessageIndex+" out of "+e.TotalCount+".",2);
        }
        internal void OnHeaderRetrieving(ActiveUp.Net.Mail.HeaderRetrievingEventArgs e)
        {
            if(HeaderRetrieving!=null) HeaderRetrieving(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieving Header at index "+e.MessageIndex+" out of "+e.TotalCount+"...",2);
        }
        internal void OnHeaderRetrieved(ActiveUp.Net.Mail.HeaderRetrievedEventArgs e)
        {
            if(HeaderRetrieved!=null) HeaderRetrieved(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieved Header at index "+e.MessageIndex+" out of "+e.TotalCount+".",2);
        }
        internal void OnAuthenticating(ActiveUp.Net.Mail.AuthenticatingEventArgs e)
        {
            if(Authenticating!=null) Authenticating(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Authenticating as "+e.Username+" on "+e.Host+"...",2);
        }
        internal void OnAuthenticated(ActiveUp.Net.Mail.AuthenticatedEventArgs e)
        {
            if(Authenticated!=null) Authenticated(this,e);
            ActiveUp.Net.Mail.Logger.AddEntry("Authenticated as "+e.Username+" on "+e.Host+".",2);
        }
        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                if (this.Client != null)
                    return this.Client.Connected;
                else
                    return false;
            }
        }

        #endregion

        #endregion

        #region Delegates and associated private fields

        private delegate string DelegateConnect(string host, int port);
        private DelegateConnect _delegateConnect;

        private delegate string DelegateConnectIPAddress(System.Net.IPAddress addr, int port);
        private DelegateConnectIPAddress _delegateConnectIPAddress;

        private delegate string DelegateConnectIPAddresses(System.Net.IPAddress[] addresses, int port);
        private DelegateConnectIPAddresses _delegateConnectIPAddresses;

#if !PocketPC
        private delegate string DelegateConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake);
        private DelegateConnectSsl _delegateConnectSsl;

        private delegate string DelegateConnectSslIPAddress(System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake);
        private DelegateConnectSslIPAddress _delegateConnectSslIPAddress;

        private delegate string DelegateConnectSslIPAddresses(System.Net.IPAddress[] addresses, int port, ActiveUp.Net.Security.SslHandShake sslHandShake);
        private DelegateConnectSslIPAddresses _delegateConnectSslIPAddresses;
#endif
        private delegate string DelegateConnectAuth(string host, int port, string username, string password);
        private DelegateConnectAuth _delegateConnectAuth;

        #endregion

        #region Methods

        #region Private utility methods

        private string write(string command)
        {
            this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs(command));
            base.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(command+"\r\n"),0,command.Length+2);
            this.OnTcpWritten(new ActiveUp.Net.Mail.TcpWrittenEventArgs(command));
            return this.ReadLine();
        }

        private string ReadLine()
        {
            this.OnTcpReading();
            System.IO.StreamReader sr = new System.IO.StreamReader(base.GetStream(),System.Text.Encoding.ASCII);
            string response = sr.ReadLine();
            this.OnTcpRead(new ActiveUp.Net.Mail.TcpReadEventArgs(response));
            if (response.StartsWith("2") || response.StartsWith("3")) return response;
            else throw new ActiveUp.Net.Mail.NntpException("Command failed : " + response);
        }

        private byte[] writeMulti(string command)
        {
            this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs(command));
            base.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(command+"\r\n"),0,command.Length+2);
            this.OnTcpWritten(new ActiveUp.Net.Mail.TcpWrittenEventArgs(command));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StreamReader sr = new System.IO.StreamReader(base.GetStream(),System.Text.Encoding.ASCII);
            string stri = sr.ReadLine();
            string str = "";
            if(stri.StartsWith("2"))
            {
                while(true)
                {
                    str = sr.ReadLine();
                    if(str!=".") 
                    {
                        if(str!="..") sb.Append(str+"\r\n");
                        else sb.Append("."+"\r\n");
                    }
                    else break;
                }
                return System.Text.Encoding.ASCII.GetBytes(stri+"\r\n"+sb.ToString());
            }
            else throw new ActiveUp.Net.Mail.NntpException(command.Split(' ')[0]+" failed : "+stri);
        }

        private string _CramMd5(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username,password));
            byte[] data = System.Convert.FromBase64String(this.Command("authinfo sasl cram-md5").Split(' ')[1].Trim(new char[] {'\r','\n'}));
            string digest = System.Text.Encoding.ASCII.GetString(data,0,data.Length);
            string response = this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username+" "+ActiveUp.Net.Mail.Crypto.HMACMD5Digest(password,digest))));
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username,password,response));
            return response;

        }

#if !PocketPC
        private void DoSslHandShake(ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this._sslStream = new System.Net.Security.SslStream(base.GetStream(), false, sslHandShake.ServerCertificateValidationCallback, sslHandShake.ClientCertificateSelectionCallback);
            this._sslStream.AuthenticateAsClient(sslHandShake.HostName, sslHandShake.ClientCertificates, sslHandShake.SslProtocol, sslHandShake.CheckRevocation);
        }
#endif

        #endregion

        #region Public methods

        #region Connecting, authenticating and disconnecting

        #region Cleartext methods

        /// <summary>
        /// Connects to the specified server.
        /// </summary>
        /// <param name="host">Server address.</param>
        /// <returns>The server's greeting response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com"); 
        /// </code>
        /// </example>
        public string Connect(string host)
        {
            return this.Connect(host,119);
        }

        public IAsyncResult BeginConnect(string host, AsyncCallback callback)
        {
            this._delegateConnect = this.Connect;
            return this._delegateConnect.BeginInvoke(host, 119, callback, this._delegateConnect);
        }

        /// <summary>
        /// Connects to the specified server.
        /// </summary>
        /// <param name="host">Server address.</param>
        /// <param name="port">Server port.</param>
        /// <returns>The server's greeting response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com",8502);
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com",8502)
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com",8502);
        /// </code>
        /// </example>
        public new string Connect(string host, int port)
        {
            this.OnConnecting();
            string response = "Connection failed.";
            try
            {
                base.Connect(host,port);
                response = this.ReadLine();
                if(response.StartsWith("2")) 
                {
                    this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
                    if(response.StartsWith("200")) this.PostingAllowed = true;
                }
                else throw new ActiveUp.Net.Mail.NntpException("Failed to connect : " + response);
            }
            catch(System.Net.Sockets.SocketException)
            {
                this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            }
            return response;
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback callback)
        {
            this._delegateConnect = this.Connect;
            return this._delegateConnect.BeginInvoke(host, port, callback, this._delegateConnect);
        }

        public new string Connect(System.Net.IPAddress addr, int port)
        {
            this.OnConnecting();
            base.Connect(addr, port);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnect(System.Net.IPAddress addr, int port, AsyncCallback callback)
        {
            this._delegateConnectIPAddress = this.Connect;
            return this._delegateConnectIPAddress.BeginInvoke(addr, port, callback, this._delegateConnectIPAddress);
        }


        public new string Connect(System.Net.IPAddress[] addresses, int port)
        {
            this.OnConnecting();
#if !PocketPC
                base.Connect(addresses, port);
#else
            if (addresses.Length > 0)
                base.Connect(addresses[0], port);
#endif
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnect(System.Net.IPAddress[] addresses, int port, AsyncCallback callback)
        {
            this._delegateConnectIPAddresses = this.Connect;
            return this._delegateConnectIPAddresses.BeginInvoke(addresses, port, callback, this._delegateConnectIPAddresses);
        }

        public string Connect(string host, string username, string password)
        {
            return this.Connect(host, 119, username, password);
        }
        public IAsyncResult BeginConnect(string host, string username, string password, AsyncCallback callback)
        {
            return this.BeginConnect(host, 119, username, password, callback);
        }

        public string Connect(string host, int port, string username, string password)
        {
            this.Connect(host, port);
            return this.Authenticate(username, password);
        }
        public IAsyncResult BeginConnect(string host, int port, string username, string password, AsyncCallback callback)
        {
            this._delegateConnectAuth = this.Connect;
            return this._delegateConnectAuth.BeginInvoke(host, port, username, password, callback, this._delegateConnectAuth);
        }

        public new string EndConnect(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        #endregion

        #region SSL methods

#if !PocketPC
        public string ConnectSsl(string host)
        {
            return this.ConnectSsl(host, 563, new ActiveUp.Net.Security.SslHandShake(host));
        }
        public IAsyncResult BeginConnectSsl(string host, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 563, new ActiveUp.Net.Security.SslHandShake(host), callback);
        }
        public string ConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            return this.ConnectSsl(host, 563, sslHandShake);
        }
        public IAsyncResult BeginConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 563, sslHandShake, callback);
        }
        public string ConnectSsl(string host, int port)
        {
            return this.ConnectSsl(host, port, new ActiveUp.Net.Security.SslHandShake(host));
        }
        public IAsyncResult BeginConnectSsl(string host, int port, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, port, new SslHandShake(host), callback);
        }
        public string ConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(host, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSsl = this.ConnectSsl;
            return this._delegateConnectSsl.BeginInvoke(host, port, sslHandShake, callback, null);
        }

        public string ConnectSsl(System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(addr, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnectSsl(System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSslIPAddress = this.ConnectSsl;
            return this._delegateConnectSslIPAddress.BeginInvoke(addr, port, sslHandShake, callback, null);
        }
        
        public string ConnectSsl(System.Net.IPAddress[] addresses, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(addresses, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnectSsl(System.Net.IPAddress[] addresses, int port, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSslIPAddresses = this.ConnectSsl;
            return this._delegateConnectSslIPAddresses.BeginInvoke(addresses, port, sslHandShake, callback, null);
        }

        public string EndConnectSsl(IAsyncResult result)
        {
            return this._delegateConnectSsl.EndInvoke(result);
        }
#endif

        #endregion

        #region Authentication

        /// <summary>
        /// Authenticates the given user.
        /// </summary>
        /// <param name="username">Username to log in.</param>
        /// <param name="password">Password.</param>
        /// <returns>True if authentication succeded.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// nntp.Authenticate("admin","password");
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// nntp.Authenticate("admin","password")
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// nntp.Authenticate("admin","password");
        /// </code>
        /// </example>
        public string Authenticate(string username, string password)
        {
            string response = this.Command("authinfo user "+username);
            if(response.StartsWith("381"))
            {
                response = this.Command("authinfo pass "+password);
                if(response.StartsWith("281")) return response;
            }
            throw new ActiveUp.Net.Mail.NntpException("Authentication failed. Server Response : " + response);
        }

        private delegate string DelegateAuthenticate(string username, string password);
        private DelegateAuthenticate _delegateAuthenticate;

        public IAsyncResult BeginAuthenticate(string username, string password, AsyncCallback callback)
        {
            this._delegateAuthenticate = this.Authenticate;
            return this._delegateAuthenticate.BeginInvoke(username, password, callback, this._delegateAuthenticate);
        }

        /// <summary>
        /// Authenticates the given user and SASL mechanism.
        /// </summary>
        /// <param name="username">Username to log in.</param>
        /// <param name="password">Password.</param>
        /// <param name="mechanism">SASL Mechanism to be used.</param>
        /// <returns>True if authentication succeded.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// nntp.Authenticate("admin","password",SaslMechanism.CramMd5);
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// nntp.Authenticate("admin","password",SaslMechanism.CramMd5)
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// nntp.Authenticate("admin","password",SaslMechanism.CramMd5);
        /// </code>
        /// </example>
        public string Authenticate(string username, string password, SaslMechanism mechanism)
        {
            switch(mechanism)
            {
                case ActiveUp.Net.Mail.SaslMechanism.CramMd5 : return this._CramMd5(username,password);
                case ActiveUp.Net.Mail.SaslMechanism.Login: throw new ActiveUp.Net.Mail.NntpException("LOGIN mechanism cannot be used for NNTP authentication. If your server accepts it, please perform the commands yourself.");
            }
            return "";
        }
        
        private delegate string DelegateAuthenticateSasl(string username, string password, SaslMechanism mechanism);
        private DelegateAuthenticateSasl _delegateAuthenticateSasl;

        public IAsyncResult BeginAuthenticate(string username, string password, SaslMechanism mechanism, AsyncCallback callback)
        {
            this._delegateAuthenticateSasl = this.Authenticate;
            return this._delegateAuthenticateSasl.BeginInvoke(username, password, mechanism, callback, this._delegateAuthenticateSasl);
        }

        public string EndAuthenticate(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        #endregion

        #region Disconnect method

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns>The server's goodbye greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Do some work
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Do some work
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com"); 
        /// //Do some work
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string Disconnect()
        {
            this.OnDisconnecting();
            string response = this.write("quit");
            this.OnDisconnected(new ActiveUp.Net.Mail.DisconnectedEventArgs(response));
            return response;
        }

        private delegate string DelegateDisconnect();
        private DelegateDisconnect _delegateDisconnect;

        public IAsyncResult BeginDisconnect(AsyncCallback callback)
        {
            this._delegateDisconnect = this.Disconnect;
            return this._delegateDisconnect.BeginInvoke(callback, this._delegateDisconnect);
        }

        public string EndDisconnect(IAsyncResult result)
        {
            return this._delegateDisconnect.EndInvoke(result);
        }

        #endregion

        #endregion

        #region Command sending, receiving and stream access

        /// <summary>
        /// Sends the specified command to the server.
        /// </summary>
        /// <param name="command">The command to be sent.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// string response = nntp.Command("XANYCOMMAND anyargument");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim response as String = nntp.Command("XANYCOMMAND anyargument")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var response:string = nntp.Command("XANYCOMMAND anyargument");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string Command(string command)
        {
            return this.write(command);
        }

        private delegate string DelegateCommand(string command);
        private DelegateCommand _delegateCommand;

        public IAsyncResult BeginCommand(string command, AsyncCallback callback)
        {
            this._delegateCommand = this.Command;
            return this._delegateCommand.BeginInvoke(command, callback, this._delegateCommand);
        }

        public string EndCommand(IAsyncResult result)
        {
            return this._delegateCommand.EndInvoke(result);
        }


        /// <summary>
        /// Sends the specified command to the server.
        /// </summary>
        /// <param name="command">The command to be sent.</param>
        /// <returns>A byte array containing the server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// nntp.SelectGroup("mygroup");
        /// byte[] articleData = nntp.CommandMultiline("ARTICLE 1");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// nntp.SelectGroup("mygroup")
        /// Dim articleData as Byte() = nntp.CommandMultiline("ARTICLE 1")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// nntp.SelectGroup("mygroup");
        /// var articleData:byte[] = nntp.CommandMultiline("ARTICLE 1");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] CommandMultiline(string command)
        {
            byte[] buffer = this.writeMulti(command);
            return buffer;
        }

        private delegate byte[] DelegateCommandMultiline(string command);
        private DelegateCommandMultiline _delegateCommandMultiline;

        public IAsyncResult BeginCommandMultiline(string command, AsyncCallback callback)
        {
            this._delegateCommandMultiline = this.CommandMultiline;
            return this._delegateCommandMultiline.BeginInvoke(command, callback, this._delegateCommandMultiline);
        }

        public byte[] EndCommandMultiline(IAsyncResult result)
        {
            return this._delegateCommandMultiline.EndInvoke(result);
        }

        #endregion

        #region Implementation of the NNTP protocol

        /// <summary>
        /// Retrieves the server's newsgroups.
        /// </summary>
        /// <returns>The server's newsgroups.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// NewsGroupCollection groups = nntp.GetNewsGroups();
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim groups as NewsGroupCollection = nntp.GetNewsGroups()
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var groups:NewsGroupCollection = nntp.GetNewsGroups();
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public NewsGroupCollection GetNewsGroups()
        {
            byte[] data = this.CommandMultiline("list");
            string response = System.Text.Encoding.ASCII.GetString(data,0,data.Length);
            NewsGroupCollection groups = new NewsGroupCollection();
            string[] _groups = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=1;i<_groups.Length-1;i++) 
            {
                string[] _splitted = _groups[i].Split(' ');
                groups.Add(new NewsGroup(_splitted[0],System.Convert.ToInt32(_splitted[2]),System.Convert.ToInt32(_splitted[1]),(_splitted[3].ToLower()=="y") ? true : false,this));
            }
            return groups;
        }

        private delegate NewsGroupCollection DelegateGetNewsGroups();
        private DelegateGetNewsGroups _delegateGetNewsGroups;

        public IAsyncResult BeginGetNewsGroups(AsyncCallback callback)
        {
            this._delegateGetNewsGroups = this.GetNewsGroups;
            return this._delegateGetNewsGroups.BeginInvoke(callback, this._delegateGetNewsGroups);
        }

        /// <summary>
        /// Retrieves the server's newsgroups that have been created since the specified date/time.
        /// </summary>
        /// <param name="startDate">The minimum creation date/time.</param>
        /// <returns>The server's newsgroups that have been created since the specified date/time.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Get groups created less than one months ago.
        /// NewsGroupCollection groups = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1));
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Get groups created less than one months ago.
        /// Dim groups as NewsGroupCollection = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1))
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Get groups created less than one months ago.
        /// var groups:NewsGroupCollection = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1));
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public NewsGroupCollection GetNewsGroups(DateTime startDate)
        {
            return this.GetNewsGroups(startDate,false,string.Empty);
        }

        private delegate NewsGroupCollection DelegateGetNewsGroupsDateTime(DateTime startDate);
        private DelegateGetNewsGroupsDateTime _delegateGetNewsGroupsDateTime;

        public IAsyncResult BeginGetNewsGroups(DateTime startDate, AsyncCallback callback)
        {
            this._delegateGetNewsGroupsDateTime = this.GetNewsGroups;
            return this._delegateGetNewsGroupsDateTime.BeginInvoke(startDate, callback, this._delegateGetNewsGroups);
        }

        /// <summary>
        /// Retrieves the server's newsgroups that have been created since the specified date/time.
        /// </summary>
        /// <param name="startDate">The minimum creation date/time.</param>
        /// <param name="gmt">Specifies if startDate is GMT or not.</param>
        /// <returns>The server's newsgroups that have been created since the specified date/time.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Get groups created less than one months ago (GMT time).
        /// NewsGroupCollection groups = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1),true);
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Get groups created less than one months ago (GMT time).
        /// Dim groups as NewsGroupCollection = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1),True)
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Get groups created less than one months ago (GMT time).
        /// var groups:NewsGroupCollection = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1),true);
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public NewsGroupCollection GetNewsGroups(DateTime startDate, bool gmt)
        {
            return this.GetNewsGroups(startDate,gmt,string.Empty);
        }

        private delegate NewsGroupCollection DelegateGetNewsGroupsDateTimeBool(DateTime startDate, bool gmt);
        private DelegateGetNewsGroupsDateTimeBool _delegateGetNewsGroupsDateTimeBool;

        public IAsyncResult BeginGetNewsGroups(DateTime startDate, bool gmt, AsyncCallback callback)
        {
            this._delegateGetNewsGroupsDateTimeBool = this.GetNewsGroups;
            return this._delegateGetNewsGroupsDateTimeBool.BeginInvoke(startDate, gmt, callback, this._delegateGetNewsGroupsDateTimeBool);
        }

        /// <summary>
        /// Retrieves the server's newsgroups that have been created since the specified date/time.
        /// </summary>
        /// <param name="startDate">The minimum creation date/time.</param>
        /// <param name="gmt">Specifies if startDate is GMT or not.</param>
        /// <param name="distribution">Distribution filter of the articles.</param>
        /// <returns>The server's newsgroups that have been created since the specified date/time and that contain articles for the specified distribution.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Get groups created less than one months ago (GMT time) and containing articles to be distributed to "staff".
        /// NewsGroupCollection groups = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1),true,"staff");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Get groups created less than one months ago (GMT time) and containing articles to be distributed to "staff".
        /// Dim groups as NewsGroupCollection = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1),True,"staff")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Get groups created less than one months ago (GMT time) and containing articles to be distributed to "staff".
        /// var groups:NewsGroupCollection = nntp.GetNewsGroups(System.DateTime.Now.AddMonth(-1),true,"staff");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public NewsGroupCollection GetNewsGroups(System.DateTime startDate, bool gmt, string distribution)
        {
            string sdistribution = (distribution.Length>0) ? " "+distribution : "";
            string sgmt = (gmt) ? " GMT" : "";
            byte[] data = this.CommandMultiline("newgroups " + startDate.ToString("yyMMdd hhmmss") + sgmt + sdistribution);
            string response = System.Text.Encoding.ASCII.GetString(data,0,data.Length);
            ActiveUp.Net.Mail.NewsGroupCollection groups = new ActiveUp.Net.Mail.NewsGroupCollection();
            string[] _groups = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=1;i<_groups.Length-1;i++) 
            {
                string[] _splitted = _groups[i].Split(' ');
                groups.Add(new ActiveUp.Net.Mail.NewsGroup(_splitted[0], System.Convert.ToInt32(_splitted[2]), System.Convert.ToInt32(_splitted[1]), (_splitted[3].ToLower() == "y") ? true : false, this));
            }
            return groups;
        }

        private delegate NewsGroupCollection DelegateGetNewsGroupsDateTimeBoolString(DateTime startDate, bool gmt, string distribution);
        private DelegateGetNewsGroupsDateTimeBoolString _delegateGetNewsGroupsDateTimeBoolString;

        public IAsyncResult BeginGetNewsGroups(DateTime startDate, bool gmt, string distribution, AsyncCallback callback)
        {
            this._delegateGetNewsGroupsDateTimeBoolString = this.GetNewsGroups;
            return this._delegateGetNewsGroupsDateTimeBoolString.BeginInvoke(startDate, gmt, distribution, callback, this._delegateGetNewsGroupsDateTimeBoolString);
        }

        public NewsGroupCollection EndGetNewsGroups(IAsyncResult result)
        {
            return (NewsGroupCollection)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Retrieves message-ids from articles that have been added since the specified date/time.
        /// </summary>
        /// <param name="newsGroup">Newsgroup to be checked.</param>
        /// <param name="startDate">Minimum addition date of the articles.</param>
        /// <returns>The article message-ids</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago.
        /// string[] newids = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1));
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago.
        /// Dim newids as String() = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1))
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago.
        /// var newids:string[] = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1));
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string[] GetNewArticleIds(string newsGroup, System.DateTime startDate)
        {
            return this.GetNewArticleIds(newsGroup,startDate,false,string.Empty);
        }

        private delegate string[] DelegateGetNewArticleIdsStringDateTime(string newsgroup, DateTime startDate);
        private DelegateGetNewArticleIdsStringDateTime _delegateGetNewArticleIdsStringDateTime;

        public IAsyncResult BeginGetNewArticleIds(string newsgroup, DateTime startDate, AsyncCallback callback)
        {
            this._delegateGetNewArticleIdsStringDateTime = this.GetNewArticleIds;
            return this._delegateGetNewArticleIdsStringDateTime.BeginInvoke(newsgroup, startDate, callback, this._delegateGetNewArticleIdsStringDateTime);
        }

        /// <summary>
        /// Retrieves message-ids from articles that have been added since the specified date/time.
        /// </summary>
        /// <param name="newsGroup">Newsgroup to be checked.</param>
        /// <param name="startDate">Minimum addition date of the articles.</param>
        /// <param name="gmt">Specifies if startDate is GMT or not.</param>
        /// <returns>The article message-ids</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago (GMT time).
        /// string[] newids = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1),true);
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago (GMT time).
        /// Dim newids as String() = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1),True)
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago (GMT time).
        /// var newids:string[] = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1),true);
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string[] GetNewArticleIds(string newsGroup, System.DateTime startDate, bool gmt)
        {
            return this.GetNewArticleIds(newsGroup,startDate,gmt,string.Empty);
        }

        private delegate string[] DelegateGetNewArticleIdsStringDateTimeBool(string newsgroup, DateTime startDate, bool gmt);
        private DelegateGetNewArticleIdsStringDateTimeBool _delegateGetNewArticleIdsStringDateTimeBool;

        public IAsyncResult BeginGetNewArticleIds(string newsgroup, DateTime startDate, bool gmt, AsyncCallback callback)
        {
            this._delegateGetNewArticleIdsStringDateTimeBool = this.GetNewArticleIds;
            return this._delegateGetNewArticleIdsStringDateTimeBool.BeginInvoke(newsgroup, startDate, gmt, callback, this._delegateGetNewArticleIdsStringDateTimeBool);
        }

        /// <summary>
        /// Retrieves message-ids from articles that have been added since the specified date/time.
        /// </summary>
        /// <param name="newsGroup">Newsgroup to be checked.</param>
        /// <param name="startDate">Minimum addition date of the articles.</param>
        /// <param name="gmt">Specifies if startDate is GMT or not.</param>
        /// <param name="distribution">The distribution filter of the articles.</param>
        /// <returns>The article message-ids</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Gets article ids that belong to the "myhost.info" newsgroup, that have been added less than one month ago (GMT time) and that should be distributed to "staff".
        /// string[] newids = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1),true,"staff");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago (GMT time) and that should be distributed to "staff".
        /// Dim newids as String() = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1),True,"staff")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Gets article ids that belong to the "myhost.info" newsgroup and have been added less than one month ago (GMT time) and that should be distributed to "staff".
        /// var newids:string[] = nntp.GetNewArticleIds("myhost.info",System.DateTime.Now.AddMonth(-1),true,"staff");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string[] GetNewArticleIds(string newsGroup, System.DateTime startDate, bool gmt, string distribution)
        {
            string sdistribution = (distribution.Length>0) ? " "+distribution : "";
            string sgmt = (gmt) ? " GMT" : "";
            byte[] data = this.CommandMultiline("newnews " + newsGroup + " " + startDate.ToString("yyMMdd hhmmss") + sgmt + sdistribution);
            string response = System.Text.Encoding.ASCII.GetString(data,0,data.Length);
            string[] articles = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            string[] farticles = new string[articles.Length-2];
            for(int i=1;i<articles.Length-1;i++) farticles[i-1] = articles[i].TrimStart('<').TrimEnd('>');
            return farticles;
        }

        private delegate string[] DelegateGetNewArticleIdsStringDateTimeBoolString(string newsgroup, DateTime startDate, bool gmt, string distribution);
        private DelegateGetNewArticleIdsStringDateTimeBoolString _delegateGetNewArticleIdsStringDateTimeBoolString;

        public IAsyncResult BeginGetNewArticleIds(string newsgroup, DateTime startDate, bool gmt, string distribution, AsyncCallback callback)
        {
            this._delegateGetNewArticleIdsStringDateTimeBoolString = this.GetNewArticleIds;
            return this._delegateGetNewArticleIdsStringDateTimeBoolString.BeginInvoke(newsgroup, startDate, gmt, distribution, callback, this._delegateGetNewArticleIdsStringDateTimeBoolString);
        }

        public string[] EndGetNewArticleIds(IAsyncResult result)
        {
            return (string[])result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Selects the specified newsgroup.
        /// </summary>
        /// <param name="groupName">The newsgroup to be selected.</param>
        /// <returns>The selected newsgroup.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Selects the "myhost.info" newsgroup as the current newsgroup.
        /// NewsGroup group = nntp.SelectGroup("myhost.info");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// 'Selects the "myhost.info" newsgroup as the current newsgroup.
        /// Dim group as NewsGroup = nntp.SelectGroup("myhost.info")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// //Selects the "myhost.info" newsgroup as the current newsgroup.
        /// var group:NewsGroup = nntp.SelectGroup("myhost.info");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public NewsGroup SelectGroup(string groupName)
        {
            string response = this.Command("group "+groupName);
            string[] parts = response.Split(' ');
            return new NewsGroup(groupName,System.Convert.ToInt32(parts[2]),System.Convert.ToInt32(parts[3]),true,this);
        }

        private delegate NewsGroup DelegateSelectGroup(string groupName);
        private DelegateSelectGroup _delegateSelectGroup;

        public IAsyncResult BeginSelectGroup(string groupName, AsyncCallback callback)
        {
            this._delegateSelectGroup = this.SelectGroup;
            return this._delegateSelectGroup.BeginInvoke(groupName, callback, this._delegateSelectGroup);
        }

        public NewsGroup EndSelectGroup(IAsyncResult result)
        {
            return (NewsGroup)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }
                
        /// <summary>
        /// Retrieves the article with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A byte array containing the article's data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// byte[] articleData = nntp.RetrieveArticle("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim articleData as Byte() = nntp.RetrieveArticle("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var articleData:byte[] = nntp.RetrieveArticle("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveArticle(string messageId)
        {
            return this.CommandMultiline("article <"+messageId.TrimStart('<').TrimEnd('>')+">");
        }

        private delegate byte[] DelegateRetrieveArticle(string messageId);
        private DelegateRetrieveArticle _delegateRetrieveArticle;

        public IAsyncResult BeginRetrieveArticle(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveArticle = this.RetrieveArticle;
            return this._delegateRetrieveArticle.BeginInvoke(messageId, callback, this._delegateRetrieveArticle);
        }

        public byte[] EndRetrieveArticle(IAsyncResult result)
        {
            return this._delegateRetrieveArticle.EndInvoke(result);
        }
        
        /// <summary>
        /// Retrieves the article Header with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A byte array containing the article's header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// byte[] headerData = nntp.RetrieveHeader("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim headerData as Byte() = nntp.RetrieveHeader("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var headerData:byte[] = nntp.RetrieveHeader("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveHeader(string messageId)
        {
            return this.CommandMultiline("head <" + messageId.TrimStart('<').TrimEnd('>') + ">");
        }

        private delegate byte[] DelegateRetrieveHeader(string messageId);
        private DelegateRetrieveHeader _delegateRetrieveHeader;

        public IAsyncResult BeginRetrieveHeader(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveHeader = this.RetrieveHeader;
            return this._delegateRetrieveHeader.BeginInvoke(messageId, callback, this._delegateRetrieveHeader);
        }

        public byte[] EndRetrieveHeader(IAsyncResult result)
        {
            return this._delegateRetrieveHeader.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves the article with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A string containing the article's data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// string articleData = nntp.RetrieveArticleString("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim articleData as String = nntp.RetrieveArticleString("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var articleData:string = nntp.RetrieveArticleString("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string RetrieveArticleString(string messageId)
        {
            byte[] data = this.CommandMultiline("article <" + messageId.TrimStart('<').TrimEnd('>') + ">");
            return System.Text.Encoding.ASCII.GetString(data,0,data.Length);
        }

        private delegate string DelegateRetrieveArticleString(string messageId);
        private DelegateRetrieveArticleString _delegateRetrieveArticleString;

        public IAsyncResult BeginRetrieveArticleString(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveArticleString = this.RetrieveArticleString;
            return this._delegateRetrieveArticleString.BeginInvoke(messageId, callback, this._delegateRetrieveArticleString);
        }

        public string EndRetrieveArticleString(IAsyncResult result)
        {
            return this._delegateRetrieveArticleString.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves the article Header with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A string containing the article's header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// string headerData = nntp.RetrieveHeaderString("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim headerData as String = nntp.RetrieveHeaderString("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var headerData:string = nntp.RetrieveHeaderString("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string RetrieveHeaderString(string messageId)
        {
            byte[] data = this.CommandMultiline("head <" + messageId + ">");
            return System.Text.Encoding.ASCII.GetString(data,0,data.Length);
        }

        private delegate string DelegateRetrieveHeaderString(string messageId);
        private DelegateRetrieveHeaderString _delegateRetrieveHeaderString;

        public IAsyncResult BeginRetrieveHeaderString(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveHeaderString = this.RetrieveHeaderString;
            return this._delegateRetrieveHeaderString.BeginInvoke(messageId, callback, this._delegateRetrieveHeaderString);
        }

        public string EndRetrieveHeaderString(IAsyncResult result)
        {
            return this._delegateRetrieveHeaderString.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves the article with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A Message object representing the article.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// Message article = nntp.RetrieveArticleObject("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim article as Message = nntp.RetrieveArticleObject("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var article:Message = nntp.RetrieveArticleObject("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public Message RetrieveArticleObject(string messageId)
        {
            return ActiveUp.Net.Mail.Parser.ParseMessage(this.CommandMultiline("article <"+messageId.Trim(new char[] {'<','>'})+">"));
        }

        private delegate Message DelegateRetrieveArticleObject(string messageId);
        private DelegateRetrieveArticleObject _delegateRetrieveArticleObject;

        public IAsyncResult BeginRetrieveArticleObject(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveArticleObject = this.RetrieveArticleObject;
            return this._delegateRetrieveArticleObject.BeginInvoke(messageId, callback, this._delegateRetrieveArticleObject);
        }

        public Message EndRetrieveArticleObject(IAsyncResult result)
        {
            return this._delegateRetrieveArticleObject.EndInvoke(result);
        }


        /// <summary>
        /// Retrieves the article Header with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A Header object representing the article's header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// Header Header = nntp.RetrieveHeaderObject("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim Header as Header = nntp.RetrieveHeaderObject("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var header:Header = nntp.RetrieveHeaderObject("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public ActiveUp.Net.Mail.Header RetrieveHeaderObject(string messageId)
        {
            return ActiveUp.Net.Mail.Parser.ParseHeader(this.CommandMultiline("head <"+messageId.Trim(new char[] {'<','>'})+">"));
        }

        private delegate Header DelegateRetrieveHeaderObject(string messageId);
        private DelegateRetrieveHeaderObject _delegateRetrieveHeaderObject;

        public IAsyncResult BeginRetrieveHeaderObject(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveHeaderObject = this.RetrieveHeaderObject;
            return this._delegateRetrieveHeaderObject.BeginInvoke(messageId, callback, this._delegateRetrieveHeaderObject);
        }

        public Header EndRetrieveHeaderObject(IAsyncResult result)
        {
            return this._delegateRetrieveHeaderObject.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves the article body with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A byte array containing the article's body.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// byte[] body = nntp.RetrieveBody("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim body as Byte() = nntp.RetrieveBody("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var body:byte[] = nntp.RetrieveBody("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveBody(string messageId)
        {
            return this.CommandMultiline("body <"+messageId.TrimEnd('>').TrimStart('<')+">");
        }

        private delegate byte[] DelegateRetrieveBody(string messageId);
        private DelegateRetrieveBody _delegateRetrieveBody;

        public IAsyncResult BeginRetrieveBody(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveBody = this.RetrieveBody;
            return this._delegateRetrieveBody.BeginInvoke(messageId, callback, this._delegateRetrieveBody);
        }

        public byte[] EndRetrieveBody(IAsyncResult result)
        {
            return this._delegateRetrieveBody.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves the article body with the specified message-id.
        /// </summary>
        /// <param name="messageId">Message-Id of the article.</param>
        /// <returns>A string containing the article's body.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// string body = nntp.RetrieveBodyString("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// Dim body as String = nntp.RetrieveBodyString("3e061eae$1@news.myhost.com")
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// var body:string = nntp.RetrieveBodyString("3e061eae$1@news.myhost.com");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string RetrieveBodyString(string messageId)
        {
            byte[] data = this.CommandMultiline("body <" + messageId.TrimEnd('>').TrimStart('<') + ">");
            return System.Text.Encoding.ASCII.GetString(data,0,data.Length);
        }

        private delegate string DelegateRetrieveBodyString(string messageId);
        private DelegateRetrieveBodyString _delegateRetrieveBodyString;

        public IAsyncResult BeginRetrieveBodyString(string messageId, AsyncCallback callback)
        {
            this._delegateRetrieveBodyString = this.RetrieveBodyString;
            return this._delegateRetrieveBodyString.BeginInvoke(messageId, callback, this._delegateRetrieveBodyString);
        }

        public string EndRetrieveBodyString(IAsyncResult result)
        {
            return this._delegateRetrieveBodyString.EndInvoke(result);
        }

        /// <summary>
        /// Posts the provided article.
        /// </summary>
        /// <param name="article">The article data as a string.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message article = new Message();
        /// article.NewsGroups = "myhost.info";
        /// article.From = new Address("john.doe@myhost.com","John Doe");
        /// article.Subject = "Test";
        /// article.Body = "Hello this is a test !";
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// if(nntp.PostingAllowed) nntp.Post(article.PostableString);
        /// else throw new NntpException("Posting not allowed. Couldn't post.");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim article as New Message
        /// article.NewsGroups = "myhost.info"
        /// article.From = New Address("john.doe@myhost.com","John Doe")
        /// article.Subject = "Test"
        /// article.Body = "Hello this is a test !"
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// If nntp.PostingAllowed Then
        /// nntp.Post(article.PostableString)
        /// Else
        /// Throw New NntpException("Posting not allowed. Couldn't post.")
        /// End If
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var article:Message = new Message();
        /// article.NewsGroups = "myhost.info";
        /// article.From = new Address("john.doe@myhost.com","John Doe");
        /// article.Subject = "Test";
        /// article.Body = "Hello this is a test !";
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// if(nntp.PostingAllowed) nntp.Post(article.PostableString);
        /// else throw new NntpException("Posting not allowed. Couldn't post.");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string Post(string article)
        {
            string response = this.Command("post");
            string messageid = "";
            if(response.StartsWith("340")) messageid = response.Substring(response.LastIndexOf(' '));
            else throw new ActiveUp.Net.Mail.NntpException("Command POST failed : " + response);
            base.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(article+"\r\n.\r\n"),0,article.Length+5);
            response = this.ReadLine();
            if(response.StartsWith("2")) return response;
            else throw new ActiveUp.Net.Mail.NntpException("POST failed : " + response);
        }
        
        private delegate string DelegatePost(string article);
        private DelegatePost _delegatePost;

        public IAsyncResult BeginPost(string article, AsyncCallback callback)
        {
            this._delegatePost = this.Post;
            return this._delegatePost.BeginInvoke(article, callback, this._delegatePost);
        }

        /// <summary>
        /// Posts the provided article.
        /// </summary>
        /// <param name="article">The article data as a string.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message article = new Message();
        /// article.HeaderFields.Add("NewsGroups","myhost.info");
        /// article.From = new Address("john.doe@myhost.com","John Doe");
        /// article.Subject = "Test";
        /// article.Body = "Hello this is a test !";
        /// NntpClient nttp = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// if(nntp.PostingAllowed) nntp.Post(article);
        /// else throw new NntpException("Posting not allowed. Couldn't post.");
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim article as New Message
        /// article.HeaderFields.Add("NewsGroups","myhost.info")
        /// article.From = New Address("john.doe@myhost.com","John Doe")
        /// article.Subject = "Test"
        /// article.Body = "Hello this is a test !"
        /// Dim nttp as New NntpClient()
        /// nntp.Connect("news.myhost.com") 
        /// If nntp.PostingAllowed Then
        /// nntp.Post(article)
        /// Else
        /// Throw New NntpException("Posting not allowed. Couldn't post.")
        /// End If
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var article:Message = new Message();
        /// article.HeaderFields.Add("NewsGroups","myhost.info");
        /// article.From = new Address("john.doe@myhost.com","John Doe");
        /// article.Subject = "Test";
        /// article.Body = "Hello this is a test !";
        /// var nntp:NntpClient = new NntpClient();
        /// nntp.Connect("news.myhost.com");
        /// if(nntp.PostingAllowed) nntp.Post(article);
        /// else throw new NntpException("Posting not allowed. Couldn't post.");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string Post(Message article)
        {
            return this.Post(article.ToMimeString());
        }

        private delegate string DelegatePostMessage(Message article);
        private DelegatePostMessage _delegatePostMessage;

        public IAsyncResult BeginPost(Message article, AsyncCallback callback)
        {
            this._delegatePostMessage = this.Post;
            return this._delegatePostMessage.BeginInvoke(article, callback, this._delegatePostMessage);
        }
        
        public string EndPost(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        #endregion

        #endregion

        #endregion 
    }
    #endregion
}