using Ical.Net.Interfaces.Components;

namespace Ical.Net.Interfaces.General
{
    public interface IUniqueComponentList<TComponentType> : ICalendarObjectList<TComponentType> where TComponentType : class, IUniqueComponent
    {
        TComponentType this[string uid] { get; set; }
    }
}