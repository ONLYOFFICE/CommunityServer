using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.CalendarComponents
{
    /// <summary>
    /// A class that represents an RFC 2445 VALARM component.
    /// FIXME: move GetOccurrences() logic into an AlarmEvaluator.
    /// </summary>    
    public class Alarm : CalendarComponent
    {
        //ToDo: Implement IEquatable
        public virtual string Action
        {
            get => Properties.Get<string>(AlarmAction.Key);
            set => Properties.Set(AlarmAction.Key, value);
        }

        public virtual Attachment Attachment
        {
            get => Properties.Get<Attachment>("ATTACH");
            set => Properties.Set("ATTACH", value);
        }

        public virtual IList<Attendee> Attendees
        {
            get => Properties.GetMany<Attendee>("ATTENDEE");
            set => Properties.Set("ATTENDEE", value);
        }

        public virtual string Description
        {
            get => Properties.Get<string>("DESCRIPTION");
            set => Properties.Set("DESCRIPTION", value);
        }

        public virtual TimeSpan Duration
        {
            get => Properties.Get<TimeSpan>("DURATION");
            set => Properties.Set("DURATION", value);
        }

        public virtual int Repeat
        {
            get => Properties.Get<int>("REPEAT");
            set => Properties.Set("REPEAT", value);
        }

        public virtual string Summary
        {
            get => Properties.Get<string>("SUMMARY");
            set => Properties.Set("SUMMARY", value);
        }

        public virtual Trigger Trigger
        {
            get => Properties.Get<Trigger>(TriggerRelation.Key);
            set => Properties.Set(TriggerRelation.Key, value);
        }

        protected virtual IList<AlarmOccurrence> Occurrences { get; set; }

        public Alarm()
        {
            Name = Components.Alarm;
            Occurrences = new List<AlarmOccurrence>();
        }

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="fromDate"/> and <paramref name="toDate"/>.
        /// </summary>
        public virtual IList<AlarmOccurrence> GetOccurrences(IRecurringComponent rc, IDateTime fromDate, IDateTime toDate)
        {
            Occurrences.Clear();

            if (Trigger == null)
            {
                return Occurrences;
            }

            // If the trigger is relative, it can recur right along with
            // the recurring items, otherwise, it happens once and
            // only once (at a precise time).
            if (Trigger.IsRelative)
            {
                // Ensure that "FromDate" has already been set
                if (fromDate == null)
                {
                    fromDate = rc.Start.Copy<IDateTime>();
                }

                var d = default(TimeSpan);
                foreach (var o in rc.GetOccurrences(fromDate, toDate))
                {
                    var dt = o.Period.StartTime;
                    if (string.Equals(Trigger.Related, TriggerRelation.End, TriggerRelation.Comparison))
                    {
                        if (o.Period.EndTime != null)
                        {
                            dt = o.Period.EndTime;
                            if (d == default(TimeSpan))
                            {
                                d = o.Period.Duration;
                            }
                        }
                        // Use the "last-found" duration as a reference point
                        else if (d != default(TimeSpan))
                        {
                            dt = o.Period.StartTime.Add(d);
                        }
                        else
                        {
                            throw new ArgumentException(
                                "Alarm trigger is relative to the START of the occurrence; however, the occurence has no discernible end.");
                        }
                    }

                    Occurrences.Add(new AlarmOccurrence(this, dt.Add(Trigger.Duration.Value), rc));
                }
            }
            else
            {
                var dt = Trigger.DateTime.Copy<IDateTime>();
                dt.AssociatedObject = this;
                Occurrences.Add(new AlarmOccurrence(this, dt, rc));
            }

            // If a REPEAT and DURATION value were specified,
            // then handle those repetitions here.
            AddRepeatedItems();

            return Occurrences;
        }

        /// <summary>
        /// Polls the <see cref="Alarm"/> component for alarms that have been triggered
        /// since the provided <paramref name="start"/> date/time.  If <paramref name="start"/>
        /// is null, all triggered alarms will be returned.
        /// </summary>
        /// <param name="start">The earliest date/time to poll trigered alarms for.</param>
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        public virtual IList<AlarmOccurrence> Poll(IDateTime start, IDateTime end)
        {
            var results = new List<AlarmOccurrence>();

            // Evaluate the alarms to determine the recurrences
            var rc = Parent as RecurringComponent;
            if (rc == null)
            {
                return results;
            }

            results.AddRange(GetOccurrences(rc, start, end));
            return results;
        }

        /// <summary>
        /// Handles the repetitions that occur from the <c>REPEAT</c> and
        /// <c>DURATION</c> properties.  Each recurrence of the alarm will
        /// have its own set of generated repetitions.
        /// </summary>
        protected virtual void AddRepeatedItems()
        {
            var len = Occurrences.Count;
            for (var i = 0; i < len; i++)
            {
                var ao = Occurrences[i];
                var alarmTime = ao.DateTime.Copy<IDateTime>();

                for (var j = 0; j < Repeat; j++)
                {
                    alarmTime = alarmTime.Add(Duration);
                    Occurrences.Add(new AlarmOccurrence(this, alarmTime.Copy<IDateTime>(), ao.Component));
                }
            }
        }
    }
}