namespace Ical.Net.Interfaces.Serialization
{
    public interface ISerializationProcessor<T>
    {
        void PreSerialization(T obj);
        void PostSerialization(T obj);
        void PreDeserialization(T obj);
        void PostDeserialization(T obj);
    }
}