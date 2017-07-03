using System;
using System.Collections.Generic;
using System.IO;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers.Components;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class CalendarSerializer : ComponentSerializer
    {
        private readonly ICalendar _calendar;

        public CalendarSerializer()
            :this(new SerializationContext()) { }

        public CalendarSerializer(ICalendar cal)
        {
            _calendar = cal;
        }

        public CalendarSerializer(ISerializationContext ctx) : base(ctx) {}

        public virtual string SerializeToString() => SerializeToString(_calendar);

        protected override IComparer<ICalendarProperty> PropertySorter => new CalendarPropertySorter();

        public override string SerializeToString(object obj)
        {
            var iCal = obj as ICalendar;

            // Ensure VERSION and PRODUCTID are both set,
            // as they are required by RFC5545.
            if (string.IsNullOrWhiteSpace(iCal.Version))
            {
                iCal.Version = CalendarVersions.Latest;
            }
            if (string.IsNullOrWhiteSpace(iCal.ProductId))
            {
                iCal.ProductId = CalendarProductIDs.Default;
            }

            return base.SerializeToString(iCal);
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr == null)
            {
                return null;
            }

            using (tr)
            {
                var ctx = SerializationContext;
                var contents = tr.ReadToEnd();

                using (var normalized = TextUtil.Normalize(contents, ctx))
                {
                    var lexer = new iCalLexer(normalized);
                    var parser = new iCalParser(lexer);

                    // Parse the iCalendar(s)!
                    var iCalendars = parser.icalendar(SerializationContext);

                    // Return the parsed iCalendar(s)
                    return iCalendars;
                }
            }
        }

        private class CalendarPropertySorter : IComparer<ICalendarProperty>
        {
            public int Compare(ICalendarProperty x, ICalendarProperty y)
            {
                if (x == y || (x == null && y == null))
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                // Alphabetize all properties except VERSION, which should appear first. 
                if (string.Equals("VERSION", x.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }
                if (string.Equals("VERSION", y.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}