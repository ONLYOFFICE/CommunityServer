using System;
using System.Collections.Generic;
using System.Globalization;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Interfaces.General;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation
{
    public abstract class Evaluator : IEvaluator
    {
        private DateTime _mEvaluationStartBounds = DateTime.MaxValue;
        private DateTime _mEvaluationEndBounds = DateTime.MinValue;

        private ICalendarObject _mAssociatedObject;
        private readonly ICalendarDataType _mAssociatedDataType;

        protected HashSet<IPeriod> MPeriods;

        protected Evaluator()
        {
            Initialize();
        }

        protected Evaluator(ICalendarObject associatedObject)
        {
            _mAssociatedObject = associatedObject;

            Initialize();
        }

        protected Evaluator(ICalendarDataType dataType)
        {
            _mAssociatedDataType = dataType;

            Initialize();
        }

        private void Initialize()
        {
            Calendar = CultureInfo.CurrentCulture.Calendar;
            MPeriods = new HashSet<IPeriod>();
        }

        protected IDateTime ConvertToIDateTime(DateTime dt, IDateTime referenceDate)
        {
            IDateTime newDt = new CalDateTime(dt, referenceDate.TzId);
            newDt.AssociateWith(referenceDate);
            return newDt;
        }

        protected void IncrementDate(ref DateTime dt, IRecurrencePattern pattern, int interval)
        {
            // FIXME: use a more specific exception.
            if (interval == 0)
            {
                throw new Exception("Cannot evaluate with an interval of zero.  Please use an interval other than zero.");
            }

            var old = dt;
            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly:
                    dt = old.AddSeconds(interval);
                    break;
                case FrequencyType.Minutely:
                    dt = old.AddMinutes(interval);
                    break;
                case FrequencyType.Hourly:
                    dt = old.AddHours(interval);
                    break;
                case FrequencyType.Daily:
                    dt = old.AddDays(interval);
                    break;
                case FrequencyType.Weekly:
                    dt = DateUtil.AddWeeks(old, interval, pattern.FirstDayOfWeek);
                    break;
                case FrequencyType.Monthly:
                    dt = old.AddDays(-old.Day + 1).AddMonths(interval);
                    break;
                case FrequencyType.Yearly:
                    dt = old.AddDays(-old.DayOfYear + 1).AddYears(interval);
                    break;
                // FIXME: use a more specific exception.
                default:
                    throw new Exception("FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.");
            }
        }

        public System.Globalization.Calendar Calendar { get; private set; }

        public virtual DateTime EvaluationStartBounds
        {
            get { return _mEvaluationStartBounds; }
            set { _mEvaluationStartBounds = value; }
        }

        public virtual DateTime EvaluationEndBounds
        {
            get { return _mEvaluationEndBounds; }
            set { _mEvaluationEndBounds = value; }
        }

        public virtual ICalendarObject AssociatedObject
        {
            get
            {
                return _mAssociatedObject ?? _mAssociatedDataType?.AssociatedObject;
            }
            protected set { _mAssociatedObject = value; }
        }

        public virtual HashSet<IPeriod> Periods => MPeriods;

        public virtual void Clear()
        {
            _mEvaluationStartBounds = DateTime.MaxValue;
            _mEvaluationEndBounds = DateTime.MinValue;
            MPeriods.Clear();
        }

        public abstract HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults);
    }
}