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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.DataStorage;

namespace ASC.Mail.Aggregator.Dal
{
    public class MailGarbageCleanDal
    {
        public interface IMailGarbage
        {
            int Id { get; }

            string Path { get; }
        }

        public class MailAttachGarbage : IMailGarbage
        {
            public int Id { get; private set; }
            public string Path { get; private set; }

            public MailAttachGarbage(string user, int attachId, string stream, int number, string storedName)
            {
                Id = attachId;
                Path = MailStoragePathCombiner.GetFileKey(user, stream, number, storedName);
            }
        }

        public class MailMessageGarbage : IMailGarbage
        {
            public int Id { get; private set; }
            public string Path { get; private set; }

            public MailMessageGarbage(string user, int id, string stream)
            {
                Id = id;
                Path = MailStoragePathCombiner.GetBodyKey(user, stream);
            }
        }

        public void CleanupMailboxData(MailBox mailbox, bool totalRemove)
        {
            if(!mailbox.IsRemoved)
                throw new Exception("Mailbox is not removed.");
            
            var deleteMailboxQuery = new SqlDelete(MailboxTable.name)
                                            .Where(MailboxTable.Columns.id, mailbox.MailBoxId)
                                            .Where(MailboxTable.Columns.id_tenant, mailbox.TenantId)
                                            .Where(MailTable.Columns.id_user, mailbox.UserId);

            var deleteMailboxMessagesQuery = new SqlDelete(MailTable.name)
                                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                                            .Where(MailTable.Columns.id_tenant, mailbox.TenantId)
                                            .Where(MailTable.Columns.id_user, mailbox.UserId);

            var deleteMailboxAttachmentsQuery = new SqlDelete(AttachmentTable.name)
                                            .Where(AttachmentTable.Columns.id_mailbox, mailbox.MailBoxId)
                                            .Where(AttachmentTable.Columns.id_tenant, mailbox.TenantId);

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    db.ExecuteNonQuery(deleteMailboxAttachmentsQuery);

                    db.ExecuteNonQuery(deleteMailboxMessagesQuery);

                    db.ExecuteNonQuery(deleteMailboxQuery);

                    if (totalRemove)
                    {
                        var deleteFoldersQuery = new SqlDelete(FolderTable.name)
                                                    .Where(MailTable.Columns.id_tenant, mailbox.TenantId)
                                                    .Where(MailTable.Columns.id_user, mailbox.UserId);

                        db.ExecuteNonQuery(deleteFoldersQuery);

                        var deleteContactsQuery = new SqlDelete(ContactsTable.name)
                                            .Where(ContactsTable.Columns.id_user, mailbox.UserId)
                                            .Where(ContactsTable.Columns.id_tenant, mailbox.TenantId);

                        db.ExecuteNonQuery(deleteContactsQuery);

                        var deleteDisplayImagesQuery = new SqlDelete(DisplayImagesTable.name)
                                            .Where(DisplayImagesTable.Columns.id_user, mailbox.UserId)
                                            .Where(DisplayImagesTable.Columns.id_tenant, mailbox.TenantId);

                        db.ExecuteNonQuery(deleteDisplayImagesQuery);

                    }

                    tx.Commit();
                }
            }

        }

        public int GetMailboxAttachsCount(MailBox mailBox)
        {
            int count;

            const string m = "m";
            const string a = "a";

            using (var db = GetDb())
            {
                var query = new SqlQuery(MailTable.name.Alias(m))
                    .InnerJoin(AttachmentTable.name.Alias(a),
                               Exp.EqColumns(MailTable.Columns.id.Prefix(m), AttachmentTable.Columns.id_mail.Prefix(a)))
                    .SelectCount()
                    .Where(MailTable.Columns.id_mailbox.Prefix(m), mailBox.MailBoxId)
                    .Where(MailTable.Columns.id_tenant.Prefix(m), mailBox.TenantId)
                    .Where(MailTable.Columns.id_user.Prefix(m), mailBox.UserId);

                count = db.ExecuteScalar<int>(query);
            }

            return count;
        }

        public List<MailAttachGarbage> GetMailboxAttachs(MailBox mailBox, int limit)
        {
            List<MailAttachGarbage> list;

            const string m = "m";
            const string a = "a";

            using (var db = GetDb())
            {
                var queryAttachemnts = new SqlQuery(MailTable.name.Alias(m))
                    .InnerJoin(AttachmentTable.name.Alias(a),
                               Exp.EqColumns(MailTable.Columns.id.Prefix(m), AttachmentTable.Columns.id_mail.Prefix(a)))
                    .Select(AttachmentTable.Columns.id.Prefix(a),
                            MailTable.Columns.stream.Prefix(m),
                            AttachmentTable.Columns.file_number.Prefix(a),
                            AttachmentTable.Columns.stored_name.Prefix(a),
                            AttachmentTable.Columns.name.Prefix(a),
                            MailTable.Columns.id.Prefix(m))
                    .Where(MailTable.Columns.id_mailbox.Prefix(m), mailBox.MailBoxId)
                    .Where(MailTable.Columns.id_tenant.Prefix(m), mailBox.TenantId)
                    .Where(MailTable.Columns.id_user.Prefix(m), mailBox.UserId)
                    .SetMaxResults(limit);

                list =
                    db.ExecuteList(queryAttachemnts)
                      .ConvertAll(
                          r =>
                          new MailAttachGarbage(mailBox.UserId, Convert.ToInt32(r[0]), r[1].ToString(), Convert.ToInt32(r[2]),
                                                r[3] != null ? r[3].ToString() : r[4].ToString()))
                      .ToList();
            }

            return list;
        }

        public void CleanupMailboxAttachs(List<MailAttachGarbage> attachGarbageList)
        {
            if (!attachGarbageList.Any()) return;

            using (var db = GetDb())
            {
                var deleteQuery = new SqlDelete(AttachmentTable.name)
                    .Where(Exp.In(AttachmentTable.Columns.id, attachGarbageList.Select(a => a.Id).ToList()));

                db.ExecuteNonQuery(deleteQuery);
            }
        }

        public int GetMailboxMessagesCount(MailBox mailBox)
        {
            int count;

            using (var db = GetDb())
            {
                var query = new SqlQuery(MailTable.name)
                    .SelectCount()
                    .Where(MailTable.Columns.id_mailbox, mailBox.MailBoxId)
                    .Where(MailTable.Columns.id_tenant, mailBox.TenantId)
                    .Where(MailTable.Columns.id_user, mailBox.UserId);

                count = db.ExecuteScalar<int>(query);
            }

            return count;
        }

        public List<MailMessageGarbage> GetMailboxMessages(MailBox mailBox, int limit)
        {
            List<MailMessageGarbage> list;

            using (var db = GetDb())
            {
                var queryAttachemnts = new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.id,
                            MailTable.Columns.stream)
                    .Where(MailTable.Columns.id_mailbox, mailBox.MailBoxId)
                    .Where(MailTable.Columns.id_tenant, mailBox.TenantId)
                    .Where(MailTable.Columns.id_user, mailBox.UserId)
                    .SetMaxResults(limit);

                list =
                    db.ExecuteList(queryAttachemnts)
                      .ConvertAll(r => new MailMessageGarbage(mailBox.UserId, Convert.ToInt32(r[0]), r[1].ToString()))
                      .ToList();
            }

            return list;
        }

        public void CleanupMailboxMessages(List<MailMessageGarbage> messageGarbageList)
        {
            if (!messageGarbageList.Any()) return;

            using (var db = GetDb())
            {
                var deleteQuery = new SqlDelete(MailTable.name)
                    .Where(Exp.In(MailTable.Columns.id, messageGarbageList.Select(a => a.Id).ToList()));

                db.ExecuteNonQuery(deleteQuery);
            }
        }

        private DbManager GetDb()
        {
            return new DbManager(MailBoxManager.ConnectionStringName);
        }

    }

    
}
