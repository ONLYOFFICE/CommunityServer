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
using ASC.Web.Core.Calendars;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Core;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Api.Projects.Calendars
{
    public sealed class ProjectCalendar : BaseCalendar
    {
        [AllDayLongUTCAttribute]
        private class Event : BaseEvent
        {
        }

        private readonly Project project;
        private readonly bool following;

        public ProjectCalendar(Project project, string backgroundColor, string textColor, SharingOptions sharingOptions, bool following)
        {
            this.project = project;
            this.following = following;

            Context.HtmlBackgroundColor = backgroundColor;
            Context.HtmlTextColor = textColor;
            Context.CanChangeAlertType = false;
            Context.CanChangeTimeZone = false;
            Context.GetGroupMethod = () => Web.Projects.Resources.ProjectsCommonResource.ProductName;
            Id = this.project.UniqID;
            EventAlertType = EventAlertType.Hour;
            Name = this.project.Title;
            Description = this.project.Description;
            SharingOptions = sharingOptions;
        }

        public override List<IEvent> LoadEvents(Guid userId, DateTime startDate, DateTime endDate)
        {
            using (var scope = DIHelper.Resolve())
            {
                var engine = scope.Resolve<EngineFactory>();
                var tasks = new List<Task>();
                if (!following)
                    tasks.AddRange(engine.TaskEngine.GetByProject(project.ID, TaskStatus.Open, userId));

                var milestones = engine.MilestoneEngine.GetByStatus(project.ID, MilestoneStatus.Open);

                var events = milestones
                    .Select(m => new Event
                    {
                        AlertType = EventAlertType.Never,
                        CalendarId = Id,
                        UtcStartDate = m.DeadLine,
                        UtcEndDate = m.DeadLine,
                        AllDayLong = true,
                        Id = m.UniqID,
                        Name = Web.Projects.Resources.MilestoneResource.Milestone + ": " + m.Title,
                        Description = m.Description
                    })
                    .Where(e => IsVisibleEvent(startDate, endDate, e.UtcStartDate, e.UtcEndDate))
                    .Cast<IEvent>()
                    .ToList();

                foreach (var t in tasks)
                {
                    var start = t.StartDate;

                    if (!t.Deadline.Equals(DateTime.MinValue))
                    {
                        start = start.Equals(DateTime.MinValue) ? t.Deadline : t.StartDate;
                    }
                    else
                    {
                        start = DateTime.MinValue;
                    }

                    var projectEvent = new Event
                    {
                        AlertType = EventAlertType.Never,
                        CalendarId = Id,
                        UtcStartDate = start,
                        UtcEndDate = t.Deadline,
                        AllDayLong = true,
                        Id = t.UniqID,
                        Name = Web.Projects.Resources.TaskResource.Task + ": " + t.Title,
                        Description = t.Description
                    };

                    if (IsVisibleEvent(startDate, endDate, projectEvent.UtcStartDate, projectEvent.UtcEndDate))
                        events.Add(projectEvent);
                }

                return events;
            }
        }

        public override TimeZoneInfo TimeZone
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TimeZone; }
        }

        private bool IsVisibleEvent(DateTime startDate, DateTime endDate, DateTime eventStartDate, DateTime eventEndDate)
        {
            return (startDate <= eventStartDate && eventStartDate <= endDate) ||
                   (startDate <= eventEndDate && eventEndDate <= endDate) ||
                   (eventStartDate < startDate && eventEndDate > endDate);
        }
    }
}