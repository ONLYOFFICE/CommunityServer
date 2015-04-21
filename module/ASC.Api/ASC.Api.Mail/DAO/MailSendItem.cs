/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Api.Mail.Helpers;
using ASC.Files.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Web.Files.Services.WCFService;
using HtmlAgilityPack;
using ASC.Mail.Aggregator;
using ActiveUp.Net.Mail;
using ASC.Api.Mail.Resources;
using FileShare = ASC.Files.Core.Security.FileShare;

namespace ASC.Api.Mail.DAO
{
    internal class MailSendItem
    {
        private readonly ILogger _logger;

        public MailSendItem()
        {
            Attachments = new List<MailAttachment>();
            AttachmentsEmbedded = new List<MailAttachment>();
            To = new List<string>();
            Cc = new List<string>();
            Bcc = new List<string>();
            Labels = new List<int>();
            _logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api");
        }

        public int MailboxId { get; set; }

        public List<string> To { get; set; }

        public List<string> Cc { get; set; }

        public List<string> Bcc { get; set; }

        public bool Important { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public List<MailAttachment> Attachments { get; set; }

        public List<MailAttachment> AttachmentsEmbedded { get; set; }

        public string StreamId { get; set; }

        public string DisplayName
        {
            get { return string.IsNullOrEmpty(_displayName) ? "" : _displayName; }
            set { _displayName = value; }
        }

        public int ReplyToId { get; set; }

        public List<int> Labels { get; set; }

        public string MimeMessageId { get; set; }

        public string MimeReplyToId { get; set; }

        public FileShare FileLinksShareMode { get; set; }

        private string _displayName = string.Empty;

        public MailMessageItem ToMailMessageItem(int tenant, string user)
        {
            Address fromVerified;
            if (Validator.ValidateSyntax(From))
                fromVerified = new Address(From, DisplayName);
            else
                throw new ArgumentException(MailApiResource.ErrorIncorrectEmailAddress
                                                           .Replace("%1", MailApiResource.FieldNameFrom));

            var messageItem = new MailMessageItem
                {
                    From = fromVerified.ToString(),
                    FromEmail = fromVerified.Email,
                    To = string.Join(", ", To.ToArray()),
                    Cc = Cc != null ? string.Join(", ", Cc.ToArray()) : "",
                    Bcc = Bcc != null ? string.Join(", ", Bcc.ToArray()) : "",
                    Subject = Subject,
                    Date = DateTime.Now,
                    Important = Important,
                    HtmlBody = HtmlBody,
                    Introduction = MailMessageItem.GetIntroduction(HtmlBody),
                    StreamId = StreamId,
                    TagIds = Labels != null && Labels.Count != 0 ? new ASC.Mail.Aggregator.Common.Collection.ItemList<int>(Labels) : null,
                    Size = HtmlBody.Length,
                    MimeReplyToId = MimeReplyToId,
                    MimeMessageId = string.IsNullOrEmpty(MimeMessageId) ? MailBoxManager.CreateMessageId() : MimeMessageId
                };

            if (messageItem.Attachments == null)
            {
                messageItem.Attachments = new List<MailAttachment>();
            }

            Attachments.ForEach(attachment =>
                {
                    attachment.tenant = tenant;
                    attachment.user = user;
                });

            messageItem.Attachments.AddRange(Attachments);
            return messageItem;
        }

        public Message ToMimeMessage(int tenant, string user, bool loadAttachments)
        {
            var mimeMessage = new Message
                {
                    Date = DateTime.UtcNow,
                    From = new Address(From, string.IsNullOrEmpty(DisplayName) ? "" : Codec.RFC2047Encode(DisplayName))
                };

            if (Important)
                mimeMessage.Priority = MessagePriority.High;

            mimeMessage.To.AddRange(To.ConvertAll(address =>
                {
                    var addr = Parser.ParseAddress(address);
                    addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                    return new Address(addr.Email, addr.Name);
                }));

            mimeMessage.Cc.AddRange(Cc.ConvertAll(address =>
                {
                    var addr = Parser.ParseAddress(address);
                    addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                    return new Address(addr.Email, addr.Name);
                }));

            mimeMessage.Bcc.AddRange(Bcc.ConvertAll(address =>
                {
                    var addr = Parser.ParseAddress(address);
                    addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                    return new Address(addr.Email, addr.Name);
                }));

            mimeMessage.Subject = Codec.RFC2047Encode(Subject);

            // Set correct body
            if (Attachments.Any() || AttachmentsEmbedded.Any())
            {
                foreach (var attachment in Attachments)
                {
                    attachment.user = user;
                    attachment.tenant = tenant;
                    var attach = CreateAttachment(attachment, loadAttachments);
                    if (attach != null)
                        mimeMessage.Attachments.Add(attach);
                }

                foreach (var embeddedAttachment in AttachmentsEmbedded)
                {
                    embeddedAttachment.user = user;
                    embeddedAttachment.tenant = tenant;
                    var attach = CreateAttachment(embeddedAttachment, true);
                    if (attach != null)
                        mimeMessage.EmbeddedObjects.Add(attach);
                }
            }

            mimeMessage.MessageId = MimeMessageId;
            mimeMessage.InReplyTo = MimeReplyToId;

            mimeMessage.BodyText.Charset = Encoding.UTF8.HeaderName;
            mimeMessage.BodyText.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mimeMessage.BodyText.Text = "";

            mimeMessage.BodyHtml.Charset = Encoding.UTF8.HeaderName;
            mimeMessage.BodyHtml.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mimeMessage.BodyHtml.Text = HtmlBody;

            mimeMessage.OriginalData = Encoding.GetEncoding("iso-8859-1").GetBytes(mimeMessage.ToMimeString());

            return mimeMessage;
        }

        public List<MailAttachment> ChangeEmbededAttachmentLinksForStoring(int tenant, string user, int mailId, MailBoxManager manager)
        {
            //Todo: This method can be separated in two
            var fckStorage = StorageManager.GetDataStoreForCkImages(tenant);
            var attachmentStorage = StorageManager.GetDataStoreForAttachments(tenant);

            var currentMailFckeditorUrl = fckStorage.GetUri(StorageManager.CKEDITOR_IMAGES_DOMAIN, "").ToString();
            var currentMailAttachmentFolderUrl = MailStoragePathCombiner.GetMessageDirectory(user, StreamId);
            var currentUserStorageUrl = MailStoragePathCombiner.GetUserMailsDirectory(user);
            _logger.Info("ChangeEmbededAttachmentLinksForStoring() Fckeditor storage base url: {0}", currentMailFckeditorUrl);
            _logger.Info("ChangeEmbededAttachmentLinksForStoring() Current mail attachment folder storage base url: {0}", currentMailAttachmentFolderUrl);
            _logger.Info("ChangeEmbededAttachmentLinksForStoring() Current user folder storage base url: {0}", currentUserStorageUrl);

            var xpathQuery = StorageManager.GetXpathQueryForAttachmentsToResaving(currentMailFckeditorUrl, currentMailAttachmentFolderUrl, currentUserStorageUrl);
            _logger.Info("ChangeEmbededAttachmentLinksForStoring() Xpath Query selector for embedded attachment links: {0}", xpathQuery);
            var attachmentsForSaving = new List<MailAttachment>();

            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlBody);

            var linkNodes = doc.DocumentNode.SelectNodes(xpathQuery);

            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    try
                    {
                        var link = linkNode.Attributes["src"].Value;

                        _logger.Info("ChangeEmbededAttachmentLinksForStoring() Original selected file_link: {0}", link);

                        var isFckImage = link.StartsWith(currentMailFckeditorUrl);
                        var prefixLength = isFckImage
                                                ? currentMailFckeditorUrl.Length
                                                : link.IndexOf(currentUserStorageUrl, StringComparison.Ordinal) +
                                                  currentUserStorageUrl.Length + 1;
                        var fileLink = HttpUtility.UrlDecode(link.Substring(prefixLength));

                        var fileName = Path.GetFileName(fileLink);
                        var attach = CreateEmbbededAttachment(fileName, link, fileLink, user, tenant, MailboxId);

                        var savedAttachmentId = manager.GetAttachmentId(mailId, attach.contentId);
                        var attachmentWasSaved = savedAttachmentId != 0;
                        var currentImgStorage = isFckImage ? fckStorage : attachmentStorage;
                        var domain = isFckImage ? StorageManager.CKEDITOR_IMAGES_DOMAIN : user;

                        if (mailId == 0 || !attachmentWasSaved)
                        {
                            attach.data = StorageManager.LoadDataStoreItemData(domain, fileLink, currentImgStorage);
                            manager.StoreAttachmentWithoutQuota(tenant, user, attach);
                            attachmentsForSaving.Add(attach);
                        }

                        if (attachmentWasSaved)
                        {
                            attach = manager.GetMessageAttachment(savedAttachmentId, tenant, user);
                            var path = MailStoragePathCombiner.GerStoredFilePath(attach);
                            currentImgStorage = attachmentStorage;
                            attach.storedFileUrl =
                                MailStoragePathCombiner.GetStoredUrl(currentImgStorage.GetUri(path));
                        }

                        _logger.Info("ChangeEmbededAttachmentLinksForStoring() Restored new file_link: {0}",
                                     attach.storedFileUrl);
                        linkNode.SetAttributeValue("src", attach.storedFileUrl);
                    }
                    catch(Exception ex)
                    {
                        _logger.Error("ChangeEmbededAttachmentLinksForStoring() failed with exception: {0}", ex.ToString());
                    }
                }

