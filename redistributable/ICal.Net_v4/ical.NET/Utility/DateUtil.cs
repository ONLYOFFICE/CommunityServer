using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ical.Net.DataTypes;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net.Utility
{
    internal static class DateUtil
    {
        public static IDateTime StartOfDay(IDateTime dt)
            => dt.AddHours(-dt.Hour).AddMinutes(-dt.Minute).AddSeconds(-dt.Second);

        public static IDateTime EndOfDay(IDateTime dt)
            => StartOfDay(dt).AddDays(1).AddTicks(-1);

        public static DateTime GetSimpleDateTimeData(IDateTime dt)
            => DateTime.SpecifyKind(dt.Value, dt.IsUtc ? DateTimeKind.Utc : DateTimeKind.Local);

        public static DateTime SimpleDateTimeToMatch(IDateTime dt, IDateTime toMatch)
        {
            if (toMatch.IsUtc && dt.IsUtc)
            {
                return dt.Value;
            }
            if (toMatch.IsUtc)
            {
                return dt.Value.ToUniversalTime();
            }
            if (dt.IsUtc)
            {
                return dt.Value.ToLocalTime();
            }
            return dt.Value;
        }

        public static IDateTime MatchTimeZone(IDateTime dt1, IDateTime dt2)
        {
            // Associate the date/time with the first.
            var copy = dt2;
            copy.AssociateWith(dt1);

            // If the dt1 time does not occur in the same time zone as the
            // dt2 time, then let's convert it so they can be used in the
            // same context (i.e. evaluation).
            if (dt1.TzId != null)
            {
                return string.Equals(dt1.TzId, copy.TzId, StringComparison.OrdinalIgnoreCase)
                    ? copy
                    : copy.ToTimeZone(dt1.TzId);
            }

            return dt1.IsUtc
                ? new CalDateTime(copy.AsUtc)
                : new CalDateTime(copy.AsSystemLocal);
        }

        public static DateTime AddWeeks(DateTime dt, int interval, DayOfWeek firstDayOfWeek)
        {
            // NOTE: fixes WeeklyUntilWkst2() eval.
            // NOTE: simplified the execution of this - fixes bug #3119920 - missing weekly occurences also
            dt = dt.AddDays(interval * 7);
            while (dt.DayOfWeek != firstDayOfWeek)
            {
                dt = dt.AddDays(-1);
            }

            return dt;
        }

        public static DateTime FirstDayOfWeek(DateTime dt, DayOfWeek firstDayOfWeek, out int offset)
        {
            offset = 0;
            while (dt.DayOfWeek != firstDayOfWeek)
            {
                dt = dt.AddDays(-1);
                offset++;
            }
            return dt;
        }

        private static readonly Lazy<Dictionary<string, string>> _windowsMapping
            = new Lazy<Dictionary<string, string>>(InitializeWindowsMappings, LazyThreadSafetyMode.PublicationOnly);

        private static Dictionary<string, string> InitializeWindowsMappings()
            => TzdbDateTimeZoneSource.Default.WindowsMapping.PrimaryMapping
                .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

        public static readonly DateTimeZone LocalDateTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

        /// <summary>
        /// Use this method to turn a raw string into a NodaTime DateTimeZone. It searches all time zone providers (IANA, BCL, serialization, etc) to see if
        /// the string matches. If it doesn't, it walks each provider, and checks to see if the time zone the provider knows about is contained within the
        /// target time zone string. Some older icalendar programs would generate nonstandard time zone strings, and this secondary check works around
        /// that.
        /// </summary>
        /// <param name="tzId">A BCL, IANA, or serialization time zone identifier</param>
        /// <param name="useLocalIfNotFound">If true, this method will return the system local time zone if tzId doesn't match a known time zone identifier.
        /// Otherwise, it will throw an exception.</param>
        public static DateTimeZone GetZone(string tzId, bool useLocalIfNotFound = true)
        {
            if (string.IsNullOrWhiteSpace(tzId))
            {
                return LocalDateTimeZone;
            }

            if (tzId.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                tzId = tzId.Substring(1, tzId.Length - 1);
            }

            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            if (_windowsMapping.Value.TryGetValue(tzId, out var ianaZone))
            {
                return DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaZone);
            }

            zone = DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            //US/Eastern is commonly represented as US-Eastern
            var newTzId = tzId.Replace("-", "/");
            zone = DateTimeZoneProviders.Serialization.GetZoneOrNull(newTzId);
            if (zone != null)
            {
                return zone;
            }

            foreach (var providerId in DateTimeZoneProviders.Tzdb.Ids.Where(tzId.Contains))
            {
                return DateTimeZoneProviders.Tzdb.GetZoneOrNull(providerId);
            }

            if (_windowsMapping.Value.Keys
                    .Where(tzId.Contains)
                    .Any(providerId => _windowsMapping.Value.TryGetValue(providerId, out ianaZone))
               )
            {
                return DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaZone);
            }

            foreach (var providerId in DateTimeZoneProviders.Serialization.Ids.Where(tzId.Contains))
            {
                return DateTimeZoneProviders.Serialization.GetZoneOrNull(providerId);
            }

            if (useLocalIfNotFound)
            {
                return LocalDateTimeZone;
            }

            throw new ArgumentException($"Unrecognized time zone id {tzId}");
        }

        public static ZonedDateTime AddYears(ZonedDateTime zonedDateTime, int years)
        {
            var futureDate = zonedDateTime.Date.PlusYears(years);
            var futureLocalDateTime = new LocalDateTime(futureDate.Year, futureDate.Month, futureDate.Day, zonedDateTime.Hour, zonedDateTime.Minute,
                zonedDateTime.Second);
            var zonedFutureDate = new ZonedDateTime(futureLocalDateTime, zonedDateTime.Zone, zonedDateTime.Offset);
            return zonedFutureDate;
        }

        public static ZonedDateTime AddMonths(ZonedDateTime zonedDateTime, int months)
        {
            var futureDate = zonedDateTime.Date.PlusMonths(months);
            var futureLocalDateTime = new LocalDateTime(futureDate.Year, futureDate.Month, futureDate.Day, zonedDateTime.Hour, zonedDateTime.Minute,
                zonedDateTime.Second);
            var zonedFutureDate = new ZonedDateTime(futureLocalDateTime, zonedDateTime.Zone, zonedDateTime.Offset);
            return zonedFutureDate;
        }

        public static ZonedDateTime ToZonedDateTimeLeniently(DateTime dateTime, string tzId)
        {
            var zone = GetZone(tzId);
            var localDt = LocalDateTime.FromDateTime(dateTime); //19:00 UTC
            var lenientZonedDateTime = localDt.InZoneLeniently(zone).WithZone(zone); //15:00 Eastern
            return lenientZonedDateTime;
        }

        public static ZonedDateTime FromTimeZoneToTimeZone(DateTime dateTime, string fromZoneId, string toZoneId)
            => FromTimeZoneToTimeZone(dateTime, GetZone(fromZoneId), GetZone(toZoneId));

        public static ZonedDateTime FromTimeZoneToTimeZone(DateTime dateTime, DateTimeZone fromZone, DateTimeZone toZone)
        {
            var oldZone = LocalDateTime.FromDateTime(dateTime).InZoneLeniently(fromZone);
            var newZone = oldZone.WithZone(toZone);
            return newZone;
        }

        public static bool IsSerializationTimeZone(DateTimeZone zone) => DateTimeZoneProviders.Serialization.GetZoneOrNull(zone.Id) != null;

        /// <summary>
        /// Truncate to the specified TimeSpan's magnitude. For example, to truncate to the nearest second, use TimeSpan.FromSeconds(1)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
            => timeSpan == TimeSpan.Zero
                ? dateTime
                : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));

        public static int WeekOfMonth(DateTime d)
        {
            var isExact = d.Day % 7 == 0;
            var offset = isExact
                ? 0
                : 1;
            return (int) Math.Floor(d.Day / 7.0) + offset;
        }
    }
}