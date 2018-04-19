using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Evaluation
{
    public class TimeZoneInfoEvaluator : RecurringEvaluator
    {
        #region Protected Properties

        protected ITimeZoneInfo TimeZoneInfo
        {
            get => Recurrable as ITimeZoneInfo;
            set => Recurrable = value;
        }

        #endregion

        public TimeZoneInfoEvaluator(IRecurrable tzi) : base(tzi)
        {

        }

        #region Overrides

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Time zones must include an effective start date/time
            // and must provide an evaluator.
            if (TimeZoneInfo != null)
            {
                // Always include the reference date in the results
                HashSet<IPeriod> periods = base.Evaluate(referenceDate, periodStart, periodEnd, true);
                return periods;
            }

            return new HashSet<IPeriod>();
        }

        #endregion
    }
}
