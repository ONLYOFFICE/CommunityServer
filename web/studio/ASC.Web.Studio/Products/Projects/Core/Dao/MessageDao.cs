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
using System.Globalization;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Projects;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Data.DAO
{
    internal class CachedMessageDao : MessageDao
    {
        private readonly HttpRequestDictionary<Message> messageCache = new HttpRequestDictionary<Message>("message");

        public CachedMessageDao(int tenant) : base(tenant)
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
        private readonly Converter<object[], Message> converter;

        public MessageDao(int tenant) : base(tenant)
        {
            converter = ToMessage;
        }

        public List<Message> GetAll()
        {
            return Db.ExecuteList(CreateQuery()).ConvertAll(converter);
        }

        public List<Message> GetByProject(int projectId)
        {
            return Db.ExecuteList(CreateQuery().Where("t.project_id", projectId).OrderBy("t.create_on", false))
                .ConvertAll(converter);
        }

        public List<Message> GetMessages(int startIndex, int max)
        {
            var query = CreateQuery()
                .OrderBy("t.create_on", false)
                .SetFirstResult(startIndex)
                .SetMaxResults(max);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Message> GetRecentMessages(int offset, int max, params int[] projects)
        {
            var query = CreateQuery()
                .SetFirstResult(offset)
                .OrderBy("t.create_on", false)
                .SetMaxResults(max);
            if (projects != null && 0 < projects.Length)
            {
                query.Where(Exp.In("t.project_id", projects));
            }
            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Message> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = CreateQuery();

            if (filter.Max > 0 && filter.Max < 150000)
            {
                query.SetFirstResult((int)filter.Offset);
                query.SetMaxResults((int)filter.Max);
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

            CreateQueryFilter(query, filter, isAdmin, checkAccess);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(MessagesTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Select("t.id")
                .GroupBy("t.id")
                .Where("t.tenant_id", Tenant);

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery().SelectCount().From(query, "t1");
            return Db.ExecuteScalar<int>(queryCount);
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(MessagesTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Select("t.create_by", "t.project_id")
                .Where("t.tenant_id", Tenant)
                .Where(Exp.Between("t.create_on", filter.GetFromDate(), filter.GetToDate()));

            if (filter.HasUserId)
            {
                query.Where(Exp.In("t.create_by", filter.GetUserIds()));
                filter.UserId = Guid.Empty;
                filter.DepartmentId = Guid.Empty;
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery()
                .SelectCount()
                .Select("t1.create_by", "t1.project_id")
                .GroupBy("create_by", "project_id")
                .From(query, "t1");

            return Db.ExecuteList(queryCount).ConvertAll(b => new Tuple<Guid, int, int>(Guid.Parse((string)b[1]), Convert.ToInt32(b[2]), Convert.ToInt32(b[0]))); ;
        }

        public virtual Message GetById(int id)
        {
            return Db.ExecuteList(CreateQuery().Where("t.id", id))
                .ConvertAll(converter)
                .SingleOrDefault();
        }

        public bool IsExists(int id)
        {
            var count = Db.ExecuteScalar<long>(Query(MessagesTable).SelectCount().Where("id", id));
            return 0 < count;
        }

        public virtual Message Save(Message msg)
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
                .InColumnValue("content", msg.Description)
                .Identity(1, 0, true);
            msg.ID = Db.ExecuteScalar<int>(insert);
            return msg;
        }

        public virtual void Delete(int id)
        {
            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Delete(CommentsTable).Where("target_uniq_id", ProjectEntity.BuildUniqId<Message>(id)));
                Db.ExecuteNonQuery(Delete(MessagesTable).Where("id", id));

                tx.Commit();
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
                IEnumerable<string> objects = NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(NotifyConstants.Event_NewCommentForMessage, NotifySource.Instance.GetRecipientsProvider().GetRecipient(CurrentUserID.ToString()));

                if (filter.ProjectIds.Count != 0)
                {
                    objects = objects.Where(r => r.Split('_')[2] == filter.ProjectIds[0].ToString(CultureInfo.InvariantCulture));
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
                if (ProjectsCommonSettings.Load().HideEntitiesInPausedProjects)
                {
                    query.Where(!Exp.Eq("p.status", ProjectStatus.Paused));
                }

                if (filter.MyProjects)
                {
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("ppp.tenant", "t.tenant_id") & Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false));
                    query.Where("ppp.participant_id", CurrentUserID);
                }
            }

            if (filter.TagId != 0)
            {
                if (filter.TagId == -1)
                {
                    query.LeftOuterJoin(ProjectTagTable + " pt", Exp.EqColumns("pt.project_id", "t.project_id"));
                    query.Where("pt.tag_id", null);
                }
                else
                {
                    query.InnerJoin(ProjectTagTable + " pt", Exp.EqColumns("pt.project_id", "t.project_id"));
                    query.Where("pt.tag_id", filter.TagId);
                }
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
                List<int> mIds;
                if (FactoryIndexer<DiscussionsWrapper>.TrySelectIds(s => s.MatchAll(filter.SearchText), out mIds))
                {
                    query.Where(Exp.In("t.id", mIds));
                }
                else
                {
                    query.Where(Exp.Like("t.title", filter.SearchText, SqlLike.AnyWhere));
                }
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
                Title = (string)r[1 + offset],
                Status = (MessageStatus)Convert.ToInt32(r[2 + offset]),
                CreateBy = ToGuid(r[3 + offset]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[4 + offset])),
                LastModifiedBy = ToGuid(r[5 + offset]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[6 + offset])),
                Description = (string)r[7 + offset]
            };
        }


        public IEnumerable<Message> GetMessages(Exp where)
        {
            return Db.ExecuteList(CreateQuery().Where(where)).ConvertAll(converter);
        }
    }
}
