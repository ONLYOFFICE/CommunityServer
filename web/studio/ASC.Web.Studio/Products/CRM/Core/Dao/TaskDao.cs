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
using System.Text.RegularExpressions;

using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;

using ASC.ElasticSearch;
using ASC.Web.CRM.Core.Search;

namespace ASC.CRM.Core.Dao
{
    public class CachedTaskDao : TaskDao
    {

        private readonly HttpRequestDictionary<Task> _contactCache = new HttpRequestDictionary<Task>("crm_task");

        public CachedTaskDao(int tenantID)
            : base(tenantID)
        {

        }

        public override Task GetByID(int taskID)
        {
            return _contactCache.Get(taskID.ToString(), () => GetByIDBase(taskID));

        }

        private Task GetByIDBase(int taskID)
        {
            return base.GetByID(taskID);
        }

        public override void DeleteTask(int taskID)
        {
            ResetCache(taskID);

            base.DeleteTask(taskID);
        }

        public override Task SaveOrUpdateTask(Task task)
        {

            if (task != null && task.ID > 0)
            {
                ResetCache(task.ID);
            }

            return base.SaveOrUpdateTask(task);
        }

        private void ResetCache(int taskID)
        {
            _contactCache.Reset(taskID.ToString());
        }

    }

    public class TaskDao : AbstractDao
    {
        #region Constructor

        public TaskDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion

        #region Methods

        public void OpenTask(int taskID)
        {
            var task = GetByID(taskID);

            if (task == null)
                throw new ArgumentException();

            CRMSecurity.DemandEdit(task);

            Db.ExecuteNonQuery(
                Update("crm_task")
                .Set("is_closed", false)
                .Where(Exp.Eq("id", taskID))
                );
        }

        public void CloseTask(int taskID)
        {
            var task = GetByID(taskID);

            if (task == null)
                throw new ArgumentException();

            CRMSecurity.DemandEdit(task);

            Db.ExecuteNonQuery(
                    Update("crm_task")
                    .Set("is_closed", true)
                    .Where(Exp.Eq("id", taskID))
                );
        }

        public virtual Task GetByID(int taskID)
        {
            var tasks = Db.ExecuteList(GetTaskQuery(Exp.Eq("id", taskID)))
                .ConvertAll(row => ToTask(row));

            return tasks.Count > 0 ? tasks[0] : null;
        }

        public List<Task> GetTasks(EntityType entityType, int entityID, bool? onlyActiveTask)
        {
            return GetTasks(String.Empty, Guid.Empty, 0, onlyActiveTask, DateTime.MinValue, DateTime.MinValue,
                            entityType, entityID, 0, 0, null);
        }
        public int GetAllTasksCount()
        {
            return Db.ExecuteScalar<int>(Query("crm_task").SelectCount());
        }

        public List<Task> GetAllTasks()
        {
            return Db.ExecuteList(
                GetTaskQuery(null)
                .OrderBy("deadline", true)
                .OrderBy("title", true))
                .ConvertAll(row => ToTask(row)).FindAll(CRMSecurity.CanAccessTo);
        }

        public void ExecAlert(IEnumerable<int> ids)
        {
            if (!ids.Any()) return;

                Db.ExecuteNonQuery(new SqlUpdate("crm_task").Set("exec_alert", true).Where(Exp.In("id", ids.ToArray())));
        }

        public List<object[]> GetInfoForReminder(DateTime scheduleDate)
        {
           var sqlQuery = new SqlQuery("crm_task")
            .Select(
             "tenant_id",
             "id", 
                    "deadline",
                    "alert_value",
                    "responsible_id"
                   )
                .Where(
                    Exp.Eq("is_closed", false) &
                    !Exp.Eq("alert_value", 0) &
                    Exp.Eq("exec_alert", false) &
                    Exp.Between("DATE_ADD(deadline, interval -alert_value minute)", scheduleDate.AddHours(-1), scheduleDate.AddHours(1))
                );

            return Db.ExecuteList(sqlQuery);
        }

