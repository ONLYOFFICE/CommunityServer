using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class SerializationTests
    {
        private static readonly DateTime _nowTime = DateTime.Now;
        private static readonly DateTime _later = _nowTime.AddHours(1);
        private static CalendarSerializer GetNewSerializer() => new CalendarSerializer();
        private static string SerializeToString(Calendar c) => GetNewSerializer().SerializeToString(c);
        private static string SerializeToString(CalendarEvent e) => SerializeToString(new Calendar { Events = { e } });
        private static CalendarEvent GetSimpleEvent() => new CalendarEvent { DtStart = new CalDateTime(_nowTime), DtEnd = new CalDateTime(_later), Duration = _later - _nowTime };
        private static Calendar UnserializeCalendar(string s) => Calendar.Load(s);

        public static void CompareCalendars(Calendar cal1, Calendar cal2)
        {
            CompareComponents(cal1, cal2);

            Assert.AreEqual(cal1.Children.Count, cal2.Children.Count, "Children count is different between calendars.");

            for (var i = 0; i < cal1.Children.Count; i++)
            {
                var component1 = cal1.Children[i] as ICalendarComponent;
                var component2 = cal2.Children[i] as ICalendarComponent;
                if (component1 != null && component2 != null)
                {
                    CompareComponents(component1, component2);
                }
            }
        }

        public static void CompareComponents(ICalendarComponent cb1, ICalendarComponent cb2)
        {
            foreach (var p1 in cb1.Properties)
            {
                var isMatch = false;
                foreach (var p2 in cb2.Properties.AllOf(p1.Name))
                {
                    try
                    {
                        Assert.AreEqual(p1, p2, "The properties '" + p1.Name + "' are not equal.");
                        if (p1.Value is IComparable)
                        {
                            Assert.AreEqual(0, ((IComparable)p1.Value).CompareTo(p2.Value), "The '" + p1.Name + "' property values do not match.");
                        }
                        else if (p1.Value is IEnumerable)
                        {
                            CompareEnumerables((IEnumerable)p1.Value, (IEnumerable)p2.Value, p1.Name);
                        }
                        else
                        {
                            Assert.AreEqual(p1.Value, p2.Value, "The '" + p1.Name + "' property values are not equal.");
                        }

                        isMatch = true;
                        break;
                    }
                    catch { }
                }

                Assert.IsTrue(isMatch, "Could not find a matching property - " + p1.Name + ":" + (p1.Value?.ToString() ?? string.Empty));
            }

            Assert.AreEqual(cb1.Children.Count, cb2.Children.Count, "The number of children are not equal.");
            for (var i = 0; i < cb1.Children.Count; i++)
            {
                var child1 = cb1.Children[i] as ICalendarComponent;
                var child2 = cb2.Children[i] as ICalendarComponent;
                if (child1 != null && child2 != null)
                {
                    CompareComponents(child1, child2);
                }
                else
                {
                    Assert.AreEqual(child1, child2, "The child objects are not equal.");
                }
            }
        }

        public static void CompareEnumerables(IEnumerable a1, IEnumerable a2, string value)
        {
            if (a1 == null && a2 == null)
            {
                return;
            }

            Assert.IsFalse((a1 == null && a2 != null) || (a1 != null && a2 == null), value + " do not match - one item is null");

            var enum1 = a1.GetEnumerator();
            var enum2 = a2.GetEnumerator();

            while (enum1.MoveNext() && enum2.MoveNext())
            {
                Assert.AreEqual(enum1.Current, enum2.Current, value + " do not match");
            }
        }

        public static string InspectSerializedSection(string serialized, string sectionName, IEnumerable<string> elements)
        {
            const string notFound = "expected '{0}' not found";
            var searchFor = "BEGIN:" + sectionName;
            var begin = serialized.IndexOf(searchFor);
            Assert.AreNotEqual(-1, begin, notFound, searchFor);

            searchFor = "END:" + sectionName;
            var end = serialized.IndexOf(searchFor, begin);
            Assert.AreNotEqual(-1, end, notFound, searchFor);

            var searchRegion = serialized.Substring(begin, end - begin + 1);

            foreach (var e in elements)
            {
                Assert.IsTrue(searchRegion.Contains(SerializationConstants.LineBreak + e + SerializationConstants.LineBreak), notFound, e);
            }

            return searchRegion;
        }

        //3 formats - UTC, local time as defined in vTimeZone, and floating,
        //at some point it would be great to independently unit test string serialization of an IDateTime object, into its 3 forms
        //http://www.kanzaki.com/docs/ical/dateTime.html
        static string CalDateString(IDateTime cdt)
        {
            var returnVar = $"{cdt.Year}{cdt.Month:D2}{cdt.Day:D2}T{cdt.Hour:D2}{cdt.Minute:D2}{cdt.Second:D2}";
            if (cdt.IsUtc)
            {
                return returnVar + 'Z';
            }

            return string.IsNullOrEmpty(cdt.TzId)
                ? returnVar
                : $"TZID={cdt.TzId}:{returnVar}";
        }

        //This method needs renaming
        static Dictionary<string, string> GetValues(string serialized, string name, string value)
        {
            var lengthened = serialized.Replace(SerializationConstants.LineBreak + ' ', string.Empty);
            //using a regex for now - for the sake of speed, it may be worth creating a C# text search later
            var match = Regex.Match(lengthened, '^' + Regex.Escape(name) + "(;.+)?:" + Regex.Escape(value) + SerializationConstants.LineBreak, RegexOptions.Multiline);
            Assert.IsTrue(match.Success, $"could not find a(n) '{name}' with value '{value}'");
            return match.Groups[1].Value.Length == 0
                ? new Dictionary<string, string>()
                : match.Groups[1].Value.Substring(1).Split(';').Select(v => v.Split('=')).ToDictionary(v => v[0], v => v.Length > 1 ? v[1] : null);
        }

        [Test, Category("Serialization"), Ignore("TODO: standard time, for NZ standard time (current example)")]
        public void TimeZoneSerialize()
        {
            //ToDo: This test is broken as of 2016-07-13
            var cal = new Calendar
            {
                Method = "PUBLISH",
                Version = "2.0"
            };

            const string exampleTz = "New Zealand Standard Time";
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(exampleTz);
            var tz = new VTimeZone(exampleTz);
            cal.AddTimeZone(tz);
            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2016, 7, 14, tz.TzId),
                End = new CalDateTime(2016, 7, 15, tz.TzId)
            };
            cal.Events.Add(evt);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(cal);

            Console.Write(serializedCalendar);

            var vTimezone = InspectSerializedSection(serializedCalendar, "VTIMEZONE", new[] { "TZID:" + tz.TzId });
            var o = tzi.BaseUtcOffset.ToString("hhmm", CultureInfo.InvariantCulture);

            InspectSerializedSection(vTimezone, "STANDARD", new[] {"TZNAME:" + tzi.StandardName, "TZOFFSETTO:" + o
                //"DTSTART:20150402T030000",
                //"RRULE:FREQ=YEARLY;BYDAY=1SU;BYHOUR=3;BYMINUTE=0;BYMONTH=4",
                //"TZOFFSETFROM:+1300"
            });


            InspectSerializedSection(vTimezone, "DAYLIGHT", new[] { "TZNAME:" + tzi.DaylightName, "TZOFFSETFROM:" + o });
        }
        [Test, Category("Serialization")]
        public void SerializeDeserialize()
        {
            //ToDo: This test is broken as of 2016-07-13
            var cal1 = new Calendar
            {
                Method = "PUBLISH",
                Version = "2.0"
            };

            var evt = new CalendarEvent
            {
                Class = "PRIVATE",
                Created = new CalDateTime(2010, 3, 25, 12, 53, 35),
                DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
                LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
                Sequence = 0,
                Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
                Priority = 5,
                Location = "here",
                Summary = "test",
                DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
                DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
            };
            cal1.Events.Add(evt);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(cal1);
            var cal2 = Calendar.Load(serializedCalendar);
            CompareCalendars(cal1, cal2);
        }

        [Test, Category("Serialization")]
        public void EventPropertiesSerialized()
        {
            //ToDo: This test is broken as of 2016-07-13
            var cal = new Calendar
            {
                Method = "PUBLISH",
                Version = "2.0"
            };

            var evt = new CalendarEvent
            {
                Class = "PRIVATE",
                Created = new CalDateTime(2010, 3, 25, 12, 53, 35),
                DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
                LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
                Sequence = 0,
                Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
                Priority = 5,
                Location = "here",
                Summary = "test",
                DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
                DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
                //not yet testing property below as serialized output currently does not comply with RTFC 2445
                //Transparency = TransparencyType.Opaque,
                //Status = EventStatus.Confirmed
            };
            cal.Events.Add(evt);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(cal);

            Console.Write(serializedCalendar);
            Assert.IsTrue(serializedCalendar.StartsWith("BEGIN:VCALENDAR"));
            Assert.IsTrue(serializedCalendar.EndsWith("END:VCALENDAR" + SerializationConstants.LineBreak));

            var expectProperties = new[] { "METHOD:PUBLISH", "VERSION:2.0" };

            foreach (var p in expectProperties)
            {
                Assert.IsTrue(serializedCalendar.Contains(SerializationConstants.LineBreak + p + SerializationConstants.LineBreak), "expected '" + p + "' not found");
            }

            InspectSerializedSection(serializedCalendar, "VEVENT",
                new[]
                {
                    "CLASS:" + evt.Class, "CREATED:" + CalDateString(evt.Created), "DTSTAMP:" + CalDateString(evt.DtStamp),
                    "LAST-MODIFIED:" + CalDateString(evt.LastModified), "SEQUENCE:" + evt.Sequence, "UID:" + evt.Uid, "PRIORITY:" + evt.Priority,
                    "LOCATION:" + evt.Location, "SUMMARY:" + evt.Summary, "DTSTART:" + CalDateString(evt.DtStart), "DTEND:" + CalDateString(evt.DtEnd)
                    //"TRANSPARENCY:" + TransparencyType.Opaque.ToString().ToUpperInvariant(),
                    //"STATUS:" + EventStatus.Confirmed.ToString().ToUpperInvariant()
                });
        }

        private static readonly IList<Attendee> _attendees = new List<Attendee>
        {
            new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James James",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Tentative
            },
            new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Mary",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Accepted
            }
        }.AsReadOnly();

        [Test, Category("Serialization")]
        public void AttendeesSerialized()
        {
            //ToDo: This test is broken as of 2016-07-13
            var cal = new Calendar
            {
                Method = "REQUEST",
                Version = "2.0"
            };

            var evt = AttendeeTest.VEventFactory();
            cal.Events.Add(evt);
            const string org = "MAILTO:james@example.com";
            evt.Organizer = new Organizer(org);

            evt.Attendees.AddRange(_attendees);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(cal);

            Console.Write(serializedCalendar);

            var vEvt = InspectSerializedSection(serializedCalendar, "VEVENT", new[] { "ORGANIZER:" + org });

            foreach (var a in evt.Attendees)
            {
                var vals = GetValues(vEvt, "ATTENDEE", a.Value.OriginalString);
                foreach (var v in new Dictionary<string, string>
                {
                    ["CN"] = a.CommonName,
                    ["ROLE"] = a.Role,
                    ["RSVP"] = a.Rsvp.ToString()
                        .ToUpperInvariant(),
                    ["PARTSTAT"] = a.ParticipationStatus
                })
                {
                    Assert.IsTrue(vals.ContainsKey(v.Key), $"could not find key '{v.Key}'");
                    Assert.AreEqual(v.Value, vals[v.Key], $"ATENDEE prop '{v.Key}' differ");
                }
            }
        }

        //todo test event:
        //-GeographicLocation
        //-Alarm

        [Test]
        public void ZeroTimeSpan_Test()
        {
            var result = new TimeSpanSerializer().SerializeToString(TimeSpan.Zero);
            Assert.IsTrue("P0D".Equals(result, StringComparison.Ordinal));
        }

        [Test]
        public void DurationIsStable_Tests()
        {
            var e = GetSimpleEvent();
            var originalDuration = e.Duration;
            var c = new Calendar();
            c.Events.Add(e);
            var serialized = SerializeToString(c);
            Assert.AreEqual(originalDuration, e.Duration);
            Assert.IsTrue(!serialized.Contains("DURATION"));
        }

        [Test]
        public void EventStatusAllCaps()
        {
            var e = GetSimpleEvent();
            e.Status = EventStatus.Confirmed;
            var serialized = SerializeToString(e);
            Assert.IsTrue(serialized.Contains(EventStatus.Confirmed, EventStatus.Comparison));

            var calendar = UnserializeCalendar(serialized);
            var eventStatus = calendar.Events.First().Status;
            Assert.IsTrue(string.Equals(EventStatus.Confirmed, eventStatus, EventStatus.Comparison));
        }

        [Test]
        public void ToDoStatusAllCaps()
        {
            var component = new Todo
            {
                Status = TodoStatus.NeedsAction
            };

            var c = new Calendar { Todos = { component } };
            var serialized = SerializeToString(c);
            Assert.IsTrue(serialized.Contains(TodoStatus.NeedsAction, TodoStatus.Comparison));

            var calendar = UnserializeCalendar(serialized);
            var status = calendar.Todos.First().Status;
            Assert.IsTrue(string.Equals(TodoStatus.NeedsAction, status, TodoStatus.Comparison));
        }

        [Test]
        public void JournalStatusAllCaps()
        {
            var component = new Journal
            {
                Status = JournalStatus.Final,
            };

            var c = new Calendar { Journals = { component } };
            var serialized = SerializeToString(c);
            Assert.IsTrue(serialized.Contains(JournalStatus.Final, JournalStatus.Comparison));

            var calendar = UnserializeCalendar(serialized);
            var status = calendar.Journals.First().Status;
            Assert.IsTrue(string.Equals(JournalStatus.Final, status, JournalStatus.Comparison));
        }

        [Test]
        public void UnicodeDescription()
        {
            const string ics = @"BEGIN:VEVENT
DTSTAMP:20171120T124856Z
DTSTART;TZID=Europe/Helsinki:20160707T110000
DTEND;TZID=Europe/Helsinki:20160707T140000
SUMMARY:Some summary
UID:20160627T123608Z-182847102@atlassian.net
DESCRIPTION:Key points:\n•	Some text (text,
 , text\, text\, TP) some text\;\n•	some tex
 t Some text (Text\, Text)\;\n•	Some tex
 t some text\, some text\, text.\;\n\nsome te
 xt some tex‘t some text. 
ORGANIZER;X-CONFLUENCE-USER-KEY=ff801df01547101c6720006;CN=Some
 user;CUTYPE=INDIVIDUAL:mailto:some.mail@domain.com
CREATED:20160627T123608Z
LAST-MODIFIED:20160627T123608Z
ATTENDEE;X-CONFLUENCE-USER-KEY=ff8080ef1df01547101c6720006;CN=Some
 text;CUTYPE=INDIVIDUAL:mailto:some.mail@domain.com
SEQUENCE:1
X-CONFLUENCE-SUBCALENDAR-TYPE:other
TRANSP:TRANSPARENT
STATUS:CONFIRMED
END:VEVENT";
            var deserializedEvent = Calendar.Load<CalendarEvent>(ics).Single();

            Assert.IsTrue(deserializedEvent.Description.Contains("\t"));
            Assert.IsTrue(deserializedEvent.Description.Contains("•"));
            Assert.IsTrue(deserializedEvent.Description.Contains("‘"));
        }
    }
}
