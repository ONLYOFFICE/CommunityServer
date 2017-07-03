using Ical.Net.General;

namespace Ical.Net.Interfaces.General
{
    public interface ICalendarPropertyListContainer : ICalendarObject
    {
        CalendarPropertyList Properties { get; }
    }
}