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
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal class GoogleDriveSecurityDao : GoogleDriveDaoBase, ISecurityDao
    {
        public GoogleDriveSecurityDao(GoogleDriveDaoSelector.GoogleDriveInfo providerInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
            : base(providerInfo, googleDriveDaoSelector)
        {

        }

        public void SetShare(FileShareRecord r)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                if (r.Share == FileShare.None)
                {
                    if (r.EntryType == FileEntryType.Folder)
                    {
                        var entryIDs = DbManager.ExecuteList(Query("files_thirdparty_id_mapping")
                                                                 .Select("hash_id")
                                                                 .Where(Exp.Like("id", r.EntryId.ToString(), SqlLike.StartWith)))
                                                .ConvertAll(x => x[0]);

                        DbManager.ExecuteNonQuery(Delete("files_security")
                                                      .Where(Exp.In("entry_id", entryIDs) &
                                                             Exp.Eq("subject", r.Subject.ToString())));
                    }
                    else
                    {
                        var d2 = Delete("files_security")
                            .Where(Exp.Eq("entry_id", MappingID(r.EntryId, true)))
                            .Where("entry_type", (int) FileEntryType.File)
                            .Where("subject", r.Subject.ToString());

                        DbManager.ExecuteNonQuery(d2);
                    }
                }
                else
                {
                    var i = new SqlInsert("files_security", true)
                        .InColumnValue("tenant_id", r.Tenant)
                        .InColumnValue("entry_id", MappingID(r.EntryId, true))
                        .InColumnValue("entry_type", (int) r.EntryType)
                        .InColumnValue("subject", r.Subject.ToString())
                        .InColumnValue("owner", r.Owner.ToString())
                        .InColumnValue("security", (int) r.Share)
                        .InColumnValue("timestamp", DateTime.UtcNow);

                    DbManager.ExecuteNonQuery(i);
                }

                tx.Commit();
            }
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<Guid> subjects)
        {
            return null;
        }

        public IEnumerable<FileShareRecord> GetShares(params FileEntry[] entry)
        {
            return null;
        }

        public void RemoveSubject(Guid subject)
        {
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(params FileEntry[] entries)
        {
            return null;
        }

        public void DeleteShareRecords(params FileShareRecord[] records)
        {
        }
    }
}