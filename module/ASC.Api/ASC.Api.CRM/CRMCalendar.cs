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
using System.Globalization;
using ASC.Web.Core.Calendars;
using ASC.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Api.CRM
{
    public sealed class CRMCalendar : BaseCalendar
    {
        [AllDayLongUTCAttribute]
        private class Event : BaseEvent
        {
        }


        public CRMCalendar(Guid userId)
        {
            Context.HtmlBackgroundColor = "";
            Context.HtmlTextColor = "";
            Context.CanChangeAlertType = false;
            Context.CanChangeTimeZone = false;
            Context.GetGroupMethod = () => Web.CRM.Resources.CRMCommonResource.ProductName;
            Id = "crm_calendar";
            EventAlertType = EventAlertType.Never;
            Name = Web.CRM.Resources.CRMCommonResource.ProductName;
            Description = "";
            SharingOptions = new SharingOptions();
            SharingOptions.PublicItems.Add(new SharingOptions.PublicItem {Id = userId, IsGroup = false});
        }

        public override List<IEvent> LoadEvents(Guid userId, DateTime startDate, DateTime endDate)
        {
            using (var scope = DIHelper.Resolve())
            {
                var _daoFactory = scope.Resolve<DaoFactory>();
                var events = new List<IEvent>();

                if (
                    !WebItemSecurity.IsAvailableForMe(WebItemManager.CRMProductID))
                {
                    return events;
                }

                var tasks = _daoFactory.TaskDao.GetTasks(String.Empty, userId, 0, false, DateTime.MinValue,
                    DateTime.MinValue, EntityType.Any, 0, 0, 0, null);

                foreach (var t in tasks)
                {
                    if (t.DeadLine == DateTime.MinValue) continue;

                    var allDayEvent = t.DeadLine.Hour == 0 && t.DeadLine.Minute == 0;
                    var utcDate = allDayEvent ? t.DeadLine.Date : Core.Tenants.TenantUtil.DateTimeToUtc(t.DeadLine);

                    var e = new Event
                    {
                        AlertType = EventAlertType.Never,
                        AllDayLong = allDayEvent,
                        CalendarId = Id,
                        UtcStartDate = utcDate,
                        UtcEndDate = utcDate,
                        Id = "crm_task_" + t.ID.ToString(CultureInfo.InvariantCulture),
                        Name = Web.CRM.Resources.CRMCommonResource.ProductName + ": " + t.Title,
                        Description = t.Description
                    };

                    if (IsVisibleEvent(startDate, endDate, e.UtcStartDate, e.UtcEndDate))
                        events.Add(e);
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