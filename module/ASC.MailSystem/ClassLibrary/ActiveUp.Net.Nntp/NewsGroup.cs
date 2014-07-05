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

    #region NewsGroup Object

    /// <summary>
    /// Represents a newsgroup.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class NewsGroup
    {

        #region Private fields

        string _name;
        int _firstArticle, _lastArticle, _pointer;
        bool _postingAllowed;
        ActiveUp.Net.Mail.NntpClient _nntp;

        #endregion

        #region Constructors

        internal NewsGroup(string name, int firstArticle, int lastArticle, bool postingAllowed, ActiveUp.Net.Mail.NntpClient nntp)
        {
            this._name = name;
            this._firstArticle = firstArticle;
            this._lastArticle = lastArticle;
            this._postingAllowed = postingAllowed;
            this._nntp = nntp;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The newsgroup's name.
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
        /// The ordinal position of the newsgroup's first article.
        /// </summary>
        public int FirstArticle
        {
            get
            {
                return this._firstArticle;
            }
            set
            {
                this._firstArticle = value;
            }
        }
        /// <summary>
        /// The ordinal position of the newsgroup's last article.
        /// </summary>
        public int LastArticle
        {
            get
            {
                return this._lastArticle;
            }
            set
            {
                this._lastArticle = value;
            }
        }
        /// <summary>
        /// The current article pointer's position.
        /// </summary>
        public int Pointer
        {
            get
            {
                return this._pointer;
            }
            set
            {
                this._nntp.Command("stat "+value.ToString());
                this._pointer = value;
            }
        }
        /// <summary>
        /// True if posting is allowed on this newsgroup.
        /// </summary>
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
        /// <summary>
        /// The amount of article in this newsgroup.
        /// </summary>
        public int ArticleCount
        {
            get
            {
                return this._lastArticle-this._firstArticle+1;
            }
        }

        #endregion

        #region Methods

        #region Public methods

        /// <summary>
        /// Advances the current article pointer to the next article.
        /// </summary>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //group.Pointer is equal to group.FirstArticle.
        /// group.Next();
        /// //group.Pointer is now equal to group.FirstArticle + 1.
        /// //Retrieve the second article in this group.
        /// Message article2 = group.RetrieveArticleObject();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'group.Pointer is equal to group.FirstArticle.
        /// group.Next()
        /// 'group.Pointer is now equal to group.FirstArticle + 1.
        /// 'Retrieve the second article in this group.
        /// Dim article2 As Message = group.RetrieveArticleObject()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //group.Pointer is equal to group.FirstArticle.
        /// group.Next();
        /// //group.Pointer is now equal to group.FirstArticle + 1.
        /// //Retrieve the second article in this group.
        /// var article2:Message = group.RetrieveArticleObject();
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string Next()
        {
            string response = this._nntp.Command("next");
            if(response.StartsWith("223")) this.Pointer = System.Convert.ToInt32(response.Split(' ')[1]);
            return response;
        }

        private delegate string DelegateNext();
        private DelegateNext _delegateNext;

        public IAsyncResult BeginNext(AsyncCallback callback)
        {
            this._delegateNext = this.Next;
            return this._delegateNext.BeginInvoke(callback, this._delegateNext);
        }

        public string EndNext(IAsyncResult result)
        {
            return this._delegateNext.EndInvoke(result);
        }

        /// <summary>
        /// Steps back the current article pointer to the previous article.
        /// </summary>
        /// <returns>The server's response.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //group.Pointer is equal to group.FirstArticle.
        /// group.Next();
        /// //group.Pointer is now equal to group.FirstArticle + 1.
        /// group.Previous();
        /// //group.Pointer is now equal to group.FirstArticle.
        /// //This retrieves the first article of the group.
        /// Message article1 = group.RetrieveArticleObject();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'group.Pointer is equal to group.FirstArticle.
        /// group.Next()
        /// 'group.Pointer is now equal to group.FirstArticle + 1.
        /// group.Previous();
        /// 'group.Pointer is now equal to group.FirstArticle.
        /// 'This retrieves the first article of the group.
        /// Dim article 1 As Message = group.RetrieveArticleObject()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //group.Pointer is equal to group.FirstArticle.
        /// group.Next();
        /// //group.Pointer is now equal to group.FirstArticle + 1.
        /// group.Previous();
        /// //group.Pointer is now equal to group.FirstArticle.
        /// //This retrieves the first article of the group.
        /// var article1:Message = group.RetrieveArticleObject();
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string Previous()
        {
            string response = this._nntp.Command("last");
            if(response.StartsWith("223")) this.Pointer = System.Convert.ToInt32(response.Split(' ')[1]);
            return response;
        }

        private delegate string DelegatePrevious();
        private DelegatePrevious _delegatePrevious;

        public IAsyncResult BeginPrevious(AsyncCallback callback)
        {
            this._delegatePrevious = this.Previous;
            return this._delegatePrevious.BeginInvoke(callback, this._delegatePrevious);
        }

        public string EndPrevious(IAsyncResult result)
        {
            return this._delegatePrevious.EndInvoke(result);
        }

        /// <summary>
        /// Retrieves the article at the specified ordinal position.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <returns>A byte array containing the article data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve article at position 29 in this group.
        /// byte[] article29 = group.RetrieveArticle(29);
        /// //Retrieve last article in this group.
        /// byte[] article = group.RetrieveArticle(group.LastArticle);
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve article at position 29 in this group.
        /// Dim article29() As Byte = group.RetrieveArticle(29)
        /// 'Retrieve last article in this group.
        /// Dim article() As Byte = group.RetrieveArticle(group.LastArticle)
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve article at position 29 in this group.
        /// var article29:byte[] = group.RetrieveArticle(29);
        /// //Retrieve last article in this group.
        /// var article:byte[] = group.RetrieveArticle(group.LastArticle);
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveArticle(int index)
        {
            this._nntp.OnMessageRetrieving(new ActiveUp.Net.Mail.MessageRetrievingEventArgs(index));
            byte[] buffer = this._nntp.CommandMultiline("article "+index.ToString());
            if(System.Text.Encoding.ASCII.GetString(buffer,0,buffer.Length).StartsWith("220")) this.Pointer = index;
            this._nntp.OnMessageRetrieved(new ActiveUp.Net.Mail.MessageRetrievedEventArgs(buffer,index));
            return buffer;
        }

        private delegate byte[] DelegateRetrieveArticleInt(int index);
        private DelegateRetrieveArticleInt _delegateRetrieveArticleInt;

        public IAsyncResult BeginRetrieveArticle(int index, AsyncCallback callback)
        {
            this._delegateRetrieveArticleInt = this.RetrieveArticle;
            return this._delegateRetrieveArticleInt.BeginInvoke(index, callback, this._delegateRetrieveArticleInt);
        }

        /// <summary>
        /// Retrieves the article at the position specified by the current article pointer.
        /// </summary>
        /// <returns>A byte array containing the article data.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the first article in this group.
        /// byte[] article = group.RetrieveArticle();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the first article in this group.
        /// Dim article() As Byte = group.RetrieveArticle()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the first article in this group.
        /// var article:byte[] = group.RetrieveArticle();
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveArticle()
        {
            if(this.Pointer!=0 & this.Pointer!=-1) return this._nntp.CommandMultiline("article");
            else 
            {
                this.Pointer = this.FirstArticle;
                this._nntp.OnMessageRetrieving(new ActiveUp.Net.Mail.MessageRetrievingEventArgs(this.Pointer));
                byte[] buffer = this._nntp.CommandMultiline("article "+this.FirstArticle.ToString());
                this._nntp.OnMessageRetrieved(new ActiveUp.Net.Mail.MessageRetrievedEventArgs(buffer,this.Pointer));
                return buffer;
            }
        }

        private delegate byte[] DelegateRetrieveArticle();
        private DelegateRetrieveArticle _delegateRetrieveArticle;

        public IAsyncResult BeginRetrieveArticle(AsyncCallback callback)
        {
            this._delegateRetrieveArticle = this.RetrieveArticle;
            return this._delegateRetrieveArticle.BeginInvoke(callback, this._delegateRetrieveArticle);
        }

        public byte[] EndRetrieveArticle(IAsyncResult result)
        {
            return (byte[])result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Retrieves the article at the specified ordinal position.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <returns>A Message object representing the article.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve article at position 29 in this group.
        /// Message article29 = group.RetrieveArticleObject(29);
        /// //Retrieve last article in this group.
        /// Message article = group.RetrieveArticleObject(group.LastArticle);
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve article at position 29 in this group.
        /// Dim article29 As Message = group.RetrieveArticleObject(29)
        /// 'Retrieve last article in this group.
        /// Dim article As Message = group.RetrieveArticleObject(group.LastArticle)
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve article at position 29 in this group.
        /// var article29:Message = group.RetrieveArticleObject(29);
        /// //Retrieve last article in this group.
        /// var article:Message = group.RetrieveArticleObject(group.LastArticle);
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public Message RetrieveArticleObject(int index)
        {
            return ActiveUp.Net.Mail.Parser.ParseMessage(this.RetrieveArticle(index));
        }

        private delegate Message DelegateRetrieveArticleObjectInt(int index);
        private DelegateRetrieveArticleObjectInt _delegateRetrieveArticleObjectInt;

        public IAsyncResult BeginRetrieveArticleObject(int index, AsyncCallback callback)
        {
            this._delegateRetrieveArticleObjectInt = this.RetrieveArticleObject;
            return this._delegateRetrieveArticleObjectInt.BeginInvoke(index, callback, this._delegateRetrieveArticleObjectInt);
        }

        /// <summary>
        /// Retrieves the article at the position specified by the current article pointer.
        /// </summary>
        /// <returns>A Message object representing the article.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the first article in this group.
        /// Message article = group.RetrieveArticleObject();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the first article in this group.
        /// Dim article As Message = group.RetrieveArticleObject()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the first article in this group.
        /// var article:Message = group.RetrieveArticleObject();
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public Message RetrieveArticleObject()
        {
            return ActiveUp.Net.Mail.Parser.ParseMessage(this.RetrieveArticle());
        }

        private delegate Message DelegateRetrieveArticleObject();
        private DelegateRetrieveArticleObject _delegateRetrieveArticleObject;

        public IAsyncResult BeginRetrieveArticleObject(AsyncCallback callback)
        {
            this._delegateRetrieveArticleObject = this.RetrieveArticleObject;
            return this._delegateRetrieveArticleObject.BeginInvoke(callback, this._delegateRetrieveArticleObject);
        }

        public Message EndRetrieveArticleObject(IAsyncResult result)
        {
            return (Message)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Retrieves the article Header at the specified ordinal position.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <returns>A byte array containing the article header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the Header of the article at position 29 in this group.
        /// byte[] header29 = group.RetrieveHeader(29);
        /// //Retrieve last Header in this group.
        /// byte[] Header = group.RetrieveHeader(group.LastHeader);
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the Header of the article at position 29 in this group.
        /// Dim header29() As Byte = group.RetrieveHeader(29)
        /// 'Retrieve last Header in this group.
        /// Dim header() As Byte = group.RetrieveHeader(group.LastHeader)
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the Header of the article at position 29 in this group.
        /// var header29:byte[] = group.RetrieveHeader(29);
        /// //Retrieve last Header in this group.
        /// var header:byte[] = group.RetrieveHeader(group.LastHeader);
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveHeader(int index)
        {
            this._nntp.OnHeaderRetrieving(new ActiveUp.Net.Mail.HeaderRetrievingEventArgs(index));
            byte[] buffer = this._nntp.CommandMultiline("head "+index.ToString());
            if(System.Text.Encoding.ASCII.GetString(buffer,0,buffer.Length).StartsWith("221")) this.Pointer = index;
            this._nntp.OnHeaderRetrieved(new ActiveUp.Net.Mail.HeaderRetrievedEventArgs(buffer,index));
            return buffer;
        }

        private delegate byte[] DelegateRetrieveHeaderInt(int index);
        private DelegateRetrieveHeaderInt _delegateRetrieveHeaderInt;

        public IAsyncResult BeginRetrieveHeader(int index, AsyncCallback callback)
        {
            this._delegateRetrieveHeaderInt = this.RetrieveHeader;
            return this._delegateRetrieveHeaderInt.BeginInvoke(index, callback, this._delegateRetrieveHeaderInt);
        }

        /// <summary>
        /// Retrieves the article Header at the position specified by the current article pointer.
        /// </summary>
        /// <returns>A byte array containing the article header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the first Header in this group.
        /// byte[] Header = group.RetrieveHeader();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the first Header in this group.
        /// Dim header() As Byte = group.RetrieveHeader()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the first Header in this group.
        /// var header:byte[] = group.RetrieveHeader();
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveHeader()
        {
            if(this.Pointer!=0 & this.Pointer!=-1) return this._nntp.CommandMultiline("head");
            else 
            {
                this.Pointer = this.FirstArticle;
                this._nntp.OnHeaderRetrieving(new ActiveUp.Net.Mail.HeaderRetrievingEventArgs(this.Pointer));
                byte[] buffer = this._nntp.CommandMultiline("head "+this.FirstArticle.ToString());
                this._nntp.OnHeaderRetrieved(new ActiveUp.Net.Mail.HeaderRetrievedEventArgs(buffer,this.Pointer));
                return buffer;
            }
        }

        private delegate byte[] DelegateRetrieveHeader();
        private DelegateRetrieveHeader _delegateRetrieveHeader;

        public IAsyncResult BeginRetrieveHeader(AsyncCallback callback)
        {
            this._delegateRetrieveHeader = this.RetrieveHeader;
            return this._delegateRetrieveHeader.BeginInvoke(callback, this._delegateRetrieveHeader);
        }

        public byte[] EndRetrieveHeader(IAsyncResult result)
        {
            return (byte[])result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Stores the article Header at the specified ordinal position to the specified path.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <param name="filePath">The destination path for the file.</param>
        /// <returns>The path the file has been saved at.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Store the Header of the article at position 29 in this group.
        /// group.StoreHeader(29,"C:\\My news\\header.txt");
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Store the Header of the article at position 29 in this group.
        /// group.StoreHeader(29,"C:\My news\header.txt")
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Store the Header of the article at position 29 in this group.
        /// group.StoreHeader(29,"C:\\My news\\header.txt");
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string StoreHeader(int index, string filePath)
        {
            return this.StoreToFile(filePath,this.RetrieveHeader(index));
        }

        private delegate string DelegateStoreHeaderInt(int index, string filepath);
        private DelegateStoreHeaderInt _delegateStoreHeaderInt;

        public IAsyncResult BeginStoreHeader(int index, string filePath, AsyncCallback callback)
        {
            this._delegateStoreHeaderInt = this.StoreHeader;
            return this._delegateStoreHeaderInt.BeginInvoke(index, filePath, callback, this._delegateStoreHeaderInt);
        }

        /// <summary>
        /// Retrieves the article Header at the position specified by the current article pointer.
        /// </summary>
        /// <param name="filePath">The destination path for the file.</param>
        /// <returns>The path the file has been saved at.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Store the first article in this group.
        /// group.StoreHeader("C:\\My news\\header.txt");
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Store the first article in this group.
        /// group.StoreHeader("C:\My news\header.txt")
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Store the first article in this group.
        /// group.StoreHeader("C:\\My news\\header.txt");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string StoreHeader(string filePath)
        {
            return this.StoreToFile(filePath,this.RetrieveHeader());
        }

        private delegate string DelegateStoreHeader(string filepath);
        private DelegateStoreHeader _delegateStoreHeader;

        public IAsyncResult BeginStoreHeader(string filepath, AsyncCallback callback)
        {
            this._delegateStoreHeader = this.StoreHeader;
            return this._delegateStoreHeader.BeginInvoke(filepath, callback, this._delegateStoreHeader);
        }

        public string EndStoreHeader(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Stores the article at the specified ordinal position to the specified path.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <param name="filePath">The destination path for the file.</param>
        /// <returns>The path the file has been saved at.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Store article at position 29 in this group.
        /// group.StoreArticle(29,"C:\\My news\\article.nws");
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Store article at position 29 in this group.
        /// group.StoreArticle(29,"C:\My news\article.nws")
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Store article at position 29 in this group.
        /// group.StoreArticle(29,"C:\\My news\\article.nws");
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string StoreArticle(int index, string filePath)
        {
            return this.StoreToFile(filePath,this.RetrieveArticle(index));
        }

        private delegate string DelegateStoreArticleInt(int index, string filepath);
        private DelegateStoreArticleInt _delegateStoreArticleInt;

        public IAsyncResult BeginStoreArticle(int index, string filePath, AsyncCallback callback)
        {
            this._delegateStoreArticleInt = this.StoreArticle;
            return this._delegateStoreArticleInt.BeginInvoke(index, filePath, callback, this._delegateStoreArticleInt);
        }

        /// <summary>
        /// Retrieves the article at the position specified by the current article pointer.
        /// </summary>
        /// <param name="filePath">The destination path for the file.</param>
        /// <returns>The path the file has been saved at.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Store the first article in this group.
        /// group.StoreArticle("C:\\My news\\article.nws");
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Store the first article in this group.
        /// group.StoreArticle("C:\My news\article.nws")
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Store the first article in this group.
        /// group.StoreArticle("C:\\My news\\article.nws");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string StoreArticle(string filePath)
        {
            return this.StoreToFile(filePath,this.RetrieveArticle());
        }

        private delegate string DelegateStoreArticle(string filepath);
        private DelegateStoreArticle _delegateStoreArticle;

        public IAsyncResult BeginStoreArticle(string filepath, AsyncCallback callback)
        {
            this._delegateStoreArticle = this.StoreArticle;
            return this._delegateStoreArticle.BeginInvoke(filepath, callback, this._delegateStoreArticle);
        }

        public string EndStoreArticle(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Stores the article body at the specified ordinal position to the specified path.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <param name="filePath">The destination path for the file.</param>
        /// <returns>The path the file has been saved at.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Store the body of the article at position 29 in this group.
        /// group.StoreBody(29,"C:\\My news\\body.txt");
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Store the body of the article at position 29 in this group.
        /// group.StoreBody(29,"C:\My news\body.txt")
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Store the body of the article at position 29 in this group.
        /// group.StoreBody(29,"C:\\My news\\body.txt");
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string StoreBody(int index, string filePath)
        {
            return this.StoreToFile(filePath,this.RetrieveBody(index));
        }

        private delegate string DelegateStoreBodyInt(int index, string filepath);
        private DelegateStoreBodyInt _delegateStoreBodyInt;

        public IAsyncResult BeginStoreBody(int index, string filePath, AsyncCallback callback)
        {
            this._delegateStoreBodyInt = this.StoreBody;
            return this._delegateStoreBodyInt.BeginInvoke(index, filePath, callback, this._delegateStoreBodyInt);
        }

        /// <summary>
        /// Retrieves the article body at the position specified by the current article pointer.
        /// </summary>
        /// <param name="filePath">The destination path for the file.</param>
        /// <returns>The path the file has been saved at.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Store the first article in this group.
        /// group.StoreBody("C:\\My news\\article.txt");
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Store the first article in this group.
        /// group.StoreBody("C:\My news\article.txt")
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Store the first article in this group.
        /// group.StoreBody("C:\\My news\\article.txt");
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public string StoreBody(string filePath)
        {
            return this.StoreToFile(filePath,this.RetrieveBody());
        }

        private delegate string DelegateStoreBody(string filepath);
        private DelegateStoreBody _delegateStoreBody;

        public IAsyncResult BeginStoreBody(string filepath, AsyncCallback callback)
        {
            this._delegateStoreBody = this.StoreBody;
            return this._delegateStoreBody.BeginInvoke(filepath, callback, this._delegateStoreBody);
        }

        public string EndStoreBody(IAsyncResult result)
        {
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Stores the given byte array into a file at the specified location.
        /// </summary>
        /// <param name="path">Path of the file to be created.</param>
        /// <param name="data">Data of the file to be created.</param>
        /// <returns>The path where the file has been created.</returns>
        private string StoreToFile(string path, byte[] data)
        {
            System.IO.FileStream fs = new System.IO.FileStream(path,System.IO.FileMode.Create,System.IO.FileAccess.Write);
            fs.Write(data,0,data.Length);
            fs.Close();
            return path;
        }

        /// <summary>
        /// Retrieves the article Header at the specified ordinal position.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <returns>A Header object representing the article header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the Header of the article at position 29 in this group.
        /// Header header29 = group.RetrieveHeaderObject(29);
        /// //Retrieve last Header in this group.
        /// Header Header = group.RetrieveHeaderObject(group.LastHeader);
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the Header of the article at position 29 in this group.
        /// Dim header29 As Header = group.RetrieveHeaderObject(29)
        /// 'Retrieve last Header in this group.
        /// Dim Header As Header = group.RetrieveHeaderObject(group.LastHeader)
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the Header of the article at position 29 in this group.
        /// var header29:Header = group.RetrieveHeaderObject(29);
        /// //Retrieve last Header in this group.
        /// var header:Header = group.RetrieveHeaderObject(group.LastHeader);
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public Header RetrieveHeaderObject(int index)
        {
            return ActiveUp.Net.Mail.Parser.ParseHeader(this.RetrieveHeader(index));
        }

        private delegate Header DelegateRetrieveHeaderObjectInt(int index);
        private DelegateRetrieveHeaderObjectInt _delegateRetrieveHeaderObjectInt;

        public IAsyncResult BeginRetrieveHeaderObject(int index, AsyncCallback callback)
        {
            this._delegateRetrieveHeaderObjectInt = this.RetrieveHeaderObject;
            return this._delegateRetrieveHeaderObjectInt.BeginInvoke(index, callback, this._delegateRetrieveHeaderObjectInt);
        }

        /// <summary>
        /// Retrieves the article Header at the position specified by the current article pointer.
        /// </summary>
        /// <returns>A Header object representing the article header.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the first Header in this group.
        /// Header Header = group.RetrieveHeaderObject();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the first Header in this group.
        /// Dim Header As Header = group.RetrieveHeaderObject()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the first Header in this group.
        /// var header:Header = group.RetrieveHeaderObject();
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public Header RetrieveHeaderObject()
        {
            return ActiveUp.Net.Mail.Parser.ParseHeader(this.RetrieveHeader());
        }

        private delegate Header DelegateRetrieveHeaderObject();
        private DelegateRetrieveHeaderObject _delegateRetrieveHeaderObject;

        public IAsyncResult BeginRetrieveHeaderObject(AsyncCallback callback)
        {
            this._delegateRetrieveHeaderObject = this.RetrieveHeaderObject;
            return this._delegateRetrieveHeaderObject.BeginInvoke(callback, this._delegateRetrieveHeaderObject);
        }

        public Header EndRetrieveHeaderObject(IAsyncResult result)
        {
            return (Header)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Retrieves the article body at the specified ordinal position.
        /// </summary>
        /// <param name="index">The ordinal position of the article to be retrieved.</param>
        /// <returns>A byte array containing the article body.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the body of the article at position 29 in this group.
        /// byte[] body29 = group.RetrieveBody(29);
        /// //Retrieve last body in this group.
        /// byte[] body = group.RetrieveBody(group.LastBody);
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the body of the article at position 29 in this group.
        /// Dim body29() As Byte = group.RetrieveBody(29)
        /// 'Retrieve last body in this group.
        /// Dim body() As Byte = group.RetrieveBody(group.LastBody)
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the body of the article at position 29 in this group.
        /// var body29:byte[] = group.RetrieveBody(29);
        /// //Retrieve last body in this group.
        /// var body:byte[] = group.RetrieveBody(group.LastBody);
        /// 
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveBody(int index)
        {
            byte[] buffer = this._nntp.CommandMultiline("body "+index.ToString());
            if(System.Text.Encoding.ASCII.GetString(buffer,0,buffer.Length).StartsWith("222")) this.Pointer = index;
            return buffer;
        }

        private delegate byte[] DelegateRetrieveBodyInt(int index);
        private DelegateRetrieveBodyInt _delegateRetrieveBodyInt;

        public IAsyncResult BeginRetrieveBody(int index, AsyncCallback callback)
        {
            this._delegateRetrieveBodyInt = this.RetrieveBody;
            return this._delegateRetrieveBodyInt.BeginInvoke(index, callback, this._delegateRetrieveBodyInt);
        }

        /// <summary>
        /// Retrieves the article body at the position specified by the current article pointer.
        /// </summary>
        /// <returns>A byte array containing the article body.</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// NntpClient nntp = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// NewsGroup group = nntp.SelectGroup("mygroup");
        /// //Retrieve the first body in this group.
        /// byte[] body = group.RetrieveBody();
        /// 
        /// nntp.Disconnect();
        /// 
        /// VB.NET
        /// 
        /// Dim nntp As New NntpClient
        /// 
        /// nntp.Connect("news.myhost.com")
        /// 
        /// Dim group As NewsGroup = nntp.SelectGroup("mygroup")
        /// 'Retrieve the first body in this group.
        /// Dim body() As Byte = group.RetrieveBody()
        /// 
        /// nntp.Disconnect()
        /// 
        /// JScript.NET
        /// 
        /// var nntp:NntpClient = new NntpClient();
        /// 
        /// nntp.Connect("news.myhost.com");
        /// 
        /// var group:NewsGroup = nntp.SelectGroup("mygroup");
        /// //Retrieve the first body in this group.
        /// var body:byte[] = group.RetrieveBody();
        /// nntp.Disconnect();
        /// </code>
        /// </example>
        public byte[] RetrieveBody()
        {
            if(this.Pointer!=0 & this.Pointer!=-1) return this._nntp.CommandMultiline("body");
            else 
            {
                this.Pointer = this.FirstArticle;
                return this._nntp.CommandMultiline("body "+this.FirstArticle.ToString());;
            }
        }

        private delegate byte[] DelegateRetrieveBody();
        private DelegateRetrieveBody _delegateRetrieveBody;

        public IAsyncResult BeginRetrieveBody(AsyncCallback callback)
        {
            this._delegateRetrieveBody = this.RetrieveBody;
            return this._delegateRetrieveBody.BeginInvoke(callback, this._delegateRetrieveBody);
        }

        public byte[] EndRetrieveBody(IAsyncResult result)
        {
            return (byte[])result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        #endregion

        #endregion

    }
    #endregion
}