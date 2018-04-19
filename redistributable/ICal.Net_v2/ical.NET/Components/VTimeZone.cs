using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net
{
    /// <summary>
    /// Represents an RFC 5545 VTIMEZONE component.
    /// </summary>
    public class VTimeZone : CalendarComponent, ITimeZone
    {
        public static VTimeZone FromLocalTimeZone()
        {
            return FromDateTimeZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());
        }

        public static VTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return FromDateTimeZone(DateTimeZoneProviders.Tzdb.GetSystemDefault(), earlistDateTimeToSupport, includeHistoricalData);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        {
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1), false);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            var zone = BclDateTimeZone.FromTimeZoneInfo(tzinfo);
            return FromDateTimeZone(zone, earlistDateTimeToSupport, includeHistoricalData);
        }

        public static VTimeZone FromTzId(string tzId)
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId) ?? DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId) ?? DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            return FromDateTimeZone(zone);
        }

        public static VTimeZone FromTzId(string tzId, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId) ?? DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId) ?? DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            return FromDateTimeZone(zone, earlistDateTimeToSupport, includeHistoricalData);
        }

        private static VTimeZone FromDateTimeZone(DateTimeZone zone)
        {
            
            return FromDateTimeZone(zone, new DateTime(DateTime.Now.Year, 1, 1), false);
        }

        private static VTimeZone FromDateTimeZone(DateTimeZone zone, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            if (zone == null)
                throw new ArgumentException("Invalid time zone", "zone");

            var vTimeZone = new VTimeZone(zone.Id);

            var earliestYear = 1900;
            // Support date/times for January 1st of the previous year by default.
            if (earlistDateTimeToSupport.Year > 1900)
                earliestYear = earlistDateTimeToSupport.Year - 1;
            var earliest = Instant.FromUtc(earliestYear, earlistDateTimeToSupport.Month,
                earlistDateTimeToSupport.Day, earlistDateTimeToSupport.Hour, earlistDateTimeToSupport.Minute);

            // Only include historical data if asked to do so.  Otherwise,
            // use only the most recent adjustment rules available.
            var intervals = zone.GetZoneIntervals(earliest, SystemClock.Instance.Now).Where(x => x.Start != Instant.MinValue).ToList();

            var matchingDaylightIntervals = new List<ZoneInterval>();
            var matchingStandardIntervals = new List<ZoneInterval>();

            // if there are no intervals, create at least one standard interval
            if (!intervals.Any())
            {
                var start = new DateTimeOffset(new DateTime(earliestYear, 1, 1), new TimeSpan(zone.MaxOffset.Ticks));
                
                var interval = new ZoneInterval(zone.Id, Instant.FromDateTimeOffset(start), Instant.FromDateTimeOffset(start) + Duration.FromHours(1),  zone.MinOffset, Offset.Zero) ;
                intervals.Add(interval);
                var zoneInfo = CreateTimeZoneInfo(intervals, new List<ZoneInterval>(), true, true);
                vTimeZone.AddChild(zoneInfo);
            }
            else
            {
                // first, get the latest standard and daylight intervals, find the oldest recurring date in both, set the RRULES for it, and create a VTimeZoneInfos out of them.
                //standard
                var standardIntervals = intervals.Where(x => x.Savings.ToTimeSpan() == new TimeSpan(0)).ToList();
                var latestStandardInterval = standardIntervals.OrderByDescending(x => x.Start).FirstOrDefault();
                matchingStandardIntervals = GetMatchingIntervals(standardIntervals, latestStandardInterval, true);
                var latestStandardTimeZoneInfo = CreateTimeZoneInfo(matchingStandardIntervals, intervals);
                vTimeZone.AddChild(latestStandardTimeZoneInfo);

                // check to see if there is no active, future daylight savings (ie, America/Phoenix)
                if(latestStandardInterval != null && latestStandardInterval.End != Instant.MaxValue)
                {
                    //daylight
                    var daylightIntervals = intervals.Where(x => x.Savings.ToTimeSpan() != new TimeSpan(0)).ToList();

                    if (daylightIntervals.Any())
                    {
                        var latestDaylightInterval = daylightIntervals.OrderByDescending(x => x.Start).FirstOrDefault();
                        matchingDaylightIntervals = GetMatchingIntervals(daylightIntervals, latestDaylightInterval, true);
                        var latestDaylightTimeZoneInfo = CreateTimeZoneInfo(matchingDaylightIntervals, intervals);
                        vTimeZone.AddChild(latestDaylightTimeZoneInfo);
                    }
                }
            }

            if (!includeHistoricalData || intervals.Count == 1)
            {
                return vTimeZone;
            }
            
            // then, do the historic intervals, using RDATE for them
            var historicIntervals = intervals.Where(x => !matchingDaylightIntervals.Contains(x) && !matchingStandardIntervals.Contains(x)).ToList();

            while (historicIntervals.Any(x=>x.Start != Instant.MinValue))
            {
                var interval = historicIntervals.FirstOrDefault(x => x.Start != Instant.MinValue);

                if (interval == null)
                {
                    break;
                }
                        
                var matchedIntervals = GetMatchingIntervals(historicIntervals, interval);
                var timeZoneInfo = CreateTimeZoneInfo(matchedIntervals, intervals, false);
                vTimeZone.AddChild(timeZoneInfo);
                historicIntervals = historicIntervals.Where(x => !matchedIntervals.Contains(x)).ToList();
            }
            
            return vTimeZone;
        }

        private static ITimeZoneInfo CreateTimeZoneInfo(List<ZoneInterval> matchedIntervals, List<ZoneInterval> intervals, bool isRRule = true, bool isOnlyInterval = false)
        {
            if(matchedIntervals == null || !matchedIntervals.Any())
                throw new ArgumentException("No intervals found in matchedIntervals");

            var oldestInterval = matchedIntervals.OrderBy(x => x.Start).FirstOrDefault();

            if (oldestInterval == null)
                throw new InvalidOperationException("oldestInterval was not found");

            var previousInterval = intervals.SingleOrDefault(x => x.End == oldestInterval.Start);

            var delta = new TimeSpan(1,0,0);

            if (previousInterval != null)
            {
                delta = (previousInterval.WallOffset - oldestInterval.WallOffset).ToTimeSpan();
            }
            else if(isOnlyInterval)
            {
                delta = new TimeSpan();
            }
           
            var utcOffset = oldestInterval.StandardOffset.ToTimeSpan();

            var timeZoneInfo = new VTimeZoneInfo();

            var isDaylight = oldestInterval.Savings.Ticks > 0;

            if (isDaylight)
            {
                timeZoneInfo.Name = "DAYLIGHT";
                timeZoneInfo.OffsetFrom = new UtcOffset(utcOffset);
                timeZoneInfo.OffsetTo = new UtcOffset(utcOffset - delta);
            }
            else
            {
                timeZoneInfo.Name = "STANDARD";
                timeZoneInfo.OffsetFrom = new UtcOffset(utcOffset + delta);
                timeZoneInfo.OffsetTo = new UtcOffset(utcOffset);
            }

            timeZoneInfo.TimeZoneName = oldestInterval.Name;

            var start = oldestInterval.IsoLocalStart.ToDateTimeUnspecified() + delta;
            timeZoneInfo.Start = new CalDateTime(start) {HasTime = true};

            if (isRRule)
            {
                PopulateTimeZoneInfoRecurrenceRules(timeZoneInfo, oldestInterval);
            }
            else
            {
                PopulateTimeZoneInfoRecurrenceDates(timeZoneInfo, matchedIntervals, delta);
            }

            return timeZoneInfo;
        }

        private static List<ZoneInterval> GetMatchingIntervals(List<ZoneInterval> intervals, ZoneInterval intervalToMatch, bool consecutiveOnly = false)
        {
            var matchedIntervals = intervals.Where(x=>x.Start != Instant.MinValue)
                .Where(x => x.IsoLocalStart.Month == intervalToMatch.IsoLocalStart.Month 
                            && x.IsoLocalStart.Hour == intervalToMatch.IsoLocalStart.Hour
                            && x.IsoLocalStart.Minute == intervalToMatch.IsoLocalStart.Minute
                            && x.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek == intervalToMatch.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek
                            && x.WallOffset == intervalToMatch.WallOffset
                            && x.Name == intervalToMatch.Name)
                .ToList();

            if (!consecutiveOnly)
                return matchedIntervals;

            var consecutiveIntervals = new List<ZoneInterval>();

            var currentYear = 0;

            // return only the intervals where there are no gaps in years
            foreach (var interval in matchedIntervals.OrderByDescending(x=>x.IsoLocalStart.Year))
            {
                if (currentYear == 0)
                    currentYear = interval.IsoLocalStart.Year;

                if (currentYear != interval.IsoLocalStart.Year)
                    break;
                
                consecutiveIntervals.Add(interval);
                currentYear--;
            }

            return consecutiveIntervals;
        }

        private static void PopulateTimeZoneInfoRecurrenceDates(ITimeZoneInfo tzi, List<ZoneInterval> intervals, TimeSpan delta)
        {
            foreach (var interval in intervals)
            {
                var periodList = new PeriodList();
                var time = interval.IsoLocalStart.ToDateTimeUnspecified();
                var date = new CalDateTime(time).Add(delta) as CalDateTime;
                if (date == null) continue;
                date.HasTime = true;
                periodList.Add(date); 
                tzi.RecurrenceDates.Add(periodList);
            }
        }

        private static void PopulateTimeZoneInfoRecurrenceRules(ITimeZoneInfo tzi, ZoneInterval interval)
        {
            var recurrence = new IntervalRecurrencePattern(interval);
            tzi.RecurrenceRules.Add(recurrence);
        }

        private class IntervalRecurrencePattern : RecurrencePattern
        {
            public IntervalRecurrencePattern(ZoneInterval interval)
            {
                Frequency = FrequencyType.Yearly;
                ByMonth.Add(interval.IsoLocalStart.Month);

                var date = interval.IsoLocalStart.ToDateTimeUnspecified();
                var weekday = date.DayOfWeek;
                var num = DateUtil.WeekOfMonth(date);

                ByDay.Add(num != 5 ? new WeekDay(weekday, num) : new WeekDay(weekday, -1));
            }
        }

        public VTimeZone()
        {
            Name = Components.Timezone;
        }
        
        public VTimeZone(string tzId) : this()
        {
            if (string.IsNullOrEmpty(tzId))
            {
                return;
            }
            
            var zone = DateTimeZoneProviders.Bcl.Ids.SingleOrDefault(x => x == tzId);

            if (string.IsNullOrEmpty(zone))
            {
                zone = DateTimeZoneProviders.Tzdb.Ids.SingleOrDefault(x => x == tzId);
                
                if (string.IsNullOrEmpty(zone))
                {
                    zone = DateTimeZoneProviders.Serialization.Ids.SingleOrDefault(x => x == tzId);

                    if (string.IsNullOrEmpty(zone))
                    {
                        throw new ArgumentException($"The tzId ({zone}) is not a valid timezone");
                    }
                }
            }

            TzId = tzId;
            Location = tzId;
        }

        private string _tzId;
        public string TzId
        {
            get
            {
                if (string.IsNullOrEmpty(_tzId))
                {
                    _tzId = Properties.Get<string>("TZID");
                }
                return _tzId;
            }
            set
            {
                _tzId = value;
                Properties.Set("TZID", _tzId);
            }
        }

        public IDateTime LastModified { get; set; }

        private Uri _url;

        public Uri Url
        {
            get => _url ?? (_url = Properties.Get<Uri>("TZURL"));
            set
            {
                _url = value;
                Properties.Set("TZURL", _url);
            }
        }

        private string _location;
        public string Location
        {
            get => _location ?? (_location = Properties.Get<string>("X-LIC-LOCATION"));
            set
            {
                _location = value;
                Properties.Set("X-LIC-LOCATION", _location);
            }
        }

        public HashSet<ITimeZoneInfo> TimeZoneInfos { get; set; }

        private bool Equals(VTimeZone other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(TzId, other.TzId, StringComparison.OrdinalIgnoreCase)
                && Equals(Url, other.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((VTimeZone)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (TzId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Url?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}