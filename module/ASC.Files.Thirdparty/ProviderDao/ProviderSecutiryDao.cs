/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Files.Core;
using ASC.Files.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderSecurityDao : ProviderDaoBase, ISecurityDao
    {
        public void Dispose()
        {

        }

        public void SetShare(FileShareRecord r)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                securityDao.SetShare(r);
            }
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<FileEntry> entries)
        {
            var result = new List<FileShareRecord>();

            if (entries == null || !entries.Any()) return result;

            var defaultSelectorEntries = entries.Where(x => x != null && Default.IsMatch(x.ID)).ToArray();
            var otherSelectorEntries = entries.Where(x => x != null && !Default.IsMatch(x.ID)).ToArray();

            if (defaultSelectorEntries.Any())
            {
                using (var securityDao = TryGetSecurityDao())
                {
                    if (securityDao != null)
                    {
                        var shares = securityDao.GetShares(defaultSelectorEntries);
                        if (shares != null) result.AddRange(shares);
                    }
                }
            }

            if (!otherSelectorEntries.Any()) return result;

            var files = otherSelectorEntries.Where(x => x.FileEntryType == FileEntryType.File).ToArray();
            var folders = otherSelectorEntries.Where(x => x.FileEntryType == FileEntryType.Folder).ToList();

            if (files.Any())
            {
                var folderIds = files.Select(x => ((File) x).FolderID).Distinct();
                foreach (var folderId in folderIds)
                {
                    GetFoldersForShare(folderId, folders);
                }

                using (var securityDao = TryGetSecurityDao())
                {
                    if (securityDao != null)
                    {
                        var pureShareRecords = securityDao.GetPureShareRecords(files);
                        if (pureShareRecords != null)
                        {
                            foreach (var pureShareRecord in pureShareRecords)
                            {
                                if (pureShareRecord == null) continue;
                                pureShareRecord.Level = -1;
                                result.Add(pureShareRecord);
                            }
                        }
                    }
                }
            }

            result.AddRange(GetShareForFolders(folders));

            return result;
        }

        public IEnumerable<FileShareRecord> GetShares(FileEntry entry)
        {
            var result = new List<FileShareRecord>();

            if (entry == null) return result;

            if (Default.IsMatch(entry.ID))
            {
                using (var securityDao = TryGetSecurityDao())
                {
                    if (securityDao != null)
                    {
                        var shares = securityDao.GetShares(entry);
                        if (shares != null) result.AddRange(shares);
                    }
                }

                return result;
            }


            var file = entry as File;
            var folders = new List<FileEntry>();
            var entryFolder = entry as Folder;
            if (entryFolder != null)
            {
                folders.Add(entryFolder);
            }

            if (file != null)
            {
                GetFoldersForShare(file.FolderID, folders);

                using (var securityDao = TryGetSecurityDao())
                {
                    if (securityDao != null)
                    {
                        var pureShareRecords = securityDao.GetPureShareRecords(entry);
                        if (pureShareRecords != null)
                        {
                            foreach (var pureShareRecord in pureShareRecords)
                            {
                                if (pureShareRecord == null) continue;
                                pureShareRecord.Level = -1;
                                result.Add(pureShareRecord);
                            }
                        }
                    }
                }
            }

            result.AddRange(GetShareForFolders(folders));

            return result;
        }

        private void GetFoldersForShare(object folderId, ICollection<FileEntry> folders)
        {
            var selector = GetSelector(folderId);
            var folderDao = selector.GetFolderDao(folderId);
            if (folderDao == null) return;

            var folder = folderDao.GetFolder(selector.ConvertId(folderId));
            if (folder != null) folders.Add(folder);
        }

        private List<FileShareRecord> GetShareForFolders(IReadOnlyCollection<FileEntry> folders)
        {
            if (!folders.Any()) return new List<FileShareRecord>();

            var result = new List<FileShareRecord>();

            foreach (var folder in folders)
            {
                var selector = GetSelector(folder.ID);
                var folderDao = selector.GetFolderDao(folder.ID);
                if (folderDao == null) continue;

                var parentFolders = folderDao.GetParentFolders(selector.ConvertId(folder.ID));
                if (parentFolders == null || !parentFolders.Any()) continue;

                parentFolders.Reverse();
                var pureShareRecords = GetPureShareRecords(parentFolders.Cast<FileEntry>().ToArray());
                if (pureShareRecords == null) continue;

                foreach (var pureShareRecord in pureShareRecords)
                {
                    if (pureShareRecord == null) continue;
                    pureShareRecord.Level = parentFolders.IndexOf(new Folder { ID = pureShareRecord.EntryId });
                    pureShareRecord.EntryId = folder.ID;
                    result.Add(pureShareRecord);
                }
            }

            return result;
        }

        public void RemoveSubject(Guid subject)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                securityDao.RemoveSubject(subject);
            }
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<Guid> subjects)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                return securityDao.GetShares(subjects);
            }
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(IEnumerable<FileEntry> entries)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                return securityDao.GetPureShareRecords(entries);
            }
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(FileEntry entry)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                return securityDao.GetPureShareRecords(entry);
            }
        }

        public void DeleteShareRecords(IEnumerable<FileShareRecord> records)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                securityDao.DeleteShareRecords(records);
            }
        }

        public bool IsShared(object entryId, FileEntryType type)
        {
            using (var securityDao = TryGetSecurityDao())
            {
                return securityDao.IsShared(entryId, type);
            }
        }
    }
}