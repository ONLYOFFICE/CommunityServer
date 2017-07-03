using Ical.Net.Interfaces.General;

namespace Ical.Net.Interfaces.Serialization
{
    public interface ISerializationContext : IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();
    }
}