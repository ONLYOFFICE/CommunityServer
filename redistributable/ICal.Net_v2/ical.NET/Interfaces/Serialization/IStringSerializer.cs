using System.IO;

namespace Ical.Net.Interfaces.Serialization
{
    public interface IStringSerializer : ISerializer
    {
        string SerializeToString(object obj);
        object Deserialize(TextReader tr);
    }
}