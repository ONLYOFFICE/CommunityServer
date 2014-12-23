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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Core.Tenants;
using DriveFile = Google.Apis.Drive.v2.Data.File;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal abstract class GoogleDriveDaoBase : IDisposable
    {
        protected class ErrorDriveEntry : DriveFile
        {
            public ErrorDriveEntry(Exception e, object id)
            {
                if (e != null) Error = e.Message;

                ErrorId = id.ToString();
            }

            public string Error { get; set; }

            public string ErrorId { get; private set; }
        }

        protected readonly GoogleDriveDaoSelector GoogleDriveDaoSelector;

        public DbManager DbManager { get; private set; }
        public int TenantID { get; private set; }
        public GoogleDriveProviderInfo GoogleDriveProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }

        protected GoogleDriveDaoBase(GoogleDriveDaoSelector.GoogleDriveInfo googleDriveInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
        {
            GoogleDriveProviderInfo = googleDriveInfo.GoogleDriveProviderInfo;
            PathPrefix = googleDriveInfo.PathPrefix;
            GoogleDriveDaoSelector = googleDriveDaoSelector;
            DbManager = new DbManager(FileConstant.DatabaseId);
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            DbManager.Dispose();
            GoogleDriveProviderInfo.Storage.Close();
        }

        protected object MappingID(object id, bool saveIfNotExist = false)
        {
            if (id == null) return null;
            int n;

            var isNumeric = int.TryParse(id.ToString(), out n);

            if (isNumeric) return n;

            object result;

            if (id.ToString().StartsWith("drive"))
                result = Regex.Replace(BitConverter.ToString(Hasher.Hash(id.ToString(), HashAlg.MD5)), "-", "").ToLower();
            else
                result = DbManager.ExecuteScalar<String>(Query("files_thirdparty_id_mapping")
                                                             .Select("id")
                                                             .Where(Exp.Eq("hash_id", id)));

            if (saveIfNotExist)
                DbManager.ExecuteNonQuery(Insert("files_thirdparty_id_mapping")
                                              .InColumnValue("id", id)
                                              .InColumnValue("hash_id", result));

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
            return table.Substring(table.IndexOf(" ")).Trim() + "." + tenant;
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
                       : driveEntry.Parents[0].Id;
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
            return string.Format("{0}{1}", PathPrefix, string.IsNullOrEmpty(path) || path == "root" ? "" : ("-|" + path));
        }

        protected String MakeFolderTitle(DriveFile driveFolder)
        {
            if (driveFolder == null || IsRoot(driveFolder))
            {
                return GoogleDriveProviderInfo.CustomerTitle;
            }

            return Global.ReplaceInvalidCharsAndTruncate(driveFolder.Title);
        }

        protected String MakeFileTitle(DriveFile driveFile)
        {
            var ext = string.Empty;
            if (driveFile == null || string.IsNullOrEmpty(driveFile.Title))
            {
                return GoogleDriveProviderInfo.ProviderKey;
            }

            Tuple<string, string> mimeData;
            if (GoogleDriveStorage.GoogleFilesMimeTypes.TryGetValue(driveFile.MimeType, out mimeData))
            {
                ext = mimeData.Item1;
            }

            return Global.ReplaceInvalidCharsAndTruncate(driveFile.Title + ext);
        }

        protected Folder ToFolder(DriveFile driveEntry)
        {
            if (driveEntry == null) return null;
            if (driveEntry is ErrorDriveEntry)
            {
                //Return error entry
                return ToErrorFolder(driveEntry as ErrorDriveEntry);
            }

            if (driveEntry.MimeType != GoogleDriveStorage.GoogleFolderMimeType)
            {
                return null;
            }

            var isRoot = IsRoot(driveEntry);

            var folder = new Folder
                {
                    ID = MakeId(driveEntry),
                    ParentFolderID = isRoot ? null : MakeId(GetParentDriveId(driveEntry)),
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = isRoot ? GoogleDriveProviderInfo.CreateOn : (driveEntry.CreatedDate.HasValue ? driveEntry.CreatedDate.Value : default(DateTime)),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = isRoot ? GoogleDriveProviderInfo.CreateOn : (driveEntry.ModifiedDate.HasValue ? driveEntry.ModifiedDate.Value : default(DateTime)),
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,

                    Shareable = false,
                    Title = MakeFolderTitle(driveEntry),
                    TotalFiles = 0, /*driveEntry.Count - childFoldersCount NOTE: Removed due to performance isssues*/
                    TotalSubFolders = 0, /*childFoldersCount NOTE: Removed due to performance isssues*/
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
            return driveFolder != null && driveFolder.MimeType == GoogleDriveStorage.GoogleFolderMimeType;
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
                    ContentLength = driveFile.FileSize.HasValue ? (long)driveFile.FileSize : 0,
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = driveFile.CreatedDate.HasValue ? TenantUtil.DateTimeFromUtc(driveFile.CreatedDate.Value) : default(DateTime),
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(GetParentDriveId(driveFile)),
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = driveFile.ModifiedDate.HasValue ? TenantUtil.DateTimeFromUtc(driveFile.ModifiedDate.Value) : default(DateTime),
                    NativeAccessor = driveFile,
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    Title = MakeFileTitle(driveFile),
                    RootFolderId = MakeId(),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    SharedByMe = false,
                    Version = 1
                };
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(GetDriveEntry(""));
        }

        protected DriveFile GetDriveEntry(object entryId)
        {
            DriveFile entry = null;
            Exception e = null;
            var driveId = MakeDriveId(entryId);
            try
            {
                entry = CacheEntry.Get(GoogleDriveProviderInfo.ID + driveId, () => GoogleDriveProviderInfo.Storage.GetEntry(driveId));
            }
            catch (Exception ex)
            {
                e = ex;
            }
            if (entry == null)
            {
                //Create error entry
                entry = new ErrorDriveEntry(e, driveId);
            }

            return entry;
        }

        protected IEnumerable<string> GetChildren(object folderId)
        {
            return GetDriveEntries(folderId).Select(entry => MakeId(entry.Id));
        }

        protected List<DriveFile> GetDriveEntries(object parentId, bool? folder = null)
        {
            var driveId = MakeDriveId(parentId);
            if (folder.HasValue)
            {
                return folder.Value
                           ? CacheChildFolders.Get(GoogleDriveProviderInfo.ID + driveId, () => GoogleDriveProviderInfo.Storage.GetEntries(driveId, true))
                           : CacheChildFiles.Get(GoogleDriveProviderInfo.ID + driveId, () => GoogleDriveProviderInfo.Storage.GetEntries(driveId, false));
            }

            if (!CacheChildFiles.HasItem(GoogleDriveProviderInfo.ID + driveId) && !CacheChildFolders.HasItem(GoogleDriveProviderInfo.ID + driveId))
            {
                var entries = GoogleDriveProviderInfo.Storage.GetEntries(driveId);

                CacheChildFiles.Add(GoogleDriveProviderInfo.ID + driveId, entries.Where(entry => entry.MimeType != GoogleDriveStorage.GoogleFolderMimeType).ToList());
                CacheChildFolders.Add(GoogleDriveProviderInfo.ID + driveId, entries.Where(entry => entry.MimeType == GoogleDriveStorage.GoogleFolderMimeType).ToList());

                return entries;
            }

            var folders = CacheChildFolders.Get(GoogleDriveProviderInfo.ID + driveId, () => GoogleDriveProviderInfo.Storage.GetEntries(driveId, true));
            var files = CacheChildFiles.Get(GoogleDriveProviderInfo.ID + driveId, () => GoogleDriveProviderInfo.Storage.GetEntries(driveId, false));

            return folders.Concat(files).ToList();
        }

        #region cache

        private static readonly CachedDictionary<DriveFile> CacheEntry =
            new CachedDictionary<DriveFile>("drive-entry", Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20), x => true);

        private static readonly CachedDictionary<List<DriveFile>> CacheChildFiles =
            new CachedDictionary<List<DriveFile>>("drive-files", Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20), x => true);

        private static readonly CachedDictionary<List<DriveFile>> CacheChildFolders =
            new CachedDictionary<List<DriveFile>>("drive-folders", Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20), x => true);

        protected void CacheReset(string driveId = null, bool? childFolder = null)
        {
            if (driveId == null)
            {
                CacheEntry.Clear();
                CacheChildFiles.Clear();
                CacheChildFolders.Clear();
            }
            else
            {
                if (driveId == GoogleDriveProviderInfo.DriveRootId) driveId = "root";
                CacheEntry.Reset(GoogleDriveProviderInfo.ID + driveId);
                CacheResetChilds(driveId, childFolder);
            }
        }

        protected void CacheResetChilds(string parentDriveId, bool? childFolder = null)
        {
            if (!childFolder.HasValue || !childFolder.Value)
                CacheChildFiles.Reset(GoogleDriveProviderInfo.ID + parentDriveId);

            if (!childFolder.HasValue || childFolder.Value)
                CacheChildFolders.Reset(GoogleDriveProviderInfo.ID + parentDriveId);
        }

        protected void CacheInsert(DriveFile driveEntry)
        {
            CacheEntry.Add(GoogleDriveProviderInfo.ID + driveEntry.Id, driveEntry);
        }

        #endregion
    }
}