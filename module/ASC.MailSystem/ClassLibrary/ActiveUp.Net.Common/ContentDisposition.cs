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
using ActiveUp.Net.Mail;
using System.Collections.Specialized;

namespace ActiveUp.Net.Mail
{
#if !PocketPC
    [System.Serializable]
#endif
    public class ContentDisposition : StructuredHeaderField
    {
        /// <summary>
        /// A bodypart should have an Inline ContentDisposition if it is intended to be displayed automatically upon display of the message.
        /// </summary>
        public const int Inline = 1;
        /// <summary>
        /// Bodyparts can have Attachment ContentDisposition to indicate that they are separate from the main body of the mail message, and that their display should not be automatic, but contingent upon some further action of the user.
        /// </summary>
        public const int Attachment = 2;

        private string _disposition = string.Empty;

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName
        {
            get
            {
                if (this.Parameters["filename"] != null) return string.Format("\"{0}\"", this.Parameters["filename"].Trim('"'));
                else if (this.Parameters["\tfilename"] != null) return this.Parameters["\tfilename"].Trim('"').Trim('\t');
                else return null;
            }
            set
            {
                if (this.Parameters["filename"] != null) this.Parameters["filename"] = string.Format("\"{0}\"", value.Trim('"'));
                else this.Parameters.Add("filename", string.Format("\"{0}\"", value.Trim('"')));
            }
        }
        /// <summary>
        /// Gets or sets the disposition.
        /// </summary>
        /// <value>The disposition.</value>
        public string Disposition
        {
            get
            {
                return this._disposition;
            }
            set
            {
                this._disposition = value;
            }
        }
        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Content-Disposition: ")
                .Append(this.Disposition);

            foreach (var key in this.Parameters.AllKeys)
            {
                sb.Append(";\r\n\t").Append(key).Append("=").Append(this.Parameters[key]);
            }

            return sb.ToString();
        }

        public static bool operator ==(ContentDisposition t1, int t2)
        {
            if (t1.Disposition.ToLower() == "inline" && t2 == 1 ||
                t1.Disposition.ToLower() == "attachment" && t2 == 2)
                return true;
            else
                return false;
        }

        public static bool operator !=(ContentDisposition t1, int t2)
        {
            if (t1.Disposition.ToLower() == "inline" && t2 == 1 ||
                t1.Disposition.ToLower() == "attachment" && t2 == 2)
                return false;
            else
                return true;
        }

        public override bool Equals(object o)
        {
            return this == (ContentDisposition)o;
        }

        public override int GetHashCode()
        {
            return this._disposition.GetHashCode();
        }


        /*/// <summary>
        /// The suggested Filename of the MimePart.
        /// </summary>
        public string Filename
        {
            get
            {
                return this._filename;
            }
            set
            {
                this._filename = value;
            }
        }
        /// <summary>
        /// The date the MimePart's content was created.
        /// </summary>
        public System.DateTime CreationDate
        {
            get
            {
                return this._creation;
            }
            set
            {
                this._creation = value;
            }
        }
        /// <summary>
        /// The date the MimePart's content was last modified.
        /// </summary>
        public System.DateTime LastModificationDate
        {
            get
            {
                return this._modification;
            }
            set
            {
                this._modification = value;
            }
        }
        /// <summary>
        /// The date the MimePart's content was last read.
        /// </summary>
        public System.DateTime LastReadDate
        {
            get
            {
                return this._read;
            }
            set
            {
                this._read = value;
            }
        }
        /// <summary>
        /// Approximate size of the file in octets.
        /// </summary>
        public int Size
        {
            get
            {
                if(this.BinaryContent.Length>0) return this.BinaryContent.Length;
                else return this.TextContent.Length;
                //return this._intsize;
            }
        }*/
    }
}
