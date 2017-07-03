using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Components
{
    public interface IAlarm : ICalendarComponent
    {
        AlarmAction Action { get; set; }
        IAttachment Attachment { get; set; }
        IList<IAttendee> Attendees { get; set; }
        string Description { get; set; }
        TimeSpan Duration { get; set; }
        int Repeat { get; set; }
        string Summary { get; set; }
        ITrigger Trigger { get; set; }

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="fromDate"/> and <paramref name="toDate"/>.
        /// </summary>
        IList<AlarmOccurrence> GetOccurrences(IRecurringComponent rc, IDateTime fromDate, IDateTime toDate);

        /// <summary>
        /// Polls the <see cref="Components.Alarm"/> component for alarms that have been triggered
        /// since the provided <paramref name="fromDate"/> date/time.  If <paramref name="fromDate"/>
        /// is null, all triggered alarms will be returned.
        /// </summary>
        /// <param name="fromDate">The earliest date/time to poll trigerred alarms for.</param>
        /// <param name="toDate">The latest date/time to poll trigerred alarms for.</param>
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        IList<AlarmOccurrence> Poll(IDateTime fromDate, IDateTime toDate);
    }
}