using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NUnit.Framework;

namespace Ical.Net.FrameworkUnitTests
{
    internal class GetOccurrenceTests
    {
        public static CalendarCollection GetCalendars(string incoming) => CalendarCollection.Load(incoming);

        [Test]
        public void WrongDurationTest()
        {
            var firstStart = new CalDateTime(DateTime.Parse("2016-01-01"));
            var firstEnd = new CalDateTime(DateTime.Parse("2016-01-05"));
            var vEvent = new CalendarEvent {DtStart = firstStart, DtEnd = firstEnd,};

            var secondStart = new CalDateTime(DateTime.Parse("2016-03-01"));
            var secondEnd = new CalDateTime(DateTime.Parse("2016-03-05"));
            var vEvent2 = new CalendarEvent {DtStart = secondStart, DtEnd = secondEnd,};

            var calendar = new Calendar();
            calendar.Events.Add(vEvent);
            calendar.Events.Add(vEvent2);

            var searchStart = DateTime.Parse("2015-12-29");
            var searchEnd = DateTime.Parse("2017-02-10");
            var occurrences = calendar.GetOccurrences(searchStart, searchEnd).OrderBy(o => o.Period.StartTime).ToList();

            var firstOccurrence = occurrences.First();
            var firstStartCopy = firstStart.Copy<CalDateTime>();
            var firstEndCopy = firstEnd.Copy<CalDateTime>();
            Assert.AreEqual(firstStartCopy, firstOccurrence.Period.StartTime);
            Assert.AreEqual(firstEndCopy, firstOccurrence.Period.EndTime);

            var secondOccurrence = occurrences.Last();
            var secondStartCopy = secondStart.Copy<CalDateTime>();
            var secondEndCopy = secondEnd.Copy<CalDateTime>();
            Assert.AreEqual(secondStartCopy, secondOccurrence.Period.StartTime);
            Assert.AreEqual(secondEndCopy, secondOccurrence.Period.EndTime);
        }

