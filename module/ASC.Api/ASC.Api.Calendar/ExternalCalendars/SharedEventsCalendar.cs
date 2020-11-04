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
using ASC.Api.Calendar.BusinessObjects;
using ASC.Core;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.ExternalCalendars
{
    public sealed class SharedEventsCalendar : BaseCalendar
    {
        public static string CalendarId { get { return "shared_events"; } }

        public SharedEventsCalendar()
        {
            Id = CalendarId;
            Context.HtmlBackgroundColor = "#0797ba";
            Context.HtmlTextColor = "#000000";
            Context.GetGroupMethod = () => Resources.CalendarApiResource.PersonalCalendarsGroup;
            Context.CanChangeTimeZone = true;
            Context.CanChangeAlertType = true;
            EventAlertType = EventAlertType.Hour;
            SharingOptions.SharedForAll = true;
        }

        public override List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate)
        {
            using (var provider = new DataProvider())
            {
                var events = provider.LoadSharedEvents(userId, CoreContext.TenantManager.GetCurrentTenant().TenantId, utcStartDate, utcEndDate);
                events.ForEach(e => e.CalendarId = Id);
                var ievents = new List<IEvent>(events.Select(e => (IEvent)e));
                return ievents;
            }
        }

        private string _name;
        public override string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? Resources.CalendarApiResource.SharedEventsCalendarName : _name;
            }
            set { _name = value; }
        }

        public override string Description
        {
            get { return Resources.CalendarApiResource.SharedEventsCalendarDescription; }
        }

        public override Guid OwnerId
        {
            get
            {
                return SecurityContext.CurrentAccount.ID;
            }
        }

        private TimeZoneInfo _timeZone;
        public override TimeZoneInfo TimeZone
        {
            get
            {
                return _timeZone ?? CoreContext.TenantManager.GetCurrentTenant().TimeZone;
            }
            set { _timeZone = value; }
        }
    }
}