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

using System.IO;
using ActiveUp.Net.Mail;
using System.Collections.Specialized;
#if !PocketPC
using System.Security.Cryptography.Pkcs;
#endif
using System;

namespace ActiveUp.Net.Mail 
{

    #region MimePart Object version 2
    
    /// <summary>
    /// The base class used by the Attachment and EmbeddedObject classes.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class MimePart
    {

        #region Constructors

        public MimePart()
        {
            ContentDisposition = new ContentDisposition();
            ContentType = new ContentType();
            BinaryContent = new byte[0];
            HeaderFields = new NameValueCollection();
            HeaderFieldNames = new NameValueCollection();
            SubParts = new MimePartCollection();
        }

        public MimePart(byte[] content, string fileName, string charset = null)
            : this()
        {
            BinaryContent = content;

            var ext = ".ext";
            if (!string.IsNullOrEmpty(fileName))
            {
                var collection = fileName.Split('.');
                if (collection.Length > 1)
                {
                    ext = collection[collection.Length - 1];
                }
            }

            ContentType.MimeType = MimeTypesHelper.GetMimeqType(ext);
            ContentDisposition.FileName = Codec.RFC2047Encode(fileName);
            ContentName = fileName;

            BuildTextContent(charset);
        }

        /// <summary>
        /// Creates a MimePart object with the content of the file located at the given path.
        /// </summary>
        /// <param name="path">File containing the content of the MimePart.</param>
        /// <param name="generateContentId">If true, a Content-ID Header field will be added to allow referencing of this part in the message.</param>
        /// <param name="charset">If the file contains text, the charset of the text can be provided to ensure better handling.</param>
        public MimePart(string path, bool generateContentId, string charset = null)
            : this(File.ReadAllBytes(path), Path.GetFileName(path), charset)
        {
            if (generateContentId)
                SetContentId();
        }

        /// <summary>
        /// Creates a MimePart object with the content of the file located at the given path.
        /// </summary>
        /// <param name="path">File containing the content of the MimePart.</param>
        /// <param name="contentId">The Content-ID Header field will be used for the part.</param>
        /// <param name="charset">If the file contains text, the charset of the text can be provided to ensure better handling.</param>
        public MimePart(string path, string contentId, string charset = null)
            : this(File.ReadAllBytes(path), Path.GetFileName(path), charset)
        {
            ContentId = contentId;
        }

        #endregion

        #region Private fields

        string _textContent, _contentName = string.Empty;
        ContentDisposition _contentDisposition = new ContentDisposition();
        ContentType _contentType = new ContentType();
        byte[] _binaryContent = new byte[0];
        NameValueCollection _fieldNames = new NameValueCollection();
        NameValueCollection _fields = new NameValueCollection();
        string _originalContent;
        MimePartCollection _subEntities = new MimePartCollection();
        MimePart _container;

        private void BuildTextContent(string charset = null)
        {
            if (IsText && !string.IsNullOrEmpty(charset))
            {
                Charset = charset;
                ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
                TextContent = System.Text.Encoding.GetEncoding(charset).GetString(BinaryContent, 0, BinaryContent.Length);
                return;
            }

            ContentTransferEncoding = ContentTransferEncoding.Base64;
            TextContent = Convert.ToBase64String(BinaryContent);
        }

        #endregion

        #region Methods

        #region Public methods

        public string MimeType
        {
            get { return ContentType.MimeType.ToLower(); }
        }

        public bool IsText
        {
            get { return MimeType.Contains("text/"); }
        }

        /// <summary>
        /// Generates a new Content-ID for the part.
        /// </summary>
        public void SetContentId()
        {
            this.ContentId = "AMLv2"+Codec.GetUniqueString()+"@"+System.Net.Dns.GetHostName();
        }

        /// <summary>
        /// Set a new Content-ID for the part.
        /// </summary>
        public void SetContentId(string contentID)
        {
            ContentId = contentID;
        }

        public string GetCidReference()
        {
            return "cid:" + ContentId.Trim('<','>');
        }

        /// <summary>
        /// Stores the part's BinaryContent to the specified path.
        /// </summary>
        /// <param name="destinationPath">The path the part has to be saved at.</param>
        /// <returns></returns>
        public string StoreToFile(string destinationPath)
        {
            File.WriteAllBytes(destinationPath, BinaryContent);
            return destinationPath;
        }

        /// <summary>
        /// The MIME string.
        /// </summary>
        /// <returns></returns>
        public string ToMimeString()
        {
            string content = string.Empty;

            if (this.ContentType.Type.Equals("multipart"))
                //|| this.ContentType.Type.Equals("image")
                //|| this.ContentType.Type.Equals("application"))
            {
                string boundary = string.Empty;

                // If there is no boundary, create one.
                if (this.ContentType.Parameters["boundary"] == null
                    || this.ContentType.Parameters["boundary"].Length < 1)
                {
                    string unique = Codec.GetUniqueString();
                    boundary = "---AU_MimePart_" + unique;
                    this.ContentType.Parameters.Add("boundary", boundary);
                }
                else boundary = this.ContentType.Parameters["boundary"];

                // Add the header.
                content += this.GetHeaderString();

                // Add the subparts.
                foreach (MimePart subpart in this.SubParts)
                {
                    content += "\r\n\r\n--" + boundary + "\r\n";
                    content += subpart.ToMimeString();
                }
                //content += this.TextContentTransferEncoded;

                // Close the packet.
                content += "\r\n\r\n" + "--" + boundary + "--" +"\r\n";

                return content;
            }
            
            // Modified by PMENGAL
            // All textplain is encoded with quoted-printable even if it is set to UTF7 or whatever.
            /*if (this.ContentTransferEncoding == ActiveUp.Net.Mail.ContentTransferEncoding.SevenBits)
            {
                content = this.TextContent;
            }
            else
            {
                if (this.ContentType.MimeType.ToLower().IndexOf("text/") != -1) content = Codec.ToQuotedPrintable(this.TextContent, (this.Charset != null) ? this.Charset : "us-ascii");
                else if (this.ContentType.MimeType.ToLower().IndexOf("message/") != -1) content = this.TextContent;
                else content = Codec.Wrap(System.Convert.ToBase64String(this.BinaryContent), 77);
            }*/
            content = this.TextContentTransferEncoded;
            
            return this.GetHeaderString() + "\r\n" + content;
        }
#if !PocketPC
        public static MimePart GetSignaturePart(SignedCms cms)
        {
            if (!cms.Detached) throw new ArgumentException("The CMS object is not a detached signature.");

            var part = new MimePart();

            part.ContentType.MimeType = "application/x-pkcs7-signature";
            part.ContentType.Parameters.Add("name", "\"smime.p7s\"");
            part.ContentTransferEncoding = ContentTransferEncoding.Base64;
            part.ContentDisposition.Disposition = "attachment";
            part.ContentDisposition.FileName = "smime.p7s";

            part.BinaryContent = cms.Encode();

            return part;
        }
#endif
        public string GetHeaderString()
        {
            string str = string.Empty;

            // Add the content-type. Default is text/plain.
            str += this.ContentType.ToString() + "\r\n";

            // Add the content-disposition if specified.
            if (this.ContentDisposition.Disposition.Length > 0) 
                str += this.ContentDisposition.ToString() + "\r\n";
            
            // Add other header fields.
            foreach (string key in this.HeaderFields.AllKeys)
            {
                // We already have content-type and disposition.
                if (!key.Equals("content-type") && !key.Equals("content-disposition"))
                    str += Codec.GetFieldName(key) + ": " + this.HeaderFields[key] + "\r\n";
            }
            
            return str.Trim('\r', '\n') + "\r\n";
        }

        #endregion

        #endregion

        #region Properties

        public MimePartCollection SubParts
        {
            get { return this._subEntities; }
            set { this._subEntities = value; }
        }

        private Message _parentMessage;
        public Message ParentMessage
        {
            get
            {
                return _parentMessage;
            }
            set
            {
                _parentMessage = value;
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
        /// The ContentName.
        /// </summary>
        public string ContentName
        {
            get
            {
                if (this.HeaderFields["content-name"] != null)
                    return this.HeaderFields.GetValues("content-name")[0];
                else if (this.ContentType != null && this.ContentType.Parameters["name"] != null)
                    return this.ContentType.Parameters["name"];
                else
                    return null;
            }
            set
            {
                if (this.HeaderFields["content-name"] != null) this.HeaderFields["content-name"] = value;
                else this.HeaderFields.Add("content-name", value);
            }
        }

        /// <summary>
        /// The ContentId.
        /// </summary>
        public string ContentId
        {
            get
            {
                if (this.HeaderFields["content-id"] != null) return "<" + this.HeaderFields.GetValues("content-id")[0].Trim('<', '>') + ">";
                else return null;
            }
            set
            {
                if (this.HeaderFields["content-id"] != null) this.HeaderFields["content-id"] = "<" + value.Trim('<', '>') + ">";
                else this.HeaderFields.Add("content-id","<" + value.Trim('<', '>') + ">");
            }
        }

        public string EmbeddedObjectLink
        {
            get
            {
                if (EmbeddedObjectContentId != null) return "cid:" + EmbeddedObjectContentId;
                else return null;
            }
        }

        public string EmbeddedObjectContentId
        {
            get
            {
                if (this.HeaderFields["content-id"] != null) return this.HeaderFields.GetValues("content-id")[0].Trim('<', '>');
                else return null;
            }
        }

        /// <summary>
        /// The Content-Description.
        /// </summary>
        public string ContentDescription
        {
            get
            {
                if(this.HeaderFields["content-description"]!=null) return this.HeaderFields.GetValues("content-description")[0];
                else return null;
            }
            set
            {
                if(this.HeaderFields["content-description"]!=null) this.HeaderFields["content-description"] = value;
                else this.HeaderFields.Add("content-description",value);
            }
        }

        /// <summary>
        /// The text content of a MIME Part.
        /// </summary>
        public string TextContent
        {
            get
            {
                return _textContent;
            }
            set
            {
                _textContent = value;
            }
        }

        /// <summary>
        /// The text content of a MIME Part.
        /// </summary>
        public string TextContentTransferEncoded
        {
           
            get
            {
                switch (ContentTransferEncoding)
                {
                    case ContentTransferEncoding.SevenBits:
                        return TextContent;
                    case ContentTransferEncoding.Base64:
                        return
                            Codec.Wrap(
                                BinaryContent.Length > 0
                                    ? Convert.ToBase64String(BinaryContent)
                                    : Convert.ToBase64String(
                                        System.Text.Encoding.GetEncoding(Charset).GetBytes(TextContent)), 78);
                    default:
                        if (IsText)
                            return Codec.ToQuotedPrintable(TextContent, Charset ?? "us-ascii");

                        if (MimeType.Contains("message/") || 
                            MimeType.Contains("image/") ||
                            MimeType.Contains("application/"))
                            return TextContent;

                        return Codec.Wrap(Convert.ToBase64String(BinaryContent), 77);
                }
            }
        }

        /// <summary>
        /// The multipart typed part containing this part.
        /// </summary>
        public MimePart Container
        {
            get
            {
                return _container;
            }
            set
            {
                this._container = value;
            }
        }

        /// <summary>
        /// The original content of a parsed MIME Part.
        /// </summary>
        public string OriginalContent
        {
            get
            {
                return _originalContent;
            }
            set
            {
                this._originalContent = value;
            }
        }
        
        /// <summary>
        /// The Content-Type of the MimePart.
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

        /// <summary>
        /// The Charset of the MimePart.
        /// </summary>
        public string Charset
        {
            get;
            set;
        }
        
        /// <summary>
        /// The Content-Disposition of the MimePart.
        /// </summary>
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
        /// The Content-Transfer-Encoding of the MimePart.
        /// </summary>
        public ContentTransferEncoding ContentTransferEncoding
        {
            get
            {
                if(HeaderFields["content-transfer-encoding"] == null)
                    return ContentTransferEncoding.Unknown;

                var headerValue = HeaderFields["content-transfer-encoding"].ToLower();

                switch (headerValue)
                {
                    case "quoted-printable":
                        return ContentTransferEncoding.QuotedPrintable;
                    case "base64":
                        return ContentTransferEncoding.Base64;
                    case "8bit":
                        return ContentTransferEncoding.EightBits;
                    case "7bit":
                        return ContentTransferEncoding.SevenBits;
                    case "binary":
                        return ContentTransferEncoding.Binary;
                    default:
                        return ContentTransferEncoding.Unknown;
                }
            }
            set
            {
                switch (value)
                {
                    case ContentTransferEncoding.Binary:
                        HeaderFields["content-transfer-encoding"] = "binary";
                        break;
                    case ContentTransferEncoding.QuotedPrintable:
                        HeaderFields["content-transfer-encoding"] = "quoted-printable";
                        break;
                    case ContentTransferEncoding.SevenBits:
                        HeaderFields["content-transfer-encoding"] = "7bit";
                        break;
                    case ContentTransferEncoding.EightBits:
                        HeaderFields["content-transfer-encoding"] = "8bit";
                        break;
                    default:
                        HeaderFields["content-transfer-encoding"] = "base64";
                        break;
                }
            }
        }

        /// <summary>
        /// The binary data of the part.
        /// </summary>
        public byte[] BinaryContent
        {
            get
            {
                return _binaryContent;
            }
            set
            {
                _binaryContent = value;
            }
        }

        /// <summary>
        /// The Content-Location.
        /// </summary>
        public string ContentLocation
        {
            get
            {
                if(this.HeaderFields["content-location"]!=null) return this.HeaderFields["content-location"];
                else return null;
            }
            set
            {
                this.HeaderFields["content-location"] = value;
            }
        }

        /// <summary>
        /// Approximate size of the file in octets.
        /// </summary>
        public int Size
        {
            get
            {
                if (this.BinaryContent.Length > 0) return this.BinaryContent.Length;
                else return this.TextContent.Length;
                //return this._intsize;
            }
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public string Filename
        {
            get
            {
                string filename = string.Empty;

                if (this.HeaderFields["filename"] != null)
                    filename = this.HeaderFields.GetValues("filename")[0];
                else if (this.ContentDisposition != null && this.ContentDisposition.FileName != null)
                    filename = this.ContentDisposition.FileName;
                else if (this.ContentDisposition.Parameters["filename"] != null)
                    filename = this.ContentDisposition.Parameters["filename"];
                else if (!string.IsNullOrEmpty(this.ContentName))
                    filename = this.ContentName;

                filename = filename.Replace("\"", string.Empty);

#if TRIAL
                return ProductHelper.GetTrialString(Codec.RFC2047Decode(filename), TrialStringType.ShortText);
#else
                return Codec.CleanFileName(Codec.RFC2047Decode(filename));
#endif
            }
            set
            {
                if (this.HeaderFields["filename"] != null) this.HeaderFields["filename"] = value;
                else this.AddHeaderField("filename", value);

                this.ContentDisposition.FileName = value;
            }
        }

        /// <summary>
        /// Adds the header field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        private void AddHeaderField(string name, string value)
        {
            string key = name.ToLower();

            if (this.HeaderFields[key] == null) this.HeaderFields.Add(key, value);
            else this.HeaderFields[key] = value;

            if (this.HeaderFieldNames[key] == null ) this.HeaderFieldNames.Add(key, name);
            else this.HeaderFieldNames[key] = name;
        }

        #endregion

        //public operator 
    }

    #endregion
}
