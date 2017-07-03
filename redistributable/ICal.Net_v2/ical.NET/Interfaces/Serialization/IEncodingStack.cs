using System.Text;

namespace Ical.Net.Interfaces.Serialization
{
    public interface IEncodingStack
    {
        Encoding Current { get; }
        void Push(Encoding encoding);
        Encoding Pop();
    }
}