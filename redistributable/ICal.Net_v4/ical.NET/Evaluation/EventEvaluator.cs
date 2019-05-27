using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation
{
    public class EventEvaluator : RecurringEvaluator
    {
        protected CalendarEvent CalendarEvent
        {
            get => Recurrable as CalendarEvent;
            set => Recurrable = value;
        }

        public EventEvaluator(CalendarEvent evt) : base(evt) {}

        /// <summary>
        /// Evaluates this event to determine the dates and times for which the event occurs.
        /// This method only evaluates events which occur between <paramref name="periodStart"/>
        /// and <paramref name="periodEnd"/>; therefore, if you require a list of events which
        /// occur outside of this range, you must specify a <paramref name="periodStart"/> and
        /// <paramref name="periodEnd"/> which encapsulate the date(s) of interest.
        /// <note type="caution">
        ///     For events with very complex recurrence rules, this method may be a bottleneck
        ///     during processing time, especially when this method in called for a large number
        ///     of events, in sequence, or for a very large time span.
        /// </note>
        /// </summary>
        /// <param name="referenceTime"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        /// <param name="includeReferenceDateInResults"></param>
        /// <returns></returns>
        public override HashSet<Period> Evaluate(IDateTime referenceTime, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Evaluate recurrences normally
            base.Evaluate(referenceTime, periodStart, periodEnd, includeReferenceDateInResults);

            foreach (var period in Periods)
            {
                period.Duration = CalendarEvent.Duration;
                period.EndTime = period.Duration == null
                    ? period.StartTime
                    : period.StartTime.Add(CalendarEvent.Duration);
            }

            // Ensure each period has a duration
            foreach (var period in Periods.Where(p => p.EndTime == null))
            {
                period.Duration = CalendarEvent.Duration;
                period.EndTime = period.Duration == null
                    ? period.StartTime
                    : period.StartTime.Add(CalendarEvent.Duration);
            }

            return Periods;
        }
    }
}