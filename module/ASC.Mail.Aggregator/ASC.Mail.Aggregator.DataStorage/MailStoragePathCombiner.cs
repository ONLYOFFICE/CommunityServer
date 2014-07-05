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
using System.Linq;
using System.Net;
using ASC.Data.Storage;
using ASC.Data.Storage.S3;
using ASC.Mail.Aggregator.Common;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Utility;

namespace ASC.Mail.Aggregator.DataStorage
{
    public static class MailStoragePathCombiner
    {
        private static readonly Dictionary<char, string> Replacements = new Dictionary<char, string>
                {
                    {'+', "%2b"}, {'#', "%23"}, {'|', "_"}, {'<', "_"}, {'>', "_"}, {'"', "_"}, {':', "_"}, {'~', "_"}, {'?', "_"}
                };

        private const string BadCharsInPath = "|<>:\"~?";

        private static string ComplexReplace(string str, string replacement)
        {
            return replacement.Aggregate(str, (current, bad_char) => current.Replace(bad_char.ToString(CultureInfo.InvariantCulture), Replacements[bad_char]));
        }

        public static string PrepareAttachmentName(string name)
        {
            return ComplexReplace(name, BadCharsInPath);
        }

        public static string GetPreSignedUri(int file_id, int id_tenant, string id_user, string stream, int file_number,
                                          string file_name, IDataStore data_store)
        {
            var attachment_path = GetFileKey(id_user, stream, file_number, file_name);

            if (data_store == null)
                data_store = MailDataStore.GetDataStore(id_tenant);

            string url;

            if (data_store is S3Storage)
            {
                var content_disposition_file_name = ContentDispositionUtil.GetHeaderValue(file_name, withoutBase: true);
                var headers_for_url = new []{"Content-Disposition:" + content_disposition_file_name};
                url = data_store.GetPreSignedUri("", attachment_path, TimeSpan.FromMinutes(10), headers_for_url).ToString();
            }
            else
            {
                //TODO: Move url to config;
                attachment_path = "/addons/mail/httphandlers/download.ashx";

                var uri_builder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(attachment_path));
                if (uri_builder.Uri.IsLoopback)
                {
                    uri_builder.Host = Dns.GetHostName();
                }
                var query = uri_builder.Query;

                query += "attachid=" + file_id + "&";
                query += "stream=" + stream + "&";
                query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file_id + stream);

                url = uri_builder.Uri + "?" + query;
            }

            return url;
        }

        public static string GetStoredUrl(Uri uri)
        {
            return GetStoredUrl(!uri.IsAbsoluteUri ? CommonLinkUtility.GetFullAbsolutePath(uri.ToString()) : uri.ToString());
        }

        private static string GetStoredUrl(string full_url)
        {
            return ComplexReplace(full_url, "#");
        }

        public static string GetFileKey(string id_user, string stream, int file_number, string file_name)
        {
            return String.Format("{0}/{1}/attachments/{2}/{3}", id_user, stream, file_number, ComplexReplace(file_name, BadCharsInPath));
        }

        public static string GetBodyKey(string stream)
        {
            return String.Format("{0}/body.html", stream);
        }

        public static string GetBodyKey(string id_user, string stream)
        {
            return String.Format("{0}/{1}/body.html", id_user, stream);
        }

        public static string GerStoredFilePath(MailAttachment attachment)
        {
            return GetFileKey(attachment.user, attachment.streamId, attachment.fileNumber, attachment.storedName);
        }

        public static string GetPreSignedUrl(MailAttachment attachment, IDataStore data_store = null)
        {
            return GetPreSignedUri(attachment.fileId, attachment.tenant, attachment.user, attachment.streamId, attachment.fileNumber, attachment.storedName, data_store);
        }

        public static string GerStoredSignatureImagePath(int id_mailbox, string stored_name)
        {
            return String.Format("signatures/{0}/{1}", id_mailbox, ComplexReplace(stored_name, BadCharsInPath));
        }
    }
}
