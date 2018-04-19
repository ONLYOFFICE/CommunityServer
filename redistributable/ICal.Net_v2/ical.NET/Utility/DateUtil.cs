using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using NodaTime;
using NodaTime.TimeZones;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Ical.Net.Utility
{
    public class DateUtil
    {
        private const string UNIX_TIME_ZONE_CONFIG_FILE = "/etc/timezone";

        public static IDateTime StartOfDay(IDateTime dt)
        {
            return dt.AddHours(-dt.Hour).AddMinutes(-dt.Minute).AddSeconds(-dt.Second);
        }

        public static IDateTime EndOfDay(IDateTime dt)
        {
            return StartOfDay(dt).AddDays(1).AddTicks(-1);
        }

        public static DateTime GetSimpleDateTimeData(IDateTime dt)
        {
            return DateTime.SpecifyKind(dt.Value, dt.IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local);
        }

        public static DateTime SimpleDateTimeToMatch(IDateTime dt, IDateTime toMatch)
        {
            if (toMatch.IsUniversalTime && dt.IsUniversalTime)
            {
                return dt.Value;
            }
            if (toMatch.IsUniversalTime)
            {
                return dt.Value.ToUniversalTime();
            }
            if (dt.IsUniversalTime)
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
                if (!string.Equals(dt1.TzId, copy.TzId))
                {
                    return copy.ToTimeZone(dt1.TzId);
                }
                return copy;
            }
            if (dt1.IsUniversalTime)
            {
                // The first date/time is in UTC time, convert!
                return new CalDateTime(copy.AsUtc);
            }
            // The first date/time is in local time, convert!
            return new CalDateTime(copy.AsSystemLocal);
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

        public static int WeekOfMonth(DateTime d)
        {
            var isExact = (d.Day % 7 == 0);
            var offset = isExact ? 0 : 1;
            return (int)Math.Floor(d.Day / 7.0) + offset;
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

        private static readonly Dictionary<string, string> _windowsMapping =
            TzdbDateTimeZoneSource.Default.WindowsMapping.PrimaryMapping.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

        //public static readonly DateTimeZone LocalDateTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

        public static readonly DateTimeZone LocalDateTimeZone = GetLocalDateTimeZone();

        /// <summary>
        /// Alternative to NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault that works on both Windows and
        /// Linux/Mono systems.
        /// System.TimeZoneInfo.Local on Mono contains "Local" for the Local property.  NodaTime cannot deal with this,
        /// and throws DateTimeZoneNotFoundException whenever you call
        /// NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault().
        /// This workaround will, on PlatformID.Unix systems, manually open and read the content of the /etc/timezone
        /// file as the timezone ID.  Typically, this will be something like "US/Central" which NodaTime can indeed
        /// recognize and happily give us back a NodaTime.DateTimeZone.
        /// </summary>
        /// <remarks>Source: https://github.com/nodatime/nodatime/issues/235#issuecomment-80932114 </remarks>
        private static DateTimeZone GetLocalDateTimeZone()
        {
            // If we aren't running on Linux (which assumes Mono), then get the DTZ from NodaTime like normal.
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                return DateTimeZoneProviders.Tzdb.GetSystemDefault();
            }

            string sTimezoneId = null;

            if (File.Exists(UNIX_TIME_ZONE_CONFIG_FILE))
            {
                try
                {
                    using (var reader = File.OpenText(UNIX_TIME_ZONE_CONFIG_FILE))
                    {
                        sTimezoneId = reader.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not open and read '{UNIX_TIME_ZONE_CONFIG_FILE}'", ex);
                }
            }
            
            if(string.IsNullOrEmpty(sTimezoneId))
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "date +%Z",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    };
                    using (var p = Process.Start(psi))
                    {
                        if (p.WaitForExit(1000))
                        {
                            sTimezoneId = p.StandardOutput.ReadToEnd();
                        }
                        p.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not run date +%Z", ex);
                }
            }
            
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(string.IsNullOrEmpty(sTimezoneId) ? "UTC" : sTimezoneId);
        }

        public static DateTimeZone GetZone(string tzId)
        {
            if (string.IsNullOrWhiteSpace(tzId))
            {
                return LocalDateTimeZone;
            }

            if (tzId.StartsWith("/"))
            {
                tzId = tzId.Substring(1, tzId.Length - 1);
            }

            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            string ianaZone;
            if (_windowsMapping.TryGetValue(tzId, out ianaZone))
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

            if (_windowsMapping.Keys
                    .Where(tzId.Contains)
                    .Any(providerId => _windowsMapping.TryGetValue(providerId, out ianaZone))
               )
            {
                return DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaZone);
            }

            foreach (var providerId in DateTimeZoneProviders.Serialization.Ids.Where(tzId.Contains))
            {
                return DateTimeZoneProviders.Serialization.GetZoneOrNull(providerId);
            }

            return LocalDateTimeZone;
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
    }
}