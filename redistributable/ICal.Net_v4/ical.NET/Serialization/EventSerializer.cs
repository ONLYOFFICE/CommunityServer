using System;
using Ical.Net.CalendarComponents;

namespace Ical.Net.Serialization
{
    public class EventSerializer : ComponentSerializer
    {
        public EventSerializer() { }

        public EventSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (CalendarEvent);

        public override string SerializeToString(object obj)
        {
            var evt = obj as CalendarEvent;

            CalendarEvent actualEvent;
            if (evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND"))
            {
                actualEvent = evt.Copy<CalendarEvent>();
                actualEvent.Properties.Remove("DURATION");
            }
            else
            {
                actualEvent = evt;
            }
            return base.SerializeToString(actualEvent);
        }
    }
}