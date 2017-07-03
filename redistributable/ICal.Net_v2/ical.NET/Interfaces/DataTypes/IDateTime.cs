using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IDateTime : IEncodableDataType, IComparable<IDateTime>, IFormattable
    {
        /// <summary>
        /// Converts the date/time to this computer's local date/time.
        /// </summary>
        DateTime AsSystemLocal { get; }

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        DateTime AsUtc { get; }

        /// <summary>
        /// Gets/sets whether the Value of this date/time represents
        /// a universal time.
        /// </summary>
        bool IsUniversalTime { get; set; }

        /// <summary>
        /// Gets the time zone name this time is in, if it references a time zone.
        /// </summary>
        string TimeZoneName { get; }

        /// <summary>
        /// Gets/sets the underlying DateTime value stored.  This should always
        /// use DateTimeKind.Utc, regardless of its actual representation.
        /// Use IsUniversalTime along with the TZID to control how this
        /// date/time is handled.
        /// </summary>
        DateTime Value { get; set; }

        /// <summary>
        /// Gets/sets whether or not this date/time value contains a 'date' part.
        /// </summary>
        bool HasDate { get; set; }

        /// <summary>
        /// Gets/sets whether or not this date/time value contains a 'time' part.
        /// </summary>
        bool HasTime { get; set; }

        /// <summary>
        /// Gets/sets the time zone ID for this date/time value.
        /// </summary>
        string TzId { get; set; }

        /// <summary>
        /// Gets the year for this date/time value.
        /// </summary>
        int Year { get; }

        /// <summary>
        /// Gets the month for this date/time value.
        /// </summary>
        int Month { get; }

        /// <summary>
        /// Gets the day for this date/time value.
        /// </summary>
        int Day { get; }

        /// <summary>
        /// Gets the hour for this date/time value.
        /// </summary>
        int Hour { get; }

        /// <summary>
        /// Gets the minute for this date/time value.
        /// </summary>
        int Minute { get; }

        /// <summary>
        /// Gets the second for this date/time value.
        /// </summary>
        int Second { get; }

        /// <summary>
        /// Gets the millisecond for this date/time value.
        /// </summary>
        int Millisecond { get; }

        /// <summary>
        /// Gets the ticks for this date/time value.
        /// </summary>
        long Ticks { get; }

        /// <summary>
        /// Gets the DayOfWeek for this date/time value.
        /// </summary>
        DayOfWeek DayOfWeek { get; }

        /// <summary>
        /// Gets the date portion of the date/time value.
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        /// Converts the date/time value to a local time
        /// within the specified time zone.
        /// </summary>
        IDateTime ToTimeZone(string newTimeZone);

        IDateTime Add(TimeSpan ts);
        IDateTime Subtract(TimeSpan ts);
        TimeSpan Subtract(IDateTime dt);

        IDateTime AddYears(int years);
        IDateTime AddMonths(int months);
        IDateTime AddDays(int days);
        IDateTime AddHours(int hours);
        IDateTime AddMinutes(int minutes);
        IDateTime AddSeconds(int seconds);
        IDateTime AddMilliseconds(int milliseconds);
        IDateTime AddTicks(long ticks);

        bool LessThan(IDateTime dt);
        bool GreaterThan(IDateTime dt);
        bool LessThanOrEqual(IDateTime dt);
        bool GreaterThanOrEqual(IDateTime dt);

        void AssociateWith(IDateTime dt);
    }
}