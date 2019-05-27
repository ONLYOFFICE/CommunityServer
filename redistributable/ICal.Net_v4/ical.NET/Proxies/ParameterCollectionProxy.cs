using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Collections;
using Ical.Net.Collections.Proxies;

namespace Ical.Net.Proxies
{
    public class ParameterCollectionProxy : GroupedCollectionProxy<string, CalendarParameter, CalendarParameter>, IParameterCollection
    {
        protected GroupedValueList<string, CalendarParameter, CalendarParameter, string> Parameters
            => RealObject as GroupedValueList<string, CalendarParameter, CalendarParameter, string>;

        public ParameterCollectionProxy(IGroupedList<string, CalendarParameter> realObject) : base(realObject) {}

        public virtual void SetParent(ICalendarObject parent)
        {
            foreach (var parameter in this)
            {
                parameter.Parent = parent;
            }
        }

        public virtual void Add(string name, string value)
        {
            RealObject.Add(new CalendarParameter(name, value));
        }

        public virtual string Get(string name)
        {
            var parameter = RealObject.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

            return parameter?.Value;
        }

        public virtual IList<string> GetMany(string name) => new GroupedValueListProxy<string, CalendarParameter, CalendarParameter, string, string>(Parameters, name);

        public virtual void Set(string name, string value)
        {
            var parameter = RealObject.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

            if (parameter == null)
            {
                RealObject.Add(new CalendarParameter(name, value));
            }
            else
            {
                parameter.SetValue(value);
            }
        }

        public virtual void Set(string name, IEnumerable<string> values)
        {
            var parameter = RealObject.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

            if (parameter == null)
            {
                RealObject.Add(new CalendarParameter(name, values));
            }
            else
            {
                parameter.SetValue(values);
            }
        }

        public virtual int IndexOf(CalendarParameter obj) => 0;

        public virtual void Insert(int index, CalendarParameter item) {}

        public virtual void RemoveAt(int index) {}

        public virtual CalendarParameter this[int index]
        {
            get { return Parameters[index]; }
            set { }
        }
    }
}