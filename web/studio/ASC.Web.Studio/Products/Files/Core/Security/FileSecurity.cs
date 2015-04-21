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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Files.Api;
using ASC.Web.Files.Configuration;
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

        public FileSecurity(IDaoFactory daoFactory)
        {
            this.daoFactory = daoFactory;
        }

        public bool CanRead(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Read);
        }

        public bool CanCreate(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Create);
        }

        public bool CanEdit(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Edit);
        }

        public bool CanDelete(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Delete);
        }

        public bool CanRead(FileEntry file)
        {
            return CanRead(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanCreate(FileEntry file)
        {
            return CanCreate(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanEdit(FileEntry file)
        {
            return CanEdit(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanDelete(FileEntry file)
        {
            return CanDelete(file, SecurityContext.CurrentAccount.ID);
        }

        public IEnumerable<Guid> WhoCanRead(FileEntry fileEntry)
        {
            return WhoCan(fileEntry, FilesSecurityActions.Read);
        }

        public IEnumerable<Guid> WhoCanCreate(FileEntry fileEntry)
        {
            return WhoCan(fileEntry, FilesSecurityActions.Create);
        }

        public IEnumerable<Guid> WhoCanEdit(FileEntry fileEntry)
        {
            return WhoCan(fileEntry, FilesSecurityActions.Edit);
        }

        public IEnumerable<Guid> WhoCanDelete(FileEntry fileEntry)
        {
            return WhoCan(fileEntry, FilesSecurityActions.Delete);
        }

        private IEnumerable<Guid> WhoCan(FileEntry fileEntry, FilesSecurityActions action)
        {
            var shares = GetShares(fileEntry);

            FileShareRecord defaultShareRecord;

            switch (fileEntry.RootFolderType)
            {
                case FolderType.COMMON:
                    defaultShareRecord = new FileShareRecord
                        {
                            Level = int.MaxValue,
                            EntryId = fileEntry.ID,
                            EntryType = fileEntry is File ? FileEntryType.File : FileEntryType.Folder,
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
                            EntryId = fileEntry.ID,
                            EntryType = fileEntry is File ? FileEntryType.File : FileEntryType.Folder,
                            Share = DefaultMyShare,
                            Subject = fileEntry.RootFolderCreator,
                            Tenant = TenantProvider.CurrentTenantID,
                            Owner = fileEntry.RootFolderCreator
                        };

                    if (!shares.Any())
                        return new List<Guid>
                            {
                                fileEntry.RootFolderCreator
                            };

                    break;
                default:
                    defaultShareRecord = null;
                    break;
            }
            // TODO: For Projects and other

            if (defaultShareRecord != null)
                shares = shares.Concat(new[] {defaultShareRecord});

            return shares.SelectMany(x =>
                                         {
                                             var groupInfo = CoreContext.UserManager.GetGroupInfo(x.Subject);

                                             if (groupInfo.ID != Constants.LostGroupInfo.ID)
                                                 return
                                                     CoreContext.UserManager.GetUsersByGroup(groupInfo.ID)
                                                                .Where(p => p.Status == EmployeeStatus.Active)
                                                                .Select(y => y.ID);

                                             return new[] {x.Subject};
                                         })
                         .Distinct()
                         .Where(x => Can(fileEntry, x, action));
        }

        public IEnumerable<FileEntry> FilterRead(IEnumerable<FileEntry> entries)
        {
            return Filter(entries, FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID);
        }

        public IEnumerable<File> FilterRead(IEnumerable<File> entries)
        {
            return Filter(entries.Cast<FileEntry>(), FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID).Cast<File>();
        }

        public IEnumerable<Folder> FilterRead(IEnumerable<Folder> entries)
        {
            return Filter(entries.Cast<FileEntry>(), FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID).Cast<Folder>();
        }

        private bool Can(FileEntry entry, Guid userId, FilesSecurityActions action)
        {
            return Filter(new[] {entry}, action, userId).Any();
        }

        private IEnumerable<FileEntry> Filter(IEnumerable<FileEntry> entries, FilesSecurityActions action, Guid userId)
        {
            if (entries == null || !entries.Any()) return Enumerable.Empty<FileEntry>();

            var user = CoreContext.UserManager.GetUsers(userId);
            var isOutsider = user.IsOutsider();

            if (isOutsider && action != FilesSecurityActions.Read) return Enumerable.Empty<FileEntry>();

            entries = entries.Where(f => f != null);
            var result = new List<FileEntry>(entries.Count());

            // save entries order
            var order = entries.Select((f, i) => new {Id = f.UniqID, Pos = i}).ToDictionary(e => e.Id, e => e.Pos);

            // common or my files
            Func<FileEntry, bool> filter =
                f => f.RootFolderType == FolderType.COMMON ||
                     f.RootFolderType == FolderType.USER ||
                     f.RootFolderType == FolderType.SHARE ||
                     f.RootFolderType == FolderType.Projects;

            var isVisitor = user.IsVisitor();

            if (entries.Any(filter))
            {
                var subjects = GetUserSubjects(userId);
                List<FileShareRecord> shares = null;
                foreach (var e in entries.Where(filter))
                {
                    if (!CoreContext.Authentication.GetAccountByID(userId).IsAuthenticated && userId != FileConstant.ShareLinkId)
                    {
                        continue;
                    }

                    if (isOutsider && (e.RootFolderType == FolderType.USER
                                       || e.RootFolderType == FolderType.SHARE
                                       || e.RootFolderType == FolderType.TRASH))
                    {
                        continue;
                    }

                    if (action != FilesSecurityActions.Read && e is Folder && ((Folder) e).FolderType == FolderType.Projects)
                    {
                        // Root Projects folder read-only
                        continue;
                    }

                    if (action != FilesSecurityActions.Read && e is Folder && ((Folder) e).FolderType == FolderType.SHARE)
                    {
                        // Root Share folder read-only
                        continue;
                    }

                    if (isVisitor && e.ProviderEntry)
                    {
                        continue;
                    }

                    if (e.RootFolderType == FolderType.USER && e.RootFolderCreator == userId && !isVisitor)
                    {
                        // user has all right in his folder
                        result.Add(e);
                        continue;
                    }

                    if (DefaultCommonShare == FileShare.Read && action == FilesSecurityActions.Read && e is Folder &&
                        ((Folder) e).FolderType == FolderType.COMMON)
                    {
                        // all can read Common folder
                        result.Add(e);
                        continue;
                    }

                    if (action == FilesSecurityActions.Read && e is Folder &&
                        ((Folder) e).FolderType == FolderType.SHARE)
                    {
                        // all can read Share folder
                        result.Add(e);
                        continue;
                    }

                    if (e.RootFolderType == FolderType.COMMON && IsAdministrator(userId))
                    {
                        // administrator in Common has all right
                        result.Add(e);
                        continue;
                    }

                    if (shares == null)
                    {
                        shares = GetShares(entries.ToArray()).Join(subjects, r => r.Subject, s => s, (r, s) => r).ToList();
                        // shares ordered by level
                    }

                    FileShareRecord ace;
                    if (e is File)
                    {
                        ace = shares
                            .OrderBy(r => r, new SubjectComparer(subjects))
                            .ThenByDescending(r => r.Share)
                            .FirstOrDefault(r => Equals(r.EntryId, e.ID) && r.EntryType == FileEntryType.File);
                        if (ace == null)
                        {
                            // share on parent folders
                            ace = shares.Where(r => Equals(r.EntryId, ((File) e).FolderID) && r.EntryType == FileEntryType.Folder)
                                        .OrderBy(r => r, new SubjectComparer(subjects))
                                        .ThenBy(r => r.Level)
                                        .ThenByDescending(r => r.Share)
                                        .FirstOrDefault();
                        }
                    }
                    else
                    {
                        ace = shares.Where(r => Equals(r.EntryId, e.ID) && r.EntryType == FileEntryType.Folder)
                                    .OrderBy(r => r, new SubjectComparer(subjects))
                                    .ThenBy(r => r.Level)
                                    .ThenByDescending(r => r.Share)
                                    .FirstOrDefault();
                    }
                    var defaultShare = e.RootFolderType == FolderType.USER ? DefaultMyShare : DefaultCommonShare;
                    e.Access = ace != null ? ace.Share : defaultShare;

                    if (action == FilesSecurityActions.Read && e.Access <= FileShare.Read) result.Add(e);
                    else if (action == FilesSecurityActions.Edit && e.Access <= FileShare.ReadWrite) result.Add(e);
                    else if (action == FilesSecurityActions.Create && e.Access <= FileShare.ReadWrite) result.Add(e);
                        // can't delete in My other people's files
                    else if (action == FilesSecurityActions.Delete && e.Access <= FileShare.ReadWrite && e.RootFolderType == FolderType.COMMON) result.Add(e);
                    else if (e.Access <= FileShare.Read && e.CreateBy == userId && (e is File || ((Folder) e).FolderType != FolderType.COMMON)) result.Add(e);

                    if (e.CreateBy == userId) e.Access = FileShare.None; //HACK: for client
                }
            }

            // files in bunch
            filter = f => f.RootFolderType == FolderType.BUNCH;
            if (entries.Any(filter))
            {
                using (var folderDao = daoFactory.GetFolderDao())
                {
                    var findedAdapters = new Dictionary<object, IFileSecurity>();
                    foreach (var e in entries.Where(filter))
                    {
                        IFileSecurity adapter = null;

                        if (!findedAdapters.ContainsKey(e.RootFolderId))
                        {
                            var root = folderDao.GetFolder(e.RootFolderId);
                            if (root != null)
                            {
                                var path = folderDao.GetBunchObjectID(root.ID);

                                adapter = FilesIntegration.GetFileSecurity(path);
                            }
                            findedAdapters[e.RootFolderId] = adapter;
                        }

                        adapter = findedAdapters[e.RootFolderId];

                        if (adapter == null) continue;

                        if (adapter.CanRead(e, userId) &&
                            adapter.CanCreate(e, userId) &&
                            adapter.CanEdit(e, userId) &&
                            adapter.CanDelete(e, userId))
                        {
                            e.Access = FileShare.None;
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
            if (entries.Any(filter))
            {
                using (var folderDao = daoFactory.GetFolderDao())
                {
                    var mytrashId = folderDao.GetFolderID(FileConstant.ModuleId, "trash", userId.ToString(), false);
                    foreach (var e in entries.Where(filter))
                    {
                        // only in my trash
                        if (Equals(e.RootFolderId, mytrashId)) result.Add(e);
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

        public void Share(object entryId, FileEntryType entryType, Guid @for, FileShare share)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                var r = new FileShareRecord
                    {
                        Tenant = TenantProvider.CurrentTenantID,
                        EntryId = entryId,
                        EntryType = entryType,
                        Subject = @for,
                        Owner = SecurityContext.CurrentAccount.ID,
                        Share = share,
                    };
                securityDao.SetShare(r);
            }
        }

        public IEnumerable<FileShareRecord> GetShares(params FileEntry[] entries)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                return securityDao.GetShares(entries);
            }
        }

        public List<FileEntry> GetSharesForMe()
        {
            using (var folderDao = daoFactory.GetFolderDao())
            using (var fileDao = daoFactory.GetFileDao())
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                var subjects = GetUserSubjects(SecurityContext.CurrentAccount.ID);

                var records = securityDao.GetShares(subjects);

                var fileIds = new Dictionary<object, FileShare>();
                var folderIds = new Dictionary<object, FileShare>();

                var recordGroup = records.GroupBy(r => new { r.EntryId, r.EntryType }, (key, group) => new
                {
                    firstRecord = group.OrderBy(r => r, new SubjectComparer(subjects))
                        .ThenByDescending(r => r.Share)
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

                //TODO: optimize
                var files = fileDao.GetFiles(fileIds.Keys.ToArray());

                files.ForEach(x =>
                                  {
                                      if (fileIds.ContainsKey(x.ID))
                                          x.Access = fileIds[x.ID];
                                  });

                var folders = folderDao.GetFolders(folderIds.Keys.ToArray());

                folders.ForEach(x =>
                                    {
                                        if (folderIds.ContainsKey(x.ID))
                                            x.Access = folderIds[x.ID];
                                    });

                var entries = files.Concat(folders.Cast<FileEntry>()).ToList();

                var failedEntries = entries.Where(x => !String.IsNullOrEmpty(x.Error));
                var failedRecords = new List<FileShareRecord>();

                foreach (var failedEntry in failedEntries)
                {
                    var entryType = failedEntry is Folder ? FileEntryType.Folder : FileEntryType.File;

                    var failedRecord = records.First(x => x.EntryId.Equals(failedEntry.ID) && x.EntryType == entryType);

                    failedRecord.Share = FileShare.None;

                    failedRecords.Add(failedRecord);
                }

                if (failedRecords.Any())
                {
                    securityDao.DeleteShareRecords(failedRecords.ToArray());
                }

                return entries.Where(x => String.IsNullOrEmpty(x.Error)).ToList();
            }
        }

        public void RemoveSubject(Guid subject)
        {
            using (var securityDao = daoFactory.GetSecurityDao())
            {
                securityDao.RemoveSubject(subject);
            }
        }

        public List<Guid> GetUserSubjects(Guid userId)
        {
            // priority order
            // User, Departments, SystemGroups
            return new[] {userId}
                .Union(CoreContext.UserManager.GetUserGroups(userId, IncludeType.Distinct).Select(g => g.ID))
                .ToList();
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
            Create,
            Edit,
            Delete,
        }
    }
}