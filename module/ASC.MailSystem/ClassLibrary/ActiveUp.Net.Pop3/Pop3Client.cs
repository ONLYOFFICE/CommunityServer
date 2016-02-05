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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ActiveUp.Net.Security;

namespace ActiveUp.Net.Mail
{
    #region Pop3Client Object version 2

    /// <summary>
    /// POP3 Client extending a System.Net.Sockets.TcpClient to send/receive POP3 command/responses.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Pop3Client : ActiveUp.Net.Common.BaseProtocolClient
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Pop3Client()
        {
        }

#if PocketPC
    /// <summary>
    /// Finds PPC Encoding replacing ISO 8859-1 as standard.
    /// </summary>
        internal static System.Text.Encoding PPCEncode
        {
            get
            {
                //Since ISO 8859-1, not supported widelys (Depends on PPC region)
                //We are using Windows Code Page 1252 which is very much nearer to ISO
                //standard.
                return System.Text.Encoding.GetEncoding(1252);
            }
        }
#endif

        #endregion

        #region Events

        #region Event definitions

        /*/// <summary>
        /// Event fired when a certain amount of bytes are read during message or Header retrieval.
        /// </summary>
        public event ActiveUp.Net.Mail.ProgressEventHandler Progress;*/

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

        #endregion

        #region Event triggers and logging

        /*internal void OnProgress(ActiveUp.Net.Mail.ProgressEventArgs e)
        {
            if(Progress!=null) Progress(this,e);
        }*/

        internal void OnAuthenticating(ActiveUp.Net.Mail.AuthenticatingEventArgs e)
        {
            if (Authenticating != null) Authenticating(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Authenticating as {0} on {1}", e.Username, e.Host), 2);
        }

        internal void OnAuthenticated(ActiveUp.Net.Mail.AuthenticatedEventArgs e)
        {
            if (Authenticated != null) Authenticated(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Authenticated as {0} on {1}", e.Username, e.Host), 2);
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
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Sending {0}", e.Command), 1);
        }

        internal void OnTcpWritten(ActiveUp.Net.Mail.TcpWrittenEventArgs e)
        {
            if (TcpWritten != null) TcpWritten(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Sent {0}", e.Command), 1);
        }

        internal void OnTcpReading()
        {
            if (TcpReading != null) TcpReading(this);
            ActiveUp.Net.Mail.Logger.AddEntry("Reading...", 1);
        }

        internal void OnTcpRead(ActiveUp.Net.Mail.TcpReadEventArgs e)
        {
            if (TcpRead != null) TcpRead(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Read {0}", e.Response), 1);
        }

        internal void OnMessageRetrieving(ActiveUp.Net.Mail.MessageRetrievingEventArgs e)
        {
            if (MessageRetrieving != null) MessageRetrieving(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Retrieving message at index {0} out of {1}", e.MessageIndex.ToString(), e.TotalCount.ToString()), 2);
        }

        internal void OnMessageRetrieved(ActiveUp.Net.Mail.MessageRetrievedEventArgs e)
        {
            if (MessageRetrieved != null) MessageRetrieved(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Retrieved message at index {0} out of {1}", e.MessageIndex.ToString(), e.TotalCount.ToString()), 2);
        }

        internal void OnHeaderRetrieving(ActiveUp.Net.Mail.HeaderRetrievingEventArgs e)
        {
            if (HeaderRetrieving != null) HeaderRetrieving(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Retrieving Header at index {0} out of {1}", e.MessageIndex.ToString(), e.TotalCount.ToString()), 2);
        }

        internal void OnHeaderRetrieved(ActiveUp.Net.Mail.HeaderRetrievedEventArgs e)
        {
            if (HeaderRetrieved != null) HeaderRetrieved(this, e);
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Retrieved Header at index {0} out of {1}", e.MessageIndex.ToString(), e.TotalCount.ToString()), 2);
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
            ActiveUp.Net.Mail.Logger.AddEntry(string.Format("Connected. Server replied : {0}", e.ServerResponse), 2);
        }

        #endregion

        #endregion

        #region Private fields

        private int _messageCount;
        private long _totalSize;
        private string host;
        //#if !PocketPC
        //        System.Net.Security.SslStream _sslStream;
        //#endif

        #endregion

        #region Properties

        /// <summary>
        /// Number of messages on the remote POP server.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// int msgCount = pop.MessageCount;
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim msgCount as Integer = pop.MessageCount
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var msgCount:int = pop.MessageCount;
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public int MessageCount
        {
            get { return this._messageCount; }
            set { this._messageCount = value; }
        }

        /// <summary>
        /// Size of all messages on the remote POP server.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// int accountSize = pop.TotalSize;
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim accountSize as Integer = pop.TotalSize
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var accountSize:int = pop.TotalSize;
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public long TotalSize
        {
            get { return this._totalSize; }
            set { this._totalSize = value; }
        }

        #endregion

        #region Delegates and associated private fields

        private delegate string DelegateConnect(string host, int port);

        private DelegateConnect _delegateConnect;

        private delegate string DelegateConnectIPAddress(System.Net.IPAddress addr, int port);

        private DelegateConnectIPAddress _delegateConnectIPAddress;

        private delegate string DelegateConnectIPAddresses(System.Net.IPAddress[] addresses, int port);

        private DelegateConnectIPAddresses _delegateConnectIPAddresses;

        private delegate string DelegateConnectAuth(string host, int port, string user, string pass);

        private DelegateConnectAuth _delegateConnectAuth;

        private delegate string DelegateConnectAPOP(string host, int port, string user, string pass);

        private DelegateConnectAPOP _delegateConnectAPOP;

#if !PocketPC
        private delegate string DelegateConnectSslAuth(
            string host, int port, string username, string password, ActiveUp.Net.Security.SslHandShake sslHandShake);

        private DelegateConnectSslAuth _delegateConnectSslAuth;

        private delegate string DelegateConnectSsl(
            string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake);

        private DelegateConnectSsl _delegateConnectSsl;

        private delegate string DelegateConnectSslIPAddress(
            System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake);

        private DelegateConnectSslIPAddress _delegateConnectSslIPAddress;

        private delegate string DelegateConnectSslIPAddresses(
            System.Net.IPAddress[] addresses, int port, ActiveUp.Net.Security.SslHandShake sslHandShake);

        private DelegateConnectSslIPAddresses _delegateConnectSslIPAddresses;

#endif

        private delegate string DelegateAuthenticate(string username, string password, SaslMechanism mechanism);

        private DelegateAuthenticate _delegateAuthenticate;

        private delegate string DelegateLogin(string username, string password);

        private DelegateLogin _delegateLogin;

        private delegate string DelegateDisconnect();

        private DelegateDisconnect _delegateDisconnect;

        private delegate string DelegateCommand(string command);

        private DelegateCommand _delegateCommand;

        private delegate byte[] DelegateRetrieveMessage(int messageIndex, bool deleteMessage);

        private DelegateRetrieveMessage _delegateRetrieveMessage;

        private delegate Message DelegateRetrieveMessageObject(int messageIndex, bool deleteMessage);

        private DelegateRetrieveMessageObject _delegateRetrieveMessageObject;

        private delegate void DelegateStoreMessage(int messageIndex, bool deleteMessage, string destinationPath);

        private DelegateStoreMessage _delegateStoreMessage;

        private delegate byte[] DelegateRetrieveHeader(int messageIndex, int numberOfBodyLines);

        private DelegateRetrieveHeader _delegateRetrieveHeader;

        private delegate Header DelegateRetrieveHeaderObject(int messageIndex);

        private DelegateRetrieveHeaderObject _delegateRetrieveHeaderObject;

        private delegate void DelegateStoreHeader(int messageIndex, string destinationPath);

        private DelegateStoreHeader _delegateStoreHeader;

        private delegate void DelegateDeleteMessage(int indexOnServer);

        private DelegateDeleteMessage _delegateDeleteMessage;

        private delegate int DelegateReset();

        private DelegateReset _delegateReset;

        private delegate string DelegateGetUniqueID(int messageIndex);

        private DelegateGetUniqueID _delegateGetUniqueID;

        private delegate System.Collections.Generic.List<PopServerUniqueId> DelegateGetUniqueIDs();

        private DelegateGetUniqueIDs _delegateGetUniqueIDs;

        private delegate int DelegateGetMessageSize(int messageIndex);

        private DelegateGetMessageSize _delegateGetMessageSize;

        private delegate void DelegateUpdateStats();

        private DelegateUpdateStats _delegateUpdateStats;

        private delegate void DelegateNoop();

        private DelegateNoop _delegateNoop;

        private delegate bool DelegateCheckAPOP(string host, int port);

        private static DelegateCheckAPOP _delegateCheckAPOP;

        private delegate bool DelegateCheckAPOPString(string host);

        private static DelegateCheckAPOPString _delegateCheckAPOPString;

        private delegate string[] DelegateGetServerCapabilities();

        private DelegateGetServerCapabilities _delegateGetServerCapabilities;

        #endregion

        #region Methods

        #region Private utility methods

        private string _CramMd5(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password));
            //string digest = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(this.Command("auth cram-md5").Split(' ')[1].Trim(new char[] { '\r', '\n' })));
            byte[] data =
                System.Convert.FromBase64String(this.Command("auth cram-md5").Split(' ')[1].Trim(new char[] {'\r', '\n'}));
#if !PocketPC
            string digest = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(data, 0, data.Length);
            string response =
                this.Command(
                    System.Convert.ToBase64String(
                        System.Text.Encoding.GetEncoding("iso-8859-1")
                              .GetBytes(username + " " + ActiveUp.Net.Mail.Crypto.HMACMD5Digest(password, digest))));
#else
            string digest = PPCEncode.GetString(data, 0, data.Length);
            string response = this.Command(System.Convert.ToBase64String(PPCEncode.GetBytes(username + " " + ActiveUp.Net.Mail.Crypto.HMACMD5Digest(password, digest))));
#endif
            //string response = this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + " " + ActiveUp.Net.Mail.Crypto.HMACMD5Digest(password, digest))));
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, response));
            return response;
        }

        public override string Login(string user, string pass)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(user, pass, this.host));
            string response = this.Command(string.Format("USER {0}", user));
            string presponse = this.Command(string.Format("PASS {0}", pass));
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(user, pass, this.host, response));
            /*response = this.Command("STAT");
                var splited = response.Split(' ');
                this._messageCount = System.Convert.ToInt32(splited[1]);
                this._totalSize = System.Convert.ToInt32(splited[2]);*/
            return presponse;
        }

