/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Security.Cryptography;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.Files
{
    public static class FilesLinkUtility
    {
        public const string FilesBaseVirtualPath = "~/products/files/";
        public const string EditorPage = "doceditor.aspx";
        private static readonly string FilesUploaderURL = WebConfigurationManager.AppSettings["files.uploader.url"] ?? "~";

        public static string FilesBaseAbsolutePath
        {
            get { return CommonLinkUtility.ToAbsolute(FilesBaseVirtualPath); }
        }

        public const string FileId = "fileid";
        public const string FolderId = "folderid";
        public const string Version = "version";
        public const string FileUri = "fileuri";
        public const string FileTitle = "title";
        public const string Action = "action";
        public const string DocShareKey = "doc";
        public const string TryParam = "try";
        public const string FolderUrl = "folderurl";
        public const string OutType = "outputtype";
        public const string AuthKey = "stream_auth";

        public static string FileHandlerPath
        {
            get { return FilesBaseAbsolutePath + "httphandlers/filehandler.ashx"; }
        }

        public static string DocServiceUrl
        {
            get
            {
                var url = GetUrlSetting("public");
                if (!string.IsNullOrEmpty(url) && url != "/")
                {
                    url = url.TrimEnd('/') + "/";
                }
                return url;
            }
            set
            {
                SetUrlSetting("api", null);

                value = (value ?? "").Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.TrimEnd('/') + "/";
                    if (!new Regex(@"(^https?:\/\/)|^\/", RegexOptions.CultureInvariant).IsMatch(value))
                    {
                        value = "http://" + value;
                    }
                }

                SetUrlSetting("public", value);
            }
        }

        public static string DocServiceUrlInternal
        {
            get
            {
                var url = GetUrlSetting("internal");
                if (string.IsNullOrEmpty(url))
                {
                    url = DocServiceUrl;
                }
                else
                {
                    url = url.TrimEnd('/') + "/";
                }
                return url;
            }
            set
            {
                SetUrlSetting("converter", null);
                SetUrlSetting("storage", null);
                SetUrlSetting("command", null);
                SetUrlSetting("docbuilder", null);

                value = (value ?? "").Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.TrimEnd('/') + "/";
                    if (!new Regex(@"(^https?:\/\/)", RegexOptions.CultureInvariant).IsMatch(value))
                    {
                        value = "http://" + value;
                    }
                }

                SetUrlSetting("internal", value);
            }
        }

        public static string DocServiceApiUrl
        {
            get
            {
                var url = GetUrlSetting("api");
                if (string.IsNullOrEmpty(url))
                {
                    url = DocServiceUrl;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url += "web-apps/apps/api/documents/api.js";
                    }
                }
                return url;
            }
        }

        public static string DocServiceConverterUrl
        {
            get
            {
                var url = GetUrlSetting("converter");
                if (string.IsNullOrEmpty(url))
                {
                    url = DocServiceUrlInternal;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url += "ConvertService.ashx";
                    }
                }
                return url;
            }
        }

        public static string DocServiceCommandUrl
        {
            get
            {
                var url = GetUrlSetting("command");
                if (string.IsNullOrEmpty(url))
                {
                    url = DocServiceUrlInternal;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url += "coauthoring/CommandService.ashx";
                    }
                }
                return url;
            }
        }

        public static string DocServiceDocbuilderUrl
        {
            get
            {
                var url = GetUrlSetting("docbuilder");
                if (string.IsNullOrEmpty(url))
                {
                    url = DocServiceUrlInternal;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url += "docbuilder";
                    }
                }
                return url;
            }
        }

        public static string DocServiceHealthcheckUrl
        {
            get
            {
                var url = GetUrlSetting("healthcheck");
                if (string.IsNullOrEmpty(url))
                {
                    url = DocServiceUrlInternal;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url += "healthcheck";
                    }
                }
                return url;
            }
        }

        public static string DocServicePortalUrl
        {
            get { return GetUrlSetting("portal"); }
            set
            {
                value = (value ?? "").Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.TrimEnd('/') + "/";
                    if (!new Regex(@"(^https?:\/\/)", RegexOptions.CultureInvariant).IsMatch(value))
                    {
                        value = "http://" + value;
                    }
                }

                SetUrlSetting("portal", value);
            }
        }

        public static string FileDownloadUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=download&" + FileId + "={0}"; }
        }

        public static string GetFileDownloadUrl(object fileId)
        {
            return GetFileDownloadUrl(fileId, 0, string.Empty);
        }

        public static string GetFileDownloadUrl(object fileId, int fileVersion, string convertToExtension)
        {
            return string.Format(FileDownloadUrlString, HttpUtility.UrlEncode(fileId.ToString()))
                   + (fileVersion > 0 ? "&" + Version + "=" + fileVersion : string.Empty)
                   + (string.IsNullOrEmpty(convertToExtension) ? string.Empty : "&" + OutType + "=" + convertToExtension);
        }

        public static string GetFileWebMediaViewUrl(object fileId)
        {
            return FilesBaseAbsolutePath + "#preview/" + HttpUtility.UrlEncode(fileId.ToString());
        }

        public static string FileWebViewerUrlString
        {
            get { return FileWebEditorUrlString + "&" + Action + "=view"; }
        }

        public static string GetFileWebViewerUrlForMobile(object fileId, int fileVersion)
        {
            var viewerUrl = CommonLinkUtility.ToAbsolute("~/../products/files/") + EditorPage + "?" + FileId + "={0}";

            return string.Format(viewerUrl, HttpUtility.UrlEncode(fileId.ToString()))
                   + (fileVersion > 0 ? "&" + Version + "=" + fileVersion : string.Empty);
        }

        public static string FileWebViewerExternalUrlString
        {
            get { return FilesBaseAbsolutePath + EditorPage + "?" + FileUri + "={0}&" + FileTitle + "={1}&" + FolderUrl + "={2}"; }
        }

        public static string GetFileWebViewerExternalUrl(string fileUri, string fileTitle, string refererUrl = "")
        {
            return string.Format(FileWebViewerExternalUrlString, HttpUtility.UrlEncode(fileUri), HttpUtility.UrlEncode(fileTitle), HttpUtility.UrlEncode(refererUrl));
        }

        public static string FileWebEditorUrlString
        {
            get { return FilesBaseAbsolutePath + EditorPage + "?" + FileId + "={0}"; }
        }

        public static string GetFileWebEditorUrl(object fileId)
        {
            return string.Format(FileWebEditorUrlString, HttpUtility.UrlEncode(fileId.ToString()));
        }

        public static string GetFileWebEditorTryUrl(FileType fileType)
        {
            return FilesBaseAbsolutePath + EditorPage + "?" + TryParam + "=" + fileType;
        }

        public static string FileWebEditorExternalUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=create&" + FileUri + "={0}&" + FileTitle + "={1}"; }
        }

        public static string GetFileWebEditorExternalUrl(string fileUri, string fileTitle)
        {
            return GetFileWebEditorExternalUrl(fileUri, fileTitle, false);
        }

        public static string GetFileWebEditorExternalUrl(string fileUri, string fileTitle, bool openFolder)
        {
            var url = string.Format(FileWebEditorExternalUrlString, HttpUtility.UrlEncode(fileUri), HttpUtility.UrlEncode(fileTitle));
            if (openFolder)
                url += "&openfolder=true";
            return url;
        }

        public static string GetFileWebPreviewUrl(string fileTitle, object fileId)
        {
            if (FileUtility.CanImageView(fileTitle) || FileUtility.CanMediaView(fileTitle))
                return GetFileWebMediaViewUrl(fileId);

            if (FileUtility.CanWebView(fileTitle))
            {
                if (FileUtility.ExtsMustConvert.Contains(FileUtility.GetFileExtension(fileTitle)))
                    return string.Format(FileWebViewerUrlString, HttpUtility.UrlEncode(fileId.ToString()));
                return GetFileWebEditorUrl(fileId);
            }

            return GetFileDownloadUrl(fileId);
        }

        public static string FileRedirectPreviewUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=redirect"; }
        }

        public static string GetFileRedirectPreviewUrl(object enrtyId, bool isFile)
        {
            return FileRedirectPreviewUrlString + "&" + (isFile ? FileId : FolderId) + "=" + HttpUtility.UrlEncode(enrtyId.ToString());
        }

        public static string GetInitiateUploadSessionUrl(object folderId, object fileId, string fileName, long contentLength)
        {
            var queryString = string.Format("?initiate=true&{0}={1}&fileSize={2}&tid={3}&userid={4}&culture={5}",
                                            FileTitle,
                                            HttpUtility.UrlEncode(fileName),
                                            contentLength,
                                            TenantProvider.CurrentTenantID,
                                            HttpUtility.UrlEncode(InstanceCrypto.Encrypt(SecurityContext.CurrentAccount.ID.ToString())),
                                            Thread.CurrentThread.CurrentUICulture.Name);

            if (fileId != null)
                queryString = queryString + "&" + FileId + "=" + HttpUtility.UrlEncode(fileId.ToString());

            if (folderId != null)
                queryString = queryString + "&" + FolderId + "=" + HttpUtility.UrlEncode(folderId.ToString());

            return CommonLinkUtility.GetFullAbsolutePath(GetFileUploaderHandlerVirtualPath() + queryString);
        }

        public static string GetUploadChunkLocationUrl(string uploadId)
        {
            var queryString = "?uid=" + uploadId;
            return CommonLinkUtility.GetFullAbsolutePath(GetFileUploaderHandlerVirtualPath() + queryString);
        }

        public static bool IsLocalFileUploader
        {
            get { return !Regex.IsMatch(FilesUploaderURL, "^http(s)?://\\.*"); }
        }

        private static string GetFileUploaderHandlerVirtualPath()
        {
            var virtualPath = FilesUploaderURL;
            return virtualPath.EndsWith(".ashx") ? virtualPath : virtualPath.TrimEnd('/') + "/ChunkedUploader.ashx";
        }

        private static string GetUrlSetting(string key, string appSettingsKey = null)
        {
            var value = string.Empty;
            if (CoreContext.Configuration.Standalone)
            {
                value = CoreContext.Configuration.GetSetting(GetSettingsKey(key));
            }
            if (string.IsNullOrEmpty(value))
            {
                value = WebConfigurationManager.AppSettings["files.docservice.url." + (appSettingsKey ?? key)];
            }
            return value;
        }

        private static void SetUrlSetting(string key, string value)
        {
            if (!CoreContext.Configuration.Standalone)
            {
                throw new NotSupportedException("Method for server edition only.");
            }
            value = (value ?? "").Trim();
            if (string.IsNullOrEmpty(value)) value = null;
            if (GetUrlSetting(key) != value)
                CoreContext.Configuration.SaveSetting(GetSettingsKey(key), value);
        }

        private static string GetSettingsKey(string key)
        {
            return "DocKey_" + key;
        }
    }
}