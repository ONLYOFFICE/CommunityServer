/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        /// <summary>
        /// Returns a list with the detailed information about all the task time spent matching the filter parameters specified in the request.
        /// </summary>
        /// <short>
        /// Get filtered task time
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32, System" method="url" name="projectid" optional="true"> Project ID</param>
        /// <param type="System.Boolean, System" method="url" name="myProjects">Specifies whether to return task time only for my projects or not</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" name="milestone" optional="true">Milestone ID</param>
        /// <param type="System.Boolean, System" method="url" name="myMilestones">Specifies whether to return task time only for my milestones or not</param>
        /// <param type="System.Int32, System" method="url" name="tag" optional="true">Project tag</param>
        /// <param type="System.Guid, System" method="url" name="departament" optional="true">Departament GUID</param>
        /// <param type="System.Guid, System" method="url" name="participant" optional="true">Participant GUID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="createdStart" optional="true">The earliest date of task creation</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="createdStop" optional="true">The latest date of task creation</param>
        /// <param type="System.Int32, System" method="url" name="lastId">Last spent time ID</param>
        /// <param type="System.Nullable{ASC.Projects.Core.Domain.PaymentStatus}, System" method="url" name="status" optional="true">Payment status ("NotChargeable", "NotBilled", or "Billed")</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TimeWrapper, ASC.Api.Projects">List of spent time</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/time/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"time/filter")]
        public IEnumerable<TimeWrapper> GetTaskTimeByFilter(
            int projectid,
            bool myProjects,
            int? milestone,
            bool myMilestones,
            int tag,
            Guid departament,
            Guid participant,
            ApiDateTime createdStart,
            ApiDateTime createdStop,
            int lastId,
            PaymentStatus? status)
        {
            var filter = CreateFilter(EntityType.TimeSpend);
            filter.DepartmentId = departament;
            filter.UserId = participant;
            filter.FromDate = createdStart;
            filter.ToDate = createdStop;
            filter.TagId = tag;
            filter.LastId = lastId;
            filter.MyProjects = myProjects;
            filter.MyMilestones = myMilestones;
            filter.Milestone = milestone;

            if (projectid != 0)
                filter.ProjectIds.Add(projectid);

            if (status.HasValue)
                filter.PaymentStatuses.Add(status.Value);

            SetTotalCount(EngineFactory.TimeTrackingEngine.GetByFilterCount(filter));

            return EngineFactory.TimeTrackingEngine.GetByFilter(filter).NotFoundIfNull().Select(TimeWrapperSelector);
        }

        /// <summary>
        /// Returns the total time spent matching the filter parameters specified in the request.
        /// </summary>
        /// <short>
        /// Get filtered total task time
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32, System" method="url" name="projectid" optional="true"> Project ID</param>
        /// <param type="System.Boolean, System" method="url" name="myProjects">Specifies whether to return task time only for my projects or not</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" name="milestone" optional="true">Milestone ID</param>
        /// <param type="System.Boolean, System" method="url" name="myMilestones">Specifies whether to return task time only for my milestones or not</param>
        /// <param type="System.Int32, System" method="url" name="tag" optional="true">Project tag</param>
        /// <param type="System.Guid, System" method="url" name="departament" optional="true">Departament GUID</param>
        /// <param type="System.Guid, System" method="url" name="participant" optional="true">Participant GUID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="createdStart" optional="true">The earliest date of task creation</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="createdStop" optional="true">The latest date of task creation</param>
        /// <param type="System.Int32, System" method="url" name="lastId">Last spent time ID</param>
        /// <param type="System.Nullable{ASC.Projects.Core.Domain.PaymentStatus}, System" method="url" name="status" optional="true">Payment status ("NotChargeable", "NotBilled", or "Billed")</param>
        /// <returns>Total spent time</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/time/filter/total</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"time/filter/total")]
        public float GetTotalTaskTimeByFilter(
            int projectid,
            bool myProjects,
            int? milestone,
            bool myMilestones,
            int tag,
            Guid departament,
            Guid participant,
            ApiDateTime createdStart,
            ApiDateTime createdStop,
            int lastId,
            PaymentStatus? status)
        {
            var filter = CreateFilter(EntityType.TimeSpend);
            filter.DepartmentId = departament;
            filter.UserId = participant;
            filter.FromDate = createdStart;
            filter.ToDate = createdStop;
            filter.TagId = tag;
            filter.LastId = lastId;
            filter.MyProjects = myProjects;
            filter.MyMilestones = myMilestones;
            filter.Milestone = milestone;

            if (projectid != 0)
            {
                filter.ProjectIds.Add(projectid);
            }

            if (status.HasValue)
            {
                filter.PaymentStatuses.Add(status.Value);
            }

            return EngineFactory.TimeTrackingEngine.GetByFilterTotal(filter);
        }

        /// <summary>
        /// Returns the time spent on the task with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get task time
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TimeWrapper, ASC.Api.Projects">Task time</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/task/{taskid}/time</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"task/{taskid:[0-9]+}/time")]
        public IEnumerable<TimeWrapper> GetTaskTime(int taskid)
        {
            if (!EngineFactory.TaskEngine.IsExists(taskid)) throw new ItemNotFoundException();
            var times = EngineFactory.TimeTrackingEngine.GetByTask(taskid).NotFoundIfNull();
            Context.SetTotalCount(times.Count);
            return times.Select(TimeWrapperSelector);
        }

        /// <summary>
        /// Adds the time to the selected task with the time parameters specified in the request.
        /// </summary>
        /// <short>
        /// Add task time
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <param type="System.String, System" name="note">Time note</param>
        /// <param type="System.DateTime, System" name="date">Date</param>
        /// <param type="System.Guid, System" name="personId">Person ID</param>
        /// <param type="System.Single, System" name="hours">Spent hours</param>
        /// <param type="System.Int32, System" name="projectId">Project ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TimeWrapper, ASC.Api.Projects">Created time</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/task/{taskid}/time</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"task/{taskid:[0-9]+}/time")]
        public TimeWrapper AddTaskTime(int taskid, string note, DateTime date, Guid personId, float hours, int projectId)
        {
            if (date == DateTime.MinValue) throw new ArgumentException("date can't be empty");
            if (personId == Guid.Empty) throw new ArgumentException("person can't be empty");

            var task = EngineFactory.TaskEngine.GetByID(taskid);

            if (task == null) throw new ItemNotFoundException();

            if (!EngineFactory.ProjectEngine.IsExists(projectId)) throw new ItemNotFoundException("project");

            var ts = new TimeSpend
            {
                Date = date.Date,
                Person = personId,
                Hours = hours,
                Note = note,
                Task = task,
                CreateBy = SecurityContext.CurrentAccount.ID
            };

            ts = EngineFactory.TimeTrackingEngine.SaveOrUpdate(ts);
            MessageService.Send(Request, MessageAction.TaskTimeCreated, MessageTarget.Create(ts.ID), task.Project.Title, task.Title, ts.Note);

            return TimeWrapperSelector(ts);
        }

        /// <summary>
        /// Updates the time for the selected task with the time parameters specified in the request.
        /// </summary>
        /// <short>
        /// Update task time
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32, System" method="url" name="timeid">Time ID</param>
        /// <param type="System.String, System" name="note">New time note</param>
        /// <param type="System.DateTime, System" name="date">New date</param>
        /// <param type="System.Guid, System" name="personId">New person ID</param>
        /// <param type="System.Single, System" name="hours">New spent hours</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TimeWrapper, ASC.Api.Projects">Updated time</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/time/{timeid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"time/{timeid:[0-9]+}")]
        public TimeWrapper UpdateTime(int timeid, string note, DateTime date, Guid personId, float hours)
        {
            if (date == DateTime.MinValue) throw new ArgumentException("date can't be empty");
            if (personId == Guid.Empty) throw new ArgumentException("person can't be empty");

            var timeTrackingEngine = EngineFactory.TimeTrackingEngine;

            var time = timeTrackingEngine.GetByID(timeid).NotFoundIfNull();

            time.Date = date.Date;
            time.Person = personId;
            time.Hours = hours;
            time.Note = note;

            timeTrackingEngine.SaveOrUpdate(time);
            MessageService.Send(Request, MessageAction.TaskTimeUpdated, MessageTarget.Create(time.ID), time.Task.Project.Title, time.Task.Title, time.Note);

            return TimeWrapperSelector(time);
        }

        /// <summary>
        /// Updates the time payment status with the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Update the time payment status
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32[], System" name="timeids">Spent time IDs</param>
        /// <param type="ASC.Projects.Core.Domain.PaymentStatus, ASC.Projects.Core.Domain" name="status">New payment status ("NotChargeable", "NotBilled", or "Billed")</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TimeWrapper, ASC.Api.Projects">Updated time</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/time/times/status</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"time/times/status")]
        public List<TimeWrapper> UpdateTimes(int[] timeids, PaymentStatus status)
        {
            var timeTrackingEngine = EngineFactory.TimeTrackingEngine;
            var times = new List<TimeWrapper>();

            foreach (var timeid in timeids)
            {
                var time = timeTrackingEngine.GetByID(timeid).NotFoundIfNull();
                timeTrackingEngine.ChangePaymentStatus(time, status);
                times.Add(TimeWrapperSelector(time));
            }

            MessageService.Send(Request, MessageAction.TaskTimesUpdatedStatus, MessageTarget.Create(timeids), times.Select(t => t.RelatedTaskTitle), LocalizedEnumConverter.ConvertToString(status));

            return times;
        }

        /// <summary>
        /// Deletes the time from the tasks with the IDs specified in the request.
        /// </summary>
        /// <short>
        /// Delete task time
        /// </short>
        /// <category>Time</category>
        /// <param type="System.Int32[], System" name="timeids">Spent time IDs</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TimeWrapper, ASC.Api.Projects">Deleted time</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/time/times/remove</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
        [Delete(@"time/times/remove")]
        public List<TimeWrapper> DeleteTaskTimes(int[] timeids)
        {
            var listDeletedTimers = new List<TimeWrapper>();
            foreach (var timeid in timeids.Distinct())
            {
                var timeTrackingEngine = EngineFactory.TimeTrackingEngine;
                var time = timeTrackingEngine.GetByID(timeid).NotFoundIfNull();

                timeTrackingEngine.Delete(time);
                listDeletedTimers.Add(TimeWrapperSelector(time));
            }

            MessageService.Send(Request, MessageAction.TaskTimesDeleted, MessageTarget.Create(timeids), listDeletedTimers.Select(t => t.RelatedTaskTitle));

            return listDeletedTimers;
        }
    }
}