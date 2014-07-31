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

using System.Security.Cryptography.X509Certificates;
#if !PocketPC
using System.Security.Cryptography.Pkcs;
#endif
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IO;
using ActiveUp.Net.Security;


namespace ActiveUp.Net.Mail
{
    #region Message Object

    /// <summary>
    /// Message Object.
    /// Represents a parsed e-mail message.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Message : Header
    {

        #region Private and internal fields

        internal System.Collections.Hashtable _CustomCollection = new System.Collections.Hashtable();
        internal AttachmentCollection _attachments = new AttachmentCollection();
        internal EmbeddedObjectCollection _embeddedObjects = new EmbeddedObjectCollection();
        internal MimePartCollection _allMimeParts = new MimePartCollection();
        internal MessageCollection _subMessages = new MessageCollection();
        internal MimePartCollection _otherParts = new MimePartCollection();
        internal bool _builtMimePartTree = false;
        MimePart _partTreeRoot = new MimePart();
        MimeBody _bodyHtml = new MimeBody(BodyFormat.Html);
        MimeBody _bodyText = new MimeBody(BodyFormat.Text);
        string _preamble, _epilogue;
        Signatures _signatures = new Signatures();

        bool _isSmimeEncrypted, _hasDomainKeySignature, _hasSmimeSignature, _hasSmimeDetachedSignature;

        #endregion

        #region Properties

        /// <summary>
        /// Collection containing attachments of the message.
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// Message message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Store message attachments.
        /// foreach(Attachment attach in message.Attachments) attach.StoreToFile("C:\\mails\\attachments\\"+attach.Filename);
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf")
        /// 'Retrieve message.
        /// Dim message As Message = pop.RetrieveMessageObject(1,False)
        /// pop.Disconnect()
        /// 'Store message attachments.
        /// For Each attach In message.Attachments
        ///        attach.StoreToFile("C:\mails\attachments\" + attach.Filename)
        ///    Next
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// var message:Message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Store message attachments.
        /// for(var i:int = 0;i&lt;message.Attachments.Count;i++) message.Attachments[i].StoreToFile("C:\\mails\\attachments\\"+message.Attachments[i].Filename);
        /// </code>
        /// </example>
        public AttachmentCollection Attachments
        {
            get
            {
                return _attachments;
            }
        }

        /// <summary>
        /// Collection containing custom string to be replaced.
        /// </summary>
        public System.Collections.Hashtable CustomCollection
        {
            get
            {
                return _CustomCollection;
            }
            set
            {
                _CustomCollection = value;
            }
        }

        /// <summary>
        /// Container for all the message's signature.
        /// </summary>
        public Signatures Signatures
        {
            get { return _signatures; }
            set { _signatures = value; }
        }

        /// <summary>
        /// Indicates whether the message has at least one DomainKey signature.
        /// </summary>
        public bool HasDomainKeySignature
        {
            get
            {
                return _hasDomainKeySignature;
            }
            set
            {
                _hasDomainKeySignature = value;
            }
        }

        /// <summary>
        /// Indicates whether the message has been signed using a S/MIME opaque (enveloping) signature.
        /// </summary>
        public bool HasSmimeSignature
        {
            get
            {
                return _hasSmimeSignature;
            }
            set
            {
                _hasSmimeSignature = value;
            }
        }

        /// <summary>
        /// Indicates whether the message has been signed using a S/MIME detached signature (using multipart/signed).
        /// </summary>
        public bool HasSmimeDetachedSignature
        {
            get
            {
                return _hasSmimeDetachedSignature;
            }
            set
            {
                _hasSmimeDetachedSignature = value;
            }
        }

        /// <summary>
        /// Indicates whether the message is in HTML format or not
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// Indicates whether the message has been encrypted using S/MIME.
        /// </summary>
        public bool IsSmimeEncrypted
        {
            get
            {
                return _isSmimeEncrypted;
            }
            set
            {
                _isSmimeEncrypted = value;
            }
        }

        /// <summary>
        /// Collection containing embedded MIME parts of the message (included text parts).
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// Message message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Store message's embedded objects.
        /// foreach(EmbeddedObject obj in message.EmbeddedObjects) obj.StoreToFile("C:\\mails\\objects\\"+obj.Filename);
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf")
        /// 'Retrieve message.
        /// Dim message As Message = pop.RetrieveMessageObject(1,False)
        /// pop.Disconnect()
        /// 'Store message's embedded objects.
        /// For Each obj In message.EmbeddedObjects
        ///        obj.StoreToFile("C:\mails\objects\" + obj.Filename)
        ///    Next
        /// 
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// var message:Message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Store message's embedded objects.
        /// for(var i:int = 0;i&lt;message.EmbeddedObjects.Count;i++) message.EmbeddedObjects[i].StoreToFile("C:\\mails\\objects\\"+message.EmbeddedObjects[i].Filename);
        /// </code>
        /// </example>
        public EmbeddedObjectCollection EmbeddedObjects
        {
            get
            {
                return _embeddedObjects;
            }
        }

        /// <summary>
        /// Contains messages that were included as message/rfc822 parts.
        /// </summary>
        public MessageCollection SubMessages
        {
            get
            {
                return _subMessages;
            }
        }

        /// <summary>
        /// Contains all parts that are not of "multipart" MIME type.
        /// </summary>
        public MimePartCollection LeafMimeParts
        {
            get
            {
                return _allMimeParts;
            }
        }

        /// <summary>
        /// Contains all parts for which no Content-Disposition header was found. Disposition is left to the final agent.
        /// </summary>
        public MimePartCollection UnknownDispositionMimeParts
        {
            get
            {
                return _otherParts;
            }
            set
            {
                _otherParts = value;
            }
        }

        /// <summary>
        /// A reference to the root of this message's part tree.
        /// </summary>
        /// <example>
        /// For multipart/alternative messages, you can access the two bodies via the Message.PartTreeRoot.SubParts property, as well as via the Message.BodyHtml and Message.BodyText properties.
        /// </example>
        public MimePart PartTreeRoot
        {
            get { return _partTreeRoot; }
            set { _partTreeRoot = value; }
        }

        /// <summary>
        /// The message's plain text body (if present).
        /// </summary>
        /// <remarks>This is often the alternate version of the HTML body used by non-HTML clients.</remarks>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// Message message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Show the message's text body 
        /// this.Response.Write("Message body : "+message.BodyText.Text);
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf")
        /// 'Retrieve message.
        /// Dim message As Message = pop.RetrieveMessageObject(1,False)
        /// pop.Disconnect()
        /// 'Show message's text body.
        /// Me.Response.Write("Message body : " + message.BodyText.Text)
        ///  
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// var message:Message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Show message's text body.
        /// this.Response.Write("Message body : "+message.BodyText.Text);
        /// </code>
        /// </example>
        public MimeBody BodyText
        {
            get
            {
                return _bodyText;
            }
            set
            {
                _bodyText = value;
            }
        }

        /*public byte[] BodyBinary
        {
            get
            {
                return this._bodyBinary;
            }
            set
            {
                this._bodyBinary = value;
            }
        }*/

        /// <summary>
        /// The message's HTML formatted body (if present).
        /// </summary>
        /// <example>
        /// <code>
        /// C#
        /// 
        /// Pop3Client pop = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// Message message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Show the message's html body 
        /// this.Response.Write("Message body : "+message.BodyHtml.Text);
        /// 
        /// VB.NET
        /// 
        /// Dim pop As New Pop3Client
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf")
        /// 'Retrieve message.
        /// Dim message As Message = pop.RetrieveMessageObject(1,False)
        /// pop.Disconnect()
        /// 'Show message's html body.
        /// Me.Response.Write("Message body : " + message.BodyHtml.Text)
        ///  
        /// JScript.NET
        /// 
        /// var pop:Pop3Client = new Pop3Client();
        /// pop.Connect("mail.myhost.com","jdoe1234","tanstaaf");
        /// //Retrieve message.
        /// var message:Message = pop.RetrieveMessageObject(1,false);
        /// pop.Disconnect();
        /// //Show message's html body.
        /// this.Response.Write("Message body : "+message.BodyHtml.Text);
        /// </code>
        /// </example>
        public MimeBody BodyHtml
        {
            get
            {
                return _bodyHtml;
            }
            set
            {
                _bodyHtml = value;
            }
        }

        public string Preamble
        {
            get
            {
                return _preamble;
            }
            set
            {
                _preamble = value;
            }
        }

        public string Epilogue
        {
            get
            {
                return _epilogue;
            }
            set
            {
                _epilogue = value;
            }
        }

        /// <summary>
        /// The size of the RFC 2822 formatted message in octets.
        /// </summary>
        public int Size
        {
            get
            {
                return OriginalData.Length;
            }
        }

        /// <summary>
        /// Returns: Is message contains type equal 'multipart/report' RFC 1894
        /// </summary>
        public bool IsMultipartReport { get; set; }

        /// <summary>
        /// Returns a printable HTML formated summary of the message.
        /// </summary>
        public new string Summary
        {
            get
            {
                string msg = (this.From.Email != "") ? this.From.Link : this.Sender.Link;
                msg += "<br />";
                msg += "To : " + this.To.Links + "<br />";
                if (this.Cc != null) msg += "Cc : " + this.Cc.Links + "<br />";
                msg += "Subject : " + this.Subject + "<br />";
                msg += "Received : " + this.DateString + "<br />";
                msg += "Body : <br />" + this.BodyText.Text;
                return msg;
            }
        }

        #endregion

        #region Constuctors

        public Message()
        {
            
        }

        public Message(Header header)
        {
            HeaderFields = header.HeaderFields;
            HeaderFieldNames = header.HeaderFieldNames;
            Trace.AddRange(header.Trace);
            To.AddRange(header.To);
            Cc.AddRange(header.Cc);
            Bcc.AddRange(header.Bcc);
            ReplyTo = header.ReplyTo;
            From = header.From;
            Sender = header.Sender;
            ContentType = header.ContentType;
            ContentDisposition = header.ContentDisposition;
        }

        #endregion

        #region Methods

        #region Private utility methods

        private string GetEncodedMimePart(MimePart part)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                sb.Append(part.ToMimeString());
                if (part.ContentTransferEncoding == ContentTransferEncoding.Base64) sb.Append("\r\n\r\n" + System.Text.RegularExpressions.Regex.Replace(System.Convert.ToBase64String(part.BinaryContent, 0, part.BinaryContent.Length), "(?<found>[^\n]{100})", "${found}\n"));
                else if (part.ContentTransferEncoding == ContentTransferEncoding.QuotedPrintable) sb.Append("\r\n\r\n" + Codec.ToQuotedPrintable(System.Text.Encoding.ASCII.GetString(part.BinaryContent, 0, part.BinaryContent.Length), part.Charset));
                else if (part.ContentTransferEncoding == ContentTransferEncoding.SevenBits) sb.Append("\r\n\r\n" + System.Text.Encoding.UTF7.GetString(part.BinaryContent, 0, part.BinaryContent.Length));
                else if (part.ContentTransferEncoding == ContentTransferEncoding.EightBits) sb.Append("\r\n\r\n" + System.Text.Encoding.UTF8.GetString(part.BinaryContent, 0, part.BinaryContent.Length));
                else sb.Append("\r\n\r\n" + System.Text.Encoding.ASCII.GetString(part.BinaryContent,0,part.BinaryContent.Length));
            }
            catch (System.Exception) { }
            return sb.ToString();
        }

        private string GetEmbeddedObjects(string boundary)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (MimePart part in this.EmbeddedObjects) sb.Append(boundary + part.ToMimeString());
            return sb.ToString();
        }

        private string GetAttachments(string boundary)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (MimePart part in this.Attachments) sb.Append(boundary + part.ToMimeString());
            return sb.ToString();
        }

        private MimePart GetBodiesPart()
        {
            MimePart bodies = new MimePart();

            bool html = (this.BodyHtml.Text.Length > 0);
            bool plain = (this.BodyText.Text.Length > 0);

            // If we have both, make a multipart/alternative container.
            if (html && plain)
            {
                bodies.ContentType.MimeType = "multipart/alternative";

                string unique = Codec.GetUniqueString();
                string boundary = "---AU_MimePart_" + unique;
                bodies.ContentType.Parameters.Add("boundary", boundary);

                bodies.SubParts.Add(this.BodyText.ToMimePart());
                bodies.SubParts.Add(this.BodyHtml.ToMimePart());
            }
            else if (html)
            {
                bodies = this.BodyHtml.ToMimePart();
            }
            else if (plain)
            {
                bodies = this.BodyText.ToMimePart();
            }

            return bodies;
        }

        private MimePart GetMultipartRelatedContainer()
        {
            MimePart part = new MimePart();

            if (this.EmbeddedObjects.Count > 0)
            {
                part.ContentType.MimeType = "multipart/related";

                string unique = Codec.GetUniqueString();
                string boundary = "---AU_MimePart_" + unique;
                part.ContentType.Parameters.Add("boundary", boundary);

                part.ContentType.Parameters.Add("type", "\"multipart/alternative\"");

                part.SubParts.Add(this.GetBodiesPart());

                foreach (MimePart embeddedObject in this.EmbeddedObjects)
                    part.SubParts.Add(embeddedObject);
            }
            else part = this.GetBodiesPart();

            return part;
        }

        private MimePart GetMultipartMixedContainer()
        {
            MimePart part = new MimePart();

            if (this.Attachments.Count > 0
                || this.UnknownDispositionMimeParts.Count > 0
                || this.SubMessages.Count > 0)
            {
                part.ContentType.MimeType = "multipart/mixed";

                string unique = Codec.GetUniqueString();
                string boundary = "---AU_MimePart_" + unique;
                part.ContentType.Parameters.Add("boundary", boundary);

                part.SubParts.Add(this.GetMultipartRelatedContainer());

                foreach (MimePart attachment in this.Attachments)
                    part.SubParts.Add(attachment);

                foreach (MimePart otherpart in this.UnknownDispositionMimeParts)
                    part.SubParts.Add(otherpart);

                foreach (Message message in this.SubMessages)
                    part.SubParts.Add(message.ToMimePart());
            }
            else part = this.GetMultipartRelatedContainer();

            return part;
        }

        #endregion

        #region Public methods

        /*public string GetBodies(string boundary)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if((this.BodyHtml.Text!=null && this.BodyHtml.Text.Length>0) && (this.BodyText.Text!=null && this.BodyText.Text.Length>0))
                {
                    sb.Append("--"+boundary);
                    boundary = "---AU_MimePart_"+Codec.GetUniqueString();
                    sb.Append("\r\nContent-Type: multipart/alternative;\r\n boundary=\""+boundary+"\"\r\n\r\n--"+boundary+"\r\nContent-Type: text/plain;\r\n charset=\""+this.BodyText.Charset+"\"\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n"+Codec.ToQuotedPrintable(this.BodyText.Text,this.BodyHtml.Charset));
                    sb.Append("\r\n\r\n--"+boundary+"\r\nContent-Type: text/html;\r\n charset=\""+this.BodyHtml.Charset+"\"\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n"+Codec.ToQuotedPrintable(this.BodyHtml.Text,this.BodyHtml.Charset));
                    sb.Append("\r\n\r\n--"+boundary+"--");
                }
                else if(this.BodyHtml.Text!=null && this.BodyHtml.Text.Length>0) sb.Append("--"+boundary+"\r\nContent-Type: text/html;\r\n charset=\""+this.BodyHtml.Charset+"\"\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n"+Codec.ToQuotedPrintable(this.BodyHtml.Text.TrimEnd('\n','\r'),this.BodyHtml.Charset));
                else if(this.BodyText.Text!=null && this.BodyText.Text.Length>0) sb.Append("--"+boundary+"\r\nContent-Type: text/plain;\r\n charset=\""+this.BodyText.Charset+"\"\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n"+Codec.ToQuotedPrintable(this.BodyText.Text.TrimEnd('\n','\r'),this.BodyText.Charset));
            } 
            catch(System.Exception) {  }
            return sb.ToString();
        }*/

        /// <summary>
        /// Converts a message to a message/rfc822 type MIME part, with a Content-Disposition set to "attachment".
        /// </summary>
        public MimePart ToMimePart()
        {
            MimePart part = new MimePart();
            try
            {
                part.Charset = this.Charset;
                part.ContentTransferEncoding = ContentTransferEncoding.SevenBits;
                part.ContentDisposition.Disposition = "attachment";
                part.ContentDisposition.FileName = this.Subject.Trim(' ').Replace(" ", "_") + ".eml";
                part.ContentType.MimeType = "message/rfc822";
                part.TextContent = this.ToMimeString();
            }
            catch (System.Exception) { }
            return part;
        }

        /// <summary>
        /// The MIME representation of the message.
        /// </summary>
        /// <returns></returns>
        public string ToMimeString()
        {
            return ToMimeString(false);
        }

        /// <summary>
        /// The MIME representation of the message.
        /// </summary>
        /// <param name="removeBlindCopies">if set to <c>true</c> remove blind copies (BCC) from the header.</param>
        /// <returns></returns>
        public string ToMimeString(bool removeBlindCopies)
        {
            
            CheckBuiltMimePartTree();

            //if (!ActiveUp.Base.InternalLicense.Status.IsRegistered)
            //{
            //    if (this.BodyHtml.Text.Length > 0
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlReceived) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlSent) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlReceived) == -1)
            //        this.BodyHtml.Text = ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent + "<br><br>" +  this.BodyHtml.Text;

            //    if ((this.BodyText.Text.Length > 0 || this.BodyHtml.Text.Length == 0)
            //        && this.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextSent) == -1
            //        && this.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextReceived) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextSent) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextReceived) == -1)
            //        this.BodyText.Text = ActiveUp.Base.InternalLicense.UnRegisteredTextSent + "\r\n\r\n" + this.BodyText.Text; 
            //}
            //else if (ActiveUp.Base.InternalLicense.IsSponsored())
            //{
            //    if (this.BodyHtml.Text.Length > 0
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlReceived) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlSent) == -1
            //        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlReceived) == -1)
            //        this.BodyHtml.Text += "<br><br>" + ActiveUp.Base.InternalLicense.SponsorHtmlSent;
            //}

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Concat(((Header)this).ToHeaderString(removeBlindCopies).TrimEnd('\r', '\n'), Tokenizer.NewLine));
            sb.Append(Tokenizer.NewLine);
            string messageAsPart = this.PartTreeRoot.ToMimeString();
            int bodyStart = Regex.Match(messageAsPart, @"(?<=\r?\n\r?\n).").Index;
            sb.Append(messageAsPart.Substring(bodyStart).TrimStart('\r', '\n'));

            string toReturn = sb.ToString();
            if (this.ContentType.Type.Equals("multipart")) toReturn = sb.ToString().TrimEnd('\r', '\n');
            
            return toReturn;
        }

        /// <summary>
        /// Checks the built MIME part tree.
        /// </summary>
        public void CheckBuiltMimePartTree()
        {
            if (!_builtMimePartTree)
            {
                BuildMimePartTree();
            }
        }

        /// <summary>
        /// Creates the MIME part structure. This method MUST be invoked before sending, storing, signing, encrypting, or invoking ToMimeString method.
        /// </summary>
        public void BuildMimePartTree()
        {
            this.PartTreeRoot = this.GetMultipartMixedContainer();

            this.ContentType = this.PartTreeRoot.ContentType;

            _builtMimePartTree = true;
        }

        /*/// <summary>
        /// The message's Rfc822 compliant representation.
        /// </summary>
        /// <remarks>This property is often used for message transfer.</remarks>
        public string ToMimeString()
        {
            if (!ActiveUp.Base.InternalLicense.Status.IsRegistered)
                {
                    if (this.BodyHtml.Text.Length > 0
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlReceived) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlSent) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlReceived) == -1)
                        this.BodyHtml.Text = ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent + "<br><br>" +  this.BodyHtml.Text;
                    
                    if ((this.BodyText.Text.Length > 0 || this.BodyHtml.Text.Length == 0)
                        && this.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextSent) == -1
                        && this.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextReceived) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextSent) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextReceived) == -1)
                        this.BodyText.Text = ActiveUp.Base.InternalLicense.UnRegisteredTextSent + "\n\n" + this.BodyText.Text; 
                }
                else if (ActiveUp.Base.InternalLicense.IsSponsored())
                {
                    if (this.BodyHtml.Text.Length > 0
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlSent) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredHtmlReceived) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlSent) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorHtmlReceived) == -1)
                        this.BodyHtml.Text += "<br><br>" + ActiveUp.Base.InternalLicense.SponsorHtmlSent;

                    if ((this.BodyText.Text.Length > 0 || this.BodyHtml.Text.Length == 0)
                        && this.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextSent) == -1
                        && this.BodyText.Text.IndexOf(ActiveUp.Base.InternalLicense.UnRegisteredTextReceived) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextSent) == -1
                        && this.BodyHtml.Text.IndexOf(ActiveUp.Base.InternalLicense.SponsorTextReceived) == -1)
                        this.BodyText.Text += "\n\n" + ActiveUp.Base.InternalLicense.SponsorTextSent;
                }
                ActiveUp.Net.Mail.Logger.AddEntry(this.ContentType.MimeType);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    //this.Date = System.DateTime.UtcNow;
                    if (this.Date == System.DateTime.MinValue)
                        this.Date = System.DateTime.UtcNow;
                    if (this.ContentType.MimeType == null) this.ContentType.MimeType = "text/plain";
                    if (this.ContentType == null || this.ContentType.MimeType.Length == 0 || this.ContentType.MimeType.ToLower().IndexOf("multipart/") != -1 || this.ContentType.MimeType.ToLower().IndexOf("text/") != -1 || this.ContentType.MimeType.ToLower().IndexOf("application/") != -1)
                    {
                        MimePartCollection attachandsub = new MimePartCollection();
                        attachandsub = this.Attachments.ConcatMessagesAsPart(this.SubMessages);
                        if(this.EmbeddedObjects.Count>0 && attachandsub.Count>0)
                        {
                            string unique = Codec.GetUniqueString();
                            string boundary = "---AU_MimePart_"+unique;
                            if(this.HeaderFields["content-type"]!=null && this.HeaderFields["content-type"].Length>0) this.HeaderFields["content-type"] = "multipart/mixed;\r\n boundary=\""+boundary+"\"";
                            else this.HeaderFields.Add("content-type","multipart/mixed;\r\n boundary=\""+boundary+"\"");
                            sb.Append(((Header)this).ToHeaderString()+"\r\n\r\n");
                            if(this.Preamble!=null && this.Preamble.Length>0) sb.Append(this.Preamble+"\r\n\r\n");
                            else sb.Append("This is a multi-part message in MIME format.\r\n\r\n");
                            string newboundary = "---AU_MimePart_"+Codec.GetUniqueString();
                            sb.Append("--"+boundary+"\r\nContent-Type: multipart/related;\r\n boundary=\""+newboundary+"\"\r\n\r\n");
                            sb.Append(this.GetBodies(newboundary));
                            foreach(MimePart part in this.EmbeddedObjects) if(part.TextContent!=this.BodyHtml.Text && part.TextContent!=this.BodyText.Text) sb.Append("\r\n\r\n--"+newboundary+"\r\n"+part.ToMimeString());
                            sb.Append("\r\n--" + newboundary + "--\r\n");
                            foreach(MimePart part in attachandsub) sb.Append("\r\n--"+boundary+"\r\n"+part.ToMimeString());
                            sb.Append("\r\n--" + boundary + "--\r\n");
                            if(this.Epilogue!=null && this.Epilogue.Length>0) sb.Append("\r\n\r\n"+this.Epilogue);
                        }
                        else if(attachandsub.Count>0 || (this.EmbeddedObjects.Count>0 && attachandsub.Count>0))
                        {
                            string unique = Codec.GetUniqueString();
                            string boundary = "---AU_MimePart_"+unique;
                            if(this.HeaderFields["content-type"]!=null && this.HeaderFields["content-type"].Length>0) this.HeaderFields["content-type"] = "multipart/mixed;\r\n boundary=\""+boundary+"\"";
                            else this.HeaderFields.Add("content-type","multipart/mixed;\r\n boundary=\""+boundary+"\"");
                            sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                            if(this.Preamble!=null && this.Preamble.Length>0) sb.Append(this.Preamble+"\r\n\r\n");
                            else sb.Append("This is a multi-part message in MIME format.\r\n\r\n");
                            sb.Append(this.GetBodies(boundary));
                            foreach(MimePart part in attachandsub)
                                sb.Append("\r\n--"+boundary+"\r\n"+part.ToMimeString());
                            sb.Append("\r\n--" + boundary + "--\r\n");
                            if(this.Epilogue!=null && this.Epilogue.Length>0) sb.Append("\r\n\r\n"+this.Epilogue);
                        }
                        else if(this.EmbeddedObjects.Count>0)
                        {
                            string unique = Codec.GetUniqueString();
                            string boundary = "---AU_MimePart_"+unique;
                            if(this.HeaderFields["content-type"]!=null && this.HeaderFields["content-type"].Length>0) this.HeaderFields["content-type"] = "multipart/related;\r\n boundary=\""+boundary+"\"";
                            else this.HeaderFields.Add("content-type","multipart/related;\r\n boundary=\""+boundary+"\"");
                            sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                            if(this.Preamble!=null && this.Preamble.Length>0) sb.Append(this.Preamble+"\r\n\r\n");
                            else sb.Append("This is a multi-part message in MIME format.\r\n\r\n");
                            sb.Append(this.GetBodies(boundary));
                            foreach(MimePart part in this.EmbeddedObjects) if(part.TextContent!=this.BodyHtml.Text && part.TextContent!=this.BodyText.Text) sb.Append("\r\n--"+boundary+"\r\n"+part.ToMimeString());
                            sb.Append("\r\n--" + boundary + "--\r\n");
                            if(this.Epilogue!=null && this.Epilogue.Length>0) sb.Append("\r\n\r\n"+this.Epilogue);
                        }
                        else
                        {
                            if((this.BodyHtml.Text!=null && this.BodyHtml.Text.Length>0) && (this.BodyText.Text!=null && this.BodyText.Text.Length>0))
                            {
                                string unique = Codec.GetUniqueString();
                                string boundary = "---AU_MimePart_"+unique;
                                if(this.HeaderFields["content-type"]!=null && this.HeaderFields["content-type"].Length>0) this.HeaderFields["content-type"] = "multipart/alternative;\r\n boundary=\""+boundary+"\"";
                                else this.HeaderFields.Add("content-type","multipart/alternative;\r\n boundary=\""+boundary+"\"");
                                sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                                if(this.Preamble!=null && this.Preamble.Length>0) sb.Append(this.Preamble+"\r\n\r\n");
                                else sb.Append("This is a multi-part message in MIME format.\r\n\r\n");
                                sb.Append("--"+boundary+"\r\nContent-Type: text/plain;\r\n charset=\""+this.BodyText.Charset+"\"\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n"+Codec.ToQuotedPrintable(this.BodyText.Text,this.BodyText.Charset));
                                sb.Append("\r\n--"+boundary+"\r\nContent-Type: text/html;\r\n charset=\""+this.BodyHtml.Charset+"\"\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n"+Codec.ToQuotedPrintable(this.BodyHtml.Text,this.BodyHtml.Charset));
                                sb.Append("\r\n--"+boundary+"--\r\n");
                                if(this.Epilogue!=null && this.Epilogue.Length>0) sb.Append("\r\n\r\n"+this.Epilogue);
                            }
                            else if (this.ContentType.MimeType != null && this.ContentType.MimeType.Equals("application/pkcs7-mime"))
                            {
                                sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                                sb.Append(Convert.ToBase64String(this.BodyBinary));
                            }
                            else if(this.BodyHtml.Text!=null && this.BodyHtml.Text.Length>0)
                            {
                                this.ContentType.MimeType = "text/html";
                                if (this.ContentTransferEncoding == ContentTransferEncoding.Unknown || this.ContentTransferEncoding == ContentTransferEncoding.None)
                                    this.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
                                this.Charset = this.BodyHtml.Charset;
                                sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                                sb.Append(Codec.ToQuotedPrintable(this.BodyHtml.Text,this.Charset));
                                if(this.Epilogue!=null && this.Epilogue.Length>0) sb.Append("\r\n\r\n"+this.Epilogue);
                            }
                            else
                            {
                                this.ContentType.MimeType = "text/plain";
                                if (this.ContentTransferEncoding == ContentTransferEncoding.Unknown || this.ContentTransferEncoding == ContentTransferEncoding.None)
                                    this.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
                                this.Charset = this.BodyText.Charset;
                                sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                                sb.Append(Codec.ToQuotedPrintable(this.BodyText.Text, this.Charset));
                                if (this.Epilogue != null && this.Epilogue.Length > 0) sb.Append("\r\n\r\n" + this.Epilogue);
                            }
                            if (this.Signatures.Smime.SignerInfos.Count > 0)
                            {

                            }
                        }
                    }
                    else 
                    {
                        byte[] bb = this.BodyBinary;
                        if(bb.Length>0)
                        {
                            sb.Append(((Header)this).ToHeaderString() + "\r\n\r\n");
                            sb.Append(Codec.Wrap(System.Convert.ToBase64String(bb),78));
                        }
                    }
                //}
                //catch(System.Exception) {  }
                return sb.ToString();
                return "";
        }*/

