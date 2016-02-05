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

using ActiveUp.Net.Mail;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ActiveUp.Net.Mail
{
    #region Header Object version 2
    
    /// <summary>
    /// Represents the Header of a RFC 2822 message.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Header
    {

        #region Private fields

        int _indexOnServer,_id = -1;
        AddressCollection _to = new AddressCollection();
        AddressCollection _cc = new AddressCollection();
        AddressCollection _bcc = new AddressCollection();
        AddressCollection _recipients = new AddressCollection();
        Address _from = new Address();
        List<TraceInfo> _trace = new List<TraceInfo>();
        Address _sender = new Address();
        Address _replyto = new Address();
        ContentType _contentType = new ContentType();
        ContentDisposition _contentDisposition = new ContentDisposition();
        NameValueCollection _fieldNames = new NameValueCollection();
        NameValueCollection _fields = new NameValueCollection();
        byte[] _data;

        #endregion

        #region Methods

        #region Private utility methods

        public void AddHeaderField(string name, string value)
        {
            string key = name.ToLower();

            if (this.HeaderFields[key] == null || key.Equals("received")) this.HeaderFields.Add(key, value);
            else this.HeaderFields[key] = value;

            if (this.HeaderFieldNames[key] == null || key.Equals("received")) this.HeaderFieldNames.Add(key, name);
            else this.HeaderFieldNames[key] = name;
        }

        #endregion

        #region Public methods

        public BounceResult GetBounceStatus()
        {
            return GetBounceStatus(null);
        }

        /// <summary>
        /// Detects if a message is a delivery failure notification.
        /// This method uses the default signatures containing in an internal ressource file.
        /// </summary>
        /// <remarks>
        /// Signature files are XML files formatted as follows : 
        /// 
        /// &lt;?xml version='1.0'?&gt;
        /// &lt;signatures&gt;
        ///        &lt;signature from=&quot;postmaster&quot; subject=&quot;Undeliverable Mail&quot; body=&quot;Unknown user&quot; search=&quot;&quot; />
        ///        ...
        /// &lt;/signatures&gt;
        /// </remarks>
        /// <returns>A BounceStatus object containing the level of revelance and if 100% identified, the erroneous email address.</returns>
        public BounceResult GetBounceStatus(string signaturesFilePath)
        {
            string ressource = string.Empty;
            
            if (signaturesFilePath == null || signaturesFilePath == string.Empty)
            {
                ressource = Header.GetResource("ActiveUp.Net.Mail.bouncedSignatures.xml");
            }
            else
                ressource = System.IO.File.OpenText(signaturesFilePath).ReadToEnd();
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(ressource);
            BounceResult result = new BounceResult();

            foreach (System.Xml.XmlElement el in doc.GetElementsByTagName("signature"))
            {
                if (this.From.Merged.IndexOf(el.GetElementsByTagName("from")[0].InnerText) != -1)
                    result.Level++;

                if (this.Subject != null && this.Subject.IndexOf(el.GetElementsByTagName("subject")[0].InnerText) != -1)
                    result.Level++;
            }

            return result;
        }

        public static string GetResource(string resource)
        {
            System.IO.Stream stm = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            System.IO.StreamReader reader = new System.IO.StreamReader(stm);
            string str = reader.ReadToEnd();
            reader.Close();
            stm.Close();
            return str;
        }

        /// <summary>
        /// The MIME representation of the header.
        /// </summary>
        /// <returns></returns>
        public string ToHeaderString()
        {
            return ToHeaderString(false);
        }

        /// <summary>
        /// The MIME representation of the header.
        /// </summary>
        /// <param name="removeBlindCopies">if set to <c>true</c> remove blind copies (BCC).</param>
        /// <returns></returns>
        public string ToHeaderString(bool removeBlindCopies)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            /*foreach (TraceInfo trace in this.Trace) sb.AppendLine("Received: " + trace.ToString());
            if (!this.From.Email.Equals(string.Empty)) sb.AppendLine("From: " + this.From.Merged);
            if (!this.Sender.Email.Equals(string.Empty)) sb.AppendLine("Sender: " + this.Sender.Merged);
            if (this.To.Count > 0) sb.AppendLine("To: " + this.To.Merged);
            if (this.Cc.Count > 0) sb.AppendLine("Cc: " + this.Cc.Merged);
            if (this.Bcc.Count > 0) sb.AppendLine("Bcc: " + this.Bcc.Merged);
            if (!this.ReplyTo.Email.Equals(string.Empty)) sb.AppendLine("Reply-to: " + this.ReplyTo.Merged);*/

            foreach (TraceInfo trace in this.Trace) this.AddHeaderField("Received",trace.ToString());
            if (!this.From.Email.Equals(string.Empty)) this.AddHeaderField("From",this.From.Merged);
            if (!this.Sender.Email.Equals(string.Empty)) this.AddHeaderField("Sender",this.Sender.Merged);
            if (this.To.Count > 0) this.AddHeaderField("To", this.To.Merged);
            if (this.Cc.Count > 0) this.AddHeaderField("Cc", this.Cc.Merged);
            if (this.Bcc.Count > 0 && !removeBlindCopies) this.AddHeaderField("Bcc", this.Bcc.Merged);
            if (!this.ReplyTo.Email.Equals(string.Empty)) this.AddHeaderField("Reply-To", this.ReplyTo.Merged);

            if (this.Date.Equals(DateTime.MinValue)) this.Date = DateTime.Now;
            
            if (string.IsNullOrEmpty(this.MessageId))
                this.MessageId = string.Format("<AU{0}@{1}>", Codec.GetUniqueString(), System.Net.Dns.GetHostName());

            if (this.ContentType.MimeType.Length > 0)
            {
                string contentType = this.ContentType.ToString();
                contentType = contentType.Substring(contentType.IndexOf(":") + 1).TrimStart(' ');
                this.AddHeaderField("Content-Type", contentType);
            }

            if (this.ContentDisposition.Disposition.Length > 0)
            {
                string contentDisposition = this.ContentDisposition.ToString();
                contentDisposition = contentDisposition.Substring(contentDisposition.IndexOf(":") + 1).TrimStart(' ');
                this.AddHeaderField("Content-Disposition", contentDisposition);
            }

            if (this.ContentType.Type.Equals("text"))
            {
                string contentTransferEncoding = this.ContentTransferEncoding.ToString();
                contentTransferEncoding = contentTransferEncoding.Substring(contentTransferEncoding.IndexOf(":") + 1).TrimStart(' ');
                this.AddHeaderField("Content-Transfer-Encoding", "quoted-printable");
            }
            //sb.Append(this.ContentType.ToString() + "\r\n");
            //if (this.ContentDisposition.Disposition == "attachment") sb.AppendLine(this.ContentDisposition.ToString());
            
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version v = asm.GetName().Version;

            this.AddHeaderField("X-Mailer", string.Format("ActiveUp.MailSystem {0}.{1}.{2} www.activeup.com", v.Major, v.Minor, v.Build));
            
            foreach (string key in this.HeaderFields.AllKeys)
            {
                for (int i = 0; i < this.HeaderFields.GetValues(key).Length ; i++)
                    sb.AppendFormat("{0}: {1}\r\n", this.HeaderFieldNames.GetValues(key)[i], this.HeaderFields.GetValues(key)[i]);
            }

            /*string header = sb.ToString().TrimEnd('\r', '\n');
            string foldedHeader = string.Empty;

            StringReader sr = new StringReader(header);
            while(sr.Peek() != -1)
            {
                foldedHeader += Parser.Fold(sr.ReadLine()) + "\r\n";
            }*/

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets the message data as received from the server or as parsed from the file.
        /// Only available to messages received and/or parsed.
        /// </summary>
        public byte[] OriginalData
        {
            get { return this._data; }
            set { this._data = value; }
        }

        public List<TraceInfo> Trace
        {
            get
            {
                return this._trace;
            }
            set
            {
                this._trace = value;
            }
        }

        public IPAddress SenderIP
        {
            get
            {
                string pattern = @"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b";

                if (this.Trace.Count > 0 && Regex.IsMatch(this.Trace[0].From, pattern))
                {
                    string extractedIP = Regex.Match(this.Trace[0].From, pattern).Value;
                    return IPAddress.Parse(extractedIP);
                }
                else
                    return null;
            }
        }

        public AddressCollection To
        {
            get
            {
                return this._to;
            }
            set
            {
                this._to = value;
            }
        }

        /// <summary>
        /// All Header fields names.
        /// Key is the Header name in lower case and value is the Header name as it was it the original message.
        /// </summary>
        public NameValueCollection HeaderFieldNames
        {
            get
            {
                return this._fieldNames;
            }
            set
            {
                this._fieldNames = value;
            }
        }

        /// <summary>
        /// All Header fields.
        /// Key is the Header name in lower case and value is the Header value.
        /// </summary>
        /// <example>
        /// //This exposes the mail client used to send the message.
        /// message.HeaderFields["x-mailer"]
        /// </example>
        public NameValueCollection HeaderFields
        {
            get
            {
                return this._fields;
            }
            set
            {
                this._fields = value;
            }
        }

        /// <summary>
        /// "Cc" Recipients of the message.
        /// </summary>
        public AddressCollection Cc
        {
            get
            {
                return this._cc;
            }
            set
            {
                this._cc = value;
            }
        }

        /// <summary>
        /// "Bcc" Recipients of the message.
        /// </summary>
        public AddressCollection Bcc
        {
            get
            {
                return this._bcc;
            }
            set
            {
                this._bcc = value;
            }
        }

        /// <summary>
        /// All recipients of the message.
        /// </summary>
        public AddressCollection Recipients
        {
            get
            {
                return this._recipients;
            }
            set
            {
                this._recipients = value;
            }
        }

        /// <summary>
        /// Sender of the message.
        /// </summary>
        /// <remark>
        /// The sender's address can be specified if the person that actually post the message is different from the message originator. For exemple a secretary should send a message on the behalf of the CEO. In that particular car, the CEO's email address is set on the From property, and the secretary on the Sender property.
        /// </remark>
        public Address Sender
        {
            get
            {
                return this._sender;
            }
            set
            {
                this._sender = value;
            }
        }

        /// <summary>
        /// From of the message.
        /// </summary>
        public Address From
        {
            get
            {
                //if(this._from.Count>0) return this._from[0];
                //else return null;
                return _from;
            }
            set
            {
                //this._from.Clear();
                //this._from.Add(value);
                _from = value;
            }
        }

        /// <summary>
        /// Reply-To of the message.
        /// </summary>
        public Address ReplyTo
        {
            get
            {
                return this._replyto;
            }
            set
            {
                this._replyto = value;
            }
        }

        /// <summary>
        /// Subject of the message.
        /// </summary>
        public string Subject
        {
            get
            {
                string subject = string.Empty;
                if (this.HeaderFields["subject"] != null)
                    subject = this.HeaderFields.GetValues("subject")[0];
                else
                    subject = null;

#if TRIAL
                return ProductHelper.GetTrialString(subject, TrialStringType.ShortText);
#else
                return subject;
#endif
            }
            set
            {
#if TRIAL
                this.AddHeaderField("Subject", ProductHelper.GetTrialString(value, TrialStringType.ShortText));
#else
                this.AddHeaderField("Subject", value);
#endif
                //this.AddHeaderField("Subject", value);
            }
        }

        /// <summary>
        /// MessageId of the message to which this is a reply.
        /// </summary>
        public string InReplyTo
        {
            get
            {
                if (this.HeaderFields["in-reply-to"] != null) return Parser.Clean(Parser.RemoveWhiteSpaces(this.HeaderFields.GetValues("in-reply-to")[0]));
                else return null;
            }
            set
            {
                this.AddHeaderField("In-Reply-To", value);
            }
        }

        /// <summary>
        /// MessageId of the messages in the same thread (conversation) as this message.
        /// </summary>
        public string References
        {
            get
            {
                if (this.HeaderFields["references"] != null) return this.HeaderFields.GetValues("references")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("References", value);
            }
        }

        /// <summary>
        /// Any additional info about the message's body.
        /// </summary>
        public string Comments
        {
            get
            {
                if (this.HeaderFields["comments"] != null) return this.HeaderFields.GetValues("comments")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Comments", value);
            }
        }

        /// <summary>
        /// Keywords related to this message that could be used by a search engine while searching for this message in a list.
        /// </summary>
        public string Keywords
        {
            get
            {
                if (this.HeaderFields["keywords"] != null) return this.HeaderFields.GetValues("keywords")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Keywords", value);
            }
        }

        /// <summary>
        /// ID of the message generated by the server.
        /// </summary>
        public string MessageId
        {
            get
            {
                if (this.HeaderFields["message-id"] != null) return Parser.Clean(Parser.RemoveWhiteSpaces(this.HeaderFields.GetValues("message-id")[0]));
                else return null;
            }
            set
            {
                this.AddHeaderField("Message-ID", value);
            }
        }

        /// <summary>
        /// The message's charset.
        /// </summary>
        public string Charset
        {
            get
            {
                if (this.ContentType.Parameters["charset"] != null) return this.ContentType.Parameters["charset"];
                else return null;
            }
            set
            {
                if (this.ContentType.Parameters["charset"] != null) this.ContentType.Parameters["charset"] = value;
                else this.ContentType.Parameters.Add("charset", value);
            }
        }

        /// <summary>
        /// Gets the received date by the mail server.
        /// </summary>
        /// <value>The received date.</value>
        public System.DateTime ReceivedDate
        {
            get
            {
                return Trace.Count > 0 ? Trace.Select(t => t.Date).Min() : Date;
            }
        }

        /// <summary>
        /// Received date of the message in Universal Time.
        /// System.DateTime.MinValue if no received date could be parsed into a DateTime object.
        /// <seealso cref="DateString"/>
        /// </summary>
        public System.DateTime Date
        {
            get
            {
                try
                {
                    return Parser.ParseAsUniversalDateTime(this.HeaderFields["date"]);
                }
                catch { return System.DateTime.MinValue; }
            }
            set
            {
                this.AddHeaderField("Date", value.ToString("r"));
            }
        }

        /// <summary>
        /// Gets or sets the return receipt email. This will enable the delivery notification.
        /// </summary>
        /// <value>The return receipt.</value>
        public Address ReturnReceipt
        {
            get
            {
                if (this.HeaderFields["return-receipt-to"] != null) return Parser.ParseAddresses(this.HeaderFields.GetValues("return-receipt-to")[0])[0];
                else return null;
            }
            set
            {
                if (value != null)
                {
                    this.AddHeaderField("Return-receipt-to", value.Merged);
                }
            }
        }

        /// <summary>
        ///     <para>Gets or sets the email address where a read confirmation should be sent.
        ///     Please note that use recipient may ignore this receipt. In that case, you won't
        ///     receive anything.</para>
        ///     <para>Please note that if you are receiving the message, the property will return
        ///     null if the sender doesn't request a read receipt, otherwise, an Address object
        ///     will be available.</para>
        /// </summary>
        /// <example>
        ///     <code lang="CS" title="C# sample">
        /// ActiveUp.Net.Mail.Message msg = new ActiveUp.Net.Mail.Message();
        /// msg.From.Name = "User1";
        /// msg.From.Email = "user1@example.com";
        /// msg.To.Add("user2@example.com");
        /// msg.Subject = "Confirm Read";
        /// msg.BodyHtml.Text = "The HTML body";
        ///  
        /// // This is the pragmatic solution
        /// msg.ConfirmRead = msg.From;
        ///  
        /// // But you can specify it yourself
        /// msg.ConfirmRead = new Address("user4", "user4@example.com");
        ///     </code>
        ///     <code lang="VB" title="VB.NET sample">
        /// Dim msg As ActiveUp.Net.Mail.Message = New ActiveUp.Net.Mail.Message()
        /// msg.From.Name = "User1"
        /// msg.From.Email = "user1@example.com"
        /// msg.To.Add("user2@example.com")
        /// msg.Subject = "Confirm Read"
        /// msg.BodyHtml.Text = "The HTML body"
        ///  
        /// ' This Is the pragmatic solution
        /// msg.ConfirmRead = msg.From
        ///  
        /// ' But you can specify it yourself
        /// msg.ConfirmRead = New Address("user4", "user4@example.com")
        ///     </code>
        /// </example>
        public Address ConfirmRead
        {
            get
            {
                if (this.HeaderFields["disposition-notification-to"] != null) return Parser.ParseAddresses(this.HeaderFields.GetValues("disposition-notification-to")[0])[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Disposition-Notification-To", value.Merged);
            }
        }

        /// <summary>
        /// Gets or sets the message flag (follow up).
        /// </summary>
        public string Flag
        {
            get
            {
                if (this.HeaderFields["x-message-flag"] != null) return this.HeaderFields.GetValues("x-message-flag")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("X-Message-Flag", value);
            }
        }

        /// <summary>
        /// Received date of the message as a string.
        /// </summary>
        public string DateString
        {
            get
            {
                if (this.HeaderFields["date"] != null) return this.HeaderFields.GetValues("date")[0];
                else return null;
            }
        }

        /// <summary>
        /// Stores the message Header to the specified path.
        /// </summary>
        /// <param name="path">Path to store the message Header at.</param>
        /// <returns>The path the message Header has been stored at.</returns>
        /// <example>
        /// This retrieves the first message's Header from the remote POP server and stores it on the disk.<br />
        /// You can read it back using the ParserHeader() method in the Parser class.
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("pop.myisp.com","username","password");
        /// Parser.ParseHeader(pop.RetrieveHeader(1)).StoreToFile("C:\\My Mails\\my_header.hdr");
        /// pop.Disconnect();
        /// this.Response.Write("Header stored.");
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New ActiveUp.MailPop3Pop3Client()
        /// pop.Connect("pop.myisp.com","username","password")
        /// Parser.ParseHeader(pop.RetrieveHeader(1)).StoreToFile("C:\\My Mails\\my_header.hdr");
        /// pop.Disconnect()
        /// Me.Response.Write("Header stored")
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("pop.myisp.com","username","password");
        /// Parser.ParseHeader(pop.RetrieveHeader(1)).StoreToFile("C:\\My Mails\\my_header.hdr");
        /// pop.Disconnect();
        /// this.Response.Write("Header stored.");
        /// </code>
        /// </example> 
        public virtual string StoreToFile(string path)
        {
            System.IO.FileStream fs = System.IO.File.Create(path);
            fs.Write(this._data,0,this._data.Length);
            fs.Close();
            return path;
        }

        public MessagePriority Priority
        {
            get
            {
                if (this.HeaderFields["x-priority"] != null)
                {
                    if (this.HeaderFields.GetValues("x-priority")[0].IndexOf("1") != -1 || this.HeaderFields.GetValues("x-priority")[0].IndexOf("High") != -1) return MessagePriority.High;
                    else if (this.HeaderFields.GetValues("x-priority")[0].IndexOf("3") != -1 || this.HeaderFields.GetValues("x-priority")[0].IndexOf("Normal") != -1) return MessagePriority.Normal;
                    else if (this.HeaderFields.GetValues("x-priority")[0].IndexOf("4") != -1 || this.HeaderFields.GetValues("x-priority")[0].IndexOf("Low") != -1) return MessagePriority.Low;
                }
                
                if(this.HeaderFields["x-msmail-priority"] != null)
                {
                    if (this.HeaderFields.GetValues("x-msmail-priority")[0].IndexOf("1") != -1 || this.HeaderFields.GetValues("x-msmail-priority")[0].IndexOf("High") != -1) return MessagePriority.High;
                    else if (this.HeaderFields.GetValues("x-msmail-priority")[0].IndexOf("3") != -1 || this.HeaderFields.GetValues("x-msmail-priority")[0].IndexOf("Normal") != -1) return MessagePriority.Normal;
                    else if (this.HeaderFields.GetValues("x-msmail-priority")[0].IndexOf("4") != -1 || this.HeaderFields.GetValues("x-msmail-priority")[0].IndexOf("Low") != -1) return MessagePriority.Low;
                }
                
                if(this.HeaderFields["importance"] != null) 
                {
                    if (this.HeaderFields.GetValues("importance")[0].IndexOf("1") != -1 || this.HeaderFields.GetValues("importance")[0].IndexOf("High") != -1) return MessagePriority.High;
                    else if (this.HeaderFields.GetValues("importance")[0].IndexOf("3") != -1 || this.HeaderFields.GetValues("importance")[0].IndexOf("Normal") != -1) return MessagePriority.Normal;
                    else if (this.HeaderFields.GetValues("importance")[0].IndexOf("4") != -1 || this.HeaderFields.GetValues("importance")[0].IndexOf("Low") != -1) return MessagePriority.Low;
                }
                
                return MessagePriority.Unknown;
            }
            set
            {
                if (this.HeaderFields["x-priority"] != null) this.HeaderFields["x-priority"] = value.ToString();
                else this.AddHeaderField("X-priority", value.ToString());
                
                if (this.HeaderFields["x-priority"] != null) this.HeaderFields["x-priority"] = value.ToString();
                else this.AddHeaderField("X-priority", value.ToString());
                
                if (this.HeaderFields["x-priority"] != null) this.HeaderFields["x-priority"] = value.ToString();
                else this.AddHeaderField("X-priority", value.ToString());
                
                if (this.HeaderFields["x-msmail-priority"] != null) this.HeaderFields["x-msmail-priority"] = value.ToString();
                else this.AddHeaderField("X-msmail-priority", value.ToString());
                
                if (this.HeaderFields["x-msmail-priority"] != null) this.HeaderFields["x-msmail-priority"] = value.ToString();
                else this.AddHeaderField("X-msmail-priority", value.ToString());
                
                if (this.HeaderFields["x-msmail-priority"] != null) this.HeaderFields["x-msmail-priority"] = value.ToString();
                else this.AddHeaderField("X-msmail-priority", value.ToString());
                
                if (this.HeaderFields["importance"] != null) this.HeaderFields["importance"] = value.ToString();
                else this.AddHeaderField("Importance", value.ToString());
                
                if (this.HeaderFields["importance"] != null) this.HeaderFields["importance"] = value.ToString();
                else this.AddHeaderField("Importance", value.ToString());
                
                if (this.HeaderFields["importance"] != null) this.HeaderFields["importance"] = value.ToString();
                else this.AddHeaderField("Importance", value.ToString());
            }
        }

        /// <summary>
        /// Content transfer encoding of the message.
        /// </summary>
        public ContentTransferEncoding ContentTransferEncoding
        {
            get
            {
                if (this.HeaderFields["content-transfer-encoding"] != null)
                {
                    if (this.HeaderFields.GetValues("content-transfer-encoding")[0].ToLower().IndexOf("quoted-printable") != -1) return ContentTransferEncoding.QuotedPrintable;
                    else if (this.HeaderFields.GetValues("content-transfer-encoding")[0].ToLower().IndexOf("base64") != -1) return ContentTransferEncoding.Base64;
                    else if (this.HeaderFields.GetValues("content-transfer-encoding")[0].ToLower().IndexOf("8bit") != -1) return ContentTransferEncoding.EightBits;
                    else if (this.HeaderFields.GetValues("content-transfer-encoding")[0].ToLower().IndexOf("7bit") != -1) return ContentTransferEncoding.SevenBits;
                    else if (this.HeaderFields.GetValues("content-transfer-encoding")[0].ToLower().IndexOf("binary") != -1) return ContentTransferEncoding.Binary;
                    else return ContentTransferEncoding.Unknown;
                }
                else return ContentTransferEncoding.None;
            }
            set
            {
                if (value == ContentTransferEncoding.EightBits)
                {
                    this.AddHeaderField("Content-Transfer-Encoding", "8bit");
                }
                else if (value == ContentTransferEncoding.SevenBits)
                {
                    this.AddHeaderField("Content-Transfer-Encoding", "7bit");
                }
                else if (value == ContentTransferEncoding.QuotedPrintable)
                {
                    this.AddHeaderField("Content-Transfer-Encoding", "quoted-printable");
                }
                else if (value == ContentTransferEncoding.Binary)
                {
                    this.AddHeaderField("Content-Transfer-Encoding", "binary");
                }
                else if (value == ContentTransferEncoding.Base64)
                {
                    this.AddHeaderField("Content-Transfer-Encoding", "base64");
                }
            }
        }

        /// <summary>
        /// Content type of the message.
        /// </summary>
        public ContentType ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                this._contentType = value;
            }
        }

        public ContentDisposition ContentDisposition
        {
            get
            {
                return this._contentDisposition;
            }
            set
            {
                this._contentDisposition = value;
            }
        }

        /// <summary>
        /// The index of the message on the server.
        /// </summary>
        public int IndexOnServer
        {
            get
            {
                return this._indexOnServer;
            }
            set
            {
                this._indexOnServer = value;
            }
        }

        /// <summary>
        /// Initializes an ID property that could be used for DB storage of messages.
        /// 0 by default.
        /// </summary>
        public int Id
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

        #region Usenet headers (see RFC 977)

        public string NewsGroups
        {
            get
            {
                if (this.HeaderFields["newsgroups"] != null) return this.HeaderFields.GetValues("newsgroups")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Newsgroups", value);
            }
        }

        /// <summary>
        /// This line shows the path the message took to reach the current system.
        /// </summary>
        public string Path
        {
            get
            {
                if (this.HeaderFields["path"] != null) return this.HeaderFields.GetValues("path")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Path", value);
            }
        }

        /// <summary>
        /// This line shows where follow-up messages are to be posted. 
        /// </summary>
        public string FollowUpTo
        {
            get
            {
                if (this.HeaderFields["followup-to"] != null) return this.HeaderFields.GetValues("followup-to")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Followup-To", value);
            }
        }

        /// <summary>
        /// Expiration date of the message.
        /// System.DateTime.MinValue if no expiration date could be parsed into a DateTime.
        /// <seealso cref="DateString"/>
        /// </summary>
        public System.DateTime Expires
        {
            get
            {
                try
                {
                    return Parser.ParseAsUniversalDateTime(this.HeaderFields["expires"]);
                }
                catch { return System.DateTime.MinValue; }
            }
            set
            {
                if (this.HeaderFields["expires"] != null) this.HeaderFields["expires"] = value.ToString("r");
                else this.AddHeaderField("Expires", value.ToString("r"));
            }
        }

        /// <summary>
        /// Received date of the message as a string.
        /// </summary>
        public string ExpiresString
        {
            get
            {
                if (this.HeaderFields["expires"] != null) return this.HeaderFields.GetValues("expires")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Expires", value);
            }
        }

        /// <summary>
        /// This field lists the Message-ID's of any messages prompting the submission of this message. 
        /// </summary>
        public string Control
        {
            get
            {
                if (this.HeaderFields["control"] != null) return this.HeaderFields.GetValues("control")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Control", value);
            }
        }

        /// <summary>
        /// This line is used to alter the distribution scope of the message.
        /// </summary>
        public string Distribution
        {
            get
            {
                if (this.HeaderFields["distribution"] != null) return this.HeaderFields.GetValues("distribution")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Distribution", value);
            }
        }

        /// <summary>
        /// The text of this line is a short phrase describing the organization    to which the sender belongs, or to which the machine belongs.
        /// </summary>
        public string Organization
        {
            get
            {
                if (this.HeaderFields["organization"] != null) return this.HeaderFields.GetValues("organization")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Organization", value);
            }
        }

        /// <summary>
        /// This line should contain a brief summary of the message.  It is    usually used as part of a follow-up to another message.
        /// </summary>
        public string Summary
        {
            get
            {
                if (this.HeaderFields["summary"] != null) return this.HeaderFields.GetValues("summary")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Summary", value);
            }
        }

        /// <summary>
        /// This line is required for any message posted to a moderated    newsgroup.
        /// </summary>
        public string Approved
        {
            get
            {
                if (this.HeaderFields["approved"] != null) return this.HeaderFields.GetValues("approved")[0];
                else return null;
            }
            set
            {
                this.AddHeaderField("Approved", value);
            }
        }

        /// <summary>
        /// This contains a count of the number of lines in the body of the message (if available).
        /// </summary>
        public int BodyLineCount
        {
            get
            {
                if (this.HeaderFields["lines"] != null) return Convert.ToInt32(this.HeaderFields.GetValues("lines")[0]);
                return -1;
            }
            set
            {
                this.AddHeaderField("Lines", value.ToString());
            }
        }

        /// <summary>
        /// Contains an XrefList object containing information about the message, at which host, newsgroups and indexes it resides.
        /// </summary>
        public UsenetXrefList Xref
        {
            get
            {
                UsenetXrefList xref = new UsenetXrefList();
                xref.Host = this.HeaderFields["xref"].Split(' ')[0];
                string[] splitted = this.HeaderFields["xref"].Split(' ');
                for (int i = 1; i < splitted.Length; i++) if (splitted[i].IndexOf(":") != -1) xref.Groups.Add(splitted[i].Split(':')[0], splitted[i].Split(':')[1]);
                return xref;
            }
            set
            {
                var sb = new StringBuilder();
                sb.Append(value.Host);
                foreach (var str in value.Groups.AllKeys) 
                    sb.AppendFormat(" {0}:{1}", str, value.Groups[str]);

                this.HeaderFields["xref"] = sb.ToString();
            }
        }

        #endregion

        #endregion

    }
    #endregion
}