using System;
using Ical.Net.CalendarComponents;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents a specific occurrence of an <see cref="Alarm"/>.        
    /// </summary>
    /// <remarks>
    /// The <see cref="AlarmOccurrence"/> contains the <see cref="Period"/> when
    /// the alarm occurs, the <see cref="Alarm"/> that fired, and the 
    /// component on which the alarm fired.
    /// </remarks>
    public class AlarmOccurrence : IComparable<AlarmOccurrence>
    {
        public Period Period { get; set; }

        public IRecurringComponent Component { get; set; }

        public Alarm Alarm { get; set; }

        public IDateTime DateTime
        {
            get => Period.StartTime;
            set => Period = new Period(value);
        }

        public AlarmOccurrence(AlarmOccurrence ao)
        {
            Period = ao.Period;
            Component = ao.Component;
            Alarm = ao.Alarm;
        }

        public AlarmOccurrence(Alarm a, IDateTime dt, IRecurringComponent rc)
        {
            Alarm = a;
            Period = new Period(dt);
            Component = rc;
        }

        public int CompareTo(AlarmOccurrence other) => Period.CompareTo(other.Period);

        protected bool Equals(AlarmOccurrence other)
            => Equals(Period, other.Period) 
                && Equals(Component, other.Component)
                && Equals(Alarm, other.Alarm);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((AlarmOccurrence)obj);
        }

        public override int GetHashCode()
        {
            // ToDo: Alarm doesn't implement Equals or GetHashCode()
            unchecked
            {
                var hashCode = Period?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Component?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Alarm?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}