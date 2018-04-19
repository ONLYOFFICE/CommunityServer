using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Utility;

namespace Ical.Net
{
    public class VTimeZoneInfo : CalendarComponent, ITimeZoneInfo
    {
        #region Private Fields

        TimeZoneInfoEvaluator _evaluator;
        DateTime _end;

        #endregion

        #region Constructors

        public VTimeZoneInfo() : base()
        {
            // FIXME: how do we ensure SEQUENCE doesn't get serialized?
            //base.Sequence = null;
            // iCalTimeZoneInfo does not allow sequence numbers
            // Perhaps we should have a custom serializer that fixes this?

            Initialize();
        }
        public VTimeZoneInfo(string name) : this()
        {
            this.Name = name;
        }

        void Initialize()
        {
            _evaluator = new TimeZoneInfoEvaluator(this);
            SetService(_evaluator);
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override bool Equals(object obj)
        {
            VTimeZoneInfo tzi = obj as VTimeZoneInfo;
            if (tzi != null)
            {
                return object.Equals(TimeZoneName, tzi.TimeZoneName) &&
                       object.Equals(OffsetFrom, tzi.OffsetFrom) &&
                       object.Equals(OffsetTo, tzi.OffsetTo);
            }
            return base.Equals(obj);
        }

        #endregion

        #region ITimeZoneInfo Members

        public virtual string TzId
        {
            get
            {
                ITimeZone tz = Parent as ITimeZone;
                if (tz != null)
                    return tz.TzId;
                return null;
            }
        }

        /// <summary>
        /// Returns the name of the current Time Zone.
        /// <example>
        ///     The following are examples:
        ///     <list type="bullet">
        ///         <item>EST</item>
        ///         <item>EDT</item>
        ///         <item>MST</item>
        ///         <item>MDT</item>
        ///     </list>
        /// </example>
        /// </summary>
        public virtual string TimeZoneName
        {
            get
            {
                if (TimeZoneNames.Count > 0)
                    return TimeZoneNames[0];
                return null;
            }
            set
            {
                TimeZoneNames.Clear();
                TimeZoneNames.Add(value);
            }
        }

        public virtual IUtcOffset TZOffsetFrom
        {
            get { return OffsetFrom; }
            set { OffsetFrom = value; }
        }

        public virtual IUtcOffset OffsetFrom
        {
            get { return Properties.Get<IUtcOffset>("TZOFFSETFROM"); }
            set { Properties.Set("TZOFFSETFROM", value); }
        }

        public virtual IUtcOffset OffsetTo
        {
            get { return Properties.Get<IUtcOffset>("TZOFFSETTO"); }
            set { Properties.Set("TZOFFSETTO", value); }
        }

        public virtual IUtcOffset TZOffsetTo
        {
            get { return OffsetTo; }
            set { OffsetTo = value; }
        }

        public virtual IList<string> TimeZoneNames
        {
            get { return Properties.GetMany<string>("TZNAME"); }
            set { Properties.Set("TZNAME", value); }
        }

        #endregion

        #region IRecurrable Members

        public virtual IDateTime DtStart
        {
            get { return Start; }
            set { Start = value; }
        }

        public virtual IDateTime Start
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

        #endregion

        #region IRecurrable Members

        public virtual void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        public virtual HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, true);
        }

        public virtual HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(dt), true);
        }

        public virtual HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, true);
        }

        public virtual HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(startTime), new CalDateTime(endTime), true);
        }

        #endregion
    }
}