#if !PocketPC
        /// <summary>
        /// Returns a MID references, using the message's Message-ID.
        /// See RFC 2111 for more information.
        /// </summary>
        public string GetMidReference()
        {
            return "mid:" + System.Web.HttpUtility.UrlEncode(this.MessageId.Trim('<', '>'));
        }

#endif

        /// <summary>
        /// Appends the message to the given mailbox.
        /// </summary>
        /// <remarks>The mailbox's sourceclient has to be connected.</remarks>
        /// <param name="imapMailbox">The mailbox the message has to be appended to.</param>
        /// <example>
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///
        ///Imap4Client imap = new Imap4Client();
        ///imap.Connect("mail.myhost.com",8505);
        ///Mailbox inbox = imap.SelectMailbox("inbox");
        ///
        ///message.Append(inbox);
        ///
        ///imap.Close();
        ///imap.Disconnect();
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///Dim imap As New Imap4Client
        ///imap.Connect("mail.myhost.com",8505)
        ///Dim inbox As Mailbox = imap.SelectMailbox("inbox")
        ///
        ///message.Append(inbox)
        ///
        ///imap.Close()
        ///imap.Disconnect()
        ///  
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///
        ///var imap:Imap4Client = new Imap4Client();
        ///imap.Connect("mail.myhost.com",8505);
        ///var inbox:Mailbox = imap.SelectMailbox("inbox");
        ///
        ///message.Append(inbox);
        ///
        ///imap.Close();
        ///imap.Disconnect();
        /// </code>
        /// </example>
        public void Append(IMailbox imapMailbox)
        {
            imapMailbox.Append(this.ToString());
        }

        private delegate void DelegateAppend(IMailbox imapMailbox);
        private DelegateAppend _delegateAppend;

        public IAsyncResult BeginAppend(IMailbox imapMailbox, AsyncCallback callback)
        {
            this._delegateAppend = this.Append;
            return this._delegateAppend.BeginInvoke(imapMailbox, callback, this._delegateAppend);
        }

        public void EndAppend(IAsyncResult result)
        {
            this._delegateAppend.EndInvoke(result);
        }

        /// <summary>
        /// Stores the message to the specified file using a temp file.
        /// </summary>
        /// <param name="fileName">Path and filename to store the message in.</param>
        /// <param name="useTemp"></param>
        /// <returns>The path the message has been stored at.</returns>
        /// <example>
        /// This retrieves the first message's from the remote POP server and stores it on the disk.<br />
        /// You can read it back using the ParserMessage() method in the Parser class.
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.StoreToFile("C:\\messages\\outbox\\tobesent.eml", true);
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.StoreToFile("C:\messages\outbox\tobesent.eml", true)
        /// 
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.StoreToFile("C:\\messages\\outbox\\tobesent.eml", true);
        /// </code>
        /// </example> 
        public string StoreToFile(string fileName, bool useTemp)
        {
            string tempPath = "";
            if (useTemp)
                tempPath = System.IO.Path.GetTempFileName();
            else
                tempPath = fileName;
            System.IO.StreamWriter sw = System.IO.File.CreateText(tempPath);
            sw.Write(this.ToMimeString());
            sw.Close();
           
            if (useTemp)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    // Create the traget path iof it does not exist
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(fileName)))
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));

                    if (string.IsNullOrEmpty(System.IO.Path.GetFileName(fileName)))
                    {
                        string savePath = System.IO.Path.Combine(fileName, System.IO.Path.GetFileNameWithoutExtension(tempPath) + ".eml");
                        System.IO.File.Move(tempPath, savePath);
                        System.IO.File.Delete(tempPath);
                        return savePath;
                    }
                    else
                    {                       
                        System.IO.File.Move(tempPath, fileName);
                        System.IO.File.Delete(tempPath);                                       
                    }
                        
                    return fileName;
                }

                return tempPath;
            }

            return tempPath;
            
            
        }

        /// <summary>
        /// Stores the message to the specified path.
        /// </summary>
        /// <param name="path">Path to store the message at.</param>
        /// <returns>The path the message has been stored at.</returns>
        /// <example>
        /// This retrieves the first message's from the remote POP server and stores it on the disk.<br />
        /// You can read it back using the ParserMessage() method in the Parser class.
        /// <code>
        /// C#
        /// 
        ///Message message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.StoreToFile("C:\\messages\\outbox\\tobesent.eml");
        /// 
        /// VB.NET
        /// 
        ///Dim message As New Message
        ///message.From = new Address("jdoe@myhost.com","John Doe")
        ///message.To.Add("mjohns@otherhost.com","Mike Johns")
        ///message.Subject = "hey!"
        ///message.Attachments.Add("C:\myfile.doc")
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John."
        ///message.StoreToFile("C:\messages\outbox\tobesent.eml")
        /// 
        /// JScript.NET
        /// 
        ///var message:Message = new Message();
        ///message.From = new Address("jdoe@myhost.com","John Doe");
        ///message.To.Add("mjohns@otherhost.com","Mike Johns");
        ///message.Subject = "hey!";
        ///message.Attachments.Add("C:\\myfile.doc");
        ///message.BodyHtml.Text = "As promised, the requested document.&lt;br />&lt;br />Regards,&lt;br>John.";
        ///message.StoreToFile("C:\\messages\\outbox\\tobesent.eml");
        /// </code>
        /// </example> 
        public override string StoreToFile(string path)
        {
            return StoreToFile(path, false);
        }

        /// <summary>
        /// Detects if a message is a delivery failure notification.
        /// This method uses the default signatures containing in an internal ressource file.
        /// </summary>
        /// <returns>A BounceStatus object containing the level of revelance and if 100% identified, the erroneous email address.</returns>
        public new BounceResult GetBounceStatus()
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
        public new BounceResult GetBounceStatus(string signaturesFilePath)
        {
            string ressource = string.Empty;
            
            if (signaturesFilePath == null || signaturesFilePath == string.Empty)
                ressource = Header.GetResource("ActiveUp.Net.Mail.bouncedSignatures.xml");
            else
                ressource = System.IO.File.OpenText(signaturesFilePath).ReadToEnd();

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(ressource);
            BounceResult result = new BounceResult();

            foreach (System.Xml.XmlElement el in doc.GetElementsByTagName("signature"))
            {
                int bestResult = result.Level;
                result.Level = 0;

                if (el.GetAttribute("from").Trim() != "")
                    if (this.From.Merged.IndexOf(el.GetAttribute("from")) != -1)
                        result.Level++;

                if (this.Subject != null && el.GetAttribute("subject").Trim() != "")
                    if (this.Subject.IndexOf(el.GetAttribute("subject")) != -1)
                        result.Level++;

                if (el.GetAttribute("body").Trim() != "")
                    if (this.BodyText.Text.IndexOf(el.GetAttribute("body")) != -1)
                        result.Level++;

                if (result.Level < bestResult)
                    result.Level = bestResult;

                if (result.Level > 0)
                {
                    int start = 0;
                    string body = this.BodyText.Text;

                    if (el.GetAttribute("body") != string.Empty)
                        start = body.IndexOf(el.GetAttribute("body"));

                    if (start < 0)
                        start = 0;

                    string emailExpression = "\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
                    System.Text.RegularExpressions.Regex regExp = new System.Text.RegularExpressions.Regex(emailExpression);

                    if (regExp.IsMatch(body, start))
                    {
                        System.Text.RegularExpressions.Match match = regExp.Match(body, start);
                        result.Email = match.Value;
                    }

                    break;
                }
            }

            return result;
        }
        
