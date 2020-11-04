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
using ASC.Web.Projects;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Engine
{
    public class TaskEngine : ProjectEntityEngine
    {
        public IDaoFactory DaoFactory { get; set; }
        public EngineFactory EngineFactory { get; set; }
        public SubtaskEngine SubtaskEngine { get { return EngineFactory.SubtaskEngine; } }
        public StatusEngine StatusEngine { get { return EngineFactory.StatusEngine; } }

        private readonly Func<Task, bool> canReadDelegate;

        public TaskEngine(bool disableNotifications)
            : base(NotifyConstants.Event_NewCommentForTask, disableNotifications)
        {
            canReadDelegate = CanRead;
        }

        #region Get Tasks

        public IEnumerable<Task> GetAll()
        {
            return DaoFactory.TaskDao.GetAll().Where(canReadDelegate);
        }

        public IEnumerable<Task> GetByProject(int projectId, TaskStatus? status, Guid participant)
        {
            var listTask = DaoFactory.TaskDao.GetByProject(projectId, status, participant).Where(canReadDelegate).ToList();
            DaoFactory.SubtaskDao.GetSubtasksForTasks(ref listTask);
            return listTask;
        }

        public TaskFilterOperationResult GetByFilter(TaskFilter filter)
        {
            if (filter.Offset < 0 || filter.Max < 0)
                return null;

            var isAdmin = ProjectSecurity.IsAdministrator(SecurityContext.CurrentAccount.ID);
            var anyOne = ProjectSecurity.IsPrivateDisabled;
            var count = DaoFactory.TaskDao.GetByFilterCount(filter, isAdmin, anyOne);

            var filterLimit = filter.Max;
            var filterOffset = filter.Offset;

            if (filterOffset > count.TasksTotal)
                return new TaskFilterOperationResult(count); //there are some records but we cant see them due to offset

            var taskList = new List<Task>();
            if (filter.HasTaskStatuses)
            {
                taskList = DaoFactory.TaskDao.GetByFilter(filter, isAdmin, anyOne);
            }
            else if (filterOffset > count.TasksOpen && count.TasksClosed != 0)
            {
                filter.TaskStatuses.Add(TaskStatus.Closed);
                filter.SortBy = "status_changed";
                filter.SortOrder = false;
                filter.Offset = filterOffset - count.TasksOpen;
                taskList = DaoFactory.TaskDao.GetByFilter(filter, isAdmin, anyOne);
            }
            else
            {
                //TODO: to one sql query using UNION ALL
                if (count.TasksOpen != 0)
                {
                    filter.TaskStatuses.Add(TaskStatus.Open);
                    taskList = DaoFactory.TaskDao.GetByFilter(filter, isAdmin, anyOne);
                }

                if (taskList.Count < filterLimit && count.TasksClosed != 0)
                {
                    filter.TaskStatuses.Clear();
                    filter.TaskStatuses.Add(TaskStatus.Closed);
                    filter.SortBy = "status_changed";
                    filter.SortOrder = false;
                    filter.Offset = 0;
                    filter.Max = filterLimit - taskList.Count;
                    taskList.AddRange(DaoFactory.TaskDao.GetByFilter(filter, isAdmin, anyOne));
                }
            }

            filter.Offset = filterOffset;
            filter.Max = filterLimit;
            filter.TaskStatuses.Clear();

            DaoFactory.SubtaskDao.GetSubtasksForTasks(ref taskList);

            var taskLinks = DaoFactory.TaskDao.GetLinks(taskList);

            Func<Task, int> idSelector = task => task.ID;
            Func<Task, IEnumerable<TaskLink>, Task> resultSelector = (task, linksCol) =>
                                                          {
                                                              task.Links.AddRange(linksCol);
                                                              return task;
                                                          };

            taskList = taskList.GroupJoin(taskLinks, idSelector, link => link.DependenceTaskId, resultSelector).ToList();
            taskList = taskList.GroupJoin(taskLinks, idSelector, link => link.ParentTaskId, resultSelector).ToList();

            return new TaskFilterOperationResult(taskList, count);
        }

        public TaskFilterCountOperationResult GetByFilterCount(TaskFilter filter)
        {
            return DaoFactory.TaskDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter)
        {
            return DaoFactory.TaskDao.GetByFilterCountForReport(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public IEnumerable<TaskFilterCountOperationResult> GetByFilterCountForStatistic(TaskFilter filter)
        {
            return DaoFactory.TaskDao.GetByFilterCountForStatistic(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public IEnumerable<Task> GetByResponsible(Guid responsibleId, params TaskStatus[] statuses)
        {
            var listTask = DaoFactory.TaskDao.GetByResponsible(responsibleId, statuses).Where(canReadDelegate).ToList();
            DaoFactory.SubtaskDao.GetSubtasksForTasks(ref listTask);
            return listTask;
        }

        public IEnumerable<Task> GetMilestoneTasks(int milestoneId)
        {
            var listTask = DaoFactory.TaskDao.GetMilestoneTasks(milestoneId).Where(canReadDelegate).ToList();
            DaoFactory.SubtaskDao.GetSubtasksForTasks(ref listTask);
            return listTask;
        }

        public override ProjectEntity GetEntityByID(int id)
        {
            return GetByID(id);
        }

        public Task GetByID(int id)
        {
            return GetByID(id, true);
        }

        public Task GetByID(int id, bool checkSecurity)
        {
            var task = DaoFactory.TaskDao.GetById(id);

            if (task != null)
            {
                task.SubTasks = DaoFactory.SubtaskDao.GetSubtasks(task.ID);
                task.Links = DaoFactory.TaskDao.GetLinks(task.ID).ToList();
            }

            if (!checkSecurity)
                return task;

            return CanRead(task) ? task : null;
        }

        public IEnumerable<Task> GetByID(ICollection<int> ids)
        {
            var listTask = DaoFactory.TaskDao.GetById(ids).Where(canReadDelegate).ToList();
            DaoFactory.SubtaskDao.GetSubtasksForTasks(ref listTask);
            return listTask;

        }

        public bool IsExists(int id)
        {
            return DaoFactory.TaskDao.IsExists(id);
        }

        private bool CanRead(Task task)
        {
            return ProjectSecurity.CanRead(task);
        }

        #endregion

        #region Save, Delete, Notify

        public Task SaveOrUpdate(Task task, IEnumerable<int> attachedFileIds, bool notifyResponsible, bool isImport = false)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (task.Project == null) throw new Exception("task.Project");

            var milestone = task.Milestone != 0 ? DaoFactory.MilestoneDao.GetById(task.Milestone) : null;
            var milestoneResponsible = milestone != null ? milestone.Responsible : Guid.Empty;

            var removeResponsibles = new List<Guid>();
            var inviteToResponsibles = new List<Guid>();

            task.Responsibles.RemoveAll(r => r.Equals(Guid.Empty));

            if (task.Deadline.Kind != DateTimeKind.Local && task.Deadline != DateTime.MinValue)
                task.Deadline = TenantUtil.DateTimeFromUtc(task.Deadline);

            if (task.StartDate.Kind != DateTimeKind.Local && task.StartDate != DateTime.MinValue)
                task.StartDate = TenantUtil.DateTimeFromUtc(task.StartDate);

            var isNew = task.ID == default(int); //Task is new

            if (isNew)
            {
                foreach (var responsible in task.Responsibles)
                {
                    if (ProjectSecurity.IsVisitor(responsible))
                    {
                        ProjectSecurity.CreateGuestSecurityException();
                    }

                    if (!ProjectSecurity.IsInTeam(task.Project, responsible))
                    {
                        ProjectSecurity.CreateSecurityException();
                    }
                }

                if (task.CreateBy == default(Guid)) task.CreateBy = SecurityContext.CurrentAccount.ID;
                if (task.CreateOn == default(DateTime)) task.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreate<Task>(task.Project);

                task = DaoFactory.TaskDao.Create(task);

                inviteToResponsibles.AddRange(task.Responsibles.Distinct());
            }
            else
            {
                var oldTask = DaoFactory.TaskDao.GetById(new[] { task.ID }).FirstOrDefault();

                if (oldTask == null) throw new ArgumentNullException("task");
                ProjectSecurity.DemandEdit(oldTask);

                var newResponsibles = task.Responsibles.Distinct().ToList();
                var oldResponsibles = oldTask.Responsibles.Distinct().ToList();

                foreach (var responsible in newResponsibles.Except(oldResponsibles))
                {
                    if (ProjectSecurity.IsVisitor(responsible))
                    {
                        ProjectSecurity.CreateGuestSecurityException();
                    }

                    if (!ProjectSecurity.IsInTeam(task.Project, responsible))
                    {
                        ProjectSecurity.CreateSecurityException();
                    }
                }

                removeResponsibles.AddRange(oldResponsibles.Where(p => !newResponsibles.Contains(p)));
                inviteToResponsibles.AddRange(newResponsibles.Where(participant => !oldResponsibles.Contains(participant)));

                task.LastModifiedBy = SecurityContext.CurrentAccount.ID;
                task.LastModifiedOn = TenantUtil.DateTimeNow();

                task = DaoFactory.TaskDao.Update(task);
            }

            FactoryIndexer<TasksWrapper>.IndexAsync(task);

            if (attachedFileIds != null && attachedFileIds.Any())
            {
                foreach (var attachedFileId in attachedFileIds)
                {
                    AttachFile(task, attachedFileId);
                }
            }

            var senders = new HashSet<Guid>(task.Responsibles) { task.Project.Responsible, milestoneResponsible, task.CreateBy };
            senders.Remove(Guid.Empty);

            foreach (var subscriber in senders)
            {
                Subscribe(task, subscriber);
            }

            inviteToResponsibles.RemoveAll(r => r.Equals(Guid.Empty));
            removeResponsibles.RemoveAll(r => r.Equals(Guid.Empty));

            NotifyTask(task, inviteToResponsibles, removeResponsibles, isNew, notifyResponsible);

            return task;
        }

        public Task ChangeStatus(Task task, CustomTaskStatus newStatus)
        {
            ProjectSecurity.DemandEdit(task);

            if (task == null) throw new ArgumentNullException("task");
            if (task.Project == null) throw new Exception("Project can't be null.");
            if (task.Project.Status == ProjectStatus.Closed) throw new Exception(EngineResource.ProjectClosedError);

            if (task.Status == newStatus.StatusType && task.CustomTaskStatus == newStatus.Id) return task;

            var status = StatusEngine.Get().FirstOrDefault(r => r.Id == newStatus.Id);
            var cannotChange =
                status != null &&
                status.Available.HasValue && !status.Available.Value &&
                task.CreateBy != SecurityContext.CurrentAccount.ID &&
                task.Project.Responsible != SecurityContext.CurrentAccount.ID &&
                !ProjectSecurity.CurrentUserAdministrator;

            if (cannotChange)
            {
                ProjectSecurity.CreateSecurityException();
            }


            var senders = GetSubscribers(task);

            if (newStatus.StatusType == TaskStatus.Closed && !DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutTaskClosing(senders, task);

            if (newStatus.StatusType == TaskStatus.Open && !DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutTaskResumed(senders, task);

            task.Status = newStatus.StatusType;
            task.CustomTaskStatus = newStatus.Id < 0 ? null : (int?)newStatus.Id;
            task.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            task.LastModifiedOn = TenantUtil.DateTimeNow();
            task.StatusChangedOn = TenantUtil.DateTimeNow();

            //subtask
            if (newStatus.StatusType == TaskStatus.Closed)
            {
                if (!task.Responsibles.Any())
                    task.Responsibles.Add(SecurityContext.CurrentAccount.ID);

                DaoFactory.SubtaskDao.CloseAllSubtasks(task);
                foreach (var subTask in task.SubTasks)
                {
                    subTask.Status = TaskStatus.Closed;
                }
            }

            return DaoFactory.TaskDao.Update(task);
        }

        public Task CopySubtasks(Task @from, Task to, IEnumerable<Participant> team)
        {
            if (from.Status == TaskStatus.Closed) return to;

            var subtaskEngine = SubtaskEngine;
            var subTasks = DaoFactory.SubtaskDao.GetSubtasks(@from.ID);

            to.SubTasks = new List<Subtask>();

            foreach (var subtask in subTasks)
            {
                to.SubTasks.Add(subtaskEngine.Copy(subtask, to, team));
            }

            return to;
        }

        public Task CopyFiles(Task from, Task to)
        {
            if (from.Project.ID != to.Project.ID) return to;

            var files = GetFiles(from);

            foreach (var file in files)
            {
                AttachFile(to, file.ID);
            }

            return to;
        }

        public Task MoveToMilestone(Task task, int milestoneID)
        {
            ProjectSecurity.DemandEdit(task);

            if (task == null) throw new ArgumentNullException("task");
            if (task.Project == null) throw new Exception("Project can be null.");

            var newMilestone = milestoneID != 0;
            var milestone = DaoFactory.MilestoneDao.GetById(newMilestone ? milestoneID : task.Milestone);

            var senders = GetSubscribers(task);

            if (!DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutTaskRemoved(senders, task, milestone, newMilestone);

            task.Milestone = milestoneID;
            task.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            task.LastModifiedOn = TenantUtil.DateTimeNow();

            return DaoFactory.TaskDao.Update(task);
        }

        public void NotifyTask(Task task, IEnumerable<Guid> inviteToResponsibles, IEnumerable<Guid> removeResponsibles, bool isNew, bool notifyResponsible)
        {
            if (DisableNotifications) return;

            var senders = GetSubscribers(task);
            senders = senders.FindAll(r => !inviteToResponsibles.Contains(new Guid(r.ID)) && !removeResponsibles.Contains(new Guid(r.ID)));

            if (senders.Any())
            {
                if (isNew)
                {
                    NotifyClient.Instance.SendAboutTaskCreating(senders, task);
                }
                else
                {
                    NotifyClient.Instance.SendAboutTaskEditing(senders, task);
                }
            }

            if (notifyResponsible)
                NotifyResponsible(task, inviteToResponsibles.ToList(), removeResponsibles.ToList());
        }

        private void NotifyResponsible(Task task, List<Guid> inviteToResponsibles, List<Guid> removeResponsibles)
        {
            if (DisableNotifications) return;

            if (inviteToResponsibles.Any())
                NotifyClient.Instance.SendAboutResponsibleByTask(inviteToResponsibles, task);

            if (removeResponsibles.Any())
                NotifyClient.Instance.SendAboutRemoveResponsibleByTask(removeResponsibles, task);
        }

        public void SendReminder(Task task)
        {
            //Don't send anything if notifications are disabled
            if (DisableNotifications || task.Responsibles == null || !task.Responsibles.Any()) return;

            NotifyClient.Instance.SendReminderAboutTask(task.Responsibles.Where(r => !r.Equals(SecurityContext.CurrentAccount.ID)).Distinct(), task);
        }


        public void Delete(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");

            ProjectSecurity.DemandDelete(task);
            DaoFactory.TaskDao.Delete(task);

            var recipients = GetSubscribers(task);

            if (recipients.Count != 0)
            {
                NotifyClient.Instance.SendAboutTaskDeleting(recipients, task);
            }

            UnSubscribeAll(task);

            FactoryIndexer<TasksWrapper>.DeleteAsync(task);
        }

        #endregion

        #region Link

        public void AddLink(Task parentTask, Task dependentTask, TaskLinkType linkType)
        {
            CheckLink(parentTask, dependentTask, linkType);

            var link = new TaskLink
            {
                ParentTaskId = parentTask.ID,
                DependenceTaskId = dependentTask.ID,
                LinkType = linkType
            };

            if (DaoFactory.TaskDao.IsExistLink(link))
                throw new Exception("link already exist");

            ProjectSecurity.DemandEdit(dependentTask);
            ProjectSecurity.DemandEdit(parentTask);

            parentTask.Links.Add(link);
            dependentTask.Links.Add(link);

            DaoFactory.TaskDao.AddLink(link);
        }

        public void RemoveLink(Task dependentTask, Task parentTask)
        {
            ProjectSecurity.DemandEdit(dependentTask);

            DaoFactory.TaskDao.RemoveLink(new TaskLink { DependenceTaskId = dependentTask.ID, ParentTaskId = parentTask.ID });
            dependentTask.Links.RemoveAll(r => r.ParentTaskId == parentTask.ID && r.DependenceTaskId == dependentTask.ID);
            parentTask.Links.RemoveAll(r => r.ParentTaskId == parentTask.ID && r.DependenceTaskId == dependentTask.ID);
        }

        private static void CheckLink(Task parentTask, Task dependentTask)
        {
            if (parentTask == null) throw new ArgumentNullException("parentTask");
            if (dependentTask == null) throw new ArgumentNullException("dependentTask");

            if (parentTask.ID == dependentTask.ID)
            {
                throw new Exception("it is impossible to create a link between one and the same task");
            }

            /*            if (parentTask.Status == TaskStatus.Closed || dependentTask.Status == TaskStatus.Closed)
                        {
                            throw new Exception("Such link don't be created. Task closed.");
                        }*/

            if (parentTask.Milestone != dependentTask.Milestone)
            {
                throw new Exception("Such link don't be created. Different Milestones");
            }

        }

        private static void CheckLink(Task parentTask, Task dependentTask, TaskLinkType linkType)
        {
            CheckLink(parentTask, dependentTask);

            switch (linkType)
            {
                case TaskLinkType.End:
                    if ((parentTask.Deadline.Equals(DateTime.MinValue) && parentTask.Milestone == 0) || (dependentTask.Deadline.Equals(DateTime.MinValue) && dependentTask.Milestone == 0))
                    {
                        throw new Exception("Such link don't be created. Incorrect task link type.");
                    }
                    break;

                case TaskLinkType.EndStart:
                    if ((parentTask.Deadline.Equals(DateTime.MinValue) && parentTask.Milestone == 0))
                    {
                        throw new Exception("Such link don't be created. Incorrect task link type.");
                    }
                    break;
            }
        }

        #endregion
    }
}
