using System.Collections.Generic;
using Ical.Net.Collections;

namespace Ical.Net
{
    public class ParameterList : GroupedValueList<string, CalendarParameter, CalendarParameter, string>, IParameterCollection
    {
        public virtual void SetParent(ICalendarObject parent)
        {
            foreach (var parameter in this)
            {
                parameter.Parent = parent;
            }
        }

        public virtual void Add(string name, string value)
        {
            Add(new CalendarParameter(name, value));
        }

        public virtual string Get(string name) => Get<string>(name);

        public virtual IList<string> GetMany(string name) => GetMany<string>(name);
    }
}