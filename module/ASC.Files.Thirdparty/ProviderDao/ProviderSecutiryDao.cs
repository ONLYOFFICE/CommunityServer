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
using ASC.Files.Core;
using ASC.Files.Core.Security;

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
            if (entries == null) return new List<FileShareRecord>();

            var result = Enumerable.Empty<FileShareRecord>();

            var defaultSelectorEntries = entries.Where(x => x != null && Default.IsMatch(x.ID));
            var otherSelectorEntries = entries.Where(x => x != null && !Default.IsMatch(x.ID));

            if (defaultSelectorEntries.Any())
                result = result.Concat(TryGetSecurityDao().GetShares(defaultSelectorEntries.ToArray()));

            if (!otherSelectorEntries.Any()) return result;

            var files = otherSelectorEntries.Where(x => x is Files.Core.File);
            var folders = otherSelectorEntries.Where(x => x is Folder);

            var foldersFiles = files.Select(x => ((Files.Core.File) x).FolderID).Distinct().ToList()
                                    .ConvertAll(x =>
                                                    {
                                                        var selector = GetSelector(x);
                                                        return selector.GetFolderDao(x).GetFolder(selector.ConvertId(x));
                                                    });

            if (foldersFiles.Any())
                folders = folders.Concat(foldersFiles.ToArray());

            if (files.Any())
            {
                var pureShareRecords = TryGetSecurityDao().GetPureShareRecords(files.ToArray()).ToList();
                pureShareRecords.ForEach(x => x.Level = -1);

                result = result.Concat(pureShareRecords);
            }

            if (folders.Any())
            {
                foreach (var folder in folders)
                {
                    var selector = GetSelector(folder.ID);

                    var parentFolders = selector.GetFolderDao(folder.ID).GetParentFolders(selector.ConvertId(folder.ID));

                    parentFolders.Reverse();

                    var pureShareRecords = GetPureShareRecords(parentFolders.Cast<FileEntry>().ToArray()).ToList();

                    pureShareRecords.ForEach(x =>
                                                 {
                                                     x.Level = parentFolders.IndexOf(new Folder
                                                         {
                                                             ID = x.EntryId
                                                         });

                                                     x.EntryId = folder.ID;
                                                 });

                    result = result.Concat(pureShareRecords);
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