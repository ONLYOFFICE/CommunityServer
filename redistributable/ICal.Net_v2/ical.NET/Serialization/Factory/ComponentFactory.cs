using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.Serialization.Factory;

namespace Ical.Net.Serialization.Factory
{
    public class ComponentFactory : ICalendarComponentFactory
    {
        public virtual ICalendarComponent Build(string objectName)
        {
            ICalendarComponent c = null;
            var name = objectName.ToUpper();

            switch (name)
            {
                case Components.Alarm:
                    c = new Alarm();
                    break;
                case Components.Event:
                    c = new Event();
                    break;
                case Components.Freebusy:
                    c = new FreeBusy();
                    break;
                case Components.Journal:
                    c = new Journal();
                    break;
                case Components.Timezone:
                    c = new VTimeZone();
                    break;
                case Components.Todo:
                    c = new Todo();
                    break;
                case Components.Calendar:
                    c = new Calendar();
                    break;
                default:
                    c = new CalendarComponent();
                    break;
            }
            c.Name = name;
            return c;
        }
    }
}