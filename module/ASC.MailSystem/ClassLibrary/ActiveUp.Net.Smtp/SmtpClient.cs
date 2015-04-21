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
using System.Linq;
using System.Security.Authentication;
using ActiveUp.Net.Security;
using ActiveUp.Net.Mail;
using System.Collections.Generic;
using System.Net;
using ActiveUp.Net.Dns;

namespace ActiveUp.Net.Mail
{

    #region SmtpClient Object version 2

    /// <summary>
    /// Allows communication with an SMTP server.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class SmtpClient : ActiveUp.Net.Common.BaseProtocolClient
    {

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public SmtpClient()
        {
            base.SendTimeout = 30000;
            base.ReceiveTimeout = 30000;
            base.CertificatePermit = false;
        }

        #endregion

        #region Private fields

        private static long _UIDcounter;
        private string host;
#if !PocketPC
        //System.Net.Security.SslStream _sslStream;
#endif

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
        /// Event fired when authentication starts.
        /// </summary>
        public event ActiveUp.Net.Mail.AuthenticatingEventHandler Authenticating;

        /// <summary>
        /// Event fired when authentication completed.
        /// </summary>
        public event ActiveUp.Net.Mail.AuthenticatedEventHandler Authenticated;

        /// <summary>
        /// Event fired when a message is being sent.
        /// </summary>
        public event ActiveUp.Net.Mail.MessageSendingEventHandler MessageSending;

        /// <summary>
        /// Event fired when a message has been sent.
        /// </summary>
        public event ActiveUp.Net.Mail.MessageSentEventHandler MessageSent;

        #endregion

        #region Event triggers and logging

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
            ActiveUp.Net.Mail.Logger.AddEntry("Connected. Server replied : " + e.ServerResponse + "...", 2);
        }

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

        #endregion

        #endregion

        #region Delegates and associated private fields

        // Methods associated wit a same command shouldn't be called concurrently.
        // So one delegate per command is enough, when parameters and return types allow it.

        #region Connection management and SMTP implementation

        private delegate string DelegateConnect(string host, int port);

        private DelegateConnect _delegateConnect;

        private delegate string DelegateConnectIPAddress(System.Net.IPAddress addr, int port);

        private DelegateConnectIPAddress _delegateConnectIPAddress;

        private delegate string DelegateConnectIPAddresses(System.Net.IPAddress[] addresses, int port);

        private DelegateConnectIPAddresses _delegateConnectIPAddresses;

#if !PocketPC
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

        private delegate string DelegateDisconnect();

        private DelegateDisconnect _delegateDisconnect;

        private delegate string DelegateCommand(string command, int expectedResponseCode);

        private DelegateCommand _delegateCommand;

        private delegate string DelegateAuthenticate(string username, string password, SaslMechanism mechanism);

        private DelegateAuthenticate _delegateAuthenticate;

        private delegate string DelegateLogin(string username, string password);

        private DelegateLogin _delegateLogin;

        private delegate string DelegateData(string data);

        private DelegateData _delegateData;

        private delegate string DelegateHelo(string domain);

        private DelegateHelo _delegateHelo;

        private delegate string DelegateEhlo(string domain);

        private DelegateEhlo _delegateEhlo;

        private delegate string DelegateHelp();

        private DelegateHelp _delegateHelp;

        private delegate string DelegateMailFrom(string address);

        private DelegateMailFrom _delegateMailFrom;

        private delegate string DelegateRcptTo(string address);

        private DelegateRcptTo _delegateRcptTo;

        private delegate void DelegateRcptToAddressCollection(AddressCollection addresses);

        private DelegateRcptToAddressCollection _delegateRcptToAddressCollection;

        private delegate string DelegateNoop();

        private DelegateNoop _delegateNoop;

        private delegate bool DelegateVrfy(string address);

        private DelegateVrfy _delegateVrfy;

        private delegate AddressCollection DelegateVrfyAddresCollection(AddressCollection addresses);

        private DelegateVrfyAddresCollection _delegateVrfyAddressCollection;

        #endregion

        // Sending methods might be called concurrently, so we need one delegate per overload.

        #region Direct Send

        private delegate int DelegateDirectSendMessageCollection(MessageCollection messages);

        private static DelegateDirectSendMessageCollection _delegateDirectSendMessageCollection;

        private delegate int DelegateDirectSendMessageCollectionServerCollection(
            MessageCollection messages, ServerCollection dnsServers);

        private static DelegateDirectSendMessageCollectionServerCollection
            _delegateDirectSendMessageCollectionServerCollection;

        private delegate int DelegateDirectSendMessageCollectionString(MessageCollection messages, string dnsHost);

        private static DelegateDirectSendMessageCollectionString _delegateDirectSendMessageCollectionString;

        private delegate int DelegateDirectSendMessageCollectionStringInt(
            MessageCollection messages, string dnsHost, int dnsPort);

        private static DelegateDirectSendMessageCollectionStringInt _delegateDirectSendMessageCollectionStringInt;

        private delegate string DelegateDirectSendMessage(Message message);

        private static DelegateDirectSendMessage _delegateDirectSendMessage;

        private delegate string DelegateDirectSendMessageServerCollection(Message message, ServerCollection collection);

        private static DelegateDirectSendMessageServerCollection _delegateDirectSendMessageServerCollection;

        private delegate string DelegateDirectSendMessageString(Message message, string dnsHost);

        private static DelegateDirectSendMessageString _delegateDirectSendMessageString;

        private delegate string DelegateDirectSendMessageStringInt(Message message, string dnsHost, int dnsPort);

        private static DelegateDirectSendMessageStringInt _delegateDirectSendMessageStringInt;

        private delegate int DelegateDirectSendMessageCollectionSmtpExceptionCollection(
            MessageCollection message, ref SmtpExceptionCollection errors);

        private static DelegateDirectSendMessageCollectionSmtpExceptionCollection
            _delegateDirectSendMessageCollectionSmtpExceptionCollection;

        #endregion

        #region Send Queued

        private delegate void DelegateSendQueuedMessage(Message message, string spoolDirectory, QueuingService service);

        private static DelegateSendQueuedMessage _delegateSendQueuedMessage;

        private delegate void DelegateSendQueuedMessageCollection(
            MessageCollection messages, string spoolDirectory, QueuingService service);

        private static DelegateSendQueuedMessageCollection _delegateSendQueuedMessageCollection;

        #endregion

        #region Send with relay servers

        private delegate bool DelegateSendMessageServerCollection(Message message, ServerCollection servers);

        private static DelegateSendMessageServerCollection _delegateSendMessageServerCollection;

        private delegate bool DelegateSendMessageServerCollectionString(
            Message message, ServerCollection servers, out string serverMessage);

        private static DelegateSendMessageServerCollectionString _delegateSendMessageServerCollectionString;

        private delegate bool DelegateSendMessageString(Message message, string server);

        private static DelegateSendMessageString _delegateSendMessageString;

        private delegate bool DelegateSendMessageStringInt(Message message, string host, int port);

        private static DelegateSendMessageStringInt _delegateSendMessageStringInt;

        private delegate bool DelegateSendMessageStringStringStringSaslMechanism(
            Message message, string host, string username, string password, SaslMechanism mechanism);

        private static DelegateSendMessageStringStringStringSaslMechanism
            _delegateSendMessageStringStringStringSaslMechanism;

        private delegate bool DelegateSendMessageStringIntStringStringSaslMechanism(
            Message message, string host, int port, string username, string password, SaslMechanism mechanism);

        private static DelegateSendMessageStringIntStringStringSaslMechanism
            _delegateSendMessageStringIntStringStringSaslMechanism;

        private delegate int DelegateSendMessageCollectionString(MessageCollection messages, string server);

        private static DelegateSendMessageCollectionString _delegateSendMessageCollectionString;

        private delegate int DelegateSendMessageCollectionStringInt(MessageCollection messages, string server, int port);

        private static DelegateSendMessageCollectionStringInt _delegateSendMessageCollectionStringInt;

        private delegate int DelegateSendMessageCollectionStringIntSmtpExceptionCollection(
            MessageCollection messages, string server, int port, ref SmtpExceptionCollection errors);

        private static DelegateSendMessageCollectionStringIntSmtpExceptionCollection
            _delegateSendMessageCollectionStringIntSmtpExceptionCollection;

        private delegate int DelegateSendMessageCollectionServerCollection(
            MessageCollection messages, ServerCollection servers);

        private static DelegateSendMessageCollectionServerCollection _delegateSendMessageCollectionServerCollection;

        private delegate int DelegateSendMessageCollectionStringSmtpExceptionCollection(
            MessageCollection messages, string server, ref SmtpExceptionCollection errors);

        private static DelegateSendMessageCollectionStringSmtpExceptionCollection
            _delegateSendMessageCollectionStringSmtpExceptionCollection;

        private delegate int DelegateSendMessageCollectionServerCollectionSmtpExceptionCollection(
            MessageCollection messages, ServerCollection servers, ref SmtpExceptionCollection errors);

        private static DelegateSendMessageCollectionServerCollectionSmtpExceptionCollection
            _delegateSendMessageCollectionServerCollectionSmtpExceptionCollection;

        private delegate int DelegateSendMessageCollectionStringStringStringSaslMechanism(
            MessageCollection messages, string host, string username, string password, SaslMechanism mechanism);

        private static DelegateSendMessageCollectionStringStringStringSaslMechanism
            _delegateSendMessageCollectionStringStringStringSaslMechanism;

        private delegate int DelegateSendMessageCollectionStringStringStringSaslMechanismSmtpExceptionCollection(
            MessageCollection messages, string host, string username, string password, SaslMechanism mechanism,
            ref SmtpExceptionCollection errors);

        private static DelegateSendMessageCollectionStringStringStringSaslMechanismSmtpExceptionCollection
            _delegateSendMessageCollectionStringStringStringSaslMechanismSmtpExceptionCollection;

        private delegate int DelegateSendMessageCollectionStringIntStringStringSaslMechanism(
            MessageCollection messages, string host, int port, string username, string password, SaslMechanism mechanism
            );

        private static DelegateSendMessageCollectionStringIntStringStringSaslMechanism
            _delegateSendMessageCollectionStringIntStringStringSaslMechanism;

        private delegate int DelegateSendMessageCollectionStringIntStringStringSaslMechanismSmtpExceptionCollection(
            MessageCollection messages, string host, int port, string username, string password, SaslMechanism mechanism,
            ref SmtpExceptionCollection errors);

        private static DelegateSendMessageCollectionStringIntStringStringSaslMechanismSmtpExceptionCollection
            _delegateSendMessageCollectionStringIntStringStringSaslMechanismSmtpExceptionCollection;

        #endregion

        #region Quick Direct Send

        private delegate void DelegateQuickDirectSend(string from, string to, string subject, string textBody);

        private static DelegateQuickDirectSend _delegateQuickDirectSend;