        public List<Task> GetTasks(
                                  String searchText,
                                  Guid responsibleID,
                                  int categoryID,
                                  bool? isClosed,
                                  DateTime fromDate,
                                  DateTime toDate,
                                  EntityType entityType,
                                  int entityID,
                                  int from,
                                  int count,
                                  OrderBy orderBy)
        {

            if (CRMSecurity.IsAdmin)
                return GetCrudeTasks(
                    searchText,
                    responsibleID,
                    categoryID,
                    isClosed,
                    fromDate,
                    toDate,
                    entityType,
                    entityID,
                    from,
                    count,
                    orderBy);


            var crudeTasks = GetCrudeTasks(
                    searchText,
                    responsibleID,
                    categoryID,
                    isClosed,
                    fromDate,
                    toDate,
                    entityType,
                    entityID,
                    0,
                    from + count,
                    orderBy);

            if (crudeTasks.Count == 0) return crudeTasks;

            if (crudeTasks.Count < from + count) return CRMSecurity.FilterRead(crudeTasks).Skip(from).ToList();

            var result = CRMSecurity.FilterRead(crudeTasks).ToList();

            if (result.Count == crudeTasks.Count) return result.Skip(from).ToList();

            var localCount = count;
            var localFrom = from + count;

            while (true)
            {
                crudeTasks = GetCrudeTasks(
                    searchText,
                    responsibleID,
                    categoryID,
                    isClosed,
                    fromDate,
                    toDate,
                    entityType,
                    entityID,
                    localFrom,
                    localCount,
                    orderBy);

                if (crudeTasks.Count == 0) break;

                result.AddRange(CRMSecurity.FilterRead(crudeTasks));

                if ((result.Count >= count + from) || (crudeTasks.Count < localCount)) break;

                localFrom += localCount;
                localCount = localCount * 2;
            }

            return result.Skip(from).Take(count).ToList();
        }


        private List<Task> GetCrudeTasks(
                                  String searchText,
                                  Guid responsibleID,
                                  int categoryID,
                                  bool? isClosed,
                                  DateTime fromDate,
                                  DateTime toDate,
                                  EntityType entityType,
                                  int entityID,
                                  int from,
                                  int count,
                                  OrderBy orderBy)
        {
            var taskTableAlias = "t";
            var sqlQuery = WhereConditional(GetTaskQuery(null, taskTableAlias), taskTableAlias, responsibleID,
                                        categoryID, isClosed, fromDate, toDate, entityType, entityID, from, count,
                                        orderBy);

            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();


                var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   .ToArray();

                if (keywords.Length > 0)
                {
                    List<int> tasksIds;
                    if (!FactoryIndexer<TasksWrapper>.TrySelectIds(s => s.MatchAll(searchText), out tasksIds))
                    {
                        sqlQuery.Where(BuildLike(new[] {taskTableAlias + ".title", taskTableAlias + ".description"}, keywords));
                    }
                    else
                    {
                        if (tasksIds.Any())
                            sqlQuery.Where(Exp.In(taskTableAlias + ".id", tasksIds));
                        else
                            return new List<Task>();
                    }

                }
            }

            return Db.ExecuteList(sqlQuery)
                .ConvertAll(row => ToTask(row));
        }

