using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers.Other
{
    public class StringSerializer : EncodableDataTypeSerializer
    {
        public StringSerializer() {}

        public StringSerializer(ISerializationContext ctx) : base(ctx) {}

        internal static readonly Regex SingleBackslashMatch = new Regex(@"(?<!\\)\\(?!\\)", RegexOptions.Compiled);

        protected virtual string Unescape(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            value = value.Replace(@"\n", "\n");
            value = value.Replace(@"\N", "\n");
            value = value.Replace(@"\;", ";");
            value = value.Replace(@"\,", ",");
            // NOTE: double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
            value = value.Replace("\\\"", "\"");

            // Replace all single-backslashes with double-backslashes.
            value = SingleBackslashMatch.Replace(value, "\\\\");

            // Unescape double backslashes
            value = value.Replace(@"\\", @"\");
            return value;
        }

        protected virtual string Escape(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            // NOTE: fixed a bug that caused text parsing to fail on
            // programmatically entered strings.
            // SEE unit test SERIALIZE25().
            value = value.Replace(SerializationConstants.LineBreak, @"\n");
            value = value.Replace("\r", @"\n");
            value = value.Replace("\n", @"\n");
            value = value.Replace(";", @"\;");
            value = value.Replace(",", @"\,");
            return value;
        }

        public override Type TargetType => typeof (string);

        public override string SerializeToString(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var values = new List<string>();
            if (obj is string)
            {
                values.Add((string) obj);
            }
            else if (obj is IEnumerable)
            {
                values.AddRange(from object child in (IEnumerable) obj select child.ToString());
            }

            var co = SerializationContext.Peek() as ICalendarObject;
            if (co != null)
            {
                // Encode the string as needed.
                var dt = new EncodableDataType
                {
                    AssociatedObject = co
                };
                for (var i = 0; i < values.Count; i++)
                {
                    values[i] = Encode(dt, Escape(values[i]));
                }

                return string.Join(",", values);
            }

            for (var i = 0; i < values.Count; i++)
            {
                values[i] = Escape(values[i]);
            }
            return string.Join(",", values);
        }

        internal static readonly Regex UnescapedCommas = new Regex(@"(?<!\\),", RegexOptions.Compiled);
        public override object Deserialize(TextReader tr)
        {
            if (tr == null)
            {
                return null;
            }

            var value = tr.ReadToEnd();

            // NOTE: this can deserialize into an IList<string> or simply a string,
            // depending on the input text.  Anything that uses this serializer should
            // be prepared to receive either a string, or an IList<string>.

            var serializeAsList = false;

            // Determine if we can serialize this property
            // with multiple values per line.
            var co = SerializationContext.Peek() as ICalendarObject;
            if (co is ICalendarProperty)
            {
                serializeAsList = GetService<IDataTypeMapper>().GetPropertyAllowsMultipleValues(co);
            }

            // Try to decode the string
            EncodableDataType dt = null;
            if (co != null)
            {
                dt = new EncodableDataType
                {
                    AssociatedObject = co
                };
            }

            var encodedValues = serializeAsList ? UnescapedCommas.Split(value) : new[] { value };
            var escapedValues = Array.ConvertAll(encodedValues, v => Decode(dt, v));
            var values = Array.ConvertAll(escapedValues, v => Unescape(v));

            if (co is ICalendarProperty)
            {
                // Determine if our we're supposed to store extra information during
                // the serialization process.  If so, let's store the escaped value.
                var settings = GetService<ISerializationSettings>();
                if (settings != null && settings.StoreExtraSerializationData)
                {
                    // Store the escaped value
                    co.SetService("EscapedValue", escapedValues.Length == 1 ? escapedValues[0] : (object) escapedValues);
                }
            }

            // Return either a single value, or the entire list.
            if (values.Length == 1)
            {
                return values[0];
            }
            return values;
        }
    }
}