        private delegate void DelegateQuickDirectSendAttach(
            string from, string to, string subject, string body, BodyFormat bodyFormat, string attachmentPath);

        private static DelegateQuickDirectSendAttach _delegateQuickDirectSendAttach;

        #endregion

        #region Quick Send

        private delegate void DelegateQuickSend(
            string from, string to, string subject, string textBody, string smtpServer);

        private static DelegateQuickSend _delegateQuickSend;

        private delegate void DelegateQuickSendAttach(
            string from, string to, string subject, string body, BodyFormat bodyFormat, string attachmentPath,
            string smtpServer);

        private static DelegateQuickSendAttach _delegateQuickSendAttach;

        #endregion

        #endregion

        #region Methods

        #region Private utility methods

        private string _CramMd5(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password));
            byte[] data =
                System.Convert.FromBase64String(
                    this.Command("auth cram-md5", 334).Split(' ')[1].Trim(new char[] {'\r', '\n'}));
            string digest = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);
            string response =
                this.Command(
                    System.Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes(username + " " +
                                                            ActiveUp.Net.Mail.Crypto.HMACMD5Digest(password, digest))),
                    235);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, response));
            return response;
        }

        private string _LoginOAuth2(string username, string accessToken)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, accessToken));
            string formatResponse = "user=" + username + "\u0001auth=Bearer " + accessToken + "\u0001\u0001";
            string authResponse = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(formatResponse));
            string response = this.Command("AUTH XOAUTH2 " + authResponse, 235);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, accessToken, response));
            return response;
        }

        private string ReadLine()
        {
            OnTcpReading();

            string response;

            using (var sr = new System.IO.StreamReader(GetStream(), System.Text.Encoding.ASCII, false,
                                                       Client.ReceiveBufferSize, true))
            {
                sr.BaseStream.ReadTimeout = Client.ReceiveTimeout;
                response = sr.ReadLine();
            }

            OnTcpRead(new TcpReadEventArgs(response));

            return response;

        }

        #endregion

        #region Public methods

        #region Connecting, authenticating and disconnecting

        #region Cleartext methods

        /// <summary>
        /// Connects to the specified server.
        /// </summary>
        /// <param name="host">Address of the server.</param>
        /// <returns>The server's welcome greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com");
        /// </code>
        /// </example>
        public string Connect(string host)
        {
            return this.ConnectPlain(host, 25);
        }

        public IAsyncResult BeginConnect(string host, AsyncCallback callback)
        {
            return this.BeginConnectPlain(host, 25, callback);
        }

        /// <summary>
        /// Connects to the specified server using the specified port.
        /// </summary>
        /// <param name="host">Address of the server.</param>
        /// <param name="port">Port to be used for connection.</param>
        /// <returns>The server's welcome greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// </code>
        /// </example>
        public override string ConnectPlain(string host, int port)
        {
            this.host = host;
            this.OnConnecting();
            base.Connect(host, port);
            var response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return base.Connected ? "+OK" : "-Connection failure";
        }

        public override IAsyncResult BeginConnectPlain(string host, int port, AsyncCallback callback)
        {
            this._delegateConnect = this.ConnectPlain;
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
                            {
                                base.Connect(addresses[0], port);
                            }
#endif
            //string response = "";
            string response = this.ReadLine();
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return response;
        }

        public IAsyncResult BeginConnect(System.Net.IPAddress[] addresses, int port, AsyncCallback callback)
        {
            this._delegateConnectIPAddresses = this.Connect;
#if !PocketPC
            return this._delegateConnectIPAddresses.BeginInvoke(addresses, port, callback,
                                                                this._delegateConnectSslIPAddresses);
#else
                        return this._delegateConnectIPAddresses.BeginInvoke(addresses, port, callback, null);
#endif
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
            return this.ConnectSsl(host, 465, new ActiveUp.Net.Security.SslHandShake(host));
        }

        public IAsyncResult BeginConnectSsl(string host, AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 465, new ActiveUp.Net.Security.SslHandShake(host), callback);
        }

        public string ConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            return this.ConnectSsl(host, 465, sslHandShake);
        }

        public IAsyncResult BeginConnectSsl(string host, ActiveUp.Net.Security.SslHandShake sslHandShake,
                                            AsyncCallback callback)
        {
            return this.BeginConnectSsl(host, 465, sslHandShake, callback);
        }

        public override string ConnectSsl(string host, int port)
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
            this.host = host;
            this.OnConnecting();
            base.Connect(host, port);
            this.DoSslHandShake(sslHandShake);
            string response = this.ReadLine();
            try
            {
                var s = this.Ehlo(host);
            }
            catch
            {
                var s = this.Helo(host);
            }
            this.OnConnected(new ActiveUp.Net.Mail.ConnectedEventArgs(response));
            return "+OK"; // if no exceptions
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
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com");
        /// smtp.Authenticate("user","pass",SASLMechanism.CramMd5);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com")
        /// smtp.Authenticate("user","pass",SASLMechanism.CramMd5)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com");
        /// smtp.Authenticate("user","pass",SASLMechanism.CramMd5);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public override string Authenticate(string username, string password, ActiveUp.Net.Mail.SaslMechanism mechanism)
        {
            switch (mechanism)
            {
                case ActiveUp.Net.Mail.SaslMechanism.CramMd5:
                    this._CramMd5(username, password);
                    break;
                case ActiveUp.Net.Mail.SaslMechanism.Login:
                    this.Login(username, password);
                    break;
                case ActiveUp.Net.Mail.SaslMechanism.OAuth2:
                    this._LoginOAuth2(username, password);
                    break;
                default:
                    return string.Empty;
            }
            return "+OK"; // if no exceptions
        }

        public override IAsyncResult BeginAuthenticate(string username, string password, SaslMechanism mechanism,
                                                       AsyncCallback callback)
        {
            this._delegateAuthenticate = this.Authenticate;
            return this._delegateAuthenticate.BeginInvoke(username, password, mechanism, callback, null);
        }

        public override string Login(string username, string password)
        {
            this.OnAuthenticating(new ActiveUp.Net.Mail.AuthenticatingEventArgs(username, password));
            this.Command("auth login", 334);
            this.Command(System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username)), 334);
            string response = this.Command(
                System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(password)), 235);
            this.OnAuthenticated(new ActiveUp.Net.Mail.AuthenticatedEventArgs(username, password, response));
            return "+OK";
        }

        public override IAsyncResult BeginLogin(string username, string password, AsyncCallback callback)
        {
            this._delegateLogin = this.Login;
            return this._delegateLogin.BeginInvoke(username, password, callback, this._delegateLogin);
        }

        #endregion

        #region Disconnect method

        /// <summary>
        /// Performs a QUIT command on the server and closes connection.
        /// </summary>
        /// <returns>The server's good bye greeting.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com");
        /// //Do some work...
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com")
        /// 'Do some work...
        /// smtp.Disconnect()
        ///  
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com");
        /// //Do some work...
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public override string Disconnect()
        {
            this.OnDisconnecting();
            try
            {
                var response = this.Command("QUIT", 221);
                this.OnDisconnected(new ActiveUp.Net.Mail.DisconnectedEventArgs(response));
                return response;
            }
            finally
            {
                if (base._sslStream != null)
                    base._sslStream.Dispose();
                base.Dispose(false);
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
        /// Performs the specified command on the server and returns the response.
        /// </summary>
        /// <param name="command">The command to be performed.</param>
        /// <param name="expectedResponseCode">The expected response code, which will allow the client to know if the command succeeded or failed.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// smtp.Command("XANYCOMMAND",213);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// smtp.Command("XANYCOMMAND",213)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// smtp.Command("XANYCOMMAND",213);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public override string Command(string command, int expectedResponseCode)
        {
            try
            {
                return base.Command(command, expectedResponseCode);
            }
            catch (Exception e)
            {
                throw new SmtpException(e.Message);
            }
        }

        public IAsyncResult BeginCommand(string command, int expectedResponseCode, AsyncCallback callback)
        {
            this._delegateCommand = new DelegateCommand(this.Command);
            return this._delegateCommand.BeginInvoke(command, expectedResponseCode, callback, null);
        }

        public string EndCommand(IAsyncResult result)
        {
            return this._delegateCommand.EndInvoke(result);
        }

        //                public new System.IO.Stream GetStream()
        //                {
        //#if !PocketPC
        //                    if (this._sslStream != null) return this._sslStream;
        //#endif
        //                    return base.GetStream();
        //                }

        public override string StartTLS(string host)
        {

            var response = this.Command("STARTTLS", 220);

            if (response != "Not supported")
            {
                this.DoSslHandShake(new ActiveUp.Net.Security.SslHandShake(host));

                response = this.SendEhloHelo();
            }

            return response;
        }

        #endregion

        #region Implementation of the SMTP and ESMTP protocols

        /// <summary>
        /// Performs a DATA command on the server with the specified data.
        /// </summary>
        /// <param name="data">The data to be transfered.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string Data(string data)
        {
            this.Command("DATA", 354);
            return this.Command(data + "\r\n.", 250);
        }

        public IAsyncResult BeginData(string data, AsyncCallback callback)
        {
            this._delegateData = this.Data;
            return this._delegateData.BeginInvoke(data, callback, null);
        }

        /// <summary>
        /// Performs a DATA command on the server with the specified data.
        /// </summary>
        /// <param name="data">The data to be transfered.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(Encoding.ASCII.GetBytes(message.ToMimeString()));
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(Encoding.ASCII.GetBytes(message.ToMimeString()))
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(Encoding.ASCII.GetBytes(message.ToMimeString()));
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string Data(byte[] data)
        {
            return this.Data(System.Text.Encoding.ASCII.GetString(data, 0, data.Length));
        }

        public IAsyncResult BeginData(byte[] data, AsyncCallback callback)
        {
            return this.BeginData(System.Text.Encoding.ASCII.GetString(data, 0, data.Length), callback);
        }

        /// <summary>
        /// Performs a DATA command on the server with the specified data.
        /// </summary>
        /// <param name="dataStream">A stream containing the data to be transfered.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(new MemoryStream(Encoding.ASCII.GetBytes(message.ToMimeString()),0,message.Size));
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(New MemoryStream(Encoding.ASCII.GetBytes(message.ToMimeString()),0,message.Size)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(new MemoryStream(Encoding.ASCII.GetBytes(message.ToMimeString()),0,message.Size));
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string Data(System.IO.MemoryStream dataStream)
        {
            byte[] buffer = new byte[dataStream.Length];
            dataStream.Read(buffer, 0, System.Convert.ToInt32(dataStream.Length));
            return this.Data(buffer);
        }

        public IAsyncResult BeginData(System.IO.MemoryStream dataStream, AsyncCallback callback)
        {
            byte[] buffer = new byte[dataStream.Length];
            dataStream.Read(buffer, 0, System.Convert.ToInt32(dataStream.Length));
            return this.BeginData(buffer, callback);
        }

        public string EndData(IAsyncResult result)
        {
            return this._delegateData.EndInvoke(result);
        }

        /// <summary>
        /// Performs a DATA command on the server with the specified data.
        /// </summary>
        /// <param name="dataFilePath">The path to a file containing the data to be transfered.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom("jdoe@myhost.com");
        /// smtp.RcptTo("mjohns@otherhost.com");
        /// smtp.DataFromFile("D:\\My mails\\mailtosend.eml");
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom("jdoe@myhost.com")
        /// smtp.RcptTo("mjohns@otherhost.com")
        /// smtp.DataFromFile("D:\My mails\mailtosend.eml")
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom("jdoe@myhost.com");
        /// smtp.RcptTo("mjohns@otherhost.com");
        /// smtp.DataFromFile("D:\\My mails\\mailtosend.eml");
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string DataFromFile(string dataFilePath)
        {
            return this.Data(new System.IO.StreamReader(dataFilePath, System.Text.Encoding.ASCII).ReadToEnd());
        }

        public IAsyncResult BeginDataFromFile(string dataFilePath, AsyncCallback callback)
        {
            return this.BeginData(new System.IO.StreamReader(dataFilePath, System.Text.Encoding.ASCII).ReadToEnd(),
                                  callback);
        }

        public string EndDataFromFile(IAsyncResult result)
        {
            return this._delegateData.EndInvoke(result);
        }

        /// <summary>
        /// Performs a EHLO command on the server.
        /// </summary>
        /// <param name="domain">The domain to be used as identifier.</param>
        /// <remarks>The use of this method indicates that the client is capable of using SMTP extensions (RFC2821).</remarks>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <seealso cref="Helo"/>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        //public string Ehlo(string domain)
        //{
        //    return this.Command("ehlo "+domain,250);
        //}
        public IAsyncResult BeginEhlo(string domain, AsyncCallback callback)
        {
            this._delegateEhlo = this.Ehlo;
            return this._delegateEhlo.BeginInvoke(domain, callback, null);
        }

        public string EndEhlo(IAsyncResult result)
        {
            return this._delegateEhlo.EndInvoke(result);
        }

        /// <summary>
        /// Performs a HELO command on the server.
        /// </summary>
        /// <param name="domain">The domain to be used as identifier.</param>
        /// <remarks>The use of this method isntead of Ehlo() indicates that the client is not capable of using SMTP extensions (RFC821).</remarks>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <seealso cref="Ehlo"/>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        //public string Helo(string domain)
        //{
        //    return this.Command("helo "+domain,250);
        //}
        public IAsyncResult BeginHelo(string domain, AsyncCallback callback)
        {
            this._delegateHelo = this.Helo;
            return this._delegateHelo.BeginInvoke(domain, callback, null);
        }

        public string EndHelo(IAsyncResult result)
        {
            return this._delegateHelo.EndInvoke(result);
        }

        /// <summary>
        /// Performs a HELP command on the server.
        /// </summary>
        /// <returns>The server's response (usually containing the commands it supports).</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// string helpString = smtp.Help();
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Dim helpString As String = smtp.Help()
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// var helpString:string = smtp.Help();
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string Help()
        {
            return this.Command("help", 211);
        }

        public IAsyncResult BeginHelp(AsyncCallback callback)
        {
            this._delegateHelp = this.Help;
            return this._delegateHelp.BeginInvoke(callback, null);
        }

        public string EndHelp(IAsyncResult result)
        {
            return this._delegateHelp.EndInvoke(result);
        }

        /// <summary>
        /// Performs a MAIL FROM: command on the server using the specified address.
        /// </summary>
        /// <param name="address">The address of the message sender.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string MailFrom(string address)
        {
            return this.Command("mail from: <" + address + ">", 250);
        }

        public IAsyncResult BeginMailFrom(string address, AsyncCallback callback)
        {
            this._delegateMailFrom = this.MailFrom;
            return this._delegateMailFrom.BeginInvoke(address, callback, null);
        }

        /// <summary>
        /// Performs a MAIL FROM: command on the server using the specified address.
        /// </summary>
        /// <param name="address">The address of the message sender.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string MailFrom(ActiveUp.Net.Mail.Address address)
        {
            return this.MailFrom(address.Email);
        }

        public IAsyncResult BeginMailFrom(Address address, AsyncCallback callback)
        {
            return this.BeginMailFrom(address.Email, callback);
        }

        public string EndMailFrom(IAsyncResult result)
        {
            return this._delegateMailFrom.EndInvoke(result);
        }

        /// <summary>
        /// Performs a NOOP command on the server (used to keep connection alive).
        /// </summary>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Noop()
        ///        //Connection still alive and timer reset on server.
        ///    }
        ///    catch
        ///    {
        ///        //Connection no longer available.
        ///    }
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///        smtp.Noop()
        ///        'Connection still alive and timer reset on server.
        ///    Catch
        ///        'Connection no longer available.
        ///    End Try
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// var helpString:string = smtp.Help();
        /// /// try
        /// {
        ///        smtp.Noop()
        ///        //Connection still alive and timer reset on server.
        ///    }
        ///    catch
        ///    {
        ///        //Connection no longer available.
        ///    }
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string Noop()
        {
            return this.Command("noop", 250);
        }

        public IAsyncResult BeginNoop(AsyncCallback callback)
        {
            this._delegateNoop = this.Noop;
            return this._delegateNoop.BeginInvoke(callback, null);
        }

        public string EndNoop(IAsyncResult result)
        {
            return this._delegateNoop.EndInvoke(result);
        }

        /// <summary>
        /// Performs a RCPT TO: command on the server using the specified address.
        /// </summary>
        /// <param name="address">The address of one of the message's recipients.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// foreach(Address address in message.To) smtp.RcptTo(address.Email);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// For Each address In message.To
        ///        smtp.RcptTo(address.Email)
        ///    Next
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// for(i:int=0;i&lt;message.To.Count;i++) smtp.RcptTo(message.To[i].Email);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string RcptTo(string address)
        {
            return this.Command("rcpt to: <" + address + ">", 250);
        }

        public IAsyncResult BeginRcptTo(string address, AsyncCallback callback)
        {
            this._delegateRcptTo = this.RcptTo;
            return this._delegateRcptTo.BeginInvoke(address, callback, this._delegateRcptTo);
        }

        /// <summary>
        /// Performs a RCPT TO: command on the server using the specified address.
        /// </summary>
        /// <param name="address">The address of one of the message's recipients.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// foreach(Address address in message.To) smtp.RcptTo(address.Email);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// For Each address In message.To
        ///        smtp.RcptTo(address)
        ///    Next
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// for(i:int=0;i&lt;message.To.Count;i++) smtp.RcptTo(message.To[i]);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public string RcptTo(ActiveUp.Net.Mail.Address address)
        {
            return this.RcptTo(address.Email);
        }

        public IAsyncResult BeginRcptTo(Address address, AsyncCallback callback)
        {
            return this.BeginRcptTo(address.Email, callback);
        }

        /// <summary>
        /// Performs a RCPT TO: command on the server using the specified addresses.
        /// </summary>
        /// <param name="addresses">The address of some of the message's recipients.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        /// smtp.MailFrom(message.From)
        /// smtp.RcptTo(message.To)
        /// smtp.Data(message.ToMimeString(),message.Charset)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        /// smtp.MailFrom(message.From);
        /// smtp.RcptTo(message.To);
        /// smtp.Data(message.ToMimeString(),message.Charset);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public void RcptTo(ActiveUp.Net.Mail.AddressCollection addresses)
        {
            foreach (ActiveUp.Net.Mail.Address address in addresses) this.RcptTo(address.Email);
        }

        public IAsyncResult BeginRcptTo(AddressCollection addresses, AsyncCallback callback)
        {
            this._delegateRcptToAddressCollection = this.RcptTo;
            return this._delegateRcptToAddressCollection.BeginInvoke(addresses, callback,
                                                                     this._delegateRcptToAddressCollection);
        }

        public string EndRcptTo(IAsyncResult result)
        {
            return
                (string)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        /// <summary>
        /// Performs a VRFY command on the server using the specified address (checks if the address refers to a mailbox on the server).
        /// </summary>
        /// <param name="address">The address to be verified.</param>
        /// <returns>True if address is valid and false if address doesn't reside on the server.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        ///    if(smtp.Verify("jdoe@myhost.com"))
        ///        bool isValid = true;
        ///        //Address is valid and resides on this server.
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        ///    If smtp.Verify("jdoe@myhost.com") Then
        ///        Dim isValid As Boolean = True
        ///        'Address is valid and resides on this server.
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        ///    if(smtp.Verify("jdoe@myhost.com")) 
        ///            var isValid:bool = true;
        ///        //Address is valid and resides on this server.
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public bool Verify(string address)
        {
            try
            {
                string rep = this.Command("vrfy " + address, 250);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IAsyncResult BeginVerify(string address, AsyncCallback callback)
        {
            this._delegateVrfy = this.Verify;
            return this._delegateVrfy.BeginInvoke(address, callback, this._delegateVrfy);
        }

        /// <summary>
        /// Performs a VRFY command on the server using the specified address (checks if the address refers to a mailbox on the server).
        /// </summary>
        /// <param name="address">The address to be verified.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        ///    if(smtp.Verify(someAddressObject))
        ///        bool isValid = true;
        ///        //Address is valid and resides on this server.
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        ///    If smtp.Verify(someAddressObject) Then
        ///        Dim isValid As Boolean = True
        ///        'Address is valid and resides on this server.
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        ///    if(smtp.Verify(someAddressObject)) 
        ///            var isValid:bool = true;
        ///        //Address is valid and resides on this server.
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public bool Verify(ActiveUp.Net.Mail.Address address)
        {
            return this.Verify(address.Email);
        }

        public IAsyncResult BeginVerify(Address address, AsyncCallback callback)
        {
            return this.BeginVerify(address.Email, callback);
        }

        /// <summary>
        /// Performs a VRFY command on the server using the specified addresses (checks if the addresses refer to mailboxes on the server).
        /// </summary>
        /// <param name="addresses">The addresses to be verified.</param>
        /// <returns>A collection containing the invalid addresses.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpClient smtp = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        ///    //Create a collection to test.
        ///    AddressCollection myaddresses = new AddressCollection();
        ///    myaddresses.Add("jdoe@myhost.com","John Doe");
        ///    myaddresses.Add("mjohns@otherhost.com","Mike Johns");
        ///    //Verifies all addresses.
        /// AddressCollection invalidAddresses = smtp.Verify(myaddresses);
        /// smtp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim smtp As New SmtpClient
        /// smtp.Connect("mail.myhost.com",8504)
        /// Try
        ///     smtp.Ehlo()
        ///    Catch
        ///        smtp.Helo()
        ///    End Try
        ///    'Create a collection to test.
        ///    Dim myaddresses As New AddressCollection
        ///    myaddresses.Add("jdoe@myhost.com","John Doe")
        ///    myaddresses.Add("mjohns@otherhost.com","Mike Johns")
        ///    'Verifies all addresses.
        /// Dim invalidAddresses As AddressCollection = smtp.Verify(myaddresses)
        /// smtp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var smtp:SmtpClient = new SmtpClient();
        /// smtp.Connect("mail.myhost.com",8504);
        /// try
        /// {
        ///        smtp.Ehlo();
        ///    }
        ///    catch
        ///    {
        ///        smtp.Helo();
        ///    }
        ///    //Create a collection to test.
        ///    var myaddresses:AddressCollection = new AddressCollection();
        ///    myaddresses.Add("jdoe@myhost.com","John Doe");
        ///    myaddresses.Add("mjohns@otherhost.com","Mike Johns");
        ///    //Verifies all addresses.
        /// var invalidAddresses:AddressCollection = smtp.Verify(myaddresses);
        /// smtp.Disconnect();
        /// </code>
        /// </example>
        public ActiveUp.Net.Mail.AddressCollection Verify(ActiveUp.Net.Mail.AddressCollection addresses)
        {
            ActiveUp.Net.Mail.AddressCollection incorrects = new ActiveUp.Net.Mail.AddressCollection();
            foreach (ActiveUp.Net.Mail.Address address in addresses)
            {
                try
                {
                    this.Verify(address.Email);
                }
                catch
                {
                    incorrects.Add(address);
                }
            }
            return incorrects;
        }

        public IAsyncResult BeginVerify(AddressCollection addresses, AsyncCallback callback)
        {
            this._delegateVrfyAddressCollection = this.Verify;
            return this._delegateVrfyAddressCollection.BeginInvoke(addresses, callback,
                                                                   this._delegateVrfyAddressCollection);
        }

        public string EndVerify(IAsyncResult result)
        {
            return
                (string)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        #endregion

        #region Sending methods

        #region Direct Send

        /// <summary>
        /// Sends the given message using the Direct Mailing method. The client connects to each recipient's mail exchange server and delivers the message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.DirectSend(message);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.DirectSend(message)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.DirectSend(message);
        /// </code>
        /// </example>
        public static string DirectSend(Message message)
        {
            return DirectSend(message, new ServerCollection());
        }

        public static IAsyncResult BeginDirectSend(Message message, AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessage = SmtpClient.DirectSend;
            return SmtpClient._delegateDirectSendMessage.BeginInvoke(message, callback,
                                                                     SmtpClient._delegateDirectSendMessage);
        }

        /// <summary>
        /// Sends the message using the specified host as dns server on the specified port.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="dnsHost">The host to be used.</param>
        /// <param name="dnsPort">The port to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.DirectSend(message,"ns1.dnsserver.com",53);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.DirectSend(message,"ns1.dnsserver.com",53)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.DirectSend(message,"ns1.dnsserver.com",53);
        /// </code>
        /// </example>
        public static string DirectSend(Message message, string dnsHost, int dnsPort)
        {
            ActiveUp.Net.Mail.ServerCollection servers = new ActiveUp.Net.Mail.ServerCollection();
            servers.Add(dnsHost, dnsPort);
            return DirectSend(message, servers);
        }

        public static IAsyncResult BeginDirectSend(Message message, string dnsHost, int dnsPort, AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageStringInt = SmtpClient.DirectSend;
            return SmtpClient._delegateDirectSendMessageStringInt.BeginInvoke(message, dnsHost, dnsPort, callback,
                                                                              SmtpClient
                                                                                  ._delegateDirectSendMessageStringInt);
        }

        /// <summary>
        /// Sends the message using the specified host as dns server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="dnsHost">The host to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.DirectSend(message,"ns1.dnsserver.com");
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.DirectSend(message,"ns1.dnsserver.com")
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.DirectSend(message,"ns1.dnsserver.com");
        /// </code>
        /// </example>
        public static string DirectSend(Message message, string dnsHost)
        {
            return DirectSend(message, dnsHost, 53);
        }

        public static IAsyncResult BeginDirectSend(Message message, string dnsHost, AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageString = SmtpClient.DirectSend;
            return SmtpClient._delegateDirectSendMessageString.BeginInvoke(message, dnsHost, callback,
                                                                           SmtpClient._delegateDirectSendMessageString);
        }

        /// <summary>
        /// Sends the message using the specified DNS servers to get mail exchange servers addresses.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="dnsServers">Servers to be used (in preference order).</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("ns1.dnsserver.com",53);
        /// servers.Add("ns2.dnsserver.com",53);
        /// 
        /// SmtpClient.DirectSend(message,servers);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("ns1.dnsserver.com",53)
        /// servers.Add("ns2.dnsserver.com",53)
        /// 
        /// SmtpClient.DirectSend(message,servers)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("ns1.dnsserver.com",53);
        /// servers.Add("ns2.dnsserver.com",53);
        /// 
        /// SmtpClient.DirectSend(message,servers);
        /// </code>
        /// </example>
        public static string DirectSend(Message message, ServerCollection dnsServers)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            string email = (message.From.Name != "(unknown)") ? message.From.Email : message.Sender.Email;
            int recipientCount = message.To.Count + message.Cc.Count + message.Bcc.Count;
#if !PocketPC
            System.Array domains = System.Array.CreateInstance(typeof (string), new int[] {recipientCount},
                                                               new int[] {0});
            System.Array adds = System.Array.CreateInstance(typeof (ActiveUp.Net.Mail.Address),
                                                            new int[] {recipientCount}, new int[] {0});
#else
                        System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { recipientCount });
                        System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { recipientCount });
#endif
            ActiveUp.Net.Mail.AddressCollection recipients = new ActiveUp.Net.Mail.AddressCollection();
            recipients += message.To;
            recipients += message.Cc;
            recipients += message.Bcc;
            for (int i = 0; i < recipients.Count; i++)
            {
                if (ActiveUp.Net.Mail.Validator.ValidateSyntax(recipients[i].Email))
                {
                    domains.SetValue(recipients[i].Email.Split('@')[1], i);
                    adds.SetValue(recipients[i], i);
                }
            }
            System.Array.Sort(domains, adds, null);
            string currentDomain = "";
            string address = "";
            string buf = "";
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            for (int j = 0; j < adds.Length; j++)
            {
                address = ((ActiveUp.Net.Mail.Address) adds.GetValue(j)).Email;
                if (((string) domains.GetValue(j)) == currentDomain)
                {
                    smtp.RcptTo(address);
                    if (j == (adds.Length - 1))
                    {
                        smtp.Data(message.ToMimeString(true));
                            //,(message.Charset!=null ? message.Charset : "iso-8859-1"));
                        smtp.Disconnect();
                    }
                }
                else
                {
                    if (currentDomain != "")
                    {
                        smtp.Data(message.ToMimeString(true));
                            //,(message.Charset!=null ? message.Charset : "iso-8859-1"));
                        smtp.Disconnect();
                        smtp = new ActiveUp.Net.Mail.SmtpClient();
                    }
                    currentDomain = (string) domains.GetValue(j);
                    buf += currentDomain + "|";

                    if (dnsServers == null || dnsServers.Count == 0)
                    {
                        if (dnsServers == null)
                            dnsServers = new ServerCollection();

                        IList<IPAddress> machineDnsServers = DnsQuery.GetMachineDnsServers();
                        foreach (IPAddress ipAddress in machineDnsServers)
                            dnsServers.Add(ipAddress.ToString());
                    }
                    ActiveUp.Net.Mail.MxRecordCollection mxs = ActiveUp.Net.Mail.Validator.GetMxRecords(currentDomain,
                                                                                                        dnsServers);
                    if (mxs != null && mxs.Count > 0) smtp.Connect(mxs.GetPrefered().Exchange);
                    else
                        throw new ActiveUp.Net.Mail.SmtpException("No MX record found for the domain \"" + currentDomain +
                                                                  "\". Check that the domain is correct and exists or specify a DNS server.");
                    try
                    {
                        smtp.Ehlo(System.Net.Dns.GetHostName());
                    }
                    catch
                    {
                        smtp.Helo(System.Net.Dns.GetHostName());
                    }
                    smtp.MailFrom(email);
                    smtp.RcptTo(address);
                    if (j == (adds.Length - 1))
                    {
                        smtp.Data(message.ToMimeString(true));
                            //,(message.Charset!=null ? message.Charset : "iso-8859-1"));                    
                        smtp.Disconnect();
                    }
                }
                //}
                //catch(ActiveUp.Net.Mail.SmtpException ex) { throw ex; }
            }
            return buf;
        }

        public static IAsyncResult BeginDirectSend(Message message, ServerCollection dnsServers, AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageServerCollection = SmtpClient.DirectSend;
            return SmtpClient._delegateDirectSendMessageServerCollection.BeginInvoke(message, dnsServers, callback,
                                                                                     SmtpClient
                                                                                         ._delegateDirectSendMessageServerCollection);
        }

        public static string EndDirectSend(IAsyncResult result)
        {
            return
                (string)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        /// <summary>
        /// Sends each message using the DirectMailing technique (SMTP connection with every recipient's mail exchange server for delivery).
        /// MX Records are cached for faster operation.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.DirectSend(messages);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.DirectSend(messages)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.DirectSend(messages);
        /// </code>
        /// </example>
        public static int DirectSendCollection(MessageCollection messages)
        {
            return DirectSendCollection(messages, new ServerCollection());
        }

        public static IAsyncResult BeginDirectSendCollection(MessageCollection messages, AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageCollection = SmtpClient.DirectSendCollection;
            return SmtpClient._delegateDirectSendMessageCollection.BeginInvoke(messages, callback,
                                                                               SmtpClient
                                                                                   ._delegateDirectSendMessageCollection);
        }

        /// <summary>
        /// Sends all messages using the specified host as the DNS server.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="dnsServers">Servers to be used to send the message (in preference order).</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("ns1.dnsserver.com",53);
        /// servers.Add("ns2.dnsserver.com",53);
        /// 
        /// SmtpClient.DirectSend(messages,servers);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("ns1.dnsserver.com",53)
        /// servers.Add("ns2.dnsserver.com",53)
        /// 
        /// SmtpClient.DirectSend(messages,servers)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("ns1.dnsserver.com",53);
        /// servers.Add("ns2.dnsserver.com",53);
        /// 
        /// SmtpClient.DirectSend(messages,servers);
        /// </code>
        /// </example>
        public static int DirectSendCollection(MessageCollection messages, ServerCollection dnsServers)
        {
            int sent = 0;
            foreach (ActiveUp.Net.Mail.Message message in messages)
            {
                ActiveUp.Net.Mail.SmtpClient.DirectSend(message);
                sent++;
            }
            return sent;
        }

        public static IAsyncResult BeginDirectSendCollection(MessageCollection messages, ServerCollection dnsServers,
                                                             AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageCollectionServerCollection = SmtpClient.DirectSendCollection;
            return SmtpClient._delegateDirectSendMessageCollectionServerCollection.BeginInvoke(messages, dnsServers,
                                                                                               callback,
                                                                                               SmtpClient
                                                                                                   ._delegateDirectSendMessageCollectionServerCollection);
        }

        /// <summary>
        /// Sends all messages using the specified host and port as the dns server.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="dnsHost">Address of the server to be used.</param>
        /// <param name="dnsPort">Port to be used.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.DirectSend(messages,"ns1.dnsserver.com",53);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.DirectSend(messages,"ns1.dnsserver.com",53)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.DirectSend(messages,"ns1.dnsserver.com",53);
        /// </code>
        /// </example>
        public static int DirectSendCollection(MessageCollection messages, string dnsHost, int dnsPort)
        {
            ActiveUp.Net.Mail.ServerCollection servers = new ActiveUp.Net.Mail.ServerCollection();
            servers.Add(dnsHost, dnsPort);
            return DirectSendCollection(messages, servers);
        }

        public static IAsyncResult BeginDirectSendCollection(MessageCollection messages, string dnsHost, int dnsPort,
                                                             AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageCollectionStringInt = SmtpClient.DirectSendCollection;
            return SmtpClient._delegateDirectSendMessageCollectionStringInt.BeginInvoke(messages, dnsHost, dnsPort,
                                                                                        callback,
                                                                                        SmtpClient
                                                                                            ._delegateDirectSendMessageCollectionStringInt);
        }

        /// <summary>
        /// Sends all messages using the specified host as the dns server.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="dnsHost">Address of the server to be used.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.DirectSend(messages,"ns1.dnsserver.com");
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.DirectSend(messages,"ns1.dnsserver.com")
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.DirectSend(messages,"ns1.dnsserver.com");
        /// </code>
        /// </example>
        public static int DirectSendCollection(MessageCollection messages, string dnsHost)
        {
            return DirectSendCollection(messages, dnsHost, 53);
        }

        public static IAsyncResult BeginDirectSendCollection(MessageCollection messages, string dnsHost,
                                                             AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageCollectionString = SmtpClient.DirectSendCollection;
            return SmtpClient._delegateDirectSendMessageCollectionString.BeginInvoke(messages, dnsHost, callback,
                                                                                     SmtpClient
                                                                                         ._delegateDirectSendMessageCollectionString);
        }

        /// <summary>
        /// Sends each message using the DirectMailing technique (SMTP connection with every recipient's mail exchange server for delivery).
        /// MX Records are cached for faster operation. Errors occuring during the process are catched and stored in a user-defined collection for review at a later time.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="errors">Reference to an SmtpException Collection to be filled with errors occuring during this process.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,myErrorCollection);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,myErrorCollection)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,myErrorCollection);
        /// </code>
        /// </example>
        public static int DirectSendCollection(MessageCollection messages, ref SmtpExceptionCollection errors)
        {
            string domain = string.Empty;
            string samer = string.Empty;
            int sent = 0;
            foreach (ActiveUp.Net.Mail.Message message in messages)
            {
                try
                {
                    ActiveUp.Net.Mail.SmtpClient.DirectSend(message);
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException ex)
                {
                    errors.Add(ex);
                }
            }
            return sent;
        }

        public static IAsyncResult BeginDirectSendCollection(MessageCollection messages,
                                                             ref SmtpExceptionCollection errors, AsyncCallback callback)
        {
            SmtpClient._delegateDirectSendMessageCollectionSmtpExceptionCollection = SmtpClient.DirectSendCollection;
            return SmtpClient._delegateDirectSendMessageCollectionSmtpExceptionCollection.BeginInvoke(messages,
                                                                                                      ref errors,
                                                                                                      callback,
                                                                                                      SmtpClient
                                                                                                          ._delegateDirectSendMessageCollectionSmtpExceptionCollection);
        }

        public static int EndDirectSendCollection(IAsyncResult result)
        {
            return
                (int)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        public static int EndDirectSendCollection(ref SmtpExceptionCollection errors, IAsyncResult result)
        {
            return
                (int)
                result.AsyncState.GetType()
                      .GetMethod("EndInvoke")
                      .Invoke(result.AsyncState, new object[] {errors, result});
        }

        #endregion

        #region Send Queued

        /// <summary>
        /// Sends the message using the specified queuing service and spool directory.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="spoolDirectory">The full path to the full directory.</param>
        /// <param name="queuingService">The queuing service to use.</param>
        public static void SendQueued(ActiveUp.Net.Mail.Message message, string spoolDirectory,
                                      ActiveUp.Net.Mail.QueuingService queuingService)
        {
            _UIDcounter++;
            if (_UIDcounter > 99999)
                _UIDcounter = 0;

            string UID = System.DateTime.Now.ToString("yyMMddhhmmss") + System.DateTime.Now.Millisecond.ToString() +
                         _UIDcounter.ToString().PadLeft(5, '0');

            ActiveUp.Net.Mail.Logger.AddEntry("UID Created for filename: '" + UID + "'", 0);

            switch (queuingService)
            {
                case ActiveUp.Net.Mail.QueuingService.MicrosoftSmtp:
                case ActiveUp.Net.Mail.QueuingService.ActiveQ:
                    message.StoreToFile(spoolDirectory.TrimEnd('\\') + "\\" + UID + ".eml", true);
                    break;
                case ActiveUp.Net.Mail.QueuingService.IpSwitchIMail:
                    string imailQueued = spoolDirectory.TrimEnd('\\') + "\\" + "D" + UID + ".SMD";
                    string imailSpec = spoolDirectory.TrimEnd('\\') + "\\" + "Q" + UID + ".SMD";
                    System.Text.StringBuilder imailDef = new System.Text.StringBuilder();
                    imailDef.Append("Q" + imailQueued + "\n");
                    imailDef.Append("H" + message.From.Email.Split('@')[1]);
                    imailDef.Append("S" + message.From.Email);
                    foreach (ActiveUp.Net.Mail.Address address in message.To)
                        imailDef.Append("R" + address.Email);
                    foreach (ActiveUp.Net.Mail.Address address in message.Cc)
                        imailDef.Append("R" + address.Email);
                    foreach (ActiveUp.Net.Mail.Address address in message.Bcc)
                        imailDef.Append("R" + address.Email);
                    System.IO.StreamWriter sw = System.IO.File.CreateText(imailSpec);
                    sw.Write(imailDef);
                    sw.Close();
                    message.StoreToFile(imailQueued, true);
                    break;
            }
        }

        public static IAsyncResult BeginSendQueued(Message message, string spoolDirectory, QueuingService queuingService,
                                                   AsyncCallback callback)
        {
            SmtpClient._delegateSendQueuedMessage = SmtpClient.SendQueued;
            return SmtpClient._delegateSendQueuedMessage.BeginInvoke(message, spoolDirectory, queuingService, callback,
                                                                     SmtpClient._delegateSendQueuedMessage);
        }

        /// <summary>
        /// Sends the messages using the specified queuing service and spool directory.
        /// </summary>
        /// <param name="messages">The messages to send.</param>
        /// <param name="spoolDirectory">The full path to the full directory.</param>
        /// <param name="queuingService">The queuing service to use.</param>
        public static void SendQueued(ActiveUp.Net.Mail.MessageCollection messages, string spoolDirectory,
                                      ActiveUp.Net.Mail.QueuingService queuingService)
        {
            foreach (ActiveUp.Net.Mail.Message message in messages)
            {
                SendQueued(message, spoolDirectory, queuingService);
            }
        }

        public static IAsyncResult BeginSendQueued(MessageCollection messages, string spoolDirectory,
                                                   QueuingService queuingService, AsyncCallback callback)
        {
            SmtpClient._delegateSendQueuedMessageCollection = SmtpClient.SendQueued;
            return SmtpClient._delegateSendQueuedMessageCollection.BeginInvoke(messages, spoolDirectory, queuingService,
                                                                               callback,
                                                                               SmtpClient
                                                                                   ._delegateSendQueuedMessageCollection);
        }

        public static void EndSendQueued(IAsyncResult result)
        {
            result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        #endregion

        #region Send with relay servers

        /// <summary>
        /// Sends the message using the specified host as mail exchange server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="servers">Servers to be used to send the message (in preference order).</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(message,servers);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("mail.myhost.com",25)
        /// servers.Add("mail2.myhost.com",25)
        /// 
        /// SmtpClient.Send(message,servers)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(message,servers);
        /// </code>
        /// </example>
        public static bool Send(Message message, ServerCollection servers)
        {
            string nothing;
            bool sent = Send(message, servers, out nothing);
            return sent;
        }

        public static IAsyncResult BeginSend(Message message, ServerCollection servers, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageServerCollection = SmtpClient.Send;
            return SmtpClient._delegateSendMessageServerCollection.BeginInvoke(message, servers, callback,
                                                                               SmtpClient
                                                                                   ._delegateSendMessageServerCollection);
        }

        /// <summary>
        /// Sends the message using the specified host as mail exchange server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="servers">Servers to be used to send the message (in preference order).</param>
        /// <param name="serverMessage"></param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(message,servers);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("mail.myhost.com",25)
        /// servers.Add("mail2.myhost.com",25)
        /// 
        /// SmtpClient.Send(message,servers)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(message,servers);
        /// </code>
        /// </example>
        public static bool Send(Message message, ServerCollection servers, out string serverMessage)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            serverMessage = string.Empty;
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            bool messageSent = false;
            for (int i = 0; i < servers.Count; i++)
            {
                try
                {
                    if (servers[i].ServerEncryptionType != EncryptionType.None)
                    {
#if !PocketPC
                        smtp.ConnectSsl(servers[i].Host, servers[i].Port);
#else
                                    smtp.Connect(servers[i].Host, servers[i].Port);
#endif
                    }
                    else
                    {
                        smtp.ConnectPlain(servers[i].Host, servers[i].Port);
                    }
                    try
                    {
                        smtp.Ehlo(System.Net.Dns.GetHostName());
                    }
                    catch
                    {
                        smtp.Helo(System.Net.Dns.GetHostName());
                    }
                    if (servers[i].Username != null && servers[i].Username.Length > 0 && servers[i].Password != null &&
                        servers[i].Password.Length > 0)
                        smtp.Authenticate(servers[i].Username, servers[i].Password, SaslMechanism.Login);
                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    serverMessage = smtp.Data(message.ToMimeString());
                        //,(message.Charset!=null ? message.Charset : "iso-8859-1"));
                    smtp.Disconnect();
                    messageSent = true;
                    break;
                }
                catch
                {
                    continue;
                }
            }

            return messageSent;
        }

        public static IAsyncResult BeginSend(Message message, ServerCollection servers, out string serverMessage,
                                             AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageServerCollectionString = SmtpClient.Send;
            return SmtpClient._delegateSendMessageServerCollectionString.BeginInvoke(message, servers, out serverMessage,
                                                                                     callback,
                                                                                     SmtpClient
                                                                                         ._delegateSendMessageServerCollectionString);
        }

        /// <summary>
        /// Sends the message using the specified host as mail exchange server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="server">Server to be used to send the message.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com");
        /// </code>
        /// </example>
        public static bool Send(Message message, string server)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.Connect(server);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
            else smtp.MailFrom(message.Sender);
            smtp.RcptTo(message.To);
            smtp.RcptTo(message.Cc);
            smtp.RcptTo(message.Bcc);
            smtp.Data(message.ToMimeString()); //,(message.Charset!=null ? message.Charset : "iso-8859-1"));
            smtp.Disconnect();
            return true;
        }

#if !PocketPC
        /// <summary>
        /// Sends the message using a secure connection with the specified host as mail exchange server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="server">Server to be used to send the message.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.SendSsl(message,"mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.SendSsl(message,"mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.SendSsl(message,"mail.myhost.com");
        /// </code>
        /// </example>
        public bool SendSsl(Message message, string server)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ConnectSsl(server);
            SendEhloHelo();
            SendMessageWith(message);
            return true;
        }

        public bool SendSsl(Message message, string server, int port)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ConnectSsl(server, port);
            SendEhloHelo();
            SendMessageWith(message);
            return true;
        }

        public bool SendSsl(Message message, string host, int port, EncryptionType enc_type)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            switch (enc_type)
            {
                case EncryptionType.SSL:
                    ConnectSsl(host, port);
                    break;
                case EncryptionType.StartTLS:
                    ConnectPlain(host, port);
                    break;
                default:
                    throw new ArgumentException("Incompatible EncriptionType with SendSSL: " + enc_type);
            }

            SendEhloHelo();

            if (enc_type == EncryptionType.StartTLS)
            {
                StartTLS(host);
            }

            SendMessageWith(message);
            return true;
        }

        private void SendMessageWith(Message message)
        {
            if (message.From.Email != string.Empty) MailFrom(message.From);
            else MailFrom(message.Sender);
            RcptTo(message.To);
            RcptTo(message.Cc);
            RcptTo(message.Bcc);
            Data(message.ToMimeString()); //,(message.Charset!=null ? message.Charset : "iso-8859-1"));
            Disconnect();
        }
#endif

        public static IAsyncResult BeginSend(Message message, string server, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageString = SmtpClient.Send;
            return SmtpClient._delegateSendMessageString.BeginInvoke(message, server, callback,
                                                                     SmtpClient._delegateSendMessageString);
        }

        /// <summary>
        /// Sends the message using the specified host as mail exchange server on the specified port.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="host">The host to be used.</param>
        /// <param name="port">The port to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com",8504);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com",8504)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com",8504);
        /// </code>
        /// </example>
        public bool Send(Message message, string host, int port)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ConnectPlain(host, port);
            try
            {
                Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                Helo(System.Net.Dns.GetHostName());
            }
            if (message.From.Email != string.Empty) MailFrom(message.From);
            else MailFrom(message.Sender);
            RcptTo(message.To);
            RcptTo(message.Cc);
            RcptTo(message.Bcc);
            Data(message.ToMimeString());
            Disconnect();
            return true;
        }

        public IAsyncResult BeginSend(Message message, string host, int port, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageStringInt = Send;
            return SmtpClient._delegateSendMessageStringInt.BeginInvoke(message, host, port, callback,
                                                                        SmtpClient._delegateSendMessageStringInt);
        }

        /// <summary>
        /// Sends the message using the specified host. Secure SASL Authentication is performed according to the requested mechanism.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="host">The host to be used.</param>
        /// <param name="username">The username to be used for authentication.</param>
        /// <param name="password">The password to be used for authentication.</param>
        /// <param name="mechanism">SASL Mechanism to be used for authentication.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// </code>
        /// </example>
        public static bool Send(Message message, string host, string username, string password, SaslMechanism mechanism)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.Connect(host);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            smtp.Authenticate(username, password, mechanism);
            if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
            else smtp.MailFrom(message.Sender);
            smtp.RcptTo(message.To);
            smtp.RcptTo(message.Cc);
            smtp.RcptTo(message.Bcc);
            smtp.Data(message.ToMimeString());
            smtp.Disconnect();
            return true;
        }

