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

namespace ActiveUp.Net.Mail
{
    #region SmtpMessageCollection object
    /// <summary>
    /// Represents a collection of Message objects
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class SmtpMessageCollection : MessageCollection
    {
        /// <summary>
        /// Sends each message using the Direct Mailing technique 
        /// (SMTP connection with every recipient's mail exchange server for delivery).
        /// MX Records are cached for faster operation.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send();
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///messages.Send()
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send();
        /// </code>
        /// </example>
        public void Send()
        {
            ActiveUp.Net.Mail.SmtpClient.DirectSendCollection(this);
        }

        /// <summary>
        /// Sends all messages using the specified host.
        /// </summary>
        /// <param name="host">Address of the server to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com");
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///messages.Send("mail.myhost.com")
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com");
        /// </code>
        /// </example>
        public void Send(string host)
        {
            ActiveUp.Net.Mail.SmtpClient.SendCollection(this, host);
        }

        /// <summary>
        /// Sends all messages using the specified host.
        /// </summary>
        /// <param name="servers">Servers to be used to send the message (in preference order).</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///ServerCollection servers = new ServerCollection();
        ///servers.Add("mail.myhost.com",25);
        ///servers.Add("mail2.myhost.com",25);
        ///
        ///messages.Send(servers);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///Dim servers As New ServerCollection
        ///servers.Add("mail.myhost.com",25)
        ///servers.Add("mail2.myhost.com",25)
        ///
        ///messages.Send(servers)
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///var servers:ServerCollection = new ServerCollection();
        ///servers.Add("mail.myhost.com",25);
        ///servers.Add("mail2.myhost.com",25);
        ///
        ///messages.Send(servers);
        /// </code>
        /// </example>
        public void Send(ActiveUp.Net.Mail.ServerCollection servers)
        {
            ActiveUp.Net.Mail.SmtpClient.SendCollection(this, servers);
        }

        /// <summary>
        /// Sends all messages using the specified host and port.
        /// </summary>
        /// <param name="host">Address of the server to be used.</param>
        /// <param name="port">Port to be used.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com",8504);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///messages.Send("mail.myhost.com",8504)
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com",8504);
        /// </code>
        /// </example>
        public void Send(string host, int port)
        {
            ActiveUp.Net.Mail.SmtpClient.SendCollection(this, host, port);
        }

        /// <summary>
        /// Sends the message using the specified host and port.
        /// A secure SASL authentication is performed according to the specified SASL mechanism.
        /// </summary>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism"></param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5)
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5);
        /// </code>
        /// </example>
        public void Send(string host, string username, string password, ActiveUp.Net.Mail.SaslMechanism mechanism)
        {
            ActiveUp.Net.Mail.SmtpClient.SendCollection(this, host, username, password, mechanism);
        }

        /// <summary>
        /// Sends the message using the specified host and port.
        /// A secure SASL authentication is performed according to the specified SASL mechanism.
        /// </summary>
        /// <param name="host">Host to be used to send the message.</param>
        /// <param name="username">Username to be used for the authentication process.</param>
        /// <param name="password">Password to be used for the authentication process.</param>
        /// <param name="mechanism">SASL Mechanism to be used for authentication.</param>
        /// <param name="errors">Reference to an SmtpException Collection where occuring errors should be stored.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,myerrorCollection);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,myerrorCollection)
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,myerrorCollection);
        /// </code>
        /// </example>
        public void Send(string host, string username, string password, ActiveUp.Net.Mail.SaslMechanism mechanism, ref ActiveUp.Net.Mail.SmtpExceptionCollection errors)
        {
            ActiveUp.Net.Mail.SmtpClient.SendCollection(this, host, username, password, mechanism, ref errors);
        }

        /// <summary>
        /// Sends the message using the specified host and port.
        /// A simple LOGIN authentication is performed.
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
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Message message1 = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///SmtpMessageCollection messages = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim message1 As New Message
        ///message1.From = new Address("jdoe@myhost.com","John Doe")
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark")
        ///message1.Subject = "correction"
        ///message1.Attachments.Add("C:\myfile.doc")
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim messages As New SmtpMessageCollection
        ///messages.Add(message)
        ///messages.Add(message1)
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504)
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var message1:Message = new Message();
        ///message1.From = new Address("jdoe@myhost.com","John Doe");
        ///message1.To.Add("dclark@blurdybloop.com","Dave Clark");
        ///message1.Subject = "correction";
        ///message1.Attachments.Add("C:\\myfile.doc");
        ///message1.HtmlBody.Text = "Here is what I sent to Mike.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///var messages:SmtpMessageCollection = new SmtpMessageCollection();
        ///messages.Add(message);
        ///messages.Add(message1);
        ///
        ///messages.Send("mail.myhost.com","jdoe1234","tanstaaf",SaslMechanism.CramMd5,8504);
        /// </code>
        /// </example>
        public void Send(string host, string username, string password, ActiveUp.Net.Mail.SaslMechanism mechanism, int port)
        {
            ActiveUp.Net.Mail.SmtpClient.SendCollection(this, host, port, username, password, mechanism);
        }

    }
    #endregion
}