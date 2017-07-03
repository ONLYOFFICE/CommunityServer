using System.Linq;
using Ical.Net.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    public class CalendarPropertyList : GroupedValueList<string, ICalendarProperty, CalendarProperty, object>
    {
        private readonly ICalendarObject _mParent;

        public CalendarPropertyList() {}

        public CalendarPropertyList(ICalendarObject parent)
        {
            _mParent = parent;
            ItemAdded += CalendarPropertyList_ItemAdded;
        }

        private void CalendarPropertyList_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = _mParent;
        }

        public ICalendarProperty this[string name]
        {
            get
            {
                return ContainsKey(name)
                    ? AllOf(name).FirstOrDefault()
                    : null;
            }
        }
    }
}