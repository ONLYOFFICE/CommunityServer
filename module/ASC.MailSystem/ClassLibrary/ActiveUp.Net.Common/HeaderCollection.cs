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
#region HeaderCollection Object
    /// <summary>
    /// Represents a collection of Header objects.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class HeaderCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header">The Header to be added in the collection.</param>
        public void Add(ActiveUp.Net.Mail.Header header)
        {
            this.List.Add(header);
        }
        /// <summary>
        /// Indexer.
        /// </summary>
        public ActiveUp.Net.Mail.Header this[int index]
        {
            get
            {
                return (ActiveUp.Net.Mail.Header)this.List[index];
            }
        }
        public System.Data.DataTable GetBindableTable(string args)
        {
            System.Data.DataTable dt = new System.Data.DataTable("BindableTable");
            if(args.IndexOf("from")!=-1) dt.Columns.Add("From");
            if(args.IndexOf("subject")!=-1)    dt.Columns.Add("Subject");
            if(args.IndexOf("sender")!=-1) dt.Columns.Add("Sender");
            if(args.IndexOf("replyto")!=-1)    dt.Columns.Add("ReplyTo");
            if(args.IndexOf("torecipient")!=-1)    dt.Columns.Add("To");
            if(args.IndexOf("ccrecipient")!=-1)    dt.Columns.Add("Cc");
            if(args.IndexOf("date")!=-1) dt.Columns.Add("Date");
            if(args.IndexOf("priority")!=-1) dt.Columns.Add("Priority");
            foreach(Header header in this)
            {
                string[] item = new string[dt.Columns.Count];
                for(int i=0;i<dt.Columns.Count;i++)
                {
                    switch(dt.Columns[i].Caption)
                    {
                        case "From" : item[i] = header.From.Merged;
                            break;
                        case "Subject" : item[i] = header.Subject;
                            break;
                        case "Sender" : item[i] = (header.Sender.Email!="Undefined") ? header.Sender.Merged : header.From.Merged;
                            break;
                        case "ReplyTo" : item[i] = header.ReplyTo.Merged;
                            break;
                        case "To" : item[i] = header.To.Merged;
                            break;
                        case "Cc" : if(header.Cc!=null) item[i] = header.Cc.Merged;
                            break;
                        case "Date" : item[i] = (header.Date!=System.DateTime.MinValue) ? header.Date.ToString() : header.DateString;
                            break;
                        case "Priority" : item[i] = header.Priority.ToString();
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