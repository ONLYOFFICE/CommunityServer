/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Mail.Aggregator.Collection;
using HtmlAgilityPack;
using System.Net.Mail;
using System.Linq;
using ASC.Mail.Aggregator;
using ActiveUp.Net.Mail;
using ASC.Mail.Service.Resources;

namespace ASC.Mail.Service.DAO
{
    [DataContract(Name = "NewMail")]
    public class MailSendItem
    {
        #region Public serialization properties
        [DataMember]
        public List<string> To { get; set; }

        [DataMember]
        public List<string> Cc { get; set; }

        [DataMember]
        public List<string> Bcc { get; set; }

        [DataMember]
        public bool Important { get; set; }

        [DataMember(IsRequired = true)]
        public string From { get; set; }

        [DataMember(IsRequired = true)]
        public string Subject { get; set; }

        [DataMember(IsRequired = true)]
        public string HtmlBody { get; set; }

        [DataMember]
        public List<MailAttachment> Attachments { get; set; }

        [DataMember]
        public string StreamId { get; set; }

        [DataMember]
        public string DisplayName { 
            get { 
                return string.IsNullOrEmpty(_displayName) ? "" : _displayName; 
            } 
            set { 
                _displayName = value; 
            } 
        }

        [DataMember]
        public int ReplyToId { get; set; }

        [DataMember]
        public List<int> Labels { get; set; }
        #endregion

        [DataContract]
        public class MailAttachment
        {
            [DataMember]
            public string Url { get; set; }

            [DataMember]
            public string FileName { get; set; }

            [DataMember]
            public int Size  { get; set; }

            [DataMember]
            public int DbId { get; set; }

            public string ContentID { get; set; }

            public MailAttachment()
            {
            }
        }




        public bool NeedReSaveAttachments { get; set; }
        
        private string _displayName = string.Empty;

        private const string EmbeddedDomain = "mail";

        /// <summary>
        /// Creates Rfc 2822 3.6.4 message-id. Syntax: '&lt;' id-left '@' id-right '&gt;'.
        /// </summary>
        /// <returns></returns>
        public static string CreateMessageID()
        {
            return "<" + Guid.NewGuid().ToString().Replace("-", "").Substring(16) + "@" +
                   Guid.NewGuid().ToString().Replace("-", "").Substring(16) + ">";
        }

        public MailMessageItem ToMailMessageItem(int tennantid, string userid, bool loadAttachments)
        {
            Address From_verified;
            if (Validator.ValidateSyntax(From))
                From_verified = new Address(From, DisplayName);
            else
                throw new ArgumentException(MailServiceResource.ResourceManager.GetString("ErrorIncorrectEmailAddress").Replace("%1", MailServiceResource.ResourceManager.GetString("FieldNameFrom")));

            List<MailAttachment> internalAttachments = new List<MailAttachment>();
            PreprocessHtml(tennantid, internalAttachments);

            MailMessageItem message_item = new MailMessageItem()
            {
                From = From_verified.ToString(),
                From_Email = From_verified.Email,
                To = string.Join(", ", To.ToArray()),
                Cc = Cc != null ? string.Join(", ", Cc.ToArray()) : "",
                Bcc = Bcc != null ? string.Join(", ", Bcc.ToArray()) : "",
                Subject = Subject,
                Date = DateTime.Now,
                Important = Important,
                HtmlBody = HtmlBody,
                StreamId = StreamId,
                TagIds = Labels != null && Labels.Count != 0 ? new ItemList<int>(Labels) : null
            };
            message_item.loadAttachments(Attachments.Select(att => CreateAttachment(tennantid, att, loadAttachments)), false);
            message_item.loadAttachments(internalAttachments.ConvertAll(att => CreateAttachment(tennantid, att, true)), true);

            return message_item;
        }

        public Message CreateMimeMessage(int tennantid, string userid, bool loadAttachments)
        {
            List<MailAttachment> internalAttachments = new List<MailAttachment>();

            PreprocessHtml(tennantid, internalAttachments);

            var mailMessage = new Message()
            {
                Date = DateTime.Now,
                Priority = this.Important ? ActiveUp.Net.Mail.MessagePriority.High : ActiveUp.Net.Mail.MessagePriority.Normal
            };

            mailMessage.From = new Address(From, string.IsNullOrEmpty(DisplayName) ? "" : Codec.RFC2047Encode(DisplayName));

            mailMessage.To.AddRange(To.ConvertAll(address => { 
                var addr = Parser.ParseAddress(address);
                addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                return new Address(addr.Email, addr.Name);
            }));
            
            mailMessage.Cc.AddRange(Cc.ConvertAll(address =>
            {
                var addr = Parser.ParseAddress(address);
                addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                return new Address(addr.Email, addr.Name);
            }));
            
            mailMessage.Bcc.AddRange(Bcc.ConvertAll(address =>
            {
                var addr = Parser.ParseAddress(address);
                addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                return new Address(addr.Email, addr.Name);
            }));

            mailMessage.Subject = Codec.RFC2047Encode(Subject); 

            // Set correct body
            if (Attachments.Count != 0 || internalAttachments.Count != 0)
            {
                foreach (MailAttachment attachment in Attachments)
                {
                    var attachPath = attachment;

                    var attach = CreateAttachment(tennantid, attachment, loadAttachments);

                    if (attach != null)
                    {
                        mailMessage.Attachments.Add(attach);
                    }

                }

                foreach (MailAttachment internalAttachment in internalAttachments)
                {
                    var attach = CreateAttachment(tennantid, internalAttachment, true);

                    if (attach != null)
                        mailMessage.EmbeddedObjects.Add(attach);
                }
            }

            mailMessage.BodyText.Charset = Encoding.UTF8.HeaderName;
            mailMessage.BodyText.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mailMessage.BodyText.Text = "";

            mailMessage.BodyHtml.Charset = Encoding.UTF8.HeaderName;
            mailMessage.BodyHtml.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mailMessage.BodyHtml.Text = HtmlBody;

            return mailMessage;
        }

