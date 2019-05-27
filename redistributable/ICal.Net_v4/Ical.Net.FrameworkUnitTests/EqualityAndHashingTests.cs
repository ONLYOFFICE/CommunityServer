using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    public class EqualityAndHashingTests
    {
        private const string _someTz = "America/Los_Angeles";
        private static readonly DateTime _nowTime = DateTime.Parse("2016-07-16T16:47:02.9310521-04:00");
        private static readonly DateTime _later = _nowTime.AddHours(1);

        [Test, TestCaseSource(nameof(CalDateTime_TestCases))]
        public void CalDateTime_Tests(CalDateTime incomingDt, CalDateTime expectedDt)
        {
            Assert.AreEqual(incomingDt.Value, expectedDt.Value);
            Assert.AreEqual(incomingDt.GetHashCode(), expectedDt.GetHashCode());
            Assert.AreEqual(incomingDt.TzId, expectedDt.TzId);
            Assert.IsTrue(incomingDt.Equals(expectedDt));
        }

        public static IEnumerable<ITestCaseData> CalDateTime_TestCases()
        {
            var nowCalDt = new CalDateTime(_nowTime);
            yield return new TestCaseData(nowCalDt, new CalDateTime(_nowTime)).SetName("Now, no time zone");

            var nowCalDtWithTz = new CalDateTime(_nowTime, _someTz);
            yield return new TestCaseData(nowCalDtWithTz, new CalDateTime(_nowTime, _someTz)).SetName("Now, with time zone");
        }

        [Test]
        public void RecurrencePatternTests()
        {
            var patternA = GetSimpleRecurrencePattern();
            var patternB = GetSimpleRecurrencePattern();

            Assert.AreEqual(patternA, patternB);
            Assert.AreEqual(patternA.GetHashCode(), patternB.GetHashCode());
        }

        [Test, TestCaseSource(nameof(Event_TestCases))]
        public void Event_Tests(CalendarEvent incoming, CalendarEvent expected)
        {
            Assert.AreEqual(incoming.DtStart, expected.DtStart);
            Assert.AreEqual(incoming.DtEnd, expected.DtEnd);
            Assert.AreEqual(incoming.Location, expected.Location);
            Assert.AreEqual(incoming.Status, expected.Status);
            Assert.AreEqual(incoming.IsActive, expected.IsActive);
            Assert.AreEqual(incoming.Duration, expected.Duration);
            Assert.AreEqual(incoming.Transparency, expected.Transparency);
            Assert.AreEqual(incoming.GetHashCode(), expected.GetHashCode());
            Assert.IsTrue(incoming.Equals(expected));
        }

        private static RecurrencePattern GetSimpleRecurrencePattern() => new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5
        };

        private static CalendarEvent GetSimpleEvent() => new CalendarEvent
        {
            DtStart = new CalDateTime(_nowTime),
            DtEnd = new CalDateTime(_later),
            Duration = TimeSpan.FromHours(1),
        };

        private static string SerializeEvent(CalendarEvent e) => new CalendarSerializer().SerializeToString(new Calendar { Events = { e } });


        public static IEnumerable<ITestCaseData> Event_TestCases()
        {
            var outgoing = GetSimpleEvent();
            var expected = GetSimpleEvent();
            yield return new TestCaseData(outgoing, expected).SetName("Events with start, end, and duration");

            var fiveA = GetSimpleRecurrencePattern();
            var fiveB = GetSimpleRecurrencePattern();

            outgoing = GetSimpleEvent();
            expected = GetSimpleEvent();
            outgoing.RecurrenceRules = new List<RecurrencePattern> { fiveA };
            expected.RecurrenceRules = new List<RecurrencePattern> { fiveB };
            yield return new TestCaseData(outgoing, expected).SetName("Events with start, end, duration, and one recurrence rule");
        }

        [Test]
        public void Calendar_Tests()
        {
            var rruleA = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var e = new CalendarEvent
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<RecurrencePattern> { rruleA },
            };

            var actualCalendar = new Calendar();
            actualCalendar.Events.Add(e);

            //Work around referential equality...
            var rruleB = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var expectedCalendar = new Calendar();
            expectedCalendar.Events.Add(new CalendarEvent
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<RecurrencePattern> { rruleB },
            });

            Assert.AreEqual(actualCalendar.GetHashCode(), expectedCalendar.GetHashCode());
            Assert.IsTrue(actualCalendar.Equals(expectedCalendar));
        }

        [Test, TestCaseSource(nameof(VTimeZone_TestCases))]
        public void VTimeZone_Tests(VTimeZone actual, VTimeZone expected)
        {
            Assert.AreEqual(actual.Url, expected.Url);
            Assert.AreEqual(actual.TzId, expected.TzId);
            Assert.AreEqual(actual, expected);
            Assert.AreEqual(actual.GetHashCode(), expected.GetHashCode());
        }

        public static IEnumerable<ITestCaseData> VTimeZone_TestCases()
        {
            const string nzSt = "New Zealand Standard Time";
            var first = new VTimeZone
            {
                TzId = nzSt,
            };
            var second = new VTimeZone(nzSt);
            yield return new TestCaseData(first, second);

            first.Url = new Uri("http://example.com/");
            second.Url = new Uri("http://example.com");
            yield return new TestCaseData(first, second);
        }

        [Test, TestCaseSource(nameof(Attendees_TestCases))]
        public void Attendees_Tests(Attendee actual, Attendee expected)
        {
            Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<ITestCaseData> Attendees_TestCases()
        {
            var tentative1 = new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James Tentative",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Tentative
            };
            var tentative2 = new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James Tentative",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Tentative
            };
            yield return new TestCaseData(tentative1, tentative2).SetName("Simple attendee test case");

            var complex1 = new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Accepted",
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Accepted,
                SentBy = new Uri("mailto:someone@example.com"),
                DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"),
                Type = "CuType",
                Members = new List<string> { "Group A", "Group B"},
                Role = ParticipationRole.Chair,
                DelegatedTo = new List<string> { "Peon A", "Peon B"},
                DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B"}
            };
            var complex2 = new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Accepted",
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Accepted,
                SentBy = new Uri("mailto:someone@example.com"),
                DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"),
                Type = "CuType",
                Members = new List<string> { "Group A", "Group B" },
                Role = ParticipationRole.Chair,
                DelegatedTo = new List<string> { "Peon A", "Peon B" },
                DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B" }
            };
            yield return new TestCaseData(complex1, complex2).SetName("Complex attendee test");
        }

        [Test, TestCaseSource(nameof(CalendarCollection_TestCases))]
        public void CalendarCollection_Tests(string rawCalendar)
        {
            var a = Calendar.Load(IcsFiles.UsHolidays);
            var b = Calendar.Load(IcsFiles.UsHolidays);
            
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreEqual(a, b);
        }

        public static IEnumerable<ITestCaseData> CalendarCollection_TestCases()
        {
            yield return new TestCaseData(IcsFiles.Google1).SetName("Google calendar test case");
            yield return new TestCaseData(IcsFiles.Parse1).SetName("Weird file parse test case");
            yield return new TestCaseData(IcsFiles.UsHolidays).SetName("US Holidays (quite large)");
        }

        [Test]
        public void Resources_Tests()
        {
            var origContents = new[] { "Foo", "Bar" };
            var e = GetSimpleEvent();
            e.Resources = new List<string>(origContents);
            Assert.IsTrue(e.Resources.Count == 2);

            e.Resources.Add("Baz");
            Assert.IsTrue(e.Resources.Count == 3);
            var serialized = SerializeEvent(e);
            Assert.IsTrue(serialized.Contains("Baz"));

            e.Resources.Remove("Baz");
            Assert.IsTrue(e.Resources.Count == 2);
            serialized = SerializeEvent(e);
            Assert.IsFalse(serialized.Contains("Baz"));

            e.Resources.Add("Hello");
            Assert.IsTrue(e.Resources.Contains("Hello"));
            serialized = SerializeEvent(e);
            Assert.IsTrue(serialized.Contains("Hello"));

            e.Resources.Clear();
            e.Resources.AddRange(origContents);
            CollectionAssert.AreEquivalent(e.Resources, origContents);
            serialized = SerializeEvent(e);
            Assert.IsTrue(serialized.Contains("Foo"));
            Assert.IsTrue(serialized.Contains("Bar"));
            Assert.IsFalse(serialized.Contains("Baz"));
            Assert.IsFalse(serialized.Contains("Hello"));
        }

        internal static (byte[] original, byte[] copy) GetAttachments()
        {
            var payload = Encoding.UTF8.GetBytes("This is an attachment!");
            var payloadCopy = new byte[payload.Length];
            Array.Copy(payload, payloadCopy, payload.Length);
            return (payload, payloadCopy);
        }

        [Test, TestCaseSource(nameof(RecurringComponentAttachment_TestCases))]
        public void RecurringComponentAttachmentTests(RecurringComponent noAttachment, RecurringComponent withAttachment)
        {
            var attachments = GetAttachments();

            Assert.AreNotEqual(noAttachment, withAttachment);
            Assert.AreNotEqual(noAttachment.GetHashCode(), withAttachment.GetHashCode());

            noAttachment.Attachments.Add(new Attachment(attachments.copy));

            Assert.AreEqual(noAttachment, withAttachment);
            Assert.AreEqual(noAttachment.GetHashCode(), withAttachment.GetHashCode());
        }

        public static IEnumerable<ITestCaseData> RecurringComponentAttachment_TestCases()
        {
            var attachments = GetAttachments();

            var journalNoAttach = new Journal { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
            var journalWithAttach = new Journal { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
            journalWithAttach.Attachments.Add(new Attachment(attachments.original));
            yield return new TestCaseData(journalNoAttach, journalWithAttach).SetName("Journal recurring component attachment");

            var todoNoAttach = new Todo { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
            var todoWithAttach = new Todo { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
            todoWithAttach.Attachments.Add(new Attachment(attachments.original));
            yield return new TestCaseData(todoNoAttach, todoWithAttach).SetName("Todo recurring component attachment");

            var eventNoAttach = GetSimpleEvent();
            var eventWithAttach = GetSimpleEvent();
            eventWithAttach.Attachments.Add(new Attachment(attachments.original));
            yield return new TestCaseData(eventNoAttach, eventWithAttach).SetName("Event recurring component attachment");
        }

        [Test, TestCaseSource(nameof(PeriodTestCases))]
        public void PeriodTests(Period a, Period b)
        {
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreEqual(a, b);
        }

        public static IEnumerable<ITestCaseData> PeriodTestCases()
        {
            yield return new TestCaseData(new Period(new CalDateTime(_nowTime)), new Period(new CalDateTime(_nowTime)))
                .SetName("Two identical CalDateTimes are equal");
        }

        [Test]
        public void PeriodListTests()
        {
            var startTimesA = new List<DateTime>
            {
                new DateTime(2017, 03, 02, 06, 00, 00),
                new DateTime(2017, 03, 03, 06, 00, 00),
                new DateTime(2017, 03, 06, 06, 00, 00),
                new DateTime(2017, 03, 07, 06, 00, 00),
                new DateTime(2017, 03, 08, 06, 00, 00),
                new DateTime(2017, 03, 09, 06, 00, 00),
                new DateTime(2017, 03, 10, 06, 00, 00),
                new DateTime(2017, 03, 13, 06, 00, 00),
                new DateTime(2017, 03, 14, 06, 00, 00),
                new DateTime(2017, 03, 17, 06, 00, 00),
                new DateTime(2017, 03, 20, 06, 00, 00),
                new DateTime(2017, 03, 21, 06, 00, 00),
                new DateTime(2017, 03, 22, 06, 00, 00),
                new DateTime(2017, 03, 23, 06, 00, 00),
                new DateTime(2017, 03, 24, 06, 00, 00),
                new DateTime(2017, 03, 27, 06, 00, 00),
                new DateTime(2017, 03, 28, 06, 00, 00),
                new DateTime(2017, 03, 29, 06, 00, 00),
                new DateTime(2017, 03, 30, 06, 00, 00),
                new DateTime(2017, 03, 31, 06, 00, 00),
                new DateTime(2017, 04, 03, 06, 00, 00),
                new DateTime(2017, 04, 05, 06, 00, 00),
                new DateTime(2017, 04, 06, 06, 00, 00),
                new DateTime(2017, 04, 07, 06, 00, 00),
                new DateTime(2017, 04, 10, 06, 00, 00),
                new DateTime(2017, 04, 11, 06, 00, 00),
                new DateTime(2017, 04, 12, 06, 00, 00),
                new DateTime(2017, 04, 13, 06, 00, 00),
                new DateTime(2017, 04, 17, 06, 00, 00),
                new DateTime(2017, 04, 18, 06, 00, 00),
                new DateTime(2017, 04, 19, 06, 00, 00),
                new DateTime(2017, 04, 20, 06, 00, 00),
                new DateTime(2017, 04, 21, 06, 00, 00),
                new DateTime(2017, 04, 24, 06, 00, 00),
                new DateTime(2017, 04, 25, 06, 00, 00),
                new DateTime(2017, 04, 27, 06, 00, 00),
                new DateTime(2017, 04, 28, 06, 00, 00),
                new DateTime(2017, 05, 01, 06, 00, 00),
            }
            .Select(dt => new Period(new CalDateTime(dt))).ToList();
            var a = new PeriodList();
            foreach (var period in startTimesA)
            {
                a.Add(period);
            }

            //Difference from A: first element became the second, and last element became the second-to-last element
            var startTimesB = new List<DateTime>
            {
                new DateTime(2017, 03, 03, 06, 00, 00),
                new DateTime(2017, 03, 02, 06, 00, 00),
                new DateTime(2017, 03, 06, 06, 00, 00),
                new DateTime(2017, 03, 07, 06, 00, 00),
                new DateTime(2017, 03, 08, 06, 00, 00),
                new DateTime(2017, 03, 09, 06, 00, 00),
                new DateTime(2017, 03, 10, 06, 00, 00),
                new DateTime(2017, 03, 13, 06, 00, 00),
                new DateTime(2017, 03, 14, 06, 00, 00),
                new DateTime(2017, 03, 17, 06, 00, 00),
                new DateTime(2017, 03, 20, 06, 00, 00),
                new DateTime(2017, 03, 21, 06, 00, 00),
                new DateTime(2017, 03, 22, 06, 00, 00),
                new DateTime(2017, 03, 23, 06, 00, 00),
                new DateTime(2017, 03, 24, 06, 00, 00),
                new DateTime(2017, 03, 27, 06, 00, 00),
                new DateTime(2017, 03, 28, 06, 00, 00),
                new DateTime(2017, 03, 29, 06, 00, 00),
                new DateTime(2017, 03, 30, 06, 00, 00),
                new DateTime(2017, 03, 31, 06, 00, 00),
                new DateTime(2017, 04, 03, 06, 00, 00),
                new DateTime(2017, 04, 05, 06, 00, 00),
                new DateTime(2017, 04, 06, 06, 00, 00),
                new DateTime(2017, 04, 07, 06, 00, 00),
                new DateTime(2017, 04, 10, 06, 00, 00),
                new DateTime(2017, 04, 11, 06, 00, 00),
                new DateTime(2017, 04, 12, 06, 00, 00),
                new DateTime(2017, 04, 13, 06, 00, 00),
                new DateTime(2017, 04, 17, 06, 00, 00),
                new DateTime(2017, 04, 18, 06, 00, 00),
                new DateTime(2017, 04, 19, 06, 00, 00),
                new DateTime(2017, 04, 20, 06, 00, 00),
                new DateTime(2017, 04, 21, 06, 00, 00),
                new DateTime(2017, 04, 24, 06, 00, 00),
                new DateTime(2017, 04, 25, 06, 00, 00),
                new DateTime(2017, 04, 27, 06, 00, 00),
                new DateTime(2017, 05, 01, 06, 00, 00),
                new DateTime(2017, 04, 28, 06, 00, 00),
            }
            .Select(dt => new Period(new CalDateTime(dt))).ToList();
            var b = new PeriodList();
            foreach (var period in startTimesB)
            {
                b.Add(period);
            }

            var collectionEqual = CollectionHelpers.Equals(a, b);
            Assert.AreEqual(true, collectionEqual);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

            var listOfListA = new List<PeriodList> { a };
            var listOfListB = new List<PeriodList> { b };
            Assert.IsTrue(CollectionHelpers.Equals(listOfListA, listOfListB));

            var aThenB = new List<PeriodList> { a, b };
            var bThenA = new List<PeriodList> { b, a };
            Assert.IsTrue(CollectionHelpers.Equals(aThenB, bThenA));
        }

        [Test]
        public void CalDateTimeTests()
        {
            var nowLocal = DateTime.Now;
            var nowUtc = nowLocal.ToUniversalTime();

            var asLocal = new CalDateTime(nowLocal, "America/New_York");
            var asUtc = new CalDateTime(nowUtc, "UTC");

            Assert.AreNotEqual(asLocal, asUtc);
        }
    }
}
