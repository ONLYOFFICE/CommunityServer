/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using ASC.Web.Core.Calendars;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Core;

namespace ASC.Api.Projects.Calendars
{
    public sealed class ProjectCalendar : BaseCalendar
    {
        private class Event : BaseEvent
        {
        }

        private readonly Project _project;
        private readonly EngineFactory _engine;
        private Guid _userId;
        private readonly bool _following;

        public ProjectCalendar(EngineFactory engine, Guid userId, Project project, string backgroundColor, string textColor, SharingOptions sharingOptions, bool following)
        {
            _project = project;
            _engine = engine;
            _userId = userId;
            _following = following;

            Context.HtmlBackgroundColor = backgroundColor;
            Context.HtmlTextColor = textColor;
            Context.CanChangeAlertType = false;
            Context.CanChangeTimeZone = false;
            Context.GetGroupMethod = () => Web.Projects.Resources.ProjectsCommonResource.ProductName;
            Id = _project.UniqID;
            EventAlertType = EventAlertType.Hour;
            Name = _project.Title;
            Description = _project.Description;
            SharingOptions = sharingOptions;
        }

        public override List<IEvent> LoadEvents(Guid userId, DateTime startDate, DateTime endDate)
        {
            var events = new List<IEvent>();

            var tasks = new List<Task>();
            if (!_following)
                tasks = _engine.GetTaskEngine().GetByProject(_project.ID, TaskStatus.Open, userId);

            var milestones = _engine.GetMilestoneEngine().GetByStatus(_project.ID, MilestoneStatus.Open);

            foreach (var m in milestones)
            {
                events.Add(new Event
                    {
                        AlertType = EventAlertType.Never,
                        CalendarId = Id,
                        UtcStartDate = m.DeadLine,
                        UtcEndDate = m.DeadLine,
                        AllDayLong = true,
                        Id = m.UniqID,
                        Name = Web.Projects.Resources.MilestoneResource.Milestone + ": " + m.Title,
                        Description = m.Description
                    });
            }

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

                events.Add(new Event
                    {
                        AlertType = EventAlertType.Never,
                        CalendarId = Id,
                        UtcStartDate = start,
                        UtcEndDate = t.Deadline,
                        AllDayLong = true,
                        Id = t.UniqID,
                        Name = Web.Projects.Resources.TaskResource.Task + ": " + t.Title,
                        Description = t.Description
                    });
            }

            return events;
        }

        public override TimeZoneInfo TimeZone
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TimeZone; }
        }
    }
}