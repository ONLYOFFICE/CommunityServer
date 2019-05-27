using System;
using System.IO;
using System.Text;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class PeriodSerializer : EncodableDataTypeSerializer
    {
        public PeriodSerializer() { }

        public PeriodSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Period);

        public override string SerializeToString(object obj)
        {
            var p = obj as Period;
            var factory = GetService<ISerializerFactory>();

            if (p == null || factory == null)
            {
                return null;
            }

            // Push the period onto the serialization context stack
            SerializationContext.Push(p);

            try
            {
                var dtSerializer = factory.Build(typeof (IDateTime), SerializationContext) as IStringSerializer;
                var timeSpanSerializer = factory.Build(typeof (TimeSpan), SerializationContext) as IStringSerializer;
                if (dtSerializer == null || timeSpanSerializer == null)
                {
                    return null;
                }
                var sb = new StringBuilder();

                // Serialize the start time                    
                sb.Append(dtSerializer.SerializeToString(p.StartTime));

                // Serialize the duration
                if (!p.StartTime.HasTime)
                {
                    // Serialize the duration
                    sb.Append("/");
                    sb.Append(timeSpanSerializer.SerializeToString(p.Duration));
                }

                // Encode the value as necessary
                return Encode(p, sb.ToString());
            }
            finally
            {
                // Pop the period off the serialization context stack
                SerializationContext.Pop();
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            var p = CreateAndAssociate() as Period;
            var factory = GetService<ISerializerFactory>();
            if (p == null || factory == null)
            {
                return null;
            }

            var dtSerializer = factory.Build(typeof (IDateTime), SerializationContext) as IStringSerializer;
            var durationSerializer = factory.Build(typeof (TimeSpan), SerializationContext) as IStringSerializer;
            if (dtSerializer == null || durationSerializer == null)
            {
                return null;
            }

            // Decode the value as necessary
            value = Decode(p, value);

            var values = value.Split('/');
            if (values.Length != 2)
            {
                return false;
            }

            p.StartTime = dtSerializer.Deserialize(new StringReader(values[0])) as IDateTime;
            p.EndTime = dtSerializer.Deserialize(new StringReader(values[1])) as IDateTime;
            if (p.EndTime == null)
            {
                p.Duration = (TimeSpan) durationSerializer.Deserialize(new StringReader(values[1]));
            }

            // Only return an object if it has been deserialized correctly.
            if (p.StartTime != null && p.Duration != null)
            {
                return p;
            }

            return null;
        }
    }
}