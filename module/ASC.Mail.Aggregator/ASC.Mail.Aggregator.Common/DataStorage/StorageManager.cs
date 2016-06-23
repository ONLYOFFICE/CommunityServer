/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using ASC.Data.Storage;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Web.Core.Files;
using HtmlAgilityPack;

namespace ASC.Mail.Aggregator.Common.DataStorage
{
    public class StorageManager
    {
        public const string CKEDITOR_IMAGES_DOMAIN = "mail";
        
        public int Tenant { get; private set; }
        public string User { get; private set; }

        private readonly ILogger _logger;

        public StorageManager(int tenant, string user)
        {
            Tenant = tenant;
            User = user;
            _logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api");
        }

        public static IDataStore GetDataStoreForCkImages(int tenant)
        {
            return StorageFactory.GetStorage(null, tenant.ToString(CultureInfo.InvariantCulture), "fckuploaders", null);
        }

        public static IDataStore GetDataStoreForAttachments(int tenant)
        {
            return StorageFactory.GetStorage(null, tenant.ToString(CultureInfo.InvariantCulture), "mailaggregator", null);
        }

        public static byte[] LoadLinkData(string link, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var data = new byte[] {};

            try
            {
                using (var webClient = new WebClient())
                {
                    data = webClient.DownloadData(link);
                }
            }
            catch (Exception)
            {
                log.Error("LoadLinkData(url='{0}')", link);
            }

            return data;
        }

        public static byte[] LoadDataStoreItemData(string domain, string fileLink, IDataStore storage)
        {
            using (var stream = storage.GetReadStream(domain, fileLink))
            {
                return stream.ReadToEnd();
            }
        }

        public string ChangeEditorImagesLinks(string html, int mailboxId)
        {
            if (string.IsNullOrEmpty(html) || mailboxId < 1)
                return html;

            var newHtml = html;

            var ckStorage = GetDataStoreForCkImages(Tenant);
            var signatureStorage = GetDataStoreForAttachments(Tenant);
            var currentMailCkeditorUrl = ckStorage.GetUri(CKEDITOR_IMAGES_DOMAIN, "").ToString();

            var xpathQuery = GetXpathQueryForCkImagesToResaving(currentMailCkeditorUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var linkNodes = doc.DocumentNode.SelectNodes(xpathQuery);

            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    try
                    {
                        var link = linkNode.Attributes["src"].Value;

                        _logger.Info("ChangeSignatureEditorImagesLinks() Original image link: {0}", link);

                         var fileLink = HttpUtility.UrlDecode(link.Substring(currentMailCkeditorUrl.Length));

                        var fileName = Path.GetFileName(fileLink);

                        var bytes = LoadDataStoreItemData(CKEDITOR_IMAGES_DOMAIN, fileLink, ckStorage);

                        var stableImageLink = StoreCKeditorImageWithoutQuota(Tenant, User, mailboxId, fileName, bytes,
                                                               signatureStorage);

                        linkNode.SetAttributeValue("src", stableImageLink);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("ChangeSignatureEditorImagesLinks() failed with exception: {0}", ex.ToString());
                    }
                }

                newHtml = doc.DocumentNode.OuterHtml;
            }

            return newHtml;
        }

        public string StoreCKeditorImageWithoutQuota(int tenant, string user, int mailboxId, string fileName, byte[] imageData, IDataStore storage)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    throw new ArgumentNullException("imageData");

                var ext = string.IsNullOrEmpty(fileName) ? ".jpg" : Path.GetExtension(fileName);

                if (string.IsNullOrEmpty(ext))
                    ext = ".jpg";

                var storeName = imageData.GetMd5();
                storeName = Path.ChangeExtension(storeName, ext);

                var contentDisposition = ContentDispositionUtil.GetHeaderValue(storeName);
                var contentType = MimeMapping.GetMimeMapping(ext);

                var signatureImagePath = MailStoragePathCombiner.GerStoredSignatureImagePath(mailboxId, storeName);

                using (var reader = new MemoryStream(imageData))
                {
                    var uploadUrl = storage.UploadWithoutQuota(user, signatureImagePath, reader, contentType, contentDisposition);
                    return MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                }
            }
            catch (Exception e)
            {
                _logger.Error("StoreCKeditorImageWithoutQuota(). filename: {0} Exception:\r\n{1}\r\n", fileName,
                           e.ToString());

                throw;
            }
        }

        public void StoreAttachmentWithoutQuota(int tenant, string user, MailAttachment attachment)
        {
            StoreAttachmentWithoutQuota(tenant, user, attachment, MailDataStore.GetDataStore(tenant));
        }

        public void StoreAttachmentWithoutQuota(int tenant, string user, MailAttachment attachment, IDataStore storage)
        {
            try
            {
                if (attachment.data == null || attachment.data.Length == 0)
                    return;

                if (string.IsNullOrEmpty(attachment.fileName))
                    attachment.fileName = "attachment.ext";

                var contentDisposition = MailStoragePathCombiner.PrepareAttachmentName(attachment.fileName);

                var ext = Path.GetExtension(attachment.fileName);

                attachment.storedName = MailUtil.CreateStreamId();

                if (!string.IsNullOrEmpty(ext))
                    attachment.storedName = Path.ChangeExtension(attachment.storedName, ext);

                attachment.fileNumber =
                    !string.IsNullOrEmpty(attachment.contentId) //Upload hack: embedded attachment have to be saved in 0 folder
                        ? 0
                        : attachment.fileNumber;

                var attachmentPath = MailStoragePathCombiner.GerStoredFilePath(attachment);

                using (var reader = new MemoryStream(attachment.data))
                {
                    var uploadUrl = storage.UploadWithoutQuota(string.Empty, attachmentPath, reader, attachment.contentType, contentDisposition);
                    attachment.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                }
            }
            catch (Exception e)
            {
                _logger.Error("StoreAttachmentWithoutQuota(). filename: {0}, ctype: {1} Exception:\r\n{2}\r\n",
                    attachment.fileName,
                    attachment.contentType,
                    e.ToString());

                throw;
            }
        }

        public static string GetXpathQueryForAttachmentsToResaving(string thisMailFckeditorUrl,
                                                                    string thisMailAttachmentFolderUrl,
                                                                    string thisUserStorageUrl)
        {
            const string src_condition_format = "contains(@src,'{0}')";
            var addedByUserToFck = string.Format(src_condition_format, thisMailFckeditorUrl);
            var addedToThisMail = string.Format(src_condition_format, thisMailAttachmentFolderUrl);
            var addedToThisUserMails = string.Format(src_condition_format, thisUserStorageUrl);
            var xpathQuery = string.Format("//img[@src and ({0} or {1}) and not({2})]", addedByUserToFck,
                                            addedToThisUserMails, addedToThisMail);
            return xpathQuery;
        }

        public static string GetXpathQueryForCkImagesToResaving(string thisMailCkeditorUrl)
        {
            const string src_condition_format = "contains(@src,'{0}')";
            var addedByUserToFck = string.Format(src_condition_format, thisMailCkeditorUrl);
            var xpathQuery = string.Format("//img[@src and {0}]", addedByUserToFck);
            return xpathQuery;
        }
    }
}
