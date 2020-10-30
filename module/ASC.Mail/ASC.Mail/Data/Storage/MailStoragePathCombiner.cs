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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Mail.Data.Contracts;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Utility;

namespace ASC.Mail.Data.Storage
{
    public static class MailStoragePathCombiner
    {
        public const string BODY_FILE_NAME = "body.html";
        public const string EML_FILE_NAME = "message.eml";

        private static readonly Dictionary<char, string> Replacements = new Dictionary<char, string>
                {
                    {'+', "%2b"}, {'#', "%23"}, {'|', "_"}, {'<', "_"}, {'>', "_"}, {'"', "_"}, {':', "_"}, {'~', "_"}, {'?', "_"}
                };

        private const string BAD_CHARS_IN_PATH = "|<>:\"~?";

        private static string ComplexReplace(string str, string replacement)
        {
            return replacement.Aggregate(str, (current, badChar) => current.Replace(badChar.ToString(CultureInfo.InvariantCulture), Replacements[badChar]));
        }

        public static string PrepareAttachmentName(string name)
        {
            return ComplexReplace(name, BAD_CHARS_IN_PATH);
        }

        public static string GetPreSignedUri(int fileId, int tenant, string user, string stream, int fileNumber, string fileName)
        {
            //TODO: Move url to config;
            const string attachmentPath = "/addons/mail/httphandlers/download.ashx";

            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(attachmentPath));
            if (uriBuilder.Uri.IsLoopback)
            {
                uriBuilder.Host = Dns.GetHostName();
            }
            var query = uriBuilder.Query;

            query += "attachid=" + fileId + "&";
            query += "stream=" + stream + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(fileId + stream);

            var url = uriBuilder.Uri + "?" + query;

            return url;
        }

        public static string GetStoredUrl(Uri uri)
        {
            var url = uri.ToString();

            if (WorkContext.IsMono && uri.Scheme == Uri.UriSchemeFile)
            {
                if (url.StartsWith("file://"))
                    url = url.Substring(7);
            }

            return GetStoredUrl(url);
        }

        private static string GetStoredUrl(string fullUrl)
        {
            return ComplexReplace(fullUrl, "#");
        }

        public static string GetFileKey(string user, string stream, int fileNumber, string fileName)
        {
            return string.Format("{0}/{1}/attachments/{2}/{3}", user, stream, fileNumber, ComplexReplace(fileName, BAD_CHARS_IN_PATH));
        }

        public static string GetTempFileKey(string user, string stream, int fileNumber, string fileName)
        {
            return string.Format("temp/{0}/{1}/attachments/{2}/{3}", user, stream, fileNumber, ComplexReplace(fileName, BAD_CHARS_IN_PATH));
        }

        public static string GetBodyKey(string stream)
        {
            return string.Format("{0}/{1}", stream, BODY_FILE_NAME);
        }

        public static string GetBodyKey(string user, string stream)
        {
            return string.Format("{0}/{1}/{2}", user, stream, BODY_FILE_NAME);
        }

        public static string GetEmlKey(string user, string stream)
        {
            return string.Format("{0}/{1}/{2}", user, stream, EML_FILE_NAME);
        }

        public static string GerStoredFilePath(MailAttachmentData mailAttachmentData)
        {
            return GetFileKey(mailAttachmentData.user, mailAttachmentData.streamId, mailAttachmentData.fileNumber, mailAttachmentData.storedName);
        }

        public static string GetTempStoredFilePath(MailAttachmentData mailAttachmentData)
        {
            return GetTempFileKey(mailAttachmentData.user, mailAttachmentData.streamId, mailAttachmentData.fileNumber, mailAttachmentData.storedName);
        }

        public static string GetPreSignedUrl(MailAttachmentData mailAttachmentData)
        {
            return GetPreSignedUri(mailAttachmentData.fileId, mailAttachmentData.tenant, mailAttachmentData.user, mailAttachmentData.streamId, mailAttachmentData.fileNumber, mailAttachmentData.storedName);
        }

        public static string GerStoredSignatureImagePath(string user, int mailboxId, string storedName)
        {
            return String.Format("{0}/signatures/{1}/{2}", user, mailboxId, ComplexReplace(storedName, BAD_CHARS_IN_PATH));
        }

        public static string GetMessageDirectory(string user, string stream)
        {
            return String.Format("{0}/{1}", user, stream);
        }

        public static string GetUserMailsDirectory(string user)
        {
            return String.Format("{0}", user);
        }

        public static string GetEditorSmileBaseUrl()
        {
             return "/usercontrols/common/ckeditor/plugins/smiley/teamlab_images";
        }

        public static string GetEditorImagesBaseUrl()
        {
            return "/usercontrols/common/ckeditor/plugins/filetype/images";
        }

        public static string GetProxyHttpHandlerUrl()
        {
            return "/httphandlers/urlProxy.ashx";
        }
    }
}
