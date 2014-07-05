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

namespace ActiveUp.Net.Mail
{

    #region Mailbox

    /// <summary>
    /// Represents a mailbox.
    /// </summary>
    public interface IMailbox
    {
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
        IMailbox CreateChild(string mailboxName);

        IAsyncResult BeginCreateChild(string mailboxName, AsyncCallback callback);

        IMailbox EndCreateChild(IAsyncResult result);

        /// <summary>
        /// Subscribes to the mailbox.
        /// </summary>
        /// <returns>The server's response.</returns>
        string Subscribe();

        IAsyncResult BeginSubscribe(AsyncCallback callback);

        string EndSubscribe(IAsyncResult result);

        /// <summary>
        /// Unsubscribes from the mailbox.
        /// </summary>
        /// <returns>The server's response.</returns>
        string Unsubscribe();

        IAsyncResult BeginUnsubscribe(AsyncCallback callback);

        string EndUnsubscribe(IAsyncResult result);

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
        string Delete();

        IAsyncResult BeginDelete(AsyncCallback callback);

        string EndDelete(IAsyncResult result);

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
        string Rename(string newMailboxName);

        IAsyncResult BeginRename(string newMailboxName, AsyncCallback callback);

        string EndRename(IAsyncResult result);

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
        int[] Search(string query);

        IAsyncResult BeginSearch(string query, AsyncCallback callback);

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
        MessageCollection SearchParse(string query);

        IAsyncResult BeginSearchParse(string query, AsyncCallback callback);

        /// <summary>
        /// Searches the mailbox for messages corresponding to the query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="charset">The charset the query has to be performed for.</param>
        /// <returns>An array of integers containing ordinal positions of messages matching the query.</returns>
        int[] Search(string charset, string query);

        IAsyncResult BeginSearch(string charset, string query, AsyncCallback callback);

        string EndSearch(IAsyncResult result);

        /// <summary>
        /// Search for messages accoridng to the given query.
        /// </summary>
        /// <param name="query">Query to use.</param>
        /// <param name="charset">The charset to apply the query for.</param>
        /// <returns>A collection of messages matching the query.</returns>
        MessageCollection SearchParse(string charset, string query);

        IAsyncResult BeginSearchParse(string charset, string query, AsyncCallback callback);

        string EndSearchParse(IAsyncResult result);

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
        string AddFlags(int messageOrdinal, IFlagCollection flags);

        IAsyncResult BeginAddFlags(int messageOrdinal, IFlagCollection flags, AsyncCallback callback);

        string EndAddFlags(IAsyncResult result);

        string UidAddFlags(int uid, IFlagCollection flags);

        IAsyncResult BeginUidAddFlags(int uid, IFlagCollection flags, AsyncCallback callback);

        string EndUidAddFlags(IAsyncResult result);

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
        string RemoveFlags(int messageOrdinal, IFlagCollection flags);

        IAsyncResult BeginRemoveFlags(int messageOrdinal, IFlagCollection flags, AsyncCallback callback);

        string EndRemoveFlags(IAsyncResult result);

        string UidRemoveFlags(int uid, ActiveUp.Net.Mail.IFlagCollection flags);

        IAsyncResult BeginUidRemoveFlags(int uid, IFlagCollection flags, AsyncCallback callback);

        string EndUidRemoveFlags(IAsyncResult result);

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
        string SetFlags(int messageOrdinal, IFlagCollection flags);

        IAsyncResult BeginSetFlags(int messageOrdinal, IFlagCollection flags, AsyncCallback callback);

        string EndSetFlags(IAsyncResult result);

        string UidSetFlags(int uid, ActiveUp.Net.Mail.IFlagCollection flags);

        IAsyncResult BeginUidSetFlags(int uid, IFlagCollection flags, AsyncCallback callback);

        string EndUidSetFlags(IAsyncResult result);

        /// <summary>
        /// Same as AddFlags() except no response is requested.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be added to the message.</param>
        void AddFlagsSilent(int messageOrdinal, IFlagCollection flags);

        IAsyncResult BeginAddFlagsSilent(int messageOrdinal, IFlagCollection flags, AsyncCallback callback);

        void EndAddFlagsSilent(IAsyncResult result);

        void UidAddFlagsSilent(int uid, IFlagCollection flags);

        IAsyncResult BeginUidAddFlagsSilent(int uid, IFlagCollection flags, AsyncCallback callback);

        void EndUidAddFlagsSilent(IAsyncResult result);

        /// <summary>
        /// Same as RemoveFlags() except no response is requested.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be removed from the message.</param>
        void RemoveFlagsSilent(int messageOrdinal, ActiveUp.Net.Mail.IFlagCollection flags);

        IAsyncResult BeginRemoveFlagsSilent(int messageOrdinal, IFlagCollection flags, AsyncCallback callback);

