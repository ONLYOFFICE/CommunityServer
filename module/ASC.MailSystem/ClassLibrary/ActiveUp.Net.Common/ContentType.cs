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
using System.Text;
using System.Collections.Specialized;

namespace ActiveUp.Net.Mail
{
#if !PocketPC
    [System.Serializable]
#endif
    public class ContentType : StructuredHeaderField
    {
        public ContentType()
        {

        }

        string _mimeType = "text/plain";

        public string Type
        {
            get
            {
                return this._mimeType.Split('/')[0];
            }
            set
            {
                this._mimeType = string.Format("{0}/{1}", value, this.SubType);
            }
        }
        public string SubType
        {
            get
            {
                return this._mimeType.Split('/')[1];
            }
            set
            {
                this._mimeType = string.Format("{0}/{1}", this.Type, value);
            }
        }
        public string MimeType
        {
            get
            {
                return this._mimeType;
            }
            set
            {
                this._mimeType = value;
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Content-Type: ")
                .Append(this.MimeType);

            foreach (var key in this.Parameters.AllKeys)
            {
                sb.Append(";\r\n\t").Append(key).Append("=");

                if (key.Equals("boundary"))
                {
                    sb.Append("\"").Append(this.Parameters[key]).Append("\"");
                }
                else sb.Append(this.Parameters[key]);

            }

            return sb.ToString();
        }
    }
}
