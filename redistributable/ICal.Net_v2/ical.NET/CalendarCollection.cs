using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net
{
    /// <summary>
    /// A list of iCalendars.
    /// </summary>
    public class CalendarCollection : List<ICalendar>, IICalendarCollection
    {
        public void ClearEvaluation()
        {
            foreach (var iCal in this)
            {
                iCal.ClearEvaluation();
            }
        }

        public HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(startTime, endTime));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(startTime, endTime));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(DateTime dt) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(startTime, endTime));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(DateTime startTime, DateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(startTime, endTime));
            }
            return occurrences;
        }

        private IFreeBusy CombineFreeBusy(IFreeBusy main, IFreeBusy current)
        {
            main?.MergeWith(current);
            return current;
        }

        public IFreeBusy GetFreeBusy(IFreeBusy freeBusyRequest)
        {
            return this.Aggregate<ICalendar, IFreeBusy>(null, (current, iCal) => CombineFreeBusy(current, iCal.GetFreeBusy(freeBusyRequest)));
        }

        public IFreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive)
        {
            return this.Aggregate<ICalendar, IFreeBusy>(null, (current, iCal) => CombineFreeBusy(current, iCal.GetFreeBusy(fromInclusive, toExclusive)));
        }

        public IFreeBusy GetFreeBusy(IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive)
        {
            return this.Aggregate<ICalendar, IFreeBusy>(null, (current, iCal) => CombineFreeBusy(current, iCal.GetFreeBusy(organizer, contacts, fromInclusive, toExclusive)));
        }

        public override int GetHashCode() => CollectionHelpers.GetHashCode(this);

        protected bool Equals(CalendarCollection obj) => CollectionHelpers.Equals(this, obj);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Event)obj);
        }
    }
}