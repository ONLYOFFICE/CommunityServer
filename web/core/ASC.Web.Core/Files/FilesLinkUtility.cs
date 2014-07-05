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

        public static string DocServiceApiUrl
        {
            get { return WebConfigurationManager.AppSettings["files.docservice.url.api"]; }
        }

        public static string DocServiceApiUrlNew
        {
            get { return WebConfigurationManager.AppSettings["files.docservice.url.apinew"]; }
        }

        public static string DocServiceConverterUrl
        {
            get { return WebConfigurationManager.AppSettings["files.docservice.url.converter"]; }
        }

        public static string DocServiceStorageUrl
        {
            get { return WebConfigurationManager.AppSettings["files.docservice.url.storage"]; }
        }

        public static string DocServiceCommandUrl
        {
            get { return WebConfigurationManager.AppSettings["files.docservice.url.command"]; }
        }

        public static string FileViewUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=view&" + FileId + "={0}"; }
        }

        public static string GetFileViewUrl(object fileId)
        {
            return GetFileViewUrl(fileId, 0);
        }

        public static string GetFileViewUrl(object fileId, int fileVersion)
        {
            return string.Format(FileViewUrlString, HttpUtility.UrlEncode(fileId.ToString()))
                   + (fileVersion > 0 ? string.Empty : "&" + Version + "=" + fileVersion);
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

        public static string GetFileWebImageViewUrl(object fileId)
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

        public static string GetFileWebViewerExternalUrl(string fileUri, string fileTitle, string refererUrl)
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
            if (FileUtility.CanImageView(fileTitle))
                return GetFileWebImageViewUrl(fileId);

            if (FileUtility.CanWebView(fileTitle))
            {
                if (FileUtility.ExtsMustConvert.Contains(FileUtility.GetFileExtension(fileTitle)))
                    return string.Format(FileWebViewerUrlString, HttpUtility.UrlEncode(fileId.ToString()));
                return GetFileWebEditorUrl(fileId);
            }

            return GetFileViewUrl(fileId);
        }

        public static string GetFileRedirectPreviewUrl(object enrtyId, bool isFile)
        {
            return FileHandlerPath + "?" + Action + "=redirect&" + (isFile ? FileId : FolderId) + "=" + enrtyId;
        }

        public static string GetInitiateUploadSessionUrl(object folderId, object fileId, string fileName, long contentLength)
        {
            var queryString = string.Format("?initiate=true&name={0}&fileSize={1}&tid={2}&userid={3}",
                                            fileName, contentLength, TenantProvider.CurrentTenantID,
                                            HttpUtility.UrlEncode(InstanceCrypto.Encrypt(SecurityContext.CurrentAccount.ID.ToString())));

            if (fileId != null)
                queryString = queryString + "&fileid=" + fileId;

            if (folderId != null)
                queryString = queryString + "&folderid=" + folderId;

            return CommonLinkUtility.GetFullAbsolutePath(GetFileUploaderHandlerVirtualPath(contentLength > 0) + queryString);
        }

        public static string GetUploadChunkLocationUrl(string uploadId, bool serviceUrl)
        {
            var queryString = "?uid=" + uploadId;
            return CommonLinkUtility.GetFullAbsolutePath(GetFileUploaderHandlerVirtualPath(serviceUrl) + queryString);
        }

        private static string GetFileUploaderHandlerVirtualPath(bool getServiceUrl)
        {
            string virtualPath = getServiceUrl
                                     ? (WebConfigurationManager.AppSettings["files.uploader.url"] ?? "~")
                                     : (WebConfigurationManager.AppSettings["files.uploader.url.local"] ?? "~/products/files");

            return virtualPath.EndsWith(".ashx") ? virtualPath : virtualPath.TrimEnd('/') + "/ChunkedUploader.ashx";
        }
    }
}