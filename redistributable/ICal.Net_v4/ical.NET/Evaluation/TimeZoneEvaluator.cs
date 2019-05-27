using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation
{
    public class TimeZoneEvaluator : Evaluator
    {
        protected VTimeZone TimeZone { get; set; }

        private List<Occurrence> _occurrences;
        public virtual List<Occurrence> Occurrences
        {
            get => _occurrences;
            set => _occurrences = value;
        }

        public TimeZoneEvaluator(VTimeZone tz)
        {
            TimeZone = tz;
            _occurrences = new List<Occurrence>();
        }

        void ProcessOccurrences(IDateTime referenceDate)
        {
            // Sort the occurrences by start time
            _occurrences.Sort(
                delegate (Occurrence o1, Occurrence o2)
                {
                    if (o1.Period?.StartTime == null)
                    {
                        return -1;
                    }
                    return o2.Period?.StartTime == null
                        ? 1
                        : o1.Period.StartTime.CompareTo(o2.Period.StartTime);
                }
            );

            for (var i = 0; i < _occurrences.Count; i++)
            {
                var curr = _occurrences[i];
                var next = i < _occurrences.Count - 1 ? _occurrences[i + 1] : null;

                // Determine end times for our periods, overwriting previously calculated end times.
                // This is important because we don't want to overcalculate our time zone information,
                // but simply calculate enough to be accurate.  When date/time ranges that are out of
                // normal working bounds are encountered, then occurrences are processed again, and
                // new end times are determined.
                curr.Period.EndTime = next != null
                    ? next.Period.StartTime.AddTicks(-1)
                    : ConvertToIDateTime(EvaluationEndBounds, referenceDate);
            }
        }

        public override void Clear()
        {
            base.Clear();
            _occurrences.Clear();
        }

        public override HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Ensure the reference date is associated with the time zone
            if (referenceDate.AssociatedObject == null)
                referenceDate.AssociatedObject = TimeZone;

            var infos = new List<VTimeZoneInfo>(TimeZone.TimeZoneInfos);

            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue)
                || periodEnd.Equals(EvaluationStartBounds)
                || periodStart.Equals(EvaluationEndBounds))
            {
                foreach (var curr in infos)
                {
                    var evaluator = curr.GetService(typeof(IEvaluator)) as IEvaluator;
                    Debug.Assert(curr.Start != null, "TimeZoneInfo.Start must not be null.");
                    Debug.Assert(curr.Start.TzId == null, "TimeZoneInfo.Start must not have a time zone reference.");
                    Debug.Assert(evaluator != null, "TimeZoneInfo.GetService(typeof(IEvaluator)) must not be null.");

                    // Time zones must include an effective start date/time
                    // and must provide an evaluator.
                    if (evaluator == null)
                    {
                        continue;
                    }

                    // Set the start bounds
                    if (EvaluationStartBounds > periodStart)
                    {
                        EvaluationStartBounds = periodStart;
                    }

                    // FIXME: 5 years is an arbitrary number, to eliminate the need
                    // to recalculate time zone information as much as possible.
                    var offsetEnd = periodEnd.AddYears(5);

                    // Determine the UTC occurrences of the Time Zone observances
                    var periods = evaluator.Evaluate(
                        referenceDate,
                        periodStart,
                        offsetEnd,
                        includeReferenceDateInResults);

                    foreach (var period in periods)
                    {
                        Periods.Add(period);
                        var o = new Occurrence(curr, period);
                        if (!_occurrences.Contains(o))
                        {
                            _occurrences.Add(o);
                        }
                    }

                    if (EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < offsetEnd)
                    {
                        EvaluationEndBounds = offsetEnd;
                    }
                }

                ProcessOccurrences(referenceDate);
            }
            else
            {
                if (EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds)
                {
                    Evaluate(referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults);
                }
            }

            return Periods;
        }
    }
}