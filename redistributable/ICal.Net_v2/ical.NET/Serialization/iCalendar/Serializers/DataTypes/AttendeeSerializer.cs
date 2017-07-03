using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers.Other;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class AttendeeSerializer : StringSerializer
    {
        public AttendeeSerializer() { }

        public AttendeeSerializer(ISerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Attendee);

        public override string SerializeToString(object obj)
        {
            var a = obj as IAttendee;
            return a?.Value == null
                ? null
                : Encode(a, a.Value.OriginalString);
        }

        public Attendee Deserialize(string attendee)
        {
            try
            {
                var a = CreateAndAssociate() as Attendee;
                var uriString = Unescape(Decode(a, attendee));

                // Prepend "mailto:" if necessary
                if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                {
                    uriString = "mailto:" + uriString;
                }

                a.Value = new Uri(uriString);
                return a;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public override object Deserialize(TextReader tr) => Deserialize(tr.ReadToEnd());
    }
}