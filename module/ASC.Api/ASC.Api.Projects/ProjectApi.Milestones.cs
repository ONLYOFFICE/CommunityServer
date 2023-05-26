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
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region milestone

        /// <summary>
        /// Returns a list of the recent milestones within all the portal projects.
        /// </summary>
        /// <short>
        /// Get recent milestones
        /// </short>
        /// <category>Milestones</category>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">List of milestones</returns>
        /// <path>api/2.0/project/milestone</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone")]
        public IEnumerable<MilestoneWrapper> GetRecentMilestones()
        {
            return EngineFactory.MilestoneEngine.GetRecentMilestones((int)Count).Select(MilestoneWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns a list of all the upcoming milestones within all the portal projects.
        /// </summary>
        /// <short>
        /// Get upcoming milestones
        /// </short>
        /// <category>Milestones</category>
        /// <returns>List of milestones</returns>
        /// <path>api/2.0/project/milestone/upcoming</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone/upcoming")]
        public IEnumerable<MilestoneWrapper> GetUpcomingMilestones()
        {
            return EngineFactory.MilestoneEngine.GetUpcomingMilestones((int)Count).Select(MilestoneWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns a list of all the milestones matching the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Get filtered milestones
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="projectid" optional="true">Project ID</param>
        /// <param type="System.Int32, System" method="url" name="tag" optional="true">Milestone tag</param>
        /// <param type="System.Nullable{ASC.Projects.Core.Domain.MilestoneStatus}, System" method="url" name="status" optional="true">Milestone status ("Open" or "Closed")</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="deadlineStart" optional="true">Minimum value of milestone deadline</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="deadlineStop" optional="true">Maximum value of milestone deadline</param>
        /// <param type="System.Nullable{System.Guid}, System" method="url" name="taskResponsible" optional="true">Milestone responsible GUID</param>
        /// <param type="System.Int32, System" method="url" name="lastId">Last milestone ID</param>
        /// <param type="System.Boolean, System" method="url" name="myProjects">Specifies whether to return milestones only from my projects or not</param>
        /// <param type="System.Guid, System" method="url" name="milestoneResponsible">Milestone responsible GUID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">List of milestones</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/milestone/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone/filter")]
        public IEnumerable<MilestoneWrapper> GetMilestonesByFilter(
            int projectid,
            int tag,
            MilestoneStatus? status,
            ApiDateTime deadlineStart,
            ApiDateTime deadlineStop, Guid? taskResponsible,
            int lastId,
            bool myProjects,
            Guid milestoneResponsible)
        {
            var milestoneEngine = EngineFactory.MilestoneEngine;

            var filter = CreateFilter(EntityType.Milestone);
            filter.UserId = milestoneResponsible;
            filter.ParticipantId = taskResponsible;
            filter.TagId = tag;
            filter.FromDate = deadlineStart;
            filter.ToDate = deadlineStop;
            filter.LastId = lastId;
            filter.MyProjects = myProjects;

            if (projectid != 0)
            {
                filter.ProjectIds.Add(projectid);
            }

            if (status != null)
            {
                filter.MilestoneStatuses.Add((MilestoneStatus)status);
            }

            SetTotalCount(milestoneEngine.GetByFilterCount(filter));

            return milestoneEngine.GetByFilter(filter).NotFoundIfNull().Select(MilestoneWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns a list of all the overdue milestones in the portal projects.
        /// </summary>
        /// <short>
        /// Get overdue milestones
        /// </short>
        /// <category>Milestones</category>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">List of milestones</returns>
        /// <path>api/2.0/project/milestone/late</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone/late")]
        public IEnumerable<MilestoneWrapper> GetLateMilestones()
        {
            return EngineFactory.MilestoneEngine.GetLateMilestones((int)Count).Select(MilestoneWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns a list of all the milestones with the deadline specified in the request.
        /// </summary>
        /// <short>
        /// Get milestones by deadline
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="year">Deadline year</param>
        /// <param type="System.Int32, System" method="url" name="month">Deadline month</param>
        /// <param type="System.Int32, System" method="url" name="day">Deadline day</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">List of milestones</returns>
        /// <path>api/2.0/project/milestone/{year}/{month}/{day}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone/{year}/{month}/{day}")]
        public IEnumerable<MilestoneWrapper> GetMilestonesByDeadLineFull(int year, int month, int day)
        {
            var milestones = EngineFactory.MilestoneEngine.GetByDeadLine(new DateTime(year, month, day));
            return milestones.Select(MilestoneWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns a list of all the milestones with the deadline month specified in the request.
        /// </summary>
        /// <short>
        /// Get milestones by deadline month
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="year">Deadline year</param>
        /// <param type="System.Int32, System" method="url" name="month">Deadline month</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">List of milestones</returns>
        /// <path>api/2.0/project/milestone/{year}/{month}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone/{year}/{month}")]
        public IEnumerable<MilestoneWrapper> GetMilestonesByDeadLineMonth(int year, int month)
        {
            var milestones = EngineFactory.MilestoneEngine.GetByDeadLine(new DateTime(year, month, DateTime.DaysInMonth(year, month)));
            return milestones.Select(MilestoneWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns the detailed information about a milestone with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a milestone
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="id">Milestone ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">Milestone</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/milestone/{id}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"milestone/{id:[0-9]+}")]
        public MilestoneWrapper GetMilestoneById(int id)
        {
            var milestoneEngine = EngineFactory.MilestoneEngine;
            if (!milestoneEngine.IsExists(id)) throw new ItemNotFoundException();
            return MilestoneWrapperSelector(milestoneEngine.GetByID(id));
        }

        /// <summary>
        /// Returns a list of all the tasks from a milestone with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get milestone tasks 
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="id">Milestone ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.TaskWrapper, ASC.Api.Projects">List of tasks</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/milestone/{id}/task</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"milestone/{id:[0-9]+}/task")]
        public IEnumerable<TaskWrapper> GetMilestoneTasks(int id)
        {
            if (!EngineFactory.MilestoneEngine.IsExists(id)) throw new ItemNotFoundException();
            return EngineFactory.TaskEngine.GetMilestoneTasks(id).Select(TaskWrapperSelector).ToList();
        }

        /// <summary>
        /// Updates the selected milestone changing the milestone parameters (title, deadline, status, etc.) specified in the request.
        /// </summary>
        /// <short>
        /// Update a milestone
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="id">Milestone ID</param>
        /// <param type="System.String, System" name="title">New milestone title</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="deadline">New milestone deadline</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="isKey">Specifies if this is a key milestone or not</param>
        /// <param type="ASC.Projects.Core.Domain.MilestoneStatus, ASC.Projects.Core.Domain" name="status">New milestone status ("Open" or "Closed")</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="isNotify">Specifies whether to remind me 48 hours before the milestone due date or not</param>
        /// <param type="System.String, System" name="description">New milestone description</param>
        /// <param type="System.Int32, System" name="projectID">New project ID</param>
        /// <param type="System.Guid, System" name="responsible">New milestone responsible</param>
        /// <param type="System.Boolean, System" name="notifyResponsible">Specifies whether to notify responsible about the milestone actions or not</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">Updated milestone</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        ///
        /// {
        ///     title:"New title",
        ///     deadline:"2011-03-23T14:27:14",
        ///     isKey:false,
        ///     status:"Open"
        /// }
        /// ]]>
        /// </example>
        /// <path>api/2.0/project/milestone/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"milestone/{id:[0-9]+}")]
        public MilestoneWrapper UpdateMilestone(int id, string title, ApiDateTime deadline, bool? isKey, MilestoneStatus status, bool? isNotify, string description, int projectID, Guid responsible, bool notifyResponsible)
        {
            var milestoneEngine = EngineFactory.MilestoneEngine;

            var milestone = milestoneEngine.GetByID(id).NotFoundIfNull();
            ProjectSecurity.DemandEdit(milestone);

            milestone.Description = Update.IfNotEmptyAndNotEquals(milestone.Description, description);
            milestone.Title = Update.IfNotEmptyAndNotEquals(milestone.Title, title);
            milestone.DeadLine = Update.IfNotEmptyAndNotEquals(milestone.DeadLine, deadline);
            milestone.Responsible = Update.IfNotEmptyAndNotEquals(milestone.Responsible, responsible);

            if (isKey.HasValue)
                milestone.IsKey = isKey.Value;

            if (isNotify.HasValue)
                milestone.IsNotify = isNotify.Value;

            if (projectID != 0)
            {
                var project = EngineFactory.ProjectEngine.GetByID(projectID).NotFoundIfNull();
                milestone.Project = project;
            }

            milestoneEngine.SaveOrUpdate(milestone, notifyResponsible);
            MessageService.Send(Request, MessageAction.MilestoneUpdated, MessageTarget.Create(milestone.ID), milestone.Project.Title, milestone.Title);

            return MilestoneWrapperSelector(milestone);
        }

        /// <summary>
        /// Updates a status of a milestone with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Update a milestone status
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="id">Milestone ID</param>
        /// <param type="ASC.Projects.Core.Domain.MilestoneStatus, ASC.Projects.Core.Domain" name="status">New milestone status ("Open" or "Closed")</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">Updated milestone</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        ///
        /// {
        ///     status:"Open"
        /// }
        /// ]]>
        /// </example>
        /// <path>api/2.0/project/milestone/{id}/status</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"milestone/{id:[0-9]+}/status")]
        public MilestoneWrapper UpdateMilestone(int id, MilestoneStatus status)
        {
            var milestoneEngine = EngineFactory.MilestoneEngine;

            var milestone = milestoneEngine.GetByID(id).NotFoundIfNull();

            milestoneEngine.ChangeStatus(milestone, status);
            MessageService.Send(Request, MessageAction.MilestoneUpdatedStatus, MessageTarget.Create(milestone.ID), milestone.Project.Title, milestone.Title, LocalizedEnumConverter.ConvertToString(milestone.Status));

            return MilestoneWrapperSelector(milestone);
        }

        /// <summary>
        /// Deletes a milestone with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Delete a milestone
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32, System" method="url" name="id">Milestone ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">Deleted milestone</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/milestone/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"milestone/{id:[0-9]+}")]
        public MilestoneWrapper DeleteMilestone(int id)
        {
            var milestoneEngine = EngineFactory.MilestoneEngine;

            var milestone = milestoneEngine.GetByID(id).NotFoundIfNull();

            milestoneEngine.Delete(milestone);
            MessageService.Send(Request, MessageAction.MilestoneDeleted, MessageTarget.Create(milestone.ID), milestone.Project.Title, milestone.Title);

            return MilestoneWrapperSelector(milestone);
        }

        /// <summary>
        /// Deletes the milestones with the IDs specified in the request.
        /// </summary>
        /// <short>
        /// Delete milestones
        /// </short>
        /// <category>Milestones</category>
        /// <param type="System.Int32[], System" name="ids">Milestone IDs</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MilestoneWrapper, ASC.Api.Projects">Deleted milestones</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/milestone</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
        [Delete(@"milestone")]
        public IEnumerable<MilestoneWrapper> DeleteMilestones(int[] ids)
        {
            var result = new List<MilestoneWrapper>(ids.Length);

            foreach (var id in ids)
            {
                try
                {
                    result.Add(DeleteMilestone(id));
                }
                catch (Exception)
                {

                }
            }

            return result;
        }

        #endregion
    }
}