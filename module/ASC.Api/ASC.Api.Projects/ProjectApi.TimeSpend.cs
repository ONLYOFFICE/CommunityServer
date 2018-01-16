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
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        ///<summary>
        ///Returns the list with the detailed information about all the time spent matching the filter parameters specified in the request
        ///</summary>
        ///<short>
        ///Get time spent by filter
        ///</short>
        ///<category>Time</category>
        ///<param name="projectid" optional="true"> Project Id</param>
        ///<param name="tag" optional="true">Project Tag</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="createdStart" optional="true">Minimum value of create time</param>
        ///<param name="createdStop" optional="true">Maximum value of create time</param>
        ///<param name="lastId">Last time spent ID</param>
        ///<param name="myProjects">Tasks time in My Projects</param>
        ///<param name="myMilestones">Tasks time in My Milestones</param>
        ///<param name="milestone" optional="true">Milestone ID</param>
        ///<param name="status" optional="true">Payment status</param>
        ///<returns>List of time spent</returns>
        ///<exception cref="ItemNotFoundException"></exception>
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

            return EngineFactory.TimeTrackingEngine.GetByFilter(filter).NotFoundIfNull().Select(r => new TimeWrapper(r));
        }

        ///<summary>
        ///Returns the total time spent matching the filter parameters specified in the request
        ///</summary>
        ///<short>
        ///Get total time spent by tilter
        ///</short>
        ///<category>Time</category>
        ///<param name="projectid" optional="true"> Project ID</param>
        ///<param name="tag" optional="true">Project tag</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="createdStart" optional="true">Minimum value of create time</param>
        ///<param name="createdStop" optional="true">Maximum value of create time</param>
        ///<param name="lastId">Last time spent ID</param>
        ///<param name="myProjects">Tasks time in My Projects</param>
        ///<param name="myMilestones">Tasks time in My Milestones</param>
        ///<param name="milestone" optional="true">Milestone ID</param>
        ///<param name="status" optional="true">Payment status</param>
        ///<returns>Total time spent</returns>
        ///<exception cref="ItemNotFoundException"></exception>
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

        ///<summary>
        ///Returns the time spent on the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Get time spent
        ///</short>
        ///<category>Time</category>
        ///<param name="taskid">Task ID</param>
        ///<returns></returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:[0-9]+}/time")]
        public IEnumerable<TimeWrapper> GetTaskTime(int taskid)
        {
            if (!EngineFactory.TaskEngine.IsExists(taskid)) throw new ItemNotFoundException();
            var times = EngineFactory.TimeTrackingEngine.GetByTask(taskid).NotFoundIfNull();
            _context.SetTotalCount(times.Count);
            return times.Select(x => new TimeWrapper(x));
        }

        ///<summary>
        ///Adds the time to the selected task with the time parameters specified in the request
        ///</summary>
        ///<short>
        ///Add task time
        ///</short>
        ///<category>Time</category>
        ///<param name="taskid">Task ID</param>
        ///<param name="note">Note</param>
        ///<param name="date">Date</param>
        ///<param name="personId">Person that spends time</param>
        ///<param name="hours">Hours spent</param>
        ///<param name="projectId">Project ID</param>
        ///<returns>Created time</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
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
                    Task = task
                };

            ts = EngineFactory.TimeTrackingEngine.SaveOrUpdate(ts);
            MessageService.Send(Request, MessageAction.TaskTimeCreated, MessageTarget.Create(ts.ID), task.Project.Title, task.Title, ts.Note);

            return new TimeWrapper(ts);
        }

        ///<summary>
        ///Updates the time for the selected task with the time parameters specified in the request
        ///</summary>
        ///<short>
        ///Update task time
        ///</short>
        ///<category>Time</category>
        ///<param name="timeid">ID of time spent</param>
        ///<param name="note">Note</param>
        ///<param name="date">Date</param>
        ///<param name="personId">Person that spends time</param>
        ///<param name="hours">Hours spent</param>
        ///<returns>Created time</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
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

            return new TimeWrapper(time);
        }

        ///<summary>
        ///Updates the time status of payment
        ///</summary>
        ///<short>
        ///Updates the time status of payment
        ///</short>
        ///<category>Time</category>
        ///<param name="timeids">List IDs of time spent</param>
        ///<param name="status">Status</param>
        ///<returns>Created time</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"time/times/status")]
        public List<TimeWrapper> UpdateTimes(int[] timeids, PaymentStatus status)
        {
            var timeTrackingEngine = EngineFactory.TimeTrackingEngine;
            var times = new List<TimeWrapper>();

            foreach (var timeid in timeids)
            {
                var time = timeTrackingEngine.GetByID(timeid).NotFoundIfNull();
                timeTrackingEngine.ChangePaymentStatus(time, status);
                times.Add(new TimeWrapper(time));
            }

            MessageService.Send(Request, MessageAction.TaskTimesUpdatedStatus, MessageTarget.Create(timeids), times.Select(t => t.Note), LocalizedEnumConverter.ConvertToString(status));

            return times;
        }

        ///<summary>
        ///Deletes the times from the tasks with the ID specified in the request
        ///</summary>
        ///<short>
        ///Delete time spents
        ///</short>
        ///<category>Time</category>
        ///<param name="timeids">IDs of time spents</param>
        ///<returns></returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"time/times/remove")]
        public List<TimeWrapper> DeleteTaskTimes(int[] timeids)
        {
            var listDeletedTimers = new List<TimeWrapper>();
            foreach (var timeid in timeids.Distinct())
            {
                var timeTrackingEngine = EngineFactory.TimeTrackingEngine;
                var time = timeTrackingEngine.GetByID(timeid).NotFoundIfNull();

                timeTrackingEngine.Delete(time);
                listDeletedTimers.Add(new TimeWrapper(time));
            }

            MessageService.Send(Request, MessageAction.TaskTimesDeleted, MessageTarget.Create(timeids), listDeletedTimers.Select(t => t.Note));

            return listDeletedTimers;
        }
    }
}