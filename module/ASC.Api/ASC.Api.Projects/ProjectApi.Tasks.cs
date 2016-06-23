/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Documents;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region tasks

        ///<summary>
        ///Returns the list with the detailed information about all tasks for the current user
        ///</summary>
        ///<short>
        ///My tasks
        ///</short>
        /// <category>Tasks</category>
        ///<returns>List of tasks</returns>
        [Read(@"task/@self")]
        public IEnumerable<TaskWrapper> GetMyTasks()
        {
            return EngineFactory
                .TaskEngine.GetByResponsible(CurrentUserId)
                .Select(x => new TaskWrapper(x));
        }

        ///<summary>
        ///Returns the list with the detailed information about the tasks for the current user with the status specified in the request
        ///</summary>
        ///<short>
        ///My tasks by status
        ///</short>
        /// <category>Tasks</category>
        ///<param name="status">Status of task. One of notaccept|open|closed|disable|unclassified|notinmilestone</param>
        ///<returns>List of tasks</returns>
        [Read(@"task/@self/{status:(notaccept|open|closed|disable|unclassified|notinmilestone)}")]
        public IEnumerable<TaskWrapper> GetMyTasks(TaskStatus status)
        {
            return EngineFactory
                .TaskEngine.GetByResponsible(CurrentUserId, status)
                .Select(x => new TaskWrapper(x));
        }

        ///<summary>
        /// Returns the detailed information about the task with the ID specified in the request
        ///</summary>
        ///<short>
        /// Get task
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}")]
        public TaskWrapper GetTask(int taskid)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            if (task.Milestone == 0) return new TaskWrapper(task);

            var milestone = EngineFactory.MilestoneEngine.GetByID(task.Milestone, false);
            return new TaskWrapper(task, milestone);
        }

        public TaskWrapper GetTask(Task task)
        {
            if (task.Milestone == 0) return new TaskWrapper(task);

            var milestone = EngineFactory.MilestoneEngine.GetByID(task.Milestone, false);
            return new TaskWrapper(task, milestone);
        }

        ///<visible>false</visible>
        [Read(@"task")]
        public IEnumerable<TaskWrapper> GetTask(IEnumerable<int> taskid)
        {
            var tasks = EngineFactory.TaskEngine.GetByID(taskid.ToList()).NotFoundIfNull();
            return tasks.Select(r => new TaskWrapper(r));
        }

        ///<summary>
        ///Returns the list with the detailed information about all the tasks matching the filter parameters specified in the request
        ///</summary>
        ///<short>
        /// Get task by filter
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid" optional="true"> Project Id</param>
        ///<param name="tag" optional="true">Project Tag</param>
        ///<param name="status" optional="true">Task Status</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="creator">Creator GUID</param>
        ///<param name="milestone" optional="true">Milestone ID</param>
        ///<param name="deadlineStart" optional="true">Minimum value of task deadline</param>
        ///<param name="deadlineStop" optional="true">Maximum value of task deadline</param>
        ///<param name="lastId">Last task ID</param>
        ///<param name="myProjects">Tasks in My Projects</param>
        ///<param name="myMilestones">Tasks in My Milestones</param>
        ///<param name="follow">Followed tasks</param>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/filter")]
        public IEnumerable<TaskWrapper> GetTaskByFilter(int projectid, bool myProjects, int? milestone,
            bool myMilestones, int tag,
            TaskStatus? status, bool follow, Guid departament, Guid? participant, Guid creator,
            ApiDateTime deadlineStart, ApiDateTime deadlineStop, int lastId)
        {
            var filter = CreateFilter();
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

            return filterResult.FilterResult.Select(r => new TaskWrapper(r));
        }

        ///<summary>
        ///Returns the list with the detailed information about all the tasks matching the filter parameters specified in the request
        ///</summary>
        ///<short>
        /// Get task by filter
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid" optional="true"> Project Id</param>
        ///<param name="tag" optional="true">Project Tag</param>
        ///<param name="status" optional="true">Task Status</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="creator">Creator GUID</param>
        ///<param name="milestone" optional="true">Milestone ID</param>
        ///<param name="deadlineStart" optional="true">Minimum value of task deadline</param>
        ///<param name="deadlineStop" optional="true">Maximum value of task deadline</param>
        ///<param name="lastId">Last task ID</param>
        ///<param name="myProjects">Tasks in My Projects</param>
        ///<param name="myMilestones">Tasks in My Milestones</param>
        ///<param name="follow">Followed tasks</param>
        ///<visible>false</visible>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/filter/simple")]
        public IEnumerable<SimpleTaskWrapper> GetSimpleTaskByFilter(int projectid, bool myProjects, int? milestone,
            bool myMilestones, int tag,
            TaskStatus? status, bool follow, Guid departament, Guid? participant, Guid creator,
            ApiDateTime deadlineStart, ApiDateTime deadlineStop, int lastId)
        {
            var filter = CreateFilter();
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

            return filterResult.FilterResult.Select(r => new SimpleTaskWrapper(r));
        }

        ///<summary>
        /// Returns the list of all files attached to the task with the ID specified in the request
        ///</summary>
        ///<short>
        /// Get task files
        ///</short>
        /// <category>Files</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>List of files</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/files")]
        public IEnumerable<FileWrapper> GetTaskFiles(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(task.Project);

            return taskEngine.GetFiles(task).Select(x => new FileWrapper(x));
        }

        ///<summary>
        /// Uploads the file specified in the request to the selected task
        ///</summary>
        ///<short>
        /// Upload file to task
        ///</short>
        /// <category>Files</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="files">File ID</param>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{taskid:[0-9]+}/files")]
        public TaskWrapper UploadFilesToTask(int taskid, IEnumerable<int> files)
        {
            var taskEngine = EngineFactory.TaskEngine;
            var fileEngine = EngineFactory.FileEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(task.Project);

            var filesList = files.ToList();
            var fileNames = new List<string>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid).NotFoundIfNull();
                fileNames.Add(file.Title);
                taskEngine.AttachFile(task, file.ID, true);
            }

            MessageService.Send(Request, MessageAction.TaskAttachedFiles, task.Project.Title, task.Title, fileNames);

            return new TaskWrapper(task);
        }

        ///<summary>
        /// Detaches the selected file from the task with the ID specified in the request
        ///</summary>
        ///<short>
        /// Detach file from task
        ///</short>
        /// <category>Files</category>
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
            MessageService.Send(Request, MessageAction.TaskDetachedFile, task.Project.Title, task.Title, file.Title);

            return new TaskWrapper(task);
        }

        ///<summary>
        /// Updates the status of the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Update task status
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="status">Status of task. Can be one of: open|closed</param>
        ///<returns>Updated task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/status")]
        public TaskWrapper UpdateTask(int taskid, TaskStatus status)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();

            EngineFactory.TaskEngine.ChangeStatus(task, status);
            MessageService.Send(Request, MessageAction.TaskUpdatedStatus, task.Project.Title, task.Title, LocalizedEnumConverter.ConvertToString(task.Status));

            return GetTask(task);
        }

        ///<summary>
        /// Updates the milestone of the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Update task milestone
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="milestoneid">Milestone ID</param>
        ///<returns>Updated task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/milestone")]
        public TaskWrapper UpdateTask(int taskid, int milestoneid)
        {
            if (milestoneid < 0) throw new ArgumentNullException("milestoneid");

            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            var milestoneEngine = EngineFactory.MilestoneEngine;
            var milestone = milestoneEngine.GetByID(milestoneid);

            taskEngine.MoveToMilestone(task, milestoneid);
            if (milestone != null)
            {
                MessageService.Send(Request, MessageAction.TaskMovedToMilestone, task.Project.Title, milestone.Title, task.Title);
            }
            else
            {
                MessageService.Send(Request, MessageAction.TaskUnlinkedMilestone, task.Project.Title, task.Title);
            }

            return GetTask(task);
        }

        ///<summary>
        ///Updates the selected task with the parameters (responsible user ID, task description, deadline time, etc) specified in the request
        ///</summary>
        ///<short>
        ///Update Task
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">task ID</param>
        ///<param name="description">description</param>
        ///<param name="deadline">deadline time</param>
        ///<param name="startDate">task start date</param>
        ///<param name="priority">priority</param>
        ///<param name="title">title</param>
        ///<param name="milestoneid">milestone ID</param>
        ///<param name="responsibles">list responsibles</param>
        ///<param name="projectID">Project ID</param>
        ///<param name="notify">notify responsible</param>
        ///<param name="status" optional="true">status</param>
        ///<param name="progress" optional="true">Progress</param>
        ///<returns>Updated task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}")]
        public TaskWrapper UpdateProjectTask(
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
            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            if (!EngineFactory.MilestoneEngine.IsExists(milestoneid) && milestoneid > 0)
            {
                throw new ItemNotFoundException("Milestone not found");
            }

            task.Responsibles = new List<Guid>(responsibles.Distinct());

            task.Deadline = Update.IfNotEquals(task.Deadline, deadline);
            task.Description = Update.IfNotEquals(task.Description, description);

            if (priority.HasValue)
            {
                task.Priority = Update.IfNotEquals(task.Priority, priority.Value);
            }

            task.Title = Update.IfNotEmptyAndNotEquals(task.Title, title);
            task.Milestone = Update.IfNotEquals(task.Milestone, milestoneid);
            task.StartDate = Update.IfNotEquals(task.StartDate, startDate);

            if (projectID.HasValue)
            {
                var project = EngineFactory.ProjectEngine.GetByID((int)projectID).NotFoundIfNull();
                task.Project = project;
            }

            if (progress.HasValue)
            {
                task.Progress = progress.Value;
            }

            taskEngine.SaveOrUpdate(task, null, notify);

            if (status.HasValue)
            {
                taskEngine.ChangeStatus(task, status.Value);
            }

            MessageService.Send(Request, MessageAction.TaskUpdated, task.Project.Title, task.Title);

            return GetTask(task);
        }

        ///<summary>
        ///Deletes the task with the ID specified in the request from the project
        ///</summary>
        ///<short>
        ///Delete task
        ///</short>
        /// <category>Projects</category>
        ///<param name="taskid">task ID</param>
        ///<returns>Deleted task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}")]
        public TaskWrapper DeleteTask(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            taskEngine.Delete(task);
            MessageService.Send(Request, MessageAction.TaskDeleted, task.Project.Title, task.Title);

            return new TaskWrapper(task);
        }

        ///<summary>
        ///Returns the list of the comments for the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Task comments
        ///</short>
        /// <category>Comments</category>
        ///<param name="taskid">Task ID</param>
        ///<returns>List of comments</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/comment")]
        public IEnumerable<CommentWrapper> GetTaskComments(int taskid)
        {
            return EngineFactory.CommentEngine.GetComments(EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull()).Select(x => new CommentWrapper(x));
        }


        ///<summary>
        ///Adds the comments for the selected task with the comment text and parent comment ID specified in the request
        ///</summary>
        ///<short>
        ///Add task comment
        ///</short>
        /// <category>Comments</category>
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

            MessageService.Send(Request, MessageAction.TaskCommentCreated, task.Project.Title, task.Title);

            return new CommentWrapper(comment);
        }

        ///<summary>
        ///Notify the responsible for the task with the ID specified in the request about the task
        ///</summary>
        ///<short>
        ///Notify task responsible
        ///</short>
        /// <category>Tasks</category>
        /// <returns>Task</returns>
        ///<param name="taskid">Task ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/notify")]
        public TaskWrapper NotifyTaskResponsible(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            taskEngine.SendReminder(task);

            return new TaskWrapper(task);
        }

        ///<summary>
        ///Subscribe to notifications about the actions performed with the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Subscribe to task action
        ///</short>
        /// <category>Tasks</category>
        /// <returns>Task</returns>
        ///<param name="taskid">Task ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/subscribe")]
        public TaskWrapper SubscribeToTask(int taskid)
        {
            var taskEngine = EngineFactory.TaskEngine;
            var task = taskEngine.GetByID(taskid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            taskEngine.Follow(task);
            MessageService.Send(Request, MessageAction.TaskUpdatedFollowing, task.Project.Title, task.Title);

            return new TaskWrapper(task);
        }

        ///<summary>
        ///Checks subscription to notifications about the actions performed with the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Check subscription to task action
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">Task ID</param>
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
        ///Add link between dependenceTaskId and parentTaskId
        ///</summary>
        ///<short>
        ///Add link 
        ///</short>
        /// <category>Tasks</category>
        ///<param name="parentTaskId">Parent Task ID</param>
        ///<param name="dependenceTaskId">Dependent Task ID</param>
        ///<param name="linkType">Link Type</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"task/{parentTaskId:[0-9]+}/link")]
        public TaskWrapper AddLink(int parentTaskId, int dependenceTaskId, TaskLinkType linkType)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var dependentTask = taskEngine.GetByID(dependenceTaskId).NotFoundIfNull();
            var parentTask = taskEngine.GetByID(parentTaskId).NotFoundIfNull();

            taskEngine.AddLink(parentTask, dependentTask, linkType);
            MessageService.Send(Request, MessageAction.TasksLinked, parentTask.Project.Title, parentTask.Title, dependentTask.Title);

            return new TaskWrapper(dependentTask);
        }

        ///<summary>
        ///Remove link between dependenceTaskId and parentTaskId
        ///</summary>
        ///<short>
        ///Remove link 
        ///</short>
        /// <category>Tasks</category>
        ///<param name="dependenceTaskId">Dependent Task ID</param>
        ///<param name="parentTaskId">Parent Task ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}/link")]
        public TaskWrapper RemoveLink(int dependenceTaskId, int parentTaskId)
        {
            var taskEngine = EngineFactory.TaskEngine;

            var dependentTask = taskEngine.GetByID(dependenceTaskId).NotFoundIfNull();
            var parentTask = taskEngine.GetByID(parentTaskId).NotFoundIfNull();

            taskEngine.RemoveLink(dependentTask, parentTask);
            MessageService.Send(Request, MessageAction.TasksUnlinked, parentTask.Project.Title, parentTask.Title, dependentTask.Title);

            return new TaskWrapper(dependentTask);
        }

        #endregion

        #region subtasks

        ///<summary>
        /// Creates the subtask with the selected title and responsible within the parent task specified in the request
        ///</summary>
        ///<short>
        /// Create subtask
        ///</short>
        /// <category>Tasks</category>
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
            MessageService.Send(Request, MessageAction.SubtaskCreated, task.Project.Title, task.Title, subtask.Title);

            return new SubtaskWrapper(subtask, task);
        }

        ///<summary>
        /// Updates the subtask with the selected title and responsible with the subtask ID specified in the request
        ///</summary>
        ///<short>
        /// Update subtask
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<param name="responsible">Subtask responsible</param>
        ///<param name="title">Subtask title</param>
        ///<returns>Task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}")]
        public SubtaskWrapper UpdateSubtask(int taskid, int subtaskid, Guid responsible, string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var subtask = task.SubTasks.Find(r => r.ID == subtaskid).NotFoundIfNull();

            subtask.Responsible = responsible;
            subtask.Title = Update.IfNotEmptyAndNotEquals(subtask.Title, title);

            EngineFactory.SubtaskEngine.SaveOrUpdate(subtask, task);
            MessageService.Send(Request, MessageAction.SubtaskUpdated, task.Project.Title, task.Title, subtask.Title);

            return new SubtaskWrapper(subtask, task);
        }

        ///<summary>
        /// Deletes the selected subtask from the parent task with the ID specified in the request
        ///</summary>
        ///<short>
        /// Delete subtask
        ///</short>
        ///<category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}")]
        public SubtaskWrapper DeleteSubtask(int taskid, int subtaskid)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var subtask = task.SubTasks.Find(r => r.ID == subtaskid).NotFoundIfNull();

            EngineFactory.SubtaskEngine.Delete(subtask, task);
            MessageService.Send(Request, MessageAction.SubtaskDeleted, task.Project.Title, task.Title, subtask.Title);

            return new SubtaskWrapper(subtask, task);
        }

        ///<summary>
        /// Updates the selected subtask status in the parent task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Update subtask status
        ///</short>
        /// <category>Tasks</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="subtaskid">Subtask ID</param>
        ///<param name="status">Status of task. Can be one of: open|closed|disable|unclassified</param>
        ///<returns>Updated task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"task/{taskid:[0-9]+}/{subtaskid:[0-9]+}/status")]
        public SubtaskWrapper UpdateSubtask(int taskid, int subtaskid, TaskStatus status)
        {
            var task = EngineFactory.TaskEngine.GetByID(taskid).NotFoundIfNull();
            var subtask = task.SubTasks.Find(r => r.ID == subtaskid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(task, subtask);

            EngineFactory.SubtaskEngine.ChangeStatus(task, subtask, status);
            MessageService.Send(Request, MessageAction.SubtaskUpdatedStatus, task.Project.Title, task.Title, subtask.Title, LocalizedEnumConverter.ConvertToString(subtask.Status));

            return new SubtaskWrapper(subtask, task);
        }

        #endregion
    }
}