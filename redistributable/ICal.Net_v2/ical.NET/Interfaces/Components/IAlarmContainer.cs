using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Interfaces.Components
{
    public interface IAlarmContainer
    {
        /// <summary>
        /// A list of <see cref="Components.Alarm"/>s for this recurring component.
        /// </summary>
        ICalendarObjectList<IAlarm> Alarms { get; }

        ///  <summary>
        ///  Polls <see cref="Alarm"/>s for occurrences within the <see cref="Evaluate"/>d
        ///  time frame of this <see cref="RecurringComponent"/>.  For each evaluated
        ///  occurrence if this component, each <see cref="Alarm"/> is polled for its
        ///  corresponding alarm occurrences.
        ///  <para>
        ///  <example>
        ///  The following is an example of polling alarms for an event.
        ///  <code>
        ///  IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        ///  IEvent evt = iCal.Events.First();
        /// 
        ///  // Poll the alarms on the event
        ///  List<AlarmOccurrence> alarms = evt.PollAlarms();
        ///  
        ///  // Here, you would eliminate alarms that the user has already dismissed.
        ///  // This information should be stored somewhere outside of the .ics file.
        ///  // You can use the component's UID, and the AlarmOccurence date/time 
        ///  // as the primary key for each alarm occurrence.
        ///  
        ///  foreach(AlarmOccurrence alarm in alarms)
        ///      MessageBox.Show(alarm.Component.Summary + "\n" + alarm.DateTime);</AlarmOccurrence>
        ///  </code>
        ///  </example>
        ///  </para>
        ///  </summary>
        /// <param name="startTime">The earliest allowable alarm occurrence to poll, or <c>null</c>.</param>
        /// <param name="endTime"></param>
        /// <returns>A List of <see cref="AlarmOccurrence"/> objects, one for each occurrence of the <see cref="Alarm"/>.</returns>
        IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime);
    }
}