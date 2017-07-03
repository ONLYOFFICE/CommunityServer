using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers.Other
{
    public class UriSerializer : EncodableDataTypeSerializer
    {
        public UriSerializer() {}

        public UriSerializer(ISerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (string);

        public override string SerializeToString(object obj)
        {
            if (obj is Uri)
            {
                var uri = (Uri) obj;

                var co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    var dt = new EncodableDataType
                    {
                        AssociatedObject = co
                    };
                    return Encode(dt, uri.OriginalString);
                }
                return uri.OriginalString;
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                var value = tr.ReadToEnd();

                var co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    var dt = new EncodableDataType
                    {
                        AssociatedObject = co
                    };
                    value = Decode(dt, value);
                }

                try
                {
                    var uri = new Uri(value);
                    return uri;
                }
                catch {}
            }
            return null;
        }
    }
}