using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class CalendarEventTest
    {
        private static readonly DateTime _now = DateTime.UtcNow;
        private static readonly DateTime _later = _now.AddHours(1);
        private static readonly string _uid = Guid.NewGuid().ToString();

        /// <summary>
        /// Ensures that events can be properly added to a calendar.
        /// </summary>
        [Test, Category("CalendarEvent")]
        public void Add1()
        {
            var cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.AreEqual(1, cal.Children.Count);
            Assert.AreSame(evt, cal.Children[0]);
        }

        /// <summary>
        /// Ensures that events can be properly removed from a calendar.
        /// </summary>
        [Test, Category("CalendarEvent")]
        public void Remove1()
        {
            var cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.AreEqual(1, cal.Children.Count);
            Assert.AreSame(evt, cal.Children[0]);

            cal.RemoveChild(evt);
            Assert.AreEqual(0, cal.Children.Count);
            Assert.AreEqual(0, cal.Events.Count);
        }

        /// <summary>
        /// Ensures that events can be properly removed from a calendar.
        /// </summary>
        [Test, Category("CalendarEvent")]
        public void Remove2()
        {
            var cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.AreEqual(1, cal.Children.Count);
            Assert.AreSame(evt, cal.Children[0]);

            cal.Events.Remove(evt);
            Assert.AreEqual(0, cal.Children.Count);
            Assert.AreEqual(0, cal.Events.Count);
        }

        /// <summary>
        /// Ensures that event DTSTAMP is set.
        /// </summary>
        [Test, Category("CalendarEvent")]
        public void EnsureDTSTAMPisNotNull()
        {
            var cal = new Calendar();

            // Do not set DTSTAMP manually
            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.IsNotNull(evt.DtStamp);
        }

        /// <summary>
        /// Ensures that automatically set DTSTAMP property is of kind UTC.
        /// </summary>
        [Test, Category("CalendarEvent")]
        public void EnsureDTSTAMPisOfTypeUTC()
        {
            var cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.IsTrue(evt.DtStamp.IsUtc, "DTSTAMP should always be of type UTC.");
        }

        /// <summary>
        /// Ensures that automatically set DTSTAMP property is being serialized with kind UTC.
        /// </summary>
        [Test, Category("Deserialization"), TestCaseSource(nameof(EnsureAutomaticallySetDtStampIsSerializedAsUtcKind_TestCases))]
        public bool EnsureAutomaticallySetDTSTAMPisSerializedAsKindUTC(string serialized)
        {
            var lines = serialized.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var result = lines.First(s => s.StartsWith("DTSTAMP"));

            return !result.Contains("TZID=") && result.EndsWith("Z");
        }

        public static IEnumerable<ITestCaseData> EnsureAutomaticallySetDtStampIsSerializedAsUtcKind_TestCases()
        {
            var emptyCalendar = new Calendar();
            var evt = new CalendarEvent();
            emptyCalendar.Events.Add(evt);

            var serializer = new CalendarSerializer();
            yield return new TestCaseData(serializer.SerializeToString(emptyCalendar))
                .SetName("Empty calendar with empty event returns true")
                .Returns(true);

            var explicitDtStampCalendar = new Calendar();
            var explicitDtStampEvent = new CalendarEvent
            {
                DtStamp = new CalDateTime(new DateTime(2016, 8, 17, 2, 30, 0, DateTimeKind.Utc))
            };
            explicitDtStampCalendar.Events.Add(explicitDtStampEvent);
            yield return new TestCaseData(serializer.SerializeToString(explicitDtStampCalendar))
                .SetName("CalendarEvent with explicitly-set DTSTAMP property returns true")
                .Returns(true);
        }

        [Test]
        public void EventWithExDateShouldNotBeEqualToSameEventWithoutExDate()
        {
            const string icalNoException = @"BEGIN:VCALENDAR
PRODID:-//Telerik Inc.//NONSGML RadScheduler//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VTIMEZONE
TZID:UTC
BEGIN:STANDARD
TZNAME:UTC
TZOFFSETTO:+0000
TZOFFSETFROM:+0000
DTSTART:16010101T000000
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
DTSTART;TZID=UTC:20161020T170000
DTEND;TZID=UTC:20161020T230000
UID:694f818f-6d67-4307-9c4d-0b5211686ff0
IMPORTANCE:None
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR";

            const string icalWithException = @"BEGIN:VCALENDAR
PRODID:-//Telerik Inc.//NONSGML RadScheduler//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VTIMEZONE
TZID:UTC
BEGIN:STANDARD
TZNAME:UTC
TZOFFSETTO:+0000
TZOFFSETFROM:+0000
DTSTART:16010101T000000
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
DTSTART;TZID=UTC:20161020T170000
DTEND;TZID=UTC:20161020T230000
UID:694f818f-6d67-4307-9c4d-0b5211686ff0
IMPORTANCE:None
RRULE:FREQ=DAILY
EXDATE;TZID=UTC:20161020T170000
END:VEVENT
END:VCALENDAR";

            var noException = Calendar.Load(icalNoException).Events.First();
            var withException = Calendar.Load(icalWithException).Events.First();

            Assert.AreNotEqual(noException, withException);
            Assert.AreNotEqual(noException.GetHashCode(), withException.GetHashCode());
        }

        private static CalendarEvent GetSimpleEvent() => new CalendarEvent
        {
            DtStart = new CalDateTime(_now),
            DtEnd = new CalDateTime(_later),
            Uid = _uid,
        };

        [Test]
        public void RrulesAreSignificantTests()
        {
            var rrule = new RecurrencePattern(FrequencyType.Daily, 1);
            var testRrule = GetSimpleEvent();
            testRrule.RecurrenceRules = new List<RecurrencePattern> { rrule };

            var simpleEvent = GetSimpleEvent();
            Assert.AreNotEqual(simpleEvent, testRrule);
            Assert.AreNotEqual(simpleEvent.GetHashCode(), testRrule.GetHashCode());

            var testRdate = GetSimpleEvent();
            testRdate.RecurrenceDates = new List<PeriodList> { new PeriodList { new Period(new CalDateTime(_now)) } };
            Assert.AreNotEqual(simpleEvent, testRdate);
            Assert.AreNotEqual(simpleEvent.GetHashCode(), testRdate.GetHashCode());
        }

        private static List<RecurrencePattern> GetSimpleRecurrenceList()
            => new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily, 1) { Count = 5 } };
        private static List<PeriodList> GetExceptionDates()
            => new List<PeriodList> { new PeriodList { new Period(new CalDateTime(_now.AddDays(1).Date)) } };

        [Test]
        public void EventWithRecurrenceAndExceptionComparison()
        {
            var vEvent = GetSimpleEvent();
            vEvent.RecurrenceRules = GetSimpleRecurrenceList();
            vEvent.ExceptionDates = GetExceptionDates();

            var calendar = new Calendar();
            calendar.Events.Add(vEvent);

            var vEvent2 = GetSimpleEvent();
            vEvent2.RecurrenceRules = GetSimpleRecurrenceList();
            vEvent2.ExceptionDates = GetExceptionDates();

            var cal2 = new Calendar();
            cal2.Events.Add(vEvent2);

            var eventA = calendar.Events.First();
            var eventB = cal2.Events.First();

            Assert.AreEqual(eventA.RecurrenceRules.First(), eventB.RecurrenceRules.First());
            Assert.AreEqual(eventA.RecurrenceRules.First().GetHashCode(), eventB.RecurrenceRules.First().GetHashCode());
            Assert.AreEqual(eventA.ExceptionDates.First(), eventB.ExceptionDates.First());
            Assert.AreEqual(eventA.ExceptionDates.First().GetHashCode(), eventB.ExceptionDates.First().GetHashCode());
            Assert.AreEqual(eventA.GetHashCode(), eventB.GetHashCode());
            Assert.AreEqual(eventA, eventB);
            Assert.AreEqual(calendar, cal2);
        }

        [Test]
        public void AddingExdateToEventShouldNotBeEqualToOriginal()
        {
            //Create a calendar with an event with a recurrence rule
            //Serialize to string, and deserialize
            //Change the original calendar.Event to have an ExDate
            //Serialize to string, and deserialize
            //CalendarEvent and Calendar hash codes and equality should NOT be the same
            var serializer = new CalendarSerializer();

            var vEvent = GetSimpleEvent();
            vEvent.RecurrenceRules = GetSimpleRecurrenceList();
            var cal1 = new Calendar();
            cal1.Events.Add(vEvent);
            var serialized = serializer.SerializeToString(cal1);
            var deserializedNoExDate = Calendar.Load(serialized);
            Assert.AreEqual(cal1, deserializedNoExDate);

            vEvent.ExceptionDates = GetExceptionDates();
            serialized = serializer.SerializeToString(cal1);
            var deserializedWithExDate = Calendar.Load(serialized);

            Assert.AreNotEqual(deserializedNoExDate.Events.First(), deserializedWithExDate.Events.First());
            Assert.AreNotEqual(deserializedNoExDate.Events.First().GetHashCode(), deserializedWithExDate.Events.First().GetHashCode());
            Assert.AreNotEqual(deserializedNoExDate, deserializedWithExDate);
        }

        [Test]
        public void ChangingRrulesShouldNotBeEqualToOriginalEvent()
        {
            var eventA = GetSimpleEvent();
            eventA.RecurrenceRules = GetSimpleRecurrenceList();

            var eventB = GetSimpleEvent();
            eventB.RecurrenceRules = GetSimpleRecurrenceList();
            Assert.IsFalse(ReferenceEquals(eventA, eventB));
            Assert.AreEqual(eventA, eventB);

            var foreverDailyRule = new RecurrencePattern(FrequencyType.Daily, 1);
            eventB.RecurrenceRules = new List<RecurrencePattern> { foreverDailyRule };

            Assert.AreNotEqual(eventA, eventB);
            Assert.AreNotEqual(eventA.GetHashCode(), eventB.GetHashCode());
        }

        [Test]
        public void EventsDifferingByDtStampAreEqual()
        {
            const string eventA = @"BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
VERSION:2.0
BEGIN:VEVENT
ATTACH;FMTTYPE=application/json;VALUE=BINARY;ENCODING=BASE64:eyJzdWJqZWN0I
 joiSFAgQ29hdGVyIGFuZCBDdXR0ZXIgQ2xlYW51cCIsInVuaXF1ZUlkZW50aWZpZXIiOiIwND
 EwNzI1NGRjNWM5MDk0YWY3MWEwZTE5N2U2NWE1NTdkZmJjYjg0IiwiaWNhbFN0cmluZyI6IiI
 sImxhYm9yRG93bnRpbWVzIjpbXSwiZGlzYWJsZWRFcXVpcG1lbnQiOlt7ImRpc2FibGVkRXF1
 aXBtZW50SW5zdGFuY2VOYW1lcyI6WyJEaWdpdGFsIFByaW50XFxIUCAyOCIsIkRpZ2l0YWwgU
 HJpbnRcXEhQIDQ0Il0sImZ1bGxUaW1lRXF1aXZhbGVudHNDb3VudCI6MC4wfV0sIm1vZGVzTm
 90QWxsb3dlZCI6W10sInJhd01hdGVyaWFsc05vdEFsbG93ZWQiOltdLCJsYWJvckFsbG9jYXR
 pb25zIjpbXX0=
DTEND;TZID=UTC:20150615T055000
DTSTAMP:20161011T195316Z
DTSTART;TZID=UTC:20150615T054000
EXDATE;TZID=UTC:20151023T054000
IMPORTANCE:None
RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR,SA
UID:04107254dc5c9094af71a0e197e65a557dfbcb84
END:VEVENT
END:VCALENDAR";

            const string eventB = @"BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
VERSION:2.0
BEGIN:VEVENT
ATTACH;FMTTYPE=application/json;VALUE=BINARY;ENCODING=BASE64:eyJzdWJqZWN0I
 joiSFAgQ29hdGVyIGFuZCBDdXR0ZXIgQ2xlYW51cCIsInVuaXF1ZUlkZW50aWZpZXIiOiIwND
 EwNzI1NGRjNWM5MDk0YWY3MWEwZTE5N2U2NWE1NTdkZmJjYjg0IiwiaWNhbFN0cmluZyI6IiI
 sImxhYm9yRG93bnRpbWVzIjpbXSwiZGlzYWJsZWRFcXVpcG1lbnQiOlt7ImRpc2FibGVkRXF1
 aXBtZW50SW5zdGFuY2VOYW1lcyI6WyJEaWdpdGFsIFByaW50XFxIUCAyOCIsIkRpZ2l0YWwgU
 HJpbnRcXEhQIDQ0Il0sImZ1bGxUaW1lRXF1aXZhbGVudHNDb3VudCI6MC4wfV0sIm1vZGVzTm
 90QWxsb3dlZCI6W10sInJhd01hdGVyaWFsc05vdEFsbG93ZWQiOltdLCJsYWJvckFsbG9jYXR
 pb25zIjpbXX0=
DTEND;TZID=UTC:20150615T055000
DTSTAMP:20161024T201419Z
DTSTART;TZID=UTC:20150615T054000
EXDATE;TZID=UTC:20151023T054000
IMPORTANCE:None
RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR,SA
UID:04107254dc5c9094af71a0e197e65a557dfbcb84
END:VEVENT
END:VCALENDAR";

            var calendarA = Calendar.Load(eventA);
            var calendarB = Calendar.Load(eventB);

            Assert.AreEqual(calendarA.Events.First().GetHashCode(), calendarB.Events.First().GetHashCode());
            Assert.AreEqual(calendarA.Events.First(), calendarB.Events.First());
            Assert.AreEqual(calendarA.GetHashCode(), calendarB.GetHashCode());
            Assert.AreEqual(calendarA, calendarB);
        }

        [Test]
        public void EventResourcesCanBeZeroedOut()
        {
            var e = GetSimpleEvent();
            var resources = new[] { "Foo", "Bar", "Baz" };

            e.Resources = new List<string>(resources);
            CollectionAssert.AreEquivalent(e.Resources, resources);

            var newResources = new[] { "Hello", "Goodbye" };
            e.Resources = new List<string>(newResources);
            CollectionAssert.AreEquivalent(e.Resources, newResources);
            Assert.IsFalse(e.Resources.Any(r => resources.Contains(r)));

            e.Resources = null;
            //See https://github.com/rianjs/ical.net/issues/208 -- this should be changed later so the collection is really null
            Assert.AreEqual(0, e.Resources?.Count);
        }

        [Test]
        public void HourMinuteSecondOffsetParsingTest()
        {
            const string ical =
@"BEGIN:VCALENDAR
PRODID:-//1&1 Mail & Media GmbH/GMX Kalender Server 3.10.0//NONSGML//DE
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:REQUEST
BEGIN:VTIMEZONE
TZID:Europe/Brussels
TZURL:http://tzurl.org/zoneinfo/Europe/Brussels
X-LIC-LOCATION:Europe/Brussels
BEGIN:DAYLIGHT
TZOFFSETFROM:-001730
TZOFFSETTO:-001730
TZNAME:CEST
DTSTART:19810329T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:+001730
TZOFFSETTO:+001730
TZNAME:BMT
DTSTART:18800101T000000
RDATE:18800101T000000
END:STANDARD
END:VTIMEZONE
END:VCALENDAR";
            var timezones = Calendar.Load(ical)
                .TimeZones.First()
                .Children.Cast<CalendarComponent>();

            var positiveOffset = timezones
                .Skip(1).Take(1).First()
                .Properties.First().Value as UtcOffset;
            var expectedPositive = TimeSpan.FromMinutes(17.5);
            Assert.AreEqual(expectedPositive, positiveOffset?.Offset);

            var negativeOffset = timezones
                .First()
                .Properties.First().Value as UtcOffset;

            var expectedNegative = TimeSpan.FromMinutes(-17.5);
            Assert.AreEqual(expectedNegative, negativeOffset?.Offset);
        }
    }
}
