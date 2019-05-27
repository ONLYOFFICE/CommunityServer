using System;

namespace Ical.Net.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, SerializationContext ctx);
    }
}