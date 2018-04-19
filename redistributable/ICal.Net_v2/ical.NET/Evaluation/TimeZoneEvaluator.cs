using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Evaluation
{
    public class TimeZoneEvaluator : Evaluator
    {
        #region Private Fields

        private List<Occurrence> _occurrences;

        #endregion

        #region Protected Properties

        protected ITimeZone TimeZone { get; set; }

        #endregion

        #region Public Properties

        public virtual List<Occurrence> Occurrences
        {
            get { return _occurrences; }
            set { _occurrences = value; }
        }
        #endregion

        #region Constructors

        public TimeZoneEvaluator(ITimeZone tz)
        {
            TimeZone = tz;
            _occurrences = new List<Occurrence>();
        }

        #endregion

        #region Private Methods

        void ProcessOccurrences(IDateTime referenceDate)
        {
            // Sort the occurrences by start time
            _occurrences.Sort(
                delegate (Occurrence o1, Occurrence o2)
                {
                    if (o1.Period?.StartTime == null)
                        return -1;
                    if (o2.Period?.StartTime == null)
                        return 1;
                    return o1.Period.StartTime.CompareTo(o2.Period.StartTime);
                }
            );

            for (int i = 0; i < _occurrences.Count; i++)
            {
                var curr = _occurrences[i];
                var next = i < _occurrences.Count - 1 ? _occurrences[i + 1] : null;

                // Determine end times for our periods, overwriting previously calculated end times.
                // This is important because we don't want to overcalculate our time zone information,
                // but simply calculate enough to be accurate.  When date/time ranges that are out of
                // normal working bounds are encountered, then occurrences are processed again, and
                // new end times are determined.
                if (next != null)
                {
                    curr.Period.EndTime = next.Period.StartTime.AddTicks(-1);
                }
                else
                {
                    curr.Period.EndTime = ConvertToIDateTime(EvaluationEndBounds, referenceDate);
                }
            }
        }


        #endregion


        #region Overrides

        public override void Clear()
        {
            base.Clear();
            _occurrences.Clear();
        }

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Ensure the reference date is associated with the time zone
            if (referenceDate.AssociatedObject == null)
                referenceDate.AssociatedObject = TimeZone;

            List<ITimeZoneInfo> infos = new List<ITimeZoneInfo>(TimeZone.TimeZoneInfos);

            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue) ||
                (periodEnd.Equals(EvaluationStartBounds)) ||
                (periodStart.Equals(EvaluationEndBounds)))
            {
                foreach (ITimeZoneInfo curr in infos)
                {
                    IEvaluator evaluator = curr.GetService(typeof(IEvaluator)) as IEvaluator;
                    Debug.Assert(curr.Start != null, "TimeZoneInfo.Start must not be null.");
                    Debug.Assert(curr.Start.TzId == null, "TimeZoneInfo.Start must not have a time zone reference.");
                    Debug.Assert(evaluator != null, "TimeZoneInfo.GetService(typeof(IEvaluator)) must not be null.");

                    // Time zones must include an effective start date/time
                    // and must provide an evaluator.
                    if (evaluator != null)
                    {
                        // Set the start bounds
                        if (EvaluationStartBounds > periodStart)
                            EvaluationStartBounds = periodStart;

                        // FIXME: 5 years is an arbitrary number, to eliminate the need
                        // to recalculate time zone information as much as possible.
                        DateTime offsetEnd = periodEnd.AddYears(5);

                        // Determine the UTC occurrences of the Time Zone observances
                        HashSet<IPeriod> periods = evaluator.Evaluate(
                            referenceDate,
                            periodStart,
                            offsetEnd,
                            includeReferenceDateInResults);

                        foreach (IPeriod period in periods)
                        {
                            if (!Periods.Contains(period))
                                Periods.Add(period);

                            Occurrence o = new Occurrence(curr, period);
                            if (!_occurrences.Contains(o))
                                _occurrences.Add(o);
                        }

                        if (EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < offsetEnd)
                            EvaluationEndBounds = offsetEnd;
                    }
                }

                ProcessOccurrences(referenceDate);
            }
            else
            {
                if (EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds)
                    Evaluate(referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults);
            }

            return Periods;
        }

        #endregion
    }
}
