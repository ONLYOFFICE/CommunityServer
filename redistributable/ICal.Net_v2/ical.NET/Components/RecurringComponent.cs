using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.General.Proxies;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Utility;

namespace Ical.Net
{
    /// <summary>
    /// An iCalendar component that recurs.
    /// </summary>
    /// <remarks>
    /// This component automatically handles
    /// RRULEs, RDATE, EXRULEs, and EXDATEs, as well as the DTSTART
    /// for the recurring item (all recurring items must have a DTSTART).
    /// </remarks>
    public class RecurringComponent : UniqueComponent, IRecurringComponent
    {
        public static IEnumerable<IRecurringComponent> SortByDate(IEnumerable<IRecurringComponent> list)
        {
            return SortByDate<IRecurringComponent>(list);
        }

        public static IEnumerable<TRecurringComponent> SortByDate<TRecurringComponent>(IEnumerable<TRecurringComponent> list) => list.OrderBy(d => d);

        protected virtual bool EvaluationIncludesReferenceDate => false;

        public virtual IList<IAttachment> Attachments
        {
            get { return Properties.GetMany<IAttachment>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

        public virtual IList<string> Categories
        {
            get { return Properties.GetMany<string>("CATEGORIES"); }
            set { Properties.Set("CATEGORIES", value); }
        }

        public virtual string Class
        {
            get { return Properties.Get<string>("CLASS"); }
            set { Properties.Set("CLASS", value); }
        }

        public virtual IList<string> Contacts
        {
            get { return Properties.GetMany<string>("CONTACT"); }
            set { Properties.Set("CONTACT", value); }
        }

        public virtual IDateTime Created
        {
            get { return Properties.Get<IDateTime>("CREATED"); }
            set { Properties.Set("CREATED", value); }
        }

        public virtual string Description
        {
            get { return Properties.Get<string>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

        /// <summary>
        /// The start date/time of the component.
        /// </summary>
        public virtual IDateTime DtStart
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        public virtual IList<IPeriodList> ExceptionDates
        {
            get { return Properties.GetMany<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        public virtual IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        public virtual IDateTime LastModified
        {
            get { return Properties.Get<IDateTime>("LAST-MODIFIED"); }
            set { Properties.Set("LAST-MODIFIED", value); }
        }

        public virtual int Priority
        {
            get { return Properties.Get<int>("PRIORITY"); }
            set { Properties.Set("PRIORITY", value); }
        }

        public virtual IList<IPeriodList> RecurrenceDates
        {
            get { return Properties.GetMany<IPeriodList>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        public virtual IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("RRULE"); }
            set { Properties.Set("RRULE", value); }
        }

        public virtual IDateTime RecurrenceId
        {
            get { return Properties.Get<IDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        public virtual IList<string> RelatedComponents
        {
            get { return Properties.GetMany<string>("RELATED-TO"); }
            set { Properties.Set("RELATED-TO", value); }
        }

        public virtual int Sequence
        {
            get { return Properties.Get<int>("SEQUENCE"); }
            set { Properties.Set("SEQUENCE", value); }
        }

        /// <summary>
        /// An alias to the DTStart field (i.e. start date/time).
        /// </summary>
        public virtual IDateTime Start
        {
            get { return DtStart; }
            set { DtStart = value; }
        }

        public virtual string Summary
        {
            get { return Properties.Get<string>("SUMMARY"); }
            set { Properties.Set("SUMMARY", value); }
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
        public virtual ICalendarObjectList<IAlarm> Alarms => new CalendarObjectListProxy<IAlarm>(Children);

        public RecurringComponent()
        {
            Initialize();
            EnsureProperties();
        }

        public RecurringComponent(string name) : base(name)
        {
            Initialize();
            EnsureProperties();
        }

        private void Initialize()
        {
            SetService(new RecurringEvaluator(this));
        }

        private void EnsureProperties()
        {
            if (!Properties.ContainsKey("SEQUENCE"))
            {
                Sequence = 0;
            }
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public virtual void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        public virtual HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, EvaluationIncludesReferenceDate);
        }

        public virtual HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(dt), EvaluationIncludesReferenceDate);
        }

        public virtual HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, EvaluationIncludesReferenceDate);
        }

        public virtual HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(startTime), new CalDateTime(endTime), EvaluationIncludesReferenceDate);
        }

        public virtual IList<AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(null, null);
        }

        public virtual IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime)
        {
            if (Alarms == null || !Alarms.Any())
            {
                return new List<AlarmOccurrence>();
            }

            var occurrences = new List<AlarmOccurrence>(16);
            foreach (var alarm in Alarms)
            {
                occurrences.AddRange(alarm.Poll(startTime, endTime));
            }
            return occurrences;
        }

        protected bool Equals(RecurringComponent other)
        {
            var result = Equals(DtStart, other.DtStart)
                && Equals(Priority, other.Priority)
                && string.Equals(Summary, other.Summary, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Class, other.Class, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase)
                && Equals(RecurrenceId, other.RecurrenceId)
                && Attachments.SequenceEqual(other.Attachments)
                && CollectionHelpers.Equals(Categories, other.Categories)
                && CollectionHelpers.Equals(Contacts, other.Contacts)
                && CollectionHelpers.Equals(ExceptionDates, other.ExceptionDates)
                && CollectionHelpers.Equals(ExceptionRules, other.ExceptionRules)
                && CollectionHelpers.Equals(RecurrenceDates, other.RecurrenceDates, orderSignificant: true)
                && CollectionHelpers.Equals(RecurrenceRules, other.RecurrenceRules, orderSignificant: true);

            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RecurringComponent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DtStart?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Priority.GetHashCode();
                hashCode = (hashCode * 397) ^ (Summary?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Class?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (RecurrenceId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Attachments);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Categories);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Contacts);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionDates);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionRules);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceDates);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceRules);
                return hashCode;
            }
        }
    }
}