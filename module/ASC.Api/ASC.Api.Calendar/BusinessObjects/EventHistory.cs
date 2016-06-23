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

        public List<DDay.iCal.IICalendar> History
        {
            get {
                var history = DDayICalParser.DeserializeCalendar(Ics);
                return history == null ? new List<DDay.iCal.IICalendar>() : history.ToList();
            }
        }

        public bool Contains(DDay.iCal.IICalendar calendar)
        {
            if (!History.Any() || calendar == null || calendar.Events == null || calendar.Events.FirstOrDefault() == null)
                return false;
            
            var eventObj = calendar.Events.First();

            var isExist = History
                .Where(x => x.Method == calendar.Method)
                .Select(x => x.Events.First())
                .Any(x => x.Sequence == eventObj.Sequence && x.DTStamp.UTC == eventObj.DTStamp.UTC);

            return isExist;
        }

        public DDay.iCal.IICalendar GetMerged()
        {
            if (!History.Any()) return null;

            var allCalendars = History
                .Where(x => x != null && x.Events != null && x.Events.FirstOrDefault() != null)
                .ToList();

            if(!allCalendars.Any()) return null;

            var recurrenceIdCalendars = new List<DDay.iCal.IICalendar>();
            var calendars = new List<DDay.iCal.IICalendar>();

            foreach (var cal in allCalendars)
            {
                if (cal.Events.First().RecurrenceID == null)
                    calendars.Add(cal);
                else
                    recurrenceIdCalendars.Add(cal);
            }

            recurrenceIdCalendars = recurrenceIdCalendars
                .OrderByDescending(x => x.Events.First().DTStamp)
                .ToList();

            if (!calendars.Any())
            {
                return recurrenceIdCalendars.FirstOrDefault(x => x.Method != DDay.iCal.CalendarMethods.Reply) ?? recurrenceIdCalendars.First();
            }

            recurrenceIdCalendars = recurrenceIdCalendars
                .Where(x => x.Method == DDay.iCal.CalendarMethods.Cancel)
                .ToList();

            var sequence = calendars
                .Select(x => x.Events.First())
                .Max(x => x.Sequence);

            calendars = calendars
                .Where(x => x.Events.First().Sequence == sequence)
                .OrderByDescending(x => x.Events.First().DTStamp)
                .ToList();

            var targetCalendar = calendars.FirstOrDefault(x => x.Method != DDay.iCal.CalendarMethods.Reply) ?? calendars.First();

            if (targetCalendar == null) return null;

            var targetEvent = targetCalendar.Events.First();

            if (targetCalendar.Method == DDay.iCal.CalendarMethods.Cancel)
                targetEvent.Status = DDay.iCal.EventStatus.Cancelled;

            calendars = calendars
                .Where(x => x.Method == DDay.iCal.CalendarMethods.Reply)
                .OrderBy(x => x.Events.First().DTStamp)
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

                        targetAttendee.ParticipationStatus = tmpAttendee.ParticipationStatus;
                        targetAttendee.RSVP = tmpAttendee.RSVP;
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

                if (targetEvent.ExceptionDates.All(x => !x.First().Contains(removedEvent.RecurrenceID)))
                {
                    targetEvent.ExceptionDates.Add(new DDay.iCal.PeriodList
                        {
                            removedEvent.RecurrenceID
                        });
                }
            }

            targetCalendar.Events.Clear();
            targetCalendar.Events.Add(targetEvent);

            return targetCalendar;
        }
    }
}