#if !PocketPC
        /// <summary>
        /// Gets a real copy of the actual message object. This will not return a reference.
        /// </summary>
        /// <returns>The new Message object.</returns>
        public Message Clone()
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            binFormatter.Serialize(stream, this);
            stream.Position = 0;

            Message ret = (Message)binFormatter.Deserialize(stream);
            ret.Signatures = this.Signatures;

            return ret;
        }
#else
        /// <summary>
        /// Gets a real copy of the actual message object. This will not return a reference.
        /// </summary>
        /// <returns>The new Message object.</returns>
        public Message Clone()
        {
            //TODO : XMLSerializer is not working. Find a method to Clone without doing too much.
            //XmlSerializer requires Add(System.String) at every level of heirarchy, so it's clearly not working.
            System.Xml.Serialization.XmlSerializer xmlForm = new System.Xml.Serialization.XmlSerializer(typeof(Message));
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            xmlForm.Serialize(stream, this);
            stream.Position = 0;
            return (Message)xmlForm.Deserialize(stream);
        }
#endif

        /// <summary>
        /// Extracts the original message from the S/MIME envelope and decrypts it. 
        /// The certificates used to decrypt are those contained in the current user's personal store.
        /// </summary>
        /// <returns>A Message object representing the message as it was before encryption.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// // We retrieved a Message object by some means and have a reference to it in variable message.
        /// 
        /// Message originalMessage = message.SmimeDevelopeAndDecrypt();
        /// 
        /// //originalMessage contains all information about the encrypted message.
        /// 
        /// </code>
        /// </example>
        #if !PocketPC
        public Message SmimeDevelopeAndDecrypt()
        {
            return this.SmimeDevelopeAndDecrypt(new X509Certificate2Collection());
        }
        #endif
        /// <summary>
        /// Extracts the original message from the S/MIME envelope and decrypts it.
        /// </summary>
        /// <param name="extraStore">Certificates with private keys to be used in addition to those found in the current user's personal store.</param>
        /// <returns>A Message object representing the message as it was before encryption.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// // Load a certificate (with private key) and add it to the collection.
        /// X509Certificate2 cert = new X509Certificate2("C:\\mycertificate.pfx");
        /// 
        /// // We retrieved a Message object by some means and have a reference to it in variable message.
        /// Message originalMessage = message.SmimeDevelopeAndDecrypt(new X509Certificate2Collection(cert));
        /// 
        /// //originalMessage contains all information about the encrypted message.
        /// 
        /// </code>
        /// </example>
