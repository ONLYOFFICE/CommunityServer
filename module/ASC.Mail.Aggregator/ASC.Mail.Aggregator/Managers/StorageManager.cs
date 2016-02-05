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
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using ASC.Data.Storage;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using HtmlAgilityPack;

namespace ASC.Mail.Aggregator.Managers
{
    public class StorageManager
    {
        public const string CKEDITOR_IMAGES_DOMAIN = "mail";
        
        public int Tenant { get; private set; }
        public string User { get; private set; }
        public MailBoxManager Manager { get; private set; }

        private readonly ILogger _logger;

        public StorageManager(int tenant, string user, MailBoxManager manager)
        {
            Tenant = tenant;
            User = user;
            Manager = manager;
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

        public static byte[] LoadLinkData(string link)
        {
            byte[] data;
            using (var webClient = new WebClient())
            {
                data = webClient.DownloadData(link);
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

        public string ChangeSignatureEditorImagesLinks(string signatureHtml, int mailboxId)
        {
            if (string.IsNullOrEmpty(signatureHtml) || mailboxId < 1)
                return signatureHtml;

            var newHtml = signatureHtml;

            var ckStorage = GetDataStoreForCkImages(Tenant);
            var signatureStorage = GetDataStoreForAttachments(Tenant);
            var currentMailCkeditorUrl = ckStorage.GetUri(CKEDITOR_IMAGES_DOMAIN, "").ToString();

            var xpathQuery = GetXpathQueryForCkImagesToResaving(currentMailCkeditorUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(signatureHtml);

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

                        var stableImageLink = Manager.StoreCKeditorImageWithoutQuota(Tenant, User, mailboxId, fileName, bytes,
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

        public static string GetXpathQueryForAttachmentsToResaving(string thisMailFckeditorUrl,
                                                                    string thisMailAttachmentFolderUrl,
                                                                    string thisUserStorageUrl)
        {
            const string src_condition_format = "contains(@src,'{0}')";
            var addedByUserToFck = String.Format(src_condition_format, thisMailFckeditorUrl);
            var addedToThisMail = String.Format(src_condition_format, thisMailAttachmentFolderUrl);
            var addedToThisUserMails = String.Format(src_condition_format, thisUserStorageUrl);
            var xpathQuery = String.Format("//img[@src and ({0} or {1}) and not({2})]", addedByUserToFck,
                                            addedToThisUserMails, addedToThisMail);
            return xpathQuery;
        }

        public static string GetXpathQueryForCkImagesToResaving(string thisMailCkeditorUrl)
        {
            const string src_condition_format = "contains(@src,'{0}')";
            var addedByUserToFck = String.Format(src_condition_format, thisMailCkeditorUrl);
            var xpathQuery = String.Format("//img[@src and {0}]", addedByUserToFck);
            return xpathQuery;
        }
    }
}
