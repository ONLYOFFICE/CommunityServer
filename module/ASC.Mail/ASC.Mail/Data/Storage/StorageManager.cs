/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using ASC.Common.Logging;
using ASC.Data.Storage;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using ASC.Web.Core.Files;
using HtmlAgilityPack;

namespace ASC.Mail.Data.Storage
{
    public class StorageManager
    {
        public const string CKEDITOR_IMAGES_DOMAIN = "mail";
        
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public StorageManager(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.StorageManager");
        }

        public static IDataStore GetDataStoreForCkImages(int tenant)
        {
            return StorageFactory.GetStorage(null, tenant.ToString(CultureInfo.InvariantCulture), "fckuploaders", null);
        }

        public static IDataStore GetDataStoreForAttachments(int tenant)
        {
            return StorageFactory.GetStorage(null, tenant.ToString(CultureInfo.InvariantCulture), "mailaggregator", null);
        }

        public static byte[] LoadLinkData(string link, ILog log = null)
        {
            if (log == null)
                log = new NullLog();

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
                log.ErrorFormat("LoadLinkData(url='{0}')", link);
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
            //todo: replace selector
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

                        Log.InfoFormat("ChangeSignatureEditorImagesLinks() Original image link: {0}", link);

                         var fileLink = HttpUtility.UrlDecode(link.Substring(currentMailCkeditorUrl.Length));

                        var fileName = Path.GetFileName(fileLink);

                        var bytes = LoadDataStoreItemData(CKEDITOR_IMAGES_DOMAIN, fileLink, ckStorage);

                        var stableImageLink = StoreCKeditorImageWithoutQuota(mailboxId, fileName, bytes,
                                                               signatureStorage);

                        linkNode.SetAttributeValue("src", stableImageLink);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("ChangeSignatureEditorImagesLinks() failed with exception: {0}", ex.ToString());
                    }
                }

                newHtml = doc.DocumentNode.OuterHtml;
            }

            return newHtml;
        }

        public string StoreCKeditorImageWithoutQuota(int mailboxId, string fileName, byte[] imageData, IDataStore storage)
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

                var signatureImagePath = MailStoragePathCombiner.GerStoredSignatureImagePath(User, mailboxId, storeName);

                using (var reader = new MemoryStream(imageData))
                {
                    var uploadUrl = storage.Save(string.Empty, signatureImagePath, reader, contentType, contentDisposition);
                    return MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("StoreCKeditorImageWithoutQuota(). filename: {0} Exception:\r\n{1}\r\n", fileName,
                           e.ToString());

                throw;
            }
        }

        public void StoreAttachmentWithoutQuota(MailAttachmentData mailAttachmentData)
        {
            try
            {
                if ((mailAttachmentData.dataStream == null || mailAttachmentData.dataStream.Length == 0) && (mailAttachmentData.data == null || mailAttachmentData.data.Length == 0))
                    return;

                if (string.IsNullOrEmpty(mailAttachmentData.fileName))
                    mailAttachmentData.fileName = "attachment.ext";

                var storage = MailDataStore.GetDataStore(Tenant);

                storage.QuotaController = null;

                if (string.IsNullOrEmpty(mailAttachmentData.storedName))
                {
                    mailAttachmentData.storedName = MailUtil.CreateStreamId();

                    var ext = Path.GetExtension(mailAttachmentData.fileName);

                    if (!string.IsNullOrEmpty(ext))
                        mailAttachmentData.storedName = Path.ChangeExtension(mailAttachmentData.storedName, ext);
                }

                mailAttachmentData.fileNumber =
                    !string.IsNullOrEmpty(mailAttachmentData.contentId) //Upload hack: embedded attachment have to be saved in 0 folder
                        ? 0
                        : mailAttachmentData.fileNumber;

                var attachmentPath = MailStoragePathCombiner.GerStoredFilePath(mailAttachmentData);

                if (mailAttachmentData.data != null)
                {
                    using (var reader = new MemoryStream(mailAttachmentData.data))
                    {
                        var uploadUrl = (mailAttachmentData.needSaveToTemp)
                            ? storage.Save("attachments_temp", attachmentPath, reader, mailAttachmentData.fileName)
                            : storage.Save(attachmentPath, reader, mailAttachmentData.fileName);

                        mailAttachmentData.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                    }
                }
                else
                {
                    var uploadUrl = (mailAttachmentData.needSaveToTemp)
                        ? storage.Save("attachments_temp", attachmentPath, mailAttachmentData.dataStream, mailAttachmentData.fileName)
                        : storage.Save(attachmentPath, mailAttachmentData.dataStream, mailAttachmentData.fileName);

                    mailAttachmentData.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                }

                if (mailAttachmentData.needSaveToTemp)
                {
                    mailAttachmentData.tempStoredUrl = mailAttachmentData.storedFileUrl;
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("StoreAttachmentWithoutQuota(). filename: {0}, ctype: {1} Exception:\r\n{2}\r\n",
                    mailAttachmentData.fileName,
                    mailAttachmentData.contentType,
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
