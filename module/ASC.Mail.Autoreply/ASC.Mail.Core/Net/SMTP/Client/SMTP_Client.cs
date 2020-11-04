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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using AUTH;
    using Dns.Client;
    using Mime;
    using TCP;

    #endregion

    /// <summary>
    /// This class implements SMTP client. Defined in RFC 2821.
    /// </summary>
    /// <example>
    /// Simple way:
    /// <code>
    /// /*
    ///  To make this code to work, you need to import following namespaces:
    ///  using LumiSoft.Net.SMTP.Client; 
    /// */
    /// 
    /// // You can send any valid SMTP message here, from disk,memory, ... or
    /// // you can use LumiSoft.Net.Mime mime classes to compose valid SMTP mail message.
    /// 
    /// // SMTP_Client.QuickSendSmartHost(...
    /// or
    /// // SMTP_Client.QuickSend(...
    /// </code>
    /// 
    /// Advanced way:
    /// <code> 
    /// /*
    ///  To make this code to work, you need to import following namespaces:
    ///  using LumiSoft.Net.SMTP.Client; 
    /// */
    /// 
    /// using(SMTP_Client smtp = new SMTP_Client()){
    ///     // If you have registered DNS host name, set it here before connecting.
    ///     // That name will be reported to SMTP server.
    ///     // smtp.LocalHostName = "mail.domain.com";
    ///     
    ///     // You can use SMTP_Client.GetDomainHosts(... to get target receipient SMTP hosts for Connect method.
    ///		smtp.Connect("hostName",WellKnownPorts.SMTP); 
    ///     // Authenticate if target server requires.
    ///     // smtp.Authenticate("user","password");
    ///     smtp.MailFrom("sender@domain.com");
    ///     // Repeat this for all recipients.
    ///     smtp.RcptTo("to@domain.com");
    /// 
    ///     // Send message to server.
    ///     // You can send any valid SMTP message here, from disk,memory, ... or
    ///     // you can use LumiSoft.Net.Mime mieclasses to compose valid SMTP mail message.
    ///     // smtp.SendMessage(.... .
    ///	}
    /// </code>
    /// </example>
    public class SMTP_Client : TCP_Client
    {
        #region Delegates

        /// <summary>
        /// Internal helper method for asynchronous Authenticate method.
        /// </summary>
        private delegate void AuthenticateDelegate(string userName, string password);

        /// <summary>
        /// Internal helper method for asynchronous SendMessage method.
        /// </summary>
        private delegate string[] GetDomainHostsDelegate(string domain);

        /// <summary>
        /// Internal helper method for asynchronous MailFrom method.
        /// </summary>
        private delegate void MailFromDelegate(string from, long messageSize);

        /// <summary>
        /// Internal helper method for asynchronous Noop method.
        /// </summary>
        private delegate void NoopDelegate();

        /// <summary>
        /// Internal helper method for asynchronous RcptTo method.
        /// </summary>
        private delegate void RcptToDelegate(string to);

        /// <summary>
        /// Internal helper method for asynchronous Reset method.
        /// </summary>
        private delegate void ResetDelegate();

        /// <summary>
        /// Internal helper method for asynchronous SendMessage method.
        /// </summary>
        private delegate void SendMessageDelegate(Stream message);

        /// <summary>
        /// Internal helper method for asynchronous StartTLS method.
        /// </summary>
        private delegate void StartTLSDelegate();

        #endregion

        #region Members

        private string m_GreetingText = "";
        private bool m_IsEsmtpSupported;
        private string m_LocalHostName;
        private string m_MailFrom;
        private GenericIdentity m_pAuthdUserIdentity;
        private List<string> m_pEsmtpFeatures;
        private List<string> m_pRecipients;
        private string m_RemoteHostName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets session authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pAuthdUserIdentity;
            }
        }

        /// <summary>
        /// Gets what ESMTP features are supported by connected SMTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is not connected.</exception>
        public string[] EsmtpFeatures
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pEsmtpFeatures.ToArray();
            }
        }

        /// <summary>
        /// Gets greeting text which was sent by SMTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is not connected.</exception>
        public string GreetingText
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_GreetingText;
            }
        }

        /// <summary>
        /// Gets if connected SMTP server suports ESMTP.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is not connected.</exception>
        public bool IsEsmtpSupported
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_IsEsmtpSupported;
            }
        }

        /// <summary>
        /// Gets or sets host name which is reported to SMTP server. If value null, then local computer name is used.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is connected.</exception>
        public string LocalHostName
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LocalHostName;
            }

            set
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (IsConnected)
                {
                    throw new InvalidOperationException(
                        "Property LocalHostName is available only when SMTP client is not connected.");
                }

                m_LocalHostName = value;
            }
        }

        /// <summary>
        /// Gets maximum message size in bytes what SMTP server accepts. Value null means not known.
        /// </summary>
        public long MaxAllowedMessageSize
        {
            get
            {
                try
                {
                    foreach (string feature in EsmtpFeatures)
                    {
                        if (feature.ToUpper().StartsWith(SMTP_ServiceExtensions.SIZE))
                        {
                            return Convert.ToInt64(feature.Split(' ')[1]);
                        }
                    }
                }
                catch
                {
                    // Never should reach here, skip errors here.
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets SMTP server host name which it reported to us.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is not connected.</exception>
        public string RemoteHostName
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_RemoteHostName;
            }
        }

        /// <summary>
        /// Gets SMTP server supported SASL authentication method.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and SMTP client is not connected.</exception>
        public string[] SaslAuthMethods
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                // Search AUTH entry.
                foreach (string feature in EsmtpFeatures)
                {
                    if (feature.ToUpper().StartsWith(SMTP_ServiceExtensions.AUTH))
                    {
                        // Remove AUTH<SP> and split authentication methods.
                        return feature.Substring(4).Trim().Split(' ');
                    }
                }

                return new string[0];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts getting specified email domain SMTP hosts.
        /// </summary>
        /// <param name="domain">Email domain or email address. For example domain.com or user@domain.com.</param>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous method.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>domain</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static IAsyncResult BeginGetDomainHosts(string domain, AsyncCallback callback, object state)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if (string.IsNullOrEmpty(domain))
            {
                throw new ArgumentException(
                    "Invalid argument 'domain' value, you need to specify domain value.");
            }

            GetDomainHostsDelegate asyncMethod = GetDomainHosts;
            AsyncResultState asyncState = new AsyncResultState(null, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(domain, asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous BeginGetDomainHosts request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <returns>Returns specified email domain SMTP hosts.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        public static string[] EndGetDomainHosts(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginGetDomainHosts method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginGetDomainHosts was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is GetDomainHostsDelegate)
            {
                return
                    ((GetDomainHostsDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(
                        castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginGetDomainHosts method.");
            }
        }

        /// <summary>
        /// Gets specified email domain SMTP hosts. Values are in descending priority order.
        /// </summary>
        /// <param name="domain">Domain name. This value can be email address too, then domain parsed automatically.</param>
        /// <returns>Returns specified email domain SMTP hosts.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>domain</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="DNS_ClientException">Is raised when DNS query failure.</exception>
        public static string[] GetDomainHosts(string domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if (string.IsNullOrEmpty(domain))
            {
                throw new ArgumentException(
                    "Invalid argument 'domain' value, you need to specify domain value.");
            }

            // We have email address, parse domain.
            if (domain.IndexOf("@") > -1)
            {
                domain = domain.Substring(domain.IndexOf('@') + 1);
            }

            List<string> retVal = new List<string>();

            // Get MX records.
            Dns_Client dns = new Dns_Client();
            DnsServerResponse response = dns.Query(domain, QTYPE.MX);
            if (response.ResponseCode == RCODE.NO_ERROR)
            {
                foreach (DNS_rr_MX mx in response.GetMXRecords())
                {
                    // Block invalid MX records.
                    if (!string.IsNullOrEmpty(mx.Host))
                    {
                        retVal.Add(mx.Host);
                    }
                }
            }
            else
            {
                throw new DNS_ClientException(response.ResponseCode);
            }

            /* RFC 2821 5.
			    If no MX records are found, but an A RR is found, the A RR is treated as if it 
                was associated with an implicit MX RR, with a preference of 0, pointing to that host.
			*/
            if (retVal.Count == 0)
            {
                retVal.Add(domain);
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Sends specified mime message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b> is null.</exception>
        public static void QuickSend(Mime message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            string from = "";
            if (message.MainEntity.From != null && message.MainEntity.From.Count > 0)
            {
                from = ((MailboxAddress) message.MainEntity.From[0]).EmailAddress;
            }

            List<string> recipients = new List<string>();
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

                // We must hide BCC
                message.MainEntity.Bcc.Clear();
            }

            foreach (string recipient in recipients)
            {
                QuickSend(null, from, recipient, new MemoryStream(message.ToByteData()));
            }
        }

        /// <summary>
        /// Sends message directly to email domain. Domain email sever resolve order: MX recordds -> A reords if no MX.
        /// </summary>
        /// <param name="from">Sender email what is reported to SMTP server.</param>
        /// <param name="to">Recipient email.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>from</b>,<b>to</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public static void QuickSend(string from, string to, Stream message)
        {
            QuickSend(null, from, to, message);
        }

        /// <summary>
        /// Sends message directly to email domain. Domain email sever resolve order: MX recordds -> A reords if no MX.
        /// </summary>
        /// <param name="localHost">Host name which is reported to SMTP server.</param>
        /// <param name="from">Sender email what is reported to SMTP server.</param>
        /// <param name="to">Recipient email.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>from</b>,<b>to</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public static void QuickSend(string localHost, string from, string to, Stream message)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (from != "" && !SMTP_Utils.IsValidAddress(from))
            {
                throw new ArgumentException("Argument 'from' has invalid value.");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (to == "")
            {
                throw new ArgumentException("Argument 'to' value must be specified.");
            }
            if (!SMTP_Utils.IsValidAddress(to))
            {
                throw new ArgumentException("Argument 'to' has invalid value.");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            QuickSendSmartHost(localHost, GetDomainHosts(to)[0], 25, false, from, new[] {to}, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="from">Sender email what is reported to SMTP server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public static void QuickSendSmartHost(string host, int port, string from, string[] to, Stream message)
        {
            QuickSendSmartHost(null, host, port, false, null, null, from, to, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="from">Sender email what is reported to SMTP server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public static void QuickSendSmartHost(string host,
                                              int port,
                                              bool ssl,
                                              string from,
                                              string[] to,
                                              Stream message)
        {
            QuickSendSmartHost(null, host, port, ssl, null, null, from, to, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="localHost">Host name which is reported to SMTP server.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="from">Sender email what is reported to SMTP server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public static void QuickSendSmartHost(string localHost,
                                              string host,
                                              int port,
                                              bool ssl,
                                              string from,
                                              string[] to,
                                              Stream message)
        {
            QuickSendSmartHost(localHost, host, port, ssl, null, null, from, to, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="localHost">Host name which is reported to SMTP server.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="userName">SMTP server user name. This value may be null, then authentication not used.</param>
        /// <param name="password">SMTP server password.</param>
        /// <param name="from">Sender email what is reported to SMTP server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public static void QuickSendSmartHost(string localHost,
                                              string host,
                                              int port,
                                              bool ssl,
                                              string userName,
                                              string password,
                                              string from,
                                              string[] to,
                                              Stream message)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (host == "")
            {
                throw new ArgumentException("Argument 'host' value may not be empty.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value must be >= 1.");
            }
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (from != "" && !SMTP_Utils.IsValidAddress(from))
            {
                throw new ArgumentException("Argument 'from' has invalid value.");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (to.Length == 0)
            {
                throw new ArgumentException("Argument 'to' must contain at least 1 recipient.");
            }
            foreach (string t in to)
            {
                if (!SMTP_Utils.IsValidAddress(t))
                {
                    throw new ArgumentException("Argument 'to' has invalid value '" + t + "'.");
                }
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            using (SMTP_Client smtp = new SMTP_Client())
            {
                if (!string.IsNullOrEmpty(localHost))
                {
                    smtp.LocalHostName = localHost;
                }
                smtp.Connect(host, port, ssl);
                if (!string.IsNullOrEmpty(userName))
                {
                    smtp.Authenticate(userName, password);
                }
                smtp.MailFrom(from, -1);
                foreach (string t in to)
                {
                    smtp.RcptTo(t);
                }
                smtp.SendMessage(message);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Closes connection to SMTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        public override void Disconnect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("SMTP client is not connected.");
            }

            try
            {
                // Send QUIT command to server.                
                WriteLine("QUIT");
            }
            catch {}

            m_LocalHostName = null;
            m_RemoteHostName = null;
            m_GreetingText = "";
            m_IsEsmtpSupported = false;
            m_pEsmtpFeatures = null;
            m_MailFrom = null;
            m_pRecipients = null;
            m_pAuthdUserIdentity = null;

            try
            {
                base.Disconnect();
            }
            catch {}
        }

        /// <summary>
        /// Starts sending NOOP command to server. This method can be used for keeping connection alive(not timing out).
        /// </summary>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        public IAsyncResult BeginNoop(AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            NoopDelegate asyncMethod = Noop;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous Noop request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndNoop(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument asyncResult was not returned by a call to the BeginNoop method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginNoop was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is NoopDelegate)
            {
                ((NoopDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginNoop method.");
            }
        }

        /// <summary>
        /// Send NOOP command to server. This method can be used for keeping connection alive(not timing out).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void Noop()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            /* RFC 2821 4.1.1.9 NOOP.
                This command does not affect any parameters or previously entered
                commands.  It specifies no action other than that the receiver send
                an OK reply.

                Syntax:
                    "NOOP" [ SP String ] CRLF
            */

            WriteLine("NOOP");

            string line = ReadLine();
            if (!line.StartsWith("250"))
            {
                throw new SMTP_ClientException(line);
            }
        }

        /// <summary>
        /// Starts switching to SSL.
        /// </summary>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected or is already secure connection.</exception>
        public IAsyncResult BeginStartTLS(AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsSecureConnection)
            {
                throw new InvalidOperationException("Connection is already secure.");
            }

            StartTLSDelegate asyncMethod = StartTLS;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous StartTLS request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndStartTLS(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginReset was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is StartTLSDelegate)
            {
                ((StartTLSDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
        }

        /// <summary>
        /// Switches SMTP connection to SSL.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected or is already secure connection.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
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

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsSecureConnection)
            {
                throw new InvalidOperationException("Connection is already secure.");
            }

            WriteLine("STARTTLS");

            string line = ReadLine();
            if (!line.ToUpper().StartsWith("220"))
            {
                throw new SMTP_ClientException(line);
            }

            SwitchToSecure();
        }

        /// <summary>
        /// Starts authentication.
        /// </summary>
        /// <param name="userName">User login name.</param>
        /// <param name="password">Password.</param>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected or is already authenticated.</exception>
        public IAsyncResult BeginAuthenticate(string userName,
                                              string password,
                                              AsyncCallback callback,
                                              object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException("Session is already authenticated.");
            }

            AuthenticateDelegate asyncMethod = Authenticate;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(userName,
                                                              password,
                                                              asyncState.CompletedCallback,
                                                              null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous authentication request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndAuthenticate(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument 'asyncResult' was not returned by a call to the BeginAuthenticate method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginAuthenticate was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is AuthenticateDelegate)
            {
                ((AuthenticateDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(
                    castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginAuthenticate method.");
            }
        }

        /// <summary>
        /// Authenticates user. Authenticate method chooses strongest possible authentication method supported by server, 
        /// preference order DIGEST-MD5 -> CRAM-MD5 -> LOGIN.
        /// </summary>
        /// <param name="userName">User login name.</param>
        /// <param name="password">Password.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected or is already authenticated.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>userName</b> is null.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void Authenticate(string userName, string password)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException("Session is already authenticated.");
            }
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }
            if (password == null)
            {
                password = "";
            }

            // Choose authentication method, we consider LOGIN as default.
            string authMethod = "LOGIN";
            List<string> authMethods = new List<string>(SaslAuthMethods);
            if (authMethods.Contains("DIGEST-MD5"))
            {
                authMethod = "DIGEST-MD5";
            }
            else if (authMethods.Contains("CRAM-MD5"))
            {
                authMethod = "CRAM-MD5";
            }

            #region AUTH LOGIN

            if (authMethod == "LOGIN")
            {
                /* LOGIN
			          Example:
			            C: AUTH LOGIN<CRLF>
			            S: 334 VXNlcm5hbWU6<CRLF>   VXNlcm5hbWU6 = base64("USERNAME")
			            C: base64(username)<CRLF>
			            S: 334 UGFzc3dvcmQ6<CRLF>   UGFzc3dvcmQ6 = base64("PASSWORD")
			            C: base64(password)<CRLF>
			            S: 235 Ok<CRLF>
			    */

                WriteLine("AUTH LOGIN");

                // Read server response.
                string line = ReadLine();
                // Response line must start with 334 or otherwise it's error response.
                if (!line.StartsWith("334"))
                {
                    throw new SMTP_ClientException(line);
                }

                // Send user name to server.
                WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(userName)));

                // Read server response.
                line = ReadLine();
                // Response line must start with 334 or otherwise it's error response.
                if (!line.StartsWith("334"))
                {
                    throw new SMTP_ClientException(line);
                }

                // Send password to server.
                WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(password)));

                // Read server response.
                line = ReadLine();
                // Response line must start with 334 or otherwise it's error response.
                if (!line.StartsWith("235"))
                {
                    throw new SMTP_ClientException(line);
                }

                m_pAuthdUserIdentity = new GenericIdentity(userName, "LOGIN");
            }

                #endregion

                #region AUTH CRAM-MD5

            else if (authMethod == "CRAM-MD5")
            {
                /* CRAM-M5
                    Description:
                        HMACMD5 key is "password".
                 
			        Example:
					    C: AUTH CRAM-MD5<CRLF>
					    S: 334 base64(md5_calculation_hash)<CRLF>
					    C: base64(username password_hash)<CRLF>
					    S: 235 Ok<CRLF>
			    */

                WriteLine("AUTH CRAM-MD5");

                // Read server response.
                string line = ReadLine();
                // Response line must start with 334 or otherwise it's error response.
                if (!line.StartsWith("334"))
                {
                    throw new SMTP_ClientException(line);
                }

                HMACMD5 kMd5 = new HMACMD5(Encoding.ASCII.GetBytes(password));
                string passwordHash =
                    Core.ToHexString(kMd5.ComputeHash(Convert.FromBase64String(line.Split(' ')[1]))).ToLower();

                // Send authentication info to server.
                WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + " " + passwordHash)));

                // Read server response.
                line = ReadLine();
                // Response line must start with 235 or otherwise it's error response
                if (!line.StartsWith("235"))
                {
                    throw new SMTP_ClientException(line);
                }

                m_pAuthdUserIdentity = new GenericIdentity(userName, "CRAM-MD5");
            }

                #endregion

                #region AUTH DIGEST-MD5

            else if (authMethod == "DIGEST-MD5")
            {
                /*
                    Example:
					    C: AUTH DIGEST-MD5<CRLF>
					    S: 334 base64(digestChallange)<CRLF>
					    C: base64(digestAuthorization)<CRLF>
                        S: 334 base64(serverResponse)<CRLF>
                        C: <CRLF>
					    S: 235 Ok<CRLF>
                */

                WriteLine("AUTH DIGEST-MD5");

                // Read server response.
                string line = ReadLine();
                // Response line must start with 334 or otherwise it's error response.
                if (!line.StartsWith("334"))
                {
                    throw new SMTP_ClientException(line);
                }

                Auth_HttpDigest digestmd5 =
                    new Auth_HttpDigest(
                        Encoding.Default.GetString(Convert.FromBase64String(line.Split(' ')[1])),
                        "AUTHENTICATE");
                digestmd5.CNonce = Auth_HttpDigest.CreateNonce();
                digestmd5.Uri = "smtp/" + digestmd5.Realm;
                digestmd5.UserName = userName;
                digestmd5.Password = password;

                // Send authentication info to server.
                WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(digestmd5.ToAuthorization(false))));

                // Read server response.
                line = ReadLine();
                // Response line must start with 334 or otherwise it's error response.
                if (!line.StartsWith("334"))
                {
                    throw new SMTP_ClientException(line);
                }

                // Send empty line.
                WriteLine("");

                // Read server response.
                line = ReadLine();
                // Response line must start with 235 or otherwise it's error response.
                if (!line.StartsWith("235"))
                {
                    throw new SMTP_ClientException(line);
                }

                m_pAuthdUserIdentity = new GenericIdentity(userName, "DIGEST-MD5");
            }

            #endregion
        }

        /// <summary>
        /// Starts sending MAIL FROM: command to SMTP server.
        /// </summary>
        /// <param name="from">Sender email address reported to SMTP server.</param>
        /// <param name="messageSize">Sendable message size in bytes, -1 if message size unknown.</param>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous disconnect.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        public IAsyncResult BeginMailFrom(string from, long messageSize, AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            MailFromDelegate asyncMethod = MailFrom;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(from,
                                                              messageSize,
                                                              asyncState.CompletedCallback,
                                                              null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous MailFrom request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndMailFrom(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginReset was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is MailFromDelegate)
            {
                ((MailFromDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
        }

        /// <summary>
        /// Sends MAIL FROM: command to SMTP server.
        /// </summary>
        /// <param name="from">Sender email address reported to SMTP server.</param>
        /// <param name="messageSize">Sendable message size in bytes, -1 if message size unknown.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void MailFrom(string from, long messageSize)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!SMTP_Utils.IsValidAddress(from))
            {
                throw new ArgumentException("Argument from has invalid value.");
            }

            /* RFC 2821 4.1.1.2 MAIL
			    Examples:
			 		MAIL FROM:<ivar@lumisoft.ee>
			  
			   RFC 1870 adds optional SIZE keyword support.
			    SIZE keyword may only be used if it's reported in EHLO command response.
			 	 Examples:
			 		MAIL FROM:<ivx@lumisoft.ee> SIZE=1000
			*/

            bool isSizeSupported = false;
            foreach (string feature in m_pEsmtpFeatures)
            {
                if (feature.ToLower() == "size")
                {
                    isSizeSupported = true;
                    break;
                }
            }

            string line = "MAIL FROM:<" + from + ">";
            if (isSizeSupported && messageSize > 0)
            {
                line = "MAIL FROM:<" + from + "> SIZE=" + messageSize;
            }
            WriteLine(line);

            // Read first line of reply, check if it's ok.
            line = ReadLine();
            if (!line.StartsWith("250"))
            {
                throw new SMTP_ClientException(line);
            }

            m_MailFrom = from;
        }

        /// <summary>
        /// Starts sending RCPT TO: command to SMTP server.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous disconnect.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        public IAsyncResult BeginRcptTo(string to, AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            RcptToDelegate asyncMethod = RcptTo;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(to, asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous RcptTo request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndRcptTo(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginReset was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is RcptToDelegate)
            {
                ((RcptToDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
        }

        /// <summary>
        /// Sends RCPT TO: command to SMTP server.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void RcptTo(string to)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!SMTP_Utils.IsValidAddress(to))
            {
                throw new ArgumentException("Argument from has invalid value.");
            }

            /* RFC 2821 4.1.1.2 RCPT.
                Syntax:
                    "RCPT TO:" ("<Postmaster@" domain ">" / "<Postmaster>" / Forward-Path) [SP Rcpt-parameters] CRLF

			    Examples:
			 		RCPT TO:<ivar@lumisoft.ee>
			*/

            WriteLine("RCPT TO:<" + to + ">");

            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (!line.StartsWith("250"))
            {
                throw new SMTP_ClientException(line);
            }

            if (!m_pRecipients.Contains(to))
            {
                m_pRecipients.Add(to);
            }
        }

        /// <summary>
        /// Starts resetting SMTP session, all state data will be deleted.
        /// </summary>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous disconnect.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        public IAsyncResult BeginReset(AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            ResetDelegate asyncMethod = Reset;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous reset request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndReset(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginReset was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is ResetDelegate)
            {
                ((ResetDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
        }

        /// <summary>
        /// Resets SMTP session, all state data will be deleted.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void Reset()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            /* RFC 2821 4.1.1.5 RESET (RSET).
                This command specifies that the current mail transaction will be
                aborted.  Any stored sender, recipients, and mail data MUST be
                discarded, and all buffers and state tables cleared.  The receiver
                MUST send a "250 OK" reply to a RSET command with no arguments.  A
                reset command may be issued by the client at any time.
            */

            WriteLine("RSET");

            string line = ReadLine();
            if (!line.StartsWith("250"))
            {
                throw new SMTP_ClientException(line);
            }

            m_MailFrom = null;
            m_pRecipients.Clear();
        }

        /// <summary>
        /// Starts sending specified raw message to SMTP server.
        /// </summary>
        /// <param name="message">Message stream. Message will be readed from current stream position and to the end of stream.</param>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous method.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b> is null.</exception>
        public IAsyncResult BeginSendMessage(Stream message, AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            SendMessageDelegate asyncMethod = SendMessage;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(message, asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous SendMessage request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void EndSendMessage(IAsyncResult asyncResult)
        {
            if (IsDisposed)
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
                    "Argument asyncResult was not returned by a call to the BeginSendMessage method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginSendMessage was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is SendMessageDelegate)
            {
                ((SendMessageDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(
                    castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginSendMessage method.");
            }
        }

        /// <summary>
        /// Sends specified raw message to SMTP server.
        /// </summary>
        /// <param name="message">Message stream. Message will be readed from current stream position and to the end of stream.</param>
        /// <remarks>The stream must contain data in MIME format, other formats normally are rejected by SMTP server.</remarks>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when SMTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b> is null.</exception>
        /// <exception cref="SMTP_ClientException">Is raised when SMTP server returns error.</exception>
        public void SendMessage(Stream message)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            // See if BDAT supported.
            bool bdatSupported = false;
            foreach (string feature in EsmtpFeatures)
            {
                if (feature.ToUpper() == SMTP_ServiceExtensions.CHUNKING)
                {
                    bdatSupported = true;
                    break;
                }
            }

            #region DATA

            if (!bdatSupported)
            {
                /* RFC 2821 4.1.1.4 DATA
			        Notes:
			 		    Message must be period handled for DATA command. This meas if message line starts with .,
			 		    additional .(period) must be added.
			 		    Message send is ended with <CRLF>.<CRLF>.
                
			 	    Examples:
			 		    C: DATA<CRLF>
			 		    S: 354 Start sending message, end with <crlf>.<crlf><CRLF>
			 		    C: send_message
			 		    C: <CRLF>.<CRLF>
                        S: 250 Ok<CRLF>
			    */

                WriteLine("DATA");

                string line = ReadLine();
                if (line.StartsWith("354"))
                {
                    long writtenCount = TcpStream.WritePeriodTerminated(message);
                    LogAddWrite(writtenCount, "Wrote " + writtenCount + " bytes.");

                    // Read server reply.
                    line = ReadLine();
                    if (!line.StartsWith("250"))
                    {
                        throw new SMTP_ClientException(line);
                    }
                }
                else
                {
                    throw new SMTP_ClientException(line);
                }
            }

                #endregion

                #region BDAT

            else
            {
                /* RFC 3030 BDAT
			 	    Syntax:
                        BDAT<SP>ChunkSize[<SP>LAST]<CRLF>
			 	
			 	    Exapmle:
    			 		C: BDAT 1000 LAST<CRLF>
	    		 		C: send_1000_byte_message
		    	 		S: 250 OK<CRLF>			  
			    */

                // TODO: Get rid of BDAT 0 LAST, this is valid syntax but many servers can't handle it.
                // We just read 1 buffer ahead, then you see when source stream has EOS.
                byte[] buffer1 = new byte[16000];
                byte[] buffer2 = new byte[16000];
                byte[] currentBuffer = buffer1;
                byte[] lastBuffer = buffer2;
                int lastReadedCount = 0;

                // Buffer first data block.
                lastReadedCount = message.Read(lastBuffer, 0, lastBuffer.Length);

                while (true)
                {
                    // Read data block to free buffer.
                    int readedCount = message.Read(currentBuffer, 0, currentBuffer.Length);

                    // End of stream reached, "last data block" is last one.
                    if (readedCount == 0)
                    {
                        WriteLine("BDAT " + lastReadedCount + " LAST");
                        TcpStream.Write(lastBuffer, 0, lastReadedCount);
                        LogAddWrite(readedCount, "Wrote " + lastReadedCount + " bytes.");

                        // Read server response.
                        string line = ReadLine();
                        if (!line.StartsWith("250"))
                        {
                            throw new SMTP_ClientException(line);
                        }

                        // We are done, exit while.
                        break;
                    }
                        // Send last data block, free it up for reuse.
                    else
                    {
                        WriteLine("BDAT " + lastReadedCount);
                        TcpStream.Write(lastBuffer, 0, lastReadedCount);
                        LogAddWrite(readedCount, "Wrote " + lastReadedCount + " bytes.");

                        // Read server response.
                        string line = ReadLine();
                        if (!line.StartsWith("250"))
                        {
                            throw new SMTP_ClientException(line);
                        }

                        // Mark last buffer as current(free it up), just mark current buffer as last for next while cycle.
                        byte[] tmp = lastBuffer;
                        lastBuffer = currentBuffer;
                        currentBuffer = tmp;
                    }

                    lastReadedCount = readedCount;
                }
            }

            #endregion
        }

        #endregion

        #region Overrides

        /// <summary>
        /// This method is called after TCP client has sucessfully connected.
        /// </summary>
        protected override void OnConnected()
        {
            /*
			  Notes: Greeting may be single or multiline response.
			 		
			  Examples:
			 	220<SP>SMTP server ready<CRLF> 
			  
			 	220-SMTP server ready<CRLF>
			 	220-Addtitional text<CRLF>
			 	220<SP>final row<CRLF>			  
			*/

            StringBuilder response = new StringBuilder();
            string line = ReadLine();
            response.AppendLine(line);
            // Read multiline response.
            while (line.Length >= 4 && line[3] == '-')
            {
                line = ReadLine();
                response.AppendLine(line);
            }

            if (line.StartsWith("220"))
            {
                m_GreetingText = response.ToString();
            }
            else
            {
                throw new SMTP_ClientException(response.ToString());
            }

            #region EHLO/HELO

            // If local host name not specified, get local computer name.
            string localHostName = m_LocalHostName;
            if (string.IsNullOrEmpty(localHostName))
            {
                localHostName = Dns.GetHostName();
            }

            // Do EHLO/HELO.
            WriteLine("EHLO " + localHostName);

            // Read server response.
            line = ReadLine();
            if (line.StartsWith("250"))
            {
                m_IsEsmtpSupported = true;

                /* RFC 2821 4.1.1.1 EHLO
					Examples:
                        C: EHLO domain<CRLF>
				    	S: 250-domain freeText<CRLF>
				        S: 250-EHLO_keyword<CRLF>
						S: 250 EHLO_keyword<CRLF>
				 
						250<SP> specifies that last EHLO response line.
			    */

                // We may have 250- or 250 SP as domain separator.
                // 250-
                if (line.StartsWith("250-"))
                {
                    m_RemoteHostName = line.Substring(4).Split(new[] {' '}, 2)[0];
                }
                    // 250 SP
                else
                {
                    m_RemoteHostName = line.Split(new[] {' '}, 3)[1];
                }

                m_pEsmtpFeatures = new List<string>();
                // Read multiline response, EHLO keywords.
                while (line.StartsWith("250-"))
                {
                    line = ReadLine();

                    if (line.StartsWith("250-"))
                    {
                        m_pEsmtpFeatures.Add(line.Substring(4));
                    }
                }
            }
                // Probably EHLO not supported, try HELO.
            else
            {
                m_IsEsmtpSupported = false;
                m_pEsmtpFeatures = new List<string>();

                WriteLine("HELO " + localHostName);

                line = ReadLine();
                if (line.StartsWith("250"))
                {
                    /* Rfc 2821 4.1.1.1 EHLO/HELO
			            Syntax: "HELO" SP Domain CRLF
                    
                        Examples:
                            C: HELO domain<CRLF>
                            S: 250 domain freeText<CRLF>
			        */

                    m_RemoteHostName = line.Split(new[] {' '}, 3)[1];
                }
                    // Server rejects us for some reason.
                else
                {
                    throw new SMTP_ClientException(line);
                }
            }

            #endregion

            m_pRecipients = new List<string>();
        }

        #endregion
    }
}