        public override IAsyncResult BeginLogin(string username, string password, AsyncCallback callback)
        {
            this._delegateLogin = this.Login;
            return this._delegateLogin.BeginInvoke(username, password, callback, this._delegateLogin);
        }

        private string _Login(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password));
            this.Command("auth login");
            //this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username)));
            //string response = this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(password)));
#if !PocketPC
            this.Command(System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(username)));
            string response =
                this.Command(
                    System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(password)));
#else
            this.Command(System.Convert.ToBase64String(PPCEncode.GetBytes(username)));
            string response = this.Command(System.Convert.ToBase64String(PPCEncode.GetBytes(password)));
#endif
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, response));
            return response;
        }

        private string ReadLine()
        {
            OnTcpReading();

            string response;

            using (var sr = new System.IO.StreamReader(GetStream(), Encoding.GetEncoding("iso-8859-1"), false,
                                                       Client.ReceiveBufferSize, true))
            {
                response = sr.ReadLine();
            }

            OnTcpRead(new TcpReadEventArgs(response));

            return response;
        }

        private string StoreToFile(string path, byte[] data)
        {
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create,
                                                               System.IO.FileAccess.Write);
            fs.Write(data, 0, data.Length);
            fs.Close();
            return path;
        }

        private System.IO.MemoryStream StoreToStream(byte[] data)
        {
            return new System.IO.MemoryStream(data, 0, data.Length, false, true);
        }

#if PocketPC
    /// <summary>
    /// This functions Injects Sleep wherever required in PocketPC. This is required so that
    /// Less powerful MessagePump in PocketPC gets chance to execute other things.
    /// </summary>
        private void PPCSleep()
        {
            System.Threading.Thread.Sleep(1);
        }
