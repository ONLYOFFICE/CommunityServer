using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net
{
    /// <summary>
    /// Represents a unique component, a component with a unique UID,
    /// which can be used to uniquely identify the component.    
    /// </summary>
    public class UniqueComponent : CalendarComponent, IUniqueComponent, IComparable<UniqueComponent>
    {
        // TODO: Add AddRelationship() public method.
        // This method will add the UID of a related component
        // to the Related_To property, along with any "RELTYPE"
        // parameter ("PARENT", "CHILD", "SIBLING", or other)
        // TODO: Add RemoveRelationship() public method.        

        public UniqueComponent()
        {
            EnsureProperties();
        }

        public UniqueComponent(string name) : base(name)
        {
            EnsureProperties();
        }

        private void EnsureProperties()
        {
            if (string.IsNullOrEmpty(Uid))
            {
                // Create a new UID for the component
                Uid = Guid.NewGuid().ToString();
            }

            // NOTE: removed setting the 'CREATED' property here since it breaks serialization.
            // See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3754354
            if (DtStamp == null)
            {
                // Here, we don't simply set to DateTime.Now because DateTime.Now contains milliseconds, and
                // the iCalendar standard doesn't care at all about milliseconds.  Therefore, when comparing
                // two calendars, one generated, and one loaded from file, they may be functionally identical,
                // but be determined to be different due to millisecond differences.
                var now = DateTime.SpecifyKind(DateTime.Today.Add(DateTime.UtcNow.TimeOfDay), DateTimeKind.Utc);
                DtStamp = new CalDateTime(now);
            }
        }

        public virtual IList<IAttendee> Attendees
        {
            get { return Properties.GetMany<IAttendee>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

        public virtual IList<string> Comments
        {
            get { return Properties.GetMany<string>("COMMENT"); }
            set { Properties.Set("COMMENT", value); }
        }

        public virtual IDateTime DtStamp
        {
            get { return Properties.Get<IDateTime>("DTSTAMP"); }
            set { Properties.Set("DTSTAMP", value); }
        }

        public virtual IOrganizer Organizer
        {
            get { return Properties.Get<IOrganizer>("ORGANIZER"); }
            set { Properties.Set("ORGANIZER", value); }
        }

        public virtual IList<IRequestStatus> RequestStatuses
        {
            get { return Properties.GetMany<IRequestStatus>("REQUEST-STATUS"); }
            set { Properties.Set("REQUEST-STATUS", value); }
        }

        public virtual Uri Url
        {
            get { return Properties.Get<Uri>("URL"); }
            set { Properties.Set("URL", value); }
        }

        protected override void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);

            EnsureProperties();
        }

        public int CompareTo(UniqueComponent other) => string.Compare(Uid, other.Uid, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
        {
            if (obj is RecurringComponent && obj != this)
            {
                var r = (RecurringComponent) obj;
                if (Uid != null)
                {
                    return Uid.Equals(r.Uid);
                }
                return Uid == r.Uid;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() => Uid?.GetHashCode() ?? base.GetHashCode();

        public virtual string Uid
        {
            get { return Properties.Get<string>("UID"); }
            set { Properties.Set("UID", value); }
        }
    }
}