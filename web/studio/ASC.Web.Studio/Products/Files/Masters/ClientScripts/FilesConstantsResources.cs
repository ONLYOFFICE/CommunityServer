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


using System.Collections.Generic;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core.Client;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files.Masters.ClientScripts
{
    public class FilesConstantsResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Files.Constants"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject("URL_OAUTH2_GOOGLE", Import.Google.Google.Location.ToLower());
            yield return RegisterObject("URL_OAUTH_DROPBOX", Import.DropBox.Dropbox.Location.ToLower());
            yield return RegisterObject("URL_OAUTH_SKYDRIVE", Import.OneDrive.OneDriveOAuth.Location.ToLower());

            yield return RegisterObject("URL_BASE", FilesLinkUtility.FilesBaseAbsolutePath);
            yield return RegisterObject("URL_WCFSERVICE", PathProvider.GetFileServicePath);
            yield return RegisterObject("URL_TEMPLATES_HANDLER", CommonLinkUtility.ToAbsolute("~/template.ashx") + "?id=" + PathProvider.TemplatePath + "&name=collection&ver=" + ClientSettings.ResetCacheKey);

            yield return RegisterObject("ADMIN", Global.IsAdministrator);
            yield return RegisterObject("MAX_NAME_LENGTH", Global.MaxTitle);
            yield return RegisterObject("CHUNK_UPLOAD_SIZE", SetupInfo.ChunkUploadSize);
            yield return RegisterObject("UPLOAD_FILTER", Global.EnableUploadFilter);
            yield return RegisterObject("ENABLE_UPLOAD_CONVERT", FileConverter.EnableAsUploaded);

            yield return RegisterObject("FOLDER_ID_MY_FILES", Global.FolderMy);
            yield return RegisterObject("FOLDER_ID_SHARE", Global.FolderShare);
            yield return RegisterObject("FOLDER_ID_COMMON_FILES", Global.FolderCommon);
            yield return RegisterObject("FOLDER_ID_PROJECT", Global.FolderProjects);
            yield return RegisterObject("FOLDER_ID_TRASH", Global.FolderTrash);

            yield return RegisterObject("ShareLinkId", FileConstant.ShareLinkId);

            yield return RegisterObject("AceStatusEnum", new object());
            yield return RegisterObject("AceStatusEnum.None", FileShare.None);
            yield return RegisterObject("AceStatusEnum.ReadWrite", FileShare.ReadWrite);
            yield return RegisterObject("AceStatusEnum.Read", FileShare.Read);
            yield return RegisterObject("AceStatusEnum.Restrict", FileShare.Restrict);
            yield return RegisterObject("AceStatusEnum.Varies", FileShare.Varies);

            yield return RegisterObject("FilterType", new object());
            yield return RegisterObject("FilterType.None", FilterType.None);
            yield return RegisterObject("FilterType.FilesOnly", FilterType.FilesOnly);
            yield return RegisterObject("FilterType.FoldersOnly", FilterType.FoldersOnly);
            yield return RegisterObject("FilterType.DocumentsOnly", FilterType.DocumentsOnly);
            yield return RegisterObject("FilterType.PresentationsOnly", FilterType.PresentationsOnly);
            yield return RegisterObject("FilterType.SpreadsheetsOnly", FilterType.SpreadsheetsOnly);
            yield return RegisterObject("FilterType.ImagesOnly", FilterType.ImagesOnly);
            yield return RegisterObject("FilterType.ArchiveOnly", FilterType.ArchiveOnly);
            yield return RegisterObject("FilterType.ByUser", FilterType.ByUser);
            yield return RegisterObject("FilterType.ByDepartment", FilterType.ByDepartment);
            yield return RegisterObject("FilterType.ByExtension", FilterType.ByExtension);
        }

        protected override string GetCacheHash()
        {
            return
                ClientSettings.ResetCacheKey
                + CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).LastModified.ToString(CultureInfo.InvariantCulture)
                + CoreContext.TenantManager.GetCurrentTenant().LastModified.ToString(CultureInfo.InvariantCulture)
                + SecurityContext.IsAuthenticated;
        }
    }
}