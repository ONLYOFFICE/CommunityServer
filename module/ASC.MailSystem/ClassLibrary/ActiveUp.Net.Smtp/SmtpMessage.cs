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

using ActiveUp.Net.Mail;
using System.Security.Cryptography.X509Certificates;
#if !PocketPC
using System.Security.Cryptography.Pkcs;
#endif
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IO;
using ActiveUp.Net.Security;

namespace ActiveUp.Net.Mail
{
    #region SmtpMessage Object

    /// <summary>
    /// Message Object.
    /// Represents a parsed e-mail message.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class SmtpMessage : Message
    {
        #region Public methods

        /// <summary>
        /// Sends the message using the specified host as the mail exchange server.
        /// </summary>
        /// <param name="host">Host to be used to send the message.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///SmtpMessage message = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.Send("mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        ///Dim message As New SmtpMessage
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com")
        ///  
        /// JScript.NET
        /// 
        ///var message:SmtpMessage = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com");
        /// </code>
        /// </example>
        public void Send(string host)
        {
            //CheckBuiltMimePartTree();
            SmtpClient.Send(this, host);
        }

        public void SendSsl(string host)
        {
            //CheckBuiltMimePartTree();
#if !PocketPC
            SmtpClient.SendSsl(this, host);
#else
            SmtpClient.Send(this, host);
#endif
        }

        public IAsyncResult BeginSend(string host, AsyncCallback callback)
        {
            return SmtpClient.BeginSend(this, host, callback);
        }

        public void EndSend(IAsyncResult result)
        {
            SmtpClient.EndSend(result);
        }

        /// <summary>
        /// Sends the message using the specified host as the mail exchange server.
        /// </summary>
        /// <param name="servers">Servers to be used to send the message (in preference order).</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///SmtpMessage message = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///ServerCollection servers = new ServerCollection();
        ///servers.Add("mail.myhost.com",25);
        ///servers.Add("mail2.myhost.com",25);
        ///
        ///message.Send(servers);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New SmtpMessage
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim servers As New ServerCollection
        ///servers.Add("mail.myhost.com",25)
        ///servers.Add("mail2.myhost.com",25)
        ///
        ///message.Send(servers)
        ///  
        /// JScript.NET
        /// 
        ///var message:SmtpMessage = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var servers:ServerCollection = new ServerCollection();
        ///servers.Add("mail.myhost.com",25);
        ///servers.Add("mail2.myhost.com",25);
        ///
        ///message.Send(servers);
        /// </code>
        /// </example>
        public void Send(ServerCollection servers)
        {
            //CheckBuiltMimePartTree();
            SmtpClient.Send(this, servers);
        }

        public IAsyncResult BeginSend(ServerCollection servers, AsyncCallback callback)
        {
            return SmtpClient.BeginSend(this, servers, callback);
        }

        /// <summary>
        /// Sends the message using the specified host as mail exchange and the specified port.
        /// </summary>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="port">Port to be used to connect to the specified host.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///SmtpMessage message = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.Send("mail.myhost.com",8504);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New SmtpMessage
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com",8504)
        ///  
        /// JScript.NET
        /// 
        ///var message:SmtpMessage = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com",8504);
        /// </code>
        /// </example>
        public void Send(string host, int port)
        {
            //CheckBuiltMimePartTree();
            SmtpClient.Send(this, host, port);
        }

        public void SendSsl(string host, int port)
        {
            //CheckBuiltMimePartTree();
#if !PocketPC
            SmtpClient.SendSsl(this, host, port);
#else
            SmtpClient.Send(this, host, port);
#endif
        }

        public IAsyncResult BeginSend(string host, int port, AsyncCallback callback)
        {
            //CheckBuiltMimePartTree();
            return SmtpClient.BeginSend(this, host, port, callback);
        }

        /// <summary>
        /// Sends the message using the specified host as mail exchange.
        /// A secure SASL authentication is performed according to the specified SASL mechanism.
        /// </summary>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism">SASL Mechanism to be used for authentication.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///SmtpMessage message = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New SmtpMessage
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5)
        ///  
        /// JScript.NET
        /// 
        ///var message:SmtpMessage = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// </code>
        /// </example>
        public void Send(string host, string username, string password, SaslMechanism mechanism)
        {
            //CheckBuiltMimePartTree();
            SmtpClient.Send(this, host, username, password, mechanism);
        }
        public void SendSsl(string host, string username, string password, SaslMechanism mechanism)
        {
            //CheckBuiltMimePartTree();
#if !PocketPC
            SmtpClient.SendSsl(this, host, username, password, mechanism);
#else
            SmtpClient.Send(this, host, username, password, mechanism);
#endif
        }

        public IAsyncResult BeginSend(string host, string username, string password, SaslMechanism mechanism, AsyncCallback callback)
        {
            return SmtpClient.BeginSend(this, host, username, password, mechanism, callback);
        }

