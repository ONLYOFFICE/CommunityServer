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
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files.Classes
{
    public static class PathProvider
    {
        public static readonly String ProjectVirtualPath = "~/products/projects/tmdocs.aspx";

        public static readonly String TemplatePath = "/products/files/templates/";

        public static readonly String StartURL = FilesLinkUtility.FilesBaseVirtualPath;

        public static readonly String GetFileServicePath = CommonLinkUtility.ToAbsolute("~/products/files/services/wcfservice/service.svc/");

        public static string GetImagePath(string imgFileName)
        {
            return WebImageSupplier.GetAbsoluteWebPath(imgFileName, Configuration.ProductEntryPoint.ID).ToLower();
        }

        public static string GetFileStaticRelativePath(string fileName)
        {
            var ext = FileUtility.GetFileExtension(fileName);
            switch (ext)
            {
                case ".js": //Attention: Only for ResourceBundleControl
                    return VirtualPathUtility.ToAbsolute("~/products/files/js/" + fileName);
                case ".ascx":
                    return CommonLinkUtility.ToAbsolute("~/products/files/controls/" + fileName).ToLowerInvariant();
                case ".css": //Attention: Only for ResourceBundleControl
                    return VirtualPathUtility.ToAbsolute("~/products/files/app_themes/default/" + fileName);
            }

            return fileName;
        }

        public static String GetFileControlPath(String fileName)
        {
            return CommonLinkUtility.ToAbsolute("~/products/files/controls/" + fileName).ToLowerInvariant();
        }

        public static String GetFolderUrl(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder", FilesCommonResource.ErrorMassage_FolderNotFound);

            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                switch (folder.RootFolderType)
                {
                    case FolderType.BUNCH:
                        var path = folderDao.GetBunchObjectID(folder.RootFolderId);

                        var projectID = path.Split('/').Last();

                        if (String.IsNullOrEmpty(projectID)) return String.Empty;

                        return CommonLinkUtility.GetFullAbsolutePath(String.Format("{0}?prjid={1}#{2}", ProjectVirtualPath, projectID, folder.ID));
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

        public static string GetFileStreamUrl(File file)
        {
            if (file == null) throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);

            const int uriLengthLimit = 1024;
            string fileUri = null;
            if (!DocumentServiceHelper.HaveExternalIP())
                fileUri = DocumentServiceHelper.GetExternalUri(file);

            if (!string.IsNullOrEmpty(fileUri))
                return fileUri;

            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                if (fileDao.IsSupportedPreSignedUri(file))
                {
                    var uri = fileDao.GetPreSignedUri(file, Global.StreamUrlExpire).ToString();
                    if (uri.Length < uriLengthLimit) return uri;
                    Global.Logger.Debug("Very long link: " + uri.Length);
                }
            }

            //NOTE: Always build path to handler!
            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            Global.Logger.Debug("FileStreamUrl: " + uriBuilder.Uri);
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=stream&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()) + "&";
            query += FilesLinkUtility.Version + "=" + file.Version + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file.ID + file.Version.ToString(CultureInfo.InvariantCulture));

            return uriBuilder.Uri + "?" + query;
        }

        public static string GetFileDifferenceUrl(File file)
        {
            if (file == null) throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);

            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=diff&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()) + "&";
            query += FilesLinkUtility.Version + "=" + file.Version + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file.ID + file.Version.ToString(CultureInfo.InvariantCulture));

            return uriBuilder.Uri + "?" + query;
        }

        public static string GetLicenseUrl(File file)
        {
            if (!CoreContext.Configuration.Standalone) return string.Empty;
            if (file == null) return string.Empty;

            //NOTE: Always build path to handler!
            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=license&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()) + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file.ID.ToString());

            return uriBuilder.Uri + "?" + query;
        }
    }
}