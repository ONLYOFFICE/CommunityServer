using System;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Serialization
{
    public class PropertySerializer : SerializerBase
    {
        public PropertySerializer() {}

        public PropertySerializer(SerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (CalendarProperty);

        public override string SerializeToString(object obj)
        {
            var prop = obj as ICalendarProperty;
            if (prop?.Values == null || !prop.Values.Any())
            {
                return null;
            }

            // Push this object on the serialization context.
            SerializationContext.Push(prop);

            // Get a serializer factory that we can use to serialize
            // the property and parameter values
            var sf = GetService<ISerializerFactory>();

            var result = new StringBuilder();
            foreach (var v in prop.Values.Where(value => value != null))
            {
                // Get a serializer to serialize the property's value.
                // If we can't serialize the property's value, the next step is worthless anyway.
                var valueSerializer = sf.Build(v.GetType(), SerializationContext) as IStringSerializer;

                // Iterate through each value to be serialized,
                // and give it a property (with parameters).
                // FIXME: this isn't always the way this is accomplished.
                // Multiple values can often be serialized within the
                // same property.  How should we fix this?

                // NOTE:
                // We Serialize the property's value first, as during 
                // serialization it may modify our parameters.
                // FIXME: the "parameter modification" operation should
                // be separated from serialization. Perhaps something
                // like PreSerialize(), etc.
                var value = valueSerializer.SerializeToString(v);

                // Get the list of parameters we'll be serializing
                var parameterList = prop.Parameters;
                if (v is ICalendarDataType)
                {
                    parameterList = (v as ICalendarDataType).Parameters;
                }

                //This says that the TZID property of an RDATE/EXDATE collection is owned by the PeriodList that contains it. There's nothing in the spec that
                //prohibits having multiple EXDATE or RDATE collections, each of which specifies a different TZID. What *should* happen during serialization is
                //that we should work with a single collection of zoned datetime objects, and we should create distinct RDATE and EXDATE collections based on
                //those values. Right now, if you add CalDateTime objects, each of which specifies a different time zone, the first one "wins". This means
                //application developers will need to handle those cases outside the library.
                if (v is PeriodList)
                {
                    var typed = (PeriodList)v;
                    if (!string.IsNullOrWhiteSpace(typed.TzId) && parameterList.All(p => string.Equals("TZID", p.Value, StringComparison.OrdinalIgnoreCase)))
                    {
                        parameterList.Set("TZID", typed.TzId);
                    }
                }

                var sb = new StringBuilder();
                sb.Append(prop.Name);
                if (parameterList.Any())
                {
                    // Get a serializer for parameters
                    var parameterSerializer = sf.Build(typeof (CalendarParameter), SerializationContext) as IStringSerializer;
                    if (parameterSerializer != null)
                    {
                        // Serialize each parameter
                        // Separate parameters with semicolons
                        sb.Append(";");
                        sb.Append(string.Join(";", parameterList.Select(param => parameterSerializer.SerializeToString(param))));
                    }
                }
                sb.Append(":");
                sb.Append(value);

                result.Append(TextUtil.FoldLines(sb.ToString()));
            }

            // Pop the object off the serialization context.
            SerializationContext.Pop();
            return result.ToString();
        }

        public override object Deserialize(TextReader tr) => null;
    }
}