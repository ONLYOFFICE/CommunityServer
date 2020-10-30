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
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class MailInfoDao : IMailInfoDao
    {
        public IDbManager Db { get; private set; }
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public MailInfoDao(IDbManager dbManager, int tenant, string user)
        {
            Db = dbManager;
            Tenant = tenant;
            User = user;
        }

        private const string MM_ALIAS = "mm";
        private const string MTM_ALIAS = "tm";
        private const string UFXM_ALIAS = "ufxm";

        private static readonly string CountMailId = "count(" + MailTable.Columns.Id.Prefix(MM_ALIAS) + ")";

        private static readonly string ConcatTagIds =
            string.Format(
                "(SELECT CAST(group_concat({4}.{0} ORDER BY {4}.{3} SEPARATOR ',') AS CHAR) from {1} as {4} WHERE {4}.{2} = {5}.{6}) tagIds",
                TagMailTable.Columns.TagId,
                TagMailTable.TABLE_NAME,
                TagMailTable.Columns.MailId,
                TagMailTable.Columns.TimeCreated,
                MTM_ALIAS,
                MM_ALIAS,
                MailTable.Columns.Id);

        public List<MailInfo> GetMailInfoList(IMessagesExp exp, bool skipSelectTags = false)
        {
            var query = new SqlQuery(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                .Select(MailTable.Columns.Id.Prefix(MM_ALIAS),
                    MailTable.Columns.From.Prefix(MM_ALIAS),
                    MailTable.Columns.To.Prefix(MM_ALIAS),
                    MailTable.Columns.Cc.Prefix(MM_ALIAS),
                    MailTable.Columns.Reply.Prefix(MM_ALIAS),
                    MailTable.Columns.Subject.Prefix(MM_ALIAS),
                    MailTable.Columns.Importance.Prefix(MM_ALIAS),
                    MailTable.Columns.DateSent.Prefix(MM_ALIAS),
                    MailTable.Columns.Size.Prefix(MM_ALIAS),
                    MailTable.Columns.AttachCount.Prefix(MM_ALIAS),
                    MailTable.Columns.Unread.Prefix(MM_ALIAS),
                    MailTable.Columns.IsAnswered.Prefix(MM_ALIAS),
                    MailTable.Columns.IsForwarded.Prefix(MM_ALIAS),
                    skipSelectTags ? "\"\" as tagIds" : ConcatTagIds,
                    MailTable.Columns.FolderRestore.Prefix(MM_ALIAS),
                    MailTable.Columns.Folder.Prefix(MM_ALIAS),
                    MailTable.Columns.ChainId.Prefix(MM_ALIAS),
                    MailTable.Columns.ChainDate.Prefix(MM_ALIAS),
                    MailTable.Columns.MailboxId.Prefix(MM_ALIAS),
                    MailTable.Columns.CalendarUid.Prefix(MM_ALIAS),
                    MailTable.Columns.Stream.Prefix(MM_ALIAS),
                    MailTable.Columns.Uidl.Prefix(MM_ALIAS),
                    MailTable.Columns.IsRemoved.Prefix(MM_ALIAS),
                    MailTable.Columns.Introduction.Prefix(MM_ALIAS));

            if (exp.TagIds != null && exp.TagIds.Any())
            {
                query
                    .InnerJoin(TagMailTable.TABLE_NAME.Alias(MTM_ALIAS),
                        Exp.EqColumns(MailTable.Columns.Id.Prefix(MM_ALIAS),
                            TagMailTable.Columns.MailId.Prefix(MTM_ALIAS)))
                    .Where(Exp.In(TagMailTable.Columns.TagId.Prefix(MTM_ALIAS), exp.TagIds))
                    .GroupBy(1)
                    .Having(Exp.Eq(CountMailId, exp.TagIds.Count));
            }

            if (exp.UserFolderId.HasValue)
            {
                query
                    .InnerJoin(UserFoldertXMailTable.TABLE_NAME.Alias(UFXM_ALIAS),
                        Exp.EqColumns(MailTable.Columns.Id.Prefix(MM_ALIAS),
                            UserFoldertXMailTable.Columns.MailId.Prefix(UFXM_ALIAS)))
                    .Where(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS), exp.UserFolderId.Value);
            }

            query.Where(exp.GetExpression());

            if (exp.StartIndex.HasValue)
            {
                query.SetFirstResult(exp.StartIndex.Value);
            }

            if (exp.Limit.HasValue)
            {
                query.SetMaxResults(exp.Limit.Value);
            }

            if (!string.IsNullOrEmpty(exp.OrderBy))
            {
                var sortField = MailTable.Columns.DateSent.Prefix(MM_ALIAS);

                if (exp.OrderBy == Defines.ORDER_BY_SUBJECT)
                {
                    sortField = MailTable.Columns.Subject.Prefix(MM_ALIAS);
                }
                else if (exp.OrderBy == Defines.ORDER_BY_SENDER)
                {
                    sortField = MailTable.Columns.From.Prefix(MM_ALIAS);
                }
                else if (exp.OrderBy == Defines.ORDER_BY_DATE_CHAIN)
                {
                    sortField = MailTable.Columns.ChainDate.Prefix(MM_ALIAS);
                }

                query.OrderBy(sortField, exp.OrderAsc != null && exp.OrderAsc.Value);
            }

            var list = Db.ExecuteList(query)
                .ConvertAll(ToMailInfo);

            return list;
        }

        public long GetMailInfoTotal(IMessagesExp exp)
        {
            long total;

            var query = new SqlQuery(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                .SelectCount(MailTable.Columns.Id.Prefix(MM_ALIAS));

            if (exp.TagIds != null && exp.TagIds.Any())
            {
                query
                    .InnerJoin(TagMailTable.TABLE_NAME.Alias(MTM_ALIAS),
                        Exp.EqColumns(MailTable.Columns.Id.Prefix(MM_ALIAS),
                            TagMailTable.Columns.MailId.Prefix(MTM_ALIAS)))
                    .Where(Exp.In(TagMailTable.Columns.TagId.Prefix(MTM_ALIAS), exp.TagIds))
                    .GroupBy(MailTable.Columns.Id.Prefix(MM_ALIAS))
                    .Having(Exp.Eq(CountMailId, exp.TagIds.Count));
            }

            if (exp.UserFolderId.HasValue)
            {
                query
                    .InnerJoin(UserFoldertXMailTable.TABLE_NAME.Alias(UFXM_ALIAS),
                        Exp.EqColumns(MailTable.Columns.Id.Prefix(MM_ALIAS),
                            UserFoldertXMailTable.Columns.MailId.Prefix(UFXM_ALIAS)))
                    .Where(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS), exp.UserFolderId.Value);
            }

            query.Where(exp.GetExpression());

            if (exp.TagIds != null && exp.TagIds.Any())
            {
                var queryTempCount = new SqlQuery()
                    .SelectCount()
                    .From(query, "tbl");

                total = Db.ExecuteScalar<long>(queryTempCount);
            }
            else
            {
                total = Db.ExecuteScalar<long>(query);
            }

            return total;
        }

        public Dictionary<int, int> GetMailCount(IMessagesExp exp)
        {
            var query = new SqlQuery(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                .Select(MailTable.Columns.Folder.Prefix(MM_ALIAS))
                .SelectCount()
                .Where(exp.GetExpression())
                .GroupBy(MailTable.Columns.Folder.Prefix(MM_ALIAS));

            return Db.ExecuteList(query)
                .ConvertAll(r => new
                {
                    folder = Convert.ToInt32(r[0]),
                    count = Convert.ToInt32(r[1])
                })
                .ToDictionary(o => o.folder, o => o.count);
        }

        public Dictionary<uint, int> GetMailUserFolderCount(List<int> userFolderIds, bool? unread = null)
        {
            var exp = Exp.Eq(UserFoldertXMailTable.Columns.Tenant.Prefix(UFXM_ALIAS), Tenant) &
                      Exp.Eq(UserFoldertXMailTable.Columns.User.Prefix(UFXM_ALIAS), User) &
                      Exp.In(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS), userFolderIds);

            if (unread.HasValue)
            {
                exp = exp & Exp.Eq(MailTable.Columns.Unread.Prefix(MM_ALIAS), unread.Value);
            }

            var query = new SqlQuery(UserFoldertXMailTable.TABLE_NAME.Alias(UFXM_ALIAS))
                .InnerJoin(MailTable.TABLE_NAME.Alias(MM_ALIAS),
                    Exp.EqColumns(
                        UserFoldertXMailTable.Columns.MailId.Prefix(UFXM_ALIAS),
                        MailTable.Columns.Id.Prefix(MM_ALIAS)))
                .Select(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS))
                .SelectCount()
                .Where(exp)
                .GroupBy(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS));

            var result = Db.ExecuteList(query)
                .ConvertAll(r => new
                {
                    folder = Convert.ToUInt32(r[0]),
                    count = Convert.ToInt32(r[1])
                })
                .ToDictionary(o => o.folder, o => o.count);

            return result;
        }

        public Dictionary<uint, int> GetMailUserFolderCount(bool? unread = null)
        {
            var exp = Exp.Eq(UserFoldertXMailTable.Columns.Tenant.Prefix(UFXM_ALIAS), Tenant) &
                      Exp.Eq(UserFoldertXMailTable.Columns.User.Prefix(UFXM_ALIAS), User);

            if (unread.HasValue)
            {
                exp = exp & Exp.Eq(MailTable.Columns.Unread.Prefix(MM_ALIAS), unread.Value);
            }

            var query = new SqlQuery(UserFoldertXMailTable.TABLE_NAME.Alias(UFXM_ALIAS))
                .InnerJoin(MailTable.TABLE_NAME.Alias(MM_ALIAS),
                    Exp.EqColumns(
                        UserFoldertXMailTable.Columns.MailId.Prefix(UFXM_ALIAS),
                        MailTable.Columns.Id.Prefix(MM_ALIAS)))
                .Select(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS))
                .SelectCount()
                .Where(exp)
                .GroupBy(UserFoldertXMailTable.Columns.FolderId.Prefix(UFXM_ALIAS));

            var result = Db.ExecuteList(query)
                .ConvertAll(r => new
                {
                    folder = Convert.ToUInt32(r[0]),
                    count = Convert.ToInt32(r[1])
                })
                .ToDictionary(o => o.folder, o => o.count);

            return result;
        }

        public Tuple<int, int> GetRangeMails(IMessagesExp exp)
        {
            var query = new SqlQuery(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                .SelectMin(MailTable.Columns.Id.Prefix(MM_ALIAS))
                .SelectMax(MailTable.Columns.Id.Prefix(MM_ALIAS))
                .Where(exp.GetExpression());

            var range = Db.ExecuteList(query)
                .ConvertAll(r => new Tuple<int, int>(Convert.ToInt32(r[0]), Convert.ToInt32(r[1])))
                .SingleOrDefault();

            return range;
        }

        public T GetFieldMaxValue<T>(IMessagesExp exp, string field)
        {
            var fieldQuery = new SqlQuery(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                .SelectMax(field.Prefix(MM_ALIAS))
                .Where(exp.GetExpression());

            var fieldVal = Db.ExecuteScalar<T>(fieldQuery);

            return fieldVal;
        }

        public int SetFieldValue<T>(IMessagesExp exp, string field, T value)
        {
            var query =
                new SqlUpdate(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                    .Set(field.Prefix(MM_ALIAS), value)
                    .Where(exp.GetExpression());

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int SetFieldsEqual(IMessagesExp exp, string fieldFrom, string fieldTo)
        {
            var query =
                new SqlUpdate(MailTable.TABLE_NAME.Alias(MM_ALIAS))
                    .Set(string.Format("{0}={1}", fieldTo.Prefix(MM_ALIAS), fieldFrom.Prefix(MM_ALIAS)))
                    .Where(exp.GetExpression());

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        protected MailInfo ToMailInfo(object[] r)
        {
            var mailInfo = new MailInfo
            {
                Id = Convert.ToInt32(r[0]),
                From = Convert.ToString(r[1]),
                To = Convert.ToString(r[2]),
                Cc = Convert.ToString(r[3]),
                ReplyTo = Convert.ToString(r[4]),
                Subject = Convert.ToString(r[5]),
                Importance = Convert.ToBoolean(r[6]),
                DateSent = Convert.ToDateTime(r[7]),
                Size = Convert.ToInt32(r[8]),
                HasAttachments = Convert.ToInt32(r[9]) > 0,
                IsNew = Convert.ToBoolean(r[10]),
                IsAnswered = Convert.ToBoolean(r[11]),
                IsForwarded = Convert.ToBoolean(r[12]),
                LabelsString = Convert.ToString(r[13]),
                FolderRestore = (FolderType) Convert.ToInt32(r[14]),
                Folder = (FolderType) Convert.ToInt32(r[15]),
                ChainId = Convert.ToString(r[16]),
                ChainDate = Convert.ToDateTime(r[17]),
                MailboxId = Convert.ToInt32(r[18]),
                CalendarUid = r[18] != null ? Convert.ToString(r[19]) : null,
                Stream = Convert.ToString(r[20]),
                Uidl = Convert.ToString(r[21]),
                IsRemoved = Convert.ToBoolean(r[22]),
                Intoduction = Convert.ToString(r[23])
            };

            return mailInfo;
        }
    }
}