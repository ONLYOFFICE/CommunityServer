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
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    internal class CachedSubtaskDao : SubtaskDao
    {
        private readonly HttpRequestDictionary<Subtask> _subtaskCache = new HttpRequestDictionary<Subtask>("subtask");

        public CachedSubtaskDao(int tenantID) : base(tenantID)
        {
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        public override Subtask GetById(int id)
        {
            return _subtaskCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetBaseById(id));
        }

        private Subtask GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Subtask Save(Subtask subtask)
        {
            if (subtask != null)
            {
                ResetCache(subtask.ID);
            }
            return base.Save(subtask);
        }

        private void ResetCache(int subtaskId)
        {
            _subtaskCache.Reset(subtaskId.ToString(CultureInfo.InvariantCulture));
        }
    }

    class SubtaskDao : BaseDao, ISubtaskDao
    {
        private readonly Converter<object[], Subtask> converter;

        public SubtaskDao(int tenantID) : base(tenantID)
        {
            converter = ToSubTask;
        }

        public List<Subtask> GetSubtasks(int taskid)
        {
            return Db.ExecuteList(CreateQuery().Where("task_id", taskid)).ConvertAll(converter);
        }

        public void GetSubtasksForTasks(ref List<Task> tasks)
        {
            var taskIds = tasks.Select(t => t.ID).ToArray();
            var subtasks = Db.ExecuteList(CreateQuery().Where(Exp.In("task_id", taskIds)))//bug: there may be too large set of tasks
                .ConvertAll(converter);

            tasks = tasks.GroupJoin(subtasks, task => task.ID, subtask => subtask.Task, (task, subtaskCol) =>
                        {
                            task.SubTasks.AddRange(subtaskCol.ToList());
                            return task;
                        }).ToList();
        }

        public List<Subtask> GetSubtasks(Exp where)
        {
            return Db.ExecuteList(CreateQuery().Where(where)).ConvertAll(converter);
        }

        public virtual Subtask GetById(int id)
        {
            return Db.ExecuteList(CreateQuery().Where("id", id)).ConvertAll(converter).SingleOrDefault();
        }

        public List<Subtask> GetById(ICollection<int> ids)
        {
            return Db.ExecuteList(CreateQuery().Where(Exp.In("id", ids.ToArray()))).ConvertAll(converter);
        }

        public List<Subtask> GetUpdates(DateTime from, DateTime to)
        {
            return Db.ExecuteList(CreateQuery().Select("status_changed")
                                                .Where(Exp.Between("create_on", from, to) |
                                                    Exp.Between("last_modified_on", from, to) |
                                                    Exp.Between("status_changed", from, to)))
                            .ConvertAll(x =>
                                            {
                                                var st = ToSubTask(x);
                                                st.StatusChangedOn = Convert.ToDateTime(x.Last());
                                                return st;
                                            }).ToList();
        }

        public List<Subtask> GetByResponsible(Guid id, TaskStatus? status = null)
        {
            var query = CreateQuery().Where("responsible_id", id);

            if (status.HasValue)
            {
              query.Where("status", status.Value);
            }

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public int GetSubtaskCount(int taskid, params TaskStatus[] statuses)
        {
            var query = Query(SubtasksTable)
                .SelectCount()
                .Where("task_id", taskid);
            if (statuses != null && 0 < statuses.Length)
            {
                query.Where(Exp.In("status", statuses));
            }
            return Db.ExecuteScalar<int>(query);
        }

        public virtual Subtask Save(Subtask subtask)
        {
            var insert = Insert(SubtasksTable)
                .InColumnValue("id", subtask.ID)
                .InColumnValue("task_id", subtask.Task)
                .InColumnValue("title", subtask.Title)
                .InColumnValue("responsible_id", subtask.Responsible.ToString())
                .InColumnValue("status", subtask.Status)
                .InColumnValue("create_by", subtask.CreateBy.ToString())
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(subtask.CreateOn))
                .InColumnValue("last_modified_by", subtask.LastModifiedBy.ToString())
                .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(subtask.LastModifiedOn))
                .InColumnValue("status_changed", TenantUtil.DateTimeToUtc(subtask.StatusChangedOn))
                .Identity(1, 0, true);

            subtask.ID = Db.ExecuteScalar<int>(insert);
            return subtask;
        }

        public void CloseAllSubtasks(Task task)
        {
            Db.ExecuteNonQuery(
                Update(SubtasksTable)
                    .Set("status", TaskStatus.Closed)
                    .Set("last_modified_by", CurrentUserID)
                    .Set("last_modified_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                    .Set("status_changed", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                    .Where("status", TaskStatus.Open)
                    .Where("task_id", task.ID));
        }

        public virtual void Delete(int id)
        {
            Db.ExecuteNonQuery(Delete(SubtasksTable).Where("id", id));
        }

        private SqlQuery CreateQuery()
        {
            return new SqlQuery(SubtasksTable)
                .Select("id", "title", "responsible_id", "status", "create_by", "create_on", "last_modified_by", "last_modified_on", "task_id")
                .OrderBy("status", true)
                .OrderBy("(case status when 1 then create_on else status_changed end)", true)
                .Where("tenant_id", Tenant);
        }

        private static Subtask ToSubTask(IList<object> r)
        {
            return new Subtask
            {
                ID = Convert.ToInt32(r[0]),
                Title = (string)r[1],
                Responsible = ToGuid(r[2]),
                Status = (TaskStatus)Convert.ToInt32(r[3]),
                CreateBy = ToGuid(r[4]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                LastModifiedBy = ToGuid(r[6]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[7])),
                Task = Convert.ToInt32(r[8])
            };
        }
    }
}
