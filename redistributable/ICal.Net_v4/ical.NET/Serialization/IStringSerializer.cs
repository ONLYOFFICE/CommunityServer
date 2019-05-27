using System.IO;

namespace Ical.Net.Serialization
{
    public interface IStringSerializer : ISerializer
    {
        string SerializeToString(object obj);
        object Deserialize(TextReader tr);
    }
}