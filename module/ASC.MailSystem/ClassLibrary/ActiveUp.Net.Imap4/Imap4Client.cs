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
using ActiveUp.Net.Mail;
using ActiveUp.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ActiveUp.Net.Mail
{
    #region Imap4Client Object version 2

    /// <summary>
    /// This class allows communication with an IMAP4 or IMAP4rev1 compatible server.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Imap4Client : ActiveUp.Net.Common.BaseProtocolClient
    {

        #region Constructors

        public Imap4Client()
        {

        }

        static Imap4Client()
        {
            _badCommandStrings = new[] {
                "\\", //Important this comes first
                "\""              
            };
        }

        #endregion

        #region Private fields

        static string[] _badCommandStrings;

        private string host, _capabilities;
//#if !PocketPC
//        System.Net.Security.SslStream _sslStream;
//#endif
        private bool _idleInProgress = false;

        #endregion

        #region Properties

        /// <summary>
        /// Server capabilities.
        /// </summary>
        public string ServerCapabilities
        {
            get
            {
                return this._capabilities;
            }
            set
            {
                this._capabilities = value;
            }
        }

        /// <summary>
        /// Turn this on to not make any parameters safe.  Injection attacks more likely.  Turn this on only if you are already doing checking or if performance is absolutely critical.  
        /// </summary>
        public bool IsUnsafeParamsAllowed { get; set; }

        #endregion

        #region Delegates and associated private fields

        private delegate string DelegateConnect(string host, int port);
        private DelegateConnect _delegateConnect;

        private delegate string DelegateConnectAuth(string host, int port, string username, string password);
        private DelegateConnectAuth _delegateConnectAuth;

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

        private delegate string DelegateDisconnect();
        private DelegateDisconnect _delegateDisconnect;

        private delegate string DelegateAuthenticate(string username, string password, SaslMechanism mechanism);
        private DelegateAuthenticate _delegateAuthenticate;

        private delegate string DelegateLogin(string username, string password, string host);
        private DelegateLogin _delegateLogin;

        private delegate string DelegateCommand(string command, string stamp, CommandOptions options);
        private DelegateCommand _delegateCommand;

        private delegate string DelegateCommandStringStringString(string command, string stamp, string checkStamp, CommandOptions options);
        private DelegateCommandStringStringString _delegateCommandStringStringString;

        private delegate string DelegateNoop();
        private DelegateNoop _delegateNoop;

        private delegate string DelegateCheck();
        private DelegateCheck _delegateCheck;

        private delegate string DelegateClose();
        private DelegateClose _delegateClose;

        private delegate void DelegateExpunge();
        private DelegateExpunge _delegateExpunge;

        private delegate Mailbox DelegateMailboxOperation(string mailboxName);
        private DelegateMailboxOperation _delegateMailboxOperation;

        private delegate string DelegateRenameMailbox(string oldMailboxName, string newMailboxName);
        private DelegateRenameMailbox _delegateRenameMailbox;

        private delegate string DelegateMailboxOperationReturnsString(string mailboxName);
        private DelegateMailboxOperationReturnsString _delegateMailboxOperationReturnsString;

        private delegate MailboxCollection DelegateGetMailboxes(string reference, string mailboxName);
        private DelegateGetMailboxes _delegateGetMailboxes;

        #endregion

        #region Events

        #region Event definitions

        /// <summary>
        /// Event fired when authentication starts.
        /// </summary>
        public event ActiveUp.Net.Mail.AuthenticatingEventHandler Authenticating;
        /// <summary>
        /// Event fired when authentication completed.
        /// </summary>
        public event ActiveUp.Net.Mail.AuthenticatedEventHandler Authenticated;
        /// <summary>
        /// Event fired when NOOP command is issued.
        /// </summary>
        public event ActiveUp.Net.Mail.NoopingEventHandler Nooping;
        /// <summary>
        /// Event fired when NOOP command completed.
        /// </summary>
        public event ActiveUp.Net.Mail.NoopedEventHandler Nooped;
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
        /// Event fired when a message is being sent.
        /// </summary>
        public event ActiveUp.Net.Mail.MessageSendingEventHandler MessageSending;
        /// <summary>
        /// Event fired when a message has been sent.
        /// </summary>
        public event ActiveUp.Net.Mail.MessageSentEventHandler MessageSent;
        /// <summary>
        /// Event fired when a new message received.
        /// </summary>
        public event ActiveUp.Net.Mail.NewMessageReceivedEventHandler NewMessageReceived;

        #endregion

        #region Event triggers and logging

        internal void OnAuthenticating(ActiveUp.Net.Mail.AuthenticatingEventArgs e)
        {
            if (Authenticating != null) Authenticating(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Authenticating as " + e.Username + " on " + e.Host + "...", 2);
        }
        internal void OnAuthenticated(ActiveUp.Net.Mail.AuthenticatedEventArgs e)
        {
            if (Authenticated != null) Authenticated(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Authenticated as " + e.Username + " on " + e.Host + ".", 2);
        }
        internal void OnNooping()
        {
            if (Nooping != null) Nooping(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Nooping...", 1);
        }
        internal void OnNooped()
        {
            if (Nooped != null) Nooped(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Nooped.", 1);
        }
        internal void OnTcpWriting(ActiveUp.Net.Mail.TcpWritingEventArgs e)
        {
            if (TcpWriting != null) TcpWriting(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Sending " + e.Command + "...", 1);
        }
        internal void OnTcpWritten(ActiveUp.Net.Mail.TcpWrittenEventArgs e)
        {
            if (TcpWritten != null) TcpWritten(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Sent " + e.Command + ".", 1);
        }
        internal void OnTcpReading()
        {
            if (TcpReading != null) TcpReading(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Reading...", 1);
        }
        internal void OnTcpRead(ActiveUp.Net.Mail.TcpReadEventArgs e)
        {
            if (TcpRead != null) TcpRead(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Read " + e.Response + ".", 1);
        }
        internal void OnMessageRetrieving(ActiveUp.Net.Mail.MessageRetrievingEventArgs e)
        {
            if (MessageRetrieving != null) MessageRetrieving(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieving message at index " + e.MessageIndex + "...", 2);
        }
        internal void OnMessageRetrieved(ActiveUp.Net.Mail.MessageRetrievedEventArgs e)
        {
            if (MessageRetrieved != null) MessageRetrieved(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieved message at index " + e.MessageIndex + ".", 2);
        }
        internal void OnHeaderRetrieving(ActiveUp.Net.Mail.HeaderRetrievingEventArgs e)
        {
            if (HeaderRetrieving != null) HeaderRetrieving(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieving Header at index " + e.MessageIndex + "...", 2);
        }
        internal void OnHeaderRetrieved(ActiveUp.Net.Mail.HeaderRetrievedEventArgs e)
        {
            if (HeaderRetrieved != null) HeaderRetrieved(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Retrieved Header at index " + e.MessageIndex + ".", 2);
        }
        internal void OnDisconnecting()
        {
            if (Disconnecting != null) Disconnecting(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Disconnecting...", 2);
        }
        internal void OnDisconnected(ActiveUp.Net.Mail.DisconnectedEventArgs e)
        {
            if (Disconnected != null) Disconnected(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Disconnected.", 2);
        }
        internal void OnConnecting()
        {
            if (Connecting != null) Connecting(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Connecting...", 2);
        }
        internal void OnConnected(ActiveUp.Net.Mail.ConnectedEventArgs e)
        {
            if (Connected != null) Connected(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Connected. Server replied : " + e.ServerResponse + ".", 2);
        }
        internal void OnMessageSending(ActiveUp.Net.Mail.MessageSendingEventArgs e)
        {
            if (MessageSending != null) MessageSending(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Sending message with subject : " + e.Message.Subject + "...", 2);
        }
        internal void OnMessageSent(ActiveUp.Net.Mail.MessageSentEventArgs e)
        {
            if (MessageSent != null) MessageSent(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("Sent message with subject : " + e.Message.Subject + "...", 2);
        }
        internal void OnNewMessageReceived(ActiveUp.Net.Mail.NewMessageReceivedEventArgs e)
        {
            if (NewMessageReceived != null) NewMessageReceived(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry("New message received : " + e.MessageCount + "...", 2);
        }

        #endregion

        #endregion

        #region Methods

        #region Private utility methods

        private string _CramMd5(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password));
            string stamp = System.DateTime.Now.ToString("yyMMddhhmmss" + System.DateTime.Now.Millisecond.ToString());
            byte[] data = System.Convert.FromBase64String(this.Command(stamp + " authenticate cram-md5", stamp).Split(' ')[1].Trim(new char[] { '\r', '\n' }));
            string digest = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);
            string response = this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + " " + ActiveUp.Net.Mail.Crypto.HMACMD5Digest(password, digest))), stamp);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, response));
            return response;
        }

        private string _Login(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password));
            string stamp = System.DateTime.Now.ToString("yyMMddhhmmss" + System.DateTime.Now.Millisecond.ToString());
            this.Command("authenticate login"); ;
            this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username)), stamp);
            string response = this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(password)), stamp);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, response));
            return response;
        }

        private static string FindLine(string[] input, string pattern)
        {
            foreach (string str in input) if (str.IndexOf(pattern) != -1) return str;
            return "";
        }
#if !PocketPC
        protected override void DoSslHandShake(ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this._sslStream = new System.Net.Security.SslStream(base.GetStream(), false, sslHandShake.ServerCertificateValidationCallback, sslHandShake.ClientCertificateSelectionCallback);
            bool authenticationFailed = false;
            try
            {
                this._sslStream.AuthenticateAsClient(sslHandShake.HostName, sslHandShake.ClientCertificates, sslHandShake.SslProtocol, sslHandShake.CheckRevocation);
            }
            catch (Exception)
            {
                authenticationFailed = true;
            }

            if (authenticationFailed)
            {
                //System.Net.ServicePointManager.CertificatePolicy' is obsolete: 'CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead.
                //System.Net.ServicePointManager.CertificatePolicy = new ActiveUp.Net.Security.TrustAllCertificatePolicy();

                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CertValidation);

                this._sslStream.AuthenticateAsClient(sslHandShake.HostName, sslHandShake.ClientCertificates, sslHandShake.SslProtocol, sslHandShake.CheckRevocation);
            }

        }

        bool CertValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyErrors)
        { // Trust all certificate policy
            return true;
        }
#endif
        private string ReadLine()
        {
            this.OnTcpReading();
            System.IO.StreamReader sr = new System.IO.StreamReader(this.GetStream(), true);
            string response = sr.ReadLine();
            this.OnTcpRead(new ActiveUp.Net.Mail.TcpReadEventArgs(response));
            return response;
        }

        /// <summary>
        /// Takes a command parameter and makes it safe for IMAP
        /// </summary>
        /// <param name="commandParam"></param>
        /// <returns></returns>
        private string renderSafeParam(string commandParam)
        {
            if (this.IsUnsafeParamsAllowed)
                return commandParam;

            var sb = new StringBuilder(commandParam);
            foreach (var badString in _badCommandStrings)
                sb.Replace(badString, "\\" + badString);

            return sb.ToString();
        }

        #endregion

        #region Public methods

        #region Connecting, authenticating and disconnecting

        #region Cleartext methods

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="host">Server address.</param>
        /// <returns>The server's response greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// ...
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// ...
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// ...
        /// </code>
        /// </example>
        public string Connect(string host)
        {
            return this.Connect(host, 143);
        }
        /// <summary>
        /// Begins the connect.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(string host, AsyncCallback callback)
        {
            this._delegateConnect = this.Connect;
            return this._delegateConnect.BeginInvoke(host, 143, callback, this._delegateConnect);
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="host">Server address.</param>
        /// <param name="port">Server port.</param>
        /// <returns>The server's response greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com",8505);
        /// ...
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com",8505)
        /// ...
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com",8505);
        /// ...
        /// </code>
        /// </example>
        public new string Connect(string host, int port)
        {
            this.host = host;
            this.OnConnecting();
            base.Connect(host, port);
            string response = this.ReadLine();
            this.ServerCapabilities = this.Command("capability");
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public string ConnectTLS(string host, int port)
        {
            this.Connect(host, port);
            //this.SendEhloHelo();
            return this.StartTLS(host);
        }
        /// <summary>
        /// Begins the connect.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public override IAsyncResult BeginConnect(string host, int port, AsyncCallback callback)
        {
            this._delegateConnect = this.Connect;
            return this._delegateConnect.BeginInvoke(host, port, callback, this._delegateConnect);
        }
        public IAsyncResult BeginConnectTLS(string host, int port, AsyncCallback callback)
        {
            this._delegateConnect = this.ConnectTLS;
            return this._delegateConnect.BeginInvoke(host, port, callback, this._delegateConnect);
        }

        /// <summary>
        /// Connects the specified addr.
        /// </summary>
        /// <param name="addr">The addr.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public new string Connect(System.Net.IPAddress addr, int port)
        {
            this.OnConnecting();
            base.Connect(addr, port);
            string response = this.ReadLine();
            this.ServerCapabilities = this.Command("capability");
            this.OnConnected(new ConnectedEventArgs(response));
            return response;
        }
        /// <summary>
        /// Begins the connect.
        /// </summary>
        /// <param name="addr">The addr.</param>
        /// <param name="port">The port.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(System.Net.IPAddress addr, int port, AsyncCallback callback)
        {
            this._delegateConnectIPAddress = this.Connect;
            return this._delegateConnectIPAddress.BeginInvoke(addr, port, callback, this._delegateConnectIPAddress);
        }


        /// <summary>
        /// Connects the specified addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public new string Connect(System.Net.IPAddress[] addresses, int port)
        {
            this.OnConnecting();
#if !PocketPC
            base.Connect(addresses, port);
#else
                if(addresses.Length>0)
                    base.Connect(addresses[0], port);
#endif
            string response = this.ReadLine();
            this.ServerCapabilities = this.Command("capability");
            this.OnConnected(new ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnect(System.Net.IPAddress[] addresses, int port, AsyncCallback callback)
        {
            this._delegateConnectIPAddresses = this.Connect;
            return this._delegateConnectIPAddresses.BeginInvoke(addresses, port, callback, this._delegateConnectIPAddresses);
        }

        public string Connect(string host, string username, string password)
        {
            return this.Connect(host, 143, username, password);
        }
        /// <summary>
        /// Begins the connect.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(string host, string username, string password, AsyncCallback callback)
        {
            return this.BeginConnect(host, 143, username, password, callback);
        }

        /// <summary>
        /// Connects the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public string Connect(string host, int port, string username, string password)
        {
            this.Connect(host, port);
            return this.LoginFast(username, password, host);
        }
        /// <summary>
        /// Begins the connect.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(string host, int port, string username, string password, AsyncCallback callback)
        {
            this._delegateConnectAuth = this.Connect;
            return this._delegateConnectAuth.BeginInvoke(host, port, username, password, callback, this._delegateConnectAuth);
        }

        /// <summary>
        /// Ends the connect.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public new string EndConnect(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected
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

        #region SSL methods
#if !PocketPC
        public string ConnectSsl(string host)
        {
            return this.ConnectSsl(host, 993, new ActiveUp.Net.Security.SslHandShake(host));
        }
        public IAsyncResult BeginConnectSsl(string host, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 993, new ActiveUp.Net.Security.SslHandShake(host), callback);
        }
        public string ConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            return this.ConnectSsl(host, 993, sslHandShake);
        }
        public IAsyncResult BeginConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 993, sslHandShake, callback);
        }
        public string ConnectSsl(string host, int port)
        {
            return this.ConnectSsl(host, port, new ActiveUp.Net.Security.SslHandShake(host));
        }
        public override IAsyncResult BeginConnectSsl(string host, int port, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, port, new SslHandShake(host), callback);
        }
#endif
#if !PocketPC
        public string ConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(host, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.ServerCapabilities = this.Command("capability");
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSsl = this.ConnectSsl;
            return this._delegateConnectSsl.BeginInvoke(host, port, sslHandShake, callback, this._delegateConnectSsl);
        }

        public string ConnectSsl(System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(addr, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.ServerCapabilities = this.Command("capability");
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnectSsl(System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSslIPAddress = this.ConnectSsl;
            return this._delegateConnectSslIPAddress.BeginInvoke(addr, port, sslHandShake, callback, this._delegateConnectSslIPAddress);
        }

        public string ConnectSsl(System.Net.IPAddress[] addresses, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(addresses, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.ServerCapabilities = this.Command("capability");
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }
        public IAsyncResult BeginConnectSsl(System.Net.IPAddress[] addresses, int port, ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSslIPAddresses = this.ConnectSsl;
            return this._delegateConnectSslIPAddresses.BeginInvoke(addresses, port, sslHandShake, callback, this._delegateConnectSslIPAddresses);
        }

        public override string EndConnectSsl(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }
#endif
        #endregion

        #region Disconnect method

        /// <summary>
        /// Logs out and closes the connection with the server.
        /// </summary>
        /// <returns>The server's googbye greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// //Do some work...
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// 'Do some work...
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// //Do some work...
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public override string Disconnect()
        {
            string greeting = this.Command("logout");
            base.Close();
            return greeting;
        }
        public IAsyncResult BeginDisconnect(AsyncCallback callback)
        {
            this._delegateDisconnect = this.Disconnect;
            return this._delegateDisconnect.BeginInvoke(callback, null);
        }

        public string EndDisconnect(IAsyncResult result)
        {
            return this._delegateDisconnect.EndInvoke(result);
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Logs in to the specified account.
        /// </summary>
        /// <param name="username">Username of the account.</param>
        /// <param name="password">Password of the account.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// //Do some work...
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// 'Do some work...
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// //Do some work...
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public override string Login(string username, string password, string _host)//Todo: remove reundant parameter
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password, this.host));
            string response = this.Command("login " + username + " " + password);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, this.host, response));
            return response;
        }
        public string LoginOAuth2(string username, string accessToken)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, accessToken, this.host));
            string formatResponse = "user=" + username + "\u0001auth=Bearer " + accessToken + "\u0001\u0001";
            string authResponse = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(formatResponse));
            string response = this.Command("AUTHENTICATE XOAUTH2 " + authResponse);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, accessToken, this.host, response));
            return response;
        }
        public IAsyncResult BeginLogin(string username, string password, AsyncCallback callback)
        {
            this._delegateLogin = this.Login;
            return this._delegateLogin.BeginInvoke(username, password, this.host, callback, this._delegateLogin);
        }


        /// <summary>
        /// Same as Login but doesn't load the AllMailboxes and Mailboxes properties of the Imap4Client object, ensuring faster operation.
        /// </summary>
        /// <param name="username">Username of the account.</param>
        /// <param name="password">Password of the account.</param>
        /// <returns>The server's response.</returns>
        public string LoginFast(string username, string password, string host)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password, this.host));
            string response = this.Command("login " + username + " " + password);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, this.host, response));
            return response;
        }
        public IAsyncResult BeginLoginFast(string username, string password, AsyncCallback callback)
        {
            this._delegateLogin = this.LoginFast;
            return this._delegateLogin.BeginInvoke(username, password, this.host, callback, this._delegateLogin);
        }

        /// <summary>
        /// Authenticates using the given SASL mechanism.
        /// </summary>
        /// <param name="username">Username to authenticate as.</param>
        /// <param name="password">Password.</param>
        /// <param name="mechanism">SASL mechanism to be used.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Authenticate("user","pass",SASLMechanism.CramMd5);
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Authenticate("user","pass",SASLMechanism.CramMd5)
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Authenticate("user","pass",SASLMechanism.CramMd5);
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Authenticate(string username, string password, ActiveUp.Net.Mail.SaslMechanism mechanism)
        {
            switch (mechanism)
            {
                case ActiveUp.Net.Mail.SaslMechanism.CramMd5:
                    return this._CramMd5(username, password);
                case ActiveUp.Net.Mail.SaslMechanism.Login:
                    return this._Login(username, password);
            }
            return string.Empty;
        }
        public override IAsyncResult BeginAuthenticate(string username, string password, SaslMechanism mechanism, AsyncCallback callback)
        {
            this._delegateAuthenticate = this.Authenticate;
            return this._delegateAuthenticate.BeginInvoke(username, password, mechanism, callback, null);
        }

        public string EndAuthenticate(IAsyncResult result)
        {
            return this._delegateAuthenticate.EndInvoke(result);
        }

        #endregion

        #region Idle

        /// <summary>
        /// Start the idle on the mail server.
        /// </summary>
        public void StartIdle()
        {
            this.Command("IDLE");

            System.IO.StreamReader sr = new System.IO.StreamReader(this.GetStream(), System.Text.Encoding.ASCII);
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            string response = string.Empty;
            _idleInProgress = true;
            while (true)
            {
                if (_idleInProgress)
                {
                    this.OnTcpReading();
                    response = sr.ReadLine();
                    this.OnTcpRead(new ActiveUp.Net.Mail.TcpReadEventArgs(response));

                    if (response.ToUpper().IndexOf("RECENT") > 0)
                    {
                        this.OnNewMessageReceived(new NewMessageReceivedEventArgs(int.Parse(response.Split(' ')[1])));
                    }
#if DEBUG
                    Console.WriteLine(response);
#endif
                }
                else
                {
                    this.Command("DONE", string.Empty);
                    break;
                }
            }
        }

        /// <summary>
        /// Stop the idle on the imap4 server.
        /// </summary>
        public void StopIdle()
        {
            _idleInProgress = false;
        }

        #endregion

        #endregion

        #region Command sending, receiving and stream access

        /// <summary>
        /// Sends the command to the server.
        /// The command tag is automatically added.
        /// </summary>
        /// <param name="command">The command (with arguments if necesary) to be sent.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.Command("select inbox");
        /// //Selected mailbox is inbox.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// imap.Command("select inbox")
        /// 'Selected mailbox is inbox.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.Command("select inbox");
        /// //Selected mailbox is inbox.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Command(string command)
        {
            return Command(command, (CommandOptions)null);
        }
        
        public string Command(string command, CommandOptions options)
        {
            return this.Command(command, System.DateTime.Now.ToString("yyMMddhhmmss" + System.DateTime.Now.Millisecond.ToString()), options);
        }
        public IAsyncResult BeginCommand(string command, AsyncCallback callback)
        {

            return this.BeginCommand(command, System.DateTime.Now.ToString("yyMMddhhmmss" + System.DateTime.Now.Millisecond.ToString()), callback);
        }

        public string Command(string command, string stamp)
        {
            return Command(command, stamp, (CommandOptions)null);
        }

        public string Command(string command, string stamp, CommandOptions options)
        {
            if (options == null)
                options = new CommandOptions();

            /*if (command.Length < 200) this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n"));
            else this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs("long command data"));*/
            //base.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n\r\n"), 0, stamp.Length + ((stamp.Length > 0) ? 1 : 0) + command.Length + 2);

            // Although I have still not read all the relevant code but here it looks that you are
            // directly trying to write to the network stream which is incorrect. I have commented your
            // line above writing directly to network stream and have slightly changed it to write to
            // sslstream. I am unable to biuld this solution as 200+ missing file errors are shown. But
            // I have run the NUnit test twice and it is passing succesfully therefore I have not checked
            // the reading portion from ssl stream. Theoreticaly decrytpion exception should only get generated
            // when there is a problem with reading from ssl stream but may be because direct attempt was made
            // to write to Network stream so some how it threw decryption exception.
            // please see it run and test it--------Atif

            // Complement the Atif changes. Use the flag for !PocketPC config for avoid build errors.

#if !PocketPC
            GetStream()
                .Write(
                    Encoding.GetEncoding("iso-8859-1")
                            .GetBytes(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n\r\n"), 0,
                    stamp.Length + ((stamp.Length > 0) ? 1 : 0) + command.Length + 2);
#endif

#if PocketPC
            GetStream().Write(Encoding.GetEncoding("iso-8859-1").GetBytes(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n\r\n"), 0, stamp.Length + ((stamp.Length > 0) ? 1 : 0) + command.Length + 2);
#endif

            OnTcpWritten(command.Length < 200
                             ? new TcpWrittenEventArgs(stamp + ((stamp.Length > 0) ? " " : "") +
                                                       command + "\r\n")
                             : new TcpWrittenEventArgs("long command data"));
            OnTcpReading();
            var sr = new System.IO.StreamReader(GetStream(), Encoding.GetEncoding("iso-8859-1"));
            var buffer = new StringBuilder();

            var command_as_upper = command.ToUpper();
            string temp;
            string lastline;
            while (true)
            {
                temp = sr.ReadLine();
                Logger.AddEntry("bordel : " + temp);
                buffer.Append(temp + "\r\n");
                if (command_as_upper.StartsWith("LIST") || command_as_upper.StartsWith("XLIST"))
                {
                    if (temp != null && (temp.StartsWith(stamp) || (temp.StartsWith("+ ") && options.IsPlusCmdAllowed)))
                    {
                        lastline = temp;
                        break;
                    }
                }

                else if (command_as_upper.StartsWith("DONE"))
                {
                    lastline = temp;
                    if (lastline != null) stamp = lastline.Split(' ')[0];
                    break;
                }
                else if (temp != null)
                {
                    //Had to remove + check - this was failing when the email contained a line with + 
                    //Please add comments as to why here, and reimplement differently
                    if (temp.StartsWith(stamp) || temp.ToLower().StartsWith("* " + command.Split(' ')[0].ToLower()) ||
                        (temp.StartsWith("+ ") && options.IsPlusCmdAllowed))
                    {
                        lastline = temp;
                        break;
                    }
                }
                else
                    throw new Imap4Exception("Unexpected end of stream");

            }
            var buffer_string = buffer.ToString();


            OnTcpRead(buffer.Length < 200
                          ? new TcpReadEventArgs(buffer_string)
                          : new TcpReadEventArgs("long data"));
            if (lastline != null &&
                (lastline.StartsWith(stamp + " OK") || temp.ToLower().StartsWith("* " + command.Split(' ')[0].ToLower()) ||
                 temp.StartsWith("+ ")))
                return buffer_string;

            var failed_string = string.Format("Command \"{0}\" failed : {1}",
                                              command.StartsWith("login") ? "LOGIN *****" : command, buffer_string);

            throw new Imap4Exception(failed_string);
        }

        public IAsyncResult BeginCommand(string command, string stamp, AsyncCallback callback)
        {
            return BeginCommand(command, stamp, callback, null);
        }

        public IAsyncResult BeginCommand(string command, string stamp, AsyncCallback callback, CommandOptions options)
        {
            this._delegateCommand = this.Command;
            return this._delegateCommand.BeginInvoke(command, stamp, options, callback, this._delegateCommand);
        }

        public string Command(string command, string stamp, string checkStamp)
        {
            return Command(command, stamp, checkStamp, null);
        }

        public string Command(string command, string stamp, string checkStamp, CommandOptions options)
        {
            if (options == null)
                options = new CommandOptions();

            if (command.Length < 200) this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n"));
            else this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs("long command data"));
            base.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n"), 0, stamp.Length + ((stamp.Length > 0) ? 1 : 0) + command.Length + 2);
            if (command.Length < 200) this.OnTcpWritten(new ActiveUp.Net.Mail.TcpWrittenEventArgs(stamp + ((stamp.Length > 0) ? " " : "") + command + "\r\n"));
            else this.OnTcpWritten(new ActiveUp.Net.Mail.TcpWrittenEventArgs("long command data"));
            this.OnTcpReading();
            System.IO.StreamReader sr = new System.IO.StreamReader(base.GetStream(), true);
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            string temp = "";
            string lastline = "";
            while (true)
            {
                temp = sr.ReadLine();
                buffer.Append(temp + "\r\n");
                if (temp.StartsWith("+ ") && options.IsPlusCmdAllowed)
                {
                    lastline = temp;
                    break;
                }

                if (temp.StartsWith(checkStamp) || temp.ToLower().StartsWith("* " + command.Split(' ')[0].ToLower()))
                {
                    lastline = temp;
                    break;
                }
            }
            if (buffer.Length < 200) this.OnTcpRead(new ActiveUp.Net.Mail.TcpReadEventArgs(buffer.ToString()));
            else this.OnTcpRead(new ActiveUp.Net.Mail.TcpReadEventArgs("long data"));
            if (lastline.StartsWith(checkStamp + " OK") || temp.ToLower().StartsWith("* " + command.Split(' ')[0].ToLower()) || temp.StartsWith("+ ")) return buffer.ToString();
            else throw new ActiveUp.Net.Mail.Imap4Exception("Command \"" + command + "\" failed : " + buffer.ToString());
        }
        
        public IAsyncResult BeginCommand(string command, string stamp, string checkStamp, AsyncCallback callback)
        {
            return BeginCommand(command, stamp, checkStamp, null);
        }

        public IAsyncResult BeginCommand(string command, string stamp, string checkStamp, AsyncCallback callback, CommandOptions options)
        {
            this._delegateCommandStringStringString = this.Command;
            return this._delegateCommandStringStringString.BeginInvoke(command, stamp, checkStamp, options, callback, this._delegateCommandStringStringString);
        }

        public string EndCommand(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Gets the communacation stream of this object.
        /// </summary>
        /// <returns>A Stream object, either of type NetworkStream or SslStream if the channel is secured.</returns>
//        public new System.IO.Stream GetStream()
//        {
//#if !PocketPC
//            if (this._sslStream != null) return this._sslStream;
//#endif
//            return base.GetStream();
//        }
        
        /// <summary>
        /// Imap STARTTLS. more info http://tools.ietf.org/html/rfc2595
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public override string StartTLS(string host)
        {
           try
            {
                var response = this.Command("STARTTLS");
                this.DoSslHandShake(new ActiveUp.Net.Security.SslHandShake(host));
                response = this.Command("capability");
                return response;
            }
            catch
            {
                return "Not supported";
            }
        }

        #endregion

        #region Implementation of the IMAP4 protocol

        /// <summary>
        /// Performs a NOOP command which is used to maintain the connection alive.
        /// </summary>
        /// <returns>The server response.</returns>
        /// <remarks>Some servers include mailbox update informations in the response.</remarks>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// try
        /// {
        ///        imap.Noop();
        ///        imap.Disconnect();
        ///    }
        ///    catch
        ///    {
        ///        throw new Exception("Connection lost.");
        ///    }
        ///     
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// Try
        ///     imap.Noop()
        ///        imap.Disconnect()
        ///    Catch
        ///        Throw New Exception("Connection lost.");
        ///    End Try
        ///    
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// try
        /// {
        ///        imap.Noop();
        ///        imap.Disconnect();
        ///    }
        ///    catch
        ///    {
        ///        throw new Exception("Connection lost.");
        ///    }
        /// </code>
        /// </example>
        public string Noop()
        {
            this.OnNooping();
            string response = this.Command("noop");
            this.OnNooped();
            return response;
        }
        public IAsyncResult BeginNoop(AsyncCallback callback)
        {
            this._delegateNoop = this.Noop;
            return this._delegateNoop.BeginInvoke(callback, this._delegateNoop);
        }

        public string EndNoop(IAsyncResult result)
        {
            return this._delegateNoop.EndInvoke(result);
        }

        /// <summary>
        /// Equivalent to Noop().
        /// </summary>
        /// <returns>The server's response.</returns>
        public string Check()
        {
            return this.Command("check");
        }
        public IAsyncResult BeginCheck(AsyncCallback callback)
        {
            this._delegateCheck = this.Check;
            return this._delegateCheck.BeginInvoke(callback, this._delegateCheck);
        }

        public string EndCheck(IAsyncResult result)
        {
            return this._delegateCheck.EndInvoke(result);
        }

        /// <summary>
        /// Closes the mailbox and removes messages marked with the Deleted flag.
        /// </summary>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// //Get the amount of messages in the inbox.
        /// int messageCount = inbox.MessageCount;
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// //Get the amount of messages in the inbox.
        /// Dim messageCount As Integer = inbox.MessageCount
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// //Get the amount of messages in the inbox.
        /// var messageCount:int = inbox.MessageCount;
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public new string Close()
        {
            return this.Command("close");
        }
        public IAsyncResult BeginClose(AsyncCallback callback)
        {
            this._delegateClose = this.Close;
            return this._delegateClose.BeginInvoke(callback, this._delegateClose);
        }

        public string EndClose(IAsyncResult result)
        {
            return this._delegateClose.EndInvoke(result);
        }

        /// <summary>
        /// Removes all messages marked with the Deleted flag.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// //Mark message 1 for deletion.
        /// inbox.DeleteMessage(1);
        /// //Effectively remove all message marked with Deleted flag.
        /// imap.Expunge();
        /// //Message 1 is permanently removed.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// 'Mark message 1 for deletion.
        /// inbox.DeleteMessage(1)
        /// 'Effectively remove all message marked with Deleted flag.
        /// imap.Expunge()
        /// 'Message 1 is permanently removed.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// //Mark message 1 for deletion.
        /// inbox.DeleteMessage(1);
        /// //Effectively remove all message marked with Deleted flag.
        /// imap.Expunge();
        /// //Message 1 is permanently removed.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public void Expunge()
        {
            this.Command("expunge");
        }
        public IAsyncResult BeginExpunge(AsyncCallback callback)
        {
            this._delegateExpunge = this.Expunge;
            return this._delegateExpunge.BeginInvoke(callback, this._delegateExpunge);
        }

        public void EndExpunge(IAsyncResult result)
        {
            this._delegateExpunge.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves a list of mailboxes.
        /// </summary>
        /// <param name="reference">The base path.</param>
        /// <param name="mailboxName">Mailbox name.</param>
        /// <returns>A MailboxCollection object containing the requested mailboxes.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// //Return all children mailboxes of "inbox".
        /// MailboxCollection mailboxes = imap.GetMailboxes("inbox","*");
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// 'Return all children mailboxes of "inbox".
        /// Dim mailboxes As MailboxCollection = imap.GetMailboxes("inbox","*")
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// //Return all children mailboxes of "inbox".
        /// var mailboxes:MailboxCollection  = imap.GetMailboxes("inbox","*");
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public MailboxCollection GetMailboxes(string reference, string mailboxName)
        {
            MailboxCollection mailboxes = new MailboxCollection();
            string response = this.Command("list \"" + reference + "\" \"" + mailboxName + "\"");
            string[] t = System.Text.RegularExpressions.Regex.Split(response, "\r\n");
            string box = "";
            for (int i = 0; i < t.Length - 2; i++)
            {
                try
                {
                    box = t[i].Substring(t[i].IndexOf("\" ") + 1).Trim(new char[] { ' ', '\"' });
                    if (box != reference && reference.ToLower() != (box.ToLower() + "/")) mailboxes.Add(this.ExamineMailbox(box));
                }
                catch { continue; }
            }
            return mailboxes;
        }
        public IAsyncResult BeginGetMailboxes(string reference, string mailboxName, AsyncCallback callback)
        {
            this._delegateGetMailboxes = this.GetMailboxes;
            return this._delegateGetMailboxes.BeginInvoke(reference, mailboxName, callback, this._delegateGetMailboxes);
        }

        public MailboxCollection EndGetMailboxes(IAsyncResult result)
        {
            return this._delegateGetMailboxes.EndInvoke(result);
        }

        /// <summary>
        /// Creates a mailbox with the specified name.
        /// </summary>
        /// <param name="mailboxName">The name of the new mailbox.</param>
        /// <returns>The newly created mailbox.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.CreateMailbox("inbox.Staff");
        /// //Child mailbox of inbox named Staff has been created.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// imap.CreateMailbox("inbox.Staff");
        /// 'Child mailbox of inbox named Staff has been created.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.CreateMailbox("inbox.Staff");
        /// //Child mailbox of inbox named Staff has been created.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public Mailbox CreateMailbox(string mailboxName)
        {
            this.Command("create \"" + mailboxName + "\"");
            return this.SelectMailbox(mailboxName);
        }
        public IAsyncResult BeginCreateMailbox(string mailboxName, AsyncCallback callback)
        {
            this._delegateMailboxOperation = this.CreateMailbox;
            return this._delegateMailboxOperation.BeginInvoke(mailboxName, callback, this._delegateMailboxOperation);
        }

        public Mailbox EndCreateMailbox(IAsyncResult result)
        {
            return this._delegateMailboxOperation.EndInvoke(result);
        }

        /// <summary>
        /// Renames a mailbox.
        /// </summary>
        /// <param name="oldMailboxName">The mailbox to be renamed.</param>
        /// <param name="newMailboxName">The new name of the mailbox.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.RenameMailbox("inbox.Staff","Staff");
        /// //The Staff mailbox is now a top-level mailbox.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// imap.RenameMailbox("inbox.Staff","Staff");
        /// 'The Staff mailbox is now a top-level mailbox.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.RenameMailbox("inbox.Staff","Staff");
        /// //The Staff mailbox is now a top-level mailbox.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string RenameMailbox(string oldMailboxName, string newMailboxName)
        {
            string response = this.Command("rename \"" + oldMailboxName + "\" \"" + newMailboxName + "\"");
            return response;
        }
        public IAsyncResult BeginRenameMailbox(string oldMailboxName, string newMailboxName, AsyncCallback callback)
        {
            this._delegateRenameMailbox = this.RenameMailbox;
            return this._delegateRenameMailbox.BeginInvoke(oldMailboxName, newMailboxName, callback, this._delegateRenameMailbox);
        }

        public void EndRenameMailbox(IAsyncResult result)
        {
            this._delegateRenameMailbox.EndInvoke(result);
        }

        /// <summary>
        /// Deletes a mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to be deleted.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.DeleteMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now deleted.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// imap.DeleteMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now deleted.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// imap.DeleteMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now deleted.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string DeleteMailbox(string mailboxName)
        {
            return this.Command("delete \"" + mailboxName + "\"");
        }
        public IAsyncResult BeginDeleteMailbox(string mailboxName, AsyncCallback callback)
        {
            this._delegateMailboxOperationReturnsString = this.DeleteMailbox;
            return this._delegateMailboxOperationReturnsString.BeginInvoke(mailboxName, callback, this._delegateMailboxOperationReturnsString);
        }

        public string EndDeleteMailbox(IAsyncResult result)
        {
            return this._delegateMailboxOperationReturnsString.EndInvoke(result);
        }

        /// <summary>
        /// Subscribes to a mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to be subscribed to.</param>
        /// <returns>The server's response.</returns>
        public string SubscribeMailbox(string mailboxName)
        {
            return this.Command("subscribe \"" + mailboxName + "\"");
        }
        public IAsyncResult BeginSubscribeMailbox(string mailboxName, AsyncCallback callback)
        {
            this._delegateMailboxOperationReturnsString = this.SubscribeMailbox;
            return this._delegateMailboxOperationReturnsString.BeginInvoke(mailboxName, callback, this._delegateMailboxOperationReturnsString);
        }

        public void EndSubscribeMailbox(IAsyncResult result)
        {
            this._delegateMailboxOperationReturnsString.EndInvoke(result);
        }

        /// <summary>
        /// Unsubscribes from a mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to be unsubscribed from.</param>
        /// <returns>The server's response.</returns>
        public string UnsubscribeMailbox(string mailboxName)
        {
            return this.Command("unsubscribe \"" + mailboxName + "\"");
        }
        public IAsyncResult BeginUnsubscribeMailbox(string mailboxName, AsyncCallback callback)
        {
            this._delegateMailboxOperationReturnsString = this.UnsubscribeMailbox;
            return this._delegateMailboxOperationReturnsString.BeginInvoke(mailboxName, callback, this._delegateMailboxOperationReturnsString);
        }

        public void EndUnsubscribeMailbox(IAsyncResult result)
        {
            this._delegateMailboxOperationReturnsString.EndInvoke(result);
        }

        /// <summary>
        /// Selects a mailbox on the server.
        /// </summary>
        /// <param name="mailboxName">The mailbox to be selected.</param>
        /// <returns>The selected mailbox.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// Mailbox mbox = imap.SelectMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now selected.
        /// mbox.Empty(true);
        /// //Mailbox inbox.Staff is now empty.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// Dim mbox As Mailbox = imap.SelectMailbox("inbox.Staff")
        /// 'The inbox.Staff mailbox is now selected.
        /// mbox.Empty(true)
        /// 'Mailbox inbox.Staff is now empty.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// var mbox:Mailbox = imap.SelectMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now selected.
        /// mbox.Empty(true);
        /// //Mailbox inbox.Staff is now empty.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public Mailbox SelectMailbox(string mailboxName)
        {
            mailboxName = renderSafeParam(mailboxName);

            ActiveUp.Net.Mail.Mailbox mailbox = new ActiveUp.Net.Mail.Mailbox();
            string response = this.Command("select \"" + mailboxName + "\"");
            string[] lines = System.Text.RegularExpressions.Regex.Split(response, "\r\n");

            // message count.
            int messageCount = 0;
            try { messageCount = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "EXISTS").Split(' ')[1]); }
            catch (Exception) { }
            mailbox.MessageCount = messageCount;

            // recent.
            int recent = 0;
            try { recent = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "RECENT").Split(' ')[1]); }
            catch (Exception) { }
            mailbox.Recent = recent;

            // unseen.
            int unseen = 0;
            try { unseen = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "[UNSEEN ").Split(' ')[3].TrimEnd(']')); }
            catch (Exception) { }
            mailbox.FirstUnseen = (response.ToLower().IndexOf("[unseen") != -1) ? unseen : 0;

            // uid validity.
            int uidValidity = 0;
            try { uidValidity = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "[UIDVALIDITY ").Split(' ')[3].TrimEnd(']')); }
            catch (Exception) { }
            mailbox.UidValidity = uidValidity;

            // flags.
            foreach (string str in ActiveUp.Net.Mail.Imap4Client.FindLine(lines, " FLAGS").Split(' '))
            {
                if (str.StartsWith("(\\") || str.StartsWith("\\"))
                {
                    mailbox.ApplicableFlags.Add(str.Trim(new char[] { ' ', '\\', ')', '(' }));
                }
            }

            // permanent flags.
            if (response.ToLower().IndexOf("[permanentflags") != -1)
            {
                foreach (string str in ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "[PERMANENTFLAGS").Split(' '))
                {
                    if (str.StartsWith("(\\") || str.StartsWith("\\"))
                    {
                        mailbox.PermanentFlags.Add(str.Trim(new char[] { ' ', '\\', ')', '(' }));
                    }
                }
            }

            // read-write and read-only.
            if (response.ToLower().IndexOf("[read-write]") != -1)
            {
                mailbox.Permission = ActiveUp.Net.Mail.MailboxPermission.ReadWrite;
            }
            else if (response.ToLower().IndexOf("[read-only]") != -1)
            {
                mailbox.Permission = ActiveUp.Net.Mail.MailboxPermission.ReadOnly;
            }

            mailbox.Name = mailboxName;
            mailbox.SourceClient = this;
            return mailbox;
        }
        public IAsyncResult BeginSelectMailbox(string mailboxName, AsyncCallback callback)
        {
            this._delegateMailboxOperation = this.SelectMailbox;
            return this._delegateMailboxOperation.BeginInvoke(mailboxName, callback, this._delegateMailboxOperation);
        }

        public Mailbox EndSelectMailbox(IAsyncResult result)
        {
            return this._delegateMailboxOperation.EndInvoke(result);
        }

        /// <summary>
        /// Same as SelectMailbox() except that the mailbox is opened with read-only permission.
        /// </summary>
        /// <param name="mailboxName">The mailbox to be examined.</param>
        /// <returns>The examined mailbox.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// Mailbox mbox = imap.ExamineMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now selected (read-only).
        /// int recentMessageCount = mbox.Recent;
        /// //There are recentMessageCount messages that haven't been read in inbox.Staff.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("user","pass")
        /// Dim mbox As Mailbox = imap.ExamineMailbox("inbox.Staff")
        /// 'The inbox.Staff mailbox is now selected (read-only).
        /// Dim recentMessageCount As Integer = mbox.Recent
        /// 'There are recentMessageCount messages that haven't been read in inbox.Staff.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("user","pass");
        /// var mbox:Mailbox = imap.ExamineMailbox("inbox.Staff");
        /// //The inbox.Staff mailbox is now selected (read-only).
        /// int recentMessageCount = mbox.Recent;
        /// //There are recentMessageCount messages that haven't been read in inbox.Staff.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public Mailbox ExamineMailbox(string mailboxName)
        {
            ActiveUp.Net.Mail.Mailbox mailbox = new ActiveUp.Net.Mail.Mailbox();
            string response = this.Command("examine \"" + mailboxName + "\"");
            string[] lines = System.Text.RegularExpressions.Regex.Split(response, "\r\n");
            mailbox.MessageCount = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "EXISTS").Split(' ')[1]);
            mailbox.Recent = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "RECENT").Split(' ')[1]);
            mailbox.FirstUnseen = (response.ToLower().IndexOf("[unseen") != -1) ? System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "[UNSEEN ").Split(' ')[3].TrimEnd(']')) : 0;
            mailbox.UidValidity = System.Convert.ToInt32(ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "[UIDVALIDITY ").Split(' ')[3].TrimEnd(']'));
            foreach (string str in ActiveUp.Net.Mail.Imap4Client.FindLine(lines, " FLAGS").Split(' ')) if (str.StartsWith("(\\") || str.StartsWith("\\")) mailbox.ApplicableFlags.Add(str.Trim(new char[] { ' ', '\\', ')', '(' }));
            if (response.ToLower().IndexOf("[permanentflags") != -1) foreach (string str in ActiveUp.Net.Mail.Imap4Client.FindLine(lines, "[PERMANENTFLAGS").Split(' ')) if (str.StartsWith("(\\") || str.StartsWith("\\")) mailbox.PermanentFlags.Add(str.Trim(new char[] { ' ', '\\', ')', '(' }));
            mailbox.Permission = ActiveUp.Net.Mail.MailboxPermission.ReadOnly;
            mailbox.Name = mailboxName;
            mailbox.SourceClient = this;
            return mailbox;
        }
        public IAsyncResult BeginExamineMailbox(string mailboxName, AsyncCallback callback)
        {
            this._delegateMailboxOperation = this.ExamineMailbox;
            return this._delegateMailboxOperation.BeginInvoke(mailboxName, callback, this._delegateMailboxOperation);
        }

        public Mailbox EndExamineMailbox(IAsyncResult result)
        {
            return this._delegateMailboxOperation.EndInvoke(result);
        }

        #endregion

        #endregion

        #endregion


    }

    #endregion
}