#endif

        #endregion

        #region Public methods

        #region Connecting, authenticating and disconnecting

        #region Cleartext methods

        /// <summary>
        /// Connects the object with the remote POP server using the given parameters.
        /// </summary>
        /// <param name="host">Remote POP server address.</param>
        /// <returns>The server's welcome greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com");
        /// </code>
        /// </example>
        public string Connect(string host)
        {
            this.host = host;
            return this.ConnectPlain(host, 110);
        }

        public IAsyncResult BeginConnect(string host, AsyncCallback callback)
        {
            return this.BeginConnectPlain(host, 110, callback);
        }

        /// <summary>
        /// Connects the object with the remote POP server using the given parameters.
        /// </summary>
        /// <param name="host">Remote POP server address.</param>
        /// <param name="port">The port to be used.</param>
        /// <returns>The server's welcome greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com",8503);
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com",8503)
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com",8503);
        /// </code>
        /// </example>
        public override string ConnectPlain(string host, int port)
        {
            this.host = host;
            this.OnConnecting();
            base.Connect(host, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            var response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }

        public string ConnectTLS(string host, int port)
        {
            this.ConnectPlain(host, port);
            return this.StartTLS(host);
        }

        public override string StartTLS(string host)
        {
            var response = this.Command("STLS");
            this.DoSslHandShake(new ActiveUp.Net.Security.SslHandShake(host));
            return response;
        }

        public override IAsyncResult BeginConnectPlain(string host, int port, AsyncCallback callback)
        {
            this._delegateConnect = this.ConnectPlain;
            return this._delegateConnect.BeginInvoke(host, port, callback, this._delegateConnect);
        }

        public IAsyncResult BeginConnectTLS(string host, int port, AsyncCallback callback)
        {
            this._delegateConnect = this.ConnectTLS;
            return this._delegateConnect.BeginInvoke(host, port, callback, this._delegateConnect);
        }

        public new string Connect(System.Net.IPAddress addr, int port)
        {
            this.OnConnecting();
            base.Connect(addr, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            string response = "";
            response = this.ReadLine();
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
                if(addresses.Length>0)
                    base.Connect(addresses[0], port);
                PPCSleep();
#endif
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            string response = "";
            response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }

        public IAsyncResult BeginConnect(System.Net.IPAddress[] addresses, int port, AsyncCallback callback)
        {
            this._delegateConnectIPAddresses = this.Connect;
            return this._delegateConnectIPAddresses.BeginInvoke(addresses, port, callback,
                                                                this._delegateConnectIPAddresses);
        }

        public string Connect(string host, string username, string password)
        {
            return this.Connect(host, 110, username, password);
        }

        public IAsyncResult BeginConnect(string host, string username, string password, AsyncCallback callback)
        {
            return this.BeginConnect(host, 110, username, password, callback);
        }

        public string Connect(string host, int port, string username, string password)
        {
            this.OnConnecting();
            base.Connect(host, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password, host));
            response = this.Command(string.Format("USER {0}", username));
            string presponse = this.Command(string.Format("PASS {0}", password));
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, host, response));
            /*response = this.Command("STAT");
            this._messageCount = System.Convert.ToInt32(response.Split(' ')[1]);
            this._totalSize = System.Convert.ToInt32(response.Split(' ')[2]);*/
            return presponse;
        }

        public IAsyncResult BeginConnect(string host, int port, string username, string password, AsyncCallback callback)
        {
            this._delegateConnectAuth = this.Connect;
            return this._delegateConnectAuth.BeginInvoke(host, port, username, password, callback,
                                                         this._delegateConnectAuth);
        }

        /// <summary>
        /// Connects the object with the remote POP server using the given parameters and APOP.
        /// </summary>
        /// <param name="user">Username on the remote POP server.</param>
        /// <param name="pass">Password on the remote POP server.</param>
        /// <param name="host">Remote POP server address.</param>
        /// <example>
        /// This will connect to the remote POP server using APOP.<br /><br />
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.APOPConnect("pop.myisp.com","username","password");
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client()
        /// pop.APOPConnect("pop.myisp.com","username","password")
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.APOPConnect("pop.myisp.com","username","password");
        /// </code>
        /// </example>
        public string APOPConnect(string host, string user, string pass)
        {
            return this.APOPConnect(host, 110, user, pass);
        }

        public IAsyncResult BeginAPOPConnect(string host, string user, string pass, AsyncCallback callback)
        {
            return this.BeginAPOPConnect(host, 110, user, pass, callback);
        }

        /// <summary>
        /// Connects the object with the remote POP server using the given parameters and APOP.
        /// </summary>
        /// <param name="user">Username on the remote POP server.</param>
        /// <param name="pass">Password on the remote POP server.</param>
        /// <param name="host">Remote POP server address.</param>
        /// <param name="port">Port to be used.</param>
        /// <example>
        /// This will connect to the remote POP server using APOP.<br /><br />
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.APOPConnect("pop.myisp.com","username","password",8503);
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client()
        /// pop.APOPConnect("pop.myisp.com","username","password",8503)
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.APOPConnect("pop.myisp.com","username","password",8503);
        /// </code>
        /// </example>
        public string APOPConnect(string host, int port, string user, string pass)
        {
            string response = this.ConnectPlain(host, port);
            string presponse = "";
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(user, pass, host));
            Match timestamp = Regex.Match(response, @"<.+@.+>");
            if (timestamp.Success)
            {
                string encrypted = timestamp.Value + pass;
                presponse = this.Command(string.Format("APOP {0} {1}", user, ActiveUp.Net.Mail.Crypto.MD5Digest(encrypted)));
                this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(user, pass, host, response));
                response = this.Command("STAT");
                this._messageCount = System.Convert.ToInt32(response.Split(' ')[1]);
                this._totalSize = System.Convert.ToInt32(response.Split(' ')[2]);
            }
            return presponse;
        }

        public IAsyncResult BeginAPOPConnect(string host, int port, string username, string password,
                                             AsyncCallback callback)
        {
            this._delegateConnectAPOP = this.APOPConnect;
            return this._delegateConnectAPOP.BeginInvoke(host, port, username, password, callback,
                                                         this._delegateConnectAPOP);
        }

        public string EndAPOPConnect(IAsyncResult result)
        {
            return
                (string)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
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
            return this.ConnectSsl(host, 995, new ActiveUp.Net.Security.SslHandShake(host));
        }

        public IAsyncResult BeginConnectSsl(string host, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 995, new ActiveUp.Net.Security.SslHandShake(host), callback);
        }

        public string ConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            return this.ConnectSsl(host, 995, sslHandShake);
        }

        public IAsyncResult BeginConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake,
                                            AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 995, sslHandShake, callback);
        }

        public override string ConnectSsl(string host, int port)
        {
            return this.ConnectSsl(host, port, new ActiveUp.Net.Security.SslHandShake(host));
        }

        public override IAsyncResult BeginConnectSsl(string host, int port, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, port, new SslHandShake(host), callback);
        }

        public string ConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.host = host;
            this.OnConnecting();
            base.Connect(host, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }

        public IAsyncResult BeginConnectSsl(string host, int port, ActiveUp.Net.Security.SslHandShake sslHandShake,
                                            AsyncCallback callback)
        {
            this._delegateConnectSsl = this.ConnectSsl;
            return this._delegateConnectSsl.BeginInvoke(host, port, sslHandShake, callback, this._delegateConnectSsl);
        }

        public string ConnectSsl(System.Net.IPAddress addr, int port, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(addr, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }

        public IAsyncResult BeginConnectSsl(System.Net.IPAddress addr, int port,
                                            ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSslIPAddress = this.ConnectSsl;
            return this._delegateConnectSslIPAddress.BeginInvoke(addr, port, sslHandShake, callback,
                                                                 this._delegateConnectSslIPAddress);
        }

        public string ConnectSsl(System.Net.IPAddress[] addresses, int port,
                                 ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(addresses, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }

        public IAsyncResult BeginConnectSsl(System.Net.IPAddress[] addresses, int port,
                                            ActiveUp.Net.Security.SslHandShake sslHandShake, AsyncCallback callback)
        {
            this._delegateConnectSslIPAddresses = this.ConnectSsl;
            return this._delegateConnectSslIPAddresses.BeginInvoke(addresses, port, sslHandShake, callback,
                                                                   this._delegateConnectSslIPAddresses);
        }

        public string ConnectSsl(string host, string user, string pass)
        {
            return this.ConnectSsl(host, 995, user, pass, new SslHandShake(host));
        }

        public IAsyncResult BeginConnectSsl(string host, string user, string pass, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 995, user, pass, new SslHandShake(host), callback);
        }

        public string ConnectSsl(string host, string user, string pass, SslHandShake sslHandShake)
        {
            return this.ConnectSsl(host, 995, user, pass, sslHandShake);
        }

        public IAsyncResult BeginConnectSsl(string host, string user, string pass, SslHandShake sslHandShake,
                                            AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 995, user, pass, sslHandShake, callback);
        }

        public string ConnectSsl(string host, int port, string user, string pass)
        {
            return this.ConnectSsl(host, port, user, pass, new SslHandShake(host));
        }

        public IAsyncResult BeginConnectSsl(string host, int port, string user, string pass, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, port, user, pass, new SslHandShake(host), callback);
        }

        public string ConnectSsl(string host, int port, string user, string pass, SslHandShake sslHandShake)
        {
            this.OnConnecting();
            base.Connect(host, port);
            base.SendTimeout = TcpSendTimeout;
            base.ReceiveTimeout = TcpReceiveTimeout;
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));

            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(user, pass, host));
            response = this.Command(string.Format("USER {0}", user));
            string presponse = this.Command(string.Format("PASS {0}", pass));
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(user, pass, host, response));
            /*response = this.Command("STAT");
            var splited = response.Split(' ');
            this._messageCount = System.Convert.ToInt32(splited[1]);
            this._totalSize = System.Convert.ToInt32(splited[2]);*/
            return presponse;
        }

        public IAsyncResult BeginConnectSsl(string host, int port, string user, string pass, SslHandShake sslHandShake,
                                            AsyncCallback callback)
        {
            this._delegateConnectSslAuth = this.ConnectSsl;
            return this._delegateConnectSslAuth.BeginInvoke(host, port, user, pass, sslHandShake, callback,
                                                            this._delegateConnectSslAuth);
        }
