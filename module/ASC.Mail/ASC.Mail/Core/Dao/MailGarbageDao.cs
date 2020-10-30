/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class MailGarbageDao
    {
        public void CleanupMailboxData(MailBoxData mailbox, bool totalRemove)
        {
            if(!mailbox.IsRemoved)
                throw new Exception("Mailbox is not removed.");

            var deleteMailboxMessagesQuery = new SqlDelete(MailTable.TABLE_NAME)
                                            .Where(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                                            .Where(MailTable.Columns.Tenant, mailbox.TenantId)
                                            .Where(MailTable.Columns.User, mailbox.UserId);

            var deleteMailboxAttachmentsQuery = new SqlDelete(AttachmentTable.TABLE_NAME)
                                            .Where(AttachmentTable.Columns.MailboxId, mailbox.MailBoxId)
                                            .Where(AttachmentTable.Columns.Tenant, mailbox.TenantId);

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoFactory = new DaoFactory(db);

                    var daoMailbox = daoFactory.CreateMailboxDao();

                    var mb = daoMailbox.GetMailBox(
                        new СoncreteUserMailboxExp(mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId, true));

                    db.ExecuteNonQuery(deleteMailboxAttachmentsQuery);

                    db.ExecuteNonQuery(deleteMailboxMessagesQuery);

                    daoFactory.CreateMailboxDao().RemoveMailbox(mb);

                    if (totalRemove)
                    {
                        daoFactory.CreateFolderDao(mailbox.TenantId, mailbox.UserId).Delete();

                        var deleteContactInfoQuery = new SqlDelete(ContactInfoTable.TABLE_NAME)
                                            .Where(ContactInfoTable.Columns.User, mailbox.UserId)
                                            .Where(ContactInfoTable.Columns.Tenant, mailbox.TenantId);

                        db.ExecuteNonQuery(deleteContactInfoQuery);

                        var deleteContactsQuery = new SqlDelete(ContactsTable.TABLE_NAME)
                                            .Where(ContactsTable.Columns.User, mailbox.UserId)
                                            .Where(ContactsTable.Columns.Tenant, mailbox.TenantId);

                        db.ExecuteNonQuery(deleteContactsQuery);

                        var deleteDisplayImagesQuery = new SqlDelete(DisplayImagesTable.TABLE_NAME)
                                            .Where(DisplayImagesTable.Columns.User, mailbox.UserId)
                                            .Where(DisplayImagesTable.Columns.Tenant, mailbox.TenantId);

                        db.ExecuteNonQuery(deleteDisplayImagesQuery);

                    }

                    tx.Commit();
                }
            }

        }

        public int GetMailboxAttachsCount(MailBoxData mailBoxData)
        {
            int count;

            const string m = "m";
            const string a = "a";

            using (var db = GetDb())
            {
                var query = new SqlQuery(MailTable.TABLE_NAME.Alias(m))
                    .InnerJoin(AttachmentTable.TABLE_NAME.Alias(a),
                               Exp.EqColumns(MailTable.Columns.Id.Prefix(m), AttachmentTable.Columns.MailId.Prefix(a)))
                    .SelectCount()
                    .Where(MailTable.Columns.MailboxId.Prefix(m), mailBoxData.MailBoxId)
                    .Where(MailTable.Columns.Tenant.Prefix(m), mailBoxData.TenantId)
                    .Where(MailTable.Columns.User.Prefix(m), mailBoxData.UserId);

                count = db.ExecuteScalar<int>(query);
            }

            return count;
        }

        public List<MailAttachGarbage> GetMailboxAttachs(MailBoxData mailBoxData, int limit)
        {
            List<MailAttachGarbage> list;

            const string m = "m";
            const string a = "a";

            using (var db = GetDb())
            {
                var queryAttachemnts = new SqlQuery(MailTable.TABLE_NAME.Alias(m))
                    .InnerJoin(AttachmentTable.TABLE_NAME.Alias(a),
                               Exp.EqColumns(MailTable.Columns.Id.Prefix(m), AttachmentTable.Columns.MailId.Prefix(a)))
                    .Select(AttachmentTable.Columns.Id.Prefix(a),
                            MailTable.Columns.Stream.Prefix(m),
                            AttachmentTable.Columns.FileNumber.Prefix(a),
                            AttachmentTable.Columns.StoredName.Prefix(a),
                            AttachmentTable.Columns.Name.Prefix(a),
                            MailTable.Columns.Id.Prefix(m))
                    .Where(MailTable.Columns.MailboxId.Prefix(m), mailBoxData.MailBoxId)
                    .Where(MailTable.Columns.Tenant.Prefix(m), mailBoxData.TenantId)
                    .Where(MailTable.Columns.User.Prefix(m), mailBoxData.UserId)
                    .SetMaxResults(limit);

                list =
                    db.ExecuteList(queryAttachemnts)
                      .ConvertAll(
                          r =>
                          new MailAttachGarbage(mailBoxData.UserId, Convert.ToInt32(r[0]), r[1].ToString(), Convert.ToInt32(r[2]),
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
                var deleteQuery = new SqlDelete(AttachmentTable.TABLE_NAME)
                    .Where(Exp.In(AttachmentTable.Columns.Id, attachGarbageList.Select(a => a.Id).ToList()));

                db.ExecuteNonQuery(deleteQuery);
            }
        }

        public int GetMailboxMessagesCount(MailBoxData mailBoxData)
        {
            int count;

            using (var db = GetDb())
            {
                var query = new SqlQuery(MailTable.TABLE_NAME)
                    .SelectCount()
                    .Where(MailTable.Columns.MailboxId, mailBoxData.MailBoxId)
                    .Where(MailTable.Columns.Tenant, mailBoxData.TenantId)
                    .Where(MailTable.Columns.User, mailBoxData.UserId);

                count = db.ExecuteScalar<int>(query);
            }

            return count;
        }

        public List<MailMessageGarbage> GetMailboxMessages(MailBoxData mailBoxData, int limit)
        {
            List<MailMessageGarbage> list;

            using (var db = GetDb())
            {
                var queryAttachemnts = new SqlQuery(MailTable.TABLE_NAME)
                    .Select(MailTable.Columns.Id,
                            MailTable.Columns.Stream)
                    .Where(MailTable.Columns.MailboxId, mailBoxData.MailBoxId)
                    .Where(MailTable.Columns.Tenant, mailBoxData.TenantId)
                    .Where(MailTable.Columns.User, mailBoxData.UserId)
                    .SetMaxResults(limit);

                list =
                    db.ExecuteList(queryAttachemnts)
                      .ConvertAll(r => new MailMessageGarbage(mailBoxData.UserId, Convert.ToInt32(r[0]), r[1].ToString()))
                      .ToList();
            }

            return list;
        }

        public void CleanupMailboxMessages(List<MailMessageGarbage> messageGarbageList)
        {
            if (!messageGarbageList.Any()) return;

            using (var db = GetDb())
            {
                var deleteQuery = new SqlDelete(MailTable.TABLE_NAME)
                    .Where(Exp.In(MailTable.Columns.Id, messageGarbageList.Select(a => a.Id).ToList()));

                db.ExecuteNonQuery(deleteQuery);
            }
        }

        private DbManager GetDb()
        {
            return new DbManager(Defines.CONNECTION_STRING_NAME);
        }

    }

    
}