                HtmlBody = doc.DocumentNode.OuterHtml;
            }

            return attachmentsForSaving;
        }


        private MailAttachment CreateEmbbededAttachment(string fileName, string link, string fileLink, string user, int tenant, int mailboxId)
        {
            return new MailAttachment
                {
                    fileName = fileName,
                    storedName = fileName,
                    contentId = link.GetMd5(),
                    storedFileUrl = fileLink,
                    streamId = StreamId,
                    user = user,
                    tenant = tenant,
                    mailboxId = mailboxId
                };
        }

        public void ChangeEmbededAttachmentLinks(int tenant, string user)
        {
            var baseAttachmentFolder = MailStoragePathCombiner.GetMessageDirectory(user, StreamId);

            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseAttachmentFolder + "'))]");
            if (linkNodes == null) return;

            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.Attributes["src"].Value;
                _logger.Info("ChangeEmbededAttachmentLinks() Embeded attachment link for changing to cid: {0}", link);
                var fileLink = HttpUtility.UrlDecode(link.Substring(baseAttachmentFolder.Length));
                var fileName = Path.GetFileName(fileLink);

                var attach = CreateEmbbededAttachment(fileName, link, fileLink, user, tenant, MailboxId);
                AttachmentsEmbedded.Add(attach);
                linkNode.SetAttributeValue("src", "cid:" + attach.contentId);
                _logger.Info("ChangeEmbededAttachmentLinks() Attachment cid: {0}", attach.contentId);
            }
            HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public void ChangeSmileLinks()
        {
            var baseSmileUrl = SmileToAttachmentConvertor.SmileBaseUrl;

            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseSmileUrl + "'))]");
            if (linkNodes == null) return;

            var smileConvertor = new SmileToAttachmentConvertor();
            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.Attributes["src"].Value;
                _logger.Info("ChangeSmileLinks() Link to smile: {0}", link);
                var attach = smileConvertor.ToMailAttachment(link);
                _logger.Info("ChangeSmileLinks() Embedded smile contentId: {0}", attach.contentId);
                linkNode.SetAttributeValue("src", "cid:" + attach.contentId);

                if (AttachmentsEmbedded.All(x => x.contentId != attach.contentId))
                {
                    AttachmentsEmbedded.Add(attach);
                }
            }
            HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public void ChangeAttachedFileLinksImages()
        {
            var baseSmileUrl = FileLinksToAttachmentConvertor.BaseUrl;

            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseSmileUrl + "'))]");
            if (linkNodes == null) return;

            var fileLinksConvertor = new FileLinksToAttachmentConvertor();
            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.Attributes["src"].Value;
                _logger.Info("ChangeAttachedFileLinksImages() Link to file link: {0}", link);
                var attach = fileLinksConvertor.ToMailAttachment(link);
                _logger.Info("ChangeAttachedFileLinksImages() Embedded file link contentId: {0}", attach.contentId);
                linkNode.SetAttributeValue("src", "cid:" + attach.contentId);

                if (AttachmentsEmbedded.All(x => x.contentId != attach.contentId))
                {
                    AttachmentsEmbedded.Add(attach);
                }
            }
            HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public void ChangeAttachedFileLinksAddresses(int tenantId)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlBody);

            var linkNodes = doc.DocumentNode.SelectNodes("//a[contains(@class,'mailmessage-filelink-link')]");
            if (linkNodes == null) return;

            var fileStorageService = new FileStorageServiceController();

            var setLinks = new List<Tuple<string, string>>();
            foreach (var linkNode in linkNodes)
            {
                var fileId = linkNode.Attributes["data-fileid"].Value;
                var objectId = "file_" + fileId;

                linkNode.Attributes["class"].Remove(); // 'mailmessage-filelink-link'
                linkNode.Attributes["data-fileid"].Remove(); // 'data-fileid'

                var setLink = setLinks.SingleOrDefault(x => x.Item1 == fileId);
                if (setLink != null)
                {
                    linkNode.SetAttributeValue("href", setLink.Item2);
                    _logger.Info("ChangeAttachedFileLinks() Change file link href: {0}", fileId);
                    continue;
                }

                var aceCollection = new AceCollection
                    {
                        Entries = new ItemList<string> { objectId },
                        Aces = new ItemList<AceWrapper>
                            {
                                new AceWrapper
                                    {
                                        SubjectId = FileConstant.ShareLinkId,
                                        SubjectGroup = true,
                                        Share = FileLinksShareMode
                                    }
                            }
                    };

                fileStorageService.SetAceObject(aceCollection, false);
                _logger.Info("ChangeAttachedFileLinks() Set public accees to file: {0}", fileId);
                var sharedInfo = fileStorageService.GetSharedInfo(new ItemList<string> { objectId }).Find(r => r.SubjectId == FileConstant.ShareLinkId);
                linkNode.SetAttributeValue("href", sharedInfo.SubjectName);
                _logger.Info("ChangeAttachedFileLinks() Change file link href: {0}", fileId);
                setLinks.Add(new Tuple<string, string>(fileId, sharedInfo.SubjectName));
            }

            linkNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'mailmessage-filelink')]");
            foreach (var linkNode in linkNodes)
            {
                linkNode.Attributes["class"].Remove();
            }

            HtmlBody = doc.DocumentNode.OuterHtml;
        }

        private static MimePart CreateAttachment(MailAttachment attachment, bool loadAttachments)
        {
            var retVal = new MimePart();

            var s3Key = MailStoragePathCombiner.GerStoredFilePath(attachment);
            var fileName = attachment.fileName ?? Path.GetFileName(s3Key);

            if (loadAttachments)
            {
                var byteArray = attachment.data;

                if (byteArray == null || byteArray.Length == 0)
                {
                    using (var stream = StorageManager.GetDataStoreForAttachments(attachment.tenant).GetReadStream(s3Key))
                    {
                        byteArray = stream.GetCorrectBuffer();
                    }
                }

                retVal = new MimePart(byteArray, fileName);

                if (attachment.contentId != null) retVal.ContentId = attachment.contentId;
            }
            else
            {
                var conentType = Common.Web.MimeMapping.GetMimeMapping(s3Key);
                retVal.ContentType = new ContentType {Type = conentType};
                retVal.Filename = fileName;
                if (attachment.contentId != null) retVal.ContentId = attachment.contentId;
                retVal.TextContent = "";
            }

            return retVal;
        }


        public void Validate()
        {
            ValidateAddresses(MailApiResource.FieldNameFrom, new[] {From}, true);
            ValidateAddresses(MailApiResource.FieldNameTo, To, true);
            ValidateAddresses(MailApiResource.FieldNameCc, Cc, false);
            ValidateAddresses(MailApiResource.FieldNameBcc, Bcc, false);

            if (string.IsNullOrEmpty(StreamId))
                throw new ArgumentException("no streamId");
        }

        private static void ValidateAddresses(string fieldName, IEnumerable<string> addresses, bool strongValidation)
        {
            var invalidEmailFound = false;
            if (addresses != null)
            {
                if (addresses.Any(addr => !Validator.ValidateSyntax(addr)))
                    invalidEmailFound = true;

                if (invalidEmailFound)
                    throw new ArgumentException(MailApiResource.ErrorIncorrectEmailAddress.Replace("%1", fieldName));
            }
            else if (strongValidation)
                throw new ArgumentException(MailApiResource.ErrorEmptyField.Replace("%1", fieldName));
        }
    }
}