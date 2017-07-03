using System.IO;
using System.Linq;
using Ical.Net.Interfaces.Components;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace Ical.Net.UnitTests
{
    internal class SerializationHelpers
    {
        public static Event DeserializeCalendarEvent(string ical)
        {
            var calendar = DeserializeCalendar(ical);
            var calendarEvent = calendar.First().Events.First() as Event;
            return calendarEvent;
        }

        public static CalendarCollection DeserializeCalendar(string ical)
        {
            using (var reader = new StringReader(ical))
            {
                return Calendar.LoadFromStream(reader) as CalendarCollection;
            }
        }

        public static string SerializeToString(IEvent calendarEvent) => SerializeToString(new Calendar { Events = { calendarEvent } });

        public static string SerializeToString(Calendar iCalendar) => new CalendarSerializer().SerializeToString(iCalendar);
    }
}
