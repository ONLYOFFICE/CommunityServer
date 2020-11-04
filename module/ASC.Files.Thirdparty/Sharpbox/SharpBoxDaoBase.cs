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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;

namespace ASC.Files.Thirdparty.Sharpbox
{
    internal abstract class SharpBoxDaoBase : IDisposable
    {
        protected class ErrorEntry : ICloudDirectoryEntry
        {
            public ErrorEntry(Exception e, object id)
            {
                if (e != null) Error = e.Message;

                Id = String.IsNullOrEmpty((id ?? "").ToString()) ? "/" : (id ?? "").ToString();
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

        public int TenantID { get; private set; }

        protected SharpBoxDaoBase(SharpBoxDaoSelector.SharpBoxInfo sharpBoxInfo, SharpBoxDaoSelector sharpBoxDaoSelector)
        {
            SharpBoxProviderInfo = sharpBoxInfo.SharpBoxProviderInfo;
            PathPrefix = sharpBoxInfo.PathPrefix;
            SharpBoxDaoSelector = sharpBoxDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            SharpBoxProviderInfo.Dispose();
        }

        protected DbManager GetDb()
        {
            return new DbManager(FileConstant.DatabaseId);
        }

        protected object MappingID(object id, bool saveIfNotExist)
        {
            if (id == null) return null;
            int n;

            var isNumeric = int.TryParse(id.ToString(), out n);

            if (isNumeric) return n;

            object result;
            using (var db = GetDb())
            {
                if (id.ToString().StartsWith("sbox"))
                    result = Regex.Replace(BitConverter.ToString(Hasher.Hash(id.ToString(), HashAlg.MD5)), "-", "").ToLower();
                else
                    result = db.ExecuteScalar<String>(Query("files_thirdparty_id_mapping")
                                                                 .Select("id")
                                                                 .Where(Exp.Eq("hash_id", id)));

                if (saveIfNotExist)
                    db.ExecuteNonQuery(Insert("files_thirdparty_id_mapping")
                                                  .InColumnValue("id", id)
                                                  .InColumnValue("hash_id", result));
            }
            return result;
        }

        protected object MappingID(object id)
        {
            return MappingID(id, false);
        }

        protected void UpdatePathInDB(String oldValue, String newValue)
        {
            if (oldValue.Equals(newValue)) return;

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var oldIDs = db.ExecuteList(Query("files_thirdparty_id_mapping")
                                                       .Select("id")
                                                       .Where(Exp.Like("id", oldValue, SqlLike.StartWith)))
                                      .ConvertAll(x => x[0].ToString());

                foreach (var oldID in oldIDs)
                {
                    var oldHashID = MappingID(oldID);
                    var newID = oldID.Replace(oldValue, newValue);
                    var newHashID = MappingID(newID);

                    db.ExecuteNonQuery(Update("files_thirdparty_id_mapping")
                                                  .Set("id", newID)
                                                  .Set("hash_id", newHashID)
                                                  .Where(Exp.Eq("hash_id", oldHashID)));

                    db.ExecuteNonQuery(Update("files_security")
                                                  .Set("entry_id", newHashID)
                                                  .Where(Exp.Eq("entry_id", oldHashID)));

                    db.ExecuteNonQuery(Update("files_tag_link")
                                                  .Set("entry_id", newHashID)
                                                  .Where(Exp.Eq("entry_id", oldHashID)));
                }

                tx.Commit();
            }
        }

        protected readonly SharpBoxDaoSelector SharpBoxDaoSelector;
        public SharpBoxProviderInfo SharpBoxProviderInfo { get; private set; }
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
            const string tenant = "tenant_id";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ", StringComparison.InvariantCulture)).Trim() + "." + tenant;
        }

        protected string MakePath(object entryId)
        {
            return string.Format("/{0}", Convert.ToString(entryId, CultureInfo.InvariantCulture).Trim('/'));
        }

