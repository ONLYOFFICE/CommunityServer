using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

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
        public IPeriod Period { get; set; }

        public IRecurringComponent Component { get; set; }

        public IAlarm Alarm { get; set; }

        public IDateTime DateTime
        {
            get { return Period.StartTime; }
            set { Period = new Period(value); }
        }

        public AlarmOccurrence(AlarmOccurrence ao)
        {
            Period = ao.Period;
            Component = ao.Component;
            Alarm = ao.Alarm;
        }

        public AlarmOccurrence(IAlarm a, IDateTime dt, IRecurringComponent rc)
        {
            Alarm = a;
            Period = new Period(dt);
            Component = rc;
        }

        public int CompareTo(AlarmOccurrence other)
        {
            return Period.CompareTo(other.Period);
        }

        protected bool Equals(AlarmOccurrence other)
        {
            return Equals(Period, other.Period) && Equals(Component, other.Component) && Equals(Alarm, other.Alarm);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AlarmOccurrence)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Period != null
                    ? Period.GetHashCode()
                    : 0);
                hashCode = (hashCode * 397) ^ (Component != null
                    ? Component.GetHashCode()
                    : 0);
                hashCode = (hashCode * 397) ^ (Alarm != null
                    ? Alarm.GetHashCode()
                    : 0);
                return hashCode;
            }
        }
    }
}