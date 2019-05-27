using System.Diagnostics;
using System.Runtime.Serialization;

namespace Ical.Net.CalendarComponents
{
    /// <summary>
    /// This class is used by the parsing framework for iCalendar components.
    /// Generally, you should not need to use this class directly.
    /// </summary>
    [DebuggerDisplay("Component: {Name}")]
    public class CalendarComponent : CalendarObject, ICalendarComponent
    {
        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
        public virtual CalendarPropertyList Properties { get; protected set; }

        public CalendarComponent() : base()
        {
            Initialize();
        }

        public CalendarComponent(string name) : base(name)
        {
            Initialize();
        }

        private void Initialize()
        {
            Properties = new CalendarPropertyList(this);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var c = obj as ICalendarComponent;
            if (c == null)
            {
                return;
            }

            Properties.Clear();
            foreach (var p in c.Properties)
            {
                Properties.Add(p);
            }
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        public virtual void AddProperty(string name, string value)
        {
            var p = new CalendarProperty(name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        public virtual void AddProperty(ICalendarProperty p)
        {
            p.Parent = this;
            Properties.Set(p.Name, p.Value);
        }
    }
}