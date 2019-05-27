using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.CalendarComponents
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
                // icalendar RFC doesn't care about sub-second time resolution, so shave off everything smaller than seconds.
                var utcNow = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1));
                DtStamp = new CalDateTime(utcNow, "UTC");
            }
        }

        public virtual IList<Attendee> Attendees
        {
            get => Properties.GetMany<Attendee>("ATTENDEE");
            set => Properties.Set("ATTENDEE", value);
        }

        public virtual IList<string> Comments
        {
            get => Properties.GetMany<string>("COMMENT");
            set => Properties.Set("COMMENT", value);
        }

        public virtual IDateTime DtStamp
        {
            get => Properties.Get<IDateTime>("DTSTAMP");
            set => Properties.Set("DTSTAMP", value);
        }

        public virtual Organizer Organizer
        {
            get => Properties.Get<Organizer>("ORGANIZER");
            set => Properties.Set("ORGANIZER", value);
        }

        public virtual IList<RequestStatus> RequestStatuses
        {
            get => Properties.GetMany<RequestStatus>("REQUEST-STATUS");
            set => Properties.Set("REQUEST-STATUS", value);
        }

        public virtual Uri Url
        {
            get => Properties.Get<Uri>("URL");
            set => Properties.Set("URL", value);
        }

        protected override void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);

            EnsureProperties();
        }

        public int CompareTo(UniqueComponent other)
            => string.Compare(Uid, other.Uid, StringComparison.OrdinalIgnoreCase);

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
            get => Properties.Get<string>("UID");
            set => Properties.Set("UID", value);
        }
    }
}