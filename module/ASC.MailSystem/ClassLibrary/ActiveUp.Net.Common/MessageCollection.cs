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
#region MessageCollection object
    /// <summary>
    /// Represents a collection of Message objects
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class MessageCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Adds a Message object to the collection. Can be useful to use the GetBindableTable() method with message from different sources.
        /// </summary>
        /// <param name="msg"></param>
        public void Add(ActiveUp.Net.Mail.Message msg)
        {
            this.List.Add(msg);
        }
        /// <summary>
        /// Indexer.
        /// </summary>
        public ActiveUp.Net.Mail.Message this[int index]
        {
            get
            {
                return (ActiveUp.Net.Mail.Message)this.List[index];
            }
        }
        /// <summary>
        /// Creates a System.Data.DataTable containing the messages in the MessageCollection object with the requested columns.
        /// </summary>
        /// <param name="args">String containing sensitive words.
        /// This string is being parsed and columns added according to the words it contains.</param>
        /// <remarks>
        /// The column order is predefined and cannot be set.
        /// </remarks>
        /// <returns>The generated System.Data.DataTable</returns>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// MessageCollection messages = new MessageCollection();
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// 
        /// for(int i=1;i&lt;=pop.MessageCount;i++) messages.Add(pop.RetrieveMessageObject(i));
        /// 
        /// myDataGrid.DataSource = messages.GetBindableTable("subectsenderdate");
        /// myDataGrid.DataBind();
        /// 
        /// VB.NET
        /// 
        /// Dim messages As New MessageCollection
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf")
        /// 
        /// Dim i As Integer
        /// For i = 1 To pop.MessageCount
        ///        messages.Add(pop.RetrieveMessageObject(i))
        ///    Next i
        /// 
        /// myDataGrid.DataSource = messages.GetBindableTable("subectsenderdate")
        /// myDataGrid.DataBind()
        /// 
        /// JScript.NET
        /// 
        /// var messages:MessageCollection = new MessageCollection();
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// 
        /// for(int i=1;i&lt;=pop.MessageCount;i++) messages.Add(pop.RetrieveMessageObject(i));
        /// 
        /// myDataGrid.DataSource = messages.GetBindableTable("subectsenderdate");
        /// myDataGrid.DataBind();
        /// </code>
        /// </example> 
        public System.Data.DataTable GetBindableTable(string args)
        {
            System.Data.DataTable dt = new System.Data.DataTable("BindableTable");
            if(args.IndexOf("subject")!=-1)    dt.Columns.Add("Subject");
            if(args.IndexOf("sender")!=-1) dt.Columns.Add("Sender");
            if(args.IndexOf("replyto")!=-1)    dt.Columns.Add("ReplyTo");
            if(args.IndexOf("torecipient")!=-1)    dt.Columns.Add("To");
            if(args.IndexOf("ccrecipient")!=-1)    dt.Columns.Add("Cc");
            if(args.IndexOf("attach")!=-1) dt.Columns.Add("Attachments");
            if(args.IndexOf("date")!=-1) dt.Columns.Add("Date");
            if(args.IndexOf("priority")!=-1) dt.Columns.Add("Priority");
            if(args.IndexOf("size")!=-1) dt.Columns.Add("Size");
            foreach(ActiveUp.Net.Mail.Message msg in this)
            {
                string[] item = new string[dt.Columns.Count];
                for(int i=0;i<dt.Columns.Count;i++)
                {
                    switch(dt.Columns[i].Caption)
                    {
                        case "Subject" : item[i] = msg.Subject;
                            break;
                        case "Sender" : item[i] = (msg.Sender.Email!="Undefined") ? msg.Sender.Merged : msg.From.Merged;
                            break;
                        case "ReplyTo" : item[i] = msg.ReplyTo.Merged;
                            break;
                        case "To" : item[i] = msg.To.Merged;
                            break;
                        case "Cc" : if(msg.Cc!=null) item[i] = msg.Cc.Merged;
                            break;
                        case "Attachments" : item[i] = msg.Attachments.Count.ToString();
                            break;
                        case "Date" : item[i] = (msg.Date!=System.DateTime.MinValue) ? msg.Date.ToString() : msg.DateString;
                            break;
                        case "Priority" : item[i] = msg.Priority.ToString();
                            break;
                    }
                }
                dt.Rows.Add(item);
            }
            return dt;
        }

    }
    #endregion
}