        public int GetTasksCount(
                                       String searchText,
                                       Guid responsibleId,
                                       int categoryId,
                                       bool? isClosed,
                                       DateTime fromDate,
                                       DateTime toDate,
                                       EntityType entityType,
                                       int entityId)
        {

            int result = 0;

            LogManager.GetLogger("ASC.CRM").DebugFormat("Starting GetTasksCount: {0}", DateTime.Now.ToString());

            var cacheKey = TenantID.ToString(CultureInfo.InvariantCulture) +
                           "tasks" +
                           SecurityContext.CurrentAccount.ID.ToString() +
                           searchText +
                           responsibleId +
                           categoryId +
                           fromDate.ToString(CultureInfo.InvariantCulture) +
                           toDate.ToString(CultureInfo.InvariantCulture) +
                           (int)entityType +
                           entityId;

            if (!String.IsNullOrEmpty(_cache.Get<String>(cacheKey)))
            {
                LogManager.GetLogger("ASC.CRM").DebugFormat("End GetTasksCount: {0}. From cache", DateTime.Now.ToString());

                return Convert.ToInt32(_cache.Get<String>(cacheKey));
            }


            if (!String.IsNullOrEmpty(searchText))
            {
                var tasks = GetCrudeTasks(searchText,
                                      responsibleId,
                                      categoryId,
                                      isClosed,
                                      fromDate,
                                      toDate,
                                      entityType,
                                      entityId,
                                      0,
                                      0,
                                      null);

                if (CRMSecurity.IsAdmin)
                    result = tasks.Count();
                else
                    result = CRMSecurity.FilterRead(tasks).Count();
            }
            else
            {
                if (CRMSecurity.IsAdmin)
                {

                    var sqlQuery = Query("crm_task tbl_tsk").SelectCount();

                    sqlQuery = WhereConditional(sqlQuery, "tbl_tsk",
                                              responsibleId,
                                              categoryId,
                                              isClosed,
                                              fromDate,
                                              toDate,
                                              entityType,
                                              entityId,
                                              0,
                                              0,
                                              null);

                    result = Db.ExecuteScalar<int>(sqlQuery);
                }
                else
                {
                    var taskIds = new List<int>();

                    var sqlQuery = Query("crm_task tbl_tsk")
                                  .Select("tbl_tsk.id")
                                  .LeftOuterJoin("crm_contact tbl_ctc", Exp.EqColumns("tbl_tsk.contact_id", "tbl_ctc.id"))
                                  .Where(Exp.Or(Exp.Eq("tbl_ctc.is_shared", Exp.Empty), Exp.Gt("tbl_ctc.is_shared", 0)));

                    sqlQuery = WhereConditional(sqlQuery, "tbl_tsk",
                                                responsibleId,
                                                categoryId,
                                                isClosed,
                                                fromDate,
                                                toDate,
                                                entityType,
                                                entityId,
                                                0,
                                                0,
                                                null);

                    // count tasks without entityId and only open contacts

                    taskIds = Db.ExecuteList(sqlQuery).Select(item => Convert.ToInt32(item[0])).ToList();

                    LogManager.GetLogger("ASC.CRM").DebugFormat("End GetTasksCount: {0}. count tasks without entityId and only open contacts", DateTime.Now.ToString());


                    sqlQuery = Query("crm_task tbl_tsk")
                                .Select("tbl_tsk.id")
                                .InnerJoin("crm_contact tbl_ctc", Exp.EqColumns("tbl_tsk.contact_id", "tbl_ctc.id"))
                                .InnerJoin("core_acl tbl_cl", Exp.EqColumns("tbl_ctc.tenant_id", "tbl_cl.tenant") &
                                Exp.Eq("tbl_cl.subject", ASC.Core.SecurityContext.CurrentAccount.ID.ToString()) &
                                Exp.Eq("tbl_cl.action", CRMSecurity._actionRead.ID.ToString()) &
                                Exp.EqColumns("tbl_cl.object", "CONCAT('ASC.CRM.Core.Entities.Company|', tbl_ctc.id)"))
                                .Where(Exp.Eq("tbl_ctc.is_shared", 0))
                                .Where(Exp.Eq("tbl_ctc.is_company", 1));

                    sqlQuery = WhereConditional(sqlQuery, "tbl_tsk",
                                                responsibleId,
                                                categoryId,
                                                isClosed,
                                                fromDate,
                                                toDate,
                                                entityType,
                                                entityId,
                                                0,
                                                0,
                                                null);

                    // count tasks with entityId and only close contacts
                    taskIds.AddRange(Db.ExecuteList(sqlQuery).Select(item => Convert.ToInt32(item[0])).ToList());

                    LogManager.GetLogger("ASC.CRM").DebugFormat("End GetTasksCount: {0}. count tasks with entityId and only close contacts", DateTime.Now.ToString());

                    sqlQuery = Query("crm_task tbl_tsk")
                              .Select("tbl_tsk.id")
                              .InnerJoin("crm_contact tbl_ctc", Exp.EqColumns("tbl_tsk.contact_id", "tbl_ctc.id"))
                              .InnerJoin("core_acl tbl_cl", Exp.EqColumns("tbl_ctc.tenant_id", "tbl_cl.tenant") &
                              Exp.Eq("tbl_cl.subject", ASC.Core.SecurityContext.CurrentAccount.ID.ToString()) &
                              Exp.Eq("tbl_cl.action", CRMSecurity._actionRead.ID.ToString()) &
                              Exp.EqColumns("tbl_cl.object", "CONCAT('ASC.CRM.Core.Entities.Person|', tbl_ctc.id)"))
                              .Where(Exp.Eq("tbl_ctc.is_shared", 0))
                              .Where(Exp.Eq("tbl_ctc.is_company", 0));

                    sqlQuery = WhereConditional(sqlQuery, "tbl_tsk",
                                                responsibleId,
                                                categoryId,
                                                isClosed,
                                                fromDate,
                                                toDate,
                                                entityType,
                                                entityId,
                                                0,
                                                0,
                                                null);

                    // count tasks with entityId and only close contacts
                    taskIds.AddRange(Db.ExecuteList(sqlQuery).Select(item => Convert.ToInt32(item[0])).ToList());

                    LogManager.GetLogger("ASC.CRM").DebugFormat("End GetTasksCount: {0}. count tasks with entityId and only close contacts", DateTime.Now.ToString());


                    sqlQuery = Query("crm_task tbl_tsk")
                                .Select("tbl_tsk.id")
                                .InnerJoin("core_acl tbl_cl", Exp.EqColumns("tbl_tsk.tenant_id", "tbl_cl.tenant") &
                                Exp.Eq("tbl_cl.subject", ASC.Core.SecurityContext.CurrentAccount.ID.ToString()) &
                                Exp.Eq("tbl_cl.action", CRMSecurity._actionRead.ID.ToString()) &
                                Exp.EqColumns("tbl_cl.object", "CONCAT('ASC.CRM.Core.Entities.Deal|', tbl_tsk.entity_id)"))
                                .Where(!Exp.Eq("tbl_tsk.entity_id", 0) & Exp.Eq("tbl_tsk.entity_type", (int)EntityType.Opportunity) & Exp.Eq("tbl_tsk.contact_id", 0));

                    sqlQuery = WhereConditional(sqlQuery, "tbl_tsk",
                                                responsibleId,
                                                categoryId,
                                                isClosed,
                                                fromDate,
                                                toDate,
                                                entityType,
                                                entityId,
                                                0,
                                                0,
                                                null);

                    // count tasks with entityId and without contact
                    taskIds.AddRange(Db.ExecuteList(sqlQuery).Select(item => Convert.ToInt32(item[0])).ToList());

                    LogManager.GetLogger("ASC.CRM").DebugFormat("End GetTasksCount: {0}. count tasks with entityId and without contact", DateTime.Now.ToString());

                    sqlQuery = Query("crm_task tbl_tsk")
                                .Select("tbl_tsk.id")
                                .InnerJoin("core_acl tbl_cl", Exp.EqColumns("tbl_tsk.tenant_id", "tbl_cl.tenant") &
                                Exp.Eq("tbl_cl.subject", ASC.Core.SecurityContext.CurrentAccount.ID.ToString()) &
                                Exp.Eq("tbl_cl.action", CRMSecurity._actionRead.ID.ToString()) &
                                Exp.EqColumns("tbl_cl.object", "CONCAT('ASC.CRM.Core.Entities.Cases|', tbl_tsk.entity_id)"))
                                .Where(!Exp.Eq("tbl_tsk.entity_id", 0) & Exp.Eq("tbl_tsk.entity_type", (int)EntityType.Case) & Exp.Eq("tbl_tsk.contact_id", 0));

                    sqlQuery = WhereConditional(sqlQuery, "tbl_tsk",
                                                responsibleId,
                                                categoryId,
                                                isClosed,
                                                fromDate,
                                                toDate,
                                                entityType,
                                                entityId,
                                                0,
                                                0,
                                                null);

                    // count tasks with entityId and without contact
                    taskIds.AddRange(Db.ExecuteList(sqlQuery).Select(item => Convert.ToInt32(item[0])).ToList());

                    result = taskIds.Distinct().Count();

                    LogManager.GetLogger("ASC.CRM").DebugFormat("End GetTasksCount: {0}. count tasks with entityId and without contact", DateTime.Now.ToString());

                    LogManager.GetLogger("ASC.CRM").Debug("Finish");

                }
            }


            if (result > 0)
            {
                _cache.Insert(cacheKey, result, TimeSpan.FromMinutes(1));
            }

            return result;
        }