        /// <summary>
        /// Sends the message using the specified host as mail exchange and the specified port.
        /// A simple Login authentication is performed.
        /// </summary>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="port">Port to be used to connect to the specified host.</param>
        /// <param name="mechanism"></param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///SmtpMessage message = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New SmtpMessage
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504)
        ///  
        /// JScript.NET
        /// 
        ///var message:SmtpMessage = new SmtpMessage();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// </code>
        /// </example>
        public void Send(string host, int port, string username, string password, SaslMechanism mechanism)
        {
            //CheckBuiltMimePartTree();
            SmtpClient.Send(this, host, port, username, password, mechanism);
        }

        public void SendSsl(string host, int port, string username, string password, SaslMechanism mechanism)
        {
            //CheckBuiltMimePartTree();
#if !PocketPC
            SmtpClient.SendSsl(this, host, port, username, password, mechanism);
#else
            SmtpClient.Send(this, host, port, username, password, mechanism);
#endif
        }

        public IAsyncResult BeginSend(string host, int port, string username, string password, SaslMechanism mechanism, AsyncCallback callback)
        {
            return SmtpClient.BeginSend(this, host, port, username, password, mechanism, callback);
        }

        /// <summary>
        /// Sends the given message using the Direct Mailing method. The client connects to each recipient's mail exchange server and delivers the message.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpMessage message = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// message.DirectSend();
        /// 
        /// VB.NET
        /// 
        /// Dim message As New SmtpMessage
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// message.DirectSend()
        /// 
        /// JScript.NET
        /// 
        /// var message:SmtpMessage = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// message.DirectSend();
        /// </code>
        /// </example>
        public string DirectSend()
        {
            return DirectSend(new ActiveUp.Net.Mail.ServerCollection());
        }

        public IAsyncResult BeginDirectSend(AsyncCallback callback)
        {
            return SmtpClient.BeginDirectSend(this, callback);
        }

        public void EndDirectSend(IAsyncResult result)
        {
            SmtpClient.EndDirectSend(result);
        }

        /// <summary>
        /// Sends the message using the specified host as dns server on the specified port.
        /// </summary>
        /// <param name="dnsHost">The host to be used.</param>
        /// <param name="dnsPort">The port to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpMessage message = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// message.DirectSend("ns1.dnsserver.com",53);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New SmtpMessage
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// message.DirectSend("ns1.dnsserver.com",53)
        /// 
        /// JScript.NET
        /// 
        /// var message:SmtpMessage = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// message.DirectSend("ns1.dnsserver.com",53);
        /// </code>
        /// </example>
        public string DirectSend(string dnsHost, int dnsPort)
        {
            ActiveUp.Net.Mail.ServerCollection servers = new ActiveUp.Net.Mail.ServerCollection();
            servers.Add(dnsHost, dnsPort);
            return DirectSend(servers);
        }

        public IAsyncResult BeginDirectSend(string dnsHost, int dnsPort, AsyncCallback callback)
        {
            return SmtpClient.BeginDirectSend(this, dnsHost, dnsPort, callback);
        }

        /// <summary>
        /// Sends the message using the specified host as dns server.
        /// </summary>
        /// <param name="dnsHost">The host to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpMessage message = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// message.DirectSend("ns1.dnsserver.com");
        /// 
        /// VB.NET
        /// 
        /// Dim message As New SmtpMessage
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// message.DirectSend("ns1.dnsserver.com")
        /// 
        /// JScript.NET
        /// 
        /// var message:SmtpMessage = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// message.DirectSend("ns1.dnsserver.com");
        /// </code>
        /// </example>
        public string DirectSend(string dnsHost)
        {
            return DirectSend(dnsHost, 53);
        }

        public IAsyncResult BeginDirectSend(string dnsHost, AsyncCallback callback)
        {
            return SmtpClient.BeginDirectSend(this, dnsHost, callback);
        }

        /// <summary>
        /// Sends the message using the specified DNS servers to get mail exchange servers addresses.
        /// </summary>
        /// <param name="dnsServers">Servers to be used (in preference order).</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// SmtpMessage message = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// servers.Add("ns1.dnsserver.com",53);
        /// servers.Add("ns2.dnsserver.com",53);
        /// 
        /// message.DirectSend(servers);
        /// 
        /// VB.NET
        /// 
        /// Dim message As New SmtpMessage
        /// message.Subject = "Test"
        /// message.From = New Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.BodyText.Text = "Hello this is a test!"
        /// 
        /// Dim servers As New ServerCollection
        /// servers.Add("ns1.dnsserver.com",53)
        /// servers.Add("ns2.dnsserver.com",53)
        /// 
        /// message.DirectSend(servers)
        /// 
        /// JScript.NET
        /// 
        /// var message:SmtpMessage = new SmtpMessage();
        /// message.Subject = "Test";
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.BodyText.Text = "Hello this is a test!";
        /// 
        /// var servers:ServerCollection = new ServerCollection();
        /// servers.Add("ns1.dnsserver.com",53);
        /// servers.Add("ns2.dnsserver.com",53);
        /// 
        /// message.DirectSend(servers);
        /// </code>
        /// </example>
        public string DirectSend(ServerCollection dnsServers)
        {
            return SmtpClient.DirectSend(this, dnsServers);
        }

        public IAsyncResult BeginDirectSend(ServerCollection dnsServers, AsyncCallback callback)
        {
            return SmtpClient.BeginDirectSend(this, dnsServers, callback);
        }

        /// <summary>
        /// Sends the message using the specified queuing service and spool directory.
        /// </summary>
        /// <param name="spoolDirectory">The full path to the full directory.</param>
        /// <param name="queuingService">The queuing service to use.</param>
        public void SendQueued(string spoolDirectory, QueuingService queuingService)
        {
            SmtpClient.SendQueued(this, spoolDirectory, queuingService);
        }

        public IAsyncResult BeginSendQueued(string spoolDirectory, QueuingService queuingService, AsyncCallback callback)
        {
            return SmtpClient.BeginSendQueued(this, spoolDirectory, queuingService, callback);
        }

        public void EndSendQueued(IAsyncResult result)
        {
            SmtpClient.EndSendQueued(result);
        }

        #endregion

    }

    #endregion
}