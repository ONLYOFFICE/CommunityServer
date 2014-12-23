/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;

namespace ASC.Projects.Data.DAO
{
    internal class CachedMessageDao : MessageDao
    {
        private readonly HttpRequestDictionary<Message> messageCache = new HttpRequestDictionary<Message>("message");

        public CachedMessageDao(string dbId, int tenant) : base(dbId, tenant)
        {
        }

        public override Message GetById(int id)
        {
            return messageCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetBaseById(id));
        }

        private Message GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Message Save(Message msg)
        {
            if (msg != null)
            {
                ResetCache(msg.ID);
            }
            return base.Save(msg);
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        private void ResetCache(int messageId)
        {
            messageCache.Reset(messageId.ToString(CultureInfo.InvariantCulture));
        }
    }

    class MessageDao : BaseDao, IMessageDao
    {
        public MessageDao(string dbId, int tenant)
            : base(dbId, tenant)
        {
        }

        public List<Message> GetAll()
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery()).ConvertAll(ToMessage);
            }
        }

        public List<Message> GetByProject(int projectId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where("t.project_id", projectId).OrderBy("t.create_on", false))
                    .ConvertAll(ToMessage);
            }
        }

        public List<Message> GetMessages(int startIndex, int max)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = CreateQuery()
                    .OrderBy("t.create_on", false)
                    .SetFirstResult(startIndex)
                    .SetMaxResults(max);

                return db.ExecuteList(query).ConvertAll(ToMessage);
            }
        }

        public List<Message> GetRecentMessages(int offset, int max, params int[] projects)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = CreateQuery()
                    .SetFirstResult(offset)
                    .OrderBy("t.create_on", false)
                    .SetMaxResults(max);
                if (projects != null && 0 < projects.Length)
                {
                    query.Where(Exp.In("t.project_id", projects));
                }
                return db.ExecuteList(query).ConvertAll(ToMessage);
            }
        }

        public List<Message> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = CreateQuery();

                if (filter.Max > 0 && filter.Max < 150000)
                {
                    query.SetFirstResult((int) filter.Offset);
                    query.SetMaxResults((int) filter.Max);
                }

                query.OrderBy("t.status", true);

                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    var sortColumns = filter.SortColumns["Message"];
                    sortColumns.Remove(filter.SortBy);

                    query.OrderBy(GetSortFilter(filter.SortBy), filter.SortOrder);

                    foreach (var sort in sortColumns.Keys)
                    {
                        query.OrderBy(GetSortFilter(sort), sortColumns[sort]);
                    }
                }

                query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

                return db.ExecuteList(query).ConvertAll(ToMessage);
            }
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = new SqlQuery(MessagesTable + " t")
                    .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                    .Select("t.id")
                    .GroupBy("t.id")
                    .Where("t.tenant_id", Tenant);

                query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

                var queryCount = new SqlQuery().SelectCount().From(query, "t1");
                return db.ExecuteScalar<int>(queryCount);
            }
        }

        public virtual Message GetById(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where("t.id", id))
                    .ConvertAll(ToMessage)
                    .SingleOrDefault();
            }
        }

        public bool IsExists(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var count = db.ExecuteScalar<long>(Query(MessagesTable).SelectCount().Where("id", id));
                return 0 < count;
            }
        }

        public virtual Message Save(Message msg)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var insert = Insert(MessagesTable)
                    .InColumnValue("id", msg.ID)
                    .InColumnValue("project_id", msg.Project != null ? msg.Project.ID : 0)
                    .InColumnValue("title", msg.Title)
                    .InColumnValue("status", msg.Status)
                    .InColumnValue("create_by", msg.CreateBy.ToString())
                    .InColumnValue("create_on", TenantUtil.DateTimeToUtc(msg.CreateOn))
                    .InColumnValue("last_modified_by", msg.LastModifiedBy.ToString())
                    .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(msg.LastModifiedOn))
                    .InColumnValue("content", msg.Content)
                    .Identity(1, 0, true);
                msg.ID = db.ExecuteScalar<int>(insert);
                return msg;
            }
        }

        public virtual void Delete(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(Delete(CommentsTable).Where("target_uniq_id", ProjectEntity.BuildUniqId<Message>(id)));
                    db.ExecuteNonQuery(Delete(MessagesTable).Where("id", id));

                    tx.Commit();
                }
            }
        }


        private SqlQuery CreateQuery()
        {
            return new SqlQuery(MessagesTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .LeftOuterJoin(CommentsTable + " pc", Exp.EqColumns("pc.target_uniq_id", "concat('Message_', cast(t.id as char))") & Exp.EqColumns("t.tenant_id", "pc.tenant_id"))
                .Select(ProjectDao.ProjectColumns.Select(c => "p." + c).ToArray())
                .Select("t.id", "t.title", "t.status", "t.create_by", "t.create_on", "t.last_modified_by", "t.last_modified_on", "t.content")
                .Select("max(coalesce(pc.create_on, t.create_on)) comments")
                .GroupBy("t.id")
                .Where("t.tenant_id", Tenant);
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (filter.Follow)
            {
                var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(NotifyConstants.Event_NewCommentForMessage, NotifySource.Instance.GetRecipientsProvider().GetRecipient(CurrentUserID.ToString())));

                if (filter.ProjectIds.Count != 0)
                {
                    objects = objects.Where(r => r.Split('_')[2] == filter.ProjectIds[0].ToString(CultureInfo.InvariantCulture)).ToList();
                }

                var ids = objects.Select(r => r.Split('_')[1]).ToArray();
                query.Where(Exp.In("t.id", ids));
            }

            if (filter.ProjectIds.Count != 0)
            {
                query.Where(Exp.In("t.project_id", filter.ProjectIds));
            }
            else
            {
                if (filter.MyProjects)
                {
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("ppp.tenant", "t.tenant_id") & Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false));
                    query.Where("ppp.participant_id", CurrentUserID);
                }
            }

            if (filter.TagId != 0)
            {
                query.InnerJoin(ProjectTagTable + " pt", Exp.EqColumns("pt.project_id", "t.project_id"));
                query.Where("pt.tag_id", filter.TagId);
            }

            if (filter.UserId != Guid.Empty)
            {
                query.Where("t.create_by", filter.UserId);
            }

            if (filter.DepartmentId != Guid.Empty)
            {
                query.InnerJoin("core_usergroup cug", Exp.EqColumns("cug.tenant", "t.tenant_id") & Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "t.create_by"));
                query.Where("cug.groupid", filter.DepartmentId);
            }

            if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MaxValue))
            {
                query.Where(Exp.Between("t.create_on", filter.FromDate, filter.ToDate.AddDays(1)));
            }

            if (filter.MessageStatus.HasValue)
            {
                query.Where("t.status", filter.MessageStatus.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query.Where(Exp.Like("t.title", filter.SearchText, SqlLike.AnyWhere));
            }

            if (checkAccess)
            {
                query.Where(Exp.Eq("p.private", false));
            }
            else if (!isAdmin)
            {
                var isInTeam = new SqlQuery(ParticipantTable).Select("security").Where(Exp.EqColumns("p.id", "project_id") & Exp.Eq("removed", false) & Exp.Eq("participant_id", CurrentUserID) & !Exp.Eq("security & " + (int)ProjectTeamSecurity.Messages, (int)ProjectTeamSecurity.Messages));
                query.Where(Exp.Eq("p.private", false) | Exp.Eq("p.responsible_id", CurrentUserID) | (Exp.Eq("p.private", true) & Exp.Exists(isInTeam)));
            }

            return query;
        }

        private static string GetSortFilter(string sortBy)
        {
            if (sortBy != "comments") return "t." + sortBy;

            return "comments";

        }

        private Message ToMessage(object[] r)
        {
            var offset = ProjectDao.ProjectColumns.Length;
            return new Message
                       {
                           Project = r[0] != null ? ProjectDao.ToProject(r) : null,
                           ID = Convert.ToInt32(r[0 + offset]),
                           Title = (string) r[1 + offset],
                           Status = (MessageStatus)Convert.ToInt32(r[2 + offset]),
                           CreateBy = ToGuid(r[3 + offset]),
                           CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[4 + offset])),
                           LastModifiedBy = ToGuid(r[5 + offset]),
                           LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[6 + offset])),
                           Content = (string) r[7 + offset]
                       };
        }


        internal List<Message> GetMessages(Exp where)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where(where)).ConvertAll(ToMessage);
            }
        }
    }
}
