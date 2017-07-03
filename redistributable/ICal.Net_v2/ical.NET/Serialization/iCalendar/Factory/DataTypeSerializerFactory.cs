using System;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers.Other;

namespace Ical.Net.Serialization.iCalendar.Factory
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
        public virtual ISerializer Build(Type objectType, ISerializationContext ctx)
        {
            if (objectType != null)
            {
                ISerializer s;

                if (typeof (IAttachment).IsAssignableFrom(objectType))
                {
                    s = new AttachmentSerializer(ctx);
                }
                else if (typeof (IAttendee).IsAssignableFrom(objectType))
                {
                    s = new AttendeeSerializer(ctx);
                }
                else if (typeof (IDateTime).IsAssignableFrom(objectType))
                {
                    s = new DateTimeSerializer(ctx);
                }
                else if (typeof (IFreeBusyEntry).IsAssignableFrom(objectType))
                {
                    s = new FreeBusyEntrySerializer(ctx);
                }
                else if (typeof (IGeographicLocation).IsAssignableFrom(objectType))
                {
                    s = new GeographicLocationSerializer(ctx);
                }
                else if (typeof (IOrganizer).IsAssignableFrom(objectType))
                {
                    s = new OrganizerSerializer(ctx);
                }
                else if (typeof (IPeriod).IsAssignableFrom(objectType))
                {
                    s = new PeriodSerializer(ctx);
                }
                else if (typeof (IPeriodList).IsAssignableFrom(objectType))
                {
                    s = new PeriodListSerializer(ctx);
                }
                else if (typeof (IRecurrencePattern).IsAssignableFrom(objectType))
                {
                    s = new RecurrencePatternSerializer(ctx);
                }
                else if (typeof (IRequestStatus).IsAssignableFrom(objectType))
                {
                    s = new RequestStatusSerializer(ctx);
                }
                else if (typeof (IStatusCode).IsAssignableFrom(objectType))
                {
                    s = new StatusCodeSerializer(ctx);
                }
                else if (typeof (ITrigger).IsAssignableFrom(objectType))
                {
                    s = new TriggerSerializer(ctx);
                }
                else if (typeof (IUtcOffset).IsAssignableFrom(objectType))
                {
                    s = new UtcOffsetSerializer(ctx);
                }
                else if (typeof (IWeekDay).IsAssignableFrom(objectType))
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