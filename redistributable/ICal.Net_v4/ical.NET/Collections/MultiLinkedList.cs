using System.Collections.Generic;

namespace Ical.Net.Collections
{
    public class MultiLinkedList<TType> :
        List<TType>,
        IMultiLinkedList<TType>
    {
        private IMultiLinkedList<TType> _previous;
        private IMultiLinkedList<TType> _next;

        public virtual void SetPrevious(IMultiLinkedList<TType> previous)
        {
            _previous = previous;
        }

        public virtual void SetNext(IMultiLinkedList<TType> next)
        {
            _next = next;
        }

        public virtual int StartIndex => _previous?.ExclusiveEnd ?? 0;

        public virtual int ExclusiveEnd => Count > 0 ? StartIndex + Count : StartIndex;
    }
}
