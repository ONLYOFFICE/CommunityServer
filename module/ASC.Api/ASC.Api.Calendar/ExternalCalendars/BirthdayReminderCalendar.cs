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
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Api.Calendar.ExternalCalendars
{
    public sealed class BirthdayReminderCalendar : BaseCalendar
    {
        public readonly static string CalendarId = "users_birthdays";

        public BirthdayReminderCalendar()
        {
            Id = CalendarId;
            Context.HtmlBackgroundColor = "#f08e1c";
            Context.HtmlTextColor = "#000000";
            Context.GetGroupMethod = () => Resources.CalendarApiResource.CommonCalendarsGroup;
            Context.CanChangeTimeZone = false;
            EventAlertType = EventAlertType.Day;
            SharingOptions.SharedForAll = true;
        }

        private sealed class BirthdayEvent : BaseEvent
        {
            public BirthdayEvent(string id, string name, DateTime birthday)
            {
                Id = "bde_" + id;
                Name = name;
                OwnerId = Guid.Empty;
                AlertType = EventAlertType.Day;
                AllDayLong = true;
                CalendarId = BirthdayReminderCalendar.CalendarId;
                UtcEndDate = birthday;
                UtcStartDate = birthday;
                RecurrenceRule.Freq = Frequency.Yearly;
            }
        }
        
        public override List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate)
        {
            var events = new List<IEvent>();
            var usrs = CoreContext.UserManager.GetUsers().Where(u => u.BirthDate.HasValue).ToList();
            foreach (var usr in usrs)
            {
                DateTime bd;

                if (DateTime.DaysInMonth(utcStartDate.Year, usr.BirthDate.Value.Month) >= usr.BirthDate.Value.Day)
                {
                    bd = new DateTime(utcStartDate.Year, usr.BirthDate.Value.Month, usr.BirthDate.Value.Day);

                    if (bd >= utcStartDate && bd <= utcEndDate)
                    {
                        events.Add(new BirthdayEvent(usr.ID.ToString(), usr.DisplayUserName(), usr.BirthDate.Value));
                        continue;
                    }
                }

                if (DateTime.DaysInMonth(utcEndDate.Year, usr.BirthDate.Value.Month) >= usr.BirthDate.Value.Day)
                {
                    bd = new DateTime(utcEndDate.Year, usr.BirthDate.Value.Month, usr.BirthDate.Value.Day);

                    if (bd >= utcStartDate && bd <= utcEndDate)
                        events.Add(new BirthdayEvent(usr.ID.ToString(), usr.DisplayUserName(), usr.BirthDate.Value));
                }
            }
            return events;
        }

        private string _name;
        public override string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? Resources.CalendarApiResource.BirthdayCalendarName : _name;
            }
            set { _name = value; }
        }

        public override string Description
        {
            get { return Resources.CalendarApiResource.BirthdayCalendarDescription; }
        }

        public override TimeZoneInfo TimeZone
        {
            get { return TimeZoneInfo.Utc; }
        }
    }
}
