using Ical.Net.Collections.Interfaces;

namespace Ical.Net.Interfaces.General
{
    public interface ICalendarProperty : ICalendarParameterCollectionContainer, ICalendarObject, IValueObject<object>
    {
        object Value { get; set; }
    }
}