        private SqlQuery WhereConditional(
                                  SqlQuery sqlQuery,
                                  String alias,
                                  Guid responsibleID,
                                  int categoryID,
                                  bool? isClosed,
                                  DateTime fromDate,
                                  DateTime toDate,
                                  EntityType entityType,
                                  int entityID,
                                  int from,
                                  int count,
                                  OrderBy orderBy)
        {
            var aliasPrefix = !String.IsNullOrEmpty(alias) ? alias + "." : "";

            if (responsibleID != Guid.Empty)
                sqlQuery.Where(Exp.Eq("responsible_id", responsibleID));

            if (entityID > 0)
                switch (entityType)
                {
                    case EntityType.Contact:
                        var isCompany = true;
                        isCompany = Db.ExecuteScalar<bool>(Query("crm_contact").Select("is_company").Where(Exp.Eq("id", entityID)));

                        if (isCompany)
                            return WhereConditional(sqlQuery, alias, responsibleID, categoryID, isClosed, fromDate, toDate, EntityType.Company, entityID, from, count, orderBy);
                        else
                            return WhereConditional(sqlQuery, alias, responsibleID, categoryID, isClosed, fromDate, toDate, EntityType.Person, entityID, from, count, orderBy);

                    case EntityType.Person:
                        sqlQuery.Where(Exp.Eq(aliasPrefix + "contact_id", entityID));
                        break;
                    case EntityType.Company:

                        var personIDs = GetRelativeToEntity(entityID, EntityType.Person, null).ToList();

                        if (personIDs.Count == 0)
                            sqlQuery.Where(Exp.Eq(aliasPrefix + "contact_id", entityID));
                        else
                        {
                            personIDs.Add(entityID);
                            sqlQuery.Where(Exp.In(aliasPrefix + "contact_id", personIDs));
                        }

                        break;
                    case EntityType.Case:
                    case EntityType.Opportunity:
                        sqlQuery.Where(Exp.Eq(aliasPrefix + "entity_id", entityID) &
                                       Exp.Eq(aliasPrefix + "entity_type", (int)entityType));
                        break;
                }



            if (isClosed.HasValue)
                sqlQuery.Where(aliasPrefix + "is_closed", isClosed);

            if (categoryID > 0)
                sqlQuery.Where(Exp.Eq(aliasPrefix + "category_id", categoryID));

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Between(aliasPrefix + "deadline", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));
            else if (fromDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Ge(aliasPrefix + "deadline", TenantUtil.DateTimeToUtc(fromDate)));
            else if (toDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Le(aliasPrefix + "deadline", TenantUtil.DateTimeToUtc(toDate)));