        void EndRemoveFlagsSilent(IAsyncResult result);

        void UidRemoveFlagsSilent(int uid, ActiveUp.Net.Mail.IFlagCollection flags);

        IAsyncResult BeginUidRemoveFlagsSilent(int uid, IFlagCollection flags, AsyncCallback callback);

        void EndUidRemoveFlagsSilent(IAsyncResult result);

        /// <summary>
        /// Same as SetFlags() except no response is requested.
        /// </summary>
        /// <param name="messageOrdinal">The message's ordinal position.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        void SetFlagsSilent(int messageOrdinal, ActiveUp.Net.Mail.IFlagCollection flags);

        IAsyncResult BeginSetFlagsSilent(int messageOrdinal, IFlagCollection flags, AsyncCallback callback);

        void EndSetFlagsSilent(IAsyncResult result);

        void UidSetFlagsSilent(int uid, ActiveUp.Net.Mail.IFlagCollection flags);

        IAsyncResult BeginUidSetFlagsSilent(int uid, IFlagCollection flags, AsyncCallback callback);

        void EndUidSetFlagsSilent(IAsyncResult result);

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
        void CopyMessage(int messageOrdinal, string destinationMailboxName);

        IAsyncResult BeginCopyMessage(int messageOrdinal, string destinationMailboxName, AsyncCallback callback);

        void EndCopyMessage(IAsyncResult result);

        void UidCopyMessage(int uid, string destinationMailboxName);

        IAsyncResult BeginUidCopyMessage(int uid, string destinationMailboxName, AsyncCallback callback);

        void EndUidCopyMessage(IAsyncResult result);

        void MoveMessage(int messageOrdinal, string destinationMailboxName);

        IAsyncResult BeginMoveMessage(int messageOrdinal, string destinationMailboxName, AsyncCallback callback);

        void EndMoveMessage(IAsyncResult result);

        void UidMoveMessage(int uid, string destinationMailboxName);

        IAsyncResult BeginUidMoveMessage(int uid, string destinationMailboxName, AsyncCallback callback);

        void EndUidMoveMessage(IAsyncResult result);


        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageLiteral">The message in a Rfc822 compliant format.</param>
        string Append(string messageLiteral);

        IAsyncResult BeginAppend(string messageLiteral, AsyncCallback callback);

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageLiteral">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        string Append(string messageLiteral, IFlagCollection flags);

        IAsyncResult BeginAppend(string messageLiteral, IFlagCollection flags, AsyncCallback callback);

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageLiteral">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <param name="dateTime">The internal date to be set for the message.</param>
        string Append(string messageLiteral, IFlagCollection flags, DateTime dateTime);

        IAsyncResult BeginAppend(string messageLiteral, IFlagCollection flags, DateTime dateTime, AsyncCallback callback);

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
        string Append(Message message);

        IAsyncResult BeginAppend(Message message, AsyncCallback callback);

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
        string Append(Message message, IFlagCollection flags);

        IAsyncResult BeginAppend(Message message, IFlagCollection flags, AsyncCallback callback);

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
        string Append(Message message, IFlagCollection flags, DateTime dateTime);

        IAsyncResult BeginAppend(Message message, IFlagCollection flags, DateTime dateTime, AsyncCallback callback);

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageData">The message in a Rfc822 compliant format.</param>
        string Append(byte[] messageData);

        IAsyncResult BeginAppend(byte[] messageData, AsyncCallback callback);

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageData">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        string Append(byte[] messageData, IFlagCollection flags);

        IAsyncResult BeginAppend(byte[] messageData, IFlagCollection flags, AsyncCallback callback);

        /// <summary>
        /// Appends the provided message to the mailbox.
        /// </summary>
        /// <param name="messageData">The message in a Rfc822 compliant format.</param>
        /// <param name="flags">Flags to be set for the message.</param>
        /// <param name="dateTime">The internal date to be set for the message.</param>
        string Append(byte[] messageData, IFlagCollection flags, DateTime dateTime);

        IAsyncResult BeginAppend(byte[] messageData, IFlagCollection flags, DateTime dateTime, AsyncCallback callback);

        string EndAppend(IAsyncResult result);

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
        void Empty(bool expunge);

        IAsyncResult BeginEmpty(bool expunge, AsyncCallback callback);

        void EndEmpty(IAsyncResult result);

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
        void DeleteMessage(int messageOrdinal, bool expunge);

        IAsyncResult BeginDeleteMessage(int messageOrdinal, bool expunge, AsyncCallback callback);

        void EndDeleteMessage(IAsyncResult result);

        void UidDeleteMessage(int uid, bool expunge);

        IAsyncResult BeginUidDeleteMessage(int uid, bool expunge, AsyncCallback callback);

        void EndUidDeleteMessage(IAsyncResult result);

        #endregion

        #endregion

    }

    #endregion

}