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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Engine
{
    public class SubtaskEngine : ProjectEntityEngine
    {
        public IDaoFactory DaoFactory { get; set; }

        public SubtaskEngine(bool disableNotifications): base(NotifyConstants.Event_NewCommentForTask, disableNotifications)
        {
        }

        #region get 

        public List<Task> GetByDate(DateTime from, DateTime to)
        {
            var subtasks = DaoFactory.SubtaskDao.GetUpdates(from, to).ToDictionary(x => x.Task, x => x);
            var ids = subtasks.Select(x => x.Value.Task).Distinct().ToList();
            var tasks = DaoFactory.TaskDao.GetById(ids);
            foreach (var task in tasks)
            {
                Subtask subtask;
                subtasks.TryGetValue(task.ID, out subtask);
                task.SubTasks.Add(subtask);
            }
            return tasks;
        }

        public List<Task> GetByResponsible(Guid id, TaskStatus? status = null)
        {
            var subtasks = DaoFactory.SubtaskDao.GetByResponsible(id, status);
            var ids = subtasks.Select(x => x.Task).Distinct().ToList();
            var tasks = DaoFactory.TaskDao.GetById(ids);
            foreach (var task in tasks)
            {
                task.SubTasks.AddRange(subtasks.FindAll(r=> r.Task == task.ID));
            }
            return tasks;
        }

        public int GetSubtaskCount(int taskid, params TaskStatus[] statuses)
        {
            return DaoFactory.SubtaskDao.GetSubtaskCount(taskid, statuses);
        }

        public int GetSubtaskCount(int taskid)
        {
            return DaoFactory.SubtaskDao.GetSubtaskCount(taskid, null);
        }

        public Subtask GetById(int id)
        {
            return DaoFactory.SubtaskDao.GetById(id);
        }

        #endregion

        #region Actions 

        public Subtask ChangeStatus(Task task, Subtask subtask, TaskStatus newStatus)
        {
            if (subtask == null) throw new Exception("subtask.Task");
            if (task == null) throw new ArgumentNullException("task");
            if (task.Status == TaskStatus.Closed) throw new Exception("task can't be closed");

            if (subtask.Status == newStatus) return subtask;

            ProjectSecurity.DemandEdit(task, subtask);
           
            subtask.Status = newStatus;
            subtask.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            subtask.LastModifiedOn = TenantUtil.DateTimeNow();
            subtask.StatusChangedOn = TenantUtil.DateTimeNow();

            if (subtask.Responsible.Equals(Guid.Empty))
                subtask.Responsible = SecurityContext.CurrentAccount.ID;

            var senders = GetSubscribers(task);

            if (task.Status != TaskStatus.Closed && newStatus == TaskStatus.Closed && !DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutSubTaskClosing(senders, task, subtask);

            if (task.Status != TaskStatus.Closed && newStatus == TaskStatus.Open && !DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutSubTaskResumed(senders, task, subtask);

            return DaoFactory.SubtaskDao.Save(subtask);
        }

        public Subtask SaveOrUpdate(Subtask subtask, Task task)
        {
            if (subtask == null) throw new Exception("subtask.Task");
            if (task == null) throw new ArgumentNullException("task");
            if (task.Status == TaskStatus.Closed) throw new Exception("task can't be closed");

            // check guest responsible
            if (ProjectSecurity.IsVisitor(subtask.Responsible))
            {
                ProjectSecurity.CreateGuestSecurityException();
            }

            var isNew = subtask.ID == default(int); //Task is new
            var oldResponsible = Guid.Empty;

            subtask.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            subtask.LastModifiedOn = TenantUtil.DateTimeNow();

            if (isNew)
            {
                if (subtask.CreateBy == default(Guid)) subtask.CreateBy = SecurityContext.CurrentAccount.ID;
                if (subtask.CreateOn == default(DateTime)) subtask.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandEdit(task);
                subtask = DaoFactory.SubtaskDao.Save(subtask);
            }
            else
            {
                var oldSubtask = DaoFactory.SubtaskDao.GetById(new[] { subtask.ID }).First();

                if (oldSubtask == null) throw new ArgumentNullException("subtask");

                oldResponsible = oldSubtask.Responsible;

                //changed task
                ProjectSecurity.DemandEdit(task, oldSubtask);
                subtask = DaoFactory.SubtaskDao.Save(subtask);
            }

            NotifySubtask(task, subtask, isNew, oldResponsible);

            var senders = new HashSet<Guid> { subtask.Responsible, subtask.CreateBy };
            senders.Remove(Guid.Empty);

            foreach (var sender in senders)
            {
                Subscribe(task, sender);
            }

            FactoryIndexer<SubtasksWrapper>.IndexAsync(subtask);

            return subtask;
        }

        public Subtask Copy(Subtask from, Task task, IEnumerable<Participant> team)
        {
            var subtask = new Subtask
            {
                ID = default(int),
                CreateBy = SecurityContext.CurrentAccount.ID,
                CreateOn = TenantUtil.DateTimeNow(),
                Task = task.ID,
                Title = from.Title,
                Status = from.Status
            };

            if (team.Any(r => r.ID == from.Responsible))
            {
                subtask.Responsible = from.Responsible;
            }

            return SaveOrUpdate(subtask, task);
        }

        private void NotifySubtask(Task task, Subtask subtask, bool isNew, Guid oldResponsible)
        {
            //Don't send anything if notifications are disabled
            if (DisableNotifications) return;

            var recipients = GetSubscribers(task);

            if (!subtask.Responsible.Equals(Guid.Empty) && (isNew || !oldResponsible.Equals(subtask.Responsible)))
            {
                NotifyClient.Instance.SendAboutResponsibleBySubTask(subtask, task);
                recipients.RemoveAll(r => r.ID.Equals(subtask.Responsible.ToString()));
            }

            if (isNew)
            {
                NotifyClient.Instance.SendAboutSubTaskCreating(recipients, task, subtask);
            }
            else
            {
                NotifyClient.Instance.SendAboutSubTaskEditing(recipients, task, subtask);
            }
        }

        public void Delete(Subtask subtask, Task task)
        {
            if (subtask == null) throw new ArgumentNullException("subtask");
            if (task == null) throw new ArgumentNullException("task");

            ProjectSecurity.DemandEdit(task, subtask);
            DaoFactory.SubtaskDao.Delete(subtask.ID);

            var recipients = GetSubscribers(task);

            if (recipients.Any())
            {
                NotifyClient.Instance.SendAboutSubTaskDeleting(recipients, task, subtask);
            }
        }

        #endregion
    }
}
