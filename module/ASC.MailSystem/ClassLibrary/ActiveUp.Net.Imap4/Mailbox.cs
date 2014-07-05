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
using System.Collections.Generic;
using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Mail
{

    #region Mailbox

    /// <summary>
    /// Represents a mailbox.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Mailbox : IMailbox
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mailbox()
        {
            this.Fetch.ParentMailbox = this;
        }

        #endregion

        #region Private fields

        string _name;
        ActiveUp.Net.Mail.Imap4Client _imap;
        ActiveUp.Net.Mail.FlagCollection _applicableFlags = new ActiveUp.Net.Mail.FlagCollection();
        ActiveUp.Net.Mail.FlagCollection _permanentFlags = new ActiveUp.Net.Mail.FlagCollection();
        ActiveUp.Net.Mail.MailboxPermission _permission = ActiveUp.Net.Mail.MailboxPermission.Unknown;
        ActiveUp.Net.Mail.MailboxCollection _subMailboxes = new ActiveUp.Net.Mail.MailboxCollection();
        ActiveUp.Net.Mail.Fetch _fetcher = new ActiveUp.Net.Mail.Fetch();
        int _recent,_messageCount,_unseen,_uidvalidity;

        #endregion

        #region Methods

        #region Public methods

        /// <summary>
        /// Creates a child mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the child mailbox to be created.</param>
        /// <returns>The newly created mailbox.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// Mailbox staff = inbox.CreateChild("Staff");
        /// int zero = staff.MessageCount
        /// //Returns 0.
        /// inbox.Close();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// Dim staff As Mailbox = inbox.CreateChild("Staff")
        /// Dim zero As Integer = staff.MessageCount
        /// 'Returns 0.
        /// inbox.Close()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// var staff:Mailbox = inbox.CreateChild("Staff");
        /// var zero:int = staff.MessageCount
        /// //Returns 0.
        /// inbox.Close();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public IMailbox CreateChild(string mailboxName)
        {
            try
            {
                string separator = this.SourceClient.Command("list \"\" \"\"").Split('\"')[1].Split('\"')[0];
                return this.SourceClient.CreateMailbox(this.Name+separator+mailboxName);
            }
            catch(System.Net.Sockets.SocketException)
            {
                throw new Imap4Exception("CreateChild failed.\nThe mailbox' source client wasn't connected anymore.");
            }
        }

        private delegate IMailbox DelegateCreateChild(string mailboxName);
        private DelegateCreateChild _delegateCreateChild;

        public IAsyncResult BeginCreateChild(string mailboxName, AsyncCallback callback)
        {
            this._delegateCreateChild = this.CreateChild;
            return this._delegateCreateChild.BeginInvoke(mailboxName, callback, this._delegateCreateChild);
        }

        public IMailbox EndCreateChild(IAsyncResult result)
        {
            return this._delegateCreateChild.EndInvoke(result);
        }

        /// <summary>
        /// Subscribes to the mailbox.
        /// </summary>
        /// <returns>The server's response.</returns>
        public string Subscribe()
        {
            try
            {
                return this.SourceClient.SubscribeMailbox(this.Name);
            }
            catch(System.Net.Sockets.SocketException)
            {
                throw new Imap4Exception("Subscribe failed.\nThe mailbox' source client wasn't connected anymore.");
            }
        }

        private delegate string DelegateSubscribe();
        private DelegateSubscribe _delegateSubscribe;

        public IAsyncResult BeginSubscribe(AsyncCallback callback)
        {
            this._delegateSubscribe = this.Subscribe;
            return this._delegateSubscribe.BeginInvoke(callback, this._delegateSubscribe);
        }

        public string EndSubscribe(IAsyncResult result)
        {
            return this._delegateSubscribe.EndInvoke(result);
        }

        /// <summary>
        /// Unsubscribes from the mailbox.
        /// </summary>
        /// <returns>The server's response.</returns>
        public string Unsubscribe()
        {
            try
            {
                return this.SourceClient.UnsubscribeMailbox(this.Name);
            }
            catch(System.Net.Sockets.SocketException)
            {
                throw new Imap4Exception("Unsubscribe failed.\nThe mailbox' source client wasn't connected anymore.");
            }
        }

        private delegate string DelegateUnsubscribe();
        private DelegateUnsubscribe _delegateUnsubscribe;

        public IAsyncResult BeginUnsubscribe(AsyncCallback callback)
        {
            this._delegateUnsubscribe = this.Unsubscribe;
            return this._delegateUnsubscribe.BeginInvoke(callback, this._delegateUnsubscribe);
        }

        public string EndUnsubscribe(IAsyncResult result)
        {
            return this._delegateUnsubscribe.EndInvoke(result);
        }

        /// <summary>
        /// Deletes the mailbox.
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
        /// inbox.Delete();
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// inbox.Delete()
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// inbox.Delete();
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Delete()
        {
            try
            {
                return this.SourceClient.DeleteMailbox(this.Name);
            }
            catch(System.Net.Sockets.SocketException)
            {
                throw new ActiveUp.Net.Mail.Imap4Exception("Delete failed.\nThe mailbox' source client wasn't connected anymore.");
            }
        }

        private delegate string DelegateDelete();
        private DelegateDelete _delegateDelete;

        public IAsyncResult BeginDelete(AsyncCallback callback)
        {
            this._delegateDelete = this.Delete;
            return this._delegateDelete.BeginInvoke(callback, this._delegateDelete);
        }

        public string EndDelete(IAsyncResult result)
        {
            return this._delegateDelete.EndInvoke(result);
        }

        /// <summary>
        /// Renames the mailbox.
        /// </summary>
        /// <param name="newMailboxName">The new name of the mailbox.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("invox");
        /// inbox.Rename("inbox");
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("invox")
        /// inbox.Rename("inbox")
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("invox");
        /// inbox.Rename("inbox");
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Rename(string newMailboxName)
        {
            try
            {
                string response = this.SourceClient.RenameMailbox(this.Name,newMailboxName);
                this.Name = newMailboxName;
                return response;
            }
            catch(System.Net.Sockets.SocketException)
            {
                throw new Imap4Exception("Rename failed.\nThe mailbox' source client wasn't connected anymore.");
            }
        }

        private delegate string DelegateRename(string newMailboxName);
        private DelegateRename _delegateRename;

        public IAsyncResult BeginRename(string newMailboxName, AsyncCallback callback)
        {
            this._delegateRename = this.Rename;
            return this._delegateRename.BeginInvoke(newMailboxName, callback, this._delegateRename);
        }

        public string EndRename(IAsyncResult result)
        {
            return this._delegateRename.EndInvoke(result);
        }
        /// <summary>
        /// Searches the mailbox for messages corresponding to the query.
        /// </summary>
        /// <param name="query">Query to use.</param>
        /// <returns>An array of integers containing ordinal positions of messages matching the query.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// int[] ids = inbox.Search("SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith");
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// Dim ids() As Integer = inbox.Search("SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith")
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// var ids:int[] = inbox.Search("SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith");
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public int[] Search(string query)
        {
            string response = this.SourceClient.Command("search "+query);
            string[] parts = response.Substring(0,response.IndexOf("\r\n")).Split(' ');
            int[] messageOrdinals = new int[parts.Length-2];
            for(int i=2;i<parts.Length;i++) messageOrdinals[i-2] = System.Convert.ToInt32(parts[i]);
            return messageOrdinals;
        }

        /// <summary>
        /// Searches the mailbox for messages corresponding to the query.
        /// </summary>
        /// <param name="query">Query to use.</param>
        /// <returns>An array of integers containing uids of messages matching the query.</returns>
        public int[] UidSearch(string query)
        {
            var response = SourceClient.Command("UID SEARCH " + query);
            var parts = new List<string>(response.Substring(0, response.IndexOf("\r\n", StringComparison.Ordinal)).Split(' '));
            parts.Remove("");
            var messageOrdinals = new int[parts.Count - 2];
            for (var i = 2; i < parts.Count; i++) messageOrdinals[i - 2] = Convert.ToInt32(parts[i]);
            return messageOrdinals;
        }

        private delegate int[] DelegateSearch(string query);
        private DelegateSearch _delegateSearch;

        public IAsyncResult BeginSearch(string query, AsyncCallback callback)
        {
            this._delegateSearch = this.Search;
            return this._delegateSearch.BeginInvoke(query, callback, this._delegateSearch);
        }

        /// <summary>
        /// Search for messages accoridng to the given query.
        /// </summary>
        /// <param name="query">Query to use.</param>
        /// <returns>A collection of messages matching the query.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// MessageCollection messages = inbox.SearchParse("SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith");
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// Dim messages As MessageCollection = inbox.SearchParse("SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith")
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// var messages:MessageCollection = inbox.SearchParse("SEARCH FLAGGED SINCE 1-Feb-1994 NOT FROM "Smith");
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public MessageCollection SearchParse(string query)
        {
            ActiveUp.Net.Mail.MessageCollection msgs = new ActiveUp.Net.Mail.MessageCollection();
            foreach(int i in this.Search(query)) msgs.Add(this.Fetch.MessageObject(i));
            return msgs;
        }

        private delegate MessageCollection DelegateSearchParse(string query);
        private DelegateSearchParse _delegateSearchParse;

        public IAsyncResult BeginSearchParse(string query, AsyncCallback callback)
        {
            this._delegateSearchParse = this.SearchParse;
            return this._delegateSearchParse.BeginInvoke(query, callback, this._delegateSearchParse);
        }

        /// <summary>
        /// Searches the mailbox for messages corresponding to the query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="charset">The charset the query has to be performed for.</param>
        /// <returns>An array of integers containing ordinal positions of messages matching the query.</returns>
        public int[] Search(string charset, string query)
        {
            
            string response = this.SourceClient.Command("search charset "+charset+" "+query);
            string[] parts = response.Substring(0,response.IndexOf("\r\n")).Split(' ');
            int[] messageOrdinals = new int[parts.Length-2];
            for(int i=2;i<parts.Length;i++) messageOrdinals[i-2] = System.Convert.ToInt32(parts[i]);
            return messageOrdinals;
        }

        private delegate int[] DelegateSearchStringString(string charset, string query);
        private DelegateSearchStringString _delegateSearchStringString;

        public IAsyncResult BeginSearch(string charset, string query, AsyncCallback callback)
        {
            this._delegateSearchStringString = this.Search;
            return this._delegateSearchStringString.BeginInvoke(charset, query, callback, this._delegateSearchStringString);
        }

        public string EndSearch(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Search for messages accoridng to the given query.
        /// </summary>
        /// <param name="query">Query to use.</param>
        /// <param name="charset">The charset to apply the query for.</param>
        /// <returns>A collection of messages matching the query.</returns>
        public MessageCollection SearchParse(string charset, string query)
        {
            MessageCollection msgs = new MessageCollection();
            foreach(int i in this.Search(charset,query)) msgs.Add(this.Fetch.MessageObject(i));
            return msgs;
        }

        private delegate MessageCollection DelegateSearchParseStringString(string charset, string query);
        private DelegateSearchParseStringString _delegateSearchParseStringString;

        public IAsyncResult BeginSearchParse(string charset, string query, AsyncCallback callback)
        {
            this._delegateSearchParseStringString = this.SearchParse;
            return this._delegateSearchParseStringString.BeginInvoke(charset, query, callback, this._delegateSearchParseStringString);
        }

        public string EndSearchParse(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Adds the specified flags to the message.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be added to the message.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// FlagCollection flags = new FlagCollection();
        /// flags.Add("Draft");
        /// inbox.AddFlags(1,flags);
        /// //Message 1 is marked as draft.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// Dim flags As New FlagCollection
        /// flags.Add("Draft")
        /// inbox.AddFlags(1,flags)
        /// 'Message 1 is marked as draft.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// var flags:FlagCollection = new FlagCollection();
        /// flags.Add("Draft");
        /// inbox.AddFlags(1,flags);
        /// //Message is marked as draft.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string AddFlags(int messageOrdinal, IFlagCollection flags)
        {
            return this.SourceClient.Command("store " + messageOrdinal.ToString() + " +flags " + ((FlagCollection)flags).Merged);
        }

        private delegate string DelegateAddFlags(int messageOrdinal, IFlagCollection flags);
        private DelegateAddFlags _delegateAddFlags;

        public IAsyncResult BeginAddFlags(int messageOrdinal, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateAddFlags = this.AddFlags;
            return this._delegateAddFlags.BeginInvoke(messageOrdinal, flags, callback, this._delegateAddFlags);
        }

        public string EndAddFlags(IAsyncResult result)
        {
            return this._delegateAddFlags.EndInvoke(result);
        }

        public string UidAddFlags(int uid, IFlagCollection flags)
        {
            return this.SourceClient.Command("uid store " + uid.ToString() + " +flags " + ((FlagCollection)flags).Merged);
        }

        private delegate string DelegateUidAddFlags(int uid, IFlagCollection flags);
        private DelegateUidAddFlags _delegateUidAddFlags;

        public IAsyncResult BeginUidAddFlags(int uid, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateUidAddFlags = this.UidAddFlags;
            return this._delegateUidAddFlags.BeginInvoke(uid, flags, callback, this._delegateUidAddFlags);
        }

        public string EndUidAddFlags(IAsyncResult result)
        {
            return this._delegateUidAddFlags.EndInvoke(result);
        }

        /// <summary>
        /// Removes the specified flags from the message.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be removed from the message.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// FlagCollection flags = new FlagCollection();
        /// flags.Add("Read");
        /// inbox.RemoveFlags(1,flags);
        /// //Message 1 is marked as unread.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// Dim flags As New FlagCollection
        /// flags.Add("Read")
        /// inbox.RemoveFlags(1,flags)
        /// 'Message 1 is marked as unread.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// var flags:FlagCollection = new FlagCollection();
        /// flags.Add("Read");
        /// inbox.RemoveFlags(1,flags);
        /// //Message 1 is marked as unread.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string RemoveFlags(int messageOrdinal, IFlagCollection flags)
        {
            return this.SourceClient.Command("store " + messageOrdinal.ToString() + " -flags " + ((FlagCollection)flags).Merged);
        }

        private delegate string DelegateRemoveFlags(int messageOrdinal, IFlagCollection flags);
        private DelegateRemoveFlags _delegateRemoveFlags;

        public IAsyncResult BeginRemoveFlags(int messageOrdinal, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateRemoveFlags = this.RemoveFlags;
            return this._delegateRemoveFlags.BeginInvoke(messageOrdinal, flags, callback, this._delegateRemoveFlags);
        }

        public string EndRemoveFlags(IAsyncResult result)
        {
            return this._delegateRemoveFlags.EndInvoke(result);
        }

        public string UidRemoveFlags(int uid, ActiveUp.Net.Mail.IFlagCollection flags)
        {
            return this.SourceClient.Command("uid store " + uid.ToString() + " -flags " + ((FlagCollection)flags).Merged);
        }

        private delegate string DelegateUidRemoveFlags(int uid, IFlagCollection flags);
        private DelegateUidRemoveFlags _delegateUidRemoveFlags;

        public IAsyncResult BeginUidRemoveFlags(int uid, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateUidRemoveFlags = this.UidRemoveFlags;
            return this._delegateUidRemoveFlags.BeginInvoke(uid, flags, callback, this._delegateUidRemoveFlags);
        }

        public string EndUidRemoveFlags(IAsyncResult result)
        {
            return this._delegateUidRemoveFlags.EndInvoke(result);
        }
        /// <summary>
        /// Sets the specified flags for the message.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be stored for the message.</param>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// FlagCollection flags = new FlagCollection();
        /// flags.Add("Read");
        /// flags.Add("Answered");
        /// inbox.AddFlags(1,flags);
        /// //Message is marked as read and answered. All prior flags are unset.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// Dim flags As New FlagCollection
        /// flags.Add("Read")
        /// flags.Add("Answered")
        /// inbox.AddFlags(1,flags)
        /// 'Message is marked as read and answered. All prior flags are unset.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// var flags:FlagCollection = new FlagCollection();
        /// flags.Add("Read");
        /// flags.Add("Answered");
        /// inbox.AddFlags(1,flags);
        /// //Message is marked as read and answered. All prior flags are unset.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string SetFlags(int messageOrdinal, IFlagCollection flags)
        {
            return this.SourceClient.Command("store " + messageOrdinal.ToString() + " flags " + ((FlagCollection)flags).Merged);
        }

        private delegate string DelegateSetFlags(int messageOrdinal, IFlagCollection flags);
        private DelegateSetFlags _delegateSetFlags;

        public IAsyncResult BeginSetFlags(int messageOrdinal, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateSetFlags = this.SetFlags;
            return this._delegateSetFlags.BeginInvoke(messageOrdinal, flags, callback, this._delegateSetFlags);
        }

        public string EndSetFlags(IAsyncResult result)
        {
            return this._delegateSetFlags.EndInvoke(result);
        }

        public string UidSetFlags(int uid, ActiveUp.Net.Mail.IFlagCollection flags)
        {
            return this.SourceClient.Command("uid store " + uid.ToString() + " flags " + ((FlagCollection)flags).Merged);
        }

        private delegate string DelegateUidSetFlags(int uid, IFlagCollection flags);
        private DelegateUidSetFlags _delegateUidSetFlags;

        public IAsyncResult BeginUidSetFlags(int uid, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateUidSetFlags = this.UidSetFlags;
            return this._delegateUidSetFlags.BeginInvoke(uid, flags, callback, this._delegateUidSetFlags);
        }

        public string EndUidSetFlags(IAsyncResult result)
        {
            return this._delegateUidSetFlags.EndInvoke(result);
        }

        /// <summary>
        /// Same as AddFlags() except no response is requested.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be added to the message.</param>
        /// <example><see cref="Mailbox.AddFlags"/></example>
        public void AddFlagsSilent(int messageOrdinal, IFlagCollection flags)
        {
            this.SourceClient.Command("store " + messageOrdinal.ToString() + " +flags.silent " + ((FlagCollection)flags).Merged);
        }

        private delegate void DelegateAddFlagsSilent(int messageOrdinal, IFlagCollection flags);
        private DelegateAddFlagsSilent _delegateAddFlagsSilent;

        public IAsyncResult BeginAddFlagsSilent(int messageOrdinal, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateAddFlagsSilent = this.AddFlagsSilent;
            return this._delegateAddFlagsSilent.BeginInvoke(messageOrdinal, flags, callback, this._delegateAddFlagsSilent);
        }

        public void EndAddFlagsSilent(IAsyncResult result)
        {
            this._delegateAddFlagsSilent.EndInvoke(result);
        }

        public void UidAddFlagsSilent(int uid, IFlagCollection flags)
        {
            this.SourceClient.Command("uid store " + uid.ToString() + " +flags.silent " + ((FlagCollection)flags).Merged);
        }

        private delegate void DelegateUidAddFlagsSilent(int uid, IFlagCollection flags);
        private DelegateUidAddFlagsSilent _delegateUidAddFlagsSilent;

        public IAsyncResult BeginUidAddFlagsSilent(int uid, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateUidAddFlagsSilent = this.UidAddFlagsSilent;
            return this._delegateUidAddFlagsSilent.BeginInvoke(uid, flags, callback, this._delegateUidAddFlagsSilent);
        }

        public void EndUidAddFlagsSilent(IAsyncResult result)
        {
            this._delegateUidAddFlagsSilent.EndInvoke(result);
        }

        /// <summary>
        /// Same as RemoveFlags() except no response is requested.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be removed from the message.</param>
        /// <example><see cref="Mailbox.RemoveFlags"/></example>
        public void RemoveFlagsSilent(int messageOrdinal, ActiveUp.Net.Mail.IFlagCollection flags)
        {
            this.SourceClient.Command("store " + messageOrdinal.ToString() + " -flags.silent " + ((FlagCollection)flags).Merged);
        }

        private delegate void DelegateRemoveFlagsSilent(int messageOrdinal, IFlagCollection flags);
        private DelegateRemoveFlagsSilent _delegateRemoveFlagsSilent;

        public IAsyncResult BeginRemoveFlagsSilent(int messageOrdinal, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateRemoveFlagsSilent = this.RemoveFlagsSilent;
            return this._delegateRemoveFlagsSilent.BeginInvoke(messageOrdinal, flags, callback, this._delegateRemoveFlagsSilent);
        }

        public void EndRemoveFlagsSilent(IAsyncResult result)
        {
            this._delegateRemoveFlagsSilent.EndInvoke(result);
        }

        public void UidRemoveFlagsSilent(int uid, ActiveUp.Net.Mail.IFlagCollection flags)
        {
            this.SourceClient.Command("uid store " + uid.ToString() + " -flags.silent " + ((FlagCollection)flags).Merged);
        }

        private delegate void DelegateUidRemoveFlagsSilent(int uid, IFlagCollection flags);
        private DelegateUidRemoveFlagsSilent _delegateUidRemoveFlagsSilent;

        public IAsyncResult BeginUidRemoveFlagsSilent(int uid, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateUidRemoveFlagsSilent = this.UidRemoveFlagsSilent;
            return this._delegateUidRemoveFlagsSilent.BeginInvoke(uid, flags, callback, this._delegateUidRemoveFlagsSilent);
        }

        public void EndUidRemoveFlagsSilent(IAsyncResult result)
        {
            this._delegateUidRemoveFlagsSilent.EndInvoke(result);
        }

        /// <summary>
        /// Same as SetFlags() except no response is requested.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <example><see cref="Mailbox.SetFlags"/></example>
        public void SetFlagsSilent(int messageOrdinal, ActiveUp.Net.Mail.IFlagCollection flags)
        {
            this.SourceClient.Command("store " + messageOrdinal.ToString() + " flags.silent " + ((FlagCollection)flags).Merged);
        }

        private delegate void DelegateSetFlagsSilent(int messageOrdinal, IFlagCollection flags);
        private DelegateSetFlagsSilent _delegateSetFlagsSilent;

        public IAsyncResult BeginSetFlagsSilent(int messageOrdinal, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateSetFlagsSilent = this.SetFlagsSilent;
            return this._delegateSetFlagsSilent.BeginInvoke(messageOrdinal, flags, callback, this._delegateSetFlagsSilent);
        }

        public void EndSetFlagsSilent(IAsyncResult result)
        {
            this._delegateSetFlagsSilent.EndInvoke(result);
        }

        public void UidSetFlagsSilent(int uid, ActiveUp.Net.Mail.IFlagCollection flags)
        {
            this.SourceClient.Command("uid store " + uid.ToString() + " flags.silent " + ((FlagCollection)flags).Merged);
        }

        private delegate void DelegateUidSetFlagsSilent(int uid, IFlagCollection flags);
        private DelegateUidSetFlagsSilent _delegateUidSetFlagsSilent;

        public IAsyncResult BeginUidSetFlagsSilent(int uid, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateUidSetFlagsSilent = this.UidSetFlagsSilent;
            return this._delegateUidSetFlagsSilent.BeginInvoke(uid, flags, callback, this._delegateUidSetFlagsSilent);
        }

        public void EndUidSetFlagsSilent(IAsyncResult result)
        {
            this._delegateUidSetFlagsSilent.EndInvoke(result);
        }

        /// <summary>
        /// Copies the specified message to the specified mailbox.
        /// </summary>
        /// <param name="messageOrdinal">The ordinal of the message to be copied.</param>
        /// <param name="destinationMailboxName">The name of the destination mailbox.</param>
        /// <returns>The destination mailbox.</returns>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// inbox.CopyMessage(1,"Read Messages");
        /// //Copies message 1 to Read Messages mailbox.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// inbox.CopyMessage(1,"Read Messages")
        /// 'Copies message 1 to Read Messages mailbox.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// inbox.CopyMessage(1,"Read Messages");
        /// //Copies message 1 to Read Messages mailbox.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public void CopyMessage(int messageOrdinal, string destinationMailboxName)
        {
            this.SourceClient.Command("copy "+messageOrdinal.ToString()+" \""+destinationMailboxName+"\"");
        }

        private delegate void DelegateCopyMessage(int messageOrdinal, string destinationMailboxName);
        private DelegateCopyMessage _delegateCopyMessage;

        public IAsyncResult BeginCopyMessage(int messageOrdinal, string destinationMailboxName, AsyncCallback callback)
        {
            this._delegateCopyMessage = this.CopyMessage;
            return this._delegateCopyMessage.BeginInvoke(messageOrdinal, destinationMailboxName, callback, this._delegateCopyMessage);
        }

        public void EndCopyMessage(IAsyncResult result)
        {
            this._delegateCopyMessage.EndInvoke(result);
        }

        public void UidCopyMessage(int uid, string destinationMailboxName)
        {
            this.SourceClient.Command("uid copy "+uid.ToString()+" \""+destinationMailboxName+"\"");
        }

        private delegate void DelegateUidCopyMessage(int uid, string destinationMailboxName);
        private DelegateUidCopyMessage _delegateUidCopyMessage;

        public IAsyncResult BeginUidCopyMessage(int uid, string destinationMailboxName, AsyncCallback callback)
        {
            this._delegateUidCopyMessage = this.UidCopyMessage;
            return this._delegateUidCopyMessage.BeginInvoke(uid, destinationMailboxName, callback, this._delegateUidCopyMessage);
        }

        public void EndUidCopyMessage(IAsyncResult result)
        {
            this._delegateUidCopyMessage.EndInvoke(result);
        }

        public void MoveMessage(int messageOrdinal, string destinationMailboxName)
        {
            this.CopyMessage(messageOrdinal, destinationMailboxName);
            this.DeleteMessage(messageOrdinal, true);
        }

        private delegate void DelegateMoveMessage(int messageOrdinal, string destinationMailboxName);
        private DelegateMoveMessage _delegateMoveMessage;

        public IAsyncResult BeginMoveMessage(int messageOrdinal, string destinationMailboxName, AsyncCallback callback)
        {
            this._delegateMoveMessage = this.MoveMessage;
            return this._delegateMoveMessage.BeginInvoke(messageOrdinal, destinationMailboxName, callback, this._delegateMoveMessage);
        }

        public void EndMoveMessage(IAsyncResult result)
        {
            this._delegateMoveMessage.EndInvoke(result);
        }

        public void UidMoveMessage(int uid, string destinationMailboxName)
        {
            this.UidCopyMessage(uid, destinationMailboxName);
            this.UidDeleteMessage(uid, true);
        }

        private delegate void DelegateUidMoveMessage(int uid, string destinationMailboxName);
        private DelegateUidMoveMessage _delegateUidMoveMessage;

        public IAsyncResult BeginUidMoveMessage(int uid, string destinationMailboxName, AsyncCallback callback)
        {
            this._delegateUidMoveMessage = this.UidMoveMessage;
            return this._delegateUidMoveMessage.BeginInvoke(uid, destinationMailboxName, callback, this._delegateUidMoveMessage);
        }

        public void EndUidMoveMessage(IAsyncResult result)
        {
            this._delegateUidMoveMessage.EndInvoke(result);
        }


        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageLiteral">The message in a Rfc822 compliant format.</param>
        public string Append(string messageLiteral)
        {
            string firststamp = System.DateTime.Now.ToString("yyMMddhhmmss"+System.DateTime.Now.Millisecond.ToString());
            this.SourceClient.Command("APPEND \""+this.Name+"\" {"+messageLiteral.Length+"}",firststamp);
            return this.SourceClient.Command(messageLiteral,"",firststamp);
        }

        private delegate string DelegateAppend(string messageLiteral);
        private DelegateAppend _delegateAppend;

        public IAsyncResult BeginAppend(string messageLiteral, AsyncCallback callback)
        {
            this._delegateAppend = this.Append;
            return this._delegateAppend.BeginInvoke(messageLiteral, callback, this._delegateAppend);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageLiteral">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        public string Append(string messageLiteral, IFlagCollection flags)
        {
            string firststamp = System.DateTime.Now.ToString("yyMMddhhmmss"+System.DateTime.Now.Millisecond.ToString());
            this.SourceClient.Command("APPEND \"" + this.Name + "\" " + ((FlagCollection)flags).Merged + " {" + (messageLiteral.Length) + "}", firststamp);
            return this.SourceClient.Command(messageLiteral,"",firststamp);
        }

        private delegate string DelegateAppendFlags(string messageLiteral, IFlagCollection flags);
        private DelegateAppendFlags _delegateAppendFlags;

        public IAsyncResult BeginAppend(string messageLiteral, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateAppendFlags = this.Append;
            return this._delegateAppendFlags.BeginInvoke(messageLiteral, flags, callback, this._delegateAppendFlags);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageLiteral">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <param name="dateTime">The internal date to be set for the message.</param>
        public string Append(string messageLiteral, IFlagCollection flags, DateTime dateTime)
        {
            string firststamp = System.DateTime.Now.ToString("yyMMddhhmmss"+System.DateTime.Now.Millisecond.ToString());
            this.SourceClient.Command("APPEND \"" + this.Name + "\" " + ((FlagCollection)flags).Merged + " " + dateTime.ToString("r") + " {" + (messageLiteral.Length) + "}", firststamp);
            return this.SourceClient.Command(messageLiteral,"",firststamp);
        }

        private delegate string DelegateAppendFlagsDateTime(string messageLiteral, IFlagCollection flags, DateTime dateTime);
        private DelegateAppendFlagsDateTime _delegateAppendFlagsDateTime;

        public IAsyncResult BeginAppend(string messageLiteral, IFlagCollection flags, DateTime dateTime, AsyncCallback callback)
        {
            this._delegateAppendFlagsDateTime = this.Append;
            return this._delegateAppendFlagsDateTime.BeginInvoke(messageLiteral, flags, dateTime, callback, this._delegateAppendFlagsDateTime);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="message">The message to be appended.</param>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Message message = new Message();
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.Subject = "hey!";
        /// message.Attachments.Add("C:\\myfile.doc");
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// Mailbox inbox = imap.SelectMailbox("inbox");
        /// inbox.Append(message);
        /// imap.Disconnect();
        ///  
        /// VB.NET
        ///  
        /// Dim message As New Message
        /// message.From = new Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.Subject = "hey!"
        /// message.Attachments.Add("C:\myfile.doc")
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        /// 
        /// Dim imap As New Imap4Client
        /// Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        /// inbox.Append(message)
        /// imap.Disconnect()
        ///   
        /// JScript.NET
        ///  
        /// var message:Message = new Message();
        /// message.From = new Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.Subject = "hey!";
        /// message.Attachments.Add("C:\\myfile.doc");
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        /// 
        /// var imap:Imap4Client = new Imap4Client();
        /// var inbox:Mailbox = imap.SelectMailbox("inbox");
        /// inbox.Append(message);
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Append(Message message)
        {
            return this.Append(message.ToMimeString());
        }

        private delegate string DelegateAppendMessage(Message message);
        private DelegateAppendMessage _delegateAppendMessage;

        public IAsyncResult BeginAppend(Message message, AsyncCallback callback)
        {
            this._delegateAppendMessage = this.Append;
            return this._delegateAppendMessage.BeginInvoke(message, callback, this._delegateAppendMessage);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="message">The message to be appended.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Message message = new Message();
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.Subject = "hey!";
        /// message.Attachments.Add("C:\\myfile.doc");
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        /// 
        /// FlagCollection flags = new FlagCollection();
        /// flags.Add("Read");
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// Mailbox inbox = imap.SelectMailbox("Read Messages");
        /// inbox.Append(message,flags);
        /// imap.Disconnect();
        ///  
        /// VB.NET
        ///  
        /// Dim message As New Message
        /// message.From = new Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.Subject = "hey!"
        /// message.Attachments.Add("C:\myfile.doc")
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        /// 
        /// Dim flags As New FlagCollection
        /// flags.Add("Read")
        ///  
        /// Dim imap As New Imap4Client
        /// Dim inbox As Mailbox = imap.SelectMailbox("Read Messages")
        /// inbox.Append(message,flags)
        /// imap.Disconnect()
        ///   
        /// JScript.NET
        ///  
        /// var message:Message = new Message();
        /// message.From = new Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.Subject = "hey!";
        /// message.Attachments.Add("C:\\myfile.doc");
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        /// 
        /// var flags:FlagCollection = new FlagCollection();
        /// flags.Add("Read");
        ///  
        /// var imap:Imap4Client = new Imap4Client();
        /// var inbox:Mailbox = imap.SelectMailbox("Read Messages");
        /// inbox.Append(message,flags);
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Append(Message message, IFlagCollection flags)
        {
            return this.Append(message.ToMimeString(),flags);
        }

        private delegate string DelegateAppendMessageFlags(Message message, IFlagCollection flags);
        private DelegateAppendMessageFlags _delegateAppendMessageFlags;

        public IAsyncResult BeginAppend(Message message, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateAppendMessageFlags = this.Append;
            return this._delegateAppendMessageFlags.BeginInvoke(message, flags, callback, this._delegateAppendMessageFlags);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="message">The message to be appended.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <param name="dateTime">The internal date to be set for the message.</param>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Message message = new Message();
        /// message.From = new Address("jdoe@myhost.com","John Doe");
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.Subject = "hey!";
        /// message.Attachments.Add("C:\\myfile.doc");
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        /// 
        /// FlagCollection flags = new FlagCollection();
        /// flags.Add("Read");
        /// 
        /// Imap4Client imap = new Imap4Client();
        /// Mailbox inbox = imap.SelectMailbox("Read Messages");
        /// inbox.Append(message,flags,System.DateTime.Now);
        /// imap.Disconnect();
        ///  
        /// VB.NET
        ///  
        /// Dim message As New Message
        /// message.From = new Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns")
        /// message.Subject = "hey!"
        /// message.Attachments.Add("C:\myfile.doc")
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        /// 
        /// Dim flags As New FlagCollection
        /// flags.Add("Read")
        ///  
        /// Dim imap As New Imap4Client
        /// Dim inbox As Mailbox = imap.SelectMailbox("Read Messages")
        /// inbox.Append(message,flags,System.DateTime.Now)
        /// imap.Disconnect()
        ///   
        /// JScript.NET
        ///  
        /// var message:Message = new Message();
        /// message.From = new Address("jdoe@myhost.com","John Doe")
        /// message.To.Add("mjohns@otherhost.com","Mike Johns");
        /// message.Subject = "hey!";
        /// message.Attachments.Add("C:\\myfile.doc");
        /// message.HtmlBody.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        /// 
        /// var flags:FlagCollection = new FlagCollection();
        /// flags.Add("Read");
        ///  
        /// var imap:Imap4Client = new Imap4Client();
        /// var inbox:Mailbox = imap.SelectMailbox("Read Messages");
        /// inbox.Append(message,flags,System.DateTime.Now);
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public string Append(Message message, IFlagCollection flags, DateTime dateTime)
        {
            return this.Append(message.ToMimeString(),flags,dateTime);
        }

        private delegate string DelegateAppendMessageFlagsDateTime(Message message, IFlagCollection flags, DateTime dateTime);
        private DelegateAppendMessageFlagsDateTime _delegateAppendMessageFlagsDateTime;

        public IAsyncResult BeginAppend(Message message, IFlagCollection flags, DateTime dateTime, AsyncCallback callback)
        {
            this._delegateAppendMessageFlagsDateTime = this.Append;
            return this._delegateAppendMessageFlagsDateTime.BeginInvoke(message, flags, dateTime, callback, this._delegateAppendMessageFlagsDateTime);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageData">The message in a Rfc822 compliant format.</param>
        public string Append(byte[] messageData)
        {
            return this.Append(System.Text.Encoding.UTF8.GetString(messageData,0,messageData.Length));
        }

        private delegate string DelegateAppendByte(byte[] messageData);
        private DelegateAppendByte _delegateAppendByte;

        public IAsyncResult BeginAppend(byte[] messageData, AsyncCallback callback)
        {
            this._delegateAppendByte = this.Append;
            return this._delegateAppendByte.BeginInvoke(messageData, callback, this._delegateAppendByte);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageData">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        public string Append(byte[] messageData, IFlagCollection flags)
        {
            return this.Append(System.Text.Encoding.UTF8.GetString(messageData,0,messageData.Length),flags);
        }

        private delegate string DelegateAppendByteFlags(byte[] messageData, IFlagCollection flags);
        private DelegateAppendByteFlags _delegateAppendByteFlags;

        public IAsyncResult BeginAppend(byte[] messageData, IFlagCollection flags, AsyncCallback callback)
        {
            this._delegateAppendByteFlags = this.Append;
            return this._delegateAppendByteFlags.BeginInvoke(messageData, flags, callback, this._delegateAppendByteFlags);
        }

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageData">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <param name="dateTime">The internal date to be set for the message.</param>
        public string Append(byte[] messageData, IFlagCollection flags, DateTime dateTime)
        {
            return this.Append(System.Text.Encoding.UTF8.GetString(messageData,0,messageData.Length),flags,dateTime);
        }

        private delegate string DelegateAppendByteFlagsDateTime(byte[] messageData, IFlagCollection flags, DateTime dateTime);
        private DelegateAppendByteFlagsDateTime _delegateAppendByteFlagsDateTime;

        public IAsyncResult BeginAppend(byte[] messageData, IFlagCollection flags, DateTime dateTime, AsyncCallback callback)
        {
            this._delegateAppendByteFlagsDateTime = this.Append;
            return this._delegateAppendByteFlagsDateTime.BeginInvoke(messageData, flags, dateTime, callback, this._delegateAppendByteFlagsDateTime);
        }

        public string EndAppend(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Empties the mailbox.
        /// </summary>
        /// <param name="expunge">If true, all messages are permanently removed. Otherwise they are all marked with the Deleted flag.</param>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// inbox.Empty(true);
        /// //Messages from inbox are permanently removed.
        /// imap.Disconnect();
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// inbox.Empty(True)
        /// 'Messages from inbox are permanently removed.
        /// imap.Disconnect()
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// inbox.Empty(true);
        /// //Messages from inbox are permanently removed.
        /// imap.Disconnect();
        /// </code>
        /// </example>
        public void Empty(bool expunge)
        {
            
            ActiveUp.Net.Mail.FlagCollection flags = new ActiveUp.Net.Mail.FlagCollection();
            flags.Add("Deleted");
            for(int i=1;i<=this.MessageCount;i++) this.AddFlagsSilent(i,flags);
            if(expunge) this.SourceClient.Expunge();
        }

        private delegate void DelegateEmpty(bool expunge);
        private DelegateEmpty _delegateEmpty;

        public IAsyncResult BeginEmpty(bool expunge, AsyncCallback callback)
        {
            this._delegateEmpty = this.Empty;
            return this._delegateEmpty.BeginInvoke(expunge, callback, this._delegateEmpty);
        }

        public void EndEmpty(IAsyncResult result)
        {
            this._delegateEmpty.EndInvoke(result);
        }

        /// <summary>
        /// Deletes the specified message.
        /// </summary>
        /// <param name="messageOrdinal">Ordinal position of the message to be deleted.</param>
        /// <param name="expunge">If true, message is permanently removed. Otherwise it is marked with the Deleted flag.</param>
        /// <example>
        /// <code>
        /// C#
        ///  
        /// Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// Mailbox inbox = imap.SelectInbox("inbox");
        /// inbox.Delete(1,false);
        /// //Message 1 has been marked for deletion but is not deleted yet.
        /// imap.Disconnect();
        /// //Message 1 is now permanently removed.
        /// 
        /// VB.NET
        ///  
        /// Dim imap As New Imap4Client
        /// imap.Connect("mail.myhost.com")
        /// imap.Login("jdoe1234","tanstaaf")
        /// Dim inbox As Mailbox = imap.SelectInbox("inbox")
        /// inbox.Delete(1,False)
        /// 'Message 1 has been marked for deletion but is not deleted yet.
        /// imap.Disconnect()
        /// 'Message 1 is now permanently removed.
        /// 
        /// JScript.NET
        ///  
        /// var imap:Imap4Client imap = new Imap4Client();
        /// imap.Connect("mail.myhost.com");
        /// imap.Login("jdoe1234","tanstaaf");
        /// var inbox:Mailbox = imap.SelectInbox("inbox");
        /// inbox.Delete(1,false);
        /// //Message 1 has been marked for deletion but is not deleted yet.
        /// imap.Disconnect();
        /// //Message 1 is now permanently removed.
        /// </code>
        /// </example>
        public void DeleteMessage(int messageOrdinal, bool expunge)
        {    
            ActiveUp.Net.Mail.FlagCollection flags = new ActiveUp.Net.Mail.FlagCollection();
            flags.Add("Deleted");
            this.AddFlagsSilent(messageOrdinal,flags);
            if(expunge) this.SourceClient.Expunge();
        }

        private delegate void DelegateDeleteMessage(int messageOrdinal, bool expunge);
        private DelegateDeleteMessage _delegateDeleteMessage;

        public IAsyncResult BeginDeleteMessage(int messageOrdinal, bool expunge, AsyncCallback callback)
        {
            this._delegateDeleteMessage = this.DeleteMessage;
            return this._delegateDeleteMessage.BeginInvoke(messageOrdinal, expunge, callback, this._delegateDeleteMessage);
        }

        public void EndDeleteMessage(IAsyncResult result)
        {
            this._delegateDeleteMessage.EndInvoke(result);
        }

        public void UidDeleteMessage(int uid, bool expunge)
        {    
            ActiveUp.Net.Mail.FlagCollection flags = new ActiveUp.Net.Mail.FlagCollection();
            flags.Add("Deleted");
            this.UidAddFlagsSilent(uid,flags);
            if(expunge) this.SourceClient.Expunge();
        }

        private delegate void DelegateUidDeleteMessage(int uid, bool expunge);
        private DelegateUidDeleteMessage _delegateUidDeleteMessage;

        public IAsyncResult BeginUidDeleteMessage(int uid, bool expunge, AsyncCallback callback)
        {
            this._delegateUidDeleteMessage = this.UidDeleteMessage;
            return this._delegateUidDeleteMessage.BeginInvoke(uid, expunge, callback, this._delegateUidDeleteMessage);
        }

        public void EndUidDeleteMessage(IAsyncResult result)
        {
            this._delegateUidDeleteMessage.EndInvoke(result);
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The Imap4Client object that will be used to perform commands on the server.
        /// </summary>
        public ActiveUp.Net.Mail.Imap4Client SourceClient
        {
            get
            {
                return this._imap;
            }
            set
            {

                this._imap = value;
            }
        }
        /// <summary>
        /// The full (hierarchical) name of the mailbox.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
        /// <summary>
        /// The name of the mailbox, without hierarchy.
        /// </summary>
        public string ShortName
        {
            get
            {
                return this._name.Substring(this._name.LastIndexOf("/")+1);
            }
        }
        /// <summary>
        /// The amount of recent messages (messages that have been added since this mailbox was last checked).
        /// </summary>
        public int Recent
        {
            get
            {
                return this._recent;
            }
            set
            {
                this._recent = value;
            }
        }
        /// <summary>
        /// The amount of messages in the mailbox.
        /// </summary>
        public int MessageCount
        {
            get
            {
                return this._messageCount;
            }
            set
            {
                this._messageCount = value;
            }
        }
        /// <summary>
        /// The ordinal position of the first unseen message in the mailbox.
        /// </summary>
        public int FirstUnseen
        {
            get
            {
                return this._unseen;
            }
            set
            {
                this._unseen = value;
            }
        }
        /// <summary>
        /// The Uid Validity number. This number allows to check if Unique Identifiers have changed since the mailbox was last checked.
        /// </summary>
        public int UidValidity
        {
            get
            {
                return this._uidvalidity;
            }
            set
            {
                this._uidvalidity = value;
            }
        }
        /// <summary>
        /// Flags that are applicable in this mailbox.
        /// </summary>
        public FlagCollection ApplicableFlags
        {
            get
            {
                return this._applicableFlags;
            }
            set
            {
                this._applicableFlags = value;
            }
        }
        /// <summary>
        /// Flags that the client can permanently set in this mailbox.
        /// </summary>
        public FlagCollection PermanentFlags
        {
            get
            {
                return this._permanentFlags;
            }
            set
            {
                this._permanentFlags = value;
            }
        }
        /// <summary>
        /// The mailbox's permission (ReadWrite or ReadOnly)
        /// </summary>
        public MailboxPermission Permission
        {
            get
            {
                return this._permission;
            }
            set
            {
                this._permission = value;
            }
        }
        /// <summary>
        /// The mailbox's child mailboxes.
        /// </summary>
        public MailboxCollection SubMailboxes
        {
            get
            {
                return this._subMailboxes;
            }
            set
            {
                this._subMailboxes = value;
            }
        }
        /// <summary>
        /// The mailbox's fetching utility.
        /// </summary>
        public Fetch Fetch
        {
            get
            {
                return this._fetcher;
            }
            set
            {
                this._fetcher = value;
            }
        }

        #endregion

    }

    #endregion

}