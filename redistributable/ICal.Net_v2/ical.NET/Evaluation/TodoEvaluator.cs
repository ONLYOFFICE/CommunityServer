using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation
{
    public class TodoEvaluator : RecurringEvaluator
    {
        protected ITodo Todo => Recurrable as ITodo;

        public TodoEvaluator(ITodo todo) : base(todo) {}

        public void EvaluateToPreviousOccurrence(IDateTime completedDate, IDateTime currDt)
        {
            var beginningDate = completedDate.Copy<IDateTime>();

            if (Todo.RecurrenceRules != null)
            {
                foreach (var rrule in Todo.RecurrenceRules)
                {
                    DetermineStartingRecurrence(rrule, ref beginningDate);
                }
            }
            if (Todo.RecurrenceDates != null)
            {
                foreach (var rdate in Todo.RecurrenceDates)
                {
                    DetermineStartingRecurrence(rdate, ref beginningDate);
                }
            }
            if (Todo.ExceptionRules != null)
            {
                foreach (var exrule in Todo.ExceptionRules)
                {
                    DetermineStartingRecurrence(exrule, ref beginningDate);
                }
            }
            if (Todo.ExceptionDates != null)
            {
                foreach (var exdate in Todo.ExceptionDates)
                {
                    DetermineStartingRecurrence(exdate, ref beginningDate);
                }
            }

            Evaluate(Todo.Start, DateUtil.GetSimpleDateTimeData(beginningDate), DateUtil.GetSimpleDateTimeData(currDt).AddTicks(1), true);
        }

        public void DetermineStartingRecurrence(IPeriodList rdate, ref IDateTime referenceDateTime)
        {
            var evaluator = rdate.GetService<IEvaluator>();

            var dt2 = referenceDateTime;
            foreach (var p in evaluator.Periods.Where(p => p.StartTime.LessThan(dt2)))
            {
                referenceDateTime = p.StartTime;
            }
        }

        public void DetermineStartingRecurrence(IRecurrencePattern recur, ref IDateTime referenceDateTime)
        {
            if (recur.Count != int.MinValue)
            {
                referenceDateTime = Todo.Start.Copy<IDateTime>();
            }
            else
            {
                var dtVal = referenceDateTime.Value;
                IncrementDate(ref dtVal, recur, -recur.Interval);
                referenceDateTime.Value = dtVal;
            }
        }

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // TODO items can only recur if a start date is specified
            if (Todo.Start == null)
            {
                return new HashSet<IPeriod>();
            }

            base.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);

            // Ensure each period has a duration
            foreach (var period in Periods.Where(period => period.EndTime == null))
            {
                period.Duration = Todo.Duration;
                if (period.Duration != null)
                {
                    period.EndTime = period.StartTime.Add(Todo.Duration);
                }
                else
                {
                    period.Duration = Todo.Duration;
                }
            }
            return Periods;
        }
    }
}