using System;

namespace Ical.Net.Interfaces.Serialization.Factory
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, ISerializationContext ctx);
    }
}