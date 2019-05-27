using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation
{
    public class RecurringEvaluator : Evaluator
    {
        protected IRecurrable Recurrable { get; set; }

        public RecurringEvaluator(IRecurrable obj)
        {
            Recurrable = obj;

            // We're not sure if the object is a calendar object
            // or a calendar data type, so we need to assign
            // the associated object manually
            if (obj is ICalendarObject)
            {
                AssociatedObject = (ICalendarObject) obj;
            }
            if (obj is ICalendarDataType)
            {
                var dt = (ICalendarDataType) obj;
                AssociatedObject = dt.AssociatedObject;
            }
        }

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period to the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        /// <param name="includeReferenceDateInResults"></param>
        protected HashSet<Period> EvaluateRRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            if (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any())
            {
                return new HashSet<Period>();
            }

            var evaluator = Recurrable.RecurrenceRules.First().GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator == null)
            {
                return new HashSet<Period>();
            }
            var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);

            if (includeReferenceDateInResults)
            {
                periods.UnionWith(new[] { new Period(referenceDate) });
            }
            return periods;
        }

        /// <summary> Evalates the RDate component, and adds each specified DateTime or Period to the Periods collection. </summary>
        protected HashSet<Period> EvaluateRDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.RecurrenceDates == null || !Recurrable.RecurrenceDates.Any())
            {
                return new HashSet<Period>();
            }

            var recurrences = new HashSet<Period>(Recurrable.RecurrenceDates.SelectMany(rdate => rdate));
            return recurrences;
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        protected HashSet<Period> EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.ExceptionRules == null || !Recurrable.ExceptionRules.Any())
            {
                return new HashSet<Period>();
            }

            var evaluator = Recurrable.ExceptionRules.First().GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator == null)
            {
                return new HashSet<Period>();
            }

            var exRuleEvaluatorQuery = Recurrable.ExceptionRules.SelectMany(exRule => evaluator.Evaluate(referenceDate, periodStart, periodEnd, false));
            var exRuleExclusions = new HashSet<Period>(exRuleEvaluatorQuery);
            return exRuleExclusions;
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or Period from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        protected HashSet<Period> EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.ExceptionDates == null || !Recurrable.ExceptionDates.Any())
            {
                return new HashSet<Period>();
            }

            var exDates = new HashSet<Period>(Recurrable.ExceptionDates.SelectMany(exDate => exDate));
            return exDates;
        }

        public override HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            Periods.Clear();

            var rruleOccurrences = EvaluateRRule(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
            if (includeReferenceDateInResults)
            {
                rruleOccurrences.UnionWith(new[] { new Period(referenceDate), });
            }

            var rdateOccurrences = EvaluateRDate(referenceDate, periodStart, periodEnd);

            var exRuleExclusions = EvaluateExRule(referenceDate, periodStart, periodEnd);
            var exDateExclusions = EvaluateExDate(referenceDate, periodStart, periodEnd);

            //Exclusions trump inclusions
            Periods.UnionWith(rruleOccurrences);
            Periods.UnionWith(rdateOccurrences);
            Periods.ExceptWith(exRuleExclusions);
            Periods.ExceptWith(exDateExclusions);

            var dateOverlaps = FindDateOverlaps(exDateExclusions);
            Periods.ExceptWith(dateOverlaps);

            if (EvaluationStartBounds == DateTime.MaxValue || EvaluationStartBounds > periodStart)
            {
                EvaluationStartBounds = periodStart;
            }
            if (EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < periodEnd)
            {
                EvaluationEndBounds = periodEnd;
            }

            return Periods;
        }

        private HashSet<Period> FindDateOverlaps(HashSet<Period> dates)
        {
            var datesWithoutTimes = new HashSet<DateTime>(dates.Where(d => d.StartTime.Value.TimeOfDay == TimeSpan.Zero).Select(d => d.StartTime.Value));
            var overlaps = new HashSet<Period>(Periods.Where(p => datesWithoutTimes.Contains(p.StartTime.Value.Date)));
            return overlaps;
        }
    }
}