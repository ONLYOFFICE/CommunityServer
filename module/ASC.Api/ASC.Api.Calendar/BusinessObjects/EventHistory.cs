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
using ASC.Api.Calendar.iCalParser;


namespace ASC.Api.Calendar.BusinessObjects
{
    public class EventHistory
    {
        public EventHistory(int calendarId, string eventUid, int eventId, string ics)
        {
            CalendarId = calendarId;
            EventUid = eventUid;
            EventId = eventId;
            Ics = ics;
        }
        
        public int CalendarId { get; set; }
        public string EventUid { get; set; }
        public int EventId { get; set; }
        public string Ics { get; set; }

        public List<Ical.Net.Calendar> History
        {
            get {
                var history = DDayICalParser.DeserializeCalendar(Ics);
                return history == null ? new List<Ical.Net.Calendar>() : history.ToList();
            }
        }

        public bool Contains(Ical.Net.Calendar calendar)
        {
            if (!History.Any() || calendar == null || calendar.Events == null || calendar.Events.FirstOrDefault() == null)
                return false;
            
            var eventObj = calendar.Events.First();

            var isExist = History
                .Where(x => x.Method == calendar.Method)
                .Select(x => x.Events.First())
                .Any(x => x.Sequence == eventObj.Sequence && DDayICalParser.ToUtc(x.DtStamp) == DDayICalParser.ToUtc(eventObj.DtStamp));

            return isExist;
        }

        public Ical.Net.Calendar GetMerged()
        {
            if (!History.Any()) return null;

            var allCalendars = History
                .Where(x => x != null && x.Events != null && x.Events.FirstOrDefault() != null)
                .ToList();

            if(!allCalendars.Any()) return null;

            var recurrenceIdCalendars = new List<Ical.Net.Calendar>();
            var calendars = new List<Ical.Net.Calendar>();

            foreach (var cal in allCalendars)
            {
                if (cal.Events.First().RecurrenceId == null)
                    calendars.Add(cal);
                else
                    recurrenceIdCalendars.Add(cal);
            }

            recurrenceIdCalendars = recurrenceIdCalendars
                .OrderByDescending(x => x.Events.First().DtStamp)
                .ToList();

            if (!calendars.Any())
            {
                return recurrenceIdCalendars.FirstOrDefault(x => x.Method != Ical.Net.CalendarMethods.Reply) ?? recurrenceIdCalendars.First();
            }

            recurrenceIdCalendars = recurrenceIdCalendars
                .Where(x => x.Method == Ical.Net.CalendarMethods.Cancel)
                .ToList();

            var sequence = calendars
                .Select(x => x.Events.First())
                .Max(x => x.Sequence);

            calendars = calendars
                .Where(x => x.Events.First().Sequence == sequence)
                .OrderByDescending(x => x.Events.First().DtStamp)
                .ToList();

            var targetCalendar = calendars.FirstOrDefault(x => x.Method != Ical.Net.CalendarMethods.Reply) ?? calendars.First();

            if (targetCalendar == null) return null;

            var targetEvent = targetCalendar.Events.First();

            if (targetCalendar.Method == Ical.Net.CalendarMethods.Cancel)
                targetEvent.Status = Ical.Net.EventStatus.Cancelled;

            calendars = calendars
                .Where(x => x.Method == Ical.Net.CalendarMethods.Reply)
                .OrderBy(x => x.Events.First().DtStamp)
                .ToList();

            foreach (var calendar in calendars)
            {
                var tmpEvent = calendar.Events.First();

                foreach (var tmpAttendee in tmpEvent.Attendees)
                {
                    var exist = false;

                    foreach (var targetAttendee in targetEvent.Attendees)
                    {
                        if (!targetAttendee.Value.OriginalString.Equals(tmpAttendee.Value.OriginalString, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var parameters = new Ical.Net.ParameterList();

                        foreach (var param in targetAttendee.Parameters)
                        {
                            switch (param.Group)
                            {
                                case "PARTSTAT":
                                    parameters.Add(new Ical.Net.CalendarParameter(param.Group, tmpAttendee.ParticipationStatus));
                                    break;
                                case "RSVP":
                                    parameters.Add(new Ical.Net.CalendarParameter(param.Group, tmpAttendee.Rsvp.ToString().ToUpper()));
                                    break;
                                default:
                                    parameters.Add(param);
                                    break;
                            }
                        }

                        targetAttendee.Parameters.Clear();

                        foreach (var param in parameters)
                        {
                            targetAttendee.Parameters.Add(param);
                        }

                        targetAttendee.ParticipationStatus = tmpAttendee.ParticipationStatus;
                        targetAttendee.Rsvp = tmpAttendee.Rsvp;
                        exist = true;
                        break;
                    }

                    if (!exist)
                        targetEvent.Attendees.Add(tmpAttendee);
                }
            }

            foreach (var recurrenceIdCalendar in recurrenceIdCalendars)
            {
                var removedEvent = recurrenceIdCalendar.Events.First();

                if (targetEvent.ExceptionDates.All(x => !x.First().Contains(removedEvent.RecurrenceId)))
                {
                    targetEvent.ExceptionDates.Add(new Ical.Net.DataTypes.PeriodList
                        {
                            removedEvent.RecurrenceId
                        });
                }
            }

            targetCalendar.Events.Clear();
            targetCalendar.Events.Add(targetEvent);

            return targetCalendar;
        }
    }
}
