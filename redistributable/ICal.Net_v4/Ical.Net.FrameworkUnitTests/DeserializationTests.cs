using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class DeserializationTests
    {

        [Test, Category("Deserialization")]
        public void Attendee1()
        {
            var iCal = Calendar.Load(IcsFiles.Attendee1);
            Assert.AreEqual(1, iCal.Events.Count);
            
            var evt = iCal.Events.First();
            // Ensure there are 2 attendees
            Assert.AreEqual(2, evt.Attendees.Count);            

            var attendee1 = evt.Attendees[0];
            var attendee2 = evt.Attendees[1];

            // Values
            Assert.AreEqual(new Uri("mailto:joecool@example.com"), attendee1.Value);
            Assert.AreEqual(new Uri("mailto:ildoit@example.com"), attendee2.Value);

            // MEMBERS
            Assert.AreEqual(1, attendee1.Members.Count);
            Assert.AreEqual(0, attendee2.Members.Count);
            Assert.AreEqual(new Uri("mailto:DEV-GROUP@example.com"), attendee1.Members[0]);

            // DELEGATED-FROM
            Assert.AreEqual(0, attendee1.DelegatedFrom.Count);
            Assert.AreEqual(1, attendee2.DelegatedFrom.Count);
            Assert.AreEqual(new Uri("mailto:immud@example.com"), attendee2.DelegatedFrom[0]);

            // DELEGATED-TO
            Assert.AreEqual(0, attendee1.DelegatedTo.Count);
            Assert.AreEqual(0, attendee2.DelegatedTo.Count);
        }

        /// <summary>
        /// Tests that multiple parameters of the
        /// same name are correctly aggregated into
        /// a single list.
        /// </summary>
        [Test, Category("Deserialization")]
        public void Attendee2()
        {
            var iCal = Calendar.Load(IcsFiles.Attendee2);
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();
            // Ensure there is 1 attendee
            Assert.AreEqual(1, evt.Attendees.Count);

            var attendee1 = evt.Attendees;

            // Values
            Assert.AreEqual(new Uri("mailto:joecool@example.com"), attendee1[0].Value);

            // MEMBERS
            Assert.AreEqual(3, attendee1[0].Members.Count);
            Assert.AreEqual(new Uri("mailto:DEV-GROUP@example.com"), attendee1[0].Members[0]);
            Assert.AreEqual(new Uri("mailto:ANOTHER-GROUP@example.com"), attendee1[0].Members[1]);
            Assert.AreEqual(new Uri("mailto:THIRD-GROUP@example.com"), attendee1[0].Members[2]);
        }

        /// <summary>
        /// Tests that Lotus Notes-style properties are properly handled.
        /// https://sourceforge.net/tracker/?func=detail&aid=2033495&group_id=187422&atid=921236
        /// Sourceforge bug #2033495
        /// </summary>
        [Test, Category("Deserialization")]
        public void Bug2033495()
        {
            var iCal = Calendar.Load(IcsFiles.Bug2033495);
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(iCal.Properties["X-LOTUS-CHILD_UID"].Value, "XXX");
        }

        /// <summary>
        /// Tests bug #2938007 - involving the HasTime property in IDateTime values.
        /// See https://sourceforge.net/tracker/?func=detail&aid=2938007&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Deserialization")]
        public void Bug2938007()
        {
            var iCal = Calendar.Load(IcsFiles.Bug2938007);
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();
            Assert.AreEqual(true, evt.Start.HasTime);
            Assert.AreEqual(true, evt.End.HasTime);

            foreach (var o in evt.GetOccurrences(new CalDateTime(2010, 1, 17, 0, 0, 0), new CalDateTime(2010, 2, 1, 0, 0, 0)))
            {
                Assert.AreEqual(true, o.Period.StartTime.HasTime);
                Assert.AreEqual(true, o.Period.EndTime.HasTime);
            }
        }

        /// <summary>
        /// Tests bug #3177278 - Serialize closes stream
        /// See https://sourceforge.net/tracker/?func=detail&aid=3177278&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Deserialization")]
        public void Bug3177278()
        {
            var calendar = new Calendar();
            var serializer = new CalendarSerializer();

            var ms = new MemoryStream();
            serializer.Serialize(calendar, ms, Encoding.UTF8);

            Assert.IsTrue(ms.CanWrite);
        }

        /// <summary>
        /// Tests that a mixed-case VERSION property is loaded properly
        /// </summary>
        [Test, Category("Deserialization")]
        public void CaseInsensitive4()
        {
            var iCal = Calendar.Load(IcsFiles.CaseInsensitive4);
            Assert.AreEqual("2.5", iCal.Version);
        }

        [Test, Category("Deserialization")]
        public void Categories1_2()
        {
            var iCal = Calendar.Load(IcsFiles.Categories1);
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            var items = new List<string>();
            items.AddRange(new[]
            {
                "One", "Two", "Three",
                "Four", "Five", "Six",
                "Seven", "A string of text with nothing less than a comma, semicolon; and a newline\n."
            });

            var found = new Dictionary<string, bool>();
            foreach (var s in evt.Categories.Where(s => items.Contains(s)))
            {
                found[s] = true;
            }

            foreach (string item in items)
            {
                Assert.IsTrue(found.ContainsKey(item), "Event should contain CATEGORY '" + item + "', but it was not found.");
            }
        }

        [Test, Category("Deserialization")]
        public void EmptyLines1()
        {
            var iCal = Calendar.Load(IcsFiles.EmptyLines1);
            Assert.AreEqual(2, iCal.Events.Count, "iCalendar should have 2 events");
        }

        [Test, Category("Deserialization")]
        public void EmptyLines2()
        {
            var calendars = CalendarCollection.Load(IcsFiles.EmptyLines2);
            Assert.AreEqual(2, calendars.Count);
            Assert.AreEqual(2, calendars[0].Events.Count, "iCalendar should have 2 events");
            Assert.AreEqual(2, calendars[1].Events.Count, "iCalendar should have 2 events");
        }

        /// <summary>
        /// Verifies that blank lines between components are allowed
        /// (as occurs with some applications/parsers - i.e. KOrganizer)
        /// </summary>
        [Test, Category("Deserialization")]
        public void EmptyLines3()
        {
            var iCal = Calendar.Load(IcsFiles.EmptyLines3);
            Assert.AreEqual(1, iCal.Todos.Count, "iCalendar should have 1 todo");
        }

        /// <summary>
        /// Similar to PARSE4 and PARSE5 tests.
        /// </summary>
        [Test, Category("Deserialization")]
        public void EmptyLines4()
        {
            var iCal = Calendar.Load(IcsFiles.EmptyLines4);
            Assert.AreEqual(28, iCal.Events.Count);
        }

        [Test]
        public void Encoding2()
        {
            var iCal = Calendar.Load(IcsFiles.Encoding2);
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            Assert.AreEqual(
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.",
                evt.Attachments[0].ToString(),
                "Attached value does not match.");
        }

        [Test]
        public void Encoding3()
        {
            var iCal = Calendar.Load(IcsFiles.Encoding3);
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            Assert.AreEqual("uuid1153170430406", evt.Uid, "UID should be 'uuid1153170430406'; it is " + evt.Uid);
            Assert.AreEqual(1, evt.Sequence, "SEQUENCE should be 1; it is " + evt.Sequence);
        }

        [Test, Category("Deserialization")]
        public void Event8()
        {
            var sr = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Apple Computer\, Inc//iCal 1.0//EN
CALSCALE:GREGORIAN
BEGIN:VEVENT
CREATED:20070404T211714Z
DTEND:20070407T010000Z
DTSTAMP:20070404T211714Z
DTSTART:20070406T230000Z
DURATION:PT2H
RRULE:FREQ=WEEKLY;UNTIL=20070801T070000Z;BYDAY=FR
SUMMARY:Friday Meetings
DTSTAMP:20040103T033800Z
SEQUENCE:1
UID:fd940618-45e2-4d19-b118-37fd7a8e3906
END:VEVENT
BEGIN:VEVENT
CREATED:20070404T204310Z
DTEND:20070416T030000Z
DTSTAMP:20070404T204310Z
DTSTART:20070414T200000Z
DURATION:P1DT7H
RRULE:FREQ=DAILY;COUNT=12;BYDAY=SA,SU
SUMMARY:Weekend Yea!
DTSTAMP:20040103T033800Z
SEQUENCE:1
UID:ebfbd3e3-cc1e-4a64-98eb-ced2598b3908
END:VEVENT
END:VCALENDAR
";
            var iCal = Calendar.Load(sr);
            Assert.IsTrue(iCal.Events.Count == 2, "There should be 2 events in the parsed calendar");
            Assert.IsNotNull(iCal.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], "Event fd940618-45e2-4d19-b118-37fd7a8e3906 should exist in the calendar");
            Assert.IsNotNull(iCal.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], "Event ebfbd3e3-cc1e-4a64-98eb-ced2598b3908 should exist in the calendar");
        }

        [Test]
        public void GeographicLocation1_2()
        {
            var iCal = Calendar.Load(IcsFiles.GeographicLocation1);
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            Assert.AreEqual(37.386013, evt.GeographicLocation.Latitude, "Latitude should be 37.386013; it is not.");
            Assert.AreEqual(-122.082932, evt.GeographicLocation.Longitude, "Longitude should be -122.082932; it is not.");
        }

        [Test, Category("Deserialization")]
        public void Google1()
        {
            var tzId = "Europe/Berlin";
            var iCal = Calendar.Load(IcsFiles.Google1);
            var evt = iCal.Events["594oeajmftl3r9qlkb476rpr3c@google.com"];
            Assert.IsNotNull(evt);

            IDateTime dtStart = new CalDateTime(2006, 12, 18, tzId);
            IDateTime dtEnd = new CalDateTime(2006, 12, 23, tzId);
            var occurrences = iCal.GetOccurrences(dtStart, dtEnd).OrderBy(o => o.Period.StartTime).ToList();

            var dateTimes = new[]
            {
                new CalDateTime(2006, 12, 18, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 19, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 20, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 21, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 22, 7, 0, 0, tzId)
            };

            for (var i = 0; i < dateTimes.Length; i++)
                Assert.AreEqual(dateTimes[i], occurrences[i].Period.StartTime, "Event should occur at " + dateTimes[i]);

            Assert.AreEqual(dateTimes.Length, occurrences.Count, "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests that valid RDATE properties are parsed correctly.
        /// </summary>
        [Test, Category("Deserialization")]
        public void RecurrenceDates1()
        {
            var iCal = Calendar.Load(IcsFiles.RecurrenceDates1);
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(3, iCal.Events.First().RecurrenceDates.Count);
            
            Assert.AreEqual((CalDateTime)new DateTime(1997, 7, 14, 12, 30, 0, DateTimeKind.Utc), iCal.Events.First().RecurrenceDates[0][0].StartTime);
            Assert.AreEqual((CalDateTime)new DateTime(1996, 4, 3, 2, 0, 0, DateTimeKind.Utc), iCal.Events.First().RecurrenceDates[1][0].StartTime);
            Assert.AreEqual((CalDateTime)new DateTime(1996, 4, 3, 4, 0, 0, DateTimeKind.Utc), iCal.Events.First().RecurrenceDates[1][0].EndTime);
            Assert.AreEqual(new CalDateTime(1997, 1, 1), iCal.Events.First().RecurrenceDates[2][0].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 1, 20), iCal.Events.First().RecurrenceDates[2][1].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 2, 17), iCal.Events.First().RecurrenceDates[2][2].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 4, 21), iCal.Events.First().RecurrenceDates[2][3].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 5, 26), iCal.Events.First().RecurrenceDates[2][4].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 7, 4), iCal.Events.First().RecurrenceDates[2][5].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 9, 1), iCal.Events.First().RecurrenceDates[2][6].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 10, 14), iCal.Events.First().RecurrenceDates[2][7].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 11, 28), iCal.Events.First().RecurrenceDates[2][8].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 11, 29), iCal.Events.First().RecurrenceDates[2][9].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 12, 25), iCal.Events.First().RecurrenceDates[2][10].StartTime);
        }

        /// <summary>
        /// Tests that valid REQUEST-STATUS properties are parsed correctly.
        /// </summary>
        [Test, Category("Deserialization")]
        public void RequestStatus1()
        {
            var iCal = Calendar.Load(IcsFiles.RequestStatus1);
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(4, iCal.Events.First().RequestStatuses.Count);

            var rs = iCal.Events.First().RequestStatuses[0];
            Assert.AreEqual(2, rs.StatusCode.Primary);
            Assert.AreEqual(0, rs.StatusCode.Secondary);
            Assert.AreEqual("Success", rs.Description);
            Assert.IsNull(rs.ExtraData);

            rs = iCal.Events.First().RequestStatuses[1];
            Assert.AreEqual(3, rs.StatusCode.Primary);
            Assert.AreEqual(1, rs.StatusCode.Secondary);
            Assert.AreEqual("Invalid property value", rs.Description);
            Assert.AreEqual("DTSTART:96-Apr-01", rs.ExtraData);

            rs = iCal.Events.First().RequestStatuses[2];
            Assert.AreEqual(2, rs.StatusCode.Primary);
            Assert.AreEqual(8, rs.StatusCode.Secondary);
            Assert.AreEqual(" Success, repeating event ignored. Scheduled as a single event.", rs.Description);
            Assert.AreEqual("RRULE:FREQ=WEEKLY;INTERVAL=2", rs.ExtraData);

            rs = iCal.Events.First().RequestStatuses[3];
            Assert.AreEqual(4, rs.StatusCode.Primary);
            Assert.AreEqual(1, rs.StatusCode.Secondary);
            Assert.AreEqual("Event conflict. Date/time is busy.", rs.Description);
            Assert.IsNull(rs.ExtraData);
        }

        /// <summary>
        /// Tests that string escaping works with Text elements.
        /// </summary>
        [Test, Category("Deserialization")]
        public void String2()
        {
            var serializer = new StringSerializer();
            var value = @"test\with\;characters";
            var unescaped = (string)serializer.Deserialize(new StringReader(value));

            Assert.AreEqual(@"test\with;characters", unescaped, "String unescaping was incorrect.");

            value = @"C:\Path\To\My\New\Information";
            unescaped = (string)serializer.Deserialize(new StringReader(value));
            Assert.AreEqual("C:\\Path\\To\\My\new\\Information", unescaped, "String unescaping was incorrect.");

            value = @"\""This\r\nis\Na\, test\""\;\\;,";
            unescaped = (string)serializer.Deserialize(new StringReader(value));

            Assert.AreEqual("\"This\\r\nis\na, test\";\\;,", unescaped, "String unescaping was incorrect.");
        }

        [Test, Category("Deserialization")]
        public void Transparency2()
        {
            var iCal = Calendar.Load(IcsFiles.Transparency2);

            Assert.AreEqual(1, iCal.Events.Count);
            var evt = iCal.Events.First();

            Assert.AreEqual(TransparencyType.Transparent, evt.Transparency);
        }

        /// <summary>
        /// Tests that DateTime values that are out-of-range are still parsed correctly
        /// and set to the closest representable date/time in .NET.
        /// </summary>
        [Test, Category("Deserialization")]
        public void DateTime1()
        {
            var iCal = Calendar.Load(IcsFiles.DateTime1);
            Assert.AreEqual(6, iCal.Events.Count);

            var evt = iCal.Events["nc2o66s0u36iesitl2l0b8inn8@google.com"];
            Assert.IsNotNull(evt);

            // The "Created" date is out-of-bounds.  It should be coerced to the
            // closest representable date/time.
            Assert.AreEqual(DateTime.MinValue, evt.Created.Value);
        }

        [Test, Category("Deserialization")]
        public void Language4()
        {
            var iCal = Calendar.Load(IcsFiles.Language4);
            Assert.IsNotNull(iCal);
        }

        [Test, Category("Deserialization")]
        public void Outlook2007_LineFolds1()
        {
            var iCal = Calendar.Load(IcsFiles.Outlook2007LineFolds);
            var events = iCal.GetOccurrences(new CalDateTime(2009, 06, 20), new CalDateTime(2009, 06, 22));
            Assert.AreEqual(1, events.Count);
        }

        [Test, Category("Deserialization")]
        public void Outlook2007_LineFolds2()
        {
            var longName = "The Exceptionally Long Named Meeting Room Whose Name Wraps Over Several Lines When Exported From Leading Calendar and Office Software Application Microsoft Office 2007";
            var iCal = Calendar.Load(IcsFiles.Outlook2007LineFolds);
            var events = iCal.GetOccurrences<CalendarEvent>(new CalDateTime(2009, 06, 20), new CalDateTime(2009, 06, 22)).OrderBy(o => o.Period.StartTime).ToList();
            Assert.AreEqual(longName, ((CalendarEvent)events[0].Source).Location);
        }

        /// <summary>
        /// Tests that multiple parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Deserialization")]
        public void Parameter1()
        {
            var iCal = Calendar.Load(IcsFiles.Parameter1);

            var evt = iCal.Events.First();
            IList<CalendarParameter> parms = evt.Properties["DTSTART"].Parameters.AllOf("VALUE").ToList();
            Assert.AreEqual(2, parms.Count);
            Assert.AreEqual("DATE", parms[0].Values.First());
            Assert.AreEqual("OTHER", parms[1].Values.First());
        }

        /// <summary>
        /// Tests that empty parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Deserialization")]
        public void Parameter2()
        {
            var iCal = Calendar.Load(IcsFiles.Parameter2);
            Assert.AreEqual(2, iCal.Events.Count);
        }

        /// <summary>
        /// Tests a calendar that should fail to properly parse.
        /// </summary>
        [Test, Category("Deserialization")]
        public void Parse1()
        {
            try
            {
                var content = IcsFiles.Parse1;
                var iCal = Calendar.Load(content);
                Assert.IsNotNull(iCal);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<SerializationException>(e);
            }
        }

        /// <summary>
        /// Tests that multiple properties are allowed in iCalObjects
        /// </summary>
        [Test, Category("Deserialization")]
        public void Property1()
        {
            var iCal = Calendar.Load(IcsFiles.Property1);

            IList<ICalendarProperty> props = iCal.Properties.AllOf("VERSION").ToList();
            Assert.AreEqual(2, props.Count);

            for (var i = 0; i < props.Count; i++)
            {
                Assert.AreEqual("2." + i, props[i].Value);
            }
        }
    }
}