#if !PocketPC
        /// <summary>
        /// Sends the message using the specified host in a secured connection. Secure SASL Authentication is performed according to the requested mechanism.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="host">The host to be used.</param>
        /// <param name="username">The username to be used for authentication.</param>
        /// <param name="password">The password to be used for authentication.</param>
        /// <param name="mechanism">SASL Mechanism to be used for authentication.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.SendSsl(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.SendSsl(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.SendSsl(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// </code>
        /// </example>
        public bool SendSsl(Message message, string host, string username, string password,
                                   SaslMechanism mechanism)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ConnectSsl(host);
            SendEhloHelo();
            SendMessageWithAuthentication(username, password, mechanism, message);
            return true;
        }

        public bool SendSsl(Message message, string host, int port, string username, string password,
                                   SaslMechanism mechanism)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ConnectSsl(host, port);
            SendEhloHelo();
            SendMessageWithAuthentication(username, password, mechanism, message);
            return true;
        }

        public bool SendSsl(Message message, string host, int port, string username, string password,
                                   SaslMechanism mechanism, EncryptionType enc_type)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            switch (enc_type)
            {
                case EncryptionType.SSL:
                    ConnectSsl(host, port);
                    break;
                case EncryptionType.StartTLS:
                    ConnectPlain(host, port);
                    break;
                default:
                    throw new ArgumentException("Incompatible EncriptionType with SendSSL: " + enc_type);
            }

            SendEhloHelo();

            if (enc_type == EncryptionType.StartTLS)
            {
                StartTLS(host);
            }

            SendMessageWithAuthentication(username, password, mechanism, message);
            return true;
        }

        private void SendMessageWithAuthentication(string username, string password,
                                                          SaslMechanism mechanism, Message message)
        {
            Authenticate(username, password, mechanism);
            if (message.From.Email != string.Empty) MailFrom(message.From);
            else MailFrom(message.Sender);
            RcptTo(message.To);
            RcptTo(message.Cc);
            RcptTo(message.Bcc);
            Data(message.ToMimeString());
            Disconnect();
        }
