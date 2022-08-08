/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Api.Attributes;
using ASC.Api.Documents;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.Common.Logging;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;
using ASC.Web.Projects;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region tasks

        ///<summary>
        ///Returns a list with the detailed information about all the tasks for the current user.
        ///</summary>
        ///<short>
        ///Get my tasks
        ///</short>
        ///<category>Tasks</category>
        ///<returns>List of tasks</returns>
        [Read(@"task/@self")]
        public IEnumerable<TaskWrapper> GetMyTasks()
        {
            return EngineFactory
                .TaskEngine.GetByResponsible(CurrentUserId)
                .Select(TaskWrapperSelector)
                .ToList();
        }

        ///<summary>
        ///Returns a list with the detailed information about the tasks for the current user with a status specified in the request.
        ///</summary>
        ///<short>
        ///Get my tasks by status
        ///</short>
        ///<category>Tasks</category>
        ///<param name="status">Task status: not accept|open|closed|disable|unclassified|not in milestone</param>
        ///<returns>List of tasks</returns>
        [Read(@"task/@self/{status:(notaccept|open|closed|disable|unclassified|notinmilestone)}")]
        public IEnumerable<TaskWrapper> GetMyTasks(TaskStatus status)
        {
            return EngineFactory
                .TaskEngine.GetByResponsible(CurrentUserId, status)
                .Select(TaskWrapperSelector)
                .ToList();
        }

        ///<summary>
        ///Returns the detailed information about a task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Get a task
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}")]
        public TaskWrapperFull GetTask(int taskid)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var commentsCount = EngineFactory.CommentEngine.Count(task);
            var isSubscribed = EngineFactory.TaskEngine.IsSubscribed(task);
            var milestone = EngineFactory.MilestoneEngine.GetByID(task.Milestone, false);
            var timeSpend = EngineFactory.TimeTrackingEngine.GetByTask(task.ID).Sum(r => r.Hours);
            var project = ProjectWrapperFullSelector(task.Project, EngineFactory.FileEngine.GetRoot(task.Project.ID));
            var files = EngineFactory.TaskEngine.GetFiles(task).Select(FileWrapperSelector);
            var comments = EngineFactory.CommentEngine.GetComments(task);
            var filteredComments = comments.Where(r => r.Parent.Equals(Guid.Empty)).Select(x => GetCommentInfo(comments, x, task)).ToList();
            return new TaskWrapperFull(this, task, milestone, project, files, filteredComments, commentsCount, isSubscribed, timeSpend);
        }

        public TaskWrapper GetTask(Task task)
        {
            if (task.Milestone == 0) return TaskWrapperSelector(task);

            var milestone = EngineFactory.MilestoneEngine.GetByID(task.Milestone, false);
            return new TaskWrapper(this, task, milestone);
        }

        ///<summary>
        ///Returns the detailed information about the tasks with the IDs specified in the request.
        ///</summary>
        ///<short>
        ///Get tasks
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task IDs</param>
        ///<returns>Task</returns>
        ///<visible>false</visible>
        [Read(@"task")]
        public IEnumerable<TaskWrapper> GetTask(IEnumerable<int> taskid)
        {
            var tasks = EngineFactory.TaskEngine.GetByID(taskid.ToList()).NotFoundIfNull();
            return tasks.Select(TaskWrapperSelector).ToList();
        }

        ///<summary>
        ///Returns a list with the detailed information about all the tasks matching the filter parameters specified in the request.
        ///</summary>
        ///<short>
        ///Get tasks by filter
        ///</short>
        ///<category>Tasks</category>
        ///<param name="projectid" optional="true"> Project ID</param>
        ///<param name="myProjects">Returns tasks only from my projects</param>
        ///<param name="milestone" optional="true">Milestone ID</param>
        ///<param name="myMilestones">Returns tasks only from my milestones</param>
        ///<param name="nomilestone">Returns tasks only without milestones</param>
        ///<param name="tag" optional="true">Project tag</param>
        ///<param name="status" optional="true">Task status</param>
        ///<param name="substatus" optional="true">Custom task status</param>
        ///<param name="follow">Messages only from followed tasks or not</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="creator">Creator GUID</param>
        ///<param name="deadlineStart" optional="true">Minimum value of task deadline</param>
        ///<param name="deadlineStop" optional="true">Maximum value of task deadline</param>
        ///<param name="lastId">Last task ID</param>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/filter")]
        public IEnumerable<TaskWrapper> GetTaskByFilter(int projectid, bool myProjects, int? milestone,
            bool myMilestones, bool nomilestone, int tag,
            TaskStatus? status, int? substatus, bool follow, Guid departament, Guid? participant, Guid creator,
            ApiDateTime deadlineStart, ApiDateTime deadlineStop, int lastId)
        {
            var filter = CreateFilter(EntityType.Task);
            filter.DepartmentId = departament;
            filter.ParticipantId = participant;
            filter.UserId = creator;
            filter.Milestone = nomilestone ? 0 : milestone;
            filter.FromDate = deadlineStart;
            filter.ToDate = deadlineStop;
            filter.TagId = tag;
            filter.LastId = lastId;
            filter.MyProjects = myProjects;
            filter.MyMilestones = myMilestones;
            filter.Follow = follow;
            filter.Substatus = substatus;

            if (projectid != 0)
                filter.ProjectIds.Add(projectid);

            if (status != null)
                filter.TaskStatuses.Add((TaskStatus)status);

            var filterResult = EngineFactory.TaskEngine.GetByFilter(filter).NotFoundIfNull();

            SetTotalCount(filterResult.FilterCount.TasksTotal);

            ProjectSecurity.GetTaskSecurityInfo(filterResult.FilterResult);

            return filterResult.FilterResult.Select(TaskWrapperSelector).ToList();
        }

        ///<summary>
        ///Returns a list of all the tasks matching the filter parameters specified in the request.
        ///</summary>
        ///<short>
        ///Get tasks without detailed information by filter
        ///</short>
        ///<category>Tasks</category>
        ///<param name="projectid" optional="true"> Project ID</param>
        ///<param name="myProjects">Returns tasks only from my projects</param>
        ///<param name="milestone" optional="true">Milestone ID</param>
        ///<param name="myMilestones">Returns tasks only from my milestones</param>
        ///<param name="tag" optional="true">Project Tag</param>
        ///<param name="status" optional="true">Task Status</param>
        ///<param name="follow">Messages only from followed tasks or not</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="creator">Creator GUID</param>
        ///<param name="deadlineStart" optional="true">Minimum value of task deadline</param>
        ///<param name="deadlineStop" optional="true">Maximum value of task deadline</param>
        ///<param name="lastId">Last task ID</param>
        ///<visible>false</visible>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/filter/simple")]
        public IEnumerable<SimpleTaskWrapper> GetSimpleTaskByFilter(int projectid, bool myProjects, int? milestone,
            bool myMilestones, int tag,
            TaskStatus? status, bool follow, Guid departament, Guid? participant, Guid creator,
            ApiDateTime deadlineStart, ApiDateTime deadlineStop, int lastId)
        {
            var filter = CreateFilter(EntityType.Task);
            filter.DepartmentId = departament;
            filter.ParticipantId = participant;
            filter.UserId = creator;
            filter.Milestone = milestone;
            filter.FromDate = deadlineStart;
            filter.ToDate = deadlineStop;
            filter.TagId = tag;
            filter.LastId = lastId;
            filter.MyProjects = myProjects;
            filter.MyMilestones = myMilestones;
            filter.Follow = follow;

            if (projectid != 0)
                filter.ProjectIds.Add(projectid);

            if (status != null)
                filter.TaskStatuses.Add((TaskStatus)status);

            var filterResult = EngineFactory.TaskEngine.GetByFilter(filter).NotFoundIfNull();

            SetTotalCount(filterResult.FilterCount.TasksTotal);

            return filterResult.FilterResult.Select(r => new SimpleTaskWrapper(this, r));
        }

        ///<summary>
        ///Returns a list of all the files attached to the task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Get task files
        ///</short>
        ///<category>Files</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>List of files</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/files")]
        public IEnumerable<FileWrapper> GetTaskFiles(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(task.Project);

            return taskEngine.GetFiles(task).Select(FileWrapperSelector);
        }

        ///<summary>
        ///Uploads the files specified in the request to the selected task.
        ///</summary>
        ///<short>
        ///Upload files to the task
        ///</short>
        ///<category>Files</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="files">File IDs</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{taskid:[0-9]+}/files")]
        public TaskWrapper UploadFilesToTask(int taskid, IEnumerable<int> files)
        {
            var taskEngine = EngineFactory.TaskEngine;
            var fileEngine = EngineFactory.FileEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(task.Project);

            var filesList = files.ToList();
            var attachments = new List<Files.Core.File>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid).NotFoundIfNull();
                attachments.Add(file);
                taskEngine.AttachFile(task, file.ID, true);
            }

            MessageService.Send(Request, MessageAction.TaskAttachedFiles, MessageTarget.Create(task.ID), task.Project.Title, task.Title, attachments.Select(x => x.Title));

            return TaskWrapperSelector(task);
        }

        ///<summary>
        ///Detaches the selected file from a task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Detach a file from a task
        ///</short>
        ///<category>Files</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="fileid">File ID</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}/files")]
        public TaskWrapper DetachFileFromTask(int taskid, int fileid)
        {
            var fileEngine = EngineFactory.FileEngine;
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(task.Project);

            var file = fileEngine.GetFile(fileid).NotFoundIfNull();
            taskEngine.DetachFile(task, fileid);
            MessageService.Send(Request, MessageAction.TaskDetachedFile, MessageTarget.Create(task.ID), task.Project.Title, task.Title, file.Title);

            return TaskWrapperSelector(task);
        }

        ///<summary>
        ///Detaches the selected files from a task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Detach files from a task
        ///</short>
        ///<category>Files</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="files">File IDs</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<visible>false</visible>
        [Delete(@"task/{taskid:[0-9]+}/filesmany")]
        public TaskWrapper DetachFileFromTask(int taskid, List<int> files)
        {
            var fileEngine = EngineFactory.FileEngine;
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(task.Project);

            var filesList = files.ToList();
            var attachments = new List<Files.Core.File>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid).NotFoundIfNull();
                attachments.Add(file);
                taskEngine.AttachFile(task, file.ID, true);
            }

            MessageService.Send(Request, MessageAction.TaskDetachedFile, MessageTarget.Create(task.ID), task.Project.Title, task.Title, attachments.Select(x => x.Title));

            return TaskWrapperSelector(task);
        }

        /// <summary>
        /// Updates a status of a task with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Update a task status by task ID
        /// </short>
        /// <category>Tasks</category>
        /// <param name="taskid">Task ID</param>
        /// <param name="status">New task status: Open or Closed</param>
        /// <param name="statusId">Custom status ID</param>
        /// <returns>Updated task</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/status")]
        public TaskWrapperFull UpdateTask(int taskid, TaskStatus status, int statusId = 0)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();

            var customStatus = EngineFactory.StatusEngine.GetWithDefaults().FirstOrDefault(r => r.Id == statusId) ??
                                        CustomTaskStatus.GetDefaults().First(r => r.StatusType == status);

            EngineFactory.TaskEngine.ChangeStatus(task, customStatus);
            MessageService.Send(Request, MessageAction.TaskUpdatedStatus, MessageTarget.Create(task.ID), task.Project.Title, task.Title, LocalizedEnumConverter.ConvertToString(task.Status));

            return GetTask(taskid);
        }

        ///<summary>
        ///Updates a status of the tasks with the IDs specified in the request.
        ///</summary>
        ///<short>
        ///Update a status of tasks
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskids">Task IDs</param>
        ///<param name="status">New task status: Open or Closed</param>
        ///<param name="statusId">New custom status ID</param>
        ///<returns>Updated tasks</returns>
        [Update(@"task/status")]
        public IEnumerable<TaskWrapperFull> UpdateTasks(int[] taskids, TaskStatus status, int statusId = 0)
        {
            var result = new List<TaskWrapperFull>(taskids.Length);

            foreach (var taskId in taskids)
            {
                try
                {
                    result.Add(UpdateTask(taskId, status, statusId));
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error("UpdateTasks", e);
                }
            }

            return result;
        }

        ///<summary>
        ///Updates a milestone of a task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Update a task milestone
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="milestoneid">Milestone ID</param>
        ///<returns>Updated task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/milestone")]
        public TaskWrapperFull UpdateTask(int taskid, int milestoneid)
        {
            if (milestoneid < 0) throw new ArgumentNullException("milestoneid");

            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            var milestoneEngine = EngineFactory.MilestoneEngine;
            var milestone = milestoneEngine.GetByID(milestoneid);

            taskEngine.MoveToMilestone(task, milestoneid);
            if (milestone != null)
            {
                MessageService.Send(Request, MessageAction.TaskMovedToMilestone, MessageTarget.Create(task.ID), task.Project.Title, milestone.Title, task.Title);
            }
            else
            {
                MessageService.Send(Request, MessageAction.TaskUnlinkedMilestone, MessageTarget.Create(task.ID), task.Project.Title, task.Title);
            }

            return GetTask(taskid);
        }

        ///<summary>
        ///Updates a milestone of the tasks with the IDs specified in the request.
        ///</summary>
        ///<short>
        ///Update a milestone of tasks
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskids">Task IDs</param>
        ///<param name="milestoneid">Milestone ID</param>
        ///<returns>Updated tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/milestone")]
        public IEnumerable<TaskWrapperFull> UpdateTasks(int[] taskids, int milestoneid)
        {
            if (milestoneid < 0) throw new ArgumentNullException("milestoneid");

            var result = new List<TaskWrapperFull>(taskids.Length);

            foreach (var taskid in taskids)
            {
                try
                {
                    result.Add(UpdateTask(taskid, milestoneid));
                }
                catch (Exception)
                {

                }
            }

            return result;
        }

        ///<summary>
        ///Copies a task with the parameters specified in the request.
        ///</summary>
        ///<short>
        ///Copy a task
        ///</short>
        ///<category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="description">Task description</param>
        ///<param name="deadline">Task deadline</param>
        ///<param name="priority">Task priority: Low|Normal|High</param>
        ///<param name="title">Task title</param>
        ///<param name="milestoneid">Task milestone ID</param>
        ///<param name="responsibles">List of task responsibles</param>
        ///<param name="notify">Notifies responsibles about the task actions or not</param>
        ///<param name="startDate">Task start date</param>
        ///<param name="copyFrom">Task ID from which the information is copied</param>
        ///<param name="copySubtasks">Specifies if the subtasks will be copied or not</param>
        ///<param name="copyFiles">Specifies if the files will be copied or not</param>
        ///<param name="removeOld">Specifies if the original task will be removed or not</param>
        ///<returns>Copied task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{copyFrom:[0-9]+}/copy")]
        public TaskWrapper CopyTask(int projectid, string description, ApiDateTime deadline,
                                          TaskPriority priority, string title, int milestoneid,
                                          IEnumerable<Guid> responsibles, bool notify, ApiDateTime startDate,
                                          int copyFrom, bool copySubtasks, bool copyFiles, bool removeOld)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            var projectEngine = EngineFactory.ProjectEngine;
            var taskEngine = EngineFactory.TaskEngine;

            var copyFromTask = taskEngine.GetByID(copyFrom).NotFoundIfNull();
            var project = projectEngine.GetByID(projectid).NotFoundIfNull();

            if (!EngineFactory.MilestoneEngine.IsExists(milestoneid) && milestoneid > 0)
            {
                throw new ItemNotFoundException("Milestone not found");
            }

            var team = projectEngine.GetTeam(project.ID);
            var teamIds = team.Select(r => r.ID).ToList();

            if (responsibles.Any(responsible => !teamIds.Contains(responsible)))
            {
                throw new ArgumentException(@"responsibles", "responsibles");
            }

            var task = new Task
            {
                CreateBy = CurrentUserId,
                CreateOn = TenantUtil.DateTimeNow(),
                Deadline = deadline,
                Description = description ?? "",
                Priority = priority,
                Status = TaskStatus.Open,
                Title = title,
                Project = project,
                Milestone = milestoneid,
                Responsibles = new List<Guid>(responsibles.Distinct()),
                StartDate = startDate
            };

            taskEngine.SaveOrUpdate(task, null, notify);

            if (copySubtasks)
            {
                taskEngine.CopySubtasks(copyFromTask, task, team);
            }

            if (copyFiles)
            {
                taskEngine.CopyFiles(copyFromTask, task);
            }

            if (removeOld)
            {
                taskEngine.Delete(copyFromTask);
            }

            MessageService.Send(Request, MessageAction.TaskCreated, MessageTarget.Create(task.ID), project.Title, task.Title);

            return GetTask(task);
        }

        ///<summary>
        ///Updates the selected task with the parameters (responsible user ID, task description, deadline time, etc) specified in the request.
        ///</summary>
        ///<short>
        ///Update a task
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="description">New task description</param>
        ///<param name="deadline">New task deadline time</param>
        ///<param name="startDate">New task start date</param>
        ///<param name="priority">New task priority</param>
        ///<param name="title">New task title</param>
        ///<param name="milestoneid">New task milestone ID</param>
        ///<param name="responsibles">New list of task responsibles</param>
        ///<param name="projectID">New task project ID</param>
        ///<param name="notify">Notifies responsibles about the task actions or not</param>
        ///<param name="status" optional="true">New task status</param>
        ///<param name="progress" optional="true">New task progress</param>
        ///<returns>Updated task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}")]
        public TaskWrapperFull UpdateProjectTask(
            int taskid,
            string description,
            ApiDateTime deadline,
            ApiDateTime startDate,
            TaskPriority? priority,
            string title,
            int milestoneid,
            IEnumerable<Guid> responsibles,
            int? projectID,
            bool notify,
            TaskStatus? status,
            int? progress)
        {
            var projectEngine = EngineFactory.ProjectEngine;
            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            if (!EngineFactory.MilestoneEngine.IsExists(milestoneid) && milestoneid > 0)
            {
                throw new ItemNotFoundException("Milestone not found");
            }

            var distinctResponsibles = new List<Guid>(responsibles.Distinct());

            var acceptance = task.Responsibles.Count == 0 && distinctResponsibles.Count == 1 && distinctResponsibles.First() == CurrentUserId;

            var hasChanges = !(task.Responsibles.Count == distinctResponsibles.Count && task.Responsibles.All(distinctResponsibles.Contains));

            task.Responsibles = distinctResponsibles;

            task.Deadline = Update.IfNotEquals(TenantUtil.DateTimeToUtc(task.Deadline), deadline, ref hasChanges);
            task.Description = Update.IfNotEquals(task.Description, description, ref hasChanges);

            if (priority.HasValue)
            {
                task.Priority = Update.IfNotEquals(task.Priority, priority.Value, ref hasChanges);
            }

            task.Title = Update.IfNotEmptyAndNotEquals(task.Title, title, ref hasChanges);
            task.Milestone = Update.IfNotEquals(task.Milestone, milestoneid, ref hasChanges);
            task.StartDate = Update.IfNotEquals(TenantUtil.DateTimeToUtc(task.StartDate), startDate, ref hasChanges);

            if (projectID.HasValue)
            {
                if (task.Project.ID != projectID.Value)
                {
                    var project = projectEngine.GetByID(projectID.Value).NotFoundIfNull();
                    task.Project = project;
                    hasChanges = true;
                }
            }

            if (progress.HasValue)
            {
                task.Progress = Update.IfNotEquals(task.Progress, progress.Value, ref hasChanges);
            }

            if (hasChanges)
            {
                taskEngine.SaveOrUpdate(task, null, notify);

                if (acceptance)
                {
                    if (!projectEngine.IsInTeam(task.Project.ID, CurrentUserId))
                    {
                        projectEngine.AddToTeam(task.Project, CurrentUserId, false);
                    }
                }
            }

            if (status.HasValue)
            {
                var newStatus = CustomTaskStatus.GetDefaults().First(r => r.StatusType == status.Value);

                if (task.Status != newStatus.StatusType || task.CustomTaskStatus != newStatus.Id)
                {
                    hasChanges = true;
                    taskEngine.ChangeStatus(task, newStatus);
                }
            }

            if (hasChanges)
            {
                MessageService.Send(Request, MessageAction.TaskUpdated, MessageTarget.Create(task.ID), task.Project.Title, task.Title);
            }

            return GetTask(taskid);
        }

        ///<summary>
        ///Deletes a task with the ID specified in the request from the project.
        ///</summary>
        ///<short>
        ///Delete a task
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>Deleted task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}")]
        public TaskWrapper DeleteTask(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            taskEngine.Delete(task);
            MessageService.Send(Request, MessageAction.TaskDeleted, MessageTarget.Create(task.ID), task.Project.Title, task.Title);

            return TaskWrapperSelector(task);
        }

        ///<summary>
        ///Deletes the tasks with the IDs specified in the request from the project.
        ///</summary>
        ///<short>
        ///Delete tasks
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskids">Task IDs</param>
        ///<returns>Deleted tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task")]
        public IEnumerable<TaskWrapper> DeleteTasks(int[] taskids)
        {
            var result = new List<TaskWrapper>(taskids.Length);

            foreach (var taskId in taskids)
            {
                try
                {
                    result.Add(DeleteTask(taskId));
                }
                catch (Exception)
                {

                }
            }

            return result;
        }

        ///<summary>
        ///Returns a list of the comments for the task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Get task comments
        ///</short>
        ///<category>Comments</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>List of comments</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/comment")]
        public IEnumerable<CommentWrapper> GetTaskComments(int taskid)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            return EngineFactory.CommentEngine.GetComments(task).Select(x => new CommentWrapper(this, x, task));
        }


        ///<summary>
        ///Adds a comment to the selected task with the comment text and parent comment ID specified in the request.
        ///</summary>
        ///<short>
        ///Add a task comment
        ///</short>
        ///<category>Comments</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="content">Comment text</param>
        ///<param name="parentid">Parent comment ID</param>
        ///<returns>List of comments</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{taskid:[0-9]+}/comment")]
        public CommentWrapper AddTaskComments(int taskid, string content, Guid parentid)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(@"Comment text is empty", content);
            if (parentid != Guid.Empty && EngineFactory.CommentEngine.GetByID(parentid) == null) throw new ItemNotFoundException("parent comment not found");

            var comment = new Comment
            {
                Content = content,
                TargetUniqID = ProjectEntity.BuildUniqId<Task>(taskid),
                CreateBy = CurrentUserId,
                CreateOn = Core.Tenants.TenantUtil.DateTimeNow()
            };

            if (parentid != Guid.Empty)
            {
                comment.Parent = parentid;
            }

            var task = EngineFactory.CommentEngine.GetEntityByTargetUniqId(comment).NotFoundIfNull();

            EngineFactory.CommentEngine.SaveOrUpdateComment(task, comment);

            MessageService.Send(Request, MessageAction.TaskCommentCreated, MessageTarget.Create(comment.ID), task.Project.Title, task.Title);

            return new CommentWrapper(this, comment, task);
        }

        ///<summary>
        ///Notifies the responsible for the task with the ID specified in the request about the task.
        ///</summary>
        ///<short>
        ///Notify the task responsible
        ///</short>
        ///<category>Tasks</category>
        ///<returns>Task</returns>
        ///<param name="taskid">Task ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/notify")]
        public TaskWrapper NotifyTaskResponsible(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            taskEngine.SendReminder(task);

            return TaskWrapperSelector(task);
        }

        ///<summary>
        ///Subscribes to the notifications about the actions performed with the selected task.
        ///</summary>
        ///<short>
        ///Subscribe to task actions
        ///</short>
        ///<category>Tasks</category>
        ///<returns>Task</returns>
        ///<param name="taskid">Task ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/subscribe")]
        public TaskWrapper SubscribeToTask(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            taskEngine.Follow(task);
            MessageService.Send(Request, MessageAction.TaskUpdatedFollowing, MessageTarget.Create(task.ID), task.Project.Title, task.Title);

            return TaskWrapperSelector(task);
        }

        ///<summary>
        ///Checks the subscription to the notifications about the actions performed with the selected task.
        ///</summary>
        ///<short>
        ///Check the subscription to task actions
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>Boolean value: True - subscribed, False - unsubscribed</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/subscribe")]
        public bool IsSubscribeToTask(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            return taskEngine.IsSubscribed(task);
        }

        ///<summary>
        ///Adds a link between the dependent and parent tasks specified in the request.
        ///</summary>
        ///<short>
        ///Add a link between tasks
        ///</short>
        ///<category>Tasks</category>
        ///<param name="parentTaskId">Parent task ID</param>
        ///<param name="dependenceTaskId">Dependent task ID</param>
        ///<param name="linkType">Link type</param>
        ///<returns>Dependent task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{parentTaskId:[0-9]+}/link")]
        public TaskWrapper AddLink(int parentTaskId, int dependenceTaskId, TaskLinkType linkType)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var dependentTask = taskEngine.GetByID(dependenceTaskId).NotFoundIfNull();
            var parentTask = taskEngine.GetByID(parentTaskId).NotFoundIfNull();

            taskEngine.AddLink(parentTask, dependentTask, linkType);
            MessageService.Send(Request, MessageAction.TasksLinked, MessageTarget.Create(new[] { parentTask.ID, dependentTask.ID }), parentTask.Project.Title, parentTask.Title, dependentTask.Title);

            return TaskWrapperSelector(dependentTask);
        }

        ///<summary>
        ///Removes a link between the dependent and parent tasks specified in the request.
        ///</summary>
        ///<short>
        ///Remove a link between tasks 
        ///</short>
        ///<category>Tasks</category>
        ///<param name="dependenceTaskId">Dependent task ID</param>
        ///<param name="parentTaskId">Parent task ID</param>
        ///<returns>Dependent task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}/link")]
        public TaskWrapper RemoveLink(int dependenceTaskId, int parentTaskId)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var dependentTask = taskEngine.GetByID(dependenceTaskId).NotFoundIfNull();
            var parentTask = taskEngine.GetByID(parentTaskId).NotFoundIfNull();

            taskEngine.RemoveLink(dependentTask, parentTask);
            MessageService.Send(Request, MessageAction.TasksUnlinked, MessageTarget.Create(new[] { parentTask.ID, dependentTask.ID }), parentTask.Project.Title, parentTask.Title, dependentTask.Title);

            return TaskWrapperSelector(dependentTask);
        }

        #endregion

        #region subtasks

        ///<summary>
        ///Creates a subtask with the title and responsible within the parent task specified in the request.
        ///</summary>
        ///<short>
        ///Create a subtask
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Parent task ID</param>
        ///<param name="responsible">Subtask responsible</param>
        ///<param name="title">Subtask title</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{taskid:[0-9]+}")]
        public SubtaskWrapper AddSubtask(int taskid, Guid responsible, string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();

            if (task.Status == TaskStatus.Closed) throw new ArgumentException(@"task can't be closed");

            var subtask = new Subtask
            {
                Responsible = responsible,
                Task = task.ID,
                Status = TaskStatus.Open,
                Title = title
            };

            subtask = EngineFactory.SubtaskEngine.SaveOrUpdate(subtask, task);
            MessageService.Send(Request, MessageAction.SubtaskCreated, MessageTarget.Create(subtask.ID), task.Project.Title, task.Title, subtask.Title);

            return new SubtaskWrapper(this, subtask, task);
        }

        ///<summary>
        ///Copies a subtask with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Copy a subtask
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<returns>New subtask</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}/copy")]
        public SubtaskWrapper CopySubtask(int taskid, int subtaskid)
        {
            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            var subtaskEngine = EngineFactory.SubtaskEngine;
            var subtask = subtaskEngine.GetById(subtaskid).NotFoundIfNull();

            var team = EngineFactory.ProjectEngine.GetTeam(task.Project.ID);

            var newSubtask = subtaskEngine.Copy(subtask, task, team);

            return new SubtaskWrapper(this, newSubtask, task);
        }

        ///<summary>
        ///Moves a subtask with the ID specified in the request to another task.
        ///</summary>
        ///<short>
        ///Move a subtask
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<returns>Updated subtask</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}/move")]
        public SubtaskWrapper MoveSubtask(int taskid, int subtaskid)
        {
            var subtaskEngine = EngineFactory.SubtaskEngine;
            var subtask = subtaskEngine.GetById(subtaskid).NotFoundIfNull();

            var taskEngine = EngineFactory.TaskEngine;
            var fromTask = taskEngine.GetByID(subtask.Task).NotFoundIfNull();

            if (taskid == subtask.Task)
            {
                return new SubtaskWrapper(this, subtask, fromTask);
            }

            var toTask = taskEngine.GetByID(taskid).NotFoundIfNull();

            var toTaskTeam = EngineFactory.ProjectEngine.GetTeam(toTask.Project.ID);

            EngineFactory.SubtaskEngine.Move(subtask, fromTask, toTask, toTaskTeam);

            MessageService.Send(Request, MessageAction.SubtaskMoved, MessageTarget.Create(subtask.ID), toTask.Project.Title, toTask.Title, subtask.Title);

            return new SubtaskWrapper(this, subtask, toTask);
        }

        ///<summary>
        ///Updates the selected subtask with the title and responsible specified in the request.
        ///</summary>
        ///<short>
        ///Update a subtask
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<param name="responsible">New subtask responsible</param>
        ///<param name="title">New subtask title</param>
        ///<returns>Updated subtask</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}")]
        public SubtaskWrapper UpdateSubtask(int taskid, int subtaskid, Guid responsible, string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var subtask = task.SubTasks.Find(r => r.ID == subtaskid).NotFoundIfNull();

            var hasChanges = false;
            var acceptance = subtask.Responsible == Guid.Empty && responsible == CurrentUserId;

            subtask.Responsible = Update.IfNotEquals(subtask.Responsible, responsible, ref hasChanges);
            subtask.Title = Update.IfNotEmptyAndNotEquals(subtask.Title, title, ref hasChanges);

            if (hasChanges)
            {
                EngineFactory.SubtaskEngine.SaveOrUpdate(subtask, task);

                if (acceptance)
                {
                    var projectEngine = EngineFactory.ProjectEngine;
                    if (!projectEngine.IsInTeam(task.Project.ID, responsible))
                    {
                        projectEngine.AddToTeam(task.Project, responsible, false);
                    }
                }

                MessageService.Send(Request, MessageAction.SubtaskUpdated, MessageTarget.Create(subtask.ID), task.Project.Title, task.Title, subtask.Title);
            }

            return new SubtaskWrapper(this, subtask, task);
        }

        ///<summary>
        ///Deletes the selected subtask from the parent task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Delete a subtask
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<returns>Subtask</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}")]
        public SubtaskWrapper DeleteSubtask(int taskid, int subtaskid)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var subtask = task.SubTasks.Find(r => r.ID == subtaskid).NotFoundIfNull();

            EngineFactory.SubtaskEngine.Delete(subtask, task);
            MessageService.Send(Request, MessageAction.SubtaskDeleted, MessageTarget.Create(subtask.ID), task.Project.Title, task.Title, subtask.Title);

            return new SubtaskWrapper(this, subtask, task);
        }

        ///<summary>
        ///Updates the selected subtask status of the parent task with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Update a subtask status
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<param name="status">New subtask status: open|closed|disable|unclassified</param>
        ///<returns>Updated subtask</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}/status")]
        public SubtaskWrapper UpdateSubtask(int taskid, int subtaskid, TaskStatus status)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var subtask = task.SubTasks.Find(r => r.ID == subtaskid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(task, subtask);

            EngineFactory.SubtaskEngine.ChangeStatus(task, subtask, status);
            MessageService.Send(Request, MessageAction.SubtaskUpdatedStatus, MessageTarget.Create(subtask.ID), task.Project.Title, task.Title, subtask.Title, LocalizedEnumConverter.ConvertToString(subtask.Status));

            return new SubtaskWrapper(this, subtask, task);
        }

        #endregion
    }
}