            if (0 < from && from < int.MaxValue)
                sqlQuery.SetFirstResult(from);

            if (0 < count && count < int.MaxValue)
                sqlQuery.SetMaxResults(count);

            sqlQuery.OrderBy(aliasPrefix + "is_closed", true);

            if (orderBy != null && Enum.IsDefined(typeof(TaskSortedByType), orderBy.SortedBy))
            {
                switch ((TaskSortedByType)orderBy.SortedBy)
                {
                    case TaskSortedByType.Title:
                        sqlQuery
                            .OrderBy(aliasPrefix + "title", orderBy.IsAsc)
                            .OrderBy(aliasPrefix + "deadline", true);
                        break;
                    case TaskSortedByType.DeadLine:
                        sqlQuery.OrderBy(aliasPrefix + "deadline", orderBy.IsAsc)
                                .OrderBy(aliasPrefix + "title", true);
                        break;
                    case TaskSortedByType.Category:
                        sqlQuery.OrderBy(aliasPrefix + "category_id", orderBy.IsAsc)
                                .OrderBy(aliasPrefix + "deadline", true)
                                .OrderBy(aliasPrefix + "title", true);
                        break;
                    case TaskSortedByType.ContactManager:
                        sqlQuery.LeftOuterJoin("core_user u", Exp.EqColumns(aliasPrefix + "responsible_id", "u.id"))
                                .OrderBy("case when u.lastname is null or u.lastname = '' then 1 else 0 end, u.lastname", orderBy.IsAsc)
                                .OrderBy("case when u.firstname is null or u.firstname = '' then 1 else 0 end, u.firstname", orderBy.IsAsc)
                                .OrderBy(aliasPrefix + "deadline", true)
                                .OrderBy(aliasPrefix + "title", true);
                        break;
                    case TaskSortedByType.Contact:
                        sqlQuery.LeftOuterJoin("crm_contact c_tbl", Exp.EqColumns(aliasPrefix + "contact_id", "c_tbl.id"))
                                .OrderBy("case when c_tbl.display_name is null then 1 else 0 end, c_tbl.display_name", orderBy.IsAsc)
                                .OrderBy(aliasPrefix + "deadline", true)
                                .OrderBy(aliasPrefix + "title", true);
                        break;
                }
            }
            else
            {
                sqlQuery
                    .OrderBy(aliasPrefix + "deadline", true)
                    .OrderBy(aliasPrefix + "title", true);
            }

