/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class UserFolderXMailDao : BaseDao, IUserFolderXMailDao
    {
        protected static ITable table = new MailTableFactory().Create<UserFoldertXMailTable>();

        protected string CurrentUserId { get; private set; }

        public UserFolderXMailDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public UserFolderXMail Get(int mailId)
        {
            var query = Query()
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId)
                .Where(UserFoldertXMailTable.Columns.MailId, mailId);

            var result = Db.ExecuteList(query)
                .ConvertAll(ToUserFolderXMail)
                .SingleOrDefault();

            return result;
        }

        public List<UserFolderXMail> GetList(uint? folderId = null)
        {
            var query = Query()
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId);

            if (folderId.HasValue)
            {
                query.Where(UserFoldertXMailTable.Columns.FolderId, folderId.Value);
            }

            var list = Db.ExecuteList(query)
                .ConvertAll(ToUserFolderXMail);

            return list;
        }

        public List<int> GetMailIds(uint folderId)
        {
            var query = new SqlQuery(UserFoldertXMailTable.TABLE_NAME)
                .Select(UserFoldertXMailTable.Columns.MailId)
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId)
                .Where(UserFoldertXMailTable.Columns.FolderId, folderId);

            var list = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]));

            return list;
        }

        private delegate SqlInsert CreateInsertDelegate();

        public void SetMessagesFolder(IEnumerable<int> messageIds, uint folderId)
        {
            var idMessages = messageIds as IList<int> ?? messageIds.ToList();
            if (!idMessages.Any())
                return;

            CreateInsertDelegate createInsertQuery = ()
                => new SqlInsert(UserFoldertXMailTable.TABLE_NAME)
                    .IgnoreExists(true)
                    .InColumns(UserFoldertXMailTable.Columns.Tenant,
                        UserFoldertXMailTable.Columns.User,
                        UserFoldertXMailTable.Columns.MailId,
                        UserFoldertXMailTable.Columns.FolderId);

            var insertQuery = createInsertQuery();

            int i, messagessLen;
            for (i = 0, messagessLen = idMessages.Count; i < messagessLen; i++)
            {
                var messageId = idMessages[i];

                insertQuery
                    .Values(Tenant, CurrentUserId, messageId, folderId);

                if ((i % 100 != 0 || i == 0) && i + 1 != messagessLen)
                    continue;

                Db.ExecuteNonQuery(insertQuery);

                insertQuery = createInsertQuery();
            }
        }

        public int Save(UserFolderXMail item)
        {
            var query = new SqlInsert(UserFoldertXMailTable.TABLE_NAME, true)
                .InColumnValue(UserFoldertXMailTable.Columns.Tenant, item.Tenant)
                .InColumnValue(UserFoldertXMailTable.Columns.User, item.User)
                .InColumnValue(UserFoldertXMailTable.Columns.MailId, item.MailId)
                .InColumnValue(UserFoldertXMailTable.Columns.FolderId, item.FolderId);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Remove(int? mailId = null, uint? folderId = null)
        {
            var query = new SqlDelete(UserFoldertXMailTable.TABLE_NAME)
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId);

            if (mailId.HasValue)
            {
                query.Where(UserFoldertXMailTable.Columns.MailId, mailId.Value);
            }

            if (folderId.HasValue)
            {
                query.Where(UserFoldertXMailTable.Columns.FolderId, folderId.Value);
            }

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Remove(List<int> mailIds)
        {
            var query = new SqlDelete(UserFoldertXMailTable.TABLE_NAME)
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId)
                .Where(Exp.In(UserFoldertXMailTable.Columns.MailId, mailIds));

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        private static readonly string QueryDeleteFormat =
                string.Format(
                    "delete t from {0} t inner join {1} m " +
                    "on t.{2} = m.{3} and t.{4} = m.{5} and t.{6} = m.{7} " +
                    "where m.{8} = @mailbox_id and m.{5} = @tenant and m.{7} = @user",
                    UserFoldertXMailTable.TABLE_NAME, MailTable.TABLE_NAME,
                    UserFoldertXMailTable.Columns.MailId, MailTable.Columns.Id,
                    UserFoldertXMailTable.Columns.Tenant, MailTable.Columns.Tenant,
                    UserFoldertXMailTable.Columns.User, MailTable.Columns.User,
                    MailTable.Columns.MailboxId);

        public int RemoveByMailbox(int mailboxId)
        {
            return Db.ExecuteNonQuery(QueryDeleteFormat, new {mailbox_id = mailboxId, tenant = Tenant, user = CurrentUserId});
        }

        protected UserFolderXMail ToUserFolderXMail(object[] r)
        {
            var folderXMail = new UserFolderXMail
            {
                Tenant = Convert.ToInt32(r[0]),
                User = Convert.ToString(r[1]),
                MailId = Convert.ToInt32(r[2]),
                FolderId = Convert.ToUInt32(r[3]),
                TimeModified = Convert.ToDateTime(r[4])
            };

            return folderXMail;
        }
    }
}