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
using System.Linq;
using System.Security;
using ASC.Common.Threading.Workers;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public static class FileMarker
    {
        private class AsyncTaskData
        {
            public AsyncTaskData()
            {
                TenantID = TenantProvider.CurrentTenantID;
                CurrentAccountId = SecurityContext.CurrentAccount.ID;
            }

            public int TenantID { get; private set; }

            public FileEntry FileEntry { get; set; }

            public List<Guid> UserIDs { get; set; }

            public Guid CurrentAccountId { get; set; }
        }

        private static readonly WorkerQueue<AsyncTaskData> AsyncTasks = new WorkerQueue<AsyncTaskData>(1, TimeSpan.FromSeconds(60), 1, true);
        private static readonly object SyncObj = new object();

        private static void ExecMarkFileAsNew(AsyncTaskData obj)
        {
            CoreContext.TenantManager.SetCurrentTenant(Convert.ToInt32(obj.TenantID));

            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                object parentFolderId;

                if (obj.FileEntry is File)
                    parentFolderId = ((File)obj.FileEntry).FolderID;
                else
                    parentFolderId = obj.FileEntry.ID;
                var parentFolders = folderDao.GetParentFolders(parentFolderId);
                parentFolders.Reverse();

                var userEntriesData = new Dictionary<Guid, List<FileEntry>>();

                if (obj.FileEntry.RootFolderType == FolderType.BUNCH)
                {
                    if (!obj.UserIDs.Any()) return;

                    parentFolders.Add(folderDao.GetFolder(Global.FolderProjects));

                    var entries = new List<FileEntry> { obj.FileEntry };
                    entries = entries.Concat(parentFolders.Cast<FileEntry>()).ToList();

                    obj.UserIDs.ForEach(userID =>
                                            {
                                                if (userEntriesData.ContainsKey(userID))
                                                    userEntriesData[userID].AddRange(entries);
                                                else
                                                    userEntriesData.Add(userID, entries);
                                            });
                }
                else
                {
                    var filesSecurity = Global.GetFilesSecurity();

                    parentFolders.ForEach(parentFolder =>
                                          filesSecurity
                                              .WhoCanRead(parentFolder)
                                              .Where(x => x != obj.CurrentAccountId)
                                              .ToList()
                                              .ForEach(userID =>
                                                           {
                                                               if (userEntriesData.ContainsKey(userID))
                                                                   userEntriesData[userID].Add(parentFolder);
                                                               else
                                                                   userEntriesData.Add(userID, new List<FileEntry> { parentFolder });
                                                           })
                        );

                    var userIDs = obj.UserIDs;

                    if (!userIDs.Any())
                        userIDs = filesSecurity.WhoCanRead(obj.FileEntry).Where(x => x != obj.CurrentAccountId).ToList();

                    if (obj.FileEntry.RootFolderType == FolderType.USER)
                    {
                        var folderShare = folderDao.GetFolder(Global.FolderShare);

                        foreach (var userID in userIDs)
                        {
                            var userFolderId = folderDao.GetFolderID(FileConstant.ModuleId, "my", userID.ToString(), false);

                            Folder rootFolder = null;
                            if (obj.FileEntry.ProviderEntry)
                            {
                                rootFolder = obj.FileEntry.RootFolderCreator == userID
                                                 ? folderDao.GetFolder(userFolderId)
                                                 : folderShare;
                            }
                            else if (!Equals(obj.FileEntry.RootFolderId, userFolderId))
                            {
                                rootFolder = folderShare;
                            }

                            if (rootFolder == null) continue;

                            if (userEntriesData.ContainsKey(userID))
                                userEntriesData[userID].Add(rootFolder);
                            else
                                userEntriesData.Add(userID, new List<FileEntry> { rootFolder });
                        }
                    }

                    if (obj.FileEntry.ProviderEntry && obj.FileEntry.RootFolderType == FolderType.COMMON)
                    {
                        var commonFolder = folderDao.GetFolder(Global.FolderCommon);
                        userIDs.ForEach(userID =>
                                            {
                                                if (userEntriesData.ContainsKey(userID))
                                                    userEntriesData[userID].Add(commonFolder);
                                                else
                                                    userEntriesData.Add(userID, new List<FileEntry> { commonFolder });
                                            });
                    }

                    userIDs.ForEach(userID =>
                                        {
                                            if (userEntriesData.ContainsKey(userID))
                                                userEntriesData[userID].Add(obj.FileEntry);
                                            else
                                                userEntriesData.Add(userID, new List<FileEntry> { obj.FileEntry });
                                        });
                }

                using (var tagDao = Global.DaoFactory.GetTagDao())
                {
                    var newTags = new List<Tag>();
                    var updateTags = new List<Tag>();

                    foreach (var userID in userEntriesData.Keys)
                    {
                        if (tagDao.GetNewTags(userID, obj.FileEntry).Any())
                            continue;

                        var entries = userEntriesData[userID].Distinct().ToList();

                        var exist = tagDao.GetNewTags(userID, entries.ToArray()).ToList();
                        var update = exist.Where(t => t.EntryType == FileEntryType.Folder).ToList();
                        update.ForEach(t => t.Count++);
                        updateTags.AddRange(update);

                        entries.ForEach(entry =>
                                            {
                                                if (exist.All(tag => !tag.EntryId.Equals(entry.ID)))
                                                {
                                                    newTags.Add(Tag.New(userID, entry));
                                                }
                                            });
                    }

                    if (updateTags.Any())
                        tagDao.UpdateNewTags(updateTags.ToArray());
                    if (newTags.Any())
                        tagDao.SaveTags(newTags.ToArray());
                }
            }
        }

        public static void MarkAsNew(FileEntry fileEntry)
        {
            MarkAsNew(fileEntry, null);
        }

        public static void MarkAsNew(FileEntry fileEntry, List<Guid> userIDs)
        {
            if (CoreContext.Configuration.Personal) return;

            if (fileEntry == null) return;
            userIDs = userIDs ?? new List<Guid>();

            var taskData = new AsyncTaskData
                {
                    FileEntry = (FileEntry)fileEntry.Clone(),
                    UserIDs = userIDs
                };

            if (fileEntry.RootFolderType == FolderType.BUNCH && !userIDs.Any())
            {
                var projectTeam = Global.GetProjectTeam(fileEntry);

                if (!projectTeam.Any()) return;

                taskData.UserIDs = projectTeam.ToList();
            }

            lock (SyncObj)
            {
                AsyncTasks.Add(taskData);

                if (!AsyncTasks.IsStarted)
                    AsyncTasks.Start(ExecMarkFileAsNew);
            }
        }

        public static void RemoveMarkAsNew(FileEntry fileEntry)
        {
            RemoveMarkAsNew(fileEntry, SecurityContext.CurrentAccount.ID);
        }

        public static void RemoveMarkAsNew(FileEntry fileEntry, Guid userID)
        {
            if (CoreContext.Configuration.Personal) return;

            if (fileEntry == null) return;

            using (var tagDao = Global.DaoFactory.GetTagDao())
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                if (!tagDao.GetNewTags(userID, fileEntry).Any()) return;

                object folderID;
                int valueNew;
                var userFolderId = folderDao.GetFolderID(FileConstant.ModuleId, "my", userID.ToString(), false);

                var removeTags = new List<Tag>();

                if (fileEntry is File)
                {
                    folderID = ((File)fileEntry).FolderID;

                    removeTags.Add(Tag.New(userID, fileEntry));
                    valueNew = 1;
                }
                else
                {
                    folderID = fileEntry.ID;

                    var listTags = tagDao.GetNewTags(userID, (Folder)fileEntry, true).ToList();
                    valueNew = listTags.FirstOrDefault(tag => tag.EntryId.Equals(fileEntry.ID)).Count;

                    if (Equals(fileEntry.ID, userFolderId) || Equals(fileEntry.ID, Global.FolderCommon) || Equals(fileEntry.ID, Global.FolderShare))
                    {
                        var folderTags = listTags.Where(tag => tag.EntryType == FileEntryType.Folder);

                        var providerFolderTags = folderTags.Select(tag => new KeyValuePair<Tag, Folder>(tag, folderDao.GetFolder(tag.EntryId)))
                                                           .Where(pair => pair.Value.ProviderEntry).ToList();

                        foreach (var providerFolderTag in providerFolderTags)
                        {
                            listTags.Remove(providerFolderTag.Key);
                            listTags.AddRange(tagDao.GetNewTags(userID, providerFolderTag.Value, true));
                        }
                    }

                    removeTags.AddRange(listTags);
                }

                var parentFolders = folderDao.GetParentFolders(folderID);
                parentFolders.Reverse();

                var rootFolder = parentFolders.Last();
                if (rootFolder.RootFolderType == FolderType.BUNCH)
                    parentFolders.Add(folderDao.GetFolder(Global.FolderProjects));
                else if (rootFolder.ProviderEntry && rootFolder.RootFolderType == FolderType.COMMON)
                    parentFolders.Add(folderDao.GetFolder(Global.FolderCommon));
                else if (rootFolder.RootFolderType == FolderType.USER)
                {
                    if (rootFolder.ProviderEntry && rootFolder.RootFolderCreator == userID)
                        parentFolders.Add(folderDao.GetFolder(userFolderId));
                    else if (!rootFolder.ProviderEntry && !Equals(rootFolder.RootFolderId, userFolderId)
                             || rootFolder.ProviderEntry && rootFolder.RootFolderCreator != userID)
                        parentFolders.Add(folderDao.GetFolder(Global.FolderShare));
                }

                var updateTags = new List<Tag>();
                foreach (var parentFolder in parentFolders)
                {
                    var parentTag = tagDao.GetNewTags(userID, parentFolder).FirstOrDefault();

                    if (parentTag != null)
                    {
                        parentTag.Count -= valueNew;

                        if (parentTag.Count > 0)
                        {
                            updateTags.Add(parentTag);
                        }
                        else
                        {
                            removeTags.Add(parentTag);
                        }
                    }
                }

                if (updateTags.Any())
                    tagDao.UpdateNewTags(updateTags.ToArray());
                if (removeTags.Any())
                    tagDao.RemoveTags(removeTags.ToArray());
            }
        }

        public static void RemoveMarkAsNewForAll(FileEntry fileEntry)
        {
            List<Guid> userIDs;

            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var tags = tagDao.GetTags(fileEntry.ID, fileEntry is File ? FileEntryType.File : FileEntryType.Folder, TagType.New);
                userIDs = tags.Select(tag => tag.Owner).Distinct().ToList();
            }

            foreach (var userID in userIDs)
            {
                RemoveMarkAsNew(fileEntry, userID);
            }
        }

        public static Dictionary<object, int> GetRootFoldersIdMarkedAsNew()
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                var folderIds = new[]
                    {
                        Global.FolderMy,
                        Global.FolderCommon,
                        Global.FolderShare,
                        Global.FolderProjects
                    };

                return tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, folderDao.GetFolders(folderIds).ToArray())
                             .ToDictionary(x => x.EntryId, x => x.Count);
            }
        }

        public static List<FileEntry> MarkedItems(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder", FilesCommonResource.ErrorMassage_FolderNotFound);
            if (!Global.GetFilesSecurity().CanRead(folder)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ViewFolder);
            if (folder.RootFolderType == FolderType.TRASH && !Equals(folder.ID, Global.FolderTrash)) throw new SecurityException(FilesCommonResource.ErrorMassage_ViewTrashItem);

            Dictionary<FileEntry, Tag> entryTags;

            using (var tagDao = Global.DaoFactory.GetTagDao())
            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                var tags = (tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, folder, true) ?? new List<Tag>()).ToList();

                if (!tags.Any()) return new List<FileEntry>();

                if (Equals(folder.ID, Global.FolderMy) || Equals(folder.ID, Global.FolderCommon) || Equals(folder.ID, Global.FolderShare))
                {
                    var folderTags = tags.Where(tag => tag.EntryType == FileEntryType.Folder);

                    var providerFolderTags = folderTags.Select(tag => new KeyValuePair<Tag, Folder>(tag, folderDao.GetFolder(tag.EntryId)))
                                                       .Where(pair => pair.Value.ProviderEntry).ToList();
                    providerFolderTags.Reverse();

                    foreach (var providerFolderTag in providerFolderTags)
                    {
                        tags.AddRange(tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, providerFolderTag.Value, true));
                    }
                }

                tags = tags.Distinct().ToList();
                tags.RemoveAll(tag => Equals(tag.EntryId, folder.ID));
                tags = tags.Where(t => t.EntryType == FileEntryType.Folder)
                           .Concat(tags.Where(t => t.EntryType == FileEntryType.File)).ToList();

                entryTags = tags.ToDictionary(
                    tag =>
                    tag.EntryType == FileEntryType.File
                        ? (FileEntry)fileDao.GetFile(tag.EntryId)
                        : (FileEntry)folderDao.GetFolder(tag.EntryId));
            }

            foreach (var entryTag in entryTags)
            {
                var entry = entryTag.Key;
                var parentId =
                    entryTag.Key is File
                        ? ((File)entry).FolderID
                        : ((Folder)entry).ParentFolderID;

                var parentEntry = entryTags.Keys.FirstOrDefault(entryCountTag => Equals(entryCountTag.ID, parentId));
                if (parentEntry != null)
                    entryTags[parentEntry].Count -= entryTag.Value.Count;
            }

            var result = new List<FileEntry>();

            foreach (var entryTag in entryTags)
            {
                if (!string.IsNullOrEmpty(entryTag.Key.Error))
                {
                    RemoveMarkAsNew(entryTag.Key);
                    continue;
                }

                if (entryTag.Value.Count > 0)
                {
                    result.Add(entryTag.Key);
                }
            }
            return result;
        }

        public static IEnumerable<FileEntry> SetTagsNew(IFolderDao folderDao, Folder parent, IEnumerable<FileEntry> entries)
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var totalTags = tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, parent, false).ToList();

                if (totalTags.Any())
                {
                    var parentFolderTag = Equals(Global.FolderShare, parent.ID)
                                              ? tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, folderDao.GetFolder(Global.FolderShare)).FirstOrDefault()
                                              : totalTags.FirstOrDefault(tag => tag.EntryType == FileEntryType.Folder && Equals(tag.EntryId, parent.ID));

                    totalTags.Remove(parentFolderTag);
                    var countSubNew = 0;
                    totalTags.ForEach(tag => countSubNew += tag.Count);

                    if (parentFolderTag == null)
                    {
                        parentFolderTag = Tag.New(SecurityContext.CurrentAccount.ID, parent, 0);
                        parentFolderTag.Id = -1;
                    }

                    if (parentFolderTag.Count != countSubNew)
                    {
                        if (countSubNew > 0)
                        {
                            var diff = parentFolderTag.Count - countSubNew;

                            parentFolderTag.Count -= diff;
                            if (parentFolderTag.Id == -1)
                            {
                                tagDao.SaveTags(parentFolderTag);
                            }
                            else
                            {
                                tagDao.UpdateNewTags(parentFolderTag);
                            }

                            var parentsList = folderDao.GetParentFolders(parent.ID);
                            parentsList.Reverse();
                            parentsList.Remove(parent);

                            if (parentsList.Any())
                            {
                                var rootFolder = parentsList.Last();
                                if (rootFolder.RootFolderType == FolderType.BUNCH)
                                    parentsList.Add(folderDao.GetFolder(Global.FolderProjects));
                                else if (rootFolder.RootFolderType == FolderType.USER && !Equals(rootFolder.RootFolderId, Global.FolderMy))
                                    parentsList.Add(folderDao.GetFolder(Global.FolderShare));

                                var fileSecurity = Global.GetFilesSecurity();

                                foreach (var folderFromList in parentsList)
                                {
                                    var parentTreeTag = tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, new FileEntry[] { folderFromList }).FirstOrDefault();

                                    if (parentTreeTag == null)
                                    {
                                        if (fileSecurity.CanRead(folderFromList))
                                        {
                                            tagDao.SaveTags(Tag.New(SecurityContext.CurrentAccount.ID, folderFromList, -diff));
                                        }
                                    }
                                    else
                                    {
                                        parentTreeTag.Count -= diff;
                                        tagDao.UpdateNewTags(parentTreeTag);
                                    }
                                }
                            }
                        }
                        else
                        {
                            RemoveMarkAsNew(parent);
                        }
                    }

                    entries.ToList().ForEach(
                        entry =>
                            {
                                var folder = entry as Folder;
                                if (folder != null)
                                {
                                    var curTag = totalTags.FirstOrDefault(tag => tag.EntryType == FileEntryType.Folder && tag.EntryId.Equals(folder.ID));

                                    folder.NewForMe = curTag != null ? curTag.Count : 0;
                                }
                            });
                }
            }

            return entries;
        }
    }
}