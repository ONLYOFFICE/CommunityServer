using System;
using System.IO;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class WeekDaySerializer : EncodableDataTypeSerializer
    {
        public WeekDaySerializer() { }

        public WeekDaySerializer(ISerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (WeekDay);

        public override string SerializeToString(object obj)
        {
            var ds = obj as IWeekDay;
            if (ds != null)
            {
                var value = string.Empty;
                if (ds.Offset != int.MinValue)
                {
                    value += ds.Offset;
                }
                value += Enum.GetName(typeof (DayOfWeek), ds.DayOfWeek).ToUpper().Substring(0, 2);

                return Encode(ds, value);
            }
            return null;
        }

        internal static readonly Regex DayOfWeek = new Regex(@"(\+|-)?(\d{1,2})?(\w{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            // Create the day specifier and associate it with a calendar object
            var ds = CreateAndAssociate() as IWeekDay;

            // Decode the value, if necessary
            value = Decode(ds, value);

            var match = DayOfWeek.Match(value);
            if (!match.Success)
            {
                return null;
            }

            if (match.Groups[2].Success)
            {
                ds.Offset = Convert.ToInt32(match.Groups[2].Value);
                if (match.Groups[1].Success && match.Groups[1].Value.Contains("-"))
                {
                    ds.Offset *= -1;
                }
            }
            ds.DayOfWeek = RecurrencePatternSerializer.GetDayOfWeek(match.Groups[3].Value);
            return ds;
        }
    }
}