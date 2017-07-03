using System;
using System.Reflection;
using Ical.Net.General;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.Serialization.iCalendar.Serializers.Components;
using Ical.Net.Serialization.iCalendar.Serializers.Other;

namespace Ical.Net.Serialization.iCalendar.Factory
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly ISerializerFactory _mDataTypeSerializerFactory;

        public SerializerFactory()
        {
            _mDataTypeSerializerFactory = new DataTypeSerializerFactory();
        }

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

                if (typeof (ICalendar).IsAssignableFrom(objectType))
                {
                    s = new CalendarSerializer(ctx);
                }
                else if (typeof (ICalendarComponent).IsAssignableFrom(objectType))
                {
                    s = typeof (IEvent).IsAssignableFrom(objectType)
                        ? new EventSerializer(ctx)
                        : new ComponentSerializer(ctx);
                }
                else if (typeof (ICalendarProperty).IsAssignableFrom(objectType))
                {
                    s = new PropertySerializer(ctx);
                }
                else if (typeof (CalendarParameter).IsAssignableFrom(objectType))
                {
                    s = new ParameterSerializer(ctx);
                }
                else if (typeof (string).IsAssignableFrom(objectType))
                {
                    s = new StringSerializer(ctx);
                }
#if NET_4
                else if (objectType.IsEnum)
                {
                    s = new EnumSerializer(objectType, ctx);
                }
#else
                else if (objectType.GetTypeInfo().IsEnum)
                {
                    s = new EnumSerializer(objectType, ctx);
                }
#endif
                else if (typeof (TimeSpan).IsAssignableFrom(objectType))
                {
                    s = new TimeSpanSerializer(ctx);
                }
                else if (typeof (int).IsAssignableFrom(objectType))
                {
                    s = new IntegerSerializer(ctx);
                }
                else if (typeof (Uri).IsAssignableFrom(objectType))
                {
                    s = new UriSerializer(ctx);
                }
                else if (typeof (ICalendarDataType).IsAssignableFrom(objectType))
                {
                    s = _mDataTypeSerializerFactory.Build(objectType, ctx);
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