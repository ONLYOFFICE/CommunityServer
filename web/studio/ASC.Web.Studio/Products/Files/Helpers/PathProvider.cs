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
using System.Linq;
using System.Web;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Classes
{
    public static class PathProvider
    {
        public static readonly String ProjectVirtualPath = "~/Products/Projects/TMDocs.aspx";

        public static readonly String TemplatePath = "/Products/Files/Templates/";

        public static readonly String StartURL = FilesLinkUtility.FilesBaseVirtualPath;

        public static readonly String GetFileServicePath = CommonLinkUtility.ToAbsolute("~/Products/Files/Services/WCFService/service.svc/");

        public static string GetImagePath(string imgFileName)
        {
            return WebImageSupplier.GetAbsoluteWebPath(imgFileName, Configuration.ProductEntryPoint.ID);
        }

        public static string GetFileStaticRelativePath(string fileName)
        {
            var ext = FileUtility.GetFileExtension(fileName);
            switch (ext)
            {
                case ".js": //Attention: Only for ResourceBundleControl
                    return VirtualPathUtility.ToAbsolute("~/Products/Files/js/" + fileName);
                case ".ascx":
                    return CommonLinkUtility.ToAbsolute("~/Products/Files/Controls/" + fileName);
                case ".css": //Attention: Only for ResourceBundleControl
                    return VirtualPathUtility.ToAbsolute("~/Products/Files/App_Themes/default/" + fileName);
            }

            return fileName;
        }

        public static String GetFileControlPath(String fileName)
        {
            return CommonLinkUtility.ToAbsolute("~/Products/Files/Controls/" + fileName);
        }

        public static string GetFolderUrl(Folder folder, int projectID = 0)
        {
            if (folder == null) throw new ArgumentNullException("folder", FilesCommonResource.ErrorMassage_FolderNotFound);

            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                switch (folder.RootFolderType)
                {
                    case FolderType.BUNCH:
                        if (projectID == 0)
                        {
                            var path = folderDao.GetBunchObjectID(folder.RootFolderId);

                            var projectIDFromDao = path.Split('/').Last();

                            if (string.IsNullOrEmpty(projectIDFromDao)) return string.Empty;

                            projectID = Convert.ToInt32(projectIDFromDao);
                        }
                        return CommonLinkUtility.GetFullAbsolutePath(string.Format("{0}?prjid={1}#{2}", ProjectVirtualPath, projectID, folder.ID));
                    default:
                        return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + "#" + HttpUtility.UrlPathEncode(folder.ID.ToString()));
                }
            }
        }

        public static String GetFolderUrl(object folderId)
        {
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                var folder = folderDao.GetFolder(folderId);

                return GetFolderUrl(folder);
            }
        }

        public static string GetFileStreamUrl(File file, string doc = null, bool lastVersion = false)
        {
            if (file == null) throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);

            //NOTE: Always build path to handler!
            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=stream&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()) + "&";
            var version = 0;
            if (!lastVersion)
            {
                version = file.Version;
                query += FilesLinkUtility.Version + "=" + file.Version + "&";
            }
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file.ID.ToString() + version);
            if (!string.IsNullOrEmpty(doc))
            {
                query += "&" + FilesLinkUtility.DocShareKey + "=" + HttpUtility.UrlEncode(doc);
            }

            return uriBuilder.Uri + "?" + query;
        }

        public static string GetFileChangesUrl(File file, string doc = null)
        {
            if (file == null) throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);

            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=diff&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()) + "&";
            query += FilesLinkUtility.Version + "=" + file.Version + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file.ID + file.Version.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(doc))
            {
                query += "&" + FilesLinkUtility.DocShareKey + "=" + HttpUtility.UrlEncode(doc);
            }

            return uriBuilder.Uri + "?" + query;
        }

        public static string GetTempUrl(Stream stream, string ext)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var store = Global.GetStore();
            var fileName = string.Format("{0}{1}", Guid.NewGuid(), ext);
            var path = Path.Combine("temp_stream", fileName);

            store.Save(
                FileConstant.StorageDomainTmp,
                path,
                stream,
                MimeMapping.GetMimeMapping(ext),
                "attachment; filename=\"" + fileName + "\"");

            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=tmp&";
            query += FilesLinkUtility.FileTitle + "=" + HttpUtility.UrlEncode(fileName) + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(fileName);

            return uriBuilder.Uri + "?" + query;
        }

        public static string GetEmptyFileUrl(string extension)
        {
            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=empty&";
            query += FilesLinkUtility.FileTitle + "=" + HttpUtility.UrlEncode(extension);

            return uriBuilder.Uri + "?" + query;
        }
    }
}