using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    public class CalDateTimeTests
    {
        private static readonly DateTime _now = DateTime.Now;
        private static readonly DateTime _later = _now.AddHours(1);
        private static CalendarEvent GetEventWithRecurrenceRules(string tzId)
        {
            var dailyForFiveDays = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5,
            };

            var calendarEvent = new CalendarEvent
            {
                Start = new CalDateTime(_now, tzId),
                End = new CalDateTime(_later, tzId),
                RecurrenceRules = new List<RecurrencePattern> { dailyForFiveDays },
                Resources = new List<string>(new[] { "Foo", "Bar", "Baz" }),
            };
            return calendarEvent;
        }

        [Test, TestCaseSource(nameof(ToTimeZoneTestCases))]
        public void ToTimeZoneTests(CalendarEvent calendarEvent, string targetTimeZone)
        {
            var startAsUtc = calendarEvent.Start.AsUtc;
            
            var convertedStart = calendarEvent.Start.ToTimeZone(targetTimeZone);
            var convertedAsUtc = convertedStart.AsUtc;

            Assert.AreEqual(startAsUtc, convertedAsUtc);
        }

        public static IEnumerable<ITestCaseData> ToTimeZoneTestCases()
        {
            const string bclCst = "Central Standard Time";
            const string bclEastern = "Eastern Standard Time";
            var bclEvent = GetEventWithRecurrenceRules(bclCst);
            yield return new TestCaseData(bclEvent, bclEastern)
                .SetName($"BCL to BCL: {bclCst} to {bclEastern}");

            const string ianaNy = "America/New_York";
            const string ianaRome = "Europe/Rome";
            var ianaEvent = GetEventWithRecurrenceRules(ianaNy);

            yield return new TestCaseData(ianaEvent, ianaRome)
                .SetName($"IANA to IANA: {ianaNy} to {ianaRome}");

            const string utc = "UTC";
            var utcEvent = GetEventWithRecurrenceRules(utc);
            yield return new TestCaseData(utcEvent, utc)
                .SetName("UTC to UTC");

            yield return new TestCaseData(bclEvent, ianaRome)
                .SetName($"BCL to IANA: {bclCst} to {ianaRome}");

            yield return new TestCaseData(ianaEvent, bclCst)
                .SetName($"IANA to BCL: {ianaNy} to {bclCst}");
        }
    }
}