#endif

        #endregion

        #region SASL authentication

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
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com");
        /// pop.Authenticate("user","pass",SASLMechanism.CramMd5);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com")
        /// pop.Authenticate("user","pass",SASLMechanism.CramMd5)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com");
        /// pop.Authenticate("user","pass",SASLMechanism.CramMd5);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public override string Authenticate(string username, string password, SaslMechanism mechanism)
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

        public override IAsyncResult BeginAuthenticate(string username, string password, SaslMechanism mechanism,
                                                       AsyncCallback callback)
        {
            this._delegateAuthenticate = this.Authenticate;
            return this._delegateAuthenticate.BeginInvoke(username, password, mechanism, callback, null);
        }

        #endregion

        #region Disconnect method

        /// <summary>
        /// Disconnects the client from the remote server.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// //Do some work...
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// 'Do some work...
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// //Do some work...
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public override string Disconnect()
        {
            this.OnDisconnecting();
            try
            {
                var response = this.Command("QUIT");
                this.OnDisconnected(new ActiveUp.Net.Mail.DisconnectedEventArgs(response));
                return response;
            }
            finally
            {
                if (base._sslStream != null)
                    base._sslStream.Dispose();
                base.Dispose(true);
            }
        }

        public override IAsyncResult BeginDisconnect(AsyncCallback callback)
        {
            this._delegateDisconnect = this.Disconnect;
            return this._delegateDisconnect.BeginInvoke(callback, null);
        }

        #endregion

        #endregion

        #region Command sending and receiving, stream access

        /// <summary>
        /// Sends the provided string to the server.
        /// </summary>
        /// <param name="command">The string to be sent to the server.</param>
        /// <returns>The server's response.</returns>
        /// <remarks>This method is to be used only with commands that return single-line responses.</remarks>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// string response = pop.Command("XANYCOMMAND anyarguments");
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim response As String = pop.Command("XANYCOMMAND anyarguments")
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var response:string = pop.Command("XANYCOMMAND anyarguments");
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public string Command(string command)
        {
            OnTcpWriting(command.Length < 200
                             ? new TcpWritingEventArgs(command)
                             : new TcpWritingEventArgs("long command data"));

            GetStream().Write(Encoding.GetEncoding("iso-8859-1").GetBytes(command + "\r\n"), 0, command.Length + 2);

            OnTcpWriting(command.Length < 200
                             ? new TcpWritingEventArgs(command)
                             : new TcpWritingEventArgs("long command data"));

            OnTcpReading();

            string response;

            using (var sr = new System.IO.StreamReader(GetStream(), Encoding.GetEncoding("iso-8859-1"), false,
                                                       Client.ReceiveBufferSize, true))
            {
                response = sr.ReadLine();
            }

            if (response == null || !response.StartsWith("+"))
                throw new Pop3Exception(command.StartsWith("PASS") ? "PASS *****" : command, response);

            OnTcpRead(response.Length < 200
                          ? new TcpReadEventArgs(response)
                          : new TcpReadEventArgs("long data"));

            return response;
        }

        public IAsyncResult BeginCommand(string command, AsyncCallback callback)
        {
            this._delegateCommand = this.Command;
            return this._delegateCommand.BeginInvoke(command, callback, null);
        }

        public string EndCommand(IAsyncResult result)
        {
            return this._delegateCommand.EndInvoke(result);
        }

        /// <summary>
        /// Sends the provided string to the server.
        /// </summary>
        /// <param name="command">The string to be sent to the server.</param>
        /// <returns>The server's response.</returns>
        /// <remarks>This method is to be used only with commands that return multi-line responses.</remarks>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// string response = pop.CommandMultiline("XANYCOMMAND anyarguments");
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim response As String = pop.CommandMultiline("XANYCOMMAND anyarguments")
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var response:string = pop.CommandMultiline("XANYCOMMAND anyarguments");
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public string CommandMultiline(string command)
        {
            OnTcpWriting(command.Length < 200
                             ? new TcpWritingEventArgs(command)
                             : new TcpWritingEventArgs("long command data"));

            GetStream().Write(Encoding.GetEncoding("iso-8859-1").GetBytes(command + "\r\n"), 0, command.Length + 2);

            OnTcpWritten(command.Length < 200
                             ? new TcpWrittenEventArgs(command + "\r\n")
                             : new TcpWrittenEventArgs("long command data"));

            OnTcpReading();

            var buffer = new StringBuilder(MAX_RESPONSE_CAPACITY);

            using (
                var sr = new StreamReader(GetStream(), Encoding.GetEncoding("iso-8859-1"), false,
                                                    Client.ReceiveBufferSize, true))
            {
                var line = sr.ReadLine();

                if (line == null)
                    throw new EndOfStreamException("Unexpected end of stream");

                if (!line.StartsWith("+"))
                    throw new Pop3Exception(command, line);

                while (true)
                {
                    line = sr.ReadLine();
                    if (line != ".") buffer.Append(line).Append("\r\n");
                    else break;
                }
            }

            var bufferString = buffer.ToString().Replace("\r\n..\r\n", "\r\n.\r\n");

            buffer.Clear();

            OnTcpRead(new TcpReadEventArgs(string.Format("{0} bytes read", bufferString.Length)));

            return bufferString;
        }

        public IAsyncResult BeginCommandMultiline(string command, AsyncCallback callback)
        {
            this._delegateCommand = this.CommandMultiline;
            return this._delegateCommand.BeginInvoke(command, callback, null);
        }

        public string EndCommandMultiline(IAsyncResult result)
        {
            return this._delegateCommand.EndInvoke(result);
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

        #endregion

        #region Mailbox management

        /// <summary>
        /// Marks the message with the given index for deletion on the remote POP server.
        /// </summary>
        /// <param name="indexOnServer">Index of the message to mark for deletion.</param>
        /// <remarks>
        /// This action can be cancelled by using the Reset() method before disconnection.
        /// <see cref="Reset"/>
        /// </remarks>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("pop.myisp.com","username","password");
        /// pop.DeleteMessage(1);
        /// pop.Disconnect();
        /// //Message 1 deleted.
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client()
        /// pop.Connect("pop.myisp.com","username","password")
        /// pop.DeleteMessage(1)
        /// pop.Disconnect()
        /// 'Message 1 deleted.
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("pop.myisp.com","username","password");
        /// pop.DeleteMessage(1);
        /// pop.Disconnect();
        /// //Message 1 deleted.
        /// </code>
        /// </example>
        public void DeleteMessage(int indexOnServer)
        {
            string response = this.Command(string.Format("DELE {0}", indexOnServer.ToString()));
            if (!response.StartsWith("+OK")) throw new ActiveUp.Net.Mail.Pop3Exception(string.Format("DELE failed : {0}", response));
        }

        public IAsyncResult BeginDeleteMessage(int indexOnServer, AsyncCallback callback)
        {
            this._delegateDeleteMessage = this.DeleteMessage;
            return this._delegateDeleteMessage.BeginInvoke(indexOnServer, callback, null);
        }

        public void EndDeleteMessage(IAsyncResult result)
        {
            this._delegateDeleteMessage.EndInvoke(result);
        }

        /// <summary>
        /// Unmarks all messages that were marked for deletion.
        /// </summary>
        /// <returns>The amount of messages unmarked.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// ActiveUp.Net.Mail.Pop3Client pop = new ActiveUp.Net.Mail.Pop3Client();
        /// pop.Connect("pop.myisp.com","username","password");
        /// pop.DeleteMessage(1);
        /// //Message is marked for deletion.
        /// pop.Reset();
        /// //Message won't be deleted.
        /// pop.Disconnect();
        /// //Nothing happened.
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New ActiveUp.Net.Mail.Pop3Client()
        /// pop.Connect("pop.myisp.com","username","password")
        /// pop.DeleteMessage(1)
        /// 'Message is marked for deletion.
        /// pop.Reset()
        /// 'Message won't be deleted.
        /// pop.Disconnect()
        /// 'Nothing happened.
        /// 
        /// JScript.NET
        /// 
        /// var pop:ActiveUp.Net.Mail.Pop3Client = new ActiveUp.Net.Mail.Pop3Client();
        /// pop.Connect("pop.myisp.com","username","password");
        /// pop.DeleteMessage(1);
        /// //Message is marked for deletion.
        /// pop.Reset();
        /// //Message won't be deleted.
        /// pop.Disconnect();
        /// //Nothing happened.
        /// </code>
        /// </example>
        public int Reset()
        {
            string response = this.Command("RSET");
            if (!response.StartsWith("+OK")) throw new ActiveUp.Net.Mail.Pop3Exception(string.Format("RSET failed : {0}", response));
            else return System.Convert.ToInt32(response.Split(' ')[1]);
        }

        public IAsyncResult BeginReset(AsyncCallback callback)
        {
            this._delegateReset = this.Reset;
            return this._delegateReset.BeginInvoke(callback, null);
        }

        public int EndReset(IAsyncResult result)
        {
            return this._delegateReset.EndInvoke(result);
        }

        #endregion

        #region Message retrieval methods

        #region Message as raw data

        public byte[] RetrieveMessage(int messageIndex)
        {
            var msg = RetrieveMessageString(messageIndex);

#if !PocketPC
            byte[] buf = System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(msg);
#else
                    byte[] buf = PPCEncode.GetBytes(sb.ToString());
#endif

            this.OnMessageRetrieved(new ActiveUp.Net.Mail.MessageRetrievedEventArgs(buf, messageIndex, this.MessageCount));
            return buf;
        }

        /// <summary>
        /// Retrieves the message at the given index.
        /// </summary>
        /// <param name="messageIndex">The index of the message to be retrieved.</param>
        /// <returns>A byte array containing the message data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// byte[] messageData = pop.RetrieveMessage(1);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim messageData as Byte() = pop.RetrieveMessage(1)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var messageData:byte[] = pop.RetrieveMessage(1);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public string RetrieveMessageString(int messageIndex)
        {
            if (messageIndex <= 0)
                throw new Pop3Exception(
                    "The specified message index is invalid. Please specify an index that is greater than 0.");

            var encoding = Encoding.GetEncoding("iso-8859-1");

            OnMessageRetrieving(new MessageRetrievingEventArgs(messageIndex, MessageCount));

            var sendCommand = string.Format("RETR {0}\r\n", messageIndex.ToString());

            OnTcpWriting(new TcpWritingEventArgs(sendCommand));

            GetStream()
                .Write(encoding.GetBytes(sendCommand), 0,
                       7 + messageIndex.ToString().Length);

            OnTcpWritten(new TcpWrittenEventArgs(sendCommand));

            OnTcpReading();

            var buffer = new StringBuilder(MAX_RESPONSE_CAPACITY);

            using (var sr = new StreamReader(GetStream(), encoding, false, Client.ReceiveBufferSize, true))
            {
                var temp = sr.ReadLine();
                if (temp != null && temp.StartsWith("+OK"))
                {
                    while (true)
                    {
                        if (sr.EndOfStream)
                            break;

                        temp = sr.ReadLine();

                        if (temp == null || temp == ".")
                            break;

                        if (temp.StartsWith("..") && !temp.StartsWith("..."))
                            temp = temp.Remove(0, 1);

                        buffer.AppendFormat("{0}\r\n", temp);

                    }
                    this.OnTcpRead(new TcpReadEventArgs("Long message data..."));
                }
                else throw new Pop3Exception("RETR", temp);
            }

            return buffer.ToString();

        }

        public IAsyncResult BeginRetrieveMessage(int messageIndex, AsyncCallback callback)
        {
            return this.BeginRetrieveMessage(messageIndex, false, callback);
        }

        /// <summary>
        /// Retrieves the message at the given index.
        /// </summary>
        /// <param name="messageIndex">The index of the message to be retrieved.</param>
        /// <param name="deleteMessage">If true, the message will be deleted after it has been retrieved.</param>
        /// <returns>A byte array containing the message data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// byte[] messageData = pop.RetrieveMessage(1,true);
        /// pop.Disconnect();
        /// //Message 1 is deleted.
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim messageData as Byte() = pop.RetrieveMessage(1,True)
        /// pop.Disconnect()
        /// 'Message 1 is deleted.
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var messageData:byte[] = pop.RetrieveMessage(1,true);
        /// pop.Disconnect();
        /// //Message 1 is deleted.
        /// </code>
        /// </example>
        public byte[] RetrieveMessage(int messageIndex, bool deleteMessage)
        {
            byte[] buffer = this.RetrieveMessage(messageIndex);
            if (deleteMessage) this.DeleteMessage(messageIndex);
            return buffer;
        }

        public IAsyncResult BeginRetrieveMessage(int messageIndex, bool deleteMessage, AsyncCallback callback)
        {
            this._delegateRetrieveMessage = this.RetrieveMessage;
            return this._delegateRetrieveMessage.BeginInvoke(messageIndex, deleteMessage, callback, null);
        }

        public byte[] EndRetrieveMessage(IAsyncResult result)
        {
            return this._delegateRetrieveMessage.EndInvoke(result);
        }

        #endregion

        #region Message as object

        /// <summary>
        /// Retrieves the message at the given index.
        /// </summary>
        /// <param name="message_index">The index of the message to be retrieved.</param>
        /// <returns>A Message object representing the message.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// Message message = pop.RetrieveMessageObject(1);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim message as Message = pop.RetrieveMessageObject(1)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var message:Message = pop.RetrieveMessageObject(1);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public Message RetrieveMessageObject(int message_index)
        {
            var msg = RetrieveMessageString(message_index);

            Message message;

            try
            {
                message = Parser.ParseMessage(ref msg, this.LoadOriginalData);
            }
            catch (Exception ex)
            {
                if (ex is ParsingException || ex is IndexOutOfRangeException)
                {
                    Logger.AddEntry(string.Format("POP3 ParseMessage Error : {0}", ex.ToString()));

                    var headerString = RetrieveHeaderString(message_index);

                    Header header;

                    if (!Parser.TryParseDefectiveHeader(headerString, out header))
                        throw;

                    message = new Message(header);

                    message.AddAttachmentFromString("original_message.eml", msg);

                    message.ParseException = ex;
                }
                else
                    throw;
            }

            return message;
        }

        public IAsyncResult BeginRetrieveMessageObject(int messageIndex, AsyncCallback callback)
        {
            return this.BeginRetrieveMessageObject(messageIndex, false, callback);
        }

        /// <summary>
        /// Retrieves the message at the given index.
        /// </summary>
        /// <param name="message_index">The index of the message to be retrieved.</param>
        /// <param name="deleteMessage">If true, the message will be deleted after it has been retrieved.</param>
        /// <returns>A Message object representing the message.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// Message message = pop.RetrieveMessageObject(1);
        /// pop.Disconnect();
        /// //Message 1 is deleted.
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim message as Message = pop.RetrieveMessageObject(1)
        /// pop.Disconnect()
        /// 'Message 1 is deleted.
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var message:Message = pop.RetrieveMessageObject(1);
        /// pop.Disconnect();
        /// //Message 1 is deleted.
        /// </code>
        /// </example>
        public ActiveUp.Net.Mail.Message RetrieveMessageObject(int message_index, bool deleteMessage)
        {
            return ActiveUp.Net.Mail.Parser.ParseMessage(this.RetrieveMessage(message_index, deleteMessage));
        }

        public IAsyncResult BeginRetrieveMessageObject(int messageIndex, bool deleteMessage, AsyncCallback callback)
        {
            this._delegateRetrieveMessageObject = this.RetrieveMessageObject;
            return this._delegateRetrieveMessageObject.BeginInvoke(messageIndex, deleteMessage, callback, null);
        }

        public Message EndRetrieveMessageObject(IAsyncResult result)
        {
            return this._delegateRetrieveMessageObject.EndInvoke(result);
        }

        #endregion

        #region Store message data to a file

        /// <summary>
        /// Retrieves and stores the message at the specified index to the specified path.
        /// Deletes the message once retrieval operation is complete.
        /// </summary>
        /// <param name="messageIndex">Index of the message to be retrieved.</param>
        /// <param name="deleteMessage">If true, the message will be deleted after it has been retrieved.</param>
        /// <param name="destinationPath">The path where the message has to be stored.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// pop.StoreMessage(1,"C:\\My headers\\myheader.eml");
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// pop.StoreMessage(1,"C:\My headers\myheader.eml")
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// pop.StoreMessage(1,"C:\\My headers\\myheader.eml");
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public void StoreMessage(int messageIndex, bool deleteMessage, string destinationPath)
        {
            this.StoreToFile(destinationPath, this.RetrieveMessage(messageIndex, deleteMessage));
        }

        public IAsyncResult BeginStoreMessage(int messageIndex, bool deleteMessage, string destinationPath,
                                              AsyncCallback callback)
        {
            this._delegateStoreMessage = this.StoreMessage;
            return this._delegateStoreMessage.BeginInvoke(messageIndex, deleteMessage, destinationPath, callback, null);
        }

        public void EndStoreMessage(IAsyncResult result)
        {
            this._delegateStoreMessage.EndInvoke(result);
        }

        #endregion

        #endregion

        #region Header retrieval methods

        #region Header as raw data

        public string RetrieveHeaderString(int messageIndex)
        {
            return this.RetrieveHeaderString(messageIndex, 0);
        }

        public byte[] RetrieveHeader(int messageIndex)
        {
            return this.RetrieveHeader(messageIndex, 0);
        }

        public IAsyncResult BeginRetrieveHeader(int messageIndex, AsyncCallback callback)
        {
            return this.BeginRetrieveHeader(messageIndex, 0, callback);
        }

        /// <summary>
        /// Retrieves the Header of the message at the given index, plus a given number of lines beyond the Header limit.
        /// </summary>
        /// <param name="messageIndex">Index of the Header to be retrieved.</param>
        /// <param name="numberOfBodyLines">Number of lines to retrieve after the Header separation.</param>
        /// <returns>A byte array containing the Header data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// byte[] headerData = pop.RetrieveHeader(1,10);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim headerData as Byte() = pop.RetrieveHeader(1,10)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var headerData:byte[] = pop.RetrieveHeader(1,10);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveHeader(int messageIndex, int numberOfBodyLines)
        {
            this.OnHeaderRetrieving(new ActiveUp.Net.Mail.HeaderRetrievingEventArgs(messageIndex, this.MessageCount));
            string header = RetrieveHeaderString(messageIndex, numberOfBodyLines);
            //header = header.Replace(header.Split('\n')[0],"").TrimStart('\n');
            //byte[] buf = System.Text.Encoding.ASCII.GetBytes(header);
#if !PocketPC
            byte[] buf = System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(header);
#else
            byte[] buf = PPCEncode.GetBytes(header);
#endif
            this.OnHeaderRetrieved(new ActiveUp.Net.Mail.HeaderRetrievedEventArgs(buf, messageIndex, this.MessageCount));
            return buf;
        }

        public string RetrieveHeaderString(int messageIndex, int numberOfBodyLines)
        {
            var header_string =
                this.CommandMultiline(string.Format("TOP {0} {1}", messageIndex.ToString(), numberOfBodyLines.ToString()));
            return header_string;
        }

        public IAsyncResult BeginRetrieveHeader(int messageIndex, int numberOfBodyLines, AsyncCallback callback)
        {
            this._delegateRetrieveHeader = this.RetrieveHeader;
            return this._delegateRetrieveHeader.BeginInvoke(messageIndex, numberOfBodyLines, callback, null);
        }

        public byte[] EndRetrieveHeader(IAsyncResult result)
        {
            return this._delegateRetrieveHeader.EndInvoke(result);
        }

        #endregion

        #region Header as object

        /// <summary>
        /// Retrieves the Header of the message at the given index.
        /// </summary>
        /// <param name="messageIndex">Index of the Header to be retrieved.</param>
        /// <returns>A Header object representing the header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// Header headerData = pop.RetrieveHeaderObject(1);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim headerData as Header = pop.RetrieveHeaderObject(1)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// var headerData:Header = pop.RetrieveHeaderObject(1);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public ActiveUp.Net.Mail.Header RetrieveHeaderObject(int messageIndex)
        {
            return ActiveUp.Net.Mail.Parser.ParseHeader(this.RetrieveHeader(messageIndex));
        }

        public IAsyncResult BeginRetrieveHeaderObject(int messageIndex, AsyncCallback callback)
        {
            this._delegateRetrieveHeaderObject = this.RetrieveHeaderObject;
            return this._delegateRetrieveHeaderObject.BeginInvoke(messageIndex, callback, null);
        }

        public Header EndRetrieveHeaderObject(IAsyncResult result)
        {
            return this._delegateRetrieveHeaderObject.EndInvoke(result);
        }

        #endregion

        #region Store header data to a file

        /// <summary>
        /// Retrieves and stores the message Header at the specified index to the specified path.
        /// </summary>
        /// <param name="messageIndex">Index of the message Header to be retrieved.</param>
        /// <param name="destinationPath">The path where the Header has to be stored.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// pop.StoreHeader(1,"C:\\My headers\\myheader.eml");
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// pop.StoreHeader(1,"C:\My headers\myheader.eml")
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// pop.StoreHeader(1,"C:\\My headers\\myheader.eml");
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public void StoreHeader(int messageIndex, string destinationPath)
        {
            this.StoreToFile(destinationPath, this.RetrieveHeader(messageIndex));
        }

        public IAsyncResult BeginStoreHeader(int messageIndex, string destinationPath, AsyncCallback callback)
        {
            this._delegateStoreHeader = this.StoreHeader;
            return this._delegateStoreHeader.BeginInvoke(messageIndex, destinationPath, callback, null);
        }

        public void StoreHeader(int messageIndex, int numberOfBodyLines, string destinationPath)
        {
            this.StoreToFile(destinationPath, this.RetrieveHeader(messageIndex, numberOfBodyLines));
        }

        public IAsyncResult BeginStoreHeader(int messageIndex, int numberOfBodyLines, string destinationPath,
                                             AsyncCallback callback)
        {
            this._delegateStoreHeader = this.StoreHeader;
            return this._delegateStoreHeader.BeginInvoke(messageIndex, destinationPath, callback, null);
        }

        public void EndStoreHeader(IAsyncResult result)
        {
            this._delegateStoreHeader.EndInvoke(result);
        }

        #endregion

        #endregion

        #region Utility commands

        /// <summary>
        /// Issues a UIDL command and retrieves the message's unique Id (assigned by the server).
        /// </summary>
        /// <param name="messageIndex">The message's index.</param>
        /// <returns>The message's unique Id.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// string uniqueId = pop.UniqueId(1);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim uniqueId As String = pop.UniqueId(1)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// uniqueId:string = pop.UniqueId(1);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public string GetUniqueId(int messageIndex)
        {
            string resp = this.Command(string.Format("UIDL {0}", messageIndex.ToString()));
            return resp.Split(' ')[2];
        }

        public IAsyncResult BeginGetUniqueId(int messageIndex, AsyncCallback callback)
        {
            this._delegateGetUniqueID = this.GetUniqueId;
            return this._delegateGetUniqueID.BeginInvoke(messageIndex, callback, null);
        }

        public string EndGetUniqueId(IAsyncResult result)
        {
            return this._delegateGetUniqueID.EndInvoke(result);
        }

        /// <summary>
        /// Issues a UIDL command and retrieves all message unique Ids (assigned by the server).
        /// </summary>
        /// <returns>A list of a structure containing the unique Id of the messages and their index.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// System.Collections.Generic.List<PopServerUniqueId> uids = pop.UniqueIds();
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim uniqueId As System.Collections.Generic.List(Of PopServerUniqueId) = pop.UniqueIds()
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// uniqueId:System.Collections.Generic.List<PopServerUniqueId> = pop.UniqueIds();
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public System.Collections.Generic.List<PopServerUniqueId> GetUniqueIds()
        {
            System.Collections.Generic.List<PopServerUniqueId> uids =
                new System.Collections.Generic.List<PopServerUniqueId>();
            string ret = this.CommandMultiline("UIDL");
            string[] lines = ret.Replace("\r", "").Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(' ');
                PopServerUniqueId pse = new PopServerUniqueId();
                pse.Index = int.Parse(parts[0]);
                pse.UniqueId = parts[1];
                uids.Add(pse);
            }

            return uids;
        }

        public IAsyncResult BeginGetUniqueIds(AsyncCallback callback)
        {
            this._delegateGetUniqueIDs = this.GetUniqueIds;
            return this._delegateGetUniqueIDs.BeginInvoke(callback, null);
        }

        public System.Collections.Generic.List<PopServerUniqueId> EndGetUniqueIds(IAsyncResult result)
        {
            return this._delegateGetUniqueIDs.EndInvoke(result);
        }

        /// <summary>
        /// Retreives message index on the pop server from its internal unique Id.
        /// </summary>
        /// <param name="serverUniqueId">The given message unique Id to retreive.</param>
        /// <returns>The index of the message on the pop server, 0 if not found.</returns>
        public int GetMessageIndex(string serverUniqueId)
        {
            System.Collections.Generic.List<PopServerUniqueId> uids = this.GetUniqueIds();
            foreach (PopServerUniqueId uid in uids)
            {
                if (uid.UniqueId == serverUniqueId) return uid.Index;
            }
            return 0;
        }

        /// <summary>
        /// Indicates if the uniqueId exists on the server
        /// </summary>
        /// <param name="serverUniqueId">The given message unique Id to retreive.</param>
        /// <returns>True if unique Id exists, False if it doesn't.</returns>
        public bool UniqueIdExists(string serverUniqueId)
        {
            return GetMessageIndex(serverUniqueId) != 0;
        }

        /// <summary>
        /// Structure containing a uniqueId for a message and its associated index on the pop server
        /// </summary>
        public class PopServerUniqueId
        {
            private int _index;

            public int Index
            {
                get { return this._index; }
                set { this._index = value; }
            }

            private string _uniqueId;

            public string UniqueId
            {
                get { return this._uniqueId; }
                set { this._uniqueId = value; }
            }
        }

        /// <summary>
        /// Returns the size of the message at the given index.
        /// </summary>
        /// <param name="messageIndex">Index of the messages.</param>
        /// <returns>The size of the message at the given index.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// int uniqueId = pop.GetMessageSize(1);
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// Dim uniqueId As Integer = pop.GetMessageSize(1)
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// uniqueId:int = pop.GetMessageSize(1);
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public int GetMessageSize(int messageIndex)
        {
            string myline = this.Command(string.Format("LIST {0}", messageIndex.ToString()));
            //string myline = this.ReadLine();
            if (!myline.StartsWith("+OK")) throw new ActiveUp.Net.Mail.Pop3Exception("LIST", myline);
            else return System.Convert.ToInt32(myline.Split(' ')[2]);
        }

        public IAsyncResult BeginGetMessageSize(int messageIndex, AsyncCallback callback)
        {
            this._delegateGetMessageSize = this.GetMessageSize;
            return this._delegateGetMessageSize.BeginInvoke(messageIndex, callback, null);
        }

        public int EndGetMessageSize(IAsyncResult result)
        {
            return this._delegateGetMessageSize.EndInvoke(result);
        }

        public void UpdateStats()
        {
            string response = this.Command("STAT");
            //ActiveUp.Net.Mail.Logger.AddEntry(response);
            this.MessageCount = Convert.ToInt32(response.Split(' ')[1]);
            this.TotalSize = Convert.ToInt64(response.Split(' ')[2]);
        }

        public IAsyncResult BeginUpdateStats(AsyncCallback callback)
        {
            this._delegateUpdateStats = this.UpdateStats;
            return this._delegateUpdateStats.BeginInvoke(callback, null);
        }

        public void EndUpdateStats(IAsyncResult result)
        {
            this._delegateUpdateStats.EndInvoke(result);
        }

        /// <summary>
        /// Performs a NOOP command on the server. The aim of this command is to keep the connection alive.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// pop.Noop();
        /// pop.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","user","pass")
        /// pop.Noop()
        /// pop.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","user","pass");
        /// pop.Noop();
        /// pop.Disconnect();
        /// </code>
        /// </example>
        public void Noop()
        {
            this.OnNooping();
            this.Command("NOOP");
            this.OnNooped();
        }

        public IAsyncResult BeginNoop(AsyncCallback callback)
        {
            this._delegateNoop = this.Noop;
            return this._delegateNoop.BeginInvoke(callback, null);
        }

        public void EndNoop(IAsyncResult result)
        {
            this._delegateNoop.EndInvoke(result);
        }

        /// <summary>
        /// Checks if specified host has APOP capability.
        /// </summary>
        /// <param name="host">Host to be checked.</param>
        /// <param name="port">Port to connect on to the host.</param>
        /// <returns>True is remote server has APOP, otherwise false.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// bool serverHasAPOP = Pop3Client.CheckAPOP("mail.myhost.com",8503);
        /// 
        /// VB.NET
        /// 
        /// Dim serverHasAPOP As Boolen = Pop3Client.CheckAPOP("mail.myhost.com",8503)
        /// 
        /// JScript.NET
        /// 
        /// var serverHasAPOP:bool Pop3Client.CheckAPOP("mail.myhost.com",8503);
        /// </code>
        /// </example>
        public static bool CheckAPOP(string host, int port)
        {
            System.Net.Sockets.TcpClient _tcp = new System.Net.Sockets.TcpClient(host, port);
            byte[] buf = new byte[256];
            _tcp.GetStream().Read(buf, 0, 256);
            //string resp = System.Text.Encoding.ASCII.GetString(buf);
#if !PocketPC
            string resp = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(buf, 0, buf.Length);
#else
            string resp = PPCEncode.GetString(buf, 0, buf.Length);
#endif
            _tcp.Close();
            if (resp.IndexOf("<") != -1 && resp.IndexOf(">") != -1 && (resp.IndexOf("@") < resp.IndexOf(">")) &&
                (resp.IndexOf("@") > resp.IndexOf("<"))) return true;
            else return false;
        }

        /// <see cref="CheckAPOP"/>
        public static IAsyncResult BeginCheckAPOP(string host, int port, AsyncCallback callback)
        {
            Pop3Client._delegateCheckAPOP = Pop3Client.CheckAPOP;
            return Pop3Client._delegateCheckAPOP.BeginInvoke(host, port, callback, Pop3Client._delegateCheckAPOP);
        }


        /// <summary>
        /// Checks if specified host has APOP capability.
        /// </summary>
        /// <param name="host">Host to be checked.</param>
        /// <returns>True is remote server has APOP, otherwise false.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// bool serverHasAPOP = Pop3Client.CheckAPOP("mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim serverHasAPOP As Boolen = Pop3Client.CheckAPOP("mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// var serverHasAPOP:bool Pop3Client.CheckAPOP("mail.myhost.com");
        /// </code>
        /// </example>
        public static bool CheckAPOP(string host)
        {
            return Pop3Client.CheckAPOP(host, 110);
        }

        /// <see cref="CheckAPOP"/>
        public static IAsyncResult BeginCheckAPOP(string host, AsyncCallback callback)
        {
            Pop3Client._delegateCheckAPOPString = Pop3Client.CheckAPOP;
            return Pop3Client._delegateCheckAPOPString.BeginInvoke(host, callback, Pop3Client._delegateCheckAPOPString);
        }

        /// <see cref="CheckAPOP"/>
        public static bool EndCheckAPOP(IAsyncResult result)
        {
            return
                (bool)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }


        /// <summary>
        /// Gets the server capabilities.
        /// </summary>
        /// <remarks>Server capabilities are returned as an array of lines. Interpretation is left to the user.</remarks>
        /// <returns>An array of strings containing the server capabilities.</returns>
        public string[] GetServerCapabilities()
        {
#if !PocketPC
            return this.CommandMultiline("CAPA")
                       .Replace("\r", "")
                       .Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
#else
            return this.CommandMultiline("CAPA").Replace("\r", "").Split(new char[] { '\n' });
#endif

        }

        /// <see cref="GetServerCapabilities"/>
        public IAsyncResult BeginGetServerCapabilities(AsyncCallback callback)
        {
            this._delegateGetServerCapabilities = this.GetServerCapabilities;
            return this._delegateGetServerCapabilities.BeginInvoke(callback, null);
        }

        /// <see cref="GetServerCapabilities"/>
        public string[] EndGetServerCapabilities(IAsyncResult result)
        {
            return this._delegateGetServerCapabilities.EndInvoke(result);
        }

        #endregion

        #endregion

        #endregion
    }

    #endregion
}