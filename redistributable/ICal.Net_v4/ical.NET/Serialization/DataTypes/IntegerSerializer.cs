using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class IntegerSerializer : EncodableDataTypeSerializer
    {
        public IntegerSerializer() { }

        public IntegerSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (int);

        public override string SerializeToString(object integer)
        {
            try
            {
                var i = Convert.ToInt32(integer);

                var obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Encode the value as needed.
                    var dt = new EncodableDataType
                    {
                        AssociatedObject = obj
                    };
                    return Encode(dt, i.ToString());
                }
                return i.ToString();
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            try
            {
                var obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Decode the value, if necessary!
                    var dt = new EncodableDataType
                    {
                        AssociatedObject = obj
                    };
                    value = Decode(dt, value);
                }

                int i;
                if (Int32.TryParse(value, out i))
                {
                    return i;
                }
            }
            catch {}

            return value;
        }
    }
}