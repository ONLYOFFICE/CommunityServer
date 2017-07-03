using System;
using Ical.Net.Interfaces.Components;

namespace Ical.Net
{
    /// <summary>
    /// Represents an RFC 5545 VTIMEZONE component.
    /// </summary>
    public class VTimeZone : CalendarComponent, ITimeZone
    {
        public static VTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(TimeZoneInfo.Local);
        }

        public static VTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return FromSystemTimeZone(TimeZoneInfo.Local, earlistDateTimeToSupport, includeHistoricalData);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        {
            // Support date/times for January 1st of the previous year by default.
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1), false);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return new VTimeZone {TzId = tzinfo.Id};
        }

        public VTimeZone()
        {
            Name = Components.Timezone;
        }

        public VTimeZone(string tzId) : this()
        {
            TzId = tzId;
        }

        private string _tzId;
        public virtual string TzId
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

        private Uri _url;
        public virtual Uri Url
        {
            get { return _url ?? (_url = Properties.Get<Uri>("TZURL")); }
            set
            {
                _url = value;
                Properties.Set("TZURL", _url);
            }
        }

        protected bool Equals(VTimeZone other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(TzId, other.TzId, StringComparison.OrdinalIgnoreCase)
                && Equals(Url, other.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VTimeZone)obj);
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