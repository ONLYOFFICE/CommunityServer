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
using System.Text;
using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Mail
{

    #region Fetch

    /// <summary>
    /// Allows to fetch (retrieve) partial or complete messages, as well as specific message informations.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Fetch
    {

        #region Private fields

        ActiveUp.Net.Mail.Mailbox _parentMailbox;

        #endregion

        #region Methods

        #region Private methods

        private CommandOptions getFetchOptions()
        {
            return new CommandOptions
            {
                IsPlusCmdAllowed = false
            };
        }

        private string GetMessageFromImapFetchResponce(string response)
        {
            //Sample responce:  1 FETCH (UID <uid> BODY[] {body_size} <mail body> ) UID OK SUCCESS
            var first_char = response.IndexOf("}", StringComparison.Ordinal) + 3;
            var body_size_starts = response.IndexOf("{", StringComparison.Ordinal);
            var body_size_string = response.Substring(body_size_starts + 1, first_char - body_size_starts - 4);
            var body_size = int.Parse(body_size_string);
            //Sometimes body size larger then responce size. We handle it.
            body_size = Math.Min(first_char + body_size, response.Length) - first_char;
            return response.Substring(first_char, body_size);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns a non-extensible form of the BodyStructure() method.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The server's response containing a parenthesized list.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// string body = inbox.Fetch.Body(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim body As String = inbox.Fetch.Body(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var body:string = inbox.Fetch.Body(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Body(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("fetch "+messageOrdinal.ToString()+" body", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(" UID")-response.IndexOf("}")-7);
        }

        private delegate string DelegateBody(int messageOrdinal);
        private DelegateBody _delegateBody;

        public IAsyncResult BeginBody(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateBody = this.Body;
            return this._delegateBody.BeginInvoke(messageOrdinal, callback, this._delegateBody);
        }

        public string EndBody(IAsyncResult result)
        {
            return this._delegateBody.EndInvoke(result);
        }
        
        public string UidBody(int uid)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(" UID")-response.IndexOf("}")-7);
        }

        private delegate string DelegateUidBody(int uid);
        private DelegateUidBody _delegateUidBody;

        public IAsyncResult BeginUidBody(int uid, AsyncCallback callback)
        {
            this._delegateUidBody = this.UidBody;
            return this._delegateUidBody.BeginInvoke(uid, callback, this._delegateUidBody);
        }

        public string EndUidBody(IAsyncResult result)
        {
            return this._delegateUidBody.EndInvoke(result);
        }

        /// <summary>
        /// Fetches a specific section of the message's body.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="section">The section (part number) to be fetched.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// string body = inbox.Fetch.BodySection(1,3);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim body As String = inbox.Fetch.Body(1,3);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var body:string = inbox.Fetch.Body(1,3);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string BodySection(int messageOrdinal, int section)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " body[" + section + "]", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(" UID")-response.IndexOf("}")-7);
        }

        private delegate string DelegateBodySection(int messageOrdinal, int section);
        private DelegateBodySection _delegateBodySection;

        public IAsyncResult BeginBodySection(int messageOrdinal, int section, AsyncCallback callback)
        {
            this._delegateBodySection = this.BodySection;
            return this._delegateBodySection.BeginInvoke(messageOrdinal, section, callback, this._delegateBodySection);
        }

        public string EndBodySection(IAsyncResult result)
        {
            return this._delegateBodySection.EndInvoke(result);
        }

        public string UidBodySection(int uid, int section)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body[" + section + "]", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(" UID")-response.IndexOf("}")-7);
        }

        private delegate string DelegateUidBodySection(int uid, int section);
        private DelegateUidBodySection _delegateUidBodySection;

        public IAsyncResult BeginUidBodySection(int uid, int section, AsyncCallback callback)
        {
            this._delegateUidBodySection = this.BodySection;
            return this._delegateUidBodySection.BeginInvoke(uid, section, callback, this._delegateUidBodySection);
        }

        public string EndUidBodySection(IAsyncResult result)
        {
            return this._delegateUidBodySection.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's body structure.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The server's response containing a parenthesized list</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// string body = inbox.Fetch.BodyStructure(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim body As String = inbox.Fetch.BodyStructure(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var body:string = inbox.Fetch.BodyStructure(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string BodyStructure(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " bodystructure", getFetchOptions());
            return response.Substring(response.IndexOf("bodystructure")+13,response.LastIndexOf(" UID")-response.IndexOf("bodystructure")-13);
        }

        private delegate string DelegateBodyStructure(int messageOrdinal);
        private DelegateBodyStructure _delegateBodyStructure;

        public IAsyncResult BeginBodyStructure(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateBodyStructure = this.BodyStructure;
            return this._delegateBodyStructure.BeginInvoke(messageOrdinal, callback, this._delegateBodyStructure);
        }

        public string EndBodyStructure(IAsyncResult result)
        {
            return this._delegateBodyStructure.EndInvoke(result);
        }

        public string UidBodyStructure(int uid)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " bodystructure", getFetchOptions());
            return response.Substring(response.IndexOf("bodystructure")+13,response.LastIndexOf(" UID")-response.IndexOf("bodystructure")-13);
        }

        private delegate string DelegateUidBodyStructure(int uid);
        private DelegateUidBodyStructure _delegateUidBodyStructure;

        public IAsyncResult BeginUidBodyStructure(int uid, AsyncCallback callback)
        {
            this._delegateUidBodyStructure = this.UidBodyStructure;
            return this._delegateUidBodyStructure.BeginInvoke(uid, callback, this._delegateUidBodyStructure);
        }

        public string EndUidBodyStructure(IAsyncResult result)
        {
            return this._delegateUidBodyStructure.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's internal date.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's internal date.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// string internalDate = inbox.Fetch.InternalDate(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim internalDate As String = inbox.Fetch.InternalDate(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var internalDate:string = inbox.Fetch.InternalDate(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string InternalDate(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " internaldate", getFetchOptions());
            return response.Split('\"')[1];
        }

        private delegate string DelegateInternalDate(int messageOrdinal);
        private DelegateInternalDate _delegateInternalDate;

        public IAsyncResult BeginInternalDate(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateInternalDate = this.InternalDate;
            return this._delegateInternalDate.BeginInvoke(messageOrdinal, callback, this._delegateInternalDate);
        }

        public string EndInternalDate(IAsyncResult result)
        {
            return this._delegateInternalDate.EndInvoke(result);
        }

        public string UidInternalDate(int uid)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " internaldate", getFetchOptions());
            return response.Split('\"')[1];
        }

        private delegate string DelegateUidInternalDate(int uid);
        private DelegateUidInternalDate _delegateUidInternalDate;

        public IAsyncResult BeginUidInternalDate(int uid, AsyncCallback callback)
        {
            this._delegateUidInternalDate = this.UidInternalDate;
            return this._delegateUidInternalDate.BeginInvoke(uid, callback, this._delegateUidInternalDate);
        }

        public string EndUidInternalDate(IAsyncResult result)
        {
            return this._delegateUidInternalDate.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's flags.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message info to be fetched.</param>
        /// <returns>A collection of flags.</returns>
            /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// FlagCollection flags = inbox.Fetch.Flags(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim flags As FlagCollection = inbox.Fetch.Flags(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var flags:FlagCollection = inbox.Fetch.Flags(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public FlagCollection Flags(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            ActiveUp.Net.Mail.FlagCollection flags = new ActiveUp.Net.Mail.FlagCollection();
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " flags", getFetchOptions());
            string flags_string = System.Text.RegularExpressions.Regex.Split(response.ToLower(),"flags ")[1].TrimStart('(').Split(')')[0];
            foreach(string str in flags_string.Split(' ')) if(str.StartsWith("\\")) flags.Add(str.Trim(new char[] {' ','\\',')','('}));
            return flags;
        }

        private delegate FlagCollection DelegateFlags(int messageOrdinal);
        private DelegateFlags _delegateFlags;

        public IAsyncResult BeginFlags(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateFlags = this.Flags;
            return this._delegateFlags.BeginInvoke(messageOrdinal, callback, this._delegateFlags);
        }

        public FlagCollection EndFlags(IAsyncResult result)
        {
            return this._delegateFlags.EndInvoke(result);
        }

        public FlagCollection UidFlags(int uid)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            ActiveUp.Net.Mail.FlagCollection flags = new ActiveUp.Net.Mail.FlagCollection();
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " flags", getFetchOptions());
            string flags_string = System.Text.RegularExpressions.Regex.Split(response.ToLower(),"flags ")[1].TrimStart('(').Split(')')[0];
            foreach(string str in flags_string.Split(' ')) if(str.StartsWith("\\")) flags.Add(str.Trim(new char[] {' ','\\',')','('}));
            return flags;
        }

        private delegate FlagCollection DelegateUidFlags(int uid);
        private DelegateUidFlags _delegateUidFlags;

        public IAsyncResult BeginUidFlags(int uid, AsyncCallback callback)
        {
            this._delegateUidFlags = this.UidFlags;
            return this._delegateUidFlags.BeginInvoke(uid, callback, this._delegateUidFlags);
        }

        public FlagCollection EndUidFlags(IAsyncResult result)
        {
            return this._delegateUidFlags.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant Header (parsable by the Parsing namespace classes).
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the Header to be fetched.</param>
        /// <returns>The message's Header as a byte array.</returns>
        public byte[] Header(int messageOrdinal)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.HeaderString(messageOrdinal));
        }

        private delegate byte[] DelegateHeader(int messageOrdinal);
        private DelegateHeader _delegateHeader;

        public IAsyncResult BeginHeader(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateHeader = this.Header;
            return this._delegateHeader.BeginInvoke(messageOrdinal, callback, this._delegateHeader);
        }

        public byte[] EndHeader(IAsyncResult result)
        {
            return this._delegateHeader.EndInvoke(result);
        }

        public byte[] UidHeader(int uid)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.UidHeaderString(uid));
        }

        private delegate byte[] DelegateUidHeader(int uid);
        private DelegateUidHeader _delegateUidHeader;

        public IAsyncResult BeginUidHeader(int uid, AsyncCallback callback)
        {
            this._delegateUidHeader = this.UidHeader;
            return this._delegateUidHeader.BeginInvoke(uid, callback, this._delegateUidHeader);
        }

        public byte[] EndUidHeader(IAsyncResult result)
        {
            return this._delegateUidHeader.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant header.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the Header to be fetched.</param>
        /// <returns>The message's Header as a Header object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// Header Header = inbox.Fetch.Header(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim Header As Header = inbox.Fetch.Header(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var header:Header = inbox.Fetch.Header(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public Header HeaderObject(int messageOrdinal)
        {
            return Parser.ParseHeader(this.Header(messageOrdinal));
        }

        private delegate Header DelegateHeaderObject(int messageOrdinal);
        private DelegateHeaderObject _delegateHeaderObject;

        public IAsyncResult BeginHeaderObject(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateHeaderObject = this.UidHeaderObject;
            return this._delegateHeaderObject.BeginInvoke(messageOrdinal, callback, this._delegateHeaderObject);
        }

        public Header EndHeaderObject(IAsyncResult result)
        {
            return this._delegateHeaderObject.EndInvoke(result);
        }

        public Header UidHeaderObject(int uid)
        {
            return Parser.ParseHeader(this.UidHeader(uid));
        }

        private delegate Header DelegateUidHeaderObject(int uid);
        private DelegateUidHeaderObject _delegateUidHeaderObject;

        public IAsyncResult BeginUidHeaderObject(int uid, AsyncCallback callback)
        {
            this._delegateUidHeaderObject = this.UidHeaderObject;
            return this._delegateUidHeaderObject.BeginInvoke(uid, callback, this._delegateUidHeaderObject);
        }

        public Header EndUidHeaderObject(IAsyncResult result)
        {
            return this._delegateUidHeaderObject.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant Header (parsable by the Parsing namespace classes).
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the Header to be fetched.</param>
        /// <returns>The message's Header as a MemoryStream.</returns>
        /// <example><see cref="Fetch.HeaderObject"/></example>
        public System.IO.MemoryStream HeaderStream(int messageOrdinal)
        {
            byte[] buf =  this.Header(messageOrdinal);
            return new System.IO.MemoryStream(buf,0,buf.Length,false);
        }

        private delegate System.IO.MemoryStream DelegateHeaderStream(int messageOrdinal);
        private DelegateHeaderStream _delegateHeaderStream;

        public IAsyncResult BeginHeaderStream(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateHeaderStream = this.HeaderStream;
            return this._delegateHeaderStream.BeginInvoke(messageOrdinal, callback, this._delegateHeaderStream);
        }

        public System.IO.MemoryStream EndHeaderStream(IAsyncResult result)
        {
            return this._delegateHeaderStream.EndInvoke(result);
        }

        public System.IO.MemoryStream UidHeaderStream(int uid)
        {
            byte[] buf =  this.UidHeader(uid);
            return new System.IO.MemoryStream(buf,0,buf.Length,false);
        }

        private delegate System.IO.MemoryStream DelegateUidHeaderStream(int uid);
        private DelegateUidHeaderStream _delegateUidHeaderStream;

        public IAsyncResult BeginUidHeaderStream(int uid, AsyncCallback callback)
        {
            this._delegateUidHeaderStream = this.UidHeaderStream;
            return this._delegateUidHeaderStream.BeginInvoke(uid, callback, this._delegateUidHeaderStream);
        }

        public System.IO.MemoryStream EndUidHeaderStream(IAsyncResult result)
        {
            return this._delegateUidHeaderStream.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant Header (parsable by the Parsing namespace classes).
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the Header to be fetched.</param>
        /// <returns>The message's Header as a string.</returns>
        /// <example><see cref="Fetch.HeaderObject"/></example>
        public string HeaderString(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            this.ParentMailbox.SourceClient.OnHeaderRetrieving(new ActiveUp.Net.Mail.HeaderRetrievingEventArgs(messageOrdinal));
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.header", getFetchOptions());
            string header = response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
            this.ParentMailbox.SourceClient.OnHeaderRetrieved(new ActiveUp.Net.Mail.HeaderRetrievedEventArgs(System.Text.Encoding.UTF8.GetBytes(header),messageOrdinal));
            return header;
        }

        private delegate string DelegateHeaderString(int messageOrdinal);
        private DelegateHeaderString _delegateHeaderString;

        public IAsyncResult BeginHeaderString(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateHeaderString = this.HeaderString;
            return this._delegateHeaderString.BeginInvoke(messageOrdinal, callback, this._delegateHeaderString);
        }

        public string EndHeaderString(IAsyncResult result)
        {
            return this._delegateHeaderString.EndInvoke(result);
        }

        public string UidHeaderString(int uid)
        {
            var response = String.Empty;

            try
            {
                this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
                this.ParentMailbox.SourceClient.OnHeaderRetrieving(new ActiveUp.Net.Mail.HeaderRetrievingEventArgs(uid));
                response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.header", getFetchOptions());
                string header = response.Substring(response.IndexOf("}") + 3, response.LastIndexOf(")") - response.IndexOf("}") - 3);
                this.ParentMailbox.SourceClient.OnHeaderRetrieved(new ActiveUp.Net.Mail.HeaderRetrievedEventArgs(System.Text.Encoding.UTF8.GetBytes(header), uid));
                return header;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving header. Response: " + response, ex);
            }
        }

        private delegate string DelegateUidHeaderString(int uid);
        private DelegateUidHeaderString _delegateUidHeaderString;

        public IAsyncResult BeginUidHeaderString(int uid, AsyncCallback callback)
        {
            this._delegateUidHeaderString = this.UidHeaderString;
            return this._delegateUidHeaderString.BeginInvoke(uid, callback, this._delegateUidHeaderString);
        }

        public string EndUidHeaderString(IAsyncResult result)
        {
            return this._delegateUidHeaderString.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the requested Header lines without setting the \Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="headerHeaders">An array of string representing the requested headers.</param>
        /// <returns>A NameValueCollection where Names are the Header delimiters and Values are the Header bodies.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// //Request the message's subject and from header.
        /// NameValueCollection lines = inbox.Fetch.HeaderLinesPeek(1,new string[] {"subject","from"});
        /// //Extract the subject.
        /// string messageSubject = lines["subject"];
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// 'Request the message's subject and from header.
        /// Dim lines As NameValueCollection = inbox.Fetch.HeaderLinesPeek(1,new string[] {"subject","from"})
        /// 'Extract the subject.
        /// Dim messageSubject As String = lines("subject")
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// //Request the message's subject and from header.
        /// var lines:NameValueCollection = inbox.Fetch.HeaderLinesPeek(1,new string[] {"subject","from"});
        /// //Extract the subject.
        /// var messageSubject:string = lines["subject"];
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public System.Collections.Specialized.NameValueCollection HeaderLinesPeek(int messageOrdinal, string[] headerHeaders)
        {            
            string delimiters = string.Format("({0})", string.Join(" ", headerHeaders));

            string response = (this.ParentMailbox.SourceClient.ServerCapabilities.ToLower().IndexOf("imap4rev1") != -1) ?
                this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " body.peek[header.fields " + delimiters + "]", getFetchOptions()) :
                this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.peek.header.GetStringRepresentation() " + delimiters, getFetchOptions());

            response = response.Substring(response.IndexOf("}") + 3, response.LastIndexOf("\r\n\r\n") - response.IndexOf("}") - 3);
            
            var fieldcol = new System.Collections.Specialized.NameValueCollection();

            Parser.ParseHeader(response, ref fieldcol);

            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateHeaderLinesPeek(int messageOrdinal, string[] headerHeaders);
        private DelegateHeaderLinesPeek _delegateHeaderLinesPeek;

        public IAsyncResult BeginHeaderLinesPeek(int messageOrdinal, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateHeaderLinesPeek = this.HeaderLinesPeek;
            return this._delegateHeaderLinesPeek.BeginInvoke(messageOrdinal, headerHeaders, callback, this._delegateHeaderLinesPeek);
        }

        public System.Collections.Specialized.NameValueCollection EndHeaderLinesPeek(IAsyncResult result)
        {
            return this._delegateHeaderLinesPeek.EndInvoke(result);
        }

        public System.Collections.Specialized.NameValueCollection UidHeaderLinesPeek(int uid, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.ToLower().IndexOf("imap4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body.peek[header.fields " + delimiters + "]", getFetchOptions());
            else this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.peek.header.GetStringRepresentation() " + delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateUidHeaderLinesPeek(int uid, string[] headerHeaders);
        private DelegateUidHeaderLinesPeek _delegateUidHeaderLinesPeek;

        public IAsyncResult BeginUidHeaderLinesPeek(int uid, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateUidHeaderLinesPeek = this.UidHeaderLinesPeek;
            return this._delegateUidHeaderLinesPeek.BeginInvoke(uid, headerHeaders, callback, this._delegateUidHeaderLinesPeek);
        }

        public System.Collections.Specialized.NameValueCollection EndUidHeaderLinesPeek(IAsyncResult result)
        {
            return this._delegateUidHeaderLinesPeek.EndInvoke(result);
        }

        /// <summary>
        /// Same as HeaderLines except that it will return all headers EXCEPT the specified ones, without setting the \Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="headerHeaders">An array of string representing the NOT-requested headers.</param>
        /// <returns>A NameValueCollection where Names are the Header delimiters and Values are the Header bodies.</returns>
        /// <example><see cref="Fetch.HeaderLines"/></example>
        public System.Collections.Specialized.NameValueCollection HeaderLinesNotPeek(int messageOrdinal, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " body.peek[header.fields.not " + delimiters + "]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.peek.header.GetStringRepresentation().not " + delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateHeaderLinesNotPeek(int messageOrdinal, string[] headerHeaders);
        private DelegateHeaderLinesNotPeek _delegateHeaderLinesNotPeek;

        public IAsyncResult BeginHeaderLinesNotPeek(int messageOrdinal, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateHeaderLinesNotPeek = this.HeaderLinesNotPeek;
            return this._delegateHeaderLinesNotPeek.BeginInvoke(messageOrdinal, headerHeaders, callback, this._delegateHeaderLinesNotPeek);
        }

        public System.Collections.Specialized.NameValueCollection EndHeaderLinesNotPeek(IAsyncResult result)
        {
            return this._delegateHeaderLinesNotPeek.EndInvoke(result);
        }

        public System.Collections.Specialized.NameValueCollection UidHeaderLinesNotPeek(int uid, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body.peek[header.fields.not " + delimiters + "]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.peek.header.GetStringRepresentation().not " + delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateUidHeaderLinesNotPeek(int uid, string[] headerHeaders);
        private DelegateUidHeaderLinesNotPeek _delegateUidHeaderLinesNotPeek;

        public IAsyncResult BeginUidHeaderLinesNotPeek(int uid, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateUidHeaderLinesNotPeek = this.UidHeaderLinesNotPeek;
            return this._delegateUidHeaderLinesNotPeek.BeginInvoke(uid, headerHeaders, callback, this._delegateUidHeaderLinesNotPeek);
        }

        public System.Collections.Specialized.NameValueCollection EndUidHeaderLinesNotPeek(IAsyncResult result)
        {
            return this._delegateUidHeaderLinesNotPeek.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the requested Header lines.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="headerHeaders">An array of string representing the requested headers.</param>
        /// <returns>A NameValueCollection where Names are the Header delimiters and Values are the Header bodies.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// //Request the message's subject and from header.
        /// NameValueCollection lines = inbox.Fetch.HeaderLines(1,new string[] {"subject","from"});
        /// //Extract the subject.
        /// string messageSubject = lines["subject"];
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// 'Request the message's subject and from header.
        /// Dim lines As NameValueCollection = inbox.Fetch.HeaderLines(1,new string[] {"subject","from"})
        /// 'Extract the subject.
        /// Dim messageSubject As String = lines("subject")
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// //Request the message's subject and from header.
        /// var lines:NameValueCollection = inbox.Fetch.HeaderLines(1,new string[] {"subject","from"});
        /// //Extract the subject.
        /// var messageSubject:string = lines["subject"];
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public System.Collections.Specialized.NameValueCollection HeaderLines(int messageOrdinal, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if(this.ParentMailbox.SourceClient.ServerCapabilities.ToLower().IndexOf("imap4rev1")!=-1) response = this.ParentMailbox.SourceClient.Command("fetch "+messageOrdinal.ToString()+" body[header.fields "+delimiters+"]", getFetchOptions());
            else this.ParentMailbox.SourceClient.Command("fetch "+messageOrdinal.ToString()+" rfc822.header.GetStringRepresentation() "+delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateHeaderLines(int messageOrdinal, string[] headerHeaders);
        private DelegateHeaderLines _delegateHeaderLines;

        public IAsyncResult BeginHeaderLines(int messageOrdinal, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateHeaderLines = this.HeaderLines;
            return this._delegateHeaderLines.BeginInvoke(messageOrdinal, headerHeaders, callback, this._delegateHeaderLines);
        }

        public System.Collections.Specialized.NameValueCollection EndHeaderLines(IAsyncResult result)
        {
            return this._delegateHeaderLines.EndInvoke(result);
        }

        public System.Collections.Specialized.NameValueCollection UidHeaderLines(int uid, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.ToLower().IndexOf("imap4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body[header.fields " + delimiters + "]", getFetchOptions());
            else this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.header.GetStringRepresentation() " + delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateUidHeaderLines(int uid, string[] headerHeaders);
        private DelegateUidHeaderLines _delegateUidHeaderLines;

        public IAsyncResult BeginUidHeaderLines(int uid, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateUidHeaderLines = this.UidHeaderLines;
            return this._delegateUidHeaderLines.BeginInvoke(uid, headerHeaders, callback, this._delegateUidHeaderLines);
        }

        public System.Collections.Specialized.NameValueCollection EndUidHeaderLines(IAsyncResult result)
        {
            return this._delegateUidHeaderLines.EndInvoke(result);
        }

        /// <summary>
        /// Same as HeaderLines except that it will return all headers EXCEPT the specified ones.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="headerHeaders">An array of string representing the NOT-requested headers.</param>
        /// <returns>A NameValueCollection where Names are the Header delimiters and Values are the Header bodies.</returns>
        /// <example><see cref="Fetch.HeaderLines"/></example>
        public System.Collections.Specialized.NameValueCollection HeaderLinesNot(int messageOrdinal, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " body[header.fields.not " + delimiters + "]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.header.GetStringRepresentation().not " + delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateHeaderLinesNot(int messageOrdinal, string[] headerHeaders);
        private DelegateHeaderLinesNot _delegateHeaderLinesNot;

        public IAsyncResult BeginHeaderLinesNot(int messageOrdinal, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateHeaderLinesNot = this.HeaderLinesNot;
            return this._delegateHeaderLinesNot.BeginInvoke(messageOrdinal, headerHeaders, callback, this._delegateHeaderLinesNot);
        }

        public System.Collections.Specialized.NameValueCollection EndHeaderLinesNot(IAsyncResult result)
        {
            return this._delegateHeaderLinesNot.EndInvoke(result);
        }

        public System.Collections.Specialized.NameValueCollection UidHeaderLinesNot(int uid, string[] headerHeaders)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string delimiters = "(";
            foreach(string str in headerHeaders) delimiters += str+" ";
            delimiters = delimiters.Trim(' ')+")";
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body[header.fields.not " + delimiters + "]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.header.GetStringRepresentation().not " + delimiters, getFetchOptions());
            response = response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n\r\n")-response.IndexOf("}")-3);
            System.Collections.Specialized.NameValueCollection fieldcol = new System.Collections.Specialized.NameValueCollection();
            string[] fields = System.Text.RegularExpressions.Regex.Split(response,"\r\n");
            for(int i=0;i<fields.Length;i++) if(fields[i].IndexOf(":")!=-1) fieldcol.Add(fields[i].Substring(0,fields[i].IndexOf(":")).ToLower().TrimEnd(':'),fields[i].Substring(fields[i].IndexOf(":")).TrimStart(new char[] {':',' '}));
            return fieldcol;
        }

        private delegate System.Collections.Specialized.NameValueCollection DelegateUidHeaderLinesNot(int uid, string[] headerHeaders);
        private DelegateUidHeaderLinesNot _delegateUidHeaderLinesNot;

        public IAsyncResult BeginUidHeaderLinesNot(int uid, string[] headerHeaders, AsyncCallback callback)
        {
            this._delegateUidHeaderLinesNot = this.UidHeaderLinesNot;
            return this._delegateUidHeaderLinesNot.BeginInvoke(uid, headerHeaders, callback, this._delegateUidHeaderLinesNot);
        }

        public System.Collections.Specialized.NameValueCollection EndUidHeaderLinesNot(IAsyncResult result)
        {
            return this._delegateUidHeaderLinesNot.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant form.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a byte array.</returns>
        /// <example><see cref="Fetch.MessageObject"/></example>
        public byte[] Message(int messageOrdinal)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.MessageString(messageOrdinal));
        }

        private delegate byte[] DelegateMessage(int messageOrdinal);
        private DelegateMessage _delegateMessage;

        public IAsyncResult BeginMessage(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessage = this.Message;
            return this._delegateMessage.BeginInvoke(messageOrdinal, callback, this._delegateMessage);
        }

        public byte[] EndMessage(IAsyncResult result)
        {
            return this._delegateMessage.EndInvoke(result);
        }


        public byte[] UidMessage(int uid)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.UidMessageString(uid));
        }

        private delegate byte[] DelegateUidMessage(int uid);
        private DelegateUidMessage _delegateUidMessage;

        public IAsyncResult BeginUidMessage(int uid, AsyncCallback callback)
        {
            this._delegateUidMessage = this.UidMessage;
            return this._delegateUidMessage.BeginInvoke(uid, callback, this._delegateUidMessage);
        }

        public byte[] EndUidMessage(IAsyncResult result)
        {
            return this._delegateUidMessage.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant form.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a Message object.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// Message message = inbox.Fetch.Message(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim message As Message = inbox.Fetch.Message(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var message:Message = inbox.Fetch.Message(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public ActiveUp.Net.Mail.Message MessageObject(int messageOrdinal)
        {
            return ActiveUp.Net.Mail.Parser.ParseMessage(this.Message(messageOrdinal));
        }

        private delegate Message DelegateMessageObject(int messageOrdinal);
        private DelegateMessageObject _delegateMessageObject;

        public IAsyncResult BeginMessageObject(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessageObject = this.MessageObject;
            return this._delegateMessageObject.BeginInvoke(messageOrdinal, callback, this._delegateMessageObject);
        }

        public Message EndMessageObject(IAsyncResult result)
        {
            return this._delegateMessageObject.EndInvoke(result);
        }

        public ActiveUp.Net.Mail.Message UidMessageObject(int uid)
        {
            var msg = UidMessageString(uid);
            return Parser.ParseMessage(ref msg);
        }

        private delegate Message DelegateUidMessageObject(int uid);
        private DelegateUidMessageObject _delegateUidMessageObject;

        public IAsyncResult BeginUidMessageObject(int uid, AsyncCallback callback)
        {
            this._delegateUidMessageObject = this.UidMessageObject;
            return this._delegateUidMessageObject.BeginInvoke(uid, callback, this._delegateUidMessageObject);
        }

        public Message EndUidMessageObject(IAsyncResult result)
        {
            return this._delegateUidMessageObject.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant form.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a MemoryStream.</returns>
        /// <example><see cref="Fetch.MessageObject"/></example>
        public System.IO.MemoryStream MessageStream(int messageOrdinal)
        {
            byte[] buf =  this.Message(messageOrdinal);
            return new System.IO.MemoryStream(buf,0,buf.Length,false);
        }

        private delegate System.IO.MemoryStream DelegateMessageStream(int messageOrdinal);
        private DelegateMessageStream _delegateMessageStream;

        public IAsyncResult BeginMessageStream(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessageStream = this.MessageStream;
            return this._delegateMessageStream.BeginInvoke(messageOrdinal, callback, this._delegateMessageStream);
        }

        public System.IO.MemoryStream EndMessageStream(IAsyncResult result)
        {
            return this._delegateMessageStream.EndInvoke(result);
        }

        public System.IO.MemoryStream UidMessageStream(int uid)
        {
            byte[] buf =  this.UidMessage(uid);
            return new System.IO.MemoryStream(buf,0,buf.Length,false);
        }

        private delegate System.IO.MemoryStream DelegateUidMessageStream(int uid);
        private DelegateUidMessageStream _delegateUidMessageStream;

        public IAsyncResult BeginUidMessageStream(int uid, AsyncCallback callback)
        {
            this._delegateUidMessageStream = this.UidMessageStream;
            return this._delegateUidMessageStream.BeginInvoke(uid, callback, this._delegateUidMessageStream);
        }

        public System.IO.MemoryStream EndUidMessageStream(IAsyncResult result)
        {
            return this._delegateUidMessageStream.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the message's Rfc822 compliant form.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a string.</returns>
        /// <example><see cref="Fetch.MessageObject"/></example>
        public string MessageString(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            this.ParentMailbox.SourceClient.OnMessageRetrieving(new ActiveUp.Net.Mail.MessageRetrievingEventArgs(messageOrdinal));
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822", getFetchOptions());
            ActiveUp.Net.Mail.Logger.AddEntry(response);
            string message = response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
            this.ParentMailbox.SourceClient.OnMessageRetrieved(new ActiveUp.Net.Mail.MessageRetrievedEventArgs(System.Text.Encoding.UTF8.GetBytes(message),messageOrdinal));
            return message;
        }

        private delegate string DelegateMessageString(int messageOrdinal);
        private DelegateMessageString _delegateMessageString;

        public IAsyncResult BeginMessageString(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessageString = this.MessageString;
            return this._delegateMessageString.BeginInvoke(messageOrdinal, callback, this._delegateMessageString);
        }

        public string EndMessageString(IAsyncResult result)
        {
            return this._delegateMessageString.EndInvoke(result);
        }

        public string UidMessageString(int uid)
        {
            ParentMailbox.SourceClient.SelectMailbox(ParentMailbox.Name);
            ParentMailbox.SourceClient.OnMessageRetrieving(new MessageRetrievingEventArgs(uid));
            var response = ParentMailbox.SourceClient.Command("uid fetch " + uid + " rfc822", getFetchOptions());
            var message = GetMessageFromImapFetchResponce(response);
            ParentMailbox.SourceClient.OnMessageRetrieved(new MessageRetrievedEventArgs(System.Text.Encoding.UTF8.GetBytes(message), uid));
            return message;
        }

        private delegate string DelegateUidMessageString(int uid);
        private DelegateUidMessageString _delegateUidMessageString;

        public IAsyncResult BeginUidMessageString(int uid, AsyncCallback callback)
        {
            this._delegateUidMessageString = this.UidMessageString;
            return this._delegateUidMessageString.BeginInvoke(uid, callback, this._delegateUidMessageString);
        }

        public string EndUidMessageString(IAsyncResult result)
        {
            return this._delegateUidMessageString.EndInvoke(result);
        }

        /// <summary>
        /// Same as Message() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a byte array.</returns>
        /// <example><see cref="Fetch.MessageObject"/></example>
        public byte[] MessagePeek(int messageOrdinal)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.MessageStringPeek(messageOrdinal));
        }

        private delegate byte[] DelegateMessagePeek(int messageOrdinal);
        private DelegateMessagePeek _delegateMessagePeek;

        public IAsyncResult BeginMessagePeek(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessagePeek = this.MessagePeek;
            return this._delegateMessagePeek.BeginInvoke(messageOrdinal, callback, this._delegateMessagePeek);
        }

        public byte[] EndMessagePeek(IAsyncResult result)
        {
            return this._delegateMessagePeek.EndInvoke(result);
        }


        public byte[] UidMessagePeek(int uid)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.UidMessageStringPeek(uid));
        }

        private delegate byte[] DelegateUidMessagePeek(int uid);
        private DelegateUidMessagePeek _delegateUidMessagePeek;

        public IAsyncResult BeginUidMessagePeek(int uid, AsyncCallback callback)
        {
            this._delegateUidMessagePeek = this.UidMessagePeek;
            return this._delegateUidMessagePeek.BeginInvoke(uid, callback, this._delegateUidMessagePeek);
        }

        public byte[] EndUidMessagePeek(IAsyncResult result)
        {
            return this._delegateUidMessagePeek.EndInvoke(result);
        }

        /// <summary>
        /// Same as MessageObject() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a Message object.</returns>
        /// <example><see cref="Fetch.MessageObject"/></example>
        public Message MessageObjectPeek(int messageOrdinal)
        {
            return Parser.ParseMessage(this.MessagePeek(messageOrdinal));
        }

        private delegate Message DelegateMessageObjectPeek(int messageOrdinal);
        private DelegateMessageObjectPeek _delegateMessageObjectPeek;

        public IAsyncResult BeginMessageObjectPeek(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessageObjectPeek = this.MessageObjectPeek;
            return this._delegateMessageObjectPeek.BeginInvoke(messageOrdinal, callback, this._delegateMessageObjectPeek);
        }

        public Message EndMessageObjectPeek(IAsyncResult result)
        {
            return this._delegateMessageObjectPeek.EndInvoke(result);
        }

        public Message UidMessageObjectPeek(int uid)
        {
            var msg = UidMessageStringPeek(uid);

            Message message;

            try
            {
                message = Parser.ParseMessage(ref msg);
            }
            catch (Exception ex)
            {
                if (ex is ParsingException || ex is IndexOutOfRangeException)
                {
                    var header_string = UidHeaderString(uid);

                    Header header;

                    if (!Parser.TryParseDefectiveHeader(header_string, out header))
                        throw;

                    message = new Message(header);

                    message.AddAttachmentFromString("original_message.eml", msg);

                    message.OriginalData = Encoding.GetEncoding("iso-8859-1").GetBytes(msg);

                    message.HasParseError = true;
                }
                else
                    throw;
            }

            return message;
        }

        private delegate Message DelegateUidMessageObjectPeek(int uid);
        private DelegateUidMessageObjectPeek _delegateUidMessageObjectPeek;

        public IAsyncResult BeginUidMessageObjectPeek(int uid, AsyncCallback callback)
        {
            this._delegateUidMessageObjectPeek = this.UidMessageObjectPeek;
            return this._delegateUidMessageObjectPeek.BeginInvoke(uid, callback, this._delegateUidMessageObjectPeek);
        }

        public Message EndUidMessageObjectPeek(IAsyncResult result)
        {
            return this._delegateUidMessageObjectPeek.EndInvoke(result);
        }

        /// <summary>
        /// Same as MessageStream() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a MemoryStream.</returns>
        /// <example><see cref="Fetch.MessageStream"/></example>
        public System.IO.MemoryStream MessageSreamPeek(int messageOrdinal)
        {
            byte[] buf =  this.MessagePeek(messageOrdinal);
            return new System.IO.MemoryStream(buf,0,buf.Length,false);
        }

        private delegate System.IO.MemoryStream DelegateMessageStreamPeek(int messageOrdinal);
        private DelegateMessageStreamPeek _delegateMessageStreamPeek;

        public IAsyncResult BeginMessageStreamPeek(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessageStreamPeek = this.MessageSreamPeek;
            return this._delegateMessageStreamPeek.BeginInvoke(messageOrdinal, callback, this._delegateMessageStreamPeek);
        }

        public System.IO.MemoryStream EndMessageStreamPeek(IAsyncResult result)
        {
            return this._delegateMessageStreamPeek.EndInvoke(result);
        }

        public System.IO.MemoryStream UidMessageStreamPeek(int uid)
        {
            byte[] buf =  this.UidMessagePeek(uid);
            return new System.IO.MemoryStream(buf,0,buf.Length,false);
        }

        private delegate System.IO.MemoryStream DelegateUidMessageStreamPeek(int uid);
        private DelegateUidMessageStreamPeek _delegateUidMessageStreamPeek;

        public IAsyncResult BeginUidMessageStreamPeek(int uid, AsyncCallback callback)
        {
            this._delegateUidMessageStreamPeek = this.UidMessageStreamPeek;
            return this._delegateUidMessageStreamPeek.BeginInvoke(uid, callback, this._delegateUidMessageStreamPeek);
        }

        public System.IO.MemoryStream EndUidMessageStreamPeek(IAsyncResult result)
        {
            return this._delegateUidMessageStreamPeek.EndInvoke(result);
        }

        /// <summary>
        /// Same as MessageString() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's data as a string.</returns>
        /// <example><see cref="Fetch.MessageString"/></example>
        public string MessageStringPeek(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            this.ParentMailbox.SourceClient.OnMessageRetrieving(new ActiveUp.Net.Mail.MessageRetrievingEventArgs(messageOrdinal));
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " body[mime]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.peek", getFetchOptions());
            string message = response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
            this.ParentMailbox.SourceClient.OnMessageRetrieved(new ActiveUp.Net.Mail.MessageRetrievedEventArgs(System.Text.Encoding.UTF8.GetBytes(message),messageOrdinal));
            return message;
        }

        private delegate string DelegateMessageStringPeek(int messageOrdinal);
        private DelegateMessageStringPeek _delegateMessageStringPeek;

        public IAsyncResult BeginMessageStringPeek(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateMessageStringPeek = this.MessageStringPeek;
            return this._delegateMessageStringPeek.BeginInvoke(messageOrdinal, callback, this._delegateMessageStringPeek);
        }

        public string EndMessageStringPeek(IAsyncResult result)
        {
            return this._delegateMessageStringPeek.EndInvoke(result);
        }

        public string UidMessageStringPeek(int uid)
        {
            ParentMailbox.SourceClient.SelectMailbox(ParentMailbox.Name);
            ParentMailbox.SourceClient.OnMessageRetrieving(new MessageRetrievingEventArgs(uid));
            var response = ParentMailbox.SourceClient.Command("uid fetch " + uid + " BODY.PEEK[]");
            var message = GetMessageFromImapFetchResponce(response);
            ParentMailbox.SourceClient.OnMessageRetrieved(new MessageRetrievedEventArgs(System.Text.Encoding.UTF8.GetBytes(message), uid));
            return message;
        }

        private delegate string DelegateUidMessageStringPeek(int uid);
        private DelegateUidMessageStringPeek _delegateUidMessageStringPeek;

        public IAsyncResult BeginUidMessageStringPeek(int uid, AsyncCallback callback)
        {
            this._delegateUidMessageStringPeek = this.UidMessageStringPeek;
            return this._delegateUidMessageStringPeek.BeginInvoke(uid, callback, this._delegateUidMessageStringPeek);
        }

        public string EndUidMessageStringPeek(IAsyncResult result)
        {
            return this._delegateUidMessageStringPeek.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the specified message's size (in bytes).
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's size in bytes.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// int size = inbox.Fetch.Size(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim size As Integer = inbox.Fetch.Size(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var size:int = inbox.Fetch.Size(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public int Size(int messageOrdinal)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.size", getFetchOptions());
            return System.Convert.ToInt32(response.Substring(response.ToLower().IndexOf("rfc822.size")+12).Split(')')[0]);
        }

        private delegate int DelegateSize(int messageOrdinal);
        private DelegateSize _delegateSize;

        public IAsyncResult BeginSize(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateSize = this.Size;
            return this._delegateSize.BeginInvoke(messageOrdinal, callback, this._delegateSize);
        }

        public int EndSize(IAsyncResult result)
        {
            return this._delegateSize.EndInvoke(result);
        }

        public int UidSize(int uid)
        {
            this.ParentMailbox.SourceClient.SelectMailbox(this.ParentMailbox.Name);
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.size", getFetchOptions());
            return System.Convert.ToInt32(response.Substring(response.ToLower().IndexOf("rfc822.size")+12).Split(')')[0]);
        }

        private delegate int DelegateUidSize(int messageOrdinal);
        private DelegateUidSize _delegateUidSize;

        public IAsyncResult BeginUidSize(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateUidSize = this.UidSize;
            return this._delegateUidSize.BeginInvoke(messageOrdinal, callback, this._delegateUidSize);
        }

        public int EndUidSize(IAsyncResult result)
        {
            return this._delegateUidSize.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the specified message's text (body).
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's text.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// string messageBody = inbox.Fetch.Text(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim messageBody As Header = inbox.Fetch.Text(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var messageBody:string = inbox.Fetch.Text(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Text(int messageOrdinal)
        {
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " rfc822.text", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
        }

        private delegate string DelegateText(int messageOrdinal);
        private DelegateText _delegateText;

        public IAsyncResult BeginText(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateText = this.Text;
            return this._delegateText.BeginInvoke(messageOrdinal, callback, this._delegateText);
        }

        public string EndText(IAsyncResult result)
        {
            return this._delegateText.EndInvoke(result);
        }

        public string UidText(int uid)
        {
            string response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " rfc822.text", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
        }

        private delegate string DelegateUidText(int uid);
        private DelegateUidText _delegateUidText;

        public IAsyncResult BeginUidText(int uid, AsyncCallback callback)
        {
            this._delegateUidText = this.UidText;
            return this._delegateUidText.BeginInvoke(uid, callback, this._delegateUidText);
        }

        public string EndUidText(IAsyncResult result)
        {
            return this._delegateUidText.EndInvoke(result);
        }

        /// <summary>
        /// Same as Text() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's text.</returns>
        /// <example><see cref="Fetch.Text"/></example>
        public string TextPeek(int messageOrdinal)
        {
            string response = "";
            if(this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1")!=-1) response = this.ParentMailbox.SourceClient.Command("fetch "+messageOrdinal.ToString()+" body[text]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("fetch "+messageOrdinal.ToString()+" rfc822.text.peek", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
        }

        private delegate string DelegateTextPeek(int messageOrdinal);
        private DelegateTextPeek _delegateTextPeek;

        public IAsyncResult BeginTextPeek(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateTextPeek = this.TextPeek;
            return this._delegateTextPeek.BeginInvoke(messageOrdinal, callback, this._delegateTextPeek);
        }

        public string EndTextPeek(IAsyncResult result)
        {
            return this._delegateTextPeek.EndInvoke(result);
        }

        public string UidTextPeek(int uid)
        {
            string response = "";
            if (this.ParentMailbox.SourceClient.ServerCapabilities.IndexOf("IMAP4rev1") != -1) response = this.ParentMailbox.SourceClient.Command("uid fetch " + uid.ToString() + " body[text]", getFetchOptions());
            else response = this.ParentMailbox.SourceClient.Command("uid fetch "+uid.ToString()+" rfc822.text.peek", getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf(")")-response.IndexOf("}")-3);
        }

        private delegate string DelegateUidTextPeek(int messageOrdinal);
        private DelegateUidTextPeek _delegateUidTextPeek;

        public IAsyncResult BeginUidTextPeek(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateUidTextPeek = this.UidTextPeek;
            return this._delegateUidTextPeek.BeginInvoke(messageOrdinal, callback, this._delegateUidTextPeek);
        }

        public string EndUidTextPeek(IAsyncResult result)
        {
            return this._delegateUidTextPeek.EndInvoke(result);
        }

        /// <summary>
        /// Fetches the specified message's Unique Identifier number.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <returns>The message's unique identifier number.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// int uid = inbox.Fetch.Uid(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// Dim uid As Integer = inbox.Fetch.Uid(1);
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// var uid:int = inbox.Fetch.Uid(1);
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public int Uid(int messageOrdinal)
        {
            string response = this.ParentMailbox.SourceClient.Command("fetch " + messageOrdinal.ToString() + " uid", getFetchOptions());
            return System.Convert.ToInt32(response.Substring(response.ToLower().IndexOf("uid")+3).Split(')')[0]);
        }

        private delegate int DelegateUid(int messageOrdinal);
        private DelegateUid _delegateUid;

        public IAsyncResult BeginUid(int messageOrdinal, AsyncCallback callback)
        {
            this._delegateUid = this.Uid;
            return this._delegateUid.BeginInvoke(messageOrdinal, callback, this._delegateUid);
        }

        public int EndUid(IAsyncResult result)
        {
            return this._delegateUid.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of BodySection().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="section">The message's body section to be retrieved.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array.</returns>
        public byte[] PartialBodySection(int messageOrdinal, int section, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.PartialBodySectionString(messageOrdinal,section,index,count));
        }

        private delegate byte[] DelegatePartialBodySection(int messageOrdinal, int section, int index, int count);
        private DelegatePartialBodySection _delegatePartialBodySection;

        public IAsyncResult BeginPartialBodySection(int messageOrdinal, int section, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialBodySection = this.PartialBodySection;
            return this._delegatePartialBodySection.BeginInvoke(messageOrdinal, section, index, count, callback, this._delegatePartialBodySection);
        }

        public byte[] EndPartialBodySection(IAsyncResult result)
        {
            return this._delegatePartialBodySection.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of Header().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array.</returns>
        public byte[] PartialHeader(int messageOrdinal, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.PartialHeaderString(messageOrdinal,index,count));
        }

        private delegate byte[] DelegatePartialHeader(int messageOrdinal, int index, int count);
        private DelegatePartialHeader _delegatePartialHeader;

        public IAsyncResult BeginPartialHeader(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialHeader = this.PartialHeader;
            return this._delegatePartialHeader.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialHeader);
        }

        public byte[] EndPartialHeader(IAsyncResult result)
        {
            return this._delegatePartialHeader.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of Message().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array.</returns>
        public byte[] PartialMessage(int messageOrdinal, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.PartialMessageString(messageOrdinal,index,count));
        }

        private delegate byte[] DelegatePartialMessage(int messageOrdinal, int index, int count);
        private DelegatePartialMessage _delegatePartialMessage;

        public IAsyncResult BeginPartialMessage(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialMessage = this.PartialMessage;
            return this._delegatePartialMessage.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialMessage);
        }

        public byte[] EndPartialMessage(IAsyncResult result)
        {
            return this._delegatePartialMessage.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of MessagePeek().
        /// Same as PartialMessage() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array.</returns>
        public byte[] PartialMessagePeek(int messageOrdinal, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.PartialMessageStringPeek(messageOrdinal, index, count));
        }

        private delegate byte[] DelegatePartialMessagePeek(int messageOrdinal, int index, int count);
        private DelegatePartialMessagePeek _delegatePartialMessagePeek;

        public IAsyncResult BeginPartialMessagePeek(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialMessagePeek = this.PartialMessagePeek;
            return this._delegatePartialMessagePeek.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialMessagePeek);
        }

        public byte[] EndPartialMessagePeek(IAsyncResult result)
        {
            return this._delegatePartialMessagePeek.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of Text().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array.</returns>
        public byte[] PartialText(int messageOrdinal, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.PartialTextString(messageOrdinal,index,count));
        }

        private delegate byte[] DelegatePartialText(int messageOrdinal, int index, int count);
        private DelegatePartialText _delegatePartialText;

        public IAsyncResult BeginPartialText(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialText = this.PartialText;
            return this._delegatePartialText.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialText);
        }

        public byte[] EndPartialText(IAsyncResult result)
        {
            return this._delegatePartialText.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of TextPeek().
        /// Same as PartialText() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array.</returns>
        public byte[] PartialTextPeek(int messageOrdinal, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetBytes(this.PartialTextStringPeek(messageOrdinal, index, count));
        }

        private delegate byte[] DelegatePartialTextPeek(int messageOrdinal, int index, int count);
        private DelegatePartialTextPeek _delegatePartialTextPeek;

        public IAsyncResult BeginPartialTextPeek(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialTextPeek = this.PartialTextPeek;
            return this._delegatePartialTextPeek.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialTextPeek);
        }

        public byte[] EndPartialTextPeek(IAsyncResult result)
        {
            return this._delegatePartialTextPeek.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of BodySectionString().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="section"></param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array converted to a string.</returns>
        public string PartialBodySectionString(int messageOrdinal, int section, int index, int count)
        {
            string response = this.ParentMailbox.SourceClient.Command("partial " + messageOrdinal.ToString() + " body[" + section + "] " + index.ToString() + " " + count.ToString(), getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n")-response.IndexOf("}")-3);
        }

        private delegate string DelegatePartialBodySectionString(int messageOrdinal, int section, int index, int count);
        private DelegatePartialBodySectionString _delegatePartialBodySectionString;

        public IAsyncResult BeginPartialBodySectionString(int messageOrdinal, int section, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialBodySectionString = this.PartialBodySectionString;
            return this._delegatePartialBodySectionString.BeginInvoke(messageOrdinal, section, index, count, callback, this._delegatePartialBodySectionString);
        }

        public string EndPartialBodySectionString(IAsyncResult result)
        {
            return this._delegatePartialBodySectionString.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of HeaderString().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array converted to a string.</returns>
        public string PartialHeaderString(int messageOrdinal, int index, int count)
        {
            string response = this.ParentMailbox.SourceClient.Command("partial " + messageOrdinal.ToString() + " rfc822.Header " + index.ToString() + " " + count.ToString(), getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n")-response.IndexOf("}")-3);
        }

        private delegate string DelegatePartialHeaderString(int messageOrdinal, int index, int count);
        private DelegatePartialHeaderString _delegatePartialHeaderString;

        public IAsyncResult BeginPartialHeaderString(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialHeaderString = this.PartialHeaderString;
            return this._delegatePartialHeaderString.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialHeaderString);
        }

        public string EndPartialHeaderString(IAsyncResult result)
        {
            return this._delegatePartialHeaderString.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of MessageString().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array converted to a string.</returns>
        public string PartialMessageString(int messageOrdinal, int index, int count)
        {
            string response = this.ParentMailbox.SourceClient.Command("partial " + messageOrdinal.ToString() + " rfc822 " + index.ToString() + " " + count.ToString(), getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n")-response.IndexOf("}")-3);
        }

        private delegate string DelegatePartialMessageString(int messageOrdinal, int index, int count);
        private DelegatePartialMessageString _delegatePartialMessageString;

        public IAsyncResult BeginPartialMessageString(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialMessageString = this.PartialMessageString;
            return this._delegatePartialMessageString.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialMessageString);
        }

        public string EndPartialMessageString(IAsyncResult result)
        {
            return this._delegatePartialMessageString.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of MessagePeekString().
        /// Same as PartialMessageString() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array converted to a string.</returns>
        public string PartialMessageStringPeek(int messageOrdinal, int index, int count)
        {
            string response = this.ParentMailbox.SourceClient.Command("partial " + messageOrdinal.ToString() + " rfc822.peek " + index.ToString() + " " + count.ToString(), getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n")-response.IndexOf("}")-3);
        }

        private delegate string DelegatePartialMessageStringPeek(int messageOrdinal, int index, int count);
        private DelegatePartialMessageStringPeek _delegatePartialMessageStringPeek;

        public IAsyncResult BeginPartialMessageStringPeek(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialMessageStringPeek = this.PartialMessageStringPeek;
            return this._delegatePartialMessageStringPeek.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialMessageStringPeek);
        }

        public string EndPartialMessageStringPeek(IAsyncResult result)
        {
            return this._delegatePartialMessageStringPeek.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of TextString().
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array converted to a string.</returns>
        public string PartialTextString(int messageOrdinal, int index, int count)
        {
            string response = this.ParentMailbox.SourceClient.Command("partial " + messageOrdinal.ToString() + " rfc822.text " + index.ToString() + " " + count.ToString(), getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n")-response.IndexOf("}")-3);
        }

        private delegate string DelegatePartialTextString(int messageOrdinal, int index, int count);
        private DelegatePartialTextString _delegatePartialTextString;

        public IAsyncResult BeginPartialTextString(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialTextString = this.PartialTextString;
            return this._delegatePartialTextString.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialTextString);
        }

        public string EndPartialTextString(IAsyncResult result)
        {
            return this._delegatePartialTextString.EndInvoke(result);
        }

        /// <summary>
        /// Fetches [count] bytes starting at [index].
        /// Partial version of TextPeekString().
        /// Same as PartialTextString() except that it doesn't set the Seen flag.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal position of the message to be fetched.</param>
        /// <param name="index">The byte index to start retrieving from.</param>
        /// <param name="count">The amount of bytes to be retrieved, starting at index.</param>
        /// <returns>The requested byte array converted to a string.</returns>
        public string PartialTextStringPeek(int messageOrdinal, int index, int count)
        {
            string response = this.ParentMailbox.SourceClient.Command("partial " + messageOrdinal.ToString() + " rfc822.text.peek " + index.ToString() + " " + count.ToString(), getFetchOptions());
            return response.Substring(response.IndexOf("}")+3,response.LastIndexOf("\r\n")-response.IndexOf("}")-3);
        }

        private delegate string DelegatePartialTextStringPeek(int messageOrdinal, int index, int count);
        private DelegatePartialTextStringPeek _delegatePartialTextStringPeek;

        public IAsyncResult BeginPartialTextStringPeek(int messageOrdinal, int index, int count, AsyncCallback callback)
        {
            this._delegatePartialTextStringPeek = this.PartialTextStringPeek;
            return this._delegatePartialTextStringPeek.BeginInvoke(messageOrdinal, index, count, callback, this._delegatePartialTextStringPeek);
        }

        public string EndPartialTextStringPeek(IAsyncResult result)
        {
            return this._delegatePartialTextStringPeek.EndInvoke(result);
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The Fetch's parent mailbox.
        /// </summary>
        public ActiveUp.Net.Mail.Mailbox ParentMailbox
        {
            get
            {
                return this._parentMailbox;
            }
            set
            {
                this._parentMailbox = value;
            }
        }
    
        #endregion
    }
    #endregion
}