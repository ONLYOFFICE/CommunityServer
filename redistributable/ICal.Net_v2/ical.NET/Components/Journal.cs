using System.Runtime.Serialization;
using Ical.Net.Interfaces.Components;

namespace Ical.Net
{
    /// <summary>
    /// A class that represents an RFC 5545 VJOURNAL component.
    /// </summary>
    public class Journal : RecurringComponent, IJournal
    {
        public JournalStatus Status
        {
            get { return Properties.Get<JournalStatus>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        }

        /// <summary>
        /// Constructs an Journal object, with an iCalObject
        /// (usually an iCalendar object) as its parent.
        /// </summary>
        public Journal()
        {
            Name = Components.Journal;
        }

        protected override bool EvaluationIncludesReferenceDate => true;
        
        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);
        }

        protected bool Equals(Journal other) => Start.Equals(other.Start) && Equals(other as RecurringComponent);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Journal)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = Start?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ base.GetHashCode();
            return hashCode;
        }
    }
}