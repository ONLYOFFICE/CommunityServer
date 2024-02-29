/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Configuration;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;

namespace ASC.Files.Core.Security
{
    public class FileSecurity : IFileSecurity
    {
        private readonly IDaoFactory daoFactory;

        public static bool IsAdministrator(Guid userId)
        {
            return CoreContext.UserManager.IsUserInGroup(userId, Constants.GroupAdmin.ID) ||
                   WebItemSecurity.IsProductAdministrator(ProductEntryPoint.ID, userId);
        }

        public FileShare DefaultMyShare
        {
            get { return FileShare.Restrict; }
        }

        public FileShare DefaultProjectsShare
        {
            get { return FileShare.ReadWrite; }
        }

        public FileShare DefaultCommonShare
        {
            get { return FileShare.Read; }
        }

        public FileShare DefaultPrivacyShare
        {
            get { return FileShare.Restrict; }
        }

        public FileSecurity(IDaoFactory daoFactory)
        {
            this.daoFactory = daoFactory;
        }

        public List<Tuple<FileEntry, bool>> CanRead(IEnumerable<FileEntry> entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Read);
        }

        public bool CanRead(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Read);
        }

        public bool CanComment(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Comment);
        }

        public bool CanFillForms(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.FillForms);
        }

        public bool CanReview(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Review);
        }

        public bool CanCustomFilterEdit(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.CustomFilter);
        }

        public bool CanCreate(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Create);
        }

        public bool CanEdit(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Edit);
        }

        public bool CanDelete(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, FilesSecurityActions.Delete);
        }

        public bool CanDownload(FileEntry entry, Guid userId)
        {
            if (!CanRead(entry, userId))
            {
                return false;
            }

            return CheckDenyDownload(entry);
        }

        public bool CanShare(FileEntry entry, Guid userId)
        {
            if (!CanEdit(entry, userId))
            {
                return false;
            }

            return CheckDenySharing(entry);
        }

        public bool CanRead(FileEntry entry)
        {
            return CanRead(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanComment(FileEntry entry)
        {
            return CanComment(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanCustomFilterEdit(FileEntry entry)
        {
            return CanCustomFilterEdit(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanFillForms(FileEntry entry)
        {
            return CanFillForms(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanReview(FileEntry entry)
        {
            return CanReview(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanCreate(FileEntry entry)
        {
            return CanCreate(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanEdit(FileEntry entry)
        {
            return CanEdit(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanDelete(FileEntry entry)
        {
            return CanDelete(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanDownload(FileEntry entry)
        {
            return CanDownload(entry, SecurityContext.CurrentAccount.ID);
        }

        public bool CanShare(FileEntry entry)
        {
            return CanShare(entry, SecurityContext.CurrentAccount.ID);
        }

        public IEnumerable<Guid> WhoCanRead(FileEntry entry)
        {
            return WhoCan(entry, FilesSecurityActions.Read);
        }

        private IEnumerable<Guid> WhoCan(FileEntry entry, FilesSecurityActions action)
        {
            var copyshares = GetShares(entry);
            IEnumerable<FileShareRecord> shares = copyshares.ToList();

            FileShareRecord defaultShareRecord;

            switch (entry.RootFolderType)
            {
                case FolderType.COMMON:
                    defaultShareRecord = new FileShareRecord
                    {
                        Level = int.MaxValue,
                        EntryId = entry.ID,
                        EntryType = entry.FileEntryType,
                        Share = DefaultCommonShare,
                        Subject = Constants.GroupEveryone.ID,
                        Tenant = TenantProvider.CurrentTenantID,
                        Owner = SecurityContext.CurrentAccount.ID
                    };

                    if (!shares.Any())
                    {
                        if ((defaultShareRecord.Share == FileShare.Read && action == FilesSecurityActions.Read) ||
                            (defaultShareRecord.Share == FileShare.ReadWrite))
                            return CoreContext.UserManager.GetUsersByGroup(defaultShareRecord.Subject)
                                              .Where(x => x.Status == EmployeeStatus.Active).Select(y => y.ID).Distinct();

                        return Enumerable.Empty<Guid>();
                    }

                    break;

                case FolderType.USER:
                    defaultShareRecord = new FileShareRecord
                    {
                        Level = int.MaxValue,
                        EntryId = entry.ID,
                        EntryType = entry.FileEntryType,
                        Share = DefaultMyShare,
                        Subject = entry.RootFolderCreator,
                        Tenant = TenantProvider.CurrentTenantID,
                        Owner = entry.RootFolderCreator
                    };

                    if (!shares.Any())
                        return new List<Guid>
                            {
                                entry.RootFolderCreator
                            };

                    break;

                case FolderType.Privacy:
                    defaultShareRecord = new FileShareRecord
                    {
                        Level = int.MaxValue,
                        EntryId = entry.ID,
                        EntryType = entry.FileEntryType,
                        Share = DefaultPrivacyShare,
                        Subject = entry.RootFolderCreator,
                        Tenant = TenantProvider.CurrentTenantID,
                        Owner = entry.RootFolderCreator
                    };

                    if (!shares.Any())
                        return new List<Guid>
                            {
                                entry.RootFolderCreator
                            };

                    break;

                case FolderType.BUNCH:
                    if (action == FilesSecurityActions.Read)
                    {
                        using (var folderDao = daoFactory.GetFolderDao())
                        {
                            var root = folderDao.GetFolder(entry.RootFolderId);
                            if (root != null)
                            {
                                var path = folderDao.GetBunchObjectID(root.ID);

                                var adapter = FilesIntegration.GetFileSecurity(path);

                                if (adapter != null)
                                {
                                    return adapter.WhoCanRead(entry);
                                }
                            }
                        }
                    }

                    // TODO: For Projects and other
                    defaultShareRecord = null;
                    break;

                default:
                    defaultShareRecord = null;
                    break;
            }

            if (defaultShareRecord != null)
                shares = shares.Concat(new[] { defaultShareRecord });

            return shares.SelectMany(x =>
                                         {
                                             var groupInfo = CoreContext.UserManager.GetGroupInfo(x.Subject);

                                             if (groupInfo.ID != Constants.LostGroupInfo.ID)
                                                 return
                                                     CoreContext.UserManager.GetUsersByGroup(groupInfo.ID)
                                                                .Where(p => p.Status == EmployeeStatus.Active)
                                                                .Select(y => y.ID);

                                             return new[] { x.Subject };
                                         })
                         .Distinct()
                         .Where(x => Can(entry, x, action, copyshares))
                         .ToList();
        }

        public List<T> FilterRead<T>(List<T> entries) where T : FileEntry
        {
            return Filter(entries, FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID);
        }

        public List<FileEntry> FilterRead(List<FileEntry> entries)
        {
            return Filter(entries, FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID);
        }

        public IEnumerable<T> FilterEdit<T>(IEnumerable<T> entries) where T : FileEntry
        {
            return Filter(entries, FilesSecurityActions.Edit, SecurityContext.CurrentAccount.ID);
        }

        private bool Can(FileEntry entry, Guid userId, FilesSecurityActions action, IEnumerable<FileShareRecord> shares = null)
        {
            return Filter(new[] { entry }, action, userId, shares).Any();
        }

        public List<T> FilterDownload<T>(List<T> entries) where T : FileEntry
        {
            return FilterRead(entries).FindAll(CheckDenyDownload);
        }

        private bool CheckDenyDownload(FileEntry entry)
        {
            return entry.DenyDownload
                ? entry.Access != FileShare.Read && entry.Access != FileShare.Comment
                : true;
        }

        public IEnumerable<T> FilterSharing<T>(IEnumerable<T> entries) where T : FileEntry
        {
            return FilterEdit(entries).Where(CheckDenySharing);
        }

        private bool CheckDenySharing(FileEntry entry)
        {
            return entry.DenySharing
                ? entry.Access != FileShare.ReadWrite
                : SecurityContext.IsAuthenticated;
        }

        private List<Tuple<FileEntry, bool>> Can(IEnumerable<FileEntry> entry, Guid userId, FilesSecurityActions action)
        {
            var filtres = Filter(entry, action, userId);
            return entry.Select(r => new Tuple<FileEntry, bool>(r, filtres.Any(a => a.ID.Equals(r.ID)))).ToList();
        }

        private List<T> Filter<T>(IEnumerable<T> entries, FilesSecurityActions action, Guid userId, IEnumerable<FileShareRecord> shares = null) where T : FileEntry
        {
            if (entries == null || !entries.Any()) return new List<T>(0);

            var user = CoreContext.UserManager.GetUsers(userId);
            var isOutsider = user.IsOutsider();

            if (isOutsider && action != FilesSecurityActions.Read) return new List<T>(0);

            entries = entries.Where(f => f != null).ToList();
            var result = new List<T>(entries.Count());

            // save entries order
            var order = entries.Select((f, i) => new { Id = f.UniqID, Pos = i }).ToDictionary(e => e.Id, e => e.Pos);

            // common or my files
            Func<T, bool> filter =
                f => f.RootFolderType == FolderType.COMMON ||
                     f.RootFolderType == FolderType.USER ||
                     f.RootFolderType == FolderType.SHARE ||
                     f.RootFolderType == FolderType.Recent ||
                     f.RootFolderType == FolderType.Favorites ||
                     f.RootFolderType == FolderType.Templates ||
                     f.RootFolderType == FolderType.Privacy ||
                     f.RootFolderType == FolderType.Projects;

            var isVisitor = user.IsVisitor();

            if (entries.Any(filter))
            {
                List<Guid> subjects = null;

                foreach (var e in entries.Where(filter))
                {
                    var isShareLink = userId == FileConstant.ShareLinkId;

                    if (!CoreContext.Authentication.GetAccountByID(userId).IsAuthenticated && !isShareLink)
                    {
                        var record = GetShareRecord(userId);

                        isShareLink = (record != null && record.SubjectType == SubjectType.ExternalLink) || FileShareLink.TryGetCurrentLinkId(out _);

                        if (!isShareLink)
                        {
                            continue;
                        }
                    }

                    if (isOutsider && (e.RootFolderType == FolderType.USER
                                       || e.RootFolderType == FolderType.SHARE
                                       || e.RootFolderType == FolderType.Privacy))
                    {
                        continue;
                    }

                    if (isVisitor && e.RootFolderType == FolderType.Recent)
                    {
                        continue;
                    }

                    if (isVisitor && e.RootFolderType == FolderType.Favorites)
                    {
                        continue;
                    }

                    if (isVisitor && e.RootFolderType == FolderType.Templates)
                    {
                        continue;
                    }

                    if (isVisitor && e.RootFolderType == FolderType.Privacy)
                    {
                        continue;
                    }

                    var folder = e as Folder;
                    var file = e as File;

                    if (action != FilesSecurityActions.Read && e.FileEntryType == FileEntryType.Folder)
                    {
                        if (folder == null) continue;

                        if (folder.FolderType == FolderType.Projects)
                        {
                            // Root Projects folder read-only
                            continue;
                        }

                        if (folder.FolderType == FolderType.SHARE)
                        {
                            // Root Share folder read-only
                            continue;
                        }

                        if (folder.FolderType == FolderType.Recent)
                        {
                            // Recent folder read-only
                            continue;
                        }

                        if (folder.FolderType == FolderType.Favorites)
                        {
                            // Favorites folder read-only
                            continue;
                        }

                        if (folder.FolderType == FolderType.Templates)
                        {
                            // Templates folder read-only
                            continue;
                        }
                    }



                    if (isVisitor && e.ProviderEntry)
                    {
                        continue;
                    }

                    //if (e.FileEntryType == FileEntryType.File
                    //    && file.IsFillFormDraft)
                    //{
                    //    e.Access = FileShare.FillForms;

                    //    if (action != FilesSecurityActions.Read
                    //        && action != FilesSecurityActions.FillForms
                    //        && action != FilesSecurityActions.Delete)
                    //    {
                    //        continue;
                    //    }
                    //}

                    if (e.RootFolderType == FolderType.USER && e.RootFolderCreator == userId && !isVisitor)
                    {
                        // user has all right in his folder
                        result.Add(e);
                        continue;
                    }

                    if (e.RootFolderType == FolderType.Privacy && e.RootFolderCreator == userId && !isVisitor)
                    {
                        // user has all right in his privacy folder
                        result.Add(e);
                        continue;
                    }

                    if (e.FileEntryType == FileEntryType.Folder)
                    {
                        if (folder == null) continue;

                        if (DefaultCommonShare == FileShare.Read && action == FilesSecurityActions.Read && folder.FolderType == FolderType.COMMON)
                        {
                            // all can read Common folder
                            result.Add(e);
                            continue;
                        }

                        if (action == FilesSecurityActions.Read && folder.FolderType == FolderType.SHARE)
                        {
                            // all can read Share folder
                            result.Add(e);
                            continue;
                        }

                        if (action == FilesSecurityActions.Read && folder.FolderType == FolderType.Recent)
                        {
                            // all can read recent folder
                            result.Add(e);
                            continue;
                        }

                        if (action == FilesSecurityActions.Read && folder.FolderType == FolderType.Favorites)
                        {
                            // all can read favorites folder
                            result.Add(e);
                            continue;
                        }

                        if (action == FilesSecurityActions.Read && folder.FolderType == FolderType.Templates)
                        {
                            // all can read templates folder
                            result.Add(e);
                            continue;
                        }
                    }


                    if (e.RootFolderType == FolderType.COMMON && IsAdministrator(userId))
                    {
                        // administrator in Common has all right
                        result.Add(e);
                        continue;
                    }

                    if (subjects == null)
                    {
                        subjects = GetUserSubjects(userId, isShareLink);
                        if (shares == null)
                        {
                            shares = GetShares(entries);
                        }
                        shares = shares
                            .Join(subjects, r => r.Subject, s => s, (r, s) => r)
                            .ToList();
                    }

                    FileShareRecord ace;
                    if (e.FileEntryType == FileEntryType.File)
                    {
                        ace = shares
                            .OrderBy(r => r, new SubjectComparer(subjects))
                            .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer())
                            .FirstOrDefault(r => Equals(r.EntryId, e.ID) && r.EntryType == FileEntryType.File);
                        if (ace == null)
                        {
                            // share on parent folders
                            ace = shares.Where(r => Equals(r.EntryId, file.FolderID) && r.EntryType == FileEntryType.Folder)
                                        .OrderBy(r => r, new SubjectComparer(subjects))
                                        .ThenBy(r => r.Level)
                                        .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer())
                                        .FirstOrDefault();
                        }
                    }
                    else
                    {
                        ace = shares.Where(r => Equals(r.EntryId, e.ID) && r.EntryType == FileEntryType.Folder)
                                    .OrderBy(r => r, new SubjectComparer(subjects))
                                    .ThenBy(r => r.Level)
                                    .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer())
                                    .FirstOrDefault();
                    }

                    if (ace != null && ace.SubjectType == SubjectType.ExternalLink && ace.EntryType == FileEntryType.Folder && ace.Subject != userId && !FileShareLink.CheckCookieOrPasswordKey(ace, null, out _))
                    {
                        continue;
                    }

                    var defaultShare = isShareLink
                        ? FileShare.Restrict
                        : e.RootFolderType == FolderType.USER
                            ? DefaultMyShare
                            : e.RootFolderType == FolderType.Privacy
                                ? DefaultPrivacyShare
                                : DefaultCommonShare;

                    e.Access = ace != null ? ace.Share : defaultShare;

                    if (action == FilesSecurityActions.Read && e.Access != FileShare.Restrict) result.Add(e);
                    else if (action == FilesSecurityActions.Comment && (e.Access == FileShare.Comment || e.Access == FileShare.Review || e.Access == FileShare.CustomFilter || e.Access == FileShare.ReadWrite)) result.Add(e);
                    else if (action == FilesSecurityActions.FillForms && (e.Access == FileShare.FillForms || e.Access == FileShare.Review || e.Access == FileShare.ReadWrite)) result.Add(e);
                    else if (action == FilesSecurityActions.Review && (e.Access == FileShare.Review || e.Access == FileShare.ReadWrite)) result.Add(e);
                    else if (action == FilesSecurityActions.CustomFilter && (e.Access == FileShare.CustomFilter || e.Access == FileShare.ReadWrite)) result.Add(e);
                    else if (action == FilesSecurityActions.Edit && e.Access == FileShare.ReadWrite) result.Add(e);
                    else if (action == FilesSecurityActions.Create && e.Access == FileShare.ReadWrite) result.Add(e);
                    else if (e.Access != FileShare.Restrict && e.CreateBy == userId && (userId != ASC.Core.Configuration.Constants.Guest.ID || e.Access == FileShare.ReadWrite) && (e.FileEntryType == FileEntryType.File || folder.FolderType != FolderType.COMMON)) result.Add(e);

                    if (e.CreateBy == userId && (userId != ASC.Core.Configuration.Constants.Guest.ID || (e.FileEntryType == FileEntryType.File && e.Access == FileShare.ReadWrite))) e.Access = FileShare.None; //HACK: for client
                }
            }

            // files in bunch
            filter = f => f.RootFolderType == FolderType.BUNCH;
            if (entries.Any(filter))
            {
                using (var folderDao = daoFactory.GetFolderDao())
                {
                    var filteredEntries = entries.Where(filter).ToList();
                    var roots = filteredEntries
                            .Select(r => r.RootFolderId)
                            .ToList();

                    var rootsFolders = folderDao.GetFolders(roots);
                    var bunches = folderDao.GetBunchObjectIDs(rootsFolders.Select(r => r.ID).ToList());
                    var findedAdapters = FilesIntegration.GetFileSecurity(bunches);

                    foreach (var e in filteredEntries)
                    {
                        var adapter = findedAdapters[e.RootFolderId.ToString()];

                        if (adapter == null) continue;

                        if (adapter.CanRead(e, userId) &&
                            adapter.CanCreate(e, userId) &&
                            adapter.CanEdit(e, userId) &&
                            adapter.CanDelete(e, userId))
                        {
                            e.Access = FileShare.None;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.Comment && adapter.CanComment(e, userId))
                        {
                            e.Access = FileShare.Comment;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.FillForms && adapter.CanFillForms(e, userId))
                        {
                            e.Access = FileShare.FillForms;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.Review && adapter.CanReview(e, userId))
                        {
                            e.Access = FileShare.Review;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.CustomFilter && adapter.CanCustomFilterEdit(e, userId))
                        {
                            e.Access = FileShare.CustomFilter;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.Create && adapter.CanCreate(e, userId))
                        {
                            e.Access = FileShare.ReadWrite;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.Delete && adapter.CanDelete(e, userId))
                        {
                            e.Access = FileShare.ReadWrite;
                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.Read && adapter.CanRead(e, userId))
                        {
                            if (adapter.CanCreate(e, userId) ||
                                adapter.CanDelete(e, userId) ||
                                adapter.CanEdit(e, userId))
                                e.Access = FileShare.ReadWrite;
                            else
                                e.Access = FileShare.Read;

                            result.Add(e);
                        }
                        else if (action == FilesSecurityActions.Edit && adapter.CanEdit(e, userId))
                        {
                            e.Access = FileShare.ReadWrite;

                            result.Add(e);
                        }
                    }
                }
            }

            // files in trash
            filter = f => f.RootFolderType == FolderType.TRASH;
            if ((action == FilesSecurityActions.Read || action == FilesSecurityActions.Delete) && entries.Any(filter))
            {
                using (var folderDao = daoFactory.GetFolderDao())
                {
                    var mytrashId = folderDao.GetFolderIDTrash(false, userId);
                    if (!Equals(mytrashId, 0))
                    {
                        result.AddRange(entries.Where(filter).Where(e => Equals(e.RootFolderId, mytrashId)));
                    }
                }
            }

            if (IsAdministrator(userId))
            {
                // administrator can work with crashed entries (crash in files_folder_tree)
                filter = f => f.RootFolderType == FolderType.DEFAULT;
                result.AddRange(entries.Where(filter));
            }

            // restore entries order
            result.Sort((x, y) => order[x.UniqID].CompareTo(order[y.UniqID]));
            return result;
        }

        public void Share(object entryId, FileEntryType entryType, Guid @for, SubjectType subjectType, FileShare share, FileShareOptions shareOptions = null)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                var r = new FileShareRecord
                {
                    Tenant = TenantProvider.CurrentTenantID,
                    EntryId = entryId,
                    EntryType = entryType,
                    Subject = @for,
                    SubjectType = subjectType,
                    Owner = SecurityContext.CurrentAccount.ID,
                    Share = share,
                    Options = shareOptions
                };
                securityDao.SetShare(r);
            }
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<FileEntry> entries)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                return securityDao.GetShares(entries);
            }
        }

        public IEnumerable<FileShareRecord> GetShares(FileEntry entry)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                return securityDao.GetShares(entry);
            }
        }

        public FileShareRecord GetShareRecord(Guid subject)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                return securityDao.GetShares(new[] { subject }).FirstOrDefault();
            }
        }

        public List<FileEntry> GetSharesForMe(FilterType filterType, bool subjectGroup, Guid subjectID, string searchText = "", bool searchInContent = false, string extension = null, bool withSubfolders = false)
        {
            using (var folderDao = daoFactory.GetFolderDao())
            using (var fileDao = daoFactory.GetFileDao())
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                var subjects = GetUserSubjects(SecurityContext.CurrentAccount.ID, false);

                var records = securityDao.GetShares(subjects);

                var fileIds = new Dictionary<object, FileShare>();
                var folderIds = new Dictionary<object, FileShare>();

                var recordGroup = records.GroupBy(r => new { r.EntryId, r.EntryType }, (key, group) => new
                {
                    firstRecord = group.OrderBy(r => r, new SubjectComparer(subjects))
                        .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer())
                        .First()
                });

                foreach (var r in recordGroup.Where(r => r.firstRecord.Share != FileShare.Restrict))
                {
                    if (r.firstRecord.EntryType == FileEntryType.Folder)
                    {
                        if (!folderIds.ContainsKey(r.firstRecord.EntryId))
                            folderIds.Add(r.firstRecord.EntryId, r.firstRecord.Share);
                    }
                    else
                    {
                        if (!fileIds.ContainsKey(r.firstRecord.EntryId))
                            fileIds.Add(r.firstRecord.EntryId, r.firstRecord.Share);
                    }
                }

                var entries = new List<FileEntry>();

                if (filterType != FilterType.FoldersOnly)
                {
                    var files = fileDao.GetFilesFiltered(fileIds.Keys.ToList(), filterType, subjectGroup, subjectID, searchText, searchInContent, extension);

                    files.ForEach(x =>
                        {
                            if (fileIds.ContainsKey(x.ID))
                            {
                                x.Access = fileIds[x.ID];
                                x.FolderIdDisplay = Global.FolderShare;
                            }
                        });

                    entries.AddRange(files);
                }

                if (filterType == FilterType.None || filterType == FilterType.FoldersOnly)
                {
                    var folders = folderDao.GetFolders(folderIds.Keys.ToList(), filterType, subjectGroup, subjectID, searchText, withSubfolders, false);

                    if (withSubfolders)
                    {
                        folders = FilterRead(folders);
                    }
                    folders.ForEach(x =>
                        {
                            if (folderIds.ContainsKey(x.ID))
                            {
                                x.Access = folderIds[x.ID];
                                x.FolderIdDisplay = Global.FolderShare;
                            }
                        });

                    entries.AddRange(folders);
                }

                if (filterType != FilterType.FoldersOnly && withSubfolders)
                {
                    var filesInSharedFolders = fileDao.GetFiles(folderIds.Keys.ToList(), filterType, subjectGroup, subjectID, searchText, searchInContent, extension);
                    filesInSharedFolders = FilterRead(filesInSharedFolders);
                    entries.AddRange(filesInSharedFolders);
                    entries = entries.Distinct().ToList();
                }

                entries = entries.Where(f =>
                                        f.RootFolderType == FolderType.USER // show users files
                                        && f.RootFolderCreator != SecurityContext.CurrentAccount.ID // don't show my files
                                        && (!f.ProviderEntry || FilesSettings.EnableThirdParty) // show thirdparty provider only if enabled
                    ).ToList();

                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
                {
                    entries = entries.Where(r => !r.ProviderEntry).ToList();
                }

                var failedEntries = entries.Where(x => !String.IsNullOrEmpty(x.Error));
                var failedRecords = new List<FileShareRecord>();

                foreach (var failedEntry in failedEntries)
                {
                    var entryType = failedEntry.FileEntryType;

                    var failedRecord = records.First(x => x.EntryId.Equals(failedEntry.ID) && x.EntryType == entryType);

                    failedRecord.Share = FileShare.None;

                    failedRecords.Add(failedRecord);
                }

                if (failedRecords.Any())
                {
                    securityDao.DeleteShareRecords(failedRecords);
                }

                return entries.Where(x => String.IsNullOrEmpty(x.Error)).ToList();
            }
        }

        public List<FileEntry> GetPrivacyForMe(FilterType filterType, bool subjectGroup, Guid subjectID, string searchText = "", bool searchInContent = false, string extension = null, bool withSubfolders = false)
        {
            using (var folderDao = daoFactory.GetFolderDao())
            using (var fileDao = daoFactory.GetFileDao())
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                var subjects = new List<Guid> { SecurityContext.CurrentAccount.ID };

                var records = securityDao.GetShares(subjects);

                var fileIds = new Dictionary<object, FileShare>();
                var folderIds = new Dictionary<object, FileShare>();

                var recordGroup = records.GroupBy(r => new { r.EntryId, r.EntryType }, (key, group) => new
                {
                    firstRecord = group.OrderBy(r => r, new SubjectComparer(subjects))
                        .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer())
                        .First()
                });

                foreach (var r in recordGroup.Where(r => r.firstRecord.Share != FileShare.Restrict))
                {
                    if (r.firstRecord.EntryType == FileEntryType.Folder)
                    {
                        if (!folderIds.ContainsKey(r.firstRecord.EntryId))
                            folderIds.Add(r.firstRecord.EntryId, r.firstRecord.Share);
                    }
                    else
                    {
                        if (!fileIds.ContainsKey(r.firstRecord.EntryId))
                            fileIds.Add(r.firstRecord.EntryId, r.firstRecord.Share);
                    }
                }

                var entries = new List<FileEntry>();

                if (filterType != FilterType.FoldersOnly)
                {
                    var files = fileDao.GetFilesFiltered(fileIds.Keys.ToList(), filterType, subjectGroup, subjectID, searchText, searchInContent, extension);

                    files.ForEach(x =>
                        {
                            if (fileIds.ContainsKey(x.ID))
                            {
                                x.Access = fileIds[x.ID];
                                x.FolderIdDisplay = Global.FolderPrivacy;
                            }
                        });

                    entries.AddRange(files);
                }

                if (filterType == FilterType.None || filterType == FilterType.FoldersOnly)
                {
                    var folders = folderDao.GetFolders(folderIds.Keys.ToList(), filterType, subjectGroup, subjectID, searchText, withSubfolders, false);

                    if (withSubfolders)
                    {
                        folders = FilterRead(folders);
                    }
                    folders.ForEach(x =>
                        {
                            if (folderIds.ContainsKey(x.ID))
                            {
                                x.Access = folderIds[x.ID];
                                x.FolderIdDisplay = Global.FolderPrivacy;
                            }
                        });

                    entries.AddRange(folders);
                }

                if (filterType != FilterType.FoldersOnly && withSubfolders)
                {
                    var filesInSharedFolders = fileDao.GetFiles(folderIds.Keys.ToList(), filterType, subjectGroup, subjectID, searchText, searchInContent, extension);
                    filesInSharedFolders = FilterRead(filesInSharedFolders);
                    entries.AddRange(filesInSharedFolders);
                    entries = entries.Distinct().ToList();
                }

                entries = entries.Where(f =>
                                        f.RootFolderType == FolderType.Privacy // show users files
                                        && f.RootFolderCreator != SecurityContext.CurrentAccount.ID // don't show my files
                    ).ToList();

                return entries;
            }
        }

        public void RemoveSubjects(IEnumerable<Guid> subjects)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                securityDao.RemoveSubjects(subjects);
            }
        }

        public List<Guid> GetUserSubjects(Guid userId, bool isLink)
        {
            // priority order
            // User, Departments, admin, everyone

            var result = new List<Guid> { userId };

            var isExist = FileShareLink.TryGetCurrentLinkId(out var linkId);
            if (isExist) result.Add(linkId);

            if (userId == FileConstant.ShareLinkId || isLink)
                return result;

            result.AddRange(CoreContext.UserManager.GetUserGroups(userId).Select(g => g.ID));
            if (IsAdministrator(userId)) result.Add(Constants.GroupAdmin.ID);
            result.Add(Constants.GroupEveryone.ID);

            return result;
        }

        private class SubjectComparer : IComparer<FileShareRecord>
        {
            private readonly List<Guid> _subjects;

            public SubjectComparer(List<Guid> subjects)
            {
                _subjects = subjects;
            }

            public int Compare(FileShareRecord x, FileShareRecord y)
            {
                if (x.Subject == y.Subject)
                {
                    return 0;
                }

                var index1 = _subjects.IndexOf(x.Subject);
                var index2 = _subjects.IndexOf(y.Subject);
                if (index1 == 0 || index2 == 0 // UserId
                    || Constants.BuildinGroups.Any(g => g.ID == x.Subject) || Constants.BuildinGroups.Any(g => g.ID == y.Subject)) // System Groups
                {
                    return index1.CompareTo(index2);
                }

                // Departments are equal.
                return 0;
            }
        }

        private enum FilesSecurityActions
        {
            Read,
            Comment,
            FillForms,
            Review,
            Create,
            Edit,
            Delete,
            CustomFilter,
        }
    }
}