            return sqlQuery;

        }

        public Dictionary<int, Task> GetNearestTask(int[] contactID)
        {
            var sqlSubQuery =
                        Query("crm_task")
                        .SelectMin("id")
                        .SelectMin("deadline")
                        .Select("contact_id")
                        .Where(Exp.In("contact_id", contactID) & Exp.Eq("is_closed", false))
                        .GroupBy("contact_id");

            var taskIDs = Db.ExecuteList(sqlSubQuery).ConvertAll(row => row[0]);

            if (taskIDs.Count == 0) return new Dictionary<int, Task>();

            var tasks = Db.ExecuteList(GetTaskQuery(Exp.In("id", taskIDs))).ConvertAll(row=>ToTask(row)).Where(CRMSecurity.CanAccessTo);

            var result = new Dictionary<int, Task>();

            foreach (var task in tasks.Where(task => !result.ContainsKey(task.ContactID)))
            {
                result.Add(task.ContactID, task);
            }

            return result;
        }

        public IEnumerable<Guid> GetResponsibles(int categoryID)
        {
            var q = Query("crm_task")
                .Select("responsible_id")
                .GroupBy(1);

            if (0 < categoryID) q.Where("category_id", categoryID);

            return Db.ExecuteList(q)
                .ConvertAll(r => (string)r[0])
                .Select(r => new Guid(r))
                .Where(g => g != Guid.Empty)
                .ToList();
        }


        public Dictionary<int, int> GetTasksCount(int[] contactID)
        {
            var sqlQuery = Query("crm_task")
                          .Select("contact_id")
                          .SelectCount()
                          .Where(Exp.In("contact_id", contactID))
                          .GroupBy("contact_id");

            var sqlResult = Db.ExecuteList(sqlQuery);

            return sqlResult.ToDictionary(item => Convert.ToInt32(item[0]), item => Convert.ToInt32(item[1]));
        }

        public int GetTasksCount(int contactID)
        {
            var result = GetTasksCount(new[] { contactID });

            if (result.Count == 0) return 0;

            return result[contactID];
        }

        public Dictionary<int, bool> HaveLateTask(int[] contactID)
        {
            var sqlQuery = Query("crm_task")
                          .Select("contact_id")
                          .Where(Exp.In("contact_id", contactID))
                          .Where(Exp.Eq("is_closed", false) &
                           Exp.Lt("deadline", DateTime.UtcNow))
                          .SelectCount()
                          .GroupBy("contact_id");

            var sqlResult = Db.ExecuteList(sqlQuery);

            return sqlResult.ToDictionary(item => Convert.ToInt32(item[0]), item => Convert.ToInt32(item[1]) > 0);
        }


        public bool HaveLateTask(int contactID)
        {
            var result = HaveLateTask(new[] { contactID });

            if (result.Count == 0) return false;

            return result[contactID];
        }

        public virtual Task SaveOrUpdateTask(Task newTask)
        {
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "tasks.*"));
            return SaveOrUpdateTaskInDb(newTask);
        }

        public virtual Task[] SaveOrUpdateTaskList(List<Task> newTasks)
        {
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "tasks.*"));
            var result = new List<Task>();
            foreach (var newTask in newTasks)
            {
                result.Add(SaveOrUpdateTaskInDb(newTask));
            }
            return result.ToArray();
        }

        private Task SaveOrUpdateTaskInDb(Task newTask)
        {
            if (String.IsNullOrEmpty(newTask.Title) || newTask.DeadLine == DateTime.MinValue ||
                newTask.CategoryID <= 0)
                throw new ArgumentException();

            if (newTask.ID == 0 || Db.ExecuteScalar<int>(Query("crm_task").SelectCount().Where(Exp.Eq("id", newTask.ID))) == 0)
            {
                newTask.CreateOn = DateTime.UtcNow;
                newTask.CreateBy = ASC.Core.SecurityContext.CurrentAccount.ID;

                newTask.LastModifedOn = DateTime.UtcNow;
                newTask.LastModifedBy = ASC.Core.SecurityContext.CurrentAccount.ID;

                newTask.ID = Db.ExecuteScalar<int>(
                               Insert("crm_task")
                              .InColumnValue("id", 0)
                              .InColumnValue("title", newTask.Title)
                              .InColumnValue("description", newTask.Description)
                              .InColumnValue("deadline", TenantUtil.DateTimeToUtc(newTask.DeadLine))
                              .InColumnValue("responsible_id", newTask.ResponsibleID)
                              .InColumnValue("contact_id", newTask.ContactID)
                              .InColumnValue("entity_type", (int)newTask.EntityType)
                              .InColumnValue("entity_id", newTask.EntityID)
                              .InColumnValue("is_closed", newTask.IsClosed)
                              .InColumnValue("category_id", newTask.CategoryID)
                              .InColumnValue("create_on", newTask.CreateOn)
                              .InColumnValue("create_by", newTask.CreateBy)
                              .InColumnValue("last_modifed_on", newTask.LastModifedOn)
                              .InColumnValue("last_modifed_by", newTask.LastModifedBy)
                              .InColumnValue("alert_value", newTask.AlertValue)
                              .Identity(1, 0, true));
            }
            else
            {
                var oldTask = Db.ExecuteList(GetTaskQuery(Exp.Eq("id", newTask.ID)))
                    .ConvertAll(row => ToTask(row))
                    .FirstOrDefault();

                CRMSecurity.DemandEdit(oldTask);

                newTask.CreateOn = oldTask.CreateOn;
                newTask.CreateBy = oldTask.CreateBy;

                newTask.LastModifedOn = DateTime.UtcNow;
                newTask.LastModifedBy = ASC.Core.SecurityContext.CurrentAccount.ID;

                newTask.IsClosed = oldTask.IsClosed;

                Db.ExecuteNonQuery(
                                Update("crm_task")
                                .Set("title", newTask.Title)
                                .Set("description", newTask.Description)
                                .Set("deadline", TenantUtil.DateTimeToUtc(newTask.DeadLine))
                                .Set("responsible_id", newTask.ResponsibleID)
                                .Set("contact_id", newTask.ContactID)
                                .Set("entity_type", (int)newTask.EntityType)
                                .Set("entity_id", newTask.EntityID)
                                .Set("category_id", newTask.CategoryID)
                                .Set("last_modifed_on", newTask.LastModifedOn)
                                .Set("last_modifed_by", newTask.LastModifedBy)
                                .Set("alert_value", (int)newTask.AlertValue)
                                .Set("exec_alert", 0)
                                .Where(Exp.Eq("id", newTask.ID)));
            }

            FactoryIndexer<TasksWrapper>.IndexAsync(newTask);
            return newTask;
        }

        public virtual int SaveTask(Task newTask)
        {
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "tasks.*"));
            return SaveTaskInDb(newTask);
        }

        private int SaveTaskInDb(Task newTask)
        {
            if (String.IsNullOrEmpty(newTask.Title) || newTask.DeadLine == DateTime.MinValue ||
                newTask.CategoryID == 0)
                throw new ArgumentException();

             var result = Db.ExecuteScalar<int>(
                               Insert("crm_task")
                              .InColumnValue("id", 0)
                              .InColumnValue("title", newTask.Title)
                              .InColumnValue("description", newTask.Description)
                              .InColumnValue("deadline", TenantUtil.DateTimeToUtc(newTask.DeadLine))
                              .InColumnValue("responsible_id", newTask.ResponsibleID)
                              .InColumnValue("contact_id", newTask.ContactID)
                              .InColumnValue("entity_type", (int)newTask.EntityType)
                              .InColumnValue("entity_id", newTask.EntityID)
                              .InColumnValue("is_closed", newTask.IsClosed)
                              .InColumnValue("category_id", newTask.CategoryID)
                              .InColumnValue("create_on", newTask.CreateOn == DateTime.MinValue ? DateTime.UtcNow : newTask.CreateOn)
                              .InColumnValue("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                              .InColumnValue("last_modifed_on", newTask.CreateOn == DateTime.MinValue ? DateTime.UtcNow : newTask.CreateOn)
                              .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                              .InColumnValue("alert_value", (int)newTask.AlertValue)
                              .Identity(1, 0, true));

            newTask.ID = result;
            FactoryIndexer<TasksWrapper>.IndexAsync(newTask);

            return result;
        }

        public virtual int[] SaveTaskList(List<Task> items)
        {
            using (var tx = Db.BeginTransaction())
            {
                var result = new List<int>();

                foreach (var item in items)
                {
                    result.Add(SaveTaskInDb(item));
                }

                tx.Commit();

                return result.ToArray();
            }
        }


        public virtual void DeleteTask(int taskID)
        {
            var task = GetByID(taskID);

            if (task == null) return;

            CRMSecurity.DemandEdit(task);

            Db.ExecuteNonQuery(Delete("crm_task").Where("id", taskID));

            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "tasks.*"));
            FactoryIndexer<TasksWrapper>.DeleteAsync(task);
        }

        public List<Task> CreateByTemplate(List<TaskTemplate> templateItems, EntityType entityType, int entityID)
        {
            if (templateItems == null || templateItems.Count == 0) return new List<Task>();

            var result = new List<Task>();

            using (var tx = Db.BeginTransaction())
            {
                foreach (var templateItem in templateItems)
                {
                    var task = new Task
                    {
                        ResponsibleID = templateItem.ResponsibleID,
                        Description = templateItem.Description,
                        DeadLine = TenantUtil.DateTimeNow().AddTicks(templateItem.Offset.Ticks),
                        CategoryID = templateItem.CategoryID,
                        Title = templateItem.Title,
                        CreateOn = TenantUtil.DateTimeNow(),
                        CreateBy = templateItem.CreateBy
                    };

                    switch (entityType)
                    {
                        case EntityType.Contact:
                        case EntityType.Person:
                        case EntityType.Company:
                            task.ContactID = entityID;
                            break;
                        case EntityType.Opportunity:
                            task.EntityType = EntityType.Opportunity;
                            task.EntityID = entityID;
                            break;
                        case EntityType.Case:
                            task.EntityType = EntityType.Case;
                            task.EntityID = entityID;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    task = SaveOrUpdateTask(task);

                    result.Add(task);

                    Db.ExecuteNonQuery(Insert("crm_task_template_task")
                                     .InColumnValue("task_id", task.ID)
                                     .InColumnValue("task_template_id", templateItem.ID));

                }

                tx.Commit();
            }

            return result;
        }

        #region Private Methods

        private static Task ToTask(object[] row)
        {
            return new Task
                {
                    ID = Convert.ToInt32(row[0]),
                    ContactID = Convert.ToInt32(row[1]),
                    Title = Convert.ToString(row[2]),
                    Description = Convert.ToString(row[3]),
                    DeadLine = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[4])),
                    ResponsibleID = ToGuid(row[5]),
                    IsClosed = Convert.ToBoolean(row[6]),
                    CategoryID = Convert.ToInt32(row[7]),
                    EntityID = Convert.ToInt32(row[8]),
                    EntityType = (EntityType)Convert.ToInt32(row[9]),
                    CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[10])),
                    CreateBy = ToGuid(row[11]),
                    AlertValue = Convert.ToInt32(row[12])
                };
        }

        
        private String[] GetTaskColumnsTable(String alias)
        {
            if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";

            var result = new List<String>
                             {
                                "id",
                                "contact_id",
                                "title",
                                "description",
                                "deadline",
                                "responsible_id",
                                "is_closed",
                                "category_id",
                                "entity_id",
                                "entity_type",
                                "create_on",
                                "create_by",
                                "alert_value"
                             };

            if (String.IsNullOrEmpty(alias)) return result.ToArray();

            return result.ConvertAll(item => String.Concat(alias, item)).ToArray();
        }

        private SqlQuery GetTaskQuery(Exp where, String alias)
        {

            var sqlQuery = Query("crm_task");

            if (!String.IsNullOrEmpty(alias))
            {
                sqlQuery = new SqlQuery(String.Concat("crm_task ", alias))
                           .Where(Exp.Eq(alias + ".tenant_id", TenantID));
                sqlQuery.Select(GetTaskColumnsTable(alias));

            }
            else
                sqlQuery.Select(GetTaskColumnsTable(String.Empty));


            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;

        }

        private SqlQuery GetTaskQuery(Exp where)
        {
            return GetTaskQuery(where, String.Empty);

        }


        #endregion


        public void ReassignTasksResponsible(Guid fromUserId, Guid toUserId)
        {
            var tasks = GetTasks(String.Empty, fromUserId, 0, false, DateTime.MinValue, DateTime.MinValue,
                            EntityType.Any, 0, 0, 0, null);

            foreach (var task in tasks)
            {
                task.ResponsibleID = toUserId;

                SaveOrUpdateTask(task);
            }
        }

        #endregion


        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="creationDate"></param>
        public void SetTaskCreationDate(int taskId, DateTime creationDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_task")
                    .Set("create_on", TenantUtil.DateTimeToUtc(creationDate))
                    .Where(Exp.Eq("id", taskId)));
            // Delete relative keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "tasks.*"));
        }

        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="lastModifedDate"></param>
        public void SetTaskLastModifedDate(int taskId, DateTime lastModifedDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_task")
                    .Set("last_modifed_on", TenantUtil.DateTimeToUtc(lastModifedDate))
                    .Where(Exp.Eq("id", taskId)));
            // Delete relative keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "tasks.*"));
        }
    }
}
