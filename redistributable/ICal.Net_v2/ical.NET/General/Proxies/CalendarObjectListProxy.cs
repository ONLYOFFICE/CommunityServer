using System.Linq;
using Ical.Net.Collections.Interfaces;
using Ical.Net.Collections.Proxies;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General.Proxies
{
    public class CalendarObjectListProxy<TType> : GroupedCollectionProxy<string, ICalendarObject, TType>, ICalendarObjectList<TType>
        where TType : class, ICalendarObject
    {
        public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list) {}

        public virtual TType this[int index] => this.Skip(index).FirstOrDefault();
    }
}