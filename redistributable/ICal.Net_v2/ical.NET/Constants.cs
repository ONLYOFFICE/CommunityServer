namespace Ical.Net
{
    public enum AlarmAction
    {
        Audio,
        Display,
        Email,
        Procedure
    }

    public enum TriggerRelation
    {
        Start,
        End
    }

    public class Components
    {
        public const string Alarm = "VALARM";
        public const string Calendar = "VCALENDAR";
        public const string Event = "VEVENT";
        public const string Freebusy = "VFREEBUSY";
        public const string Todo = "VTODO";
        public const string Journal = "VJOURNAL";
        public const string Timezone = "VTIMEZONE";
        public const string Daylight = "DAYLIGHT";
        public const string Standard = "STANDARD";
    }

    public static class EventParticipationStatus
    {
        public const string ParticipationStatus = "PARTSTAT";

        /// <summary> Event needs action </summary>
        public const string NeedsAction = "NEEDS-ACTION";
        /// <summary> Event accepted </summary>
        public const string Accepted = "ACCEPTED";
        /// <summary> Event declined </summary>
        public const string Declined = "DECLINED";
        /// <summary> Event tentatively accepted </summary>
        public const string Tentative = "TENTATIVE";
        /// <summary> Event delegated </summary>
        public const string Delegated = "DELEGATED";

        public static string ParamName => ParticipationStatus;
        public static string Default => NeedsAction;
    }

    public static class ToDoParticipationStatus
    {
        public const string ParticipationStatus = "PARTSTAT";

        /// <summary> To-do needs action </summary>
        public const string NeedsAction = "NEEDS-ACTION";
        /// <summary> To-do accepted </summary>
        public const string Accepted = "ACCEPTED";
        /// <summary> To-do declined </summary>
        public const string Declined = "DECLINED";
        /// <summary> To-do tentatively accepted </summary>
        public const string Tentative = "TENTATIVE";
        /// <summary> To-do delegated </summary>
        public const string Delegated = "DELEGATED";
        /// <summary> To-do completed </summary>
        public const string Completed = "COMPLETED";
        /// <summary> To-do in process </summary>
        public const string InProcess = "IN-PROCESS";

        public static string ParamName => ParticipationStatus;
        public static string Default => NeedsAction;
    }

    public static class JournalParticipationStatus
    {
        public const string ParticipationStatus = "PARTSTAT";

        /// <summary> Event needs action </summary>
        public const string NeedsAction = "NEEDS-ACTION";
        /// <summary> Event accepted </summary>
        public const string Accepted = "ACCEPTED";
        /// <summary> Event declined </summary>
        public const string Declined = "DECLINED";

        public static string ParamName => ParticipationStatus;
        public static string Default => NeedsAction;
    }

    public static class ParticipationRole
    {
        public const string Role = "ROLE";

        /// <summary> Indicates the chair of the calendar entity </summary>
        public const string Chair = "CHAIR";

        /// <summary> Indicates a participant whose participation is required </summary>
        public const string RequiredParticipant = "REQ-PARTICIPANT";

        /// <summary> Indicates a participant whose participation is optional </summary>
        public const string OptionalParticipant = "OPT-PARTICIPANT";

        /// <summary> Indicates a participant who is copied for information purposes only </summary>
        public const string NonParticipant = "NON-PARTICIPANT";

        public static string Default => RequiredParticipant;
        public static string ParamName => Role;
    }

    public class SerializationConstants
    {
        public const string LineBreak = "\r\n";
        public const string NonStandardPropertyPrefix = "X-";
    }

    /// <summary>
    /// Status codes available to an <see cref="Components.Event"/> item
    /// </summary>
    public enum EventStatus
    {
        Tentative,
        Confirmed,
        Cancelled
    }

    /// <summary>
    /// Status codes available to a <see cref="Components.Todo"/> item.
    /// </summary>
    public enum TodoStatus
    {
        NeedsAction,
        Completed,
        InProcess,
        Cancelled
    }

    /// <summary>
    /// Status codes available to a <see cref="Components.Journal"/> entry.
    /// </summary>    
    public enum JournalStatus
    {
        Draft, // Indicates journal is draft.
        Final, // Indicates journal is final.
        Cancelled // Indicates journal is removed.
    }

    public enum FreeBusyStatus
    {
        Free = 0,
        BusyTentative = 1,
        BusyUnavailable = 2,
        Busy = 3
    }

    public enum FrequencyType
    {
        None,
        Secondly,
        Minutely,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    /// <summary>
    /// Indicates the occurrence of the specific day within a
    /// MONTHLY or YEARLY recurrence frequency. For example, within
    /// a MONTHLY frequency, consider the following:
    /// 
    /// RecurrencePattern r = new RecurrencePattern();
    /// r.Frequency = FrequencyType.Monthly;
    /// r.ByDay.Add(new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.First));
    /// 
    /// The above example represents the first Monday within the month,
    /// whereas if FrequencyOccurrence.Last were specified, it would 
    /// represent the last Monday of the month.
    /// 
    /// For a YEARLY frequency, consider the following:
    /// 
    /// Recur r = new Recur();
    /// r.Frequency = FrequencyType.Yearly;
    /// r.ByDay.Add(new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.Second));
    /// 
    /// The above example represents the second Monday of the year.  This can
    /// also be represented with the following code:
    /// 
    /// r.ByDay.Add(new WeekDay(DayOfWeek.Monday, 2));
    /// </summary>
    public enum FrequencyOccurrence
    {
        None = int.MinValue,
        Last = -1,
        SecondToLast = -2,
        ThirdToLast = -3,
        FourthToLast = -4,
        FifthToLast = -5,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5
    }

    public enum RecurrenceRestrictionType
    {
        /// <summary>
        /// Same as RestrictSecondly.
        /// </summary>
        Default,

        /// <summary>
        /// Does not restrict recurrence evaluation - WARNING: this may cause very slow performance!
        /// </summary>
        NoRestriction,

        /// <summary>
        /// Disallows use of the SECONDLY frequency for recurrence evaluation
        /// </summary>
        RestrictSecondly,

        /// <summary>
        /// Disallows use of the MINUTELY and SECONDLY frequencies for recurrence evaluation
        /// </summary>
        RestrictMinutely,

        /// <summary>
        /// Disallows use of the HOURLY, MINUTELY, and SECONDLY frequencies for recurrence evaluation
        /// </summary>
        RestrictHourly
    }

    public enum RecurrenceEvaluationModeType
    {
        /// <summary>
        /// Same as ThrowException.
        /// </summary>
        Default,

        /// <summary>
        /// Automatically adjusts the evaluation to the next-best frequency based on the restriction type.
        /// For example, if the restriction were IgnoreSeconds, and the frequency were SECONDLY, then
        /// this would cause the frequency to be adjusted to MINUTELY, the next closest thing.
        /// </summary>
        AdjustAutomatically,

        /// <summary>
        /// This will throw an exception if a recurrence rule is evaluated that does not meet the minimum
        /// restrictions.  For example, if the restriction were IgnoreSeconds, and a SECONDLY frequency
        /// were evaluated, an exception would be thrown.
        /// </summary>
        ThrowException
    }

    public enum TransparencyType
    {
        Opaque,
        Transparent
    }

    public class CalendarProductIDs
    {
        public const string Default = "-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN";
    }

    public class CalendarVersions
    {
        public const string Latest = "2.0";
    }

    public class CalendarScales
    {
        public const string Gregorian = "GREGORIAN";
    }

    public class CalendarMethods
    {
        /// <summary>
        /// Used to publish an iCalendar object to one or
        /// more "Calendar Users".  There is no interactivity
        /// between the publisher and any other "Calendar User".
        /// An example might include a baseball team publishing
        /// its schedule to the public.
        /// </summary>
        public const string Publish = "PUBLISH";

        /// <summary>
        /// Used to schedule an iCalendar object with other
        /// "Calendar Users".  Requests are interactive in
        /// that they require the receiver to respond using
        /// the reply methods.  Meeting requests, busy-time
        /// requests, and the assignment of tasks to other
        /// "Calendar Users" are all examples.  Requests are
        /// also used by the Organizer to update the status
        /// of an iCalendar object. 
        /// </summary>
        public const string Request = "REQUEST";

        /// <summary>
        /// A reply is used in response to a request to
        /// convey Attendee status to the Organizer.
        /// Replies are commonly used to respond to meeting
        /// and task requests.     
        /// </summary>
        public const string Reply = "REPLY";

        /// <summary>
        /// Add one or more new instances to an existing
        /// recurring iCalendar object. 
        /// </summary>
        public const string Add = "ADD";

        /// <summary>
        /// Cancel one or more instances of an existing
        /// iCalendar object.
        /// </summary>
        public const string Cancel = "CANCEL";

        /// <summary>
        /// Used by an Attendee to request the latest
        /// version of an iCalendar object.
        /// </summary>
        public const string Refresh = "REFRESH";

        /// <summary>
        /// Used by an Attendee to negotiate a change in an
        /// iCalendar object.  Examples include the request
        /// to change a proposed event time or change the
        /// due date for a task.
        /// </summary>
        public const string Counter = "COUNTER";

        /// <summary>
        /// Used by the Organizer to decline the proposed
        /// counter-proposal.
        /// </summary>
        public const string DeclineCounter = "DECLINECOUNTER";
    }
}