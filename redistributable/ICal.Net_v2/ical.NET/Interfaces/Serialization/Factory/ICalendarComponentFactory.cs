using Ical.Net.Interfaces.Components;

namespace Ical.Net.Interfaces.Serialization.Factory
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Build(string objectName);
    }
}