/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
