using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class RecurrencePatternSerializer : EncodableDataTypeSerializer
    {
        public RecurrencePatternSerializer() { }

        public RecurrencePatternSerializer(SerializationContext ctx) : base(ctx) { }

        public static DayOfWeek GetDayOfWeek(string value)
        {
            switch (value.ToUpper())
            {
                case "SU":
                    return DayOfWeek.Sunday;
                case "MO":
                    return DayOfWeek.Monday;
                case "TU":
                    return DayOfWeek.Tuesday;
                case "WE":
                    return DayOfWeek.Wednesday;
                case "TH":
                    return DayOfWeek.Thursday;
                case "FR":
                    return DayOfWeek.Friday;
                case "SA":
                    return DayOfWeek.Saturday;
            }
            throw new ArgumentException(value + " is not a valid iCal day-of-week indicator.");
        }

        protected static void AddInt32Values(IList<int> list, string value)
        {
            var values = value.Split(',');
            foreach (var v in values)
            {
                list.Add(Convert.ToInt32(v));
            }
        }

        public virtual void CheckRange(string name, IList<int> values, int min, int max)
        {
            var allowZero = (min == 0 || max == 0);
            foreach (var value in values)
            {
                CheckRange(name, value, min, max, allowZero);
            }
        }

        public virtual void CheckRange(string name, int value, int min, int max)
        {
            var allowZero = min == 0 || max == 0;
            CheckRange(name, value, min, max, allowZero);
        }

        public virtual void CheckRange(string name, int value, int min, int max, bool allowZero)
        {
            if (value != int.MinValue && (value < min || value > max || (!allowZero && value == 0)))
            {
                throw new ArgumentException(name + " value " + value + " is out of range. Valid values are between " + min + " and " + max +
                                            (allowZero ? "" : ", excluding zero (0)") + ".");
            }
        }

        public virtual void CheckMutuallyExclusive<T, TU>(string name1, string name2, T obj1, TU obj2)
        {
            if (Equals(obj1, default(T)) || Equals(obj2, default(TU)))
            {
                return;
            }
            // If the object is MinValue instead of its default, consider
            // that to be unassigned.

            var t1 = obj1.GetType();

            var fi1 = t1.GetField("MinValue");
            var fi2 = t1.GetField("MinValue");

            var isMin1 = fi1 != null && obj1.Equals(fi1.GetValue(null));
            var isMin2 = fi2 != null && obj2.Equals(fi2.GetValue(null));
            if (isMin1 || isMin2)
            {
                return;
            }

            throw new ArgumentException("Both " + name1 + " and " + name2 + " cannot be supplied together; they are mutually exclusive.");
        }

        private void SerializeByValue(List<string> aggregate, IList<int> byValue, string name)
        {
            if (byValue.Any())
            {
                aggregate.Add(name + "=" + string.Join(",", byValue.Select(i => i.ToString())));
            }
        }

        public override Type TargetType => typeof (RecurrencePattern);

        public override string SerializeToString(object obj)
        {
            var recur = obj as RecurrencePattern;
            var factory = GetService<ISerializerFactory>();
            if (recur == null || factory == null)
            {
                return null;
            }

            // Push the recurrence pattern onto the serialization stack
            SerializationContext.Push(recur);
            var values = new List<string>()
            {
                "FREQ=" + Enum.GetName(typeof(FrequencyType), recur.Frequency).ToUpper()
            };


            //-- FROM RFC2445 --
            //The INTERVAL rule part contains a positive integer representing how
            //often the recurrence rule repeats. The default value is "1", meaning
            //every second for a SECONDLY rule, or every minute for a MINUTELY
            //rule, every hour for an HOURLY rule, every day for a DAILY rule,
            //every week for a WEEKLY rule, every month for a MONTHLY rule and
            //every year for a YEARLY rule.
            var interval = recur.Interval;
            if (interval == int.MinValue)
            {
                interval = 1;
            }

            if (interval != 1)
            {
                values.Add("INTERVAL=" + interval);
            }

            if (recur.Until != DateTime.MinValue)
            {
                var serializer = factory.Build(typeof (IDateTime), SerializationContext) as IStringSerializer;
                if (serializer != null)
                {
                    IDateTime until = new CalDateTime(recur.Until);
                    until.HasTime = true;
                    values.Add("UNTIL=" + serializer.SerializeToString(until));
                }
            }

            if (recur.FirstDayOfWeek != DayOfWeek.Monday)
            {
                values.Add("WKST=" + Enum.GetName(typeof (DayOfWeek), recur.FirstDayOfWeek).ToUpper().Substring(0, 2));
            }

            if (recur.Count != int.MinValue)
            {
                values.Add("COUNT=" + recur.Count);
            }

            if (recur.ByDay.Count > 0)
            {
                var bydayValues = new List<string>();

                var serializer = factory.Build(typeof (WeekDay), SerializationContext) as IStringSerializer;
                if (serializer != null)
                {
                    bydayValues.AddRange(recur.ByDay.Select(byday => serializer.SerializeToString(byday)));
                }

                values.Add("BYDAY=" + string.Join(",", bydayValues.ToArray()));
            }

            SerializeByValue(values, recur.ByHour, "BYHOUR");
            SerializeByValue(values, recur.ByMinute, "BYMINUTE");
            SerializeByValue(values, recur.ByMonth, "BYMONTH");
            SerializeByValue(values, recur.ByMonthDay, "BYMONTHDAY");
            SerializeByValue(values, recur.BySecond, "BYSECOND");
            SerializeByValue(values, recur.BySetPosition, "BYSETPOS");
            SerializeByValue(values, recur.ByWeekNo, "BYWEEKNO");
            SerializeByValue(values, recur.ByYearDay, "BYYEARDAY");

            // Pop the recurrence pattern off the serialization stack
            SerializationContext.Pop();

            return Encode(recur, string.Join(";", values.ToArray()));
        }

        //Compiling these is a one-time penalty of about 80ms
        private const RegexOptions _ciCompiled = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        internal static readonly Regex OtherInterval =
            new Regex(@"every\s+(?<Interval>other|\d+)?\w{0,2}\s*(?<Freq>second|minute|hour|day|week|month|year)s?,?\s*(?<More>.+)", _ciCompiled);

        internal static readonly Regex AdverbFrequencies = new Regex(@"FREQ=(SECONDLY|MINUTELY|HOURLY|DAILY|WEEKLY|MONTHLY|YEARLY);?(.*)", _ciCompiled);

        internal static readonly Regex NumericTemporalUnits = new Regex(@"(?<Num>\d+)\w\w\s+(?<Type>second|minute|hour|day|week|month)", _ciCompiled);

        internal static readonly Regex TemporalUnitType = new Regex(@"(?<Type>second|minute|hour|day|week|month)\s+(?<Num>\d+)", _ciCompiled);

        internal static readonly Regex RelativeDaysOfWeek =
            new Regex(
                @"(?<Num>\d+\w{0,2})?(\w|\s)+?(?<First>first)?(?<Last>last)?\s*((?<Day>sunday|monday|tuesday|wednesday|thursday|friday|saturday)\s*(and|or)?\s*)+",
                _ciCompiled);

        internal static readonly Regex Time = new Regex(@"at\s+(?<Hour>\d{1,2})(:(?<Minute>\d{2})((:|\.)(?<Second>\d{2}))?)?\s*(?<Meridian>(a|p)m?)?",
            _ciCompiled);

        internal static readonly Regex RecurUntil = new Regex(@"^\s*until\s+(?<DateTime>.+)$", _ciCompiled);

        internal static readonly Regex SpecificRecurrenceCount = new Regex(@"^\s*for\s+(?<Count>\d+)\s+occurrences\s*$", _ciCompiled);

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            // Instantiate the data type
            var r = CreateAndAssociate() as RecurrencePattern;
            var factory = GetService<ISerializerFactory>();
            if (r == null || factory == null)
            {
                return r;
            }

            // Decode the value, if necessary
            value = Decode(r, value);

            var match = AdverbFrequencies.Match(value);
            if (match.Success)
            {
                // Parse the frequency type
                r.Frequency = (FrequencyType) Enum.Parse(typeof (FrequencyType), match.Groups[1].Value, true);

                // NOTE: fixed a bug where the group 2 match
                // resulted in an empty string, which caused
                // an error.
                if (match.Groups[2].Success && match.Groups[2].Length > 0)
                {
                    var keywordPairs = match.Groups[2].Value.Split(';');
                    foreach (var keywordPair in keywordPairs)
                    {
                        var keyValues = keywordPair.Split('=');
                        var keyword = keyValues[0];
                        var keyValue = keyValues[1];

                        switch (keyword.ToUpper())
                        {
                            case "UNTIL":
                            {
                                var serializer = factory.Build(typeof (IDateTime), SerializationContext) as IStringSerializer;
                                var dt = serializer?.Deserialize(new StringReader(keyValue)) as IDateTime;
                                if (dt != null)
                                {
                                    r.Until = dt.Value;
                                }
                            }
                                break;
                            case "COUNT":
                                r.Count = Convert.ToInt32(keyValue);
                                break;
                            case "INTERVAL":
                                r.Interval = Convert.ToInt32(keyValue);
                                break;
                            case "BYSECOND":
                                AddInt32Values(r.BySecond, keyValue);
                                break;
                            case "BYMINUTE":
                                AddInt32Values(r.ByMinute, keyValue);
                                break;
                            case "BYHOUR":
                                AddInt32Values(r.ByHour, keyValue);
                                break;
                            case "BYDAY":
                            {
                                var days = keyValue.Split(',');
                                foreach (var day in days)
                                {
                                    r.ByDay.Add(new WeekDay(day));
                                }
                            }
                                break;
                            case "BYMONTHDAY":
                                AddInt32Values(r.ByMonthDay, keyValue);
                                break;
                            case "BYYEARDAY":
                                AddInt32Values(r.ByYearDay, keyValue);
                                break;
                            case "BYWEEKNO":
                                AddInt32Values(r.ByWeekNo, keyValue);
                                break;
                            case "BYMONTH":
                                AddInt32Values(r.ByMonth, keyValue);
                                break;
                            case "BYSETPOS":
                                AddInt32Values(r.BySetPosition, keyValue);
                                break;
                            case "WKST":
                                r.FirstDayOfWeek = GetDayOfWeek(keyValue);
                                break;
                        }
                    }
                }
            }

            //
            // This matches strings such as:
            //
            // "Every 6 minutes"
            // "Every 3 days"
            //
            else if ((match = OtherInterval.Match(value)).Success)
            {
                if (match.Groups["Interval"].Success)
                {
                    int interval;
                    r.Interval = !int.TryParse(match.Groups["Interval"].Value, out interval)
                        ? 2
                        : interval;
                }
                else
                {
                    r.Interval = 1;
                }

                switch (match.Groups["Freq"].Value.ToLower())
                {
                    case "second":
                        r.Frequency = FrequencyType.Secondly;
                        break;
                    case "minute":
                        r.Frequency = FrequencyType.Minutely;
                        break;
                    case "hour":
                        r.Frequency = FrequencyType.Hourly;
                        break;
                    case "day":
                        r.Frequency = FrequencyType.Daily;
                        break;
                    case "week":
                        r.Frequency = FrequencyType.Weekly;
                        break;
                    case "month":
                        r.Frequency = FrequencyType.Monthly;
                        break;
                    case "year":
                        r.Frequency = FrequencyType.Yearly;
                        break;
                }

                var values = match.Groups["More"].Value.Split(',');
                foreach (var item in values)
                {
                    if ((match = NumericTemporalUnits.Match(item)).Success || (match = TemporalUnitType.Match(item)).Success)
                    {
                        int num;
                        if (!int.TryParse(match.Groups["Num"].Value, out num))
                        {
                            continue;
                        }

                        switch (match.Groups["Type"].Value.ToLower())
                        {
                            case "second":
                                r.BySecond.Add(num);
                                break;
                            case "minute":
                                r.ByMinute.Add(num);
                                break;
                            case "hour":
                                r.ByHour.Add(num);
                                break;
                            case "day":
                                switch (r.Frequency)
                                {
                                    case FrequencyType.Yearly:
                                        r.ByYearDay.Add(num);
                                        break;
                                    case FrequencyType.Monthly:
                                        r.ByMonthDay.Add(num);
                                        break;
                                }
                                break;
                            case "week":
                                r.ByWeekNo.Add(num);
                                break;
                            case "month":
                                r.ByMonth.Add(num);
                                break;
                        }
                    }
                    else if ((match = RelativeDaysOfWeek.Match(item)).Success)
                    {
                        var num = int.MinValue;
                        if (match.Groups["Num"].Success)
                        {
                            if (int.TryParse(match.Groups["Num"].Value, out num))
                            {
                                if (match.Groups["Last"].Success)
                                {
                                    // Make number negative
                                    num *= -1;
                                }
                            }
                        }
                        else if (match.Groups["Last"].Success)
                        {
                            num = -1;
                        }
                        else if (match.Groups["First"].Success)
                        {
                            num = 1;
                        }

                        var dayOfWeekQuery = from Capture capture in match.Groups["Day"].Captures
                            select (DayOfWeek) Enum.Parse(typeof(DayOfWeek), capture.Value, true) into dayOfWeek
                            select new WeekDay(dayOfWeek) {Offset = num};

                        r.ByDay.AddRange(dayOfWeekQuery);
                    }
                    else if ((match = Time.Match(item)).Success)
                    {
                        int hour;

                        if (!int.TryParse(match.Groups["Hour"].Value, out hour))
                        {
                            continue;
                        }

                        // Adjust for PM
                        if (match.Groups["Meridian"].Success && match.Groups["Meridian"].Value.ToUpper().StartsWith("P"))
                        {
                            hour += 12;
                        }

                        r.ByHour.Add(hour);

                        int minute;
                        if (match.Groups["Minute"].Success && int.TryParse(match.Groups["Minute"].Value, out minute))
                        {
                            r.ByMinute.Add(minute);
                            int second;
                            if (match.Groups["Second"].Success && int.TryParse(match.Groups["Second"].Value, out second))
                            {
                                r.BySecond.Add(second);
                            }
                        }
                    }
                    else if ((match = RecurUntil.Match(item)).Success)
                    {
                        var dt = DateTime.Parse(match.Groups["DateTime"].Value);
                        DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                        r.Until = dt;
                    }
                    else if ((match = SpecificRecurrenceCount.Match(item)).Success)
                    {
                        int count;
                        if (!int.TryParse(match.Groups["Count"].Value, out count))
                        {
                            return false;
                        }
                        r.Count = count;
                    }
                }
            }
            else
            {
                // Couldn't parse the object, return null!
                r = null;
            }

            if (r == null)
            {
                return r;
            }

            CheckMutuallyExclusive("COUNT", "UNTIL", r.Count, r.Until);
            CheckRange("INTERVAL", r.Interval, 1, int.MaxValue);
            CheckRange("COUNT", r.Count, 1, int.MaxValue);
            CheckRange("BYSECOND", r.BySecond, 0, 59);
            CheckRange("BYMINUTE", r.ByMinute, 0, 59);
            CheckRange("BYHOUR", r.ByHour, 0, 23);
            CheckRange("BYMONTHDAY", r.ByMonthDay, -31, 31);
            CheckRange("BYYEARDAY", r.ByYearDay, -366, 366);
            CheckRange("BYWEEKNO", r.ByWeekNo, -53, 53);
            CheckRange("BYMONTH", r.ByMonth, 1, 12);
            CheckRange("BYSETPOS", r.BySetPosition, -366, 366);

            return r;
        }
    }
}