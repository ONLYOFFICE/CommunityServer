using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.FrameworkUnitTests
{
    public class VTimeZoneTest
    {
        [Test, Category("VTimeZone")]
        public void InvalidTzIdShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => new VTimeZone("shouldFail"));
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneFromDateTimeZoneNullZoneShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => CreateTestCalendar("shouldFail"));
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaPhoenixShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Phoenix");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Phoenix"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("DTSTART:19670430T020000"), "Daylight savings for Phoenix was not serialized properly.");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaPhoenixShouldSerializeProperly2()
        {
            var iCal = CreateTestCalendar("America/Phoenix", DateTime.Now, false);
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Phoenix"), "Time zone not found in serialization");
            Assert.IsFalse(serialized.Contains("BEGIN:DAYLIGHT"), "Daylight savings should not exist for Phoenix.");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneUsMountainStandardTimeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("US Mountain Standard Time");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:US Mountain Standard Time"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"));
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"));
            Assert.IsTrue(serialized.Contains("X-LIC-LOCATION"), "X-LIC-LOCATION was not serialized");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneCentralAmericaStandardTimeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Central America Standard Time");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:Central America Standard Time"), "Time zone not found in serialization");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneEasternStandardTimeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Eastern Standard Time");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:Eastern Standard Time"), "Time zone not found in serialization");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneEuropeMoscowShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Europe/Moscow");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:Europe/Moscow"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");

            Assert.IsTrue(serialized.Contains("TZNAME:MSD"), "MSD was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:MSK"), "MSK info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:MSD"), "MSD was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:MST"), "MST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:MMT"), "MMT was not serialized");
            Assert.IsTrue(serialized.Contains("TZOFFSETFROM:+023017"), "TZOFFSETFROM:+023017 was not serialized");
            Assert.IsTrue(serialized.Contains("TZOFFSETTO:+023017"), "TZOFFSETTO:+023017 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19180916T010000"), "DTSTART:19180916T010000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19171228T000000"), "DTSTART:19171228T000000 was not serialized");
            Assert.IsTrue(serialized.Contains("RDATE:19991031T030000"), "RDATE:19991031T030000 was not serialized");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaChicagoShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Chicago");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Chicago"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:CDT"), "CDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:CST"), "CST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EST"), "EST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:CWT"), "CWT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:CPT"), "CPT was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19181027T020000"), "DTSTART:19181027T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19450814T180000"), "DTSTART:19450814T180000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19420209T020000"), "DTSTART:19420209T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19360301T020000"), "DTSTART:19360301T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaLosAngelesShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Los_Angeles");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Los_Angeles"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BYDAY=2SU"), "BYDAY=2SU was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:PDT"), "PDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:PST"), "PST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:PPT"), "PPT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:PWT"), "PWT was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19180331T020000"), "DTSTART:19180331T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");

            //Assert.IsTrue(serialized.Contains("TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles"), "TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles was not serialized");
            //Assert.IsTrue(serialized.Contains("RDATE:19600424T010000"), "RDATE:19600424T010000 was not serialized");  // NodaTime doesn't match with what tzurl has
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaAnchorageShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Anchorage");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Anchorage"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AHST"), "AHST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AHDT"), "AHDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AKST"), "AKST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:YST"), "YST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AHDT"), "AHDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:LMT"), "LMT was not serialized");
            Assert.IsTrue(serialized.Contains("RDATE:19731028T020000"), "RDATE:19731028T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("RDATE:19801026T020000"), "RDATE:19801026T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19420209T020000"), "DTSTART:19420209T020000 was not serialized");
            Assert.IsFalse(serialized.Contains("RDATE:19670401/P1D"), "RDate was not properly serialized for vtimezone, should be RDATE:19670401T000000");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaEirunepeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Eirunepe");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Eirunepe"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:-04"), "-04 was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:-05"), "-05 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19311003T110000"), "DTSTART:19311003T110000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19320401T000000"), "DTSTART:19320401T000000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20080624T000000"), "DTSTART:20080624T000000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19501201T000000"), "DTSTART:19501201T000000 was not serialized");
            
            // Should not contain the following
            Assert.IsFalse(serialized.Contains("RDATE:19501201T000000/P1D"), "The RDATE was not serialized correctly, should be RDATE:19501201T000000");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaDetroitShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Detroit");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Detroit"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EDT"), "EDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EPT"), "EPT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EST"), "EST was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
        }

        private static Calendar CreateTestCalendar(string tzId, DateTime? earliestTime = null, bool includeHistoricalData = true)
        {
            var iCal = new Calendar();

            if (earliestTime == null)
            {
                earliestTime = new DateTime(1900, 1, 1);
            }
            iCal.AddTimeZone(tzId, earliestTime.Value, includeHistoricalData);

            var calEvent = new CalendarEvent
            {
                Description = "Test Recurring Event",
                Start = new CalDateTime(DateTime.Now, tzId),
                End = new CalDateTime(DateTime.Now.AddHours(1), tzId),
                RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily) }
            };
            iCal.Events.Add(calEvent);

            var calEvent2 = new CalendarEvent
            {
                Description = "Test Recurring Event 2",
                Start = new CalDateTime(DateTime.Now.AddHours(2), tzId),
                End = new CalDateTime(DateTime.Now.AddHours(3), tzId),
                RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily) }
            };
            iCal.Events.Add(calEvent2);
            return iCal;
        }
    }
}