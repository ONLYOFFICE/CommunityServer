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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal abstract class GoogleDriveDaoBase
    {
        protected readonly GoogleDriveDaoSelector GoogleDriveDaoSelector;

        public int TenantID { get; private set; }
        public GoogleDriveProviderInfo GoogleDriveProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }


        protected GoogleDriveDaoBase(GoogleDriveDaoSelector.GoogleDriveInfo googleDriveInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
        {
            GoogleDriveProviderInfo = googleDriveInfo.GoogleDriveProviderInfo;
            PathPrefix = googleDriveInfo.PathPrefix;
            GoogleDriveDaoSelector = googleDriveDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            GoogleDriveProviderInfo.Dispose();
        }

        protected DbManager GetDb()
        {
            return new DbManager(FileConstant.DatabaseId);
        }

        protected object MappingID(object id, bool saveIfNotExist = false)
        {
            if (id == null) return null;
            int n;

            var isNumeric = int.TryParse(id.ToString(), out n);

            if (isNumeric) return n;

            object result;
            using (var db = GetDb())
            {
                if (id.ToString().StartsWith("drive"))
                {
                    result = Regex.Replace(BitConverter.ToString(Hasher.Hash(id.ToString(), HashAlg.MD5)), "-", "").ToLower();
                }
                else
                {
                    result = db.ExecuteScalar<String>(Query("files_thirdparty_id_mapping")
                                                          .Select("id")
                                                          .Where(Exp.Eq("hash_id", id)));
                }
                if (saveIfNotExist)
                {
                    db.ExecuteNonQuery(Insert("files_thirdparty_id_mapping")
                                           .InColumnValue("id", id)
                                           .InColumnValue("hash_id", result));
                }
            }
            return result;
        }

        protected SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumns(GetTenantColumnName(table)).Values(TenantID);
        }

        private static string GetTenantColumnName(string table)
        {
            const string tenant = "tenant_id";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ", StringComparison.InvariantCulture)).Trim() + "." + tenant;
        }


        protected static string MakeDriveId(object entryId)
        {
            var id = Convert.ToString(entryId, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(id)
                       ? "root"
                       : id.TrimStart('/');
        }

        protected static string GetParentDriveId(DriveFile driveEntry)
        {
            return driveEntry == null || driveEntry.Parents == null || driveEntry.Parents.Count == 0
                       ? null
                       : driveEntry.Parents[0];
        }

        protected string MakeId(DriveFile driveEntry)
        {
            var path = string.Empty;
            if (driveEntry != null)
            {
                path = IsRoot(driveEntry) ? "root" : driveEntry.Id;
            }

            return MakeId(path);
        }

        protected string MakeId(string path = null)
        {
            return string.Format("{0}{1}", PathPrefix,
                                 string.IsNullOrEmpty(path) || path == "root" || path == GoogleDriveProviderInfo.DriveRootId
                                     ? "" : ("-|" + path.TrimStart('/')));
        }

        protected String MakeFolderTitle(DriveFile driveFolder)
        {
            if (driveFolder == null || IsRoot(driveFolder))
            {
                return GoogleDriveProviderInfo.CustomerTitle;
            }

            return Global.ReplaceInvalidCharsAndTruncate(driveFolder.Name);
        }

        protected String MakeFileTitle(DriveFile driveFile)
        {
            if (driveFile == null || string.IsNullOrEmpty(driveFile.Name))
            {
                return GoogleDriveProviderInfo.ProviderKey;
            }

            var title = driveFile.Name;

            var gExt = MimeMapping.GetExtention(driveFile.MimeType);
            if (GoogleLoginProvider.GoogleDriveExt.Contains(gExt))
            {
                var downloadableExtension = FileUtility.GetGoogleDownloadableExtension(gExt);
                if (!downloadableExtension.Equals(FileUtility.GetFileExtension(title)))
                {
                    title += downloadableExtension;
                }
            }

            return Global.ReplaceInvalidCharsAndTruncate(title);
        }

        protected Folder ToFolder(DriveFile driveEntry)
        {
            if (driveEntry == null) return null;
            if (driveEntry is ErrorDriveEntry)
            {
                //Return error entry
                return ToErrorFolder(driveEntry as ErrorDriveEntry);
            }

            if (driveEntry.MimeType != GoogleLoginProvider.GoogleDriveMimeTypeFolder)
            {
                return null;
            }

            var isRoot = IsRoot(driveEntry);

            var folder = new Folder
                {
                    ID = MakeId(driveEntry),
                    ParentFolderID = isRoot ? null : MakeId(GetParentDriveId(driveEntry)),
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = isRoot ? GoogleDriveProviderInfo.CreateOn : (driveEntry.CreatedTime.HasValue ? driveEntry.CreatedTime.Value : default(DateTime)),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = isRoot ? GoogleDriveProviderInfo.CreateOn : (driveEntry.ModifiedTime.HasValue ? driveEntry.ModifiedTime.Value : default(DateTime)),
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,

                    Shareable = false,
                    Title = MakeFolderTitle(driveEntry),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                };

            if (folder.CreateOn != DateTime.MinValue && folder.CreateOn.Kind == DateTimeKind.Utc)
                folder.CreateOn = TenantUtil.DateTimeFromUtc(folder.CreateOn);

            if (folder.ModifiedOn != DateTime.MinValue && folder.ModifiedOn.Kind == DateTimeKind.Utc)
                folder.ModifiedOn = TenantUtil.DateTimeFromUtc(folder.ModifiedOn);

            return folder;
        }

        protected static bool IsRoot(DriveFile driveFolder)
        {
            return IsDriveFolder(driveFolder) && GetParentDriveId(driveFolder) == null;
        }

        private static bool IsDriveFolder(DriveFile driveFolder)
        {
            return driveFolder != null && driveFolder.MimeType == GoogleLoginProvider.GoogleDriveMimeTypeFolder;
        }

        private File ToErrorFile(ErrorDriveEntry driveEntry)
        {
            if (driveEntry == null) return null;
            return new File
                {
                    ID = MakeId(driveEntry.ErrorId),
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    Title = MakeFileTitle(driveEntry),
                    Error = driveEntry.Error
                };
        }

        private Folder ToErrorFolder(ErrorDriveEntry driveEntry)
        {
            if (driveEntry == null) return null;
            return new Folder
                {
                    ID = MakeId(driveEntry.ErrorId),
                    ParentFolderID = null,
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    Shareable = false,
                    Title = MakeFolderTitle(driveEntry),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                    Error = driveEntry.Error
                };
        }

        public File ToFile(DriveFile driveFile)
        {
            if (driveFile == null) return null;

            if (driveFile is ErrorDriveEntry)
            {
                //Return error entry
                return ToErrorFile(driveFile as ErrorDriveEntry);
            }

            return new File
                {
                    ID = MakeId(driveFile.Id),
                    Access = FileShare.None,
                    ContentLength = driveFile.Size.HasValue ? (long)driveFile.Size : 0,
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = driveFile.CreatedTime.HasValue ? TenantUtil.DateTimeFromUtc(driveFile.CreatedTime.Value) : default(DateTime),
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(GetParentDriveId(driveFile)),
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = driveFile.ModifiedTime.HasValue ? TenantUtil.DateTimeFromUtc(driveFile.ModifiedTime.Value) : default(DateTime),
                    NativeAccessor = driveFile,
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    Title = MakeFileTitle(driveFile),
                    RootFolderId = MakeId(),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    Shared = false,
                    Version = 1
                };
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(GetDriveEntry(""));
        }

        protected DriveFile GetDriveEntry(object entryId)
        {
            var driveId = MakeDriveId(entryId);
            try
            {
                var entry = GoogleDriveProviderInfo.GetDriveEntry(driveId);
                return entry;
            }
            catch (Exception ex)
            {
                return new ErrorDriveEntry(ex, driveId);
            }
        }

        protected IEnumerable<string> GetChildren(object folderId)
        {
            return GetDriveEntries(folderId).Select(entry => MakeId(entry.Id));
        }

        protected List<DriveFile> GetDriveEntries(object parentId, bool? folder = null)
        {
            var parentDriveId = MakeDriveId(parentId);
            var entries = GoogleDriveProviderInfo.GetDriveEntries(parentDriveId, folder);
            return entries;
        }


        protected sealed class ErrorDriveEntry : DriveFile
        {
            public string Error { get; set; }

            public string ErrorId { get; private set; }


            public ErrorDriveEntry(Exception e, object id)
            {
                ErrorId = id.ToString();
                if (id.ToString() == "root")
                {
                    MimeType = GoogleLoginProvider.GoogleDriveMimeTypeFolder;
                }
                if (e != null)
                {
                    Error = e.Message;
                }
            }
        }
    }
}