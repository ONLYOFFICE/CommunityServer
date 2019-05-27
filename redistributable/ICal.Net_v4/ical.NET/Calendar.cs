using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Proxies;
using Ical.Net.Serialization;
using Ical.Net.Utility;

namespace Ical.Net
{
    public class Calendar : CalendarComponent, IGetOccurrencesTyped, IGetFreeBusy, IMergeable
    {
        public static Calendar Load(string iCalendarString)
            => CalendarCollection.Load(new StringReader(iCalendarString)).SingleOrDefault();

        /// <summary>
        /// Loads an <see cref="Calendar"/> from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the <see cref="Calendar"/> object</param>
        /// <returns>An <see cref="Calendar"/> object</returns>
        public static Calendar Load(Stream s)
            => CalendarCollection.Load(new StreamReader(s, Encoding.UTF8)).SingleOrDefault();

        public static Calendar Load(TextReader tr)
            => CalendarCollection.Load(tr).OfType<Calendar>().SingleOrDefault();

        public static IList<T> Load<T>(Stream s, Encoding e)
            => Load<T>(new StreamReader(s, e));

        public static IList<T> Load<T>(TextReader tr)
            => SimpleDeserializer.Default.Deserialize(tr).OfType<T>().ToList();

        public static IList<T> Load<T>(string ical)
            => Load<T>(new StringReader(ical));

        private IUniqueComponentList<IUniqueComponent> _mUniqueComponents;
        private IUniqueComponentList<CalendarEvent> _mEvents;
        private IUniqueComponentList<Todo> _mTodos;
        private ICalendarObjectList<Journal> _mJournals;
        private IUniqueComponentList<FreeBusy> _mFreeBusy;
        private ICalendarObjectList<VTimeZone> _mTimeZones;

        /// <summary>
        /// To load an existing an iCalendar object, use one of the provided LoadFromXXX methods.
        /// <example>
        /// For example, use the following code to load an iCalendar object from a URL:
        /// <code>
        ///     IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        /// </code>
        /// </example>
        /// </summary>
        public Calendar()
        {
            Name = Components.Calendar;
            Initialize();
        }