#endif

        public static IAsyncResult BeginSend(Message message, string host, string username, string password,
                                             SaslMechanism mechanism, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageStringStringStringSaslMechanism = SmtpClient.Send;
            return SmtpClient._delegateSendMessageStringStringStringSaslMechanism.BeginInvoke(message, host, username,
                                                                                              password, mechanism,
                                                                                              callback,
                                                                                              SmtpClient
                                                                                                  ._delegateSendMessageStringStringStringSaslMechanism);
        }

        /// <summary>
        /// Sends the message using the specified host on the specified port. Secure SASL Authentication is performed according to the requested mechanism.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="host">The host to be used.</param>
        /// <param name="username">The username to be used for authentication.</param>
        /// <param name="password">The password to be used for authentication.</param>
        /// <param name="mechanism">SASL Mechanism to be used for authentication.</param>
        /// <param name="port">The port to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// SmtpClient.Send(message,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// </code>
        /// </example>
        public bool Send(Message message, string host, int port, string username, string password,
                                SaslMechanism mechanism)
        {
            // Ensure that the mime part tree is built
            message.CheckBuiltMimePartTree();

            ConnectPlain(host, port);
            SendEhloHelo();
            Authenticate(username, password, mechanism);
            if (message.From.Email != string.Empty) MailFrom(message.From);
            else MailFrom(message.Sender);
            RcptTo(message.To);
            RcptTo(message.Cc);
            RcptTo(message.Bcc);
            Data(message.ToMimeString());
            Disconnect();
            return true;
        }

        public IAsyncResult BeginSend(Message message, string host, int port, string username, string password,
                                             SaslMechanism mechanism, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageStringIntStringStringSaslMechanism = Send;
            return SmtpClient._delegateSendMessageStringIntStringStringSaslMechanism.BeginInvoke(message, host, port,
                                                                                                 username, password,
                                                                                                 mechanism, callback,
                                                                                                 SmtpClient
                                                                                                     ._delegateSendMessageStringIntStringStringSaslMechanism);
        }

        public static bool EndSend(IAsyncResult result)
        {
            return
                (bool)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        /// <summary>
        /// Sends all messages using the specified host.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="host">Address of the server to be used.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com");
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host)
        {
            return SendCollection(messages, host, 25);
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionString = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionString.BeginInvoke(messages, host, callback,
                                                                               SmtpClient
                                                                                   ._delegateSendMessageCollectionString);
        }

        /// <summary>
        /// Sends all messages using the specified host.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="host">Address of the server to be used.</param>
        /// <param name="errors">Reference to SmtpException object collection where errors occuring during the process will be stored.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",myErrorCollection);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",myErrorCollection)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",myErrorCollection);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host, ref SmtpExceptionCollection errors)
        {
            return ActiveUp.Net.Mail.SmtpClient.SendCollection(messages, host, 25, ref errors);
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host,
                                                       ref SmtpExceptionCollection errors, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringSmtpExceptionCollection = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionStringSmtpExceptionCollection.BeginInvoke(messages, host,
                                                                                                      ref errors,
                                                                                                      callback,
                                                                                                      SmtpClient
                                                                                                          ._delegateSendMessageCollectionStringSmtpExceptionCollection);
        }

        /// <summary>
        /// Sends all messages using the specified host.
        /// </summary>
        /// <param name="servers">Servers to be used to send the message (in preference order).</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(messages,servers);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("mail.myhost.com",25)
        /// servers.Add("mail2.myhost.com",25)
        /// 
        /// SmtpClient.Send(messages,servers)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(messages,servers);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, ServerCollection servers)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    for (int i = 0; i < servers.Count; i++)
                    {
                        try
                        {
                            if (servers[i].ServerEncryptionType != EncryptionType.None)
                            {
#if !PocketPC
                                smtp.ConnectSsl(servers[i].Host, servers[i].Port);
#else
                                            smtp.Connect(servers[i].Host, servers[i].Port);
#endif
                            }
                            else
                            {
                                smtp.ConnectPlain(servers[i].Host, servers[i].Port);
                            }
                            try
                            {
                                smtp.Ehlo(System.Net.Dns.GetHostName());
                            }
                            catch
                            {
                                smtp.Helo(System.Net.Dns.GetHostName());
                            }
                            if (servers[i].Username != null && servers[i].Username.Length > 0 &&
                                servers[i].Password != null && servers[i].Password.Length > 0)
                                smtp.Authenticate(servers[i].Username, servers[i].Password, SaslMechanism.Login);
                            if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                            else smtp.MailFrom(message.Sender);
                            smtp.RcptTo(message.To);
                            smtp.RcptTo(message.Cc);
                            smtp.RcptTo(message.Bcc);
                            smtp.Data(message.ToMimeString());
                            smtp.Disconnect();
                            sent++;
                            break;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch (ActiveUp.Net.Mail.SmtpException)
                {
                }
            }
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, ServerCollection servers,
                                                       AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionServerCollection = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionServerCollection.BeginInvoke(messages, servers, callback,
                                                                                         SmtpClient
                                                                                             ._delegateSendMessageCollectionServerCollection);
        }

        /// <summary>
        /// Sends all messages using the specified host.
        /// </summary>
        /// <param name="servers">Servers to be used to send the message (in preference order).</param>
        /// <param name="messages">MessageCollection to be sent.</param>
        /// <param name="errors">Reference to SmtpException object collection where errors occuring during the process will be stored.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(messages,servers,myErrorCollection);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("mail.myhost.com",25)
        /// servers.Add("mail2.myhost.com",25)
        /// 
        /// SmtpClient.Send(messages,servers,myErrorCollection)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("mail.myhost.com",25);
        /// servers.Add("mail2.myhost.com",25);
        /// 
        /// SmtpClient.Send(messages,servers,myErrorCollection);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, ServerCollection servers,
                                         ref SmtpExceptionCollection errors)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    for (int i = 0; i < servers.Count; i++)
                    {
                        try
                        {
                            if (servers[i].ServerEncryptionType != EncryptionType.None)
                            {
#if !PocketPC
                                smtp.ConnectSsl(servers[i].Host, servers[i].Port);
#else
                                            smtp.Connect(servers[i].Host, servers[i].Port);
#endif
                            }
                            else
                            {
                                smtp.ConnectPlain(servers[i].Host, servers[i].Port);
                            }
                            try
                            {
                                smtp.Ehlo(System.Net.Dns.GetHostName());
                            }
                            catch
                            {
                                smtp.Helo(System.Net.Dns.GetHostName());
                            }
                            if (servers[i].Username != null && servers[i].Username.Length > 0 &&
                                servers[i].Password != null && servers[i].Password.Length > 0)
                                smtp.Authenticate(servers[i].Username, servers[i].Password, SaslMechanism.Login);
                            if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                            else smtp.MailFrom(message.Sender);
                            smtp.RcptTo(message.To);
                            smtp.RcptTo(message.Cc);
                            smtp.RcptTo(message.Bcc);
                            smtp.Data(message.ToMimeString());
                            smtp.Disconnect();
                            sent++;
                            break;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch (ActiveUp.Net.Mail.SmtpException ex)
                {
                    errors.Add(ex);
                }
            }
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, ServerCollection servers,
                                                       ref SmtpExceptionCollection errors, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionServerCollectionSmtpExceptionCollection = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionServerCollectionSmtpExceptionCollection.BeginInvoke(
                messages, servers, ref errors, callback,
                SmtpClient._delegateSendMessageCollectionServerCollectionSmtpExceptionCollection);
        }

        /// <summary>
        /// Sends all messages using the specified host and port.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="host">Address of the server to be used.</param>
        /// <param name="port">Port to be used.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",8504);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",8504)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",8504);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host, int port)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.ConnectPlain(host, port);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    smtp.Data(message.ToMimeString());
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException)
                {
                }
            }
            smtp.Disconnect();
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, int port,
                                                       AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringInt = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionStringInt.BeginInvoke(messages, host, port, callback,
                                                                                  SmtpClient
                                                                                      ._delegateSendMessageCollectionStringInt);
        }

        /// <summary>
        /// Sends all messages using the specified host and port.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="host">Address of the server to be used.</param>
        /// <param name="port">Port to be used.</param>
        /// <param name="errors">Reference to an SmtpException Collection to be filled with errors occuring during this process.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",8504,myErrorCollection);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",8504,myErrorCollection)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com",8504,myErrorCollection);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host, int port,
                                         ref SmtpExceptionCollection errors)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.ConnectPlain(host, port);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    smtp.Data(message.ToMimeString());
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException ex)
                {
                    errors.Add(ex);
                }
            }
            smtp.Disconnect();
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, int port,
                                                       ref SmtpExceptionCollection errors, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringIntSmtpExceptionCollection = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionStringIntSmtpExceptionCollection.BeginInvoke(messages, host,
                                                                                                         port,
                                                                                                         ref errors,
                                                                                                         callback,
                                                                                                         SmtpClient
                                                                                                             ._delegateSendMessageCollectionStringIntSmtpExceptionCollection);
        }

        /// <summary>
        /// Sends the message using the specified host and port after authentication.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism">SASL mechanism to be used.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host, string username, string password,
                                         SaslMechanism mechanism)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.ConnectPlain(host, 25);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            smtp.Authenticate(username, password, mechanism);
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    smtp.Data(message.ToMimeString());
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException)
                {
                }
            }
            smtp.Disconnect();
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, string username,
                                                       string password, SaslMechanism mechanism, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringStringStringSaslMechanism = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionStringStringStringSaslMechanism.BeginInvoke(messages, host,
                                                                                                        username,
                                                                                                        password,
                                                                                                        mechanism,
                                                                                                        callback,
                                                                                                        SmtpClient
                                                                                                            ._delegateSendMessageCollectionStringStringStringSaslMechanism);
        }

        /// <summary>
        /// Sends the message using the specified host and port after authentication.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism">SASL mechanism to be used.</param>
        /// <param name="errors">Reference to SmtpException object collection where errors occuring during the process will be stored.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,myErrorCollection);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,myErrorCollection)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,myErrorCollection);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host, string username, string password,
                                         SaslMechanism mechanism, ref SmtpExceptionCollection errors)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.ConnectPlain(host, 25);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            smtp.Authenticate(username, password, mechanism);
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    smtp.Data(message.ToMimeString());
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException ex)
                {
                    errors.Add(ex);
                }
            }
            smtp.Disconnect();
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, string username,
                                                       string password, SaslMechanism mechanism,
                                                       ref SmtpExceptionCollection errors, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringStringStringSaslMechanismSmtpExceptionCollection =
                SmtpClient.SendCollection;
            return
                SmtpClient._delegateSendMessageCollectionStringStringStringSaslMechanismSmtpExceptionCollection
                          .BeginInvoke(messages, host, username, password, mechanism, ref errors, callback,
                                       SmtpClient
                                           ._delegateSendMessageCollectionStringStringStringSaslMechanismSmtpExceptionCollection);
        }

        /// <summary>
        /// Sends the message using the specified host and port after authentication.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism">SASL mechanism to be used.</param>
        /// <param name="port">Port to be used to connect to the specified host.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Message message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// Message message1 = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New Message
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim message1 As New Message
        /// message1.Subject = "Hey David!"
        /// message1.From = New Address("jdoe@myhost.com","John Doe")
        /// message1.To.Add("dclarck@otherhost.com","David Clark")
        /// message1.BodyText.Text = "How you doing ?"
        /// 
        /// Dim messages As New MessageCollection
        /// messages.Add(message)
        /// messages.Add(message1)
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504)
        /// 
        /// JScript.NET
        /// 
        /// var message:Message = new Message();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var message1:Message = new Message();
        /// message1.Subject = "Hey David!";
        /// message1.From = New Address("jdoe@myhost.com","John Doe");
        /// message1.To.Add("dclarck@otherhost.com","David Clark");
        /// message1.BodyText.Text = "How you doing ?";
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// messages.Add(message);
        /// messages.Add(message1);
        /// 
        /// SmtpClient.Send(messages,"mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// </code>
        /// </example>
        public static int SendCollection(MessageCollection messages, string host, int port, string username,
                                         string password, SaslMechanism mechanism)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.ConnectPlain(host, port);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            smtp.Authenticate(username, password, mechanism);
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    smtp.Data(message.ToMimeString());
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException)
                {
                }
            }
            smtp.Disconnect();
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, int port,
                                                       string username, string password, SaslMechanism mechanism,
                                                       AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringIntStringStringSaslMechanism = SmtpClient.SendCollection;
            return SmtpClient._delegateSendMessageCollectionStringIntStringStringSaslMechanism.BeginInvoke(messages,
                                                                                                           host, port,
                                                                                                           username,
                                                                                                           password,
                                                                                                           mechanism,
                                                                                                           callback,
                                                                                                           SmtpClient
                                                                                                               ._delegateSendMessageCollectionStringIntStringStringSaslMechanism);
        }

        /// <summary>
        /// Sends the message using the specified host and port after authentication.
        /// </summary>
        /// <param name="messages">The message collection to be sent.</param>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism">SASL mechanism to be used.</param>
        /// <param name="port">Port to be used to connect to the specified host.</param>
        /// <param name="errors">Reference to SmtpException object collection where errors occuring during the process will be stored.</param>
        /// <returns>Amount of messages successfully sent.</returns>
        public static int SendCollection(MessageCollection messages, string host, int port, string username,
                                         string password, SaslMechanism mechanism, ref SmtpExceptionCollection errors)
        {
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            smtp.ConnectPlain(host, port);
            try
            {
                smtp.Ehlo(System.Net.Dns.GetHostName());
            }
            catch
            {
                smtp.Helo(System.Net.Dns.GetHostName());
            }
            smtp.Authenticate(username, password, mechanism);
            int sent = 0;
            foreach (Message message in messages)
            {
                try
                {
                    // Ensure that the mime part tree is built
                    message.CheckBuiltMimePartTree();

                    if (message.From.Email != string.Empty) smtp.MailFrom(message.From);
                    else smtp.MailFrom(message.Sender);
                    smtp.RcptTo(message.To);
                    smtp.RcptTo(message.Cc);
                    smtp.RcptTo(message.Bcc);
                    smtp.Data(message.ToMimeString());
                    sent++;
                }
                catch (ActiveUp.Net.Mail.SmtpException ex)
                {
                    errors.Add(ex);
                }
            }
            smtp.Disconnect();
            return sent;
        }

        public static IAsyncResult BeginSendCollection(MessageCollection messages, string host, int port,
                                                       string username, string password, SaslMechanism mechanism,
                                                       ref SmtpExceptionCollection errors, AsyncCallback callback)
        {
            SmtpClient._delegateSendMessageCollectionStringIntStringStringSaslMechanismSmtpExceptionCollection =
                SmtpClient.SendCollection;
            return
                SmtpClient._delegateSendMessageCollectionStringIntStringStringSaslMechanismSmtpExceptionCollection
                          .BeginInvoke(messages, host, port, username, password, mechanism, ref errors, callback,
                                       SmtpClient
                                           ._delegateSendMessageCollectionStringIntStringStringSaslMechanismSmtpExceptionCollection);
        }

        public static int EndSendCollection(IAsyncResult result)
        {
            return
                (int)
                result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        public static int EndSendCollection(ref SmtpExceptionCollection errors, IAsyncResult result)
        {
            return
                (int)
                result.AsyncState.GetType()
                      .GetMethod("EndInvoke")
                      .Invoke(result.AsyncState, new object[] {errors, result});
        }

        #endregion

        #region Quick Direct Send

        /// <summary>
        /// This static method allows to send an email in 1 line of code.
        /// </summary>
        /// <param name="from">The email address of the person sending the message.</param>
        /// <param name="to">The email address of the message's recipient.</param>
        /// <param name="subject">The message's subject.</param>
        /// <param name="textBody">The text body of the message.</param>
        /// <example>
        /// C#
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!");
        ///
        /// VB.NET
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!")
        /// 
        /// JScript.NET
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!");
        /// </example>
        public void QuickDirectSend(string from, string to, string subject, string textBody)
        {
            QuickSend(from, to, subject, textBody, BodyFormat.Text, string.Empty,
                                                   string.Empty);
            /*ActiveUp.Net.Mail.Message message = new ActiveUp.Net.Mail.Message();
            //message.From.Add(new ActiveUp.Net.Mail.Address(from));
            message.From = new ActiveUp.Net.Mail.Address(from);
            message.To.Add(to);
            message.Subject = subject;
            message.BodyText.Text = textBody;
            message.DirectSend();*/
        }

        public IAsyncResult BeginQuickDirectSend(string from, string to, string subject, string textBody,
                                                        AsyncCallback callback)
        {
            _delegateQuickDirectSend = QuickDirectSend;
            return _delegateQuickDirectSend.BeginInvoke(from, to, subject, textBody, callback,
                                                                   _delegateQuickDirectSend);
        }

        /// <summary>
        /// This static method allows to send an email in 1 line of code.
        /// </summary>
        /// <param name="from">The email address of the person sending the message.</param>
        /// <param name="to">The email address of the message's recipient.</param>
        /// <param name="subject">The message's subject.</param>
        /// <param name="textBody">The text body of the message.</param>
        /// <param name="attachmentPath">The path to a file to be attached to the message.</param>
        /// <example>
        /// C#
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","C:\\My Documents\\file.doc");
        ///
        /// VB.NET
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","C:\My Documents\file.doc")
        /// 
        /// JScript.NET
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","C:\\My Documents\\file.doc");
        /// </example>
        public void QuickDirectSend(string from, string to, string subject, string textBody,
                                           string attachmentPath)
        {
            QuickSend(from, to, subject, textBody, BodyFormat.Text, attachmentPath,
                                                   string.Empty);
            /*ActiveUp.Net.Mail.Message message = new ActiveUp.Net.Mail.Message();
            //message.From.Add(new ActiveUp.Net.Mail.Address(from));
            message.From = new ActiveUp.Net.Mail.Address(from);
            message.To.Add(to);
            message.Subject = subject;
            message.BodyText.Text = textBody;
            message.Attachments.Add(attachmentPath,false);
            message.DirectSend();*/
        }

        public IAsyncResult BeginQuickDirectSend(string from, string to, string subject, string textBody,
                                                        string attachmentPath, AsyncCallback callback)
        {
            SmtpClient._delegateQuickDirectSendAttach = QuickDirectSend;
            return SmtpClient._delegateQuickDirectSendAttach.BeginInvoke(from, to, subject, textBody, BodyFormat.Text,
                                                                         attachmentPath, callback,
                                                                         SmtpClient._delegateQuickDirectSendAttach);
        }

        public static void EndQuickDirectSend(IAsyncResult result)
        {
            result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        /// <summary>
        /// This static method allows to send an email in 1 line of code.
        /// </summary>
        /// <param name="from">The email address of the person sending the message.</param>
        /// <param name="to">The email address of the message's recipient.</param>
        /// <param name="subject">The message's subject.</param>
        /// <param name="body">The text body of the message.</param>
        /// <param name="bodyFormat">The body format of the message.</param>
        /// <param name="attachmentPath">The path to a file to be attached to the message.</param>
        /// <example>
        /// C#
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!",BodyFormat.Text,"C:\\My Documents\\file.doc");
        ///
        /// VB.NET
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!",BodyFormat.Text,"C:\My Documents\file.doc")
        /// 
        /// JScript.NET
        /// 
        /// SmtpClient.QuickDirectSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!",BodyFormat.Text,"C:\\My Documents\\file.doc");
        /// </example>
        public void QuickDirectSend(string from, string to, string subject, string body, BodyFormat bodyFormat,
                                           string attachmentPath)
        {
            QuickSend(from, to, subject, body, bodyFormat, attachmentPath, string.Empty);
            /*ActiveUp.Net.Mail.Message message = new ActiveUp.Net.Mail.Message();
            //message.From.Add(new ActiveUp.Net.Mail.Address(from));
            message.From = new ActiveUp.Net.Mail.Address(from);
            message.To.Add(to);
            message.Subject = subject;
            if (bodyFormat == BodyFormat.Text)
                message.BodyText.Text = body;
            else
                message.BodyHtml.Text = body;
            if (!string.IsNullOrEmpty(attachmentPath))
                message.Attachments.Add(attachmentPath, false);
            message.DirectSend();*/
        }

        public IAsyncResult BeginQuickDirectSend(string from, string to, string subject, string body,
                                                        BodyFormat bodyFormat, string attachmentPath,
                                                        AsyncCallback callback)
        {
            SmtpClient._delegateQuickDirectSendAttach = QuickDirectSend;
            return SmtpClient._delegateQuickDirectSendAttach.BeginInvoke(from, to, subject, body, bodyFormat,
                                                                         attachmentPath, callback,
                                                                         SmtpClient._delegateQuickDirectSendAttach);
        }

        #endregion

        #region Quick Send

        /// <summary>
        /// This static method allows to send an email in 1 line of code.
        /// </summary>
        /// <param name="from">The email address of the person sending the message.</param>
        /// <param name="to">The email address of the message's recipient.</param>
        /// <param name="subject">The message's subject.</param>
        /// <param name="textBody">The text body of the message.</param>
        /// <param name="smtpServer"></param>
        /// <example>
        /// C#
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","mail.myhost.com");
        ///
        /// VB.NET
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","mail.myhost.com");
        /// </example>
        public void QuickSend(string from, string to, string subject, string textBody, string smtpServer)
        {
           QuickSend(from, to, subject, textBody, BodyFormat.Text, string.Empty,
                                                   smtpServer);
            /*ActiveUp.Net.Mail.Message message = new ActiveUp.Net.Mail.Message();
            message.From = new ActiveUp.Net.Mail.Address(from);
            message.To.Add(to);
            message.Subject = subject;
            message.BodyText.Text = textBody;
            message.Send(smtpServer);*/
        }

        public IAsyncResult BeginQuickSend(string from, string to, string subject, string textBody,
                                                  string smtpServer, AsyncCallback callback)
        {
            SmtpClient._delegateQuickSend = QuickSend;
            return SmtpClient._delegateQuickSend.BeginInvoke(from, to, subject, textBody, smtpServer, callback,
                                                             SmtpClient._delegateQuickSend);
        }

        /// <summary>
        /// This static method allows to send an email in 1 line of code.
        /// </summary>
        /// <param name="from">The email address of the person sending the message.</param>
        /// <param name="to">The email address of the message's recipient.</param>
        /// <param name="subject">The message's subject.</param>
        /// <param name="textBody">The text body of the message.</param>
        /// <param name="attachmentPath"></param>
        /// <param name="smtpServer"></param>
        /// <example>
        /// C#
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","C:\\My Documents\\file.doc","mail.myhost.com");
        ///
        /// VB.NET
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","C:\My Documents\file.doc","mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!","C:\\My Documents\\file.doc","mail.myhost.com");
        /// </example>
        public void QuickSend(string from, string to, string subject, string textBody, string attachmentPath,
                                     string smtpServer)
        {
            QuickSend(from, to, subject, textBody, BodyFormat.Text, attachmentPath,
                                                   smtpServer);
            /*ActiveUp.Net.Mail.Message message = new ActiveUp.Net.Mail.Message();
            message.From = new ActiveUp.Net.Mail.Address(from);
            message.To.Add(to);
            message.Subject = subject;
            message.BodyText.Text = textBody;
            message.Attachments.Add(attachmentPath,false);
            message.Send(smtpServer);*/
        }

        public IAsyncResult BeginQuickSend(string from, string to, string subject, string textBody,
                                                  string attachmentPath, string smtpServer, AsyncCallback callback)
        {
            _delegateQuickSendAttach = QuickSend;
            return _delegateQuickSendAttach.BeginInvoke(from, to, subject, textBody, BodyFormat.Text,
                                                                   attachmentPath, smtpServer, callback,
                                                                   _delegateQuickSendAttach);
        }

        /// <summary>
        /// This static method allows to send an email in 1 line of code.
        /// </summary>
        /// <param name="from">The email address of the person sending the message.</param>
        /// <param name="to">The email address of the message's recipient.</param>
        /// <param name="subject">The message's subject.</param>
        /// <param name="body">The text body of the message.</param>
        /// <param name="bodyFormat">The body format of the message.</param>
        /// <param name="attachmentPath"></param>
        /// <param name="smtpServer"></param>
        /// <example>
        /// C#
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!",BodyFormat.Text,"C:\\My Documents\\file.doc","mail.myhost.com");
        ///
        /// VB.NET
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!",BodyFormat.Text,"C:\My Documents\file.doc","mail.myhost.com")
        /// 
        /// JScript.NET
        /// 
        /// SmtpClient.QuickSend("jdoe@myhost.com","mjohns@otherhost.com","Test","Hello this is a test!",BodyFormat.Text,"C:\\My Documents\\file.doc","mail.myhost.com");
        /// </example>
        public void QuickSend(string from, string to, string subject, string body, BodyFormat bodyFormat,
                                     string attachmentPath, string smtpServer)
        {
            var message = new ActiveUp.Net.Mail.Message();
            message.From = new ActiveUp.Net.Mail.Address(from);
            message.To.Add(to);
            message.Subject = subject;
            if (bodyFormat == BodyFormat.Text)
                message.BodyText.Text = body;
            else
                message.BodyHtml.Text = body;
            if (!string.IsNullOrEmpty(attachmentPath))
                message.Attachments.Add(attachmentPath, false);
            if (!string.IsNullOrEmpty(smtpServer))
                Send(message, smtpServer);
            else
                DirectSend(message);
        }

        public static void EndQuickSend(IAsyncResult result)
        {
            result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] {result});
        }

        #endregion

        #endregion

        #endregion

        #endregion

    }

    #endregion
}
