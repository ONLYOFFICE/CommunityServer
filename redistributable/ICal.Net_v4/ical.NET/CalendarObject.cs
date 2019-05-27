using System;
using System.Runtime.Serialization;
using Ical.Net.Collections;

namespace Ical.Net
{
    /// <summary>
    /// The base class for all iCalendar objects and components.
    /// </summary>
    public class CalendarObject : CalendarObjectBase, ICalendarObject
    {
        private ICalendarObjectList<ICalendarObject> _children;
        private ServiceProvider _serviceProvider;

        internal CalendarObject()
        {
            Initialize();
        }

        public CalendarObject(string name) : this()
        {
            Name = name;
        }

        public CalendarObject(int line, int col) : this()
        {
            Line = line;
            Column = col;
        }

        private void Initialize()
        {
            //ToDo: I'm fairly certain this is ONLY used for null checking. If so, maybe it can just be a bool? CalendarObjectList is an empty object, and
            //ToDo: its constructor parameter is ignored
            _children = new CalendarObjectList(this);
            _serviceProvider = new ServiceProvider();

            _children.ItemAdded += Children_ItemAdded;
        }

        [OnDeserializing]
        internal void DeserializingInternal(StreamingContext context) => OnDeserializing(context);

        [OnDeserialized]
        internal void DeserializedInternal(StreamingContext context) => OnDeserialized(context);

        protected virtual void OnDeserializing(StreamingContext context) => Initialize();

        protected virtual void OnDeserialized(StreamingContext context) {}

        private void Children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject, int> e) => e.First.Parent = this;

        protected bool Equals(CalendarObject other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CalendarObject) obj);
        }

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public override void CopyFrom(ICopyable c)
        {
            var obj = c as ICalendarObject;
            if (obj == null)
            {
                return;
            }

            // Copy the name and basic information
            Name = obj.Name;
            Parent = obj.Parent;
            Line = obj.Line;
            Column = obj.Column;

            // Add each child
            Children.Clear();
            foreach (var child in obj.Children)
            {
                this.AddChild(child);
            }
        }

        /// <summary>
        /// Returns the parent iCalObject that owns this one.
        /// </summary>
        public virtual ICalendarObject Parent { get; set; }

        /// <summary>
        /// A collection of iCalObjects that are children of the current object.
        /// </summary>
        public virtual ICalendarObjectList<ICalendarObject> Children => _children;

        /// <summary>
        /// Gets or sets the name of the iCalObject.  For iCalendar components, this is the RFC 5545 name of the component.
        /// </summary>        
        public virtual string Name { get; set; }

        /// <summary>
        /// Returns the <see cref="Calendar"/> that this DDayiCalObject belongs to.
        /// </summary>
        public virtual Calendar Calendar
        {
            get
            {
                ICalendarObject obj = this;
                while (!(obj is Calendar) && obj.Parent != null)
                {
                    obj = obj.Parent;
                }

                return obj as Calendar;
            }
            protected set { }
        }

        public virtual int Line { get; set; }

        public virtual int Column { get; set; }

        public virtual object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

        public virtual object GetService(string name) => _serviceProvider.GetService(name);

        public virtual T GetService<T>() => _serviceProvider.GetService<T>();

        public virtual T GetService<T>(string name) => _serviceProvider.GetService<T>(name);

        public virtual void SetService(string name, object obj) => _serviceProvider.SetService(name, obj);

        public virtual void SetService(object obj) => _serviceProvider.SetService(obj);

        public virtual void RemoveService(Type type) => _serviceProvider.RemoveService(type);

        public virtual void RemoveService(string name) => _serviceProvider.RemoveService(name);

        public virtual string Group
        {
            get => Name;
            set => Name = value;
        }
    }
}