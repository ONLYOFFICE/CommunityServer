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
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using ASC.Data.Storage;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using HtmlAgilityPack;

namespace ASC.Api.Mail.Helpers
{
    public class StorageManager
    {
        public const string CkeditorImagesDomain = "mail";
        
        public int Tenant { get; private set; }
        public string Username { get; private set; }
        public MailBoxManager Manager { get; private set; }

        private readonly ILogger _logger;

        public StorageManager(int tenant, string username, MailBoxManager manager)
        {
            Tenant = tenant;
            Username = username;
            Manager = manager;
            _logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api");
        }

        public static IDataStore GetDataStoreForCkImages(int tenant)
        {
            return StorageFactory.GetStorage(null, tenant.ToString(CultureInfo.InvariantCulture), "fckuploaders", null, null);
        }

        public static IDataStore GetDataStoreForAttachments(int tenant)
        {
            return StorageFactory.GetStorage(null, tenant.ToString(CultureInfo.InvariantCulture), "mailaggregator", null, null);
        }

        public static byte[] LoadLinkData(string link)
        {
            byte[] data;
            using (var web_client = new WebClient())
            {
                data = web_client.DownloadData(link);
            }
            return data;
        }

        public static byte[] LoadDataStoreItemData(string domain, string file_link, IDataStore storage)
        {
            using (var stream = storage.GetReadStream(domain, file_link))
            {
                return stream.GetCorrectBuffer();
            }
        }

        public string ChangeSignatureEditorImagesLinks(string signature_html, int mailbox_id)
        {
            if (string.IsNullOrEmpty(signature_html) || mailbox_id < 1)
                return signature_html;

            var new_html = signature_html;

            var ck_storage = GetDataStoreForCkImages(Tenant);
            var signature_storage = GetDataStoreForAttachments(Tenant);
            var current_mail_ckeditor_url = ck_storage.GetUri(CkeditorImagesDomain, "").ToString();

            var xpath_query = GetXpathQueryForCkImagesToResaving(current_mail_ckeditor_url);

            var doc = new HtmlDocument();
            doc.LoadHtml(signature_html);

            var link_nodes = doc.DocumentNode.SelectNodes(xpath_query);

            if (link_nodes != null)
            {
                foreach (var link_node in link_nodes)
                {
                    try
                    {
                        var link = link_node.Attributes["src"].Value;

                        _logger.Info("ChangeSignatureEditorImagesLinks() Original image link: {0}", link);

                         var file_link = HttpUtility.UrlDecode(link.Substring(current_mail_ckeditor_url.Length));

                        var file_name = Path.GetFileName(file_link);

                        var bytes = LoadDataStoreItemData(CkeditorImagesDomain, file_link, ck_storage);

                        var stable_image_link = Manager.StoreCKeditorImageWithoutQuota(Tenant, Username, mailbox_id, file_name, bytes,
                                                               signature_storage);

                        link_node.SetAttributeValue("src", stable_image_link);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("ChangeSignatureEditorImagesLinks() failed with exception: {0}", ex.ToString());
                    }
                }

                new_html = doc.DocumentNode.OuterHtml;
            }

            return new_html;
        }

        public static string GetXpathQueryForAttachmentsToResaving(string this_mail_fckeditor_url,
                                                                    string this_mail_attachment_folder_url,
                                                                    string this_user_storage_url)
        {
            const string src_condition_format = "contains(@src,'{0}')";
            var added_by_user_to_fck = String.Format(src_condition_format, this_mail_fckeditor_url);
            var added_to_this_mail = String.Format(src_condition_format, this_mail_attachment_folder_url);
            var added_to_this_user_mails = String.Format(src_condition_format, this_user_storage_url);
            var xpath_query = String.Format("//img[@src and ({0} or {1}) and not({2})]", added_by_user_to_fck,
                                            added_to_this_user_mails, added_to_this_mail);
            return xpath_query;
        }

        public static string GetXpathQueryForCkImagesToResaving(string this_mail_ckeditor_url)
        {
            const string src_condition_format = "contains(@src,'{0}')";
            var added_by_user_to_fck = String.Format(src_condition_format, this_mail_ckeditor_url);
            var xpath_query = String.Format("//img[@src and {0}]", added_by_user_to_fck);
            return xpath_query;
        }
    }
}
