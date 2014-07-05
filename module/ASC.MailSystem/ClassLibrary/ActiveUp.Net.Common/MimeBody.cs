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

using System.Text.RegularExpressions;

namespace ActiveUp.Net.Mail 
{
#region MimeBody Class
    /// <summary>
    /// Represents a message's body.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class MimeBody
    {
        string _charset = "iso-8859-1";
        string _text = string.Empty;
        ContentTransferEncoding _encoding = ContentTransferEncoding.QuotedPrintable;
        BodyFormat _format;

        public MimeBody(BodyFormat format)
        {
            this._format = format;
        }
        
        /// <summary>
        /// The type of the body.
        /// </summary>
        public BodyFormat Format
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
            }
        }

        /// <summary>
        /// The character set (encoding) of the body.
        /// </summary>
        public string Charset
        {
            get
            {
                return this._charset;
            }
            set
            {
                this._charset = value;
            }
        }

        /// <summary>
        /// The body's text (content).
        /// </summary>
        public string Text
        {
            get
            {
                /*#if TRIAL
                                if (string.IsNullOrEmpty(this._text))
                                    return this._text;

                                if (this.Format == BodyFormat.Html)
                                    return ProductHelper.GetTrialString(string.Empty, TrialStringType.LongHtml)
                                    + this._text + ProductHelper.GetTrialString(string.Empty, TrialStringType.LongHtml);
                                else
                                    return ProductHelper.GetTrialString(this._text, TrialStringType.LongText);
                #else
                                string ret = this._text;
                                ret = ret.Replace("FLAGS (Seen)", string.Empty);
                                return ret;
                
                #endif*/
               return _text;
            }
            set
            {
                this._text = value;
            }
        }

        static readonly Regex _htmlRegex = new Regex("<[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        /// <summary>
        /// The body's text (content) stripped (without HTML tags).
        /// </summary>
        public string TextStripped
        {
            get
            {
                string newText = _text.Replace("</p>", "\n\n").Replace("</P>", "\n\n").Replace("<br>", "\n").Replace("<BR>", "\n").Replace("&nbsp;", " ");
                //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("<[^>]*>");
                newText = _htmlRegex.Replace(newText, string.Empty);
                return newText;
            }
        }

        public ContentTransferEncoding ContentTransferEncoding
        {
            get
            {
                return this._encoding;
            }
            set
            {
                this._encoding = value;
            }
        }

        #region Methods

        public MimePart ToMimePart()
        {
            MimePart part = new MimePart();

            if (this.Format.Equals(BodyFormat.Text)) part.ContentType.MimeType = "text/plain";
            else if (this.Format.Equals(BodyFormat.Html)) part.ContentType.MimeType = "text/html";

            part.ContentType.Parameters.Add("charset", this.Charset);

            part.Charset = this.Charset;

            part.ContentTransferEncoding = this.ContentTransferEncoding;

            string content = this.Text;

            /*if (this.ContentTransferEncoding.Equals(ContentTransferEncoding.Base64))
            {
                byte[] contentBytes = Encoding.GetEncoding(this.Charset).GetBytes(this.Text);
                content = Convert.ToBase64String(contentBytes);
            }
            else if (this.ContentTransferEncoding.Equals(ContentTransferEncoding.QuotedPrintable))
            {
                content = Codec.ToQuotedPrintable(this.Text, this.Charset);
            }
            else content = this.Text;*/

            part.TextContent = content;

            return part;
        }

        #endregion
    }
    #endregion
}
