using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class CopyTest
    {
        [Test, TestCaseSource(nameof(CopyCalendarTest_TestCases)), Category("Copy tests")]
        public void CopyCalendarTest(string calendarString)
        {
            var iCal1 = Calendar.Load(calendarString);
            var iCal2 = iCal1.Copy<Calendar>();
            SerializationTests.CompareCalendars(iCal1, iCal2);
        }

        public static IEnumerable<ITestCaseData> CopyCalendarTest_TestCases()
        {
            yield return new TestCaseData(IcsFiles.Attachment3).SetName("Attachment3");
            yield return new TestCaseData(IcsFiles.Bug2148092).SetName("Bug2148092");
            yield return new TestCaseData(IcsFiles.CaseInsensitive1).SetName("CaseInsensitive1");
            yield return new TestCaseData(IcsFiles.CaseInsensitive2).SetName("CaseInsensitive2");
            yield return new TestCaseData(IcsFiles.CaseInsensitive3).SetName("CaseInsensitive3");
            yield return new TestCaseData(IcsFiles.Categories1).SetName("Categories1");
            yield return new TestCaseData(IcsFiles.Duration1).SetName("Duration1");
            yield return new TestCaseData(IcsFiles.Encoding1).SetName("Encoding1");
            yield return new TestCaseData(IcsFiles.Event1).SetName("Event1");
            yield return new TestCaseData(IcsFiles.Event2).SetName("Event2");
            yield return new TestCaseData(IcsFiles.Event3).SetName("Event3");
            yield return new TestCaseData(IcsFiles.Event4).SetName("Event4");
            yield return new TestCaseData(IcsFiles.GeographicLocation1).SetName("GeographicLocation1");
            yield return new TestCaseData(IcsFiles.Language1).SetName("Language1");
            yield return new TestCaseData(IcsFiles.Language2).SetName("Language2");
            yield return new TestCaseData(IcsFiles.Language3).SetName("Language3");
            yield return new TestCaseData(IcsFiles.TimeZone1).SetName("TimeZone1");
            yield return new TestCaseData(IcsFiles.TimeZone2).SetName("TimeZone2");
            yield return new TestCaseData(IcsFiles.TimeZone3).SetName("TimeZone3");
            yield return new TestCaseData(IcsFiles.XProperty1).SetName("XProperty1");
            yield return new TestCaseData(IcsFiles.XProperty2).SetName("XProperty2");
        }

        private static readonly DateTime _now = DateTime.Now;
        private static readonly DateTime _later = _now.AddHours(1);

        private static CalendarEvent GetSimpleEvent() => new CalendarEvent
        {
            DtStart = new CalDateTime(_now),
            DtEnd = new CalDateTime(_later),
            Duration = TimeSpan.FromHours(1),
        };

        private static string SerializeEvent(CalendarEvent e) => new CalendarSerializer().SerializeToString(new Calendar { Events = { e } });

        [Test]
        public void EventUid_Tests()
        {
            var e = GetSimpleEvent();
            e.Uid = "Hello";
            var copy = e.Copy<CalendarEvent>();
            Assert.AreEqual(e.Uid, copy.Uid);

            copy.Uid = "Goodbye";

            const string uidPattern = "UID:";
            var serializedOrig = SerializeEvent(e);
            Assert.AreEqual(1, Regex.Matches(serializedOrig, uidPattern).Count);

            var serializedCopy = SerializeEvent(copy);
            Assert.AreEqual(1, Regex.Matches(serializedCopy, uidPattern).Count);
        }
    }
}
