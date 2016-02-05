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

using System.Text;
using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Contains informations about one trace information (one Received header).
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class TraceInfo
    {
        public TraceInfo()
        {

        }
        public TraceInfo(string from, System.DateTime date, string by, string via, string with, string ffor, string id)
        {
            this.Initialize(from, date, by, via, with, ffor, id);
        }
        public TraceInfo(string from, System.DateTime date, string by, string via, string with, string ffor)
        {
            this.Initialize(from, date, by, via, with, ffor, string.Empty);
        }
        public TraceInfo(string from, System.DateTime date, string by, string via, string with)
        {
            this.Initialize(from, date, by, via, with, string.Empty, string.Empty);
        }
        public TraceInfo(string from, System.DateTime date, string by, string via)
        {
            this.Initialize(from, date, by, via, string.Empty, string.Empty, string.Empty);
        }
        public TraceInfo(string from, System.DateTime date, string by)
        {
            this.Initialize(from, date, by, string.Empty, string.Empty, string.Empty, string.Empty
                );
        }
        private void Initialize(string from, System.DateTime date, string by, string via, string with, string ffor, string id)
        {
            this._from = from;
            this._by = by;
            this._via = via;
            this._with = with;
            this._for = ffor;
            this._id = id;
            this._date = date;
        }
        string _from = string.Empty,_by = string.Empty,_via = string.Empty,_with = string.Empty,_for = string.Empty,_id = string.Empty,_source = string.Empty;
        System.DateTime _date;

        /// <summary>
        /// Contains both (1) the name of the source host as presented in the EHLO command to the SMTP server and (2) an address literal containing the IP address of the source, determined from the TCP connection with the SMTP server.
        /// </summary>
        public string From
        {
            get
            {
                return this._from;
            }
            set
            {
                this._from = value;
            }
        }
        /// <summary>
        /// Contains the name of the SMTP host who received and processed the message.
        /// </summary>
        public string By
        {
            get
            {
                return this._by;
            }
            set
            {
                this._by = value;
            }
        }
        /// <summary>
        /// Contains a mean of communication that was used for the transaction between the SMTP server and the FROM user.
        /// </summary>
        /// <example>"TCP"</example>
        public string Via
        {
            get
            {
                return this._via;
            }
            set
            {
                this._via = value;
            }
        }
        /// <summary>
        /// The protocol used for the transaction by the SMTP server and the FROM user.
        /// </summary>
        public string With
        {
            get
            {
                return this._with;
            }
            set
            {
                this._with = value;
            }
        }
        /// <summary>
        /// An identification string for the transaction.
        /// </summary>
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }
        /// <summary>
        /// The destination mailbox for which the transaction was executed.
        /// </summary>
        public string For
        {
            get
            {
                return this._for;
            }
            set
            {
                this._for = value;
            }
        }
        public override string ToString()
        {
            var source = new StringBuilder();

            if (!this.From.Equals(string.Empty)) source.Append(" from ").Append(this.From).Append("\r\n ");
            if (!this.By.Equals(string.Empty)) source.Append(" by ").Append(this.By).Append("\r\n ");
            if (!this.With.Equals(string.Empty)) source.Append(" with ").Append(this.With).Append("\r\n ");
            if (!this.For.Equals(string.Empty)) source.Append(" for ").Append(this.For).Append("\r\n ");
            if (!this.Via.Equals(string.Empty)) source.Append(" via ").Append(this.Via).Append("\r\n ");
            if (!this.Id.Equals(string.Empty)) source.Append(" id ").Append(this.Id).Append("\r\n ");

            var str = source.ToString();

            return string.Format("{0};{1}", str.Remove(0, str.Length - 3), this.Date.ToString("r"));
        }
        /// <summary>
        /// The date and time of the transaction.
        /// </summary>
        public System.DateTime Date
        {
            get
            {
                return this._date;
            }
            set
            {
                this._date = value;
            }
        }
    }
}
