using System;
using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Interfaces.Evaluation
{
    public interface IEvaluator
    {
        /// <summary>
        /// The system calendar that governs the evaluation rules.
        /// </summary>
        System.Globalization.Calendar Calendar { get; }

        /// <summary>
        /// The start bounds of the evaluation.  This gives
        /// the first date/time that is covered by the evaluation.
        /// This together with EvaluationEndBounds determines
        /// what time frames have already been evaluated, so
        /// duplicate evaluation doesn't occur.
        /// </summary>
        DateTime EvaluationStartBounds { get; }

        /// <summary>
        /// The end bounds of the evaluation.
        /// See <see cref="EvaluationStartBounds"/> for more info.
        /// </summary>
        DateTime EvaluationEndBounds { get; }

        /// <summary>
        /// Gets a list of periods collected so far during
        /// the evaluation process.
        /// </summary>
        HashSet<IPeriod> Periods { get; }

        /// <summary>
        /// Gets the object associated with this evaluator.
        /// </summary>
        ICalendarObject AssociatedObject { get; }

        /// <summary>
        /// Clears the evaluation, eliminating all data that has
        /// been collected up to this point.  Since this data is cached
        /// as needed, this method can be useful to gather information
        /// that is guaranteed to not be out-of-date.
        /// </summary>
        void Clear();

        /// <summary>
        /// Evaluates this item to determine the dates and times for which it occurs/recurs.
        /// This method only evaluates items which occur/recur between <paramref name="periodStart"/>
        /// and <paramref name="periodEnd"/>; therefore, if you require a list of items which
        /// occur outside of this range, you must specify a <paramref name="periodStart"/> and
        /// <paramref name="periodEnd"/> which encapsulate the date(s) of interest.
        /// This method evaluates using the <paramref name="periodStart" /> as the beginning
        /// point.  For example, for a WEEKLY occurrence, the <paramref name="periodStart"/>
        /// determines the day of week that this item will recur on.
        /// <note type="caution">
        ///     For events with very complex recurrence rules, this method may be a bottleneck
        ///     during processing time, especially when this method is called for a large number
        ///     of items, in sequence, or for a very large time span.
        /// </note>
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart"></param>
        /// <param name="periodEnd"></param>
        /// <param name="includeReferenceDateInResults"></param>
        /// <returns>
        ///     A list of <see cref="System.DateTime"/> objects for
        ///     each date/time when this item occurs/recurs.
        /// </returns>
        HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults);
    }
}