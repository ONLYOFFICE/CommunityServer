using System;
using System.IO;
using System.Text;
using Ical.Net.General;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class ParameterSerializer : SerializerBase
    {
        public ParameterSerializer() {}

        public ParameterSerializer(ISerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (CalendarParameter);

        public override string SerializeToString(object obj)
        {
            var p = obj as CalendarParameter;

            var builder = new StringBuilder(512);
            builder.Append(p.Name + "=");

            // "Section 3.2:  Property parameter values MUST NOT contain the DQUOTE character."
            // Therefore, let's strip any double quotes from the value.
            var values = string.Join(",", p.Values).Replace("\"", string.Empty);

            // Surround the parameter value with double quotes, if the value
            // contains any problematic characters.
            if (values.IndexOfAny(new[] { ';', ':', ',' }) >= 0)
            {
                values = "\"" + values + "\"";
            }
            builder.Append(values);
            return builder.ToString();
        }

        public override object Deserialize(TextReader tr)
        {
            using (tr)
            {
                var lexer = new iCalLexer(tr);
                var parser = new iCalParser(lexer);
                var p = parser.parameter(SerializationContext, null);
                return p;
            }
        }
    }
}