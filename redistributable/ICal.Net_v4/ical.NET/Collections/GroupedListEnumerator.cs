using System.Collections;
using System.Collections.Generic;

namespace Ical.Net.Collections
{
    public class GroupedListEnumerator<TType> :
        IEnumerator<TType>
    {
        private readonly IList<IMultiLinkedList<TType>> _lists;
        private IEnumerator<IMultiLinkedList<TType>> _listsEnumerator;
        private IEnumerator<TType> _listEnumerator;

        public GroupedListEnumerator(IList<IMultiLinkedList<TType>> lists) => _lists = lists;

        public virtual TType Current
            => _listEnumerator == null
                ? default(TType)
                : _listEnumerator.Current;

        public virtual void Dispose()
        {
            Reset();
        }

        private void DisposeListEnumerator()
        {
            if (_listEnumerator == null)
            {
                return;
            }
            _listEnumerator.Dispose();
            _listEnumerator = null;
        }

        object IEnumerator.Current
            => _listEnumerator == null
                ? default(TType)
                : _listEnumerator.Current;

        private bool MoveNextList()
        {
            if (_listsEnumerator == null)
            {
                _listsEnumerator = _lists.GetEnumerator();
            }

            if (_listsEnumerator == null)
            {
                return false;
            }

            if (!_listsEnumerator.MoveNext())
            {
                return false;
            }

            DisposeListEnumerator();
            if (_listsEnumerator.Current == null)
            {
                return false;
            }

            _listEnumerator = _listsEnumerator.Current.GetEnumerator();
            return true;
        }

        public virtual bool MoveNext()
        {
            while (true)
            {
                if (_listEnumerator == null)
                {
                    if (MoveNextList())
                    {
                        continue;
                    }
                }
                else
                {
                    if (_listEnumerator.MoveNext())
                    {
                        return true;
                    }
                    DisposeListEnumerator();
                    if (MoveNextList())
                    {
                        continue;
                    }
                }
                return false;
            }
        }

        public virtual void Reset()
        {
            if (_listsEnumerator == null)
            {
                return;
            }

            _listsEnumerator.Dispose();
            _listsEnumerator = null;
        }
    }
}
