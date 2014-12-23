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

            yield return RegisterObject("URL_SHARE_GOOGLE_PLUS", "https://plus.google.com/share?url={0}");
            yield return RegisterObject("URL_SHARE_TWITTER", "https://twitter.com/intent/tweet?text={0}");
            yield return RegisterObject("URL_SHARE_FACEBOOK", "http://www.facebook.com/sharer.php?s=100&p[url]={0}&p[title]={1}&p[images][0]={2}&p[summary]={3}");

            yield return RegisterObject("URL_BASE", FilesLinkUtility.FilesBaseAbsolutePath);
            yield return RegisterObject("URL_WCFSERVICE", PathProvider.GetFileServicePath);
            yield return RegisterObject("URL_TEMPLATES_HANDLER", CommonLinkUtility.ToAbsolute("~/template.ashx") + "?id=" + PathProvider.TemplatePath + "&name=collection&ver=" + ClientSettings.ResetCacheKey);

            yield return RegisterObject("ADMIN", Global.IsAdministrator);
            yield return RegisterObject("MAX_NAME_LENGTH", Global.MaxTitle);
            yield return RegisterObject("CHUNK_UPLOAD_SIZE", SetupInfo.ChunkUploadSize);
            yield return RegisterObject("UPLOAD_FILTER", Global.EnableUploadFilter);
            yield return RegisterObject("REQUEST_CONVERT_DELAY", FileConverter.TimerConvertPeriod);
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