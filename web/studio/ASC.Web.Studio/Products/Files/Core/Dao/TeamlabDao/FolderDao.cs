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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FullTextIndex;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;
using Newtonsoft.Json.Linq;

namespace ASC.Files.Core.Data
{
    internal class CachedFolderDao : FolderDao
    {
        #region Constructor

        public CachedFolderDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {


        }

        #endregion

        #region Override Methods

        public override object GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            var cachedKey = String.Format("{0}/{1}/{2}/{3}", TenantID, module, bunch, data);

            var folderId = _cache[cachedKey];

            if (folderId != null && !folderId.Equals(0))
                return folderId;

            folderId = base.GetFolderID(module, bunch, data, createIfNotExists);

            if (!folderId.Equals(0))
                _cache.Insert(cachedKey, folderId, null, Cache.NoAbsoluteExpiration,
                              TimeSpan.FromMinutes(15));

            return folderId;
        }

        #endregion
    }

    public class FolderDao : AbstractDao, IFolderDao
    {
        private const string my = "my";
        private const string common = "common";
        private const string share = "share";
        private const string trash = "trash";
        private const string projects = "projects";

        public FolderDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }

        public Folder GetFolder(object folderId)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(GetFolderQuery(Exp.Eq("id", folderId)))
                    .ConvertAll(ToFolder)
                    .SingleOrDefault();
            }
        }

        public Folder GetFolder(String title, object parentId)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(
                        GetFolderQuery(Exp.Eq("title", title) & Exp.Eq("parent_id", parentId)).OrderBy("create_on", true))
                    .ConvertAll(r => ToFolder(r))
                    .FirstOrDefault();
            }
        }

        public Folder GetRootFolder(object folderId)
        {
            var q = new SqlQuery("files_folder_tree")
                .Select("parent_id")
                .Where("folder_id", folderId)
                .SetMaxResults(1)
                .OrderBy("level", false);

            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(GetFolderQuery(Exp.EqColumns("id", q)))
                    .ConvertAll(r => ToFolder(r))
                    .SingleOrDefault();
            }
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            var subq = Query("files_file")
                .Select("folder_id")
                .Where("id", fileId)
                .Distinct();

            var q = new SqlQuery("files_folder_tree")
                .Select("parent_id")
                .Where(Exp.EqColumns("folder_id", subq))
                .SetMaxResults(1)
                .OrderBy("level", false);

            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(GetFolderQuery(Exp.EqColumns("id", q)))
                    .ConvertAll(r => ToFolder(r))
                    .SingleOrDefault();
            }
        }

        public List<Folder> GetFolders(object parentId)
        {
            return GetFolders(parentId, default(OrderBy), default(FilterType), default(Guid), string.Empty);
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            if (filterType == FilterType.FilesOnly) return new List<Folder>();

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var q = GetFolderQuery(Exp.Eq("parent_id", parentId));
            switch (orderBy.SortedBy)
            {
                case SortedByType.Author:
                    q.OrderBy("create_by", orderBy.IsAsc);
                    break;
                case SortedByType.AZ:
                    q.OrderBy("title", orderBy.IsAsc);
                    break;
                case SortedByType.DateAndTime:
                    q.OrderBy("create_on", orderBy.IsAsc);
                    break;
                default:
                    q.OrderBy("title", true);
                    break;
            }

            if (!string.IsNullOrEmpty(searchText))
                q.Where(Exp.Like("lower(title)", searchText.ToLower().Trim()));

            if (filterType == FilterType.ByDepartment || filterType == FilterType.ByUser ||
                filterType == FilterType.DocumentsOnly || filterType == FilterType.ImagesOnly ||
                filterType == FilterType.PresentationsOnly || filterType == FilterType.SpreadsheetsOnly)
            {
                var existsQuery = Query("files_file file")
                    .From("files_folder_tree tree")
                    .Select("file.id")
                    .Where(Exp.EqColumns("file.folder_id", "tree.folder_id"))
                    .Where(Exp.EqColumns("tree.parent_id", "f.id"));
                switch (filterType)
                {
                    case FilterType.DocumentsOnly:
                    case FilterType.ImagesOnly:
                    case FilterType.PresentationsOnly:
                    case FilterType.SpreadsheetsOnly:
                        existsQuery.Where("file.category", (int) filterType);
                        break;
                    case FilterType.ByUser:
                        existsQuery.Where("file.create_by", subjectID.ToString());
                        break;
                    case FilterType.ByDepartment:
                        var users = CoreContext.UserManager.GetUsersByGroup(subjectID).Select(u => u.ID.ToString()).ToArray();
                        existsQuery.Where(Exp.In("file.create_by", users));
                        break;
                }
                q.Where(Exp.Exists(existsQuery));
            }

            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(q)
                    .ConvertAll(r => ToFolder(r));
            }
        }

        public List<Folder> GetFolders(object[] folderIds)
        {
            var q = GetFolderQuery(Exp.In("id", folderIds));
            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(q)
                    .ConvertAll(r => ToFolder(r));
            }
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var q = GetFolderQuery(Exp.Empty)
                .InnerJoin("files_folder_tree t", Exp.EqColumns("id", "t.parent_id"))
                .Where("t.folder_id", folderId)
                .OrderBy("t.level", false);

            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(q)
                    .ConvertAll(r => ToFolder(r));
            }
        }

        public Folder SaveFolder(Folder folder)
        {
            return SaveFolder(folder, null);
        }

        public Folder SaveFolder(Folder folder, DbManager dbManager)
        {
            var ownsManager = false;
            if (folder == null) throw new ArgumentNullException("folder");

            folder.Title = Global.ReplaceInvalidCharsAndTruncate(folder.Title);

            folder.ModifiedOn = TenantUtil.DateTimeNow();
            folder.ModifiedBy = SecurityContext.CurrentAccount.ID;

            if (folder.CreateOn == default(DateTime)) folder.CreateOn = TenantUtil.DateTimeNow();
            if (folder.CreateBy == default(Guid)) folder.CreateBy = SecurityContext.CurrentAccount.ID;
            try
            {
                if (dbManager == null)
                {
                    ownsManager = true;
                    dbManager = GetDbManager();
                }

                using (var tx = dbManager.BeginTransaction(true))
                {
                    if (folder.ID != null && IsExist(folder.ID))
                    {
                        dbManager.ExecuteNonQuery(
                            Update("files_folder")
                                .Set("title", folder.Title)
                                .Set("modified_on", TenantUtil.DateTimeToUtc(folder.ModifiedOn))
                                .Set("modified_by", folder.ModifiedBy.ToString())
                                .Where("id", folder.ID));
                    }
                    else
                    {
                        folder.ID = dbManager.ExecuteScalar<int>(
                            Insert("files_folder")
                                .InColumnValue("id", 0)
                                .InColumnValue("parent_id", folder.ParentFolderID)
                                .InColumnValue("title", folder.Title)
                                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(folder.CreateOn))
                                .InColumnValue("create_by", folder.CreateBy.ToString())
                                .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(folder.ModifiedOn))
                                .InColumnValue("modified_by", folder.ModifiedBy.ToString())
                                .InColumnValue("folder_type", (int) folder.FolderType)
                                .Identity(1, 0, true));

                        //itself link
                        dbManager.ExecuteNonQuery(
                            new SqlInsert("files_folder_tree")
                                .InColumns("folder_id", "parent_id", "level")
                                .Values(folder.ID, folder.ID, 0));

                        //full path to root
                        dbManager.ExecuteNonQuery(
                            new SqlInsert("files_folder_tree")
                                .InColumns("folder_id", "parent_id", "level")
                                .Values(
                                    new SqlQuery("files_folder_tree t")
                                        .Select(folder.ID.ToString(), "t.parent_id", "t.level + 1")
                                        .Where("t.folder_id", folder.ParentFolderID)));
                    }

                    tx.Commit();
                }

                if (!dbManager.InTransaction)
                {
                    RecalculateFoldersCount(dbManager, folder.ID);
                }
            }
            finally
            {
                if (dbManager != null && ownsManager)
                {
                    //If it's our manager - dispose
                    dbManager.Dispose();
                }
            }

            return folder;
        }

        private bool IsExist(object folderId)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager.ExecuteScalar<int>(Query("files_folder").SelectCount().Where(Exp.Eq("id", folderId))) > 0;
            }
        }

        public void DeleteFolder(object folderId)
        {
            if (folderId == null) throw new ArgumentNullException("folderId");

            var id = int.Parse(Convert.ToString(folderId));

            if (id == 0) return;

            using (var DbManager = GetDbManager())
            {
                using (var tx = DbManager.BeginTransaction())
                {
                    var subfolders = DbManager
                        .ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", id))
                        .ConvertAll(r => Convert.ToInt32(r[0]));
                    if (!subfolders.Contains(id)) subfolders.Add(id); // chashed folder_tree

                    var parent = DbManager.ExecuteScalar<int>(Query("files_folder").Select("parent_id").Where("id", id));

                    DbManager.ExecuteNonQuery(Delete("files_folder").Where(Exp.In("id", subfolders)));
                    DbManager.ExecuteNonQuery(new SqlDelete("files_folder_tree").Where(Exp.In("folder_id", subfolders)));
                    DbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", subfolders)).Where("entry_type", FileEntryType.Folder));
                    DbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                    DbManager.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", subfolders)).Where("entry_type", FileEntryType.Folder));

                    tx.Commit();

                    RecalculateFoldersCount(DbManager, parent);
                }
            }
        }

        public void MoveFolder(object id, object toRootFolderId)
        {
            using (var DbManager = GetDbManager())
            {
                using (var tx = DbManager.BeginTransaction())
                {
                    var folder = GetFolder(id);

                    if (folder.FolderType != FolderType.DEFAULT)
                        throw new ArgumentException("It is forbidden to move to the System folder.", "id");

                    var recalcFolders = new List<object> {toRootFolderId};
                    var parent = DbManager.ExecuteScalar<int>(Query("files_folder").Select("parent_id").Where("id", id));
                    if (parent != 0 && !recalcFolders.Contains(parent)) recalcFolders.Add(parent);

                    DbManager.ExecuteNonQuery(
                        Update("files_folder")
                            .Set("parent_id", toRootFolderId)
                            .Set("modified_on", DateTime.UtcNow)
                            .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                            .Where("id", id));

                    var subfolders = DbManager
                        .ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id", "level").Where("parent_id",
                                                                                                          id))
                        .ToDictionary(r => Convert.ToInt32(r[0]), r => Convert.ToInt32(r[1]));

                    DbManager.ExecuteNonQuery(
                        new SqlDelete("files_folder_tree").Where(Exp.In("folder_id", subfolders.Keys) &
                                                                 !Exp.In("parent_id", subfolders.Keys)));

                    foreach (var subfolder in subfolders)
                    {
                        DbManager.ExecuteNonQuery(new SqlInsert("files_folder_tree", true)
                                                      .InColumns("folder_id", "parent_id", "level")
                                                      .Values(new SqlQuery("files_folder_tree")
                                                                  .Select(
                                                                      subfolder.Key.ToString(
                                                                          CultureInfo.InvariantCulture), "parent_id",
                                                                      "level + 1 + " +
                                                                      subfolder.Value.ToString(
                                                                          CultureInfo.InvariantCulture))
                                                                  .Where("folder_id", toRootFolderId)));
                    }

                    tx.Commit();

                    recalcFolders.ForEach(id1 => RecalculateFoldersCount(DbManager, id1));
                    recalcFolders.ForEach(fid => DbManager.ExecuteNonQuery(GetRecalculateFilesCountUpdate(fid)));
                }
            }
        }

        public Folder CopyFolder(object id, object toRootFolderId)
        {
            var folder = GetFolder(id);

            var toFolder = GetFolder(toRootFolderId);

            if (folder.FolderType == FolderType.BUNCH)
                folder.FolderType = FolderType.DEFAULT;

            var copy = new Folder
                {
                    ParentFolderID = toRootFolderId,
                    RootFolderId = toFolder.RootFolderId,
                    RootFolderCreator = toFolder.RootFolderCreator,
                    RootFolderType = toFolder.RootFolderType,
                    Title = folder.Title,
                    FolderType = folder.FolderType
                };

            return SaveFolder(copy);
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            var result = new Dictionary<object, string>();

            using (var DbManager = GetDbManager())
            {
                foreach (var folderId in folderIds)
                {
                    var count = DbManager.ExecuteScalar<int>(new SqlQuery("files_folder_tree").SelectCount().Where("parent_id", folderId).Where("folder_id", to));
                    if (0 < count)
                    {
                        throw new InvalidOperationException(FilesCommonResource.ErrorMassage_FolderCopyError);
                    }

                    var title = DbManager.ExecuteScalar<string>(Query("files_folder").Select("lower(title)").Where("id", folderId));
                    var conflict = DbManager.ExecuteScalar<int>(Query("files_folder").Select("id").Where("lower(title)", title).Where("parent_id", to));
                    if (conflict != 0)
                    {
                        DbManager.ExecuteList(new SqlQuery("files_file f1")
                                                  .InnerJoin("files_file f2", Exp.EqColumns("lower(f1.title)", "lower(f2.title)"))
                                                  .Select("f1.id", "f1.title")
                                                  .Where(Exp.Eq("f1.tenant_id", TenantID) & Exp.Eq("f1.current_version", true) &
                                                         Exp.Eq("f1.folder_id", folderId))
                                                  .Where(Exp.Eq("f2.tenant_id", TenantID) & Exp.Eq("f2.current_version", true) &
                                                         Exp.Eq("f2.folder_id", conflict)))
                                 .ForEach(r => result[Convert.ToInt32(r[0])] = (string) r[1]);

                        var childs = DbManager.ExecuteList(Query("files_folder").Select("id").Where("parent_id", folderId)).ConvertAll(r => r[0]);
                        foreach (var pair in CanMoveOrCopy(childs.ToArray(), conflict))
                        {
                            result.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            return result;
        }

        public object RenameFolder(object folderId, string title)
        {
            using (var DbManager = GetDbManager())
            {
                DbManager.ExecuteNonQuery(
                    Update("files_folder")
                        .Set("title", Global.ReplaceInvalidCharsAndTruncate(title))
                        .Set("modified_on", DateTime.UtcNow)
                        .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                        .Where("id", folderId));
            }
            return folderId;
        }

        public List<object> GetFiles(object parentId, bool withSubfolders)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager.ExecuteList(
                    Query("files_file")
                        .Select("id")
                        .Where(Exp.Eq("folder_id", parentId) & Exp.Eq("current_version", true)))
                                .ConvertAll(r => r[0]);
            }
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filter, Guid subjectId, string searchText)
        {
            if (filter == FilterType.FoldersOnly) return new List<File>();

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var q = GetFileQuery(Exp.Eq("current_version", true) & Exp.Eq("folder_id", parentId));

            switch (orderBy.SortedBy)
            {
                case SortedByType.Author:
                    q.OrderBy("create_by", orderBy.IsAsc);
                    break;
                case SortedByType.Size:
                    q.OrderBy("content_length", orderBy.IsAsc);
                    break;
                case SortedByType.AZ:
                    q.OrderBy("title", orderBy.IsAsc);
                    break;
                case SortedByType.DateAndTime:
                    q.OrderBy("create_on", orderBy.IsAsc);
                    break;
                default:
                    q.OrderBy("title", true);
                    break;
            }

            if (!string.IsNullOrEmpty(searchText))
                q.Where(Exp.Like("lower(title)", searchText.ToLower().Trim()));

            switch (filter)
            {
                case FilterType.DocumentsOnly:
                case FilterType.ImagesOnly:
                case FilterType.PresentationsOnly:
                case FilterType.SpreadsheetsOnly:
                    q.Where("category", (int) filter);
                    break;
                case FilterType.ByUser:
                    q.Where("create_by", subjectId.ToString());
                    break;
                case FilterType.ByDepartment:
                    var users = CoreContext.UserManager.GetUsersByGroup(subjectId).Select(u => u.ID.ToString()).ToArray();
                    q.Where(Exp.In("create_by", users));
                    break;
            }

            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(q)
                    .ConvertAll(r => ToFile(r));
            }
        }

        public int GetItemsCount(object folderId, bool withSubfoldes)
        {
            return GetFoldersCount(folderId, withSubfoldes) +
                   GetFilesCount(folderId, withSubfoldes);
        }

        private int GetFoldersCount(object parentId, bool withSubfoldes)
        {
            var q = new SqlQuery("files_folder_tree").SelectCount().Where("parent_id", parentId);
            if (withSubfoldes)
            {
                q.Where(Exp.Gt("level", 0));
            }
            else
            {
                q.Where("level", 1);
            }
            using (var DbManager = GetDbManager())
            {
                return DbManager.ExecuteScalar<int>(q);
            }
        }

        private int GetFilesCount(object folderId, bool withSubfoldes)
        {
            var q = Query("files_file").SelectCount("distinct id");

            if (withSubfoldes)
            {
                q.Where(Exp.In("folder_id", new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", folderId)));
            }
            else
            {
                q.Where("folder_id", folderId);
            }

            using (var DbManager = GetDbManager())
            {
                return DbManager.ExecuteScalar<int>(q);
            }
        }

        public bool UseTrashForRemove(Folder folder)
        {
            return folder.RootFolderType != FolderType.TRASH && folder.FolderType != FolderType.BUNCH;
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            return true;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            return chunkedUpload ? SetupInfo.MaxChunkedUploadSize : SetupInfo.MaxUploadSize;
        }

        private void RecalculateFoldersCount(DbManager dbManager, object id)
        {
            dbManager.ExecuteNonQuery(
                Update("files_folder")
                    .Set("foldersCount = (select count(*) - 1 from files_folder_tree where parent_id = id)")
                    .Where(Exp.In("id", new SqlQuery("files_folder_tree").Select("parent_id").Where("folder_id", id))));
        }

        #region Only for TMFolderDao



        public IEnumerable<Folder> Search(string text, FolderType folderType)
        {
            if (string.IsNullOrEmpty(text)) return new List<Folder>();

            if (FullTextSearch.SupportModule(FullTextSearch.FileModule))
            {
                var ids = FullTextSearch.Search(text, FullTextSearch.FileModule)
                                        .GetIdentifiers()
                                        .Where(id => !string.IsNullOrEmpty(id) && id[0] == 'd')
                                        .Select(id => int.Parse(id.Substring(1)))
                                        .ToList();
                using (var DbManager = GetDbManager())
                {
                    return DbManager
                        .ExecuteList(GetFolderQuery(Exp.In("id", ids)))
                        .ConvertAll(r => ToFolder(r))
                        .Where(
                            f =>
                            folderType == FolderType.BUNCH
                                ? f.RootFolderType == FolderType.BUNCH
                                : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON);
                }
            }
            else
            {
                var keywords = text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                                   .Where(k => 3 <= k.Trim().Length)
                                   .ToList();
                if (keywords.Count == 0) return new List<Folder>();

                var where = Exp.Empty;
                keywords.ForEach(k => where &= Exp.Like("title", k));
                using (var DbManager = GetDbManager())
                {
                    return DbManager
                        .ExecuteList(GetFolderQuery(where))
                        .ConvertAll(r => ToFolder(r))
                        .Where(
                            f =>
                            folderType == FolderType.BUNCH
                                ? f.RootFolderType == FolderType.BUNCH
                                : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON);
                }
            }
        }

        public virtual IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data, bool createIfNotExists)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
            if (string.IsNullOrEmpty(bunch)) throw new ArgumentNullException("bunch");

            using (var dbManager = GetDbManager())
            {
                var keys = data.Select(id => (object) string.Format("{0}/{1}/{2}", module, bunch, id)).ToArray();

                var folderIdsQuery = new SqlQuery("files_bunch_objects")
                    .Select("left_node", "right_node")
                    .Where(keys.Length > 1 ? Exp.In("right_node", keys) : Exp.Eq("right_node", keys[0]))
                    .Where(Exp.Eq("tenant_id", TenantID));

                var folderIdsDictionary = dbManager.ExecuteList(folderIdsQuery).ToDictionary(r => r[1], r => r[0]);

                var folderIds = new List<object>();

                foreach (var key in keys)
                {
                    object folderId = null;
                    if (createIfNotExists && !folderIdsDictionary.TryGetValue(key, out folderId))
                    {
                        var folder = new Folder {ParentFolderID = 0};
                        switch (bunch)
                        {
                            case my:
                                folder.FolderType = FolderType.USER;
                                folder.Title = my;
                                break;
                            case common:
                                folder.FolderType = FolderType.COMMON;
                                folder.Title = common;
                                break;
                            case trash:
                                folder.FolderType = FolderType.TRASH;
                                folder.Title = trash;
                                break;
                            case share:
                                folder.FolderType = FolderType.SHARE;
                                folder.Title = share;
                                break;
                            case projects:
                                folder.FolderType = FolderType.Projects;
                                folder.Title = projects;
                                break;
                            default:
                                folder.FolderType = FolderType.BUNCH;
                                folder.Title = (string) key;
                                break;
                        }
                        using (var tx = dbManager.BeginTransaction()) //NOTE: Maybe we shouldn't start transaction here at all
                        {
                            folderId = SaveFolder(folder, dbManager).ID; //Save using our db manager

                            dbManager.ExecuteNonQuery(
                                Insert("files_bunch_objects")
                                    .InColumnValue("left_node", folderId)
                                    .InColumnValue("right_node", key));

                            tx.Commit(); //Commit changes
                        }
                    }
                    folderIds.Add(folderId);
                }
                return folderIds;
            }
        }

        public virtual object GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
            if (string.IsNullOrEmpty(bunch)) throw new ArgumentNullException("bunch");

            using (var DbManager = GetDbManager())
            {

                var key = string.Format("{0}/{1}/{2}", module, bunch, data);
                var folderId = DbManager.ExecuteScalar<object>(
                    Query("files_bunch_objects")
                        .Select("left_node")
                        .Where("right_node", key)
                        .Where("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId));

                if (createIfNotExists && folderId == null)
                {
                    var folder = new Folder {ParentFolderID = 0};
                    switch (bunch)
                    {
                        case my:
                            folder.FolderType = FolderType.USER;
                            folder.Title = my;
                            break;
                        case common:
                            folder.FolderType = FolderType.COMMON;
                            folder.Title = common;
                            break;
                        case trash:
                            folder.FolderType = FolderType.TRASH;
                            folder.Title = trash;
                            break;
                        case share:
                            folder.FolderType = FolderType.SHARE;
                            folder.Title = share;
                            break;
                        case projects:
                            folder.FolderType = FolderType.Projects;
                            folder.Title = projects;
                            break;
                        default:
                            folder.FolderType = FolderType.BUNCH;
                            folder.Title = key;
                            break;
                    }
                    using (var tx = DbManager.BeginTransaction()) //NOTE: Maybe we shouldn't start transaction here at all
                    {
                        folderId = SaveFolder(folder, DbManager).ID; //Save using our db manager

                        DbManager.ExecuteNonQuery(
                            Insert("files_bunch_objects")
                                .InColumnValue("left_node", folderId)
                                .InColumnValue("right_node", key));

                        tx.Commit(); //Commit changes
                    }
                }
                return Convert.ToInt32(folderId);
            }
        }


        public object GetFolderIDProjects(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, projects, null, createIfNotExists);
        }

        public object GetFolderIDTrash(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, trash, SecurityContext.CurrentAccount.ID.ToString(), createIfNotExists);
        }

        public object GetFolderIDCommon(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, common, null, createIfNotExists);
        }

        public object GetFolderIDUser(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, my, SecurityContext.CurrentAccount.ID.ToString(), createIfNotExists);
        }

        public object GetFolderIDShare(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, share, null, createIfNotExists);
        }

        #endregion

        protected SqlQuery GetFolderQuery(Exp where)
        {
            return Query("files_folder f")
                .Select("f.id")
                .Select("f.parent_id")
                .Select("f.title")
                .Select("f.create_on")
                .Select("f.create_by")
                .Select("f.modified_on")
                .Select("f.modified_by")
                .Select("f.folder_type")
                .Select("f.foldersCount")
                .Select("f.filesCount")
                .Select(GetRootFolderType("parent_id"))
                .Select(GetSharedQuery(FileEntryType.Folder))
                .Where(@where);
        }

        public String GetBunchObjectID(object folderID)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager.ExecuteScalar<String>(
                    Query("files_bunch_objects")
                        .Select("right_node")
                        .Where(Exp.Eq("left_node", folderID)));
            }
        }

        private String GetProjectTitle(object folderID)
        {
            var cacheKey = "documents/folders/" + folderID.ToString();

            var projectTitle = Convert.ToString(_cache.Get(cacheKey));

            if (!String.IsNullOrEmpty(projectTitle)) return projectTitle;

            var bunchObjectID = GetBunchObjectID(folderID);

            if (String.IsNullOrEmpty(bunchObjectID))
                throw new Exception("Bunch Object id is null for " + folderID);

            if (!bunchObjectID.StartsWith("projects/project/"))
                return String.Empty;

            var bunchObjectIDParts = bunchObjectID.Split('/');

            if (bunchObjectIDParts.Length < 3)
                throw new Exception("Bunch object id is not supported format");

            var projectID = Convert.ToInt32(bunchObjectIDParts[bunchObjectIDParts.Length - 1]);

            if (HttpContext.Current == null)
                return string.Empty;

            var apiServer = new Api.ApiServer();

            var apiUrl = String.Format("{0}project/{1}.json", SetupInfo.WebApiBaseUrl, projectID);

            var responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiServer.GetApiResponse(apiUrl, "GET"))))["response"];

            if (responseApi != null && responseApi.HasValues)
            {
                projectTitle = Global.ReplaceInvalidCharsAndTruncate(responseApi["title"].Value<String>());
            }
            else
            {
                return string.Empty;
            }
            if (!String.IsNullOrEmpty(projectTitle))
                _cache.Insert(cacheKey, projectTitle, null, Cache.NoAbsoluteExpiration,
                              TimeSpan.FromMinutes(15));

            return projectTitle;
        }

        protected Folder ToFolder(object[] r)
        {
            var f = new Folder
                {
                    ID = Convert.ToInt32(r[0]),
                    ParentFolderID = Convert.ToInt32(r[1]),
                    Title = Convert.ToString(r[2]),
                    CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3])),
                    CreateBy = new Guid(r[4].ToString()),
                    ModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                    ModifiedBy = new Guid(r[6].ToString()),
                    FolderType = (FolderType) Convert.ToInt32(r[7]),
                    TotalSubFolders = Convert.ToInt32(r[8]),
                    TotalFiles = Convert.ToInt32(r[9]),
                    RootFolderType = ParseRootFolderType(r[10]),
                    RootFolderCreator = ParseRootFolderCreator(r[10]),
                    RootFolderId = ParseRootFolderId(r[10]),
                    SharedByMe = Convert.ToBoolean(r[11]),
                };
            switch (f.FolderType)
            {
                case FolderType.COMMON:
                    f.Title = FilesUCResource.CorporateFiles;
                    break;
                case FolderType.USER:
                    f.Title = FilesUCResource.MyFiles;
                    break;
                case FolderType.SHARE:
                    f.Title = FilesUCResource.SharedForMe;
                    break;
                case FolderType.TRASH:
                    f.Title = FilesUCResource.Trash;
                    break;
                case FolderType.Projects:
                    f.Title = FilesUCResource.ProjectFiles;
                    break;
                case FolderType.BUNCH:
                    try
                    {
                        f.Title = GetProjectTitle(f.ID);
                    }
                    catch (Exception e)
                    {
                        Global.Logger.Error(e);
                    }
                    break;
            }

            if (f.FolderType != FolderType.DEFAULT && 0.Equals(f.ParentFolderID)) f.RootFolderType = f.FolderType;
            if (f.FolderType != FolderType.DEFAULT && f.RootFolderCreator == default(Guid)) f.RootFolderCreator = f.CreateBy;
            if (f.FolderType != FolderType.DEFAULT && 0.Equals(f.RootFolderId)) f.RootFolderId = f.ID;
            return f;
        }
    }
}