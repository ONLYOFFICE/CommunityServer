/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

using ASC.Api;
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
            return dbManager
                .ExecuteList(GetFolderQuery(Exp.Eq("id", folderId)))
                .ConvertAll(ToFolder)
                .SingleOrDefault();
        }

        public Folder GetFolder(String title, object parentId)
        {
            return dbManager
                .ExecuteList(
                    GetFolderQuery(Exp.Eq("title", title) & Exp.Eq("parent_id", parentId)).OrderBy("create_on", true))
                .ConvertAll(ToFolder)
                .FirstOrDefault();
        }

        public Folder GetRootFolder(object folderId)
        {
            var q = new SqlQuery("files_folder_tree")
                .Select("parent_id")
                .Where("folder_id", folderId)
                .SetMaxResults(1)
                .OrderBy("level", false);

            return dbManager
                .ExecuteList(GetFolderQuery(Exp.EqColumns("id", q)))
                .ConvertAll(ToFolder)
                .SingleOrDefault();
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            var subq = Query("files_file")
                .Select("folder_id")
                .Where(Exp.Eq("id", fileId) & Exp.Eq("current_version", true))
                .Distinct();

            var q = new SqlQuery("files_folder_tree")
                .Select("parent_id")
                .Where(Exp.EqColumns("folder_id", subq))
                .SetMaxResults(1)
                .OrderBy("level", false);

            return dbManager
                .ExecuteList(GetFolderQuery(Exp.EqColumns("id", q)))
                .ConvertAll(ToFolder)
                .SingleOrDefault();
        }

        public List<Folder> GetFolders(object parentId)
        {
            return GetFolders(parentId, default(OrderBy), default(FilterType), default(Guid), string.Empty);
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText, bool withSubfolders = false)
        {
            if (filterType == FilterType.FilesOnly || filterType == FilterType.ByExtension) return new List<Folder>();

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var q = GetFolderQuery(Exp.Eq("parent_id", parentId));

            if (withSubfolders)
            {
                q = GetFolderQuery(Exp.Eq("fft.parent_id", parentId) & !Exp.Eq("fft.level", 0))
                    .InnerJoin("files_folder_tree fft", Exp.EqColumns("fft.folder_id", "f.id"));
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                q.Where(Exp.Like("lower(title)", searchText.ToLower().Trim()));
            }
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

            if (filterType == FilterType.ByDepartment || filterType == FilterType.ByUser ||
                filterType == FilterType.DocumentsOnly || filterType == FilterType.ImagesOnly ||
                filterType == FilterType.PresentationsOnly || filterType == FilterType.SpreadsheetsOnly ||
                filterType == FilterType.ArchiveOnly)
            {
                var selfQuery = Exp.False;
                var existsQuery = Query("files_file file")
                    .From("files_folder_tree tree")
                    .Select("file.id")
                    .Where("file.current_version", true)
                    .Where(Exp.EqColumns("file.folder_id", "tree.folder_id"))
                    .Where(Exp.EqColumns("tree.parent_id", "f.id"));
                switch (filterType)
                {
                    case FilterType.DocumentsOnly:
                    case FilterType.ImagesOnly:
                    case FilterType.PresentationsOnly:
                    case FilterType.SpreadsheetsOnly:
                    case FilterType.ArchiveOnly:
                        existsQuery.Where("file.category", (int)filterType);
                        break;
                    case FilterType.ByUser:
                        selfQuery = Exp.Eq("f.create_by", subjectID.ToString());
                        existsQuery.Where("file.create_by", subjectID.ToString());
                        break;
                    case FilterType.ByDepartment:
                        var users = CoreContext.UserManager.GetUsersByGroup(subjectID).Select(u => u.ID.ToString()).ToArray();
                        selfQuery = Exp.In("f.create_by", users);
                        existsQuery.Where(Exp.In("file.create_by", users));
                        break;
                }
                q.Where(Exp.Or(selfQuery, Exp.Exists(existsQuery)));
            }

            return dbManager
                .ExecuteList(q)
                .ConvertAll(ToFolder);
        }

        public List<Folder> GetFolders(object[] folderIds, string searchText = "", bool searchSubfolders = false, bool checkShare = true)
        {
            var q = GetFolderQuery(Exp.In("id", folderIds), checkShare);

            if (!string.IsNullOrEmpty(searchText))
            {
                var likeExp = Exp.Like("lower(title)", searchText.ToLower().Trim());
                if (searchSubfolders)
                {
                    q = GetFolderQuery(Exp.In("fft.parent_id", folderIds))
                        .InnerJoin("files_folder_tree fft", Exp.EqColumns("fft.folder_id", "f.id"));
                    likeExp = likeExp | Exp.Eq("level", 0);
                }

                q.Where(likeExp);
            }

            return dbManager
                .ExecuteList(q)
                .ConvertAll(ToFolder);
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var q = GetFolderQuery(Exp.Empty)
                .InnerJoin("files_folder_tree t", Exp.EqColumns("id", "t.parent_id"))
                .Where("t.folder_id", folderId)
                .OrderBy("t.level", false);


            return dbManager
                .ExecuteList(q)
                .ConvertAll(ToFolder);
        }

        public object SaveFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");

            folder.Title = Global.ReplaceInvalidCharsAndTruncate(folder.Title);

            folder.ModifiedOn = TenantUtil.DateTimeNow();
            folder.ModifiedBy = SecurityContext.CurrentAccount.ID;

            if (folder.CreateOn == default(DateTime)) folder.CreateOn = TenantUtil.DateTimeNow();
            if (folder.CreateBy == default(Guid)) folder.CreateBy = SecurityContext.CurrentAccount.ID;

            var isnew = false;

            using (var tx = dbManager.BeginTransaction(true))
            {
                if (folder.ID != null && IsExist(folder.ID))
                {
                    dbManager.ExecuteNonQuery(
                        Update("files_folder")
                            .Set("title", folder.Title)
                            .Set("create_by", folder.CreateBy.ToString())
                            .Set("modified_on", TenantUtil.DateTimeToUtc(folder.ModifiedOn))
                            .Set("modified_by", folder.ModifiedBy.ToString())
                            .Where("id", folder.ID));
                }
                else
                {
                    isnew = true;
                    folder.ID = dbManager.ExecuteScalar<int>(
                        Insert("files_folder")
                            .InColumnValue("id", 0)
                            .InColumnValue("parent_id", folder.ParentFolderID)
                            .InColumnValue("title", folder.Title)
                            .InColumnValue("create_on", TenantUtil.DateTimeToUtc(folder.CreateOn))
                            .InColumnValue("create_by", folder.CreateBy.ToString())
                            .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(folder.ModifiedOn))
                            .InColumnValue("modified_by", folder.ModifiedBy.ToString())
                            .InColumnValue("folder_type", (int)folder.FolderType)
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

            if (!dbManager.InTransaction && isnew)
            {
                RecalculateFoldersCount(folder.ID);
            }

            return folder.ID;
        }

        private bool IsExist(object folderId)
        {
            return dbManager.ExecuteScalar<int>(Query("files_folder").SelectCount().Where(Exp.Eq("id", folderId))) > 0;
        }

        public void DeleteFolder(object folderId)
        {
            if (folderId == null) throw new ArgumentNullException("folderId");

            var id = int.Parse(Convert.ToString(folderId));

            if (id == 0) return;

            using (var tx = dbManager.BeginTransaction())
            {
                var subfolders = dbManager
                    .ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", id))
                    .ConvertAll(r => Convert.ToInt32(r[0]));
                if (!subfolders.Contains(id)) subfolders.Add(id); // chashed folder_tree

                var parent = dbManager.ExecuteScalar<int>(Query("files_folder").Select("parent_id").Where("id", id));

                dbManager.ExecuteNonQuery(Delete("files_folder").Where(Exp.In("id", subfolders)));
                dbManager.ExecuteNonQuery(new SqlDelete("files_folder_tree").Where(Exp.In("folder_id", subfolders)));
                dbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", subfolders)).Where("entry_type", (int)FileEntryType.Folder));

                var tagsToRemove = dbManager.ExecuteList(
                    Query("files_tag")
                    .Select("id")
                    .Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))))
                    .ConvertAll(r => Convert.ToInt32(r[0]));

                dbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.In("id", tagsToRemove)));

                dbManager.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", subfolders)).Where("entry_type", (int)FileEntryType.Folder));
                dbManager.ExecuteNonQuery(Delete("files_bunch_objects").Where("left_node", id));

                tx.Commit();

                RecalculateFoldersCount(parent);
            }
        }

        public object MoveFolder(object folderId, object toFolderId)
        {
            using (var tx = dbManager.BeginTransaction())
            {
                var folder = GetFolder(folderId);

                if (folder.FolderType != FolderType.DEFAULT)
                    throw new ArgumentException("It is forbidden to move the System folder.", "folderId");

                var recalcFolders = new List<object> { toFolderId };
                var parent = dbManager.ExecuteScalar<int>(Query("files_folder").Select("parent_id").Where("id", folderId));
                if (parent != 0 && !recalcFolders.Contains(parent)) recalcFolders.Add(parent);

                dbManager.ExecuteNonQuery(
                    Update("files_folder")
                        .Set("parent_id", toFolderId)
                        .Set("modified_on", DateTime.UtcNow)
                        .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                        .Where("id", folderId));

                var subfolders = dbManager
                    .ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id", "level").Where("parent_id",
                                                                                                        folderId))
                    .ToDictionary(r => Convert.ToInt32(r[0]), r => Convert.ToInt32(r[1]));

                dbManager.ExecuteNonQuery(
                    new SqlDelete("files_folder_tree").Where(Exp.In("folder_id", subfolders.Keys) &
                                                                !Exp.In("parent_id", subfolders.Keys)));

                foreach (var subfolder in subfolders)
                {
                    dbManager.ExecuteNonQuery(new SqlInsert("files_folder_tree", true)
                                                    .InColumns("folder_id", "parent_id", "level")
                                                    .Values(new SqlQuery("files_folder_tree")
                                                                .Select(
                                                                    subfolder.Key.ToString(
                                                                        CultureInfo.InvariantCulture), "parent_id",
                                                                    "level + 1 + " +
                                                                    subfolder.Value.ToString(
                                                                        CultureInfo.InvariantCulture))
                                                                .Where("folder_id", toFolderId)));
                }

                tx.Commit();

                recalcFolders.ForEach(RecalculateFoldersCount);
                recalcFolders.ForEach(fid => dbManager.ExecuteNonQuery(GetRecalculateFilesCountUpdate(fid)));
            }
            return folderId;
        }

        public Folder CopyFolder(object folderId, object toFolderId)
        {
            var folder = GetFolder(folderId);

            var toFolder = GetFolder(toFolderId);

            if (folder.FolderType == FolderType.BUNCH)
                folder.FolderType = FolderType.DEFAULT;

            var copy = new Folder
                {
                    ParentFolderID = toFolderId,
                    RootFolderId = toFolder.RootFolderId,
                    RootFolderCreator = toFolder.RootFolderCreator,
                    RootFolderType = toFolder.RootFolderType,
                    Title = folder.Title,
                    FolderType = folder.FolderType
                };

            return GetFolder(SaveFolder(copy));
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            var result = new Dictionary<object, string>();

            foreach (var folderId in folderIds)
            {
                var count = dbManager.ExecuteScalar<int>(new SqlQuery("files_folder_tree").SelectCount().Where("parent_id", folderId).Where("folder_id", to));
                if (0 < count)
                {
                    throw new InvalidOperationException(FilesCommonResource.ErrorMassage_FolderCopyError);
                }

                var title = dbManager.ExecuteScalar<string>(Query("files_folder").Select("lower(title)").Where("id", folderId));
                var conflict = dbManager.ExecuteScalar<int>(Query("files_folder").Select("id").Where("lower(title)", title).Where("parent_id", to));
                if (conflict != 0)
                {
                    dbManager.ExecuteList(new SqlQuery("files_file f1")
                                                .InnerJoin("files_file f2", Exp.EqColumns("lower(f1.title)", "lower(f2.title)"))
                                                .Select("f1.id", "f1.title")
                                                .Where(Exp.Eq("f1.tenant_id", TenantID) & Exp.Eq("f1.current_version", true) &
                                                        Exp.Eq("f1.folder_id", folderId))
                                                .Where(Exp.Eq("f2.tenant_id", TenantID) & Exp.Eq("f2.current_version", true) &
                                                        Exp.Eq("f2.folder_id", conflict)))
                                .ForEach(r => result[Convert.ToInt32(r[0])] = (string)r[1]);

                    var childs = dbManager.ExecuteList(Query("files_folder").Select("id").Where("parent_id", folderId)).ConvertAll(r => r[0]);
                    foreach (var pair in CanMoveOrCopy(childs.ToArray(), conflict))
                    {
                        result.Add(pair.Key, pair.Value);
                    }
                }
            }

            return result;
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            dbManager.ExecuteNonQuery(
                Update("files_folder")
                    .Set("title", Global.ReplaceInvalidCharsAndTruncate(newTitle))
                    .Set("modified_on", DateTime.UtcNow)
                    .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                    .Where("id", folder.ID));
            return folder.ID;
        }

        public int GetItemsCount(object folderId)
        {
            return GetFoldersCount(folderId) +
                   GetFilesCount(folderId);
        }

        private int GetFoldersCount(object parentId)
        {
            var q = new SqlQuery("files_folder_tree").SelectCount().Where("parent_id", parentId)
                .Where(Exp.Gt("level", 0));

            return dbManager.ExecuteScalar<int>(q);
        }

        private int GetFilesCount(object folderId)
        {
            var q = Query("files_file").SelectCount("distinct id")
                .Where(Exp.In("folder_id", new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", folderId)));

            return dbManager.ExecuteScalar<int>(q);
        }

        public bool IsEmpty(object folderId)
        {
            return GetItemsCount(folderId) == 0;
        }

        public bool UseTrashForRemove(Folder folder)
        {
            return folder.RootFolderType != FolderType.TRASH && folder.FolderType != FolderType.BUNCH;
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            return true;
        }

        public bool CanCalculateSubitems(object entryId)
        {
            return true;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            var tmp = long.MaxValue;

            if (CoreContext.Configuration.Personal && SetupInfo.IsVisibleSettings("PersonalMaxSpace"))
                tmp = CoreContext.Configuration.PersonalMaxSpace - Global.GetUserUsedSpace();

            return Math.Min(tmp, chunkedUpload ? SetupInfo.MaxChunkedUploadSize : SetupInfo.MaxUploadSize);
        }

        private void RecalculateFoldersCount(object id)
        {
            dbManager.ExecuteNonQuery(
                Update("files_folder")
                    .Set("foldersCount = (select count(*) - 1 from files_folder_tree where parent_id = id)")
                    .Where(Exp.In("id", new SqlQuery("files_folder_tree").Select("parent_id").Where("folder_id", id))));
        }

        #region Only for TMFolderDao

        public void ReassignFolders(object[] folderIds, Guid newOwnerId)
        {
            dbManager.ExecuteNonQuery(
                Update("files_folder")
                    .Set("create_by", newOwnerId.ToString())
                    .Where(Exp.In("id", folderIds)));
        }

        public IEnumerable<Folder> Search(string text, FolderType folderType)
        {
            return Search(text).Where(f => folderType == f.RootFolderType).ToList();
        }

        public IEnumerable<Folder> Search(string text, FolderType folderType1, FolderType folderType2)
        {
            return Search(text).Where(f => folderType1 == f.RootFolderType || folderType2 == f.RootFolderType).ToList();
        }

        private List<Folder> Search(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<Folder>();

            if (FullTextSearch.SupportModule(FullTextSearch.FileModule))
            {
                var ids = FullTextSearch.Search(FullTextSearch.FileFolderModule.Match(text));

                return dbManager
                    .ExecuteList(GetFolderQuery(Exp.In("id", ids)))
                    .ConvertAll(ToFolder);
            }

            var keywords = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => 3 <= k.Trim().Length)
                .ToList();
            if (keywords.Count == 0) return new List<Folder>();

            var where = Exp.Empty;
            keywords.ForEach(k => @where &= Exp.Like("title", k));
            return dbManager
                .ExecuteList(GetFolderQuery(@where))
                .ConvertAll(ToFolder);
        }

        public virtual IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data, bool createIfNotExists)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
            if (string.IsNullOrEmpty(bunch)) throw new ArgumentNullException("bunch");

            var keys = data.Select(id => (object)string.Format("{0}/{1}/{2}", module, bunch, id)).ToArray();

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
                    var folder = new Folder { ParentFolderID = 0 };
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
                            folder.Title = (string)key;
                            break;
                    }
                    using (var tx = dbManager.BeginTransaction()) //NOTE: Maybe we shouldn't start transaction here at all
                    {
                        folderId = SaveFolder(folder); //Save using our db manager

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

        public virtual object GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
            if (string.IsNullOrEmpty(bunch)) throw new ArgumentNullException("bunch");

            var key = string.Format("{0}/{1}/{2}", module, bunch, data);
            var folderId = dbManager.ExecuteScalar<object>(
                Query("files_bunch_objects")
                    .Select("left_node")
                    .Where("right_node", key)
                    .Where("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId));

            if (createIfNotExists && folderId == null)
            {
                var folder = new Folder { ParentFolderID = 0 };
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
                using (var tx = dbManager.BeginTransaction()) //NOTE: Maybe we shouldn't start transaction here at all
                {
                    folderId = SaveFolder(folder); //Save using our db manager

                    dbManager.ExecuteNonQuery(
                        Insert("files_bunch_objects")
                            .InColumnValue("left_node", folderId)
                            .InColumnValue("right_node", key));

                    tx.Commit(); //Commit changes
                }
            }
            return Convert.ToInt32(folderId);
        }


        public object GetFolderIDProjects(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, projects, null, createIfNotExists);
        }

        public object GetFolderIDTrash(bool createIfNotExists, Guid? userId = null)
        {
            return GetFolderID(FileConstant.ModuleId, trash, (userId ?? SecurityContext.CurrentAccount.ID).ToString(), createIfNotExists);
        }

        public object GetFolderIDCommon(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, common, null, createIfNotExists);
        }

        public object GetFolderIDUser(bool createIfNotExists, Guid? userId = null)
        {
            return GetFolderID(FileConstant.ModuleId, my, (userId ?? SecurityContext.CurrentAccount.ID).ToString(), createIfNotExists);
        }

        public object GetFolderIDShare(bool createIfNotExists)
        {
            return GetFolderID(FileConstant.ModuleId, share, null, createIfNotExists);
        }

        #endregion

        protected SqlQuery GetFolderQuery(Exp where, bool checkShare = true)
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
                .Select(checkShare ? GetSharedQuery(FileEntryType.Folder) : new SqlQuery().Select("1"))
                .Where(@where);
        }

        public String GetBunchObjectID(object folderID)
        {
            return dbManager.ExecuteScalar<String>(
                Query("files_bunch_objects")
                    .Select("right_node")
                    .Where(Exp.Eq("left_node", (folderID ?? string.Empty).ToString())));
        }

        public Dictionary<string, string> GetBunchObjectIDs(List<object> folderIDs)
        {
            return dbManager.ExecuteList(
                Query("files_bunch_objects")
                    .Select("left_node", "right_node")
                    .Where(Exp.In("left_node", folderIDs.Select(folderID => (folderID ?? string.Empty).ToString()).ToList())))
                    .ToDictionary(r=> r[0].ToString(), r=> r[1].ToString());
        }

        private String GetProjectTitle(object folderID)
        {
            if (!ApiServer.Available)
            {
                return string.Empty;
            }

            var cacheKey = "documents/folders/" + folderID.ToString();

            var projectTitle = Convert.ToString(cache.Get<string>(cacheKey));

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

            var apiServer = new ApiServer();

            var apiUrl = String.Format("{0}project/{1}.json?fields=id,title", SetupInfo.WebApiBaseUrl, projectID);

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
            {
                cache.Insert(cacheKey, projectTitle, TimeSpan.FromMinutes(15));
            }
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
                    FolderType = (FolderType)Convert.ToInt32(r[7]),
                    TotalSubFolders = Convert.ToInt32(r[8]),
                    TotalFiles = Convert.ToInt32(r[9]),
                    RootFolderType = ParseRootFolderType(r[10]),
                    RootFolderCreator = ParseRootFolderCreator(r[10]),
                    RootFolderId = ParseRootFolderId(r[10]),
                    Shared = Convert.ToBoolean(r[11]),
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