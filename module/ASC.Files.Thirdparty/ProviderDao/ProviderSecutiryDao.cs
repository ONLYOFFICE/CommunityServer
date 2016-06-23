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
            var selector = GetSelector(r.EntryId);

            selector.GetSecurityDao(r.EntryId).SetShare(r);
        }

        public IEnumerable<FileShareRecord> GetShares(params FileEntry[] entries)
        {
            var result = new List<FileShareRecord>();

            if (entries == null || !entries.Any()) return result;

            var defaultSelectorEntries = entries.Where(x => x != null && Default.IsMatch(x.ID)).ToArray();
            var otherSelectorEntries = entries.Where(x => x != null && !Default.IsMatch(x.ID)).ToArray();

            if (defaultSelectorEntries.Any())
            {
                var securityDao = TryGetSecurityDao();
                if (securityDao != null)
                {
                    var shares = securityDao.GetShares(defaultSelectorEntries);
                    if (shares != null) result.AddRange(shares);
                }
            }

            if (!otherSelectorEntries.Any()) return result;

            var files = otherSelectorEntries.Where(x => x is Files.Core.File).ToArray();
            var folders = otherSelectorEntries.Where(x => x is Folder).ToList();

            if (files.Any())
            {
                var folderIds = files.Select(x => ((Files.Core.File) x).FolderID).Distinct();
                foreach (var folderId in folderIds)
                {
                    var selector = GetSelector(folderId);
                    var folderDao = selector.GetFolderDao(folderId);
                    if (folderDao == null) continue;

                    var folder = folderDao.GetFolder(selector.ConvertId(folderId));
                    if (folder != null) folders.Add(folder);
                }

                var securityDao = TryGetSecurityDao();
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

            if (folders.Any())
            {
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
                        pureShareRecord.Level = parentFolders.IndexOf(new Folder {ID = pureShareRecord.EntryId});
                        pureShareRecord.EntryId = folder.ID;
                        result.Add(pureShareRecord);
                    }
                }
            }

            return result;
        }

        public void RemoveSubject(Guid subject)
        {
            TryGetSecurityDao().RemoveSubject(subject);
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<Guid> subjects)
        {
            return TryGetSecurityDao().GetShares(subjects);
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(params FileEntry[] entries)
        {
            return TryGetSecurityDao().GetPureShareRecords(entries);
        }

        public void DeleteShareRecords(params FileShareRecord[] records)
        {
            TryGetSecurityDao().DeleteShareRecords(records);
        }
    }
}