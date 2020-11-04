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
                                URL_OAUTH2_GOOGLE = ASC.Web.Studio.ThirdParty.Google.Location.ToLower(),
                                URL_OAUTH_DROPBOXV2 = Dropbox.Location.ToLower(),
                                URL_OAUTH_SKYDRIVE = OneDrive.Location.ToLower(),
                                URL_OAUTH_DOCUSIGN = Studio.ThirdParty.DocuSign.Location.ToLower(),
                                URL_OAUTH_DOCUSIGN_LINK = DocuSignLoginProvider.Instance.DocuSignHost,

                                URL_BASE = FilesLinkUtility.FilesBaseAbsolutePath,
                                URL_WCFSERVICE = PathProvider.GetFileServicePath,
                                URL_TEMPLATES_HANDLER = CommonLinkUtility.ToAbsolute("~/template.ashx") + "?id=" + PathProvider.TemplatePath + "&name=collection&ver=" + ClientSettings.ResetCacheKey,
                                URL_LOADER = CommonLinkUtility.ToAbsolute(FilesLinkUtility.FilesBaseVirtualPath + "loader.html"),

                                ADMIN = Global.IsAdministrator,
                                MAX_NAME_LENGTH = Global.MaxTitle,
                                CHUNK_UPLOAD_SIZE = SetupInfo.ChunkUploadSize,
                                UPLOAD_FILTER = Global.EnableUploadFilter,
                                ENABLE_UPLOAD_CONVERT = FileConverter.EnableAsUploaded,

                                FOLDER_ID_MY_FILES = Global.FolderMy,
                                FOLDER_ID_SHARE = Global.FolderShare,
                                FOLDER_ID_RECENT = Global.FolderRecent,
                                FOLDER_ID_FAVORITES = Global.FolderFavorites,
                                FOLDER_ID_TEMPLATES = Global.FolderTemplates,
                                FOLDER_ID_PRIVACY = Global.FolderPrivacy,
                                FOLDER_ID_COMMON_FILES = Global.FolderCommon,
                                FOLDER_ID_PROJECT = Global.FolderProjects,
                                FOLDER_ID_TRASH = Global.FolderTrash,

                                FileConstant.ShareLinkId,

                                AceStatusEnum = new
                                    {
                                        FileShare.None,
                                        FileShare.ReadWrite,
                                        FileShare.CustomFilter,
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