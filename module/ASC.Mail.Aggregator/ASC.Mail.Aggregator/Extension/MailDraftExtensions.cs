using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ActiveUp.Net.Mail;
using ASC.Files.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Mail.Aggregator.Managers;
using ASC.Web.Files.Services.WCFService;
using HtmlAgilityPack;

namespace ASC.Mail.Aggregator.Extension
{
    public static class MailDraftExtensions
    {
        public static MailMessage ToMailMessage(this MailDraft draft)
        {
            Address fromVerified;

            if (string.IsNullOrEmpty(draft.From))
                throw new DraftException(DraftException.ErrorTypes.EmptyField, "Empty email address in {0} field",
                    DraftFieldTypes.From);

            if (Validator.ValidateSyntax(draft.From))
                fromVerified = new Address(draft.From, draft.DisplayName);
            else
                throw new DraftException(DraftException.ErrorTypes.IncorrectField, "Incorrect email address",
                    DraftFieldTypes.From);

            if (string.IsNullOrEmpty(draft.MimeMessageId))
                throw new ArgumentException("MimeMessageId");

            var messageItem = new MailMessage
            {
                From = fromVerified.ToString(),
                FromEmail = fromVerified.Email,
                To = string.Join(", ", draft.To.ToArray()),
                Cc = draft.Cc != null ? string.Join(", ", draft.Cc.ToArray()) : "",
                Bcc = draft.Bcc != null ? string.Join(", ", draft.Bcc.ToArray()) : "",
                Subject = draft.Subject,
                Date = DateTime.UtcNow,
                Important = draft.Important,
                HtmlBody = draft.HtmlBody,
                Introduction = MailMessage.GetIntroduction(draft.HtmlBody),
                StreamId = draft.StreamId,
                TagIds = draft.Labels != null && draft.Labels.Count != 0 ? new Common.Collection.ItemList<int>(draft.Labels) : null,
                Size = draft.HtmlBody.Length,
                MimeReplyToId = draft.MimeReplyToId,
                MimeMessageId = draft.MimeMessageId,
                IsNew = false,
                Folder = MailFolder.Ids.drafts,
                ChainId = draft.MimeMessageId
            };

            if (messageItem.Attachments == null)
            {
                messageItem.Attachments = new List<MailAttachment>();
            }

            draft.Attachments.ForEach(attachment =>
            {
                attachment.tenant = draft.Mailbox.TenantId;
                attachment.user = draft.Mailbox.UserId;
            });

            messageItem.Attachments.AddRange(draft.Attachments);
            return messageItem;
        }

        public static Message ToMimeMessage(this MailDraft draft, bool loadAttachments)
        {
            var mimeMessage = new Message
            {
                Date = DateTime.UtcNow,
                From = new Address(draft.From, string.IsNullOrEmpty(draft.DisplayName) ? "" : Codec.RFC2047Encode(draft.DisplayName))
            };

            if (draft.Important)
                mimeMessage.Priority = MessagePriority.High;

            mimeMessage.To.AddRange(draft.To.ConvertAll(address =>
            {
                var addr = Parser.ParseAddress(address);
                addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                return new Address(addr.Email, addr.Name);
            }));

            mimeMessage.Cc.AddRange(draft.Cc.ConvertAll(address =>
            {
                var addr = Parser.ParseAddress(address);
                addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                return new Address(addr.Email, addr.Name);
            }));

            mimeMessage.Bcc.AddRange(draft.Bcc.ConvertAll(address =>
            {
                var addr = Parser.ParseAddress(address);
                addr.Name = string.IsNullOrEmpty(addr.Name) ? "" : Codec.RFC2047Encode(addr.Name);
                return new Address(addr.Email, addr.Name);
            }));

            mimeMessage.Subject = string.IsNullOrEmpty(draft.Subject) ? "" : Codec.RFC2047Encode(draft.Subject);

            // Set correct body
            if (draft.Attachments.Any() || draft.AttachmentsEmbedded.Any())
            {
                foreach (var attachment in draft.Attachments)
                {
                    attachment.user = draft.Mailbox.UserId;
                    attachment.tenant = draft.Mailbox.TenantId;
                    var attach = attachment.ToMimePart(loadAttachments);
                    if (attach != null)
                        mimeMessage.Attachments.Add(attach);
                }

                foreach (var embeddedAttachment in draft.AttachmentsEmbedded)
                {
                    embeddedAttachment.user = draft.Mailbox.UserId;
                    embeddedAttachment.tenant = draft.Mailbox.TenantId;
                    var attach = embeddedAttachment.ToMimePart(true);
                    if (attach != null)
                        mimeMessage.EmbeddedObjects.Add(attach);
                }
            }

            mimeMessage.MessageId = draft.MimeMessageId;
            mimeMessage.InReplyTo = draft.MimeReplyToId;

            mimeMessage.BodyText.Charset = Encoding.UTF8.HeaderName;
            mimeMessage.BodyText.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mimeMessage.BodyText.Text = "";

            mimeMessage.BodyHtml.Charset = Encoding.UTF8.HeaderName;
            mimeMessage.BodyHtml.ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable;
            mimeMessage.BodyHtml.Text = draft.HtmlBody;

            //Need for store in sent folder
            mimeMessage.OriginalData = Encoding.GetEncoding("iso-8859-1").GetBytes(mimeMessage.ToMimeString());

            return mimeMessage;
        }

