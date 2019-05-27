using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation
{
    public class TimeZoneInfoEvaluator : RecurringEvaluator
    {
        protected VTimeZoneInfo TimeZoneInfo
        {
            get => Recurrable as VTimeZoneInfo;
            set => Recurrable = value;
        }

        public TimeZoneInfoEvaluator(IRecurrable tzi) : base(tzi) { }

        public override HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Time zones must include an effective start date/time
            // and must provide an evaluator.
            if (TimeZoneInfo == null)
            {
                return new HashSet<Period>();
            }

            // Always include the reference date in the results
            var periods = base.Evaluate(referenceDate, periodStart, periodEnd, true);
            return periods;
        }
    }
}