using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Collections.Enumerators;
using Ical.Net.Collections.Interfaces;

namespace Ical.Net.Collections
{
    /// <summary>
    /// A list of objects that are keyed.
    /// </summary>
    public class GroupedList<TGroup, TItem> :
        IGroupedList<TGroup, TItem>
        where TItem : class, IGroupedObject<TGroup>
    {
        private readonly List<IMultiLinkedList<TItem>> _lists = new List<IMultiLinkedList<TItem>>();
        private readonly Dictionary<TGroup, IMultiLinkedList<TItem>> _dictionary = new Dictionary<TGroup, IMultiLinkedList<TItem>>();

        private IMultiLinkedList<TItem> EnsureList(TGroup group)
        {
            if (group == null)
            {
                return null;
            }

            if (_dictionary.ContainsKey(group))
            {
                return _dictionary[group];
            }

            var list = new MultiLinkedList<TItem>();
            _dictionary[group] = list;

            _lists.Add(list);
            return list;
        }

        private IMultiLinkedList<TItem> ListForIndex(int index, out int relativeIndex)
        {
            foreach (var list in _lists.Where(list => list.StartIndex <= index && list.ExclusiveEnd > index))
            {
                relativeIndex = index - list.StartIndex;
                return list;
            }
            relativeIndex = -1;
            return null;
        }

        public event EventHandler<ObjectEventArgs<TItem, int>> ItemAdded;

        protected void OnItemAdded(TItem obj, int index)
        {
            ItemAdded?.Invoke(this, new ObjectEventArgs<TItem, int>(obj, index));
        }

        public virtual void Add(TItem item)
        {
            if (item == null)
            {
                return;
            }

            // Add a new list if necessary
            var group = item.Group;
            var list = EnsureList(group);
            var index = list.Count;
            list.Add(item);
            OnItemAdded(item, list.StartIndex + index);
        }

        public virtual int IndexOf(TItem item)
        {
            var group = item.Group;
            if (!_dictionary.ContainsKey(group))
            {
                return -1;
            }

            // Get the list associated with this object's group
            var list = _dictionary[group];

            // Find the object within the list.
            var index = list.IndexOf(item);

            // Return the index within the overall KeyedList
            if (index >= 0)
                return list.StartIndex + index;
            return -1;
        }

        public virtual void Clear(TGroup group)
        {
            if (!_dictionary.ContainsKey(group))
            {
                return;
            }

            // Clear the list (note that this also clears the list in the _Lists object).
            _dictionary[group].Clear();
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
            _lists.Clear();
        }

        public virtual bool ContainsKey(TGroup group) => _dictionary.ContainsKey(@group);

        public virtual int Count => _lists.Sum(list => list.Count);

        public virtual int CountOf(TGroup group)
        {
            return _dictionary.ContainsKey(group)
                ? _dictionary[group].Count
                : 0;
        }

        public virtual IEnumerable<TItem> Values() => _dictionary.Values.SelectMany(i => i);

        public virtual IEnumerable<TItem> AllOf(TGroup group)
        {
            return _dictionary.ContainsKey(@group)
                ? (IEnumerable<TItem>) _dictionary[@group]
                : new TItem[0];
        }

        public virtual bool Remove(TItem obj)
        {
            var group = obj.Group;
            if (!_dictionary.ContainsKey(group))
            {
                return false;
            }

            var items = _dictionary[group];
            var index = items.IndexOf(obj);

            if (index < 0)
            {
                return false;
            }

            items.RemoveAt(index);
            return true;
        }

        public virtual bool Remove(TGroup group)
        {
            if (!_dictionary.ContainsKey(group))
            {
                return false;
            }

            var list = _dictionary[group];
            for (var i = list.Count - 1; i >= 0; i--)
            {
                list.RemoveAt(i);
            }
            return true;
        }

        public virtual bool Contains(TItem item)
        {
            var group = item.Group;
            return _dictionary.ContainsKey(group) && _dictionary[group].Contains(item);
        }

        public virtual void CopyTo(TItem[] array, int arrayIndex)
        {
            _dictionary.SelectMany(kvp => kvp.Value).ToArray().CopyTo(array, arrayIndex);
        }

        public virtual bool IsReadOnly => false;

        public virtual void Insert(int index, TItem item)
        {
            int relativeIndex;
            var list = ListForIndex(index, out relativeIndex);
            if (list == null)
            {
                return;
            }

            list.Insert(relativeIndex, item);
            OnItemAdded(item, index);
        }

        public virtual void RemoveAt(int index)
        {
            int relativeIndex;
            var list = ListForIndex(index, out relativeIndex);
            if (list == null)
            {
                return;
            }
            var item = list[relativeIndex];
            list.RemoveAt(relativeIndex);
        }

        public virtual TItem this[int index]
        {
            get
            {
                int relativeIndex;
                var list = ListForIndex(index, out relativeIndex);
                return list?[relativeIndex];
            }
            set
            {
                int relativeIndex;
                var list = ListForIndex(index, out relativeIndex);
                if (list == null)
                {
                    return;
                }

                // Remove the item at that index and replace it
                var item = list[relativeIndex];
                list.RemoveAt(relativeIndex);
                list.Insert(relativeIndex, value);
                OnItemAdded(item, index);
            }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return new GroupedListEnumerator<TItem>(_lists);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new GroupedListEnumerator<TItem>(_lists);
        }
    }    
}
