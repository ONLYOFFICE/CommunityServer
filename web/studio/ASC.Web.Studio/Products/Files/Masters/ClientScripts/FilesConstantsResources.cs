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


using System.Collections.Generic;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core.Client;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.ThirdParty;
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
            return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                            {
                                URL_OAUTH_BOX = Box.Location.ToLower(),
                                URL_OAUTH2_GOOGLE = Google.Location.ToLower(),
                                URL_OAUTH_DROPBOXV2 = Dropbox.Location.ToLower(),
                                URL_OAUTH_SKYDRIVE = OneDrive.Location.ToLower(),
                                URL_OAUTH_DOCUSIGN = Studio.ThirdParty.DocuSign.Location.ToLower(),
                                URL_OAUTH_DOCUSIGN_LINK = DocuSignLoginProvider.Instance.DocuSignHost,

                                URL_BASE = FilesLinkUtility.FilesBaseAbsolutePath,
                                URL_WCFSERVICE = PathProvider.GetFileServicePath,
                                URL_TEMPLATES_HANDLER = CommonLinkUtility.ToAbsolute("~/template.ashx") + "?id=" + PathProvider.TemplatePath + "&name=collection&ver=" + ClientSettings.ResetCacheKey,

                                ADMIN = Global.IsAdministrator,
                                MAX_NAME_LENGTH = Global.MaxTitle,
                                CHUNK_UPLOAD_SIZE = SetupInfo.ChunkUploadSize,
                                UPLOAD_FILTER = Global.EnableUploadFilter,
                                ENABLE_UPLOAD_CONVERT = FileConverter.EnableAsUploaded,

                                FOLDER_ID_MY_FILES = Global.FolderMy,
                                FOLDER_ID_SHARE = Global.FolderShare,
                                FOLDER_ID_COMMON_FILES = Global.FolderCommon,
                                FOLDER_ID_PROJECT = Global.FolderProjects,
                                FOLDER_ID_TRASH = Global.FolderTrash,

                                FileConstant.ShareLinkId,

                                AceStatusEnum = new
                                    {
                                        FileShare.None,
                                        FileShare.ReadWrite,
                                        FileShare.Read,
                                        FileShare.Restrict,
                                        FileShare.Varies,
                                        FileShare.Review,
                                        FileShare.FillForms,
                                        FileShare.Comment
                                    },

                                FilterType = new
                                    {
                                        FilterType.None,
                                        FilterType.FilesOnly,
                                        FilterType.FoldersOnly,
                                        FilterType.DocumentsOnly,
                                        FilterType.PresentationsOnly,
                                        FilterType.SpreadsheetsOnly,
                                        FilterType.ImagesOnly,
                                        FilterType.ArchiveOnly,
                                        FilterType.ByUser,
                                        FilterType.ByDepartment,
                                        FilterType.ByExtension,
                                        FilterType.MediaOnly
                                    },

                                ConflictResolveType = new
                                    {
                                        FileConflictResolveType.Skip,
                                        FileConflictResolveType.Overwrite,
                                        FileConflictResolveType.Duplicate
                                    },

                                DocuSignFormats = DocuSignHelper.SupportedFormats,
                                SetupInfo.AvailableFileSize,
                            })
                };
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