        protected string MakeId(ICloudFileSystemEntry entry)
        {
            var path = string.Empty;
            if (entry != null && !(entry is ErrorEntry))
            {
                try
                {
                    path = SharpBoxProviderInfo.Storage.GetFileSystemObjectPath(entry);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Sharpbox makeId error", ex);
                }
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
                return SharpBoxProviderInfo.CustomerTitle;
            }

            return Global.ReplaceInvalidCharsAndTruncate(fsEntry.Name);
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
                CreateBy = SharpBoxProviderInfo.Owner,
                CreateOn = isRoot ? SharpBoxProviderInfo.CreateOn : fsEntry.Modified,
                FolderType = FolderType.DEFAULT,
                ModifiedBy = SharpBoxProviderInfo.Owner,
                ModifiedOn = isRoot ? SharpBoxProviderInfo.CreateOn : fsEntry.Modified,
                ProviderId = SharpBoxProviderInfo.ID,
                ProviderKey = SharpBoxProviderInfo.ProviderKey,
                RootFolderCreator = SharpBoxProviderInfo.Owner,
                RootFolderId = MakeId(RootFolder()),
                RootFolderType = SharpBoxProviderInfo.RootFolderType,

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

        private static bool IsRoot(ICloudDirectoryEntry entry)
        {
            if (entry != null && entry.Name != null)
                return string.IsNullOrEmpty(entry.Name.Trim('/'));
            return false;
        }

        private File ToErrorFile(ErrorEntry fsEntry)
        {
            if (fsEntry == null) return null;

            return new File
            {
                ID = MakeId(fsEntry),
                CreateBy = SharpBoxProviderInfo.Owner,
                CreateOn = fsEntry.Modified,
                ModifiedBy = SharpBoxProviderInfo.Owner,
                ModifiedOn = fsEntry.Modified,
                ProviderId = SharpBoxProviderInfo.ID,
                ProviderKey = SharpBoxProviderInfo.ProviderKey,
                RootFolderCreator = SharpBoxProviderInfo.Owner,
                RootFolderId = MakeId(null),
                RootFolderType = SharpBoxProviderInfo.RootFolderType,
                Title = MakeTitle(fsEntry),
                Error = fsEntry.Error
            };
        }

        private Folder ToErrorFolder(ErrorEntry fsEntry)
        {
            if (fsEntry == null) return null;

            return new Folder
            {
                ID = MakeId(fsEntry),
                ParentFolderID = null,
                CreateBy = SharpBoxProviderInfo.Owner,
                CreateOn = fsEntry.Modified,
                FolderType = FolderType.DEFAULT,
                ModifiedBy = SharpBoxProviderInfo.Owner,
                ModifiedOn = fsEntry.Modified,
                ProviderId = SharpBoxProviderInfo.ID,
                ProviderKey = SharpBoxProviderInfo.ProviderKey,
                RootFolderCreator = SharpBoxProviderInfo.Owner,
                RootFolderId = MakeId(null),
                RootFolderType = SharpBoxProviderInfo.RootFolderType,
                Shareable = false,
                Title = MakeTitle(fsEntry),
                TotalFiles = 0,
                TotalSubFolders = 0,
                Error = fsEntry.Error
            };
        }

        protected File ToFile(ICloudFileSystemEntry fsEntry)
        {
            if (fsEntry == null) return null;

            if (fsEntry is ErrorEntry)
            {
                //Return error entry
                return ToErrorFile(fsEntry as ErrorEntry);
            }

            return new File
            {
                ID = MakeId(fsEntry),
                Access = FileShare.None,
                ContentLength = fsEntry.Length,
                CreateBy = SharpBoxProviderInfo.Owner,
                CreateOn = fsEntry.Modified.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(fsEntry.Modified) : fsEntry.Modified,
                FileStatus = FileStatus.None,
                FolderID = MakeId(fsEntry.Parent),
                ModifiedBy = SharpBoxProviderInfo.Owner,
                ModifiedOn = fsEntry.Modified.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(fsEntry.Modified) : fsEntry.Modified,
                NativeAccessor = fsEntry,
                ProviderId = SharpBoxProviderInfo.ID,
                ProviderKey = SharpBoxProviderInfo.ProviderKey,
                Title = MakeTitle(fsEntry),
                RootFolderId = MakeId(RootFolder()),
                RootFolderType = SharpBoxProviderInfo.RootFolderType,
                RootFolderCreator = SharpBoxProviderInfo.Owner,
                Shared = false,
                Version = 1
            };
        }

        protected ICloudDirectoryEntry RootFolder()
        {
            return SharpBoxProviderInfo.Storage.GetRoot();
        }

        protected ICloudDirectoryEntry GetFolderById(object folderId)
        {
            try
            {
                var path = MakePath(folderId);
                return path == "/"
                           ? RootFolder()
                           : SharpBoxProviderInfo.Storage.GetFolder(path);
            }
            catch (SharpBoxException sharpBoxException)
            {
                if (sharpBoxException.ErrorCode == SharpBoxErrorCodes.ErrorFileNotFound)
                {
                    return null;
                }
                return new ErrorEntry(sharpBoxException, folderId);
            }
            catch (Exception ex)
            {
                return new ErrorEntry(ex, folderId);
            }
        }

        protected ICloudFileSystemEntry GetFileById(object fileId)
        {
            try
            {
                return SharpBoxProviderInfo.Storage.GetFile(MakePath(fileId), null);
            }
            catch (SharpBoxException sharpBoxException)
            {
                if (sharpBoxException.ErrorCode == SharpBoxErrorCodes.ErrorFileNotFound)
                {
                    return null;
                }
                return new ErrorEntry(sharpBoxException, fileId);
            }
            catch (Exception ex)
            {
                return new ErrorEntry(ex, fileId);
            }
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderFiles(object folderId)
        {
            return GetFolderFiles(SharpBoxProviderInfo.Storage.GetFolder(MakePath(folderId)));
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderSubfolders(object folderId)
        {
            return GetFolderSubfolders(SharpBoxProviderInfo.Storage.GetFolder(MakePath(folderId)));
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderFiles(ICloudDirectoryEntry folder)
        {
            return folder.Where(x => !(x is ICloudDirectoryEntry));
        }

        protected IEnumerable<ICloudFileSystemEntry> GetFolderSubfolders(ICloudDirectoryEntry folder)
        {
            return folder.Where(x => (x is ICloudDirectoryEntry));
        }

        protected String GetAvailableTitle(String requestTitle, ICloudDirectoryEntry parentFolder, Func<string, ICloudDirectoryEntry, bool> isExist)
        {
            if (!isExist(requestTitle, parentFolder)) return requestTitle;

            var re = new Regex(@"( \(((?<index>[0-9])+)\)(\.[^\.]*)?)$");
            var match = re.Match(requestTitle);

            if (!match.Success)
            {
                var insertIndex = requestTitle.Length;
                if (requestTitle.LastIndexOf(".", StringComparison.InvariantCulture) != -1)
                {
                    insertIndex = requestTitle.LastIndexOf(".", StringComparison.InvariantCulture);
                }
                requestTitle = requestTitle.Insert(insertIndex, " (1)");
            }

            while (isExist(requestTitle, parentFolder))
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