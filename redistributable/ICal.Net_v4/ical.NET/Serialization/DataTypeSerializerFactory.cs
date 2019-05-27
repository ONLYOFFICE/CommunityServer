using System;
using System.Reflection;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Serialization
{
    public class DataTypeSerializerFactory : ISerializerFactory
    {
        /// <summary>
        /// Returns a serializer that can be used to serialize and object
        /// of type <paramref name="objectType"/>.
        /// <note>
        ///     TODO: Add support for caching.
        /// </note>
        /// </summary>
        /// <param name="objectType">The type of object to be serialized.</param>
        /// <param name="ctx">The serialization context.</param>
        public virtual ISerializer Build(Type objectType, SerializationContext ctx)
        {
            if (objectType != null)
            {
                ISerializer s;

                if (typeof (Attachment).IsAssignableFrom(objectType))
                {
                    s = new AttachmentSerializer(ctx);
                }
                else if (typeof (Attendee).IsAssignableFrom(objectType))
                {
                    s = new AttendeeSerializer(ctx);
                }
                else if (typeof (IDateTime).IsAssignableFrom(objectType))
                {
                    s = new DateTimeSerializer(ctx);
                }
                else if (typeof (FreeBusyEntry).IsAssignableFrom(objectType))
                {
                    s = new FreeBusyEntrySerializer(ctx);
                }
                else if (typeof (GeographicLocation).IsAssignableFrom(objectType))
                {
                    s = new GeographicLocationSerializer(ctx);
                }
                else if (typeof (Organizer).IsAssignableFrom(objectType))
                {
                    s = new OrganizerSerializer(ctx);
                }
                else if (typeof (Period).IsAssignableFrom(objectType))
                {
                    s = new PeriodSerializer(ctx);
                }
                else if (typeof (PeriodList).IsAssignableFrom(objectType))
                {
                    s = new PeriodListSerializer(ctx);
                }
                else if (typeof (RecurrencePattern).IsAssignableFrom(objectType))
                {
                    s = new RecurrencePatternSerializer(ctx);
                }
                else if (typeof (RequestStatus).IsAssignableFrom(objectType))
                {
                    s = new RequestStatusSerializer(ctx);
                }
                else if (typeof (StatusCode).IsAssignableFrom(objectType))
                {
                    s = new StatusCodeSerializer(ctx);
                }
                else if (typeof (Trigger).IsAssignableFrom(objectType))
                {
                    s = new TriggerSerializer(ctx);
                }
                else if (typeof (UtcOffset).IsAssignableFrom(objectType))
                {
                    s = new UtcOffsetSerializer(ctx);
                }
                else if (typeof (WeekDay).IsAssignableFrom(objectType))
                {
                    s = new WeekDaySerializer(ctx);
                }
                // Default to a string serializer, which simply calls
                // ToString() on the value to serialize it.
                else
                {
                    s = new StringSerializer(ctx);
                }

                return s;
            }
            return null;
        }
    }
}