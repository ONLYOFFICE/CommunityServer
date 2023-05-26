/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Web;
using ASC.Common.Utils;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json.Linq;

using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public static class FileShareLink
    {
        private const string SESSION_ID_KEY = "sessionId";
        private const string LINK_ID_KEY = "linkId";
        private const string PASSWORD_KEY = "passwordKey";
        
        #region Common

        public static string CreateKey(string fileId, Guid shareLinkId = default(Guid), string shareLinkPassword = default(string))
        {
            if (shareLinkId != FileConstant.ShareLinkId && shareLinkId != Guid.Empty)
            {
                var jobject = new JObject(
                    new JProperty("entry", fileId),
                    new JProperty("link", shareLinkId)
                );

                if (!string.IsNullOrEmpty(shareLinkPassword))
                {
                    jobject.Add("password", shareLinkPassword);
                }

                fileId = jobject.ToString(Newtonsoft.Json.Formatting.None);
            }

            return Signature.Create(fileId, Global.GetDocDbKey());
        }

        public static string Parse(string doc, out Guid shareLinkId, out string shareLinkPassword)
        {
            shareLinkId = FileConstant.ShareLinkId;
            shareLinkPassword = null;

            var value = Signature.Read<string>(doc ?? string.Empty, Global.GetDocDbKey());

            if (!string.IsNullOrEmpty(value) && value.StartsWith("{") && value.EndsWith("}"))
            {
                try
                {
                    var jobject = JObject.Parse(value);
                    value = jobject.Value<string>("entry");
                    var linkId = jobject.Value<string>("link");
                    shareLinkId = new Guid(linkId);
                    shareLinkPassword = jobject.Value<string>("password");
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("FileShareLink: Cant parse json: " + value, ex);
                    return null;
                }
            }

            return value;
        }

        public static bool Check(string doc, bool checkRead, IFileDao fileDao, out File file, out FileShare fileShare, out Guid linkId)
        {
            fileShare = Check(doc, fileDao, out file, out linkId);
            return (!checkRead
                    && (fileShare == FileShare.ReadWrite
                        || fileShare == FileShare.CustomFilter
                        || fileShare == FileShare.Review
                        || fileShare == FileShare.FillForms
                        || fileShare == FileShare.Comment))
                || (checkRead && fileShare != FileShare.Restrict);
        }

        public static FileShare Check(string doc, IFileDao fileDao, out File file, out Guid linkId)
        {
            file = null;
            linkId = FileConstant.ShareLinkId;

            if (!FilesSettings.ExternalShare) return FileShare.Restrict;
            if (string.IsNullOrEmpty(doc)) return FileShare.Restrict;

            var fileId = Parse(doc, out linkId, out _);
            if (string.IsNullOrEmpty(fileId)) return FileShare.Restrict;

            file = fileDao.GetFile(fileId);
            if (file == null) return FileShare.Restrict;

            var filesSecurity = Global.GetFilesSecurity();
            if (filesSecurity.CanEdit(file, linkId)) return FileShare.ReadWrite;
            if (filesSecurity.CanCustomFilterEdit(file, linkId)) return FileShare.CustomFilter;
            if (filesSecurity.CanReview(file, linkId)) return FileShare.Review;
            if (filesSecurity.CanFillForms(file, linkId)) return FileShare.FillForms;
            if (filesSecurity.CanComment(file, linkId)) return FileShare.Comment;
            if (filesSecurity.CanRead(file, linkId)) return FileShare.Read;
            return FileShare.Restrict;
        }

        public static bool CheckCookieKey(Guid linkId, out string cookieValue)
        {
            cookieValue = null;

            if (linkId == FileConstant.ShareLinkId)
            {
                return true;
            }

            var fileSecurity = Global.GetFilesSecurity();
            var shareRecord = fileSecurity.GetShareRecord(linkId);

            return shareRecord == null ? false : CheckCookieOrPasswordKey(shareRecord, null, out cookieValue);
        }

        public static bool CheckCookieOrPasswordKey(FileShareRecord shareRecord, string passwordKey, out string cookieValue)
        {
            cookieValue = null;

            if (shareRecord.SubjectType != SubjectType.ExternalLink ||
                shareRecord.Subject == FileConstant.ShareLinkId ||
                shareRecord.Options == null)
            {
                return true;
            }

            if (shareRecord.Options.IsExpired())
            {
                return false;
            }

            if (string.IsNullOrEmpty(shareRecord.Options.Password))
            {
                return true;
            }

            var key = shareRecord.Options.GetPasswordKey();

            if (!string.IsNullOrEmpty(passwordKey) && passwordKey == key)
            {
                return true;
            }

            var itemId = shareRecord.Subject.ToString();
            var cookie = CallContext.GetData(PASSWORD_KEY)?.ToString() ?? CookiesManager.GetCookies(CookiesType.ShareLink, itemId, true);
            if (string.IsNullOrEmpty(cookie))
            {
                return false;
            }

            if (cookie == key)
            {
                cookieValue = cookie;
                return true;
            }

            CookiesManager.ClearCookies(CookiesType.ShareLink, itemId);
            return false;
        }

        public static void SetCookieKey(FileShareRecord shareRecord)
        {
            if (shareRecord.SubjectType != SubjectType.ExternalLink ||
                shareRecord.Subject == FileConstant.ShareLinkId ||
                shareRecord.Options == null ||
                string.IsNullOrEmpty(shareRecord.Options.Password))
            {
                return;
            }

            var itemId = shareRecord.Subject.ToString();
            var key = shareRecord.Options.GetPasswordKey();
            CookiesManager.SetCookies(CookiesType.ShareLink, itemId, key, null, true, true); //TODO: session cookie?
        }

        #endregion

        #region File

        public static string GetLink(File file, bool withHash = true, Guid shareLinkId = default(Guid))
        {
            var url = file.DownloadUrl;

            if (FileUtility.CanWebView(file.Title))
                url = FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID);

            if (withHash)
            {
                var linkParams = CreateKey(file.ID.ToString(), shareLinkId);
                url += "&" + FilesLinkUtility.DocShareKey + "=" + HttpUtility.UrlEncode(linkParams);
            }

            return CommonLinkUtility.GetFullAbsolutePath(url);
        }

        public static string GetPasswordProtectedFileLink(string key)
        {
            var link = string.Format("confirm.aspx?type={0}&key={1}", ConfirmType.ShareLinkPassword, key);
            return CommonLinkUtility.GetFullAbsolutePath(link);
        }

        public static bool CheckSignatureKey(string doc)
        {
            var fileId = Parse(doc, out Guid linkId, out string linkPassword);

            if (string.IsNullOrEmpty(fileId))
            {
                return false;
            }

            if (linkId == FileConstant.ShareLinkId)
            {
                return true;
            }

            var fileSecurity = Global.GetFilesSecurity();
            var shareRecord = fileSecurity.GetShareRecord(linkId);

            return shareRecord == null ? false : CheckCookieOrPasswordKey(shareRecord, linkPassword, out string _);
        }

        #endregion

        #region Folder

        public static bool TryGetCurrentLinkId(out Guid linkId)
        {
            linkId = default;

            if (!FilesSettings.ExternalShare)
            {
                return false;
            }

            var contextId = CallContext.GetData(LINK_ID_KEY);

            if (contextId != null)
            {
                return Guid.TryParse(contextId.ToString(), out linkId);
            }

            var key = HttpContext.Current?.Request[FilesLinkUtility.FolderShareKey] ?? string.Empty;

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var folderId = Parse(key, out linkId, out _);

            return !string.IsNullOrEmpty(folderId);
        }

        public static bool CheckSignatureKey(string key, out Folder folder)
        {
            folder = null;

            if (!FilesSettings.ExternalShare)
            {
                return false;
            }

            var folderId = Parse(key, out var linkId, out _);

            if (string.IsNullOrEmpty(folderId))
            {
                return false;
            }

            var fileSecurity = Global.GetFilesSecurity();
            var shareRecord = fileSecurity.GetShareRecord(linkId);

            var success = shareRecord != null && CheckCookieOrPasswordKey(shareRecord, null, out _);

            if (!success)
            {
                return false;
            }
            
            try
            { 
                folder = Global.FileStorageService.GetFolder(folderId);
            }
            catch (Exception ex)
            {
                Global.Logger.Error("FileShareLink: Cant get folder: " + folderId, ex);
                return true;
            }

            if (!SecurityContext.IsAuthenticated && string.IsNullOrEmpty(CookiesManager.GetCookies(CookiesType.AnonymousSessionKey)))
            {
                CookiesManager.SetCookies(CookiesType.AnonymousSessionKey, CreateKey(Guid.NewGuid().ToString()), true);
            }

            return true;
        }

        public static string GetLink(Folder folder, Guid shareLinkId)
        {
            var folderId = folder.ID.ToString();
            var key = CreateKey(folderId, shareLinkId);

            var url = FilesLinkUtility.FilesBaseAbsolutePath + "?" + FilesLinkUtility.FolderShareKey + "=" + HttpUtility.UrlEncode(key) + "#" + folderId;

            return CommonLinkUtility.GetFullAbsolutePath(url);
        }

        public static string GetPasswordProtectedFolderLink(string key)
        {
            var link = string.Format("confirm.aspx?type={0}&key={1}&folder={2}", ConfirmType.ShareLinkPassword, key, true);
            return CommonLinkUtility.GetFullAbsolutePath(link);
        }

        public static string CreateDownloadSessionKey(Guid linkId, Guid sessionId)
        {
            var jobject = new JObject(
                   new JProperty("link", linkId),
                   new JProperty("session", sessionId)
               );

            var payload = jobject.ToString(Newtonsoft.Json.Formatting.None);

            return Signature.Create(payload, Global.GetDocDbKey());
        }

        public static bool ParseDownloadSessionKey(string session, out Guid linkId, out Guid sessionId)
        {
            linkId = Guid.Empty;
            sessionId = Guid.Empty;

            var value = Signature.Read<string>(session ?? string.Empty, Global.GetDocDbKey());

            if (!string.IsNullOrEmpty(value) && value.StartsWith("{") && value.EndsWith("}"))
            {
                try
                {
                    var jobject = JObject.Parse(value);
                    linkId = Guid.Parse(jobject.Value<string>("link"));
                    sessionId = Guid.Parse(jobject.Value<string>("session"));
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("FileShareLink: Cant parse json: " + value, ex);
                    return false;
                }
            }

            return true;
        }

        public static bool TryGetSessionId(out Guid sessionId)
        {
            sessionId = default;

            var contextSessionId = CallContext.GetData(SESSION_ID_KEY);

            if (contextSessionId != null)
            {
                return Guid.TryParse(contextSessionId.ToString(), out sessionId);
            }
            
            var key = CookiesManager.GetCookies(CookiesType.AnonymousSessionKey);

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var parsedKey = Parse(key, out _, out _);

            return !string.IsNullOrEmpty(parsedKey) && Guid.TryParse(parsedKey, out sessionId);
        }

        public static string GetPasswordKey(Guid linkId)
        {
            return CookiesManager.GetCookies(CookiesType.ShareLink, linkId.ToString(), true);
        }

        public static void SetCurrentLinkData(Guid linkId, Guid sessionId, string passwordKey)
        {
            if (linkId != default)
            {
                CallContext.SetData(LINK_ID_KEY, linkId.ToString());
            }

            if (sessionId != default)
            { 
                CallContext.SetData(SESSION_ID_KEY, sessionId.ToString());   
            }
            
            CallContext.SetData(PASSWORD_KEY, passwordKey);
        }

        #endregion
    }
}