        public static void ChangeAttachedFileLinksAddresses(this MailDraft draft, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var doc = new HtmlDocument();
            doc.LoadHtml(draft.HtmlBody);

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
                    log.Info("ChangeAttachedFileLinks() Change file link href: {0}", fileId);
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
                                        Share = draft.FileLinksShareMode
                                    }
                            }
                };

                fileStorageService.SetAceObject(aceCollection, false);
                log.Info("ChangeAttachedFileLinks() Set public accees to file: {0}", fileId);
                var sharedInfo =
                    fileStorageService.GetSharedInfo(new ItemList<string> { objectId })
                                      .Find(r => r.SubjectId == FileConstant.ShareLinkId);
                linkNode.SetAttributeValue("href", sharedInfo.SubjectName);
                log.Info("ChangeAttachedFileLinks() Change file link href: {0}", fileId);
                setLinks.Add(new Tuple<string, string>(fileId, sharedInfo.SubjectName));
            }

            linkNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'mailmessage-filelink')]");
            foreach (var linkNode in linkNodes)
            {
                linkNode.Attributes["class"].Remove();
            }

            draft.HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public static List<MailAttachment> ChangeEmbededAttachmentLinksForStoring(this MailDraft draft,
                                                                                  MailBoxManager manager,
                                                                                  ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            //Todo: This method can be separated in two
            var fckStorage = StorageManager.GetDataStoreForCkImages(draft.Mailbox.TenantId);
            var attachmentStorage = StorageManager.GetDataStoreForAttachments(draft.Mailbox.TenantId);

            var currentMailFckeditorUrl = fckStorage.GetUri(StorageManager.CKEDITOR_IMAGES_DOMAIN, "").ToString();
            var currentMailAttachmentFolderUrl = MailStoragePathCombiner.GetMessageDirectory(draft.Mailbox.UserId, draft.StreamId);
            var currentUserStorageUrl = MailStoragePathCombiner.GetUserMailsDirectory(draft.Mailbox.UserId);
            log.Info("ChangeEmbededAttachmentLinksForStoring() Fckeditor storage base url: {0}", currentMailFckeditorUrl);
            log.Info("ChangeEmbededAttachmentLinksForStoring() Current mail attachment folder storage base url: {0}",
                     currentMailAttachmentFolderUrl);
            log.Info("ChangeEmbededAttachmentLinksForStoring() Current user folder storage base url: {0}",
                     currentUserStorageUrl);

            var xpathQuery = StorageManager.GetXpathQueryForAttachmentsToResaving(currentMailFckeditorUrl,
                                                                                  currentMailAttachmentFolderUrl,
                                                                                  currentUserStorageUrl);
            log.Info(
                "ChangeEmbededAttachmentLinksForStoring() Xpath Query selector for embedded attachment links: {0}",
                xpathQuery);
            var attachmentsForSaving = new List<MailAttachment>();

            var doc = new HtmlDocument();
            doc.LoadHtml(draft.HtmlBody);

            var linkNodes = doc.DocumentNode.SelectNodes(xpathQuery);

            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    try
                    {
                        var link = linkNode.Attributes["src"].Value;

                        log.Info("ChangeEmbededAttachmentLinksForStoring() Original selected file_link: {0}", link);

                        var isFckImage = link.StartsWith(currentMailFckeditorUrl);
                        var prefixLength = isFckImage
                                               ? currentMailFckeditorUrl.Length
                                               : link.IndexOf(currentUserStorageUrl, StringComparison.Ordinal) +
                                                 currentUserStorageUrl.Length + 1;
                        var fileLink = HttpUtility.UrlDecode(link.Substring(prefixLength));

                        var fileName = Path.GetFileName(fileLink);
                        var attach = CreateEmbbededAttachment(fileName, link, fileLink, draft.Mailbox.UserId, draft.Mailbox.TenantId,
                                                              draft.Mailbox.MailBoxId, draft.StreamId);

                        var savedAttachmentId = manager.GetAttachmentId(draft.Id, attach.contentId);
                        var attachmentWasSaved = savedAttachmentId != 0;
                        var currentImgStorage = isFckImage ? fckStorage : attachmentStorage;
                        var domain = isFckImage ? StorageManager.CKEDITOR_IMAGES_DOMAIN : draft.Mailbox.UserId;

                        if (draft.Id == 0 || !attachmentWasSaved)
                        {
                            attach.data = StorageManager.LoadDataStoreItemData(domain, fileLink, currentImgStorage);
                            manager.StoreAttachmentWithoutQuota(draft.Mailbox.TenantId, draft.Mailbox.UserId, attach);
                            attachmentsForSaving.Add(attach);
                        }

                        if (attachmentWasSaved)
                        {
                            attach = manager.GetMessageAttachment(savedAttachmentId, draft.Mailbox.TenantId, draft.Mailbox.UserId);
                            var path = MailStoragePathCombiner.GerStoredFilePath(attach);
                            currentImgStorage = attachmentStorage;
                            attach.storedFileUrl =
                                MailStoragePathCombiner.GetStoredUrl(currentImgStorage.GetUri(path));
                        }

                        log.Info("ChangeEmbededAttachmentLinksForStoring() Restored new file_link: {0}",
                                 attach.storedFileUrl);
                        linkNode.SetAttributeValue("src", attach.storedFileUrl);
                    }
                    catch (Exception ex)
                    {
                        log.Error("ChangeEmbededAttachmentLinksForStoring() failed with exception: {0}", ex.ToString());
                    }
                }

                draft.HtmlBody = doc.DocumentNode.OuterHtml;
            }

            return attachmentsForSaving;
        }

        public static void ChangeEmbededAttachmentLinks(this MailDraft draft, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var baseAttachmentFolder = MailStoragePathCombiner.GetMessageDirectory(draft.Mailbox.UserId, draft.StreamId);

            var doc = new HtmlDocument();
            doc.LoadHtml(draft.HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseAttachmentFolder + "'))]");
            if (linkNodes == null) return;

            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.Attributes["src"].Value;
                log.Info("ChangeEmbededAttachmentLinks() Embeded attachment link for changing to cid: {0}", link);
                var fileLink = HttpUtility.UrlDecode(link.Substring(baseAttachmentFolder.Length));
                var fileName = Path.GetFileName(fileLink);

                var attach = CreateEmbbededAttachment(fileName, link, fileLink, draft.Mailbox.UserId, draft.Mailbox.TenantId, draft.Mailbox.MailBoxId, draft.StreamId);
                draft.AttachmentsEmbedded.Add(attach);
                linkNode.SetAttributeValue("src", "cid:" + attach.contentId);
                log.Info("ChangeEmbededAttachmentLinks() Attachment cid: {0}", attach.contentId);
            }
            draft.HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public static void ChangeSmileLinks(this MailDraft draft, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var baseSmileUrl = MailStoragePathCombiner.GetEditorSmileBaseUrl();

            var doc = new HtmlDocument();
            doc.LoadHtml(draft.HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseSmileUrl + "'))]");
            if (linkNodes == null) return;

            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.Attributes["src"].Value;

                log.Info("ChangeSmileLinks() Link to smile: {0}", link);

                var fileName = Path.GetFileName(link);

                var attach = new MailAttachment
                {
                    fileName = fileName,
                    storedName = fileName,
                    contentId = link.GetMd5(),
                    data = StorageManager.LoadLinkData(link)
                };

                log.Info("ChangeSmileLinks() Embedded smile contentId: {0}", attach.contentId);

                linkNode.SetAttributeValue("src", "cid:" + attach.contentId);

                if (draft.AttachmentsEmbedded.All(x => x.contentId != attach.contentId))
                {
                    draft.AttachmentsEmbedded.Add(attach);
                }
            }
            draft.HtmlBody = doc.DocumentNode.OuterHtml;
        }

        public static void ChangeAttachedFileLinksImages(this MailDraft draft, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var baseSmileUrl = MailStoragePathCombiner.GetEditorImagesBaseUrl();

            var doc = new HtmlDocument();
            doc.LoadHtml(draft.HtmlBody);
            var linkNodes = doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" + baseSmileUrl + "'))]");
            if (linkNodes == null) return;

            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.Attributes["src"].Value;
                log.Info("ChangeAttachedFileLinksImages() Link to file link: {0}", link);

                var fileName = Path.GetFileName(link);

                var attach = new MailAttachment
                {
                    fileName = fileName,
                    storedName = fileName,
                    contentId = link.GetMd5(),
                    data = StorageManager.LoadLinkData(link)
                };

                log.Info("ChangeAttachedFileLinksImages() Embedded file link contentId: {0}", attach.contentId);
                linkNode.SetAttributeValue("src", "cid:" + attach.contentId);

                if (draft.AttachmentsEmbedded.All(x => x.contentId != attach.contentId))
                {
                    draft.AttachmentsEmbedded.Add(attach);
                }
            }

            draft.HtmlBody = doc.DocumentNode.OuterHtml;
        }

        private static MailAttachment CreateEmbbededAttachment(string fileName, string link, string fileLink, string user,
                                                        int tenant, int mailboxId, string streamId)
        {
            return new MailAttachment
            {
                fileName = fileName,
                storedName = fileName,
                contentId = link.GetMd5(),
                storedFileUrl = fileLink,
                streamId = streamId,
                user = user,
                tenant = tenant,
                mailboxId = mailboxId
            };
        }
    }
}
