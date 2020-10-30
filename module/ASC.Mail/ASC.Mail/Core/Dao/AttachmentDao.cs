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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class AttachmentDao : BaseDao, IAttachmentDao
    {
        protected static ITable table = new MailTableFactory().Create<AttachmentTable>();

        protected string CurrentUserId { get; private set; }

        public AttachmentDao(IDbManager dbManager, int tenant, string user = null) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public Attachment GetAttachment(IAttachmentExp exp)
        {
            var query = Query(AttachmentTable.TABLE_NAME)
                .InnerJoin(MailTable.TABLE_NAME.Alias(MailTable.TABLE_NAME),
                    Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.TABLE_NAME),
                        AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME)))
                .Select(MailTable.Columns.Stream.Prefix(MailTable.TABLE_NAME),
                    MailTable.Columns.User.Prefix(MailTable.TABLE_NAME))
                .Where(exp.GetExpression());

            return Db.ExecuteList(query)
                .ConvertAll(ToAttachment)
                .FirstOrDefault();
        }

        public List<Attachment> GetAttachments(IAttachmentsExp exp)
        {
            var query = Query(AttachmentTable.TABLE_NAME)
                .InnerJoin(MailTable.TABLE_NAME.Alias(MailTable.TABLE_NAME),
                    Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.TABLE_NAME),
                        AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME)))
                .Select(MailTable.Columns.Stream.Prefix(MailTable.TABLE_NAME),
                    MailTable.Columns.User.Prefix(MailTable.TABLE_NAME))
                .Where(exp.GetExpression());

            return Db.ExecuteList(query)
                .ConvertAll(ToAttachment);
        }

        public long GetAttachmentsSize(IAttachmentsExp exp)
        {
            var query = new SqlQuery(AttachmentTable.TABLE_NAME.Alias(AttachmentTable.TABLE_NAME))
                .InnerJoin(MailTable.TABLE_NAME.Alias(MailTable.TABLE_NAME),
                    Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.TABLE_NAME),
                        AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME)))
                .SelectSum(AttachmentTable.Columns.Size.Prefix(AttachmentTable.TABLE_NAME))
                .Where(exp.GetExpression());

            var size = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt64(r[0]))
                .FirstOrDefault();

            return size;
        }

        public int GetAttachmentsMaxFileNumber(IAttachmentsExp exp)
        {
            var query = new SqlQuery(AttachmentTable.TABLE_NAME.Alias(AttachmentTable.TABLE_NAME))
                .InnerJoin(MailTable.TABLE_NAME.Alias(MailTable.TABLE_NAME),
                    Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.TABLE_NAME),
                        AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME)))
                .SelectMax(AttachmentTable.Columns.FileNumber.Prefix(AttachmentTable.TABLE_NAME))
                .Where(exp.GetExpression());

            var max = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .SingleOrDefault();

            return max;
        }

        public int GetAttachmentsCount(IAttachmentsExp exp)
        {
            var query = new SqlQuery(AttachmentTable.TABLE_NAME.Alias(AttachmentTable.TABLE_NAME))
                .InnerJoin(MailTable.TABLE_NAME.Alias(MailTable.TABLE_NAME),
                    Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.TABLE_NAME),
                        AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME)))
                .SelectCount(AttachmentTable.Columns.Id.Prefix(AttachmentTable.TABLE_NAME))
                .Where(exp.GetExpression());

            var count = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .SingleOrDefault();

            return count;
        }

        public bool SetAttachmnetsRemoved(IAttachmentsExp exp)
        {
            var query = new SqlUpdate(AttachmentTable.TABLE_NAME.Alias(AttachmentTable.TABLE_NAME))
                .InnerJoin(MailTable.TABLE_NAME.Alias(MailTable.TABLE_NAME),
                    Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.TABLE_NAME),
                        AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME)))
                .Set(AttachmentTable.Columns.IsRemoved, true)
                .Where(exp.GetExpression());

            var result = Db.ExecuteNonQuery(query);

            return result > 0;
        }

        public int SaveAttachment(Attachment attachment)
        {
            var query = new SqlInsert(AttachmentTable.TABLE_NAME, true)
                .InColumnValue(AttachmentTable.Columns.Id, attachment.Id)
                .InColumnValue(AttachmentTable.Columns.MailId, attachment.MailId)
                .InColumnValue(AttachmentTable.Columns.Name, attachment.Name)
                .InColumnValue(AttachmentTable.Columns.StoredName, attachment.StoredName)
                .InColumnValue(AttachmentTable.Columns.Type, attachment.Type)
                .InColumnValue(AttachmentTable.Columns.Size, attachment.Size)
                .InColumnValue(AttachmentTable.Columns.FileNumber, attachment.FileNumber)
                .InColumnValue(AttachmentTable.Columns.IsRemoved, attachment.IsRemoved)
                .InColumnValue(AttachmentTable.Columns.ContentId, attachment.ContentId)
                .InColumnValue(AttachmentTable.Columns.Tenant, attachment.Tenant)
                .InColumnValue(AttachmentTable.Columns.MailboxId, attachment.MailboxId)
                .Identity(0, 0, true);

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        protected Attachment ToAttachment(object[] r)
        {
            var a = new Attachment
            {
                Id = Convert.ToInt32(r[0]),
                MailId = Convert.ToInt32(r[1]),
                Name = Convert.ToString(r[2]),
                StoredName = Convert.ToString(r[3]),
                Type = Convert.ToString(r[4]),
                Size = Convert.ToInt32(r[5]),
                IsRemoved = Convert.ToBoolean(r[6]),
                FileNumber = Convert.ToInt32(r[7]),
                ContentId = Convert.ToString(r[8]),
                Tenant = Convert.ToInt32(r[9]),
                MailboxId = Convert.ToInt32(r[10]),
                Stream = Convert.ToString(r[11]),
                User = Convert.ToString(r[12])
            };

            return a;
        }
    }
}