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


using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using System;
using System.Collections.Generic;

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
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (r.Share == FileShare.None)
                {
                    if (r.EntryType == FileEntryType.Folder)
                    {
                        var entryIDs = db.ExecuteList(Query("files_thirdparty_id_mapping")
                                                                 .Select("hash_id")
                                                                 .Where(Exp.Like("id", r.EntryId.ToString(), SqlLike.StartWith)))
                                                .ConvertAll(x => x[0]);

                        db.ExecuteNonQuery(Delete("files_security")
                                                      .Where(Exp.In("entry_id", entryIDs) &
                                                             Exp.Eq("subject", r.Subject.ToString())));
                    }
                    else
                    {
                        var d2 = Delete("files_security")
                            .Where(Exp.Eq("entry_id", MappingID(r.EntryId, true)))
                            .Where("entry_type", (int)FileEntryType.File)
                            .Where("subject", r.Subject.ToString());

                        db.ExecuteNonQuery(d2);
                    }
                }
                else
                {
                    var i = new SqlInsert("files_security", true)
                        .InColumnValue("tenant_id", r.Tenant)
                        .InColumnValue("entry_id", MappingID(r.EntryId, true))
                        .InColumnValue("entry_type", (int)r.EntryType)
                        .InColumnValue("subject", r.Subject.ToString())
                        .InColumnValue("owner", r.Owner.ToString())
                        .InColumnValue("security", (int)r.Share)
                        .InColumnValue("timestamp", DateTime.UtcNow);

                    db.ExecuteNonQuery(i);
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

        public bool IsShared(object entryId, FileEntryType type)
        {
            throw new NotImplementedException();
        }
    }
}