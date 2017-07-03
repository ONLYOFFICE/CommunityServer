using System;
using System.IO;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that is used to specify exactly when an <see cref="Components.Alarm"/> component will trigger.
    /// Usually this date/time is relative to the component to which the Alarm is associated.
    /// </summary>    
    public class Trigger : EncodableDataType, ITrigger
    {
        private IDateTime _mDateTime;
        private TimeSpan? _mDuration;
        private TriggerRelation _mRelated = TriggerRelation.Start;

        public virtual IDateTime DateTime
        {
            get { return _mDateTime; }
            set
            {
                _mDateTime = value;
                if (_mDateTime != null)
                {
                    // NOTE: this, along with the "Duration" setter, fixes the bug tested in
                    // TODO11(), as well as this thread: https://sourceforge.net/forum/forum.php?thread_id=1926742&forum_id=656447

                    // DateTime and Duration are mutually exclusive
                    Duration = null;

                    // Do not allow timeless date/time values
                    _mDateTime.HasTime = true;
                }
            }
        }

        public virtual TimeSpan? Duration
        {
            get { return _mDuration; }
            set
            {
                _mDuration = value;
                if (_mDuration != null)
                {
                    // NOTE: see above.

                    // DateTime and Duration are mutually exclusive
                    DateTime = null;
                }
            }
        }

        public virtual TriggerRelation Related
        {
            get { return _mRelated; }
            set { _mRelated = value; }
        }

        public virtual bool IsRelative => _mDuration != null;

        public Trigger() {}

        public Trigger(TimeSpan ts)
        {
            Duration = ts;
        }

        public Trigger(string value) : this()
        {
            var serializer = new TriggerSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is ITrigger)
            {
                var t = (ITrigger) obj;
                DateTime = t.DateTime;
                Duration = t.Duration;
                Related = t.Related;
            }
        }

        protected bool Equals(Trigger other)
        {
            return Equals(_mDateTime, other._mDateTime) && _mDuration.Equals(other._mDuration) && _mRelated == other._mRelated;
        }

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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Trigger) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _mDateTime?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ _mDuration.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _mRelated;
                return hashCode;
            }
        }
    }
}