        private void PreprocessHtml(int tennantid, List<MailAttachment> internalAttachments)
        {
            //Parse html attachments
            var storage = GetDataStoreForEmbeddedAttachments(tennantid);
            string baseValue = storage.GetUri(EmbeddedDomain, "").ToString();
            //Load html doc
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseValue + "'))]");
            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    var link = linkNode.Attributes["src"].Value;
                    var file_link = link.Substring(baseValue.Length);

                    //Change
                    var attach = new MailAttachment()
                    {
                        Url = EmbeddedDomain + "|" + file_link,
                        ContentID = GenerateContentId()
                    };
                    internalAttachments.Add(attach);
                    linkNode.SetAttributeValue("src", "cid:" + attach.ContentID);
                }
            }
            HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public static string GenerateContentId()
        {
            return string.Format("{0}@{1}",
                                 Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(16),
                                 Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(16));
        }

        private static MimePart CreateAttachment(int tennantid, MailAttachment attachment, bool loadAttachments)
        {
            ActiveUp.Net.Mail.MimePart retVal = new ActiveUp.Net.Mail.MimePart();
            var fileName = Path.GetFileName(HttpUtility.UrlDecode(attachment.Url.Split('|')[1]));

            if (loadAttachments)
            {
                var pathInfo = attachment.Url.Split('|');

                var is_embedded = !string.IsNullOrEmpty(attachment.ContentID);

                using (var s = is_embedded ?
                    GetDataStoreForEmbeddedAttachments(tennantid).GetReadStream(EmbeddedDomain, pathInfo[1]) :
                    GetDataStoreForAttachments(tennantid, pathInfo[0]).GetReadStream(HttpUtility.UrlDecode(pathInfo[1])))
                {
                    retVal = new MimePart(s.GetCorrectBuffer(), fileName);
                    retVal.ContentDisposition.Disposition = "attachment";
                }
                if (attachment.ContentID != null) retVal.ContentId = attachment.ContentID;
            }
            else
            {
                var conentType = ASC.Common.Web.MimeMapping.GetMimeMapping(attachment.Url);
                retVal.ContentType = new ActiveUp.Net.Mail.ContentType() { Type = conentType };
                retVal.Filename = Path.GetFileName(HttpUtility.UrlDecode(fileName));
                if (attachment.ContentID != null) retVal.ContentId = attachment.ContentID;
                retVal.TextContent = "";
            }

            return retVal;
        }

        private static IDataStore GetDataStoreForEmbeddedAttachments(int tennantId)
        {
            return StorageFactory.GetStorage(null, tennantId.ToString(), "fckuploaders", null, null);
        }

        private static IDataStore GetDataStoreForAttachments(int tennantId, string domain)
        {
            return StorageFactory.GetStorage(null, tennantId.ToString(), domain, null, null);
        }

        public void Validate()
        {
            ValidateAddresses(MailServiceResource.FieldNameFrom, new string[1] { From }, true);
            ValidateAddresses(MailServiceResource.FieldNameTo, To, true);
            ValidateAddresses(MailServiceResource.FieldNameCc, Cc, false);
            ValidateAddresses(MailServiceResource.FieldNameBcc, Bcc, false);

            if (string.IsNullOrEmpty(StreamId)) throw new ArgumentException("no streamId");

        }

        private void ValidateAddresses(string fieldName, IEnumerable<string> addresses, bool strongValidation)
        {
            bool invalid_email_found = false;
            if (addresses != null)
            {
                foreach (string addr in addresses)
                {
                    if (!Validator.ValidateSyntax(addr)) invalid_email_found = true;
                }
                if (invalid_email_found)
                    throw new ArgumentException(MailServiceResource.ErrorIncorrectEmailAddress.Replace("%1", fieldName));
            }
            else if (strongValidation)
                throw new ArgumentException(MailServiceResource.ErrorEmptyField.Replace("%1", fieldName));
        }

    }
}