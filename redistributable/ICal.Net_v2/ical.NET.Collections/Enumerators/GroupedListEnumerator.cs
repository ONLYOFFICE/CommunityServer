using System.Collections;
using System.Collections.Generic;
using Ical.Net.Collections.Interfaces;

namespace Ical.Net.Collections.Enumerators
{
    public class GroupedListEnumerator<TType> :
        IEnumerator<TType>
    {
        private readonly IList<IMultiLinkedList<TType>> _lists;
        private IEnumerator<IMultiLinkedList<TType>> _listsEnumerator;
        private IEnumerator<TType> _listEnumerator;

        public GroupedListEnumerator(IList<IMultiLinkedList<TType>> lists)
        {
            _lists = lists;
        }

        public virtual TType Current
        {
            get
            {
                if (_listEnumerator != null)
                    return _listEnumerator.Current;
                return default(TType);
            }
        }

        public virtual void Dispose()
        {
            Reset();
        }

        private void DisposeListEnumerator()
        {
            if (_listEnumerator != null)
            {
                _listEnumerator.Dispose();
                _listEnumerator = null;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                if (_listEnumerator != null)
                    return _listEnumerator.Current;
                return default(TType);
            }
        }

        private bool MoveNextList()
        {
            if (_listsEnumerator == null)
            {
                _listsEnumerator = _lists.GetEnumerator();
            }

            if (_listsEnumerator != null)
            {
                if (_listsEnumerator.MoveNext())
                {
                    DisposeListEnumerator();
                    if (_listsEnumerator.Current != null)
                    {
                        _listEnumerator = _listsEnumerator.Current.GetEnumerator();
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool MoveNext()
        {
            if (_listEnumerator != null)
            {
                if (_listEnumerator.MoveNext())
                {
                    return true;
                }
                DisposeListEnumerator();
                if (MoveNextList())
                    return MoveNext();
            }
            else
            {
                if (MoveNextList())
                    return MoveNext();
            }
            return false;
        }

        public virtual void Reset()
        {

            if (_listsEnumerator != null)
            {
                _listsEnumerator.Dispose();
                _listsEnumerator = null;
            }
        }
    }
}
