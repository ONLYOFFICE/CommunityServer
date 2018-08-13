/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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