#if !PocketPC
        public Message SmimeDevelopeAndDecrypt(X509Certificate2Collection extraStore)
        {
            if (!this.IsSmimeEncrypted) throw new InvalidOperationException("This message doesn't seem to be encrypted, or the encryption method is unknown.");
            else
            {
                EnvelopedCms cms = new EnvelopedCms();
                cms.Decode(this.PartTreeRoot.BinaryContent);
                cms.Decrypt(extraStore);

                Message sub = Parser.ParseMessage(cms.ContentInfo.Content);

                return sub;
            }
        }
#endif
        /// <summary>
        /// Extracts the original message from the S/MIME envelope and exposes the SignedCms object containing signature informations via the Message.Signatures.Smime property.
        /// </summary>
        /// <returns>A Message object representing the message as it was before signing.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// // We retrieved a Message object by some means and have a reference to it in variable message.
        /// Message originalMessage = message.SmimeDevelopeAndExposeSignature();
        /// 
        /// // Check the signature. The "true" argument indicates that we don't want to verify the signe's certificates at this time, but only the signature itself.
        /// try
        /// {
        ///     originalMessage.Signatures.Smime.CheckSignature(true);
        /// }
        /// catch(CryptographicException ex)
        /// {
        ///     // Signature is invalid, do something.
        /// }
        /// </code>
        /// </example>
        #if !PocketPC
        public Message SmimeDevelopeAndExposeSignature()
        {
            if (!this.HasSmimeSignature) throw new InvalidOperationException("This message doesn't seem to be signed, or the signing method is unknown.");
            else
            {
                SignedCms cms = new SignedCms();
                cms.Decode(this.PartTreeRoot.BinaryContent);

                Message sub = Parser.ParseMessage(cms.ContentInfo.Content);

                sub.Signatures.Smime = cms;

                return sub;
            }
        }
        #endif
        /// <summary>
        /// Encrypts the message and envelopes it for one recipient.
        /// </summary>
        /// <param name="recipient">An object containing the recipient's certificate with public key.</param>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// X509Certificate2 cert = new X509Ceritificate2("C:\\recipient.cer");
        /// 
        /// CmsRecipient recipient = new CmsRecipient(cert);
        /// 
        /// message.SmimeEnvelopeAndEncryptFor(recipient);
        /// </code>
        /// </example>
