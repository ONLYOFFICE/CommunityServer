namespace Ical.Net.Interfaces.Components
{
    public interface ITimeZone : ICalendarComponent
    {
        string TzId { get; set; }
    }
}