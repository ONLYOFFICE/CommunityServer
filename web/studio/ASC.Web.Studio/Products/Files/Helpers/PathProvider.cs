/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
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

                        return String.Format("{0}?prjid={1}#{2}", CommonLinkUtility.ToAbsolute(ProjectVirtualPath),
                                             projectID, folder.ID);
                    default:
                        return FilesLinkUtility.FilesBaseAbsolutePath + "#" + HttpUtility.UrlPathEncode(folder.ID.ToString());
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
                    int validateTimespan;
                    int.TryParse(WebConfigurationManager.AppSettings["files.stream-url-minute"], out validateTimespan);
                    if (validateTimespan <= 0) validateTimespan = 10;
                    var uri = fileDao.GetPreSignedUri(file, TimeSpan.FromMinutes(validateTimespan)).ToString();
                    if (uri.Length < uriLengthLimit) return uri;
                    Global.Logger.Debug("Very long link: " + uri.Length);
                }
            }

            //NOTE: Always build path to handler!
            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            if (uriBuilder.Uri.IsLoopback)
            {
                uriBuilder.Host = Dns.GetHostName();
            }
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=stream&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()) + "&";
            query += FilesLinkUtility.Version + "=" + file.Version + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(file.ID + file.Version.ToString(CultureInfo.InvariantCulture));

            return uriBuilder.Uri + "?" + query;
        }
    }
}