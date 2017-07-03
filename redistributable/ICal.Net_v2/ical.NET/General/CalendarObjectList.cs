using Ical.Net.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    /// <summary>
    /// A collection of calendar objects.
    /// </summary>
    public class CalendarObjectList : GroupedList<string, ICalendarObject>, ICalendarObjectList<ICalendarObject>
    {
        public CalendarObjectList(ICalendarObject parent) {}
    }
}