namespace Ical.Net
{
    public interface ICalendarPropertyListContainer : ICalendarObject
    {
        CalendarPropertyList Properties { get; }
    }
}