        [Test]
        public void SkippedOccurrenceOnWeeklyPattern()
        {
            const int evaluationsCount = 1000;
            var eventStart = new CalDateTime(new DateTime(2016, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var eventEnd = new CalDateTime(new DateTime(2016, 1, 1, 11, 0, 0, DateTimeKind.Utc));
            var vEvent = new CalendarEvent
            {
                DtStart = eventStart,
                DtEnd = eventEnd,
            };

            var pattern = new RecurrencePattern
            {
                Frequency = FrequencyType.Weekly,
                ByDay = new List<WeekDay> { new WeekDay(DayOfWeek.Friday) }
            };
            vEvent.RecurrenceRules.Add(pattern);
            var calendar = new Calendar();
            calendar.Events.Add(vEvent);

            var intervalStart = eventStart;
            var intervalEnd = intervalStart.AddDays(7 * evaluationsCount);

            var occurrences = RecurrenceUtil.GetOccurrences(
                recurrable: vEvent,
                periodStart: intervalStart,
                periodEnd: intervalEnd,
                includeReferenceDateInResults: false);
            var occurrenceSet = new HashSet<IDateTime>(occurrences.Select(o => o.Period.StartTime));

            Assert.AreEqual(evaluationsCount, occurrenceSet.Count);

            for (var currentOccurrence = intervalStart; currentOccurrence.CompareTo(intervalEnd) < 0; currentOccurrence = (CalDateTime)currentOccurrence.AddDays(7))
            {
                var contains = occurrenceSet.Contains(currentOccurrence);
                Assert.IsTrue(contains, $"Collection does not contain {currentOccurrence}, but it is a {currentOccurrence.DayOfWeek}");
            }
        }


        [Test]
        public void EnumerationChangedException()
        {
            const string ical = @"BEGIN:VCALENDAR
PRODID:-//Google Inc//Google Calendar 70.9054//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
X-WR-CALNAME:name
X-WR-TIMEZONE:America/New_York
BEGIN:VTIMEZONE
TZID:America/New_York
X-LIC-LOCATION:America/New_York
BEGIN:DAYLIGHT
TZOFFSETFROM:-0500
TZOFFSETTO:-0400
TZNAME:EDT
DTSTART:19700308T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:-0400
TZOFFSETTO:-0500
TZNAME:EST
DTSTART:19701101T020000
RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
END:STANDARD
END:VTIMEZONE

BEGIN:VEVENT
DTSTART;TZID=America/New_York:20161011T170000
DTEND;TZID=America/New_York:20161011T180000
DTSTAMP:20160930T115710Z
UID:blablabla
RECURRENCE-ID;TZID=America/New_York:20161011T170000
CREATED:20160830T144559Z
DESCRIPTION:
LAST-MODIFIED:20160928T142659Z
LOCATION:Location1
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:Summary1
TRANSP:OPAQUE
END:VEVENT

END:VCALENDAR";

            var calendar = GetCalendars(ical);
            var date = new DateTime(2016, 10, 11);
            var occurrences = calendar.GetOccurrences(date);

            //We really want to make sure this doesn't explode
            Assert.AreEqual(1, occurrences.Count);
        }

        [Test]
        public void GetOccurrencesShouldEnumerate()
        {
            const string ical =
   @"BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
VERSION:2.0
BEGIN:VTIMEZONE
TZID:W. Europe Standard Time
BEGIN:STANDARD
DTSTART:16010101T030000
RRULE:FREQ=YEARLY;BYDAY=SU;BYMONTH=10;BYSETPOS=-1
TZNAME:Mitteleuropäische Zeit
TZOFFSETFROM:+0200
TZOFFSETTO:+0100
END:STANDARD
BEGIN:DAYLIGHT
DTSTART:00010101T020000
RRULE:FREQ=YEARLY;BYDAY=SU;BYMONTH=3;BYSETPOS=-1
TZNAME:Mitteleuropäische Sommerzeit
TZOFFSETFROM:+0100
TZOFFSETTO:+0200
END:DAYLIGHT
END:VTIMEZONE
BEGIN:VEVENT
BACKGROUND:BUSY
DESCRIPTION:Backup Daten
DTEND;TZID=W. Europe Standard Time:20150305T043000
DTSTAMP:20161122T120652Z
DTSTART;TZID=W. Europe Standard Time:20150305T000100
RESOURCES:server
RRULE:FREQ=WEEKLY;BYDAY=MO
SUMMARY:Server
UID:a30ed847-8000-4c53-9e58-99c8f9cf7c4b
X-LIGHTSOUT-ACTION:START=WakeUp\;END=Reboot\,Force
X-LIGHTSOUT-MODE:TimeSpan
X-MICROSOFT-CDO-BUSYSTATUS:BUSY
END:VEVENT
BEGIN:VEVENT
BACKGROUND:BUSY
DESCRIPTION:Backup Daten
DTEND;TZID=W. Europe Standard Time:20161128T043000
DTSTAMP:20161122T120652Z
DTSTART;TZID=W. Europe Standard Time:20161128T000100
RECURRENCE-ID:20161128T000100
RESOURCES:server
SEQUENCE:0
SUMMARY:Server
UID:a30ed847-8000-4c53-9e58-99c8f9cf7c4b
X-LIGHTSOUT-ACTION:START=WakeUp\;END=Reboot\,Force
X-LIGHTSOUT-MODE:TimeSpan
X-MICROSOFT-CDO-BUSYSTATUS:BUSY
END:VEVENT
END:VCALENDAR
";

            var collection = Calendar.Load(ical);
            var startCheck = new DateTime(2016, 11, 11);
            var occurrences = collection.GetOccurrences<CalendarEvent>(startCheck, startCheck.AddMonths(1));

            Assert.IsTrue(occurrences.Count == 4);
        }
    }
}
