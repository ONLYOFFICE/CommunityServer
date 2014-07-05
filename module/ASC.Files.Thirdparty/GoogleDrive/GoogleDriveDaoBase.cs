/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using AppLimit.CloudComputing.SharpBox;
using ASC.Core.Tenants;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal abstract class GoogleDriveDaoBase : IDisposable
    {
        protected class ErrorEntry : ICloudDirectoryEntry
        {
            public ErrorEntry(Exception e, object id)
            {
                if (e != null) Error = e.Message;

                Id = String.IsNullOrEmpty(id.ToString()) ? "/" : id.ToString();
            }

            public string Error { get; set; }

            public string Name
            {
                get { return "/"; }
            }

            public string Id { get; private set; }

            public long Length
            {
                get { return 0; }
            }

            public DateTime Modified
            {
                get { return DateTime.UtcNow; }
            }

            public string ParentID
            {
                get { return ""; }
                set { }
            }

            public ICloudDirectoryEntry Parent
            {
                get { return null; }
                set { }
            }

            public ICloudFileDataTransfer GetDataTransferAccessor()
            {
                return null;
            }

            public string GetPropertyValue(string key)
            {
                return null;
            }

            private readonly List<ICloudFileSystemEntry> _entries = new List<ICloudFileSystemEntry>(0);

            public IEnumerator<ICloudFileSystemEntry> GetEnumerator()
            {
                return _entries.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public ICloudFileSystemEntry GetChild(string name)
            {
                return null;
            }

            public ICloudFileSystemEntry GetChild(string name, bool bThrowException)
            {
                if (bThrowException) throw new ArgumentNullException(name);
                return null;
            }

            public ICloudFileSystemEntry GetChild(string idOrName, bool bThrowException, bool firstByNameIfNotFound)
            {
                if (bThrowException) throw new ArgumentNullException(idOrName);
                return null;
            }

            public ICloudFileSystemEntry GetChild(int idx)
            {
                return null;
            }

            public int Count
            {
                get { return 0; }
            }

            public nChildState HasChildrens
            {
                get { return nChildState.HasNoChilds; }
            }
        }

        public DbManager DbManager { get; private set; }

        public int TenantID { get; private set; }

        protected GoogleDriveDaoBase(GoogleDriveDaoSelector.GoogleDriveInfo googleDriveInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
        {
            GoogleDriveProviderInfo = googleDriveInfo.GoogleDriveProviderInfo;
            PathPrefix = googleDriveInfo.PathPrefix;
            GoogleDriveDaoSelector = googleDriveDaoSelector;
            DbManager = new DbManager(FileConstant.DatabaseId);
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        protected object MappingID(object id, bool saveIfNotExist)
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

        protected object MappingID(object id)
        {
            return MappingID(id, false);
        }

        protected void UpdatePathInDB(String oldValue, String newValue)
        {
            if (oldValue.Equals(newValue)) return;

            using (var tx = DbManager.BeginTransaction())
            {
                var oldIDs = DbManager.ExecuteList(Query("files_thirdparty_id_mapping")
                                                       .Select("id")
                                                       .Where(Exp.Like("id", oldValue, SqlLike.StartWith)))
                                      .ConvertAll(x => x[0].ToString());

                foreach (var oldID in oldIDs)
                {
                    var oldHashID = MappingID(oldID);
                    var newID = oldID.Replace(oldValue, newValue);
                    var newHashID = MappingID(newID);

                    DbManager.ExecuteNonQuery(Update("files_thirdparty_id_mapping")
                                                  .Set("id", newID)
                                                  .Set("hash_id", newHashID)
                                                  .Where(Exp.Eq("hash_id", oldHashID)));

                    DbManager.ExecuteNonQuery(Update("files_security")
                                                  .Set("entry_id", newHashID)
                                                  .Where(Exp.Eq("entry_id", oldHashID)));

                    DbManager.ExecuteNonQuery(Update("files_tag_link")
                                                  .Set("entry_id", newHashID)
                                                  .Where(Exp.Eq("entry_id", oldHashID)));
                }

                tx.Commit();
            }
        }

        protected readonly GoogleDriveDaoSelector GoogleDriveDaoSelector;
        public GoogleDriveProviderInfo GoogleDriveProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }

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

        protected SqlUpdate Update(string table)
        {
            return new SqlUpdate(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected string GetTenantColumnName(string table)
        {
            var tenant = "tenant_id";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ")).Trim() + "." + tenant;
        }

        protected string MakePath(object entryId)
        {
            return string.Format("/{0}", Convert.ToString(entryId, CultureInfo.InvariantCulture).TrimStart('/').TrimEnd('/'));
        }

        protected string MakeId(ICloudFileSystemEntry entry)
        {
            var path = string.Empty;
            if (entry != null && !(entry is ErrorEntry))
            {
                path = GoogleDriveProviderInfo.Storage.GetFileSystemObjectPath(entry);
            }
            else if (entry != null)
            {
                path = entry.Id;
            }

            return string.Format("{0}{1}", PathPrefix, string.IsNullOrEmpty(path) || path == "/" ? "" : ("-" + path.Replace('/', '|')));
        }

        protected String MakeTitle(ICloudFileSystemEntry fsEntry)
        {
            if (fsEntry is ICloudDirectoryEntry && IsRoot(fsEntry as ICloudDirectoryEntry))
            {
                return GoogleDriveProviderInfo.CustomerTitle;
            }

            return Web.Files.Classes.Global.ReplaceInvalidCharsAndTruncate(fsEntry.Name);
        }

        protected string PathParent(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var index = path.TrimEnd('/').LastIndexOf('/');
                if (index != -1)
                {
                    //Cut to it
                    return path.Substring(0, index);
                }
            }
            return path;
        }

        public void Dispose()
        {
            DbManager.Dispose();
            GoogleDriveProviderInfo.Storage.Close();
        }

        protected Folder ToFolder(ICloudDirectoryEntry fsEntry)
        {
            if (fsEntry == null) return null;
            if (fsEntry is ErrorEntry)
            {
                //Return error entry
                return ToErrorFolder(fsEntry as ErrorEntry);
            }

            //var childFoldersCount = fsEntry.OfType<ICloudDirectoryEntry>().Count();//NOTE: Removed due to performance isssues
            var isRoot = IsRoot(fsEntry);

            var folder = new Folder
                {
                    ID = MakeId(fsEntry),
                    ParentFolderID = isRoot ? null : MakeId(fsEntry.Parent),
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = isRoot ? GoogleDriveProviderInfo.CreateOn : fsEntry.Modified,
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = isRoot ? GoogleDriveProviderInfo.CreateOn : fsEntry.Modified,
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(RootFolder()),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,

                    Shareable = false,
                    Title = MakeTitle(fsEntry),
                    TotalFiles = 0, /*fsEntry.Count - childFoldersCount NOTE: Removed due to performance isssues*/
                    TotalSubFolders = 0, /*childFoldersCount NOTE: Removed due to performance isssues*/
                };

            if (folder.CreateOn != DateTime.MinValue && folder.CreateOn.Kind == DateTimeKind.Utc)
                folder.CreateOn = TenantUtil.DateTimeFromUtc(folder.CreateOn);

            if (folder.ModifiedOn != DateTime.MinValue && folder.ModifiedOn.Kind == DateTimeKind.Utc)
                folder.ModifiedOn = TenantUtil.DateTimeFromUtc(folder.ModifiedOn);

            return folder;
        }

        private bool IsRoot(ICloudDirectoryEntry entry)
        {
            if (entry != null && entry.Name != null)
                return string.IsNullOrEmpty(entry.Name.Trim('/'));
            return false;
        }

        private Files.Core.File ToErrorFile(ErrorEntry fsEntry)
        {
            return new Files.Core.File
                {
                    ID = MakeId(fsEntry),
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = fsEntry.Modified,
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = fsEntry.Modified,
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(RootFolder()),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    Title = MakeTitle(fsEntry),
                    Error = fsEntry.Error
                };
        }

        private Folder ToErrorFolder(ErrorEntry fsEntry)
        {
            ICloudDirectoryEntry rootFolder = null;
            try
            {
                rootFolder = RootFolder();
            }
            catch (Exception)
            {
            }

            return new Folder
                {
                    ID = MakeId(fsEntry),
                    ParentFolderID = null,
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = fsEntry.Modified,
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = fsEntry.Modified,
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    RootFolderId = MakeId(rootFolder),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    Shareable = false,
                    Title = MakeTitle(fsEntry),
                    TotalFiles = fsEntry.Count - 0,
                    TotalSubFolders = 0,
                    Error = fsEntry.Error
                };
        }

        protected Files.Core.File ToFile(ICloudFileSystemEntry fsEntry)
        {
            if (fsEntry == null) return null;

            if (fsEntry is ErrorEntry)
            {
                //Return error entry
                return ToErrorFile(fsEntry as ErrorEntry);
            }

            return new Files.Core.File
                {
                    ID = MakeId(fsEntry),
                    Access = FileShare.None,
                    ContentLength = fsEntry.Length,
                    CreateBy = GoogleDriveProviderInfo.Owner,
                    CreateOn = fsEntry.Modified.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(fsEntry.Modified) : fsEntry.Modified,
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(fsEntry.Parent),
                    ModifiedBy = GoogleDriveProviderInfo.Owner,
                    ModifiedOn = fsEntry.Modified.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(fsEntry.Modified) : fsEntry.Modified,
                    NativeAccessor = fsEntry,
                    ProviderId = GoogleDriveProviderInfo.ID,
                    ProviderKey = GoogleDriveProviderInfo.ProviderKey,
                    Title = MakeTitle(fsEntry),
                    RootFolderId = MakeId(RootFolder()),
                    RootFolderType = GoogleDriveProviderInfo.RootFolderType,
                    RootFolderCreator = GoogleDriveProviderInfo.Owner,
                    SharedByMe = false,
                    Version = 1
                };
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(RootFolder());
        }

        protected ICloudDirectoryEntry RootFolder()
        {
            return GoogleDriveProviderInfo.Storage.GetRoot();
        }

        protected ICloudDirectoryEntry GetFolderById(object folderId)
        {
            ICloudDirectoryEntry entry = null;
            Exception e = null;
            try
            {
                entry = GoogleDriveProviderInfo.Storage.GetFolder(MakePath(folderId));
            }
            catch (Exception ex)
            {
                e = ex;
            }
            if (entry == null)
            {
                //Create error entry
                entry = new ErrorEntry(e, folderId);
            }

            return entry;
        }

        protected ICloudFileSystemEntry GetFileById(object fileId)
        {
            ICloudFileSystemEntry entry = null;
            Exception e = null;
            try
            {
                entry = GoogleDriveProviderInfo.Storage.GetFile(MakePath(fileId), RootFolder());
            }
            catch (Exception ex)
            {
                e = ex;
            }
            if (entry == null)
            {
                //Create error entry
                entry = new ErrorEntry(e, fileId);
            }

            return entry;
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderFiles(object folderId)
        {
            return GetFolderFiles(GoogleDriveProviderInfo.Storage.GetFolder(MakePath(folderId)));
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderSubfolders(object folderId)
        {
            return GetFolderSubfolders(GoogleDriveProviderInfo.Storage.GetFolder(MakePath(folderId)));
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderFiles(ICloudDirectoryEntry folder)
        {
            return folder.Where(x => !(x is ICloudDirectoryEntry));
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderSubfolders(ICloudDirectoryEntry folder)
        {
            return folder.Where(x => (x is ICloudDirectoryEntry));
        }

        protected String GetAvailableTitle(String requestTitle, ICloudDirectoryEntry parentFolderID, Func<string, ICloudDirectoryEntry, bool> isExist)
        {
            if (!isExist(requestTitle, parentFolderID)) return requestTitle;

            var re = new Regex(@"( \(((?<index>[0-9])+)\)(\.[^\.]*)?)$");
            var match = re.Match(requestTitle);

            if (!match.Success)
            {
                var insertIndex = requestTitle.Length;
                if (requestTitle.LastIndexOf(".") != -1)
                {
                    insertIndex = requestTitle.LastIndexOf(".");
                }
                requestTitle = requestTitle.Insert(insertIndex, " (1)");
            }

            while (isExist(requestTitle, parentFolderID))
            {
                requestTitle = re.Replace(requestTitle, MatchEvaluator);
            }
            return requestTitle;
        }

        private static String MatchEvaluator(Match match)
        {
            var index = Convert.ToInt32(match.Groups[2].Value);
            var staticText = match.Value.Substring(String.Format(" ({0})", index).Length);
            return String.Format(" ({0}){1}", index + 1, staticText);
        }
    }
}