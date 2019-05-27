using System.Collections.Generic;
using Ical.Net.Collections;

namespace Ical.Net
{
    public interface IParameterCollection : IGroupedList<string, CalendarParameter>
    {
        void SetParent(ICalendarObject parent);
        void Add(string name, string value);
        string Get(string name);
        IList<string> GetMany(string name);
        void Set(string name, string value);
        void Set(string name, IEnumerable<string> values);
    }
}