        private void Initialize()
        {
            _mUniqueComponents = new UniqueComponentListProxy<IUniqueComponent>(Children);
            _mEvents = new UniqueComponentListProxy<CalendarEvent>(Children);
            _mTodos = new UniqueComponentListProxy<Todo>(Children);
            _mJournals = new CalendarObjectListProxy<Journal>(Children);
            _mFreeBusy = new UniqueComponentListProxy<FreeBusy>(Children);
            _mTimeZones = new CalendarObjectListProxy<VTimeZone>(Children);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected bool Equals(Calendar other)
            => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && CollectionHelpers.Equals(UniqueComponents, other.UniqueComponents)
                && CollectionHelpers.Equals(Events, other.Events)
                && CollectionHelpers.Equals(Todos, other.Todos)
                && CollectionHelpers.Equals(Journals, other.Journals)
                && CollectionHelpers.Equals(FreeBusy, other.FreeBusy)
                && CollectionHelpers.Equals(TimeZones, other.TimeZones);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == GetType() && Equals((Calendar)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(UniqueComponents);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Events);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Todos);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Journals);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(FreeBusy);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(TimeZones);
                return hashCode;
            }
        }

        public virtual IUniqueComponentList<IUniqueComponent> UniqueComponents => _mUniqueComponents;

        public virtual IEnumerable<IRecurrable> RecurringItems => Children.OfType<IRecurrable>();

        /// <summary>
        /// A collection of <see cref="Components.Event"/> components in the iCalendar.
        /// </summary>
        public virtual IUniqueComponentList<CalendarEvent> Events => _mEvents;

        /// <summary>
        /// A collection of <see cref="CalendarComponents.FreeBusy"/> components in the iCalendar.
        /// </summary>
        public virtual IUniqueComponentList<FreeBusy> FreeBusy => _mFreeBusy;

        /// <summary>
        /// A collection of <see cref="Journal"/> components in the iCalendar.
        /// </summary>
        public virtual ICalendarObjectList<Journal> Journals => _mJournals;

        /// <summary>
        /// A collection of VTimeZone components in the iCalendar.
        /// </summary>
        public virtual ICalendarObjectList<VTimeZone> TimeZones => _mTimeZones;

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        public virtual IUniqueComponentList<Todo> Todos => _mTodos;

        public virtual string Version
        {
            get => Properties.Get<string>("VERSION");
            set => Properties.Set("VERSION", value);
        }

        public virtual string ProductId
        {
            get => Properties.Get<string>("PRODID");
            set => Properties.Set("PRODID", value);
        }

        public virtual string Scale
        {
            get => Properties.Get<string>("CALSCALE");
            set => Properties.Set("CALSCALE", value);
        }

        public virtual string Method
        {
            get => Properties.Get<string>("METHOD");
            set => Properties.Set("METHOD", value);
        }

        public virtual RecurrenceRestrictionType RecurrenceRestriction
        {
            get => Properties.Get<RecurrenceRestrictionType>("X-DDAY-ICAL-RECURRENCE-RESTRICTION");
            set => Properties.Set("X-DDAY-ICAL-RECURRENCE-RESTRICTION", value);
        }

        public virtual RecurrenceEvaluationModeType RecurrenceEvaluationMode
        {
            get => Properties.Get<RecurrenceEvaluationModeType>("X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE");
            set => Properties.Set("X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE", value);
        }

        /// <summary>
        /// Adds a time zone to the iCalendar.  This time zone may
        /// then be used in date/time objects contained in the 
        /// calendar.
        /// </summary>        
        /// <returns>The time zone added to the calendar.</returns>
        public VTimeZone AddTimeZone(VTimeZone tz)
        {
            this.AddChild(tz);
            return tz;
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time.
        /// <example>
        ///     For example, if you are displaying a month-view for January 2007,
        ///     you would want to evaluate recurrences for Jan. 1, 2007 to Jan. 31, 2007
        ///     to display relevant information for those dates.
        /// </example>
        /// </summary>
        /// <param name="fromDate">The beginning date/time of the range to test.</param>
        /// <param name="toDate">The end date/time of the range to test.</param>
        [Obsolete("This method is no longer supported.  Use GetOccurrences() instead.")]
        public void Evaluate(IDateTime fromDate, IDateTime toDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time, for
        /// the type of recurring component specified.
        /// </summary>
        /// <typeparam name="T">The type of component to be evaluated for recurrences.</typeparam>
        /// <param name="fromDate">The beginning date/time of the range to test.</param>
        /// <param name="toDate">The end date/time of the range to test.</param>
        [Obsolete("This method is no longer supported.  Use GetOccurrences() instead.")]
        public void Evaluate<T>(IDateTime fromDate, IDateTime toDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");
        }

        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        public void ClearEvaluation()
        {
            foreach (var recurrable in RecurringItems)
            {
                recurrable.ClearEvaluation();
            }
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        public virtual HashSet<Occurrence> GetOccurrences(IDateTime dt)
            => GetOccurrences<IRecurringComponent>(new CalDateTime(dt.AsSystemLocal.Date), new CalDateTime(dt.AsSystemLocal.Date.AddDays(1).AddSeconds(-1)));

        public virtual HashSet<Occurrence> GetOccurrences(DateTime dt)
            => GetOccurrences<IRecurringComponent>(new CalDateTime(dt.Date), new CalDateTime(dt.Date.AddDays(1).AddSeconds(-1)));

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// that occur between <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="startTime">The beginning date/time of the range.</param>
        /// <param name="endTime">The end date/time of the range.</param>
        /// <returns>A list of occurrences that fall between the dates provided.</returns>
        public virtual HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
            => GetOccurrences<IRecurringComponent>(startTime, endTime);

        public virtual HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
            => GetOccurrences<IRecurringComponent>(new CalDateTime(startTime), new CalDateTime(endTime));

        /// <summary>
        /// Returns all occurrences of components of type T that start on the date provided.
        /// All components starting between 12:00:00AM and 11:59:59 PM will be
        /// returned.
        /// <note>
        /// This will first Evaluate() the date range required in order to
        /// determine the occurrences for the date provided, and then return
        /// the occurrences.
        /// </note>
        /// </summary>
        /// <param name="dt">The date for which to return occurrences.</param>
        /// <returns>A list of Periods representing the occurrences of this object.</returns>
        public virtual HashSet<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent
            => GetOccurrences<T>(new CalDateTime(dt.AsSystemLocal.Date), new CalDateTime(dt.AsSystemLocal.Date.AddDays(1).AddTicks(-1)));

        public virtual HashSet<Occurrence> GetOccurrences<T>(DateTime dt) where T : IRecurringComponent
            => GetOccurrences<T>(new CalDateTime(dt.Date), new CalDateTime(dt.Date.AddDays(1).AddTicks(-1)));

        /// <summary>
        /// Returns all occurrences of components of type T that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        public virtual HashSet<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>(RecurringItems
                .OfType<T>()
                .SelectMany(recurrable => recurrable.GetOccurrences(startTime, endTime)));

            var removeOccurrencesQuery = occurrences
                .Where(o => o.Source is UniqueComponent)
                .GroupBy(o => ((UniqueComponent)o.Source).Uid)
                .SelectMany(group => group
                    .Where(o => o.Source.RecurrenceId != null)
                    .SelectMany(occurrence => group.
                        Where(o => o.Source.RecurrenceId == null && occurrence.Source.RecurrenceId.Date.Equals(o.Period.StartTime.Date))));

            occurrences.ExceptWith(removeOccurrencesQuery);
            return occurrences;
        }

        public virtual HashSet<Occurrence> GetOccurrences<T>(DateTime startTime, DateTime endTime) where T : IRecurringComponent
            => GetOccurrences<T>(new CalDateTime(startTime), new CalDateTime(endTime));

        /// <summary>
        /// Creates a typed object that is a direct child of the iCalendar itself.  Generally,
        /// you would invoke this method to create an Event, Todo, Journal, VTimeZone, FreeBusy,
        /// or other base component type.
        /// </summary>
        /// <example>
        /// To create an event, use the following:
        /// <code>
        /// IICalendar iCal = new iCalendar();
        /// 
        /// Event evt = iCal.Create&lt;Event&gt;();
        /// </code>
        /// 
        /// This creates the event, and adds it to the Events list of the iCalendar.
        /// </example>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <returns>An object of the type specified</returns>
        public T Create<T>() where T : ICalendarComponent
        {
            var obj = Activator.CreateInstance(typeof (T)) as ICalendarObject;
            if (obj is T)
            {
                this.AddChild(obj);
                return (T) obj;
            }
            return default(T);
        }

        public void Dispose()
        {
            Children.Clear();
        }

        public virtual void MergeWith(IMergeable obj)
        {
            var c = obj as Calendar;
            if (c == null)
            {
                return;
            }

            if (Name == null)
            {
                Name = c.Name;
            }

            Method = c.Method;
            Version = c.Version;
            ProductId = c.ProductId;
            Scale = c.Scale;

            foreach (var p in c.Properties.Where(p => !Properties.ContainsKey(p.Name)))
            {
                Properties.Add(p);
            }

            foreach (var child in c.Children)
            {
                if (child is IUniqueComponent)
                {
                    if (!UniqueComponents.ContainsKey(((IUniqueComponent) child).Uid))
                    {
                        this.AddChild(child);
                    }
                }
                else
                {
                    this.AddChild(child);
                }
            }
        }

        public virtual FreeBusy GetFreeBusy(FreeBusy freeBusyRequest) => CalendarComponents.FreeBusy.Create(this, freeBusyRequest);

        public virtual FreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive)
            => CalendarComponents.FreeBusy.Create(this, CalendarComponents.FreeBusy.CreateRequest(fromInclusive, toExclusive, null, null));

        public virtual FreeBusy GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, IDateTime fromInclusive, IDateTime toExclusive)
            => CalendarComponents.FreeBusy.Create(this, CalendarComponents.FreeBusy.CreateRequest(fromInclusive, toExclusive, organizer, contacts));

        /// <summary>
        /// Adds a system time zone to the iCalendar.  This time zone may
        /// then be used in date/time objects contained in the 
        /// calendar.
        /// </summary>
        /// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
        /// <returns>The time zone added to the calendar.</returns>
        public VTimeZone AddTimeZone(TimeZoneInfo tzi)
        {
            var tz = VTimeZone.FromSystemTimeZone(tzi);
            this.AddChild(tz);
            return tz;
        }

        public VTimeZone AddTimeZone(TimeZoneInfo tzi, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        {
            var tz = VTimeZone.FromSystemTimeZone(tzi, earliestDateTimeToSupport, includeHistoricalData);
            this.AddChild(tz);
            return tz;
        }

        public VTimeZone AddTimeZone(string tzId)
        {
            var tz = VTimeZone.FromDateTimeZone(tzId);
            this.AddChild(tz);
            return tz;
        }

        public VTimeZone AddTimeZone(string tzId, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        {
            var tz = VTimeZone.FromDateTimeZone(tzId, earliestDateTimeToSupport, includeHistoricalData);
            this.AddChild(tz);
            return tz;
        }

        public VTimeZone AddLocalTimeZone(DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        {
            var tz = VTimeZone.FromLocalTimeZone(earliestDateTimeToSupport, includeHistoricalData);
            this.AddChild(tz);
            return tz;
        }
    }
}