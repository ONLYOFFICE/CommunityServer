using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Interfaces;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    public class ConcurrentDeserializationTests
    {
        [Test]
        public void ConcurrentDeserialization_Test()
        {
            // https://github.com/rianjs/ical.net/issues/40
            var calendars = new List<string>
            {
                IcsFiles.DailyCount2,
                IcsFiles.DailyInterval2,
                IcsFiles.DailyByDay1,
                IcsFiles.RecurrenceDates1,
                IcsFiles.DailyByHourMinute1,
            };

            var deserializedCalendars = calendars.AsParallel().SelectMany(c =>
            {
                IICalendarCollection calendar;
                using (var reader = new StringReader(c ?? string.Empty))
                {
                    calendar = Calendar.LoadFromStream(reader);
                }
                return calendar;
            });

            var materialized = deserializedCalendars.ToList();
            Assert.AreEqual(5, materialized.Count);
        }
    }
}