#if !PocketPC
        public void SmimeEnvelopeAndEncryptFor(CmsRecipient recipient)
        {
            this.SmimeEnvelopeAndEncryptFor(new CmsRecipientCollection(recipient));
        }
        /// <summary>
        /// Encrypts the message and envelopes it for multiple recipients.
        /// </summary>
        /// <param name="recipients">An object containing the recipients' certificates with public keys.</param>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// CmsRecipientCollection recipients = new CmsRecipientCollection();
        /// 
        /// recipients.Add(new CmsRecipient(new X509Certificate2("C:\\recipient1.cer")));
        /// recipients.Add(new CmsRecipient(new X509Certificate2("C:\\recipient2.cer")));
        /// 
        /// message.SmimeEnvelopeAndEncryptFor(recipients);
        /// </code>
        /// </example>
        public void SmimeEnvelopeAndEncryptFor(CmsRecipientCollection recipients)
        {
            string mimeString = this.ToMimeString();
            byte[] toencrypt = Encoding.ASCII.GetBytes(mimeString);
            EnvelopedCms cms = new EnvelopedCms(new ContentInfo(toencrypt));
            cms.Encrypt(recipients);

            MimePart envelope = new MimePart();

            envelope.ContentType.MimeType = "application/pkcs7-mime";
            envelope.ContentType.Parameters.Add("smime-type", "encrypted-data");
            envelope.ContentType.Parameters.Add("name", "smime.p7m");
            envelope.ContentDisposition.Disposition = "attachment";
            envelope.ContentDisposition.FileName = "smime.p7m";
            envelope.ContentTransferEncoding = ContentTransferEncoding.Base64;

            envelope.BinaryContent = cms.Encode();

            this.PartTreeRoot = envelope;

            this.ContentType = this.PartTreeRoot.ContentType;
            this.ContentDisposition = this.PartTreeRoot.ContentDisposition;
            this.ContentTransferEncoding = this.PartTreeRoot.ContentTransferEncoding;
        }

        /// <summary>
        /// Signs the message and envelopes it.
        /// </summary>
        /// <param name="signer">An object containing the signer's information.</param>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// CmsSigner signer = new CmsSigner(new X509Certificate2("C:\\mycertificate.pfx"));
        /// 
        /// // Here we only want the signer's certificate to be sent along. Not the whole chain.
        /// signer.IncludeOption = X509IncludeOption.EndCertOnly;
        /// 
        /// message.SmimeEnvelopeAndSignBy(signer);
        /// </code>
        /// </example>
        public void SmimeEnvelopeAndSignBy(CmsSigner signer)
        {
            string mimeString = this.ToMimeString();
            byte[] tosign = Encoding.ASCII.GetBytes(mimeString);
            SignedCms cms = new SignedCms(new ContentInfo(tosign));
            cms.ComputeSignature(signer);

            MimePart envelope = new MimePart();

            envelope.ContentType.MimeType = "application/pkcs7-mime";
            envelope.ContentType.Parameters.Add("smime-type", "signed-data");
            envelope.ContentType.Parameters.Add("name", "smime.p7m");
            envelope.ContentDisposition.Disposition = "attachment";
            envelope.ContentDisposition.FileName = "smime.p7m";
            envelope.ContentTransferEncoding = ContentTransferEncoding.Base64;

            envelope.BinaryContent = cms.Encode();

            this.PartTreeRoot = envelope;

            this.ContentType = this.PartTreeRoot.ContentType;
            this.ContentDisposition = this.PartTreeRoot.ContentDisposition;
            this.ContentTransferEncoding = this.PartTreeRoot.ContentTransferEncoding;
        }

        /// <summary>
        /// Attaches a clear signature to the message. It is advised to do so when the receiving party might not be S/MIME capable.
        /// The content of the message is still visible, i.e. the message isn't enveloped.
        /// </summary>
        /// <param name="signer">An object containing the signer's information.</param>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// CmsSigner signer = new CmsSigner(new X509Certificate2("C:\\mycertificate.pfx"));
        /// 
        /// // Here we only want the signer's certificate to be sent along. Not the whole chain.
        /// signer.IncludeOption = X509IncludeOption.EndCertOnly;
        /// 
        /// message.SmimeAttachSignatureBy(signer);
        /// </code>
        /// </example>
        public void SmimeAttachSignatureBy(CmsSigner signer)
        {
            string body = this.PartTreeRoot.ToMimeString();
            byte[] tosign = Encoding.ASCII.GetBytes(body.TrimEnd('\r', '\n') + "\r\n");

            SignedCms cms = new SignedCms(new ContentInfo(tosign), true);
            cms.ComputeSignature(signer);

            MimePart envelope = new MimePart();

            this.Signatures.Smime = cms;
            envelope.ContentType.MimeType = "multipart/signed";
            envelope.ContentType.Parameters.Add("protocol", "\"application/x-pkcs7-signature\"");
            envelope.ContentType.Parameters.Add("micalg", cms.SignerInfos[0].DigestAlgorithm.FriendlyName);
            string unique = Codec.GetUniqueString();
            string boundary = "---AU_MimePart_" + unique;
            envelope.ContentType.Parameters.Add("boundary", boundary);

            envelope.SubParts.Add(this.PartTreeRoot);
            envelope.SubParts.Add(MimePart.GetSignaturePart(cms));

            this.PartTreeRoot = envelope;

            this.ContentType = this.PartTreeRoot.ContentType;
            this.ContentDisposition = this.PartTreeRoot.ContentDisposition;
            this.ContentTransferEncoding = this.PartTreeRoot.ContentTransferEncoding;
        }

        /// <summary>
        /// Generates the confirm read message.
        /// </summary>
        /// <returns></returns>
        public Message GenerateConfirmReadMessage()
        {
            Message message = new Message();

            // Inverse the recipient and sender
            message.To.Add(this.ConfirmRead);
            message.From = this.To[0];

            // Create the subject
            message.Subject = "Read: " + this.Subject;

            // Adds the original message ID
            
            message.AddHeaderField("In-Reply-To", this.MessageId);

            // Prepare the bodies

            DateTime dateReceived = this.Date;
            DateTime dateRead = DateTime.Now;

            message.BodyText.Text = string.Format(@"Your message

    To:  {0}
    Subject:  {1}
    Sent:  {2} {3}

was read on {4} {5}.", this.To[0].Email, this.Subject, dateReceived.ToShortDateString(), dateReceived.ToShortTimeString(),
                 dateRead.ToShortDateString(), dateRead.ToShortTimeString());

            message.BodyHtml.Text = string.Format(@"<P><FONT SIZE=3D2>Your message<BR>
<BR>
&nbsp;&nbsp;&nbsp; To:&nbsp; {0}<BR>
&nbsp;&nbsp;&nbsp; Subject:&nbsp; {1}<BR>
&nbsp;&nbsp;&nbsp; Sent:&nbsp; {2} {3}<BR>
<BR>
was read on {4} {5}.</FONT>
</P>", this.To[0].Email, this.Subject, dateReceived.ToShortDateString(), dateReceived.ToShortTimeString(),
                 dateRead.ToShortDateString(), dateRead.ToShortTimeString());

            // Create the repot mime part
            MimePart notificationPart = new MimePart();
            notificationPart.ContentType.MimeType = "message/disposition-notification";
            notificationPart.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;

            notificationPart.TextContent = string.Format(@"Reporting-UA: {0}; ActiveUp.MailSystem
Final-Recipient: rfc822;{1}
Original-Message-ID: <{2}>
Disposition: manual-action/MDN-sent-manually; displayed", "domain", this.To[0].Email, this.MessageId);

            message.UnknownDispositionMimeParts.Add(notificationPart);

            // Now we return the result
            return message;
        }
#endif

        public void AddAttachmentFromString(string filename, string body)
        {
            AddAttachmentFromString(filename, body, Encoding.GetEncoding("iso-8859-1"));
        }

        public void AddAttachmentFromString(string filename, string body, Encoding encoding)
        {
            var decoded_bytes = encoding.GetBytes(body);

            var text_attach = new MimePart(decoded_bytes, filename);

            Attachments.Add(text_attach);
        }


        #endregion

        #endregion

    }

    #endregion
}