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


using ASC.Common.Caching;
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
using ASC.Web.Files.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DriveFile = Google.Apis.Drive.v2.Data.File;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal abstract class GoogleDriveDaoBase : IDisposable
    {
        private static readonly ConcurrentDictionary<string, DriveFile> CacheEntry = new ConcurrentDictionary<string, DriveFile>();
        private static readonly ConcurrentDictionary<string, List<DriveFile>> CacheChildFiles = new ConcurrentDictionary<string, List<DriveFile>>();
        private static readonly ConcurrentDictionary<string, List<DriveFile>> CacheChildFolders = new ConcurrentDictionary<string, List<DriveFile>>();
        private static readonly ICacheNotify CacheNotify;

        protected readonly GoogleDriveDaoSelector GoogleDriveDaoSelector;

        public int TenantID { get; private set; }
        public GoogleDriveProviderInfo GoogleDriveProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }


        static GoogleDriveDaoBase()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<GoogleDriveCacheItem>((i, o) =>
            {
                if (i.ResetEntry)
                {
                    DriveFile d;
                    CacheEntry.TryRemove(i.Key, out d);
                }
                if (i.ResetAll)
                {
                    CacheEntry.Clear();
                    CacheChildFiles.Clear();
                    CacheChildFolders.Clear();
                }
                if (i.ResetChilds)
                {
                    List<DriveFile> value;
                    if (!i.ChildFolder.HasValue || !i.ChildFolder.Value)
                    {
                        CacheChildFiles.TryRemove(i.Key, out value);
                    }
                    if (!i.ChildFolder.HasValue || i.ChildFolder.Value)
                    {
                        CacheChildFolders.TryRemove(i.Key, out value);
                    }
                }
            });
        }


        protected GoogleDriveDaoBase(GoogleDriveDaoSelector.GoogleDriveInfo googleDriveInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
        {
            GoogleDriveProviderInfo = googleDriveInfo.GoogleDriveProviderInfo;
            PathPrefix = googleDriveInfo.PathPrefix;
            GoogleDriveDaoSelector = googleDriveDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            GoogleDriveProviderInfo.Storage.Close();
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

            return Global.ReplaceInvalidCharsAndTruncate(driveFolder.Title);
        }

        protected String MakeFileTitle(DriveFile driveFile)
        {
            if (driveFile == null || string.IsNullOrEmpty(driveFile.Title))
            {
                return GoogleDriveProviderInfo.ProviderKey;
            }

            var title = driveFile.Title;

            var gExt = MimeMapping.GetExtention(driveFile.MimeType);
            if (GoogleLoginProvider.GoogleDriveExt.Contains(gExt))
            {
                title += gExt;
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
                if (!CacheEntry.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out entry))
                {
                    entry = GoogleDriveProviderInfo.Storage.GetEntry(driveId);
                    CacheEntry.TryAdd(GoogleDriveProviderInfo.ID + driveId, entry);
                }
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
                List<DriveFile> value;
                if (folder.Value)
                {
                    if (!CacheChildFolders.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out value))
                    {
                        value = GoogleDriveProviderInfo.Storage.GetEntries(driveId, true);
                        CacheChildFolders.TryAdd(GoogleDriveProviderInfo.ID + driveId, value);
                    }
                }
                else
                {
                    if (!CacheChildFiles.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out value))
                    {
                        value = GoogleDriveProviderInfo.Storage.GetEntries(driveId, false);
                        CacheChildFolders.TryAdd(GoogleDriveProviderInfo.ID + driveId, value);
                    }
                }
                return value;
            }
            List<DriveFile> value1 = null;
            if (!CacheChildFiles.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out value1) &&
                !CacheChildFolders.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out value1))
            {
                var entries = GoogleDriveProviderInfo.Storage.GetEntries(driveId);

                CacheChildFiles.TryAdd(GoogleDriveProviderInfo.ID + driveId, entries.Where(entry => entry.MimeType != GoogleLoginProvider.GoogleDriveMimeTypeFolder).ToList());
                CacheChildFolders.TryAdd(GoogleDriveProviderInfo.ID + driveId, entries.Where(entry => entry.MimeType == GoogleLoginProvider.GoogleDriveMimeTypeFolder).ToList());

                return entries;
            }

            List<DriveFile> folders = null;
            if (!CacheChildFolders.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out folders))
            {
                folders = GoogleDriveProviderInfo.Storage.GetEntries(driveId, true);
                CacheChildFolders.TryAdd(GoogleDriveProviderInfo.ID + driveId, folders);
            }
            List<DriveFile> files = null;
            if (!CacheChildFiles.TryGetValue(GoogleDriveProviderInfo.ID + driveId, out files))
            {
                files = GoogleDriveProviderInfo.Storage.GetEntries(driveId, false);
                CacheChildFiles.TryAdd(GoogleDriveProviderInfo.ID + driveId, files);
            }
            return folders.Concat(files).ToList();
        }


        protected void CacheInsert(DriveFile driveEntry)
        {
            if (driveEntry != null)
            {
                CacheNotify.Publish(new GoogleDriveCacheItem { ResetEntry = true, Key = GoogleDriveProviderInfo.ID + driveEntry.Id }, CacheNotifyAction.Remove);
            }
        }

        protected void CacheReset(string driveId = null, bool? childFolder = null)
        {
            if (driveId == null)
            {
                CacheNotify.Publish(new GoogleDriveCacheItem { ResetAll = true }, CacheNotifyAction.Remove);
            }
            else
            {
                if (driveId == GoogleDriveProviderInfo.DriveRootId)
                {
                    driveId = "root";
                }
                var key = GoogleDriveProviderInfo.ID + driveId;

                CacheNotify.Publish(new GoogleDriveCacheItem { ResetEntry = true, ResetChilds = true, Key = key, ChildFolder = childFolder }, CacheNotifyAction.Remove);
            }
        }

        protected void CacheResetChilds(string parentDriveId, bool? childFolder = null)
        {
            CacheNotify.Publish(new GoogleDriveCacheItem { ResetChilds = true, Key = GoogleDriveProviderInfo.ID + parentDriveId, ChildFolder = childFolder }, CacheNotifyAction.Remove);
        }



        class GoogleDriveCacheItem
        {
            public bool ResetAll { get; set; }
            public bool ResetEntry { get; set; }
            public bool ResetChilds { get; set; }
            public string Key { get; set; }
            public bool? ChildFolder { get; set; }
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