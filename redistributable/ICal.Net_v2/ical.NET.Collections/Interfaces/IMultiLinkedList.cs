using System.Collections.Generic;

namespace Ical.Net.Collections.Interfaces
{
    public interface IMultiLinkedList<TType> :
        IList<TType>
    {
        int StartIndex { get; }
        int ExclusiveEnd { get; }
    }
}
