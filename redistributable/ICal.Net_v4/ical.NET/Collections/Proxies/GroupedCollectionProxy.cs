using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Collections.Proxies
{
    /// <summary>
    /// A proxy for a keyed list.
    /// </summary>
    public class GroupedCollectionProxy<TGroup, TOriginal, TNew> :
        IGroupedCollection<TGroup, TNew>
        where TOriginal : class, IGroupedObject<TGroup>
        where TNew : class, TOriginal
    {
        private readonly Func<TNew, bool> _predicate;

        public GroupedCollectionProxy(IGroupedCollection<TGroup, TOriginal> realObject, Func<TNew, bool> predicate = null)
        {
            _predicate = predicate ?? (o => true);
            SetProxiedObject(realObject);
        }

        public virtual event EventHandler<ObjectEventArgs<TNew, int>> ItemAdded;
        public virtual event EventHandler<ObjectEventArgs<TNew, int>> ItemRemoved;

        protected void OnItemAdded(TNew item, int index)
        {
            ItemAdded?.Invoke(this, new ObjectEventArgs<TNew, int>(item, index));
        }

        protected void OnItemRemoved(TNew item, int index)
        {
            ItemRemoved?.Invoke(this, new ObjectEventArgs<TNew, int>(item, index));
        }

        public virtual bool Remove(TGroup group) => RealObject.Remove(group);

        public virtual void Clear(TGroup group)
        {
            RealObject.Clear(group);
        }

        public virtual bool ContainsKey(TGroup group) => RealObject.ContainsKey(group);

        public virtual int CountOf(TGroup group) => RealObject.OfType<TGroup>().Count();

        public virtual IEnumerable<TNew> AllOf(TGroup group) => RealObject
            .AllOf(group)
            .OfType<TNew>()
            .Where(_predicate);

        public virtual void Add(TNew item)
        {
            RealObject.Add(item);
        }

        public virtual void Clear()
        {
            // Only clear items of this type
            // that match the predicate.

            var items = RealObject
                .OfType<TNew>()
                .ToArray();

            foreach (var item in items)
            {
                RealObject.Remove(item);
            }
        }

        public virtual bool Contains(TNew item) => RealObject.Contains(item);

        public virtual void CopyTo(TNew[] array, int arrayIndex)
        {
            var i = 0;
            foreach (var item in this)
            {
                array[arrayIndex + (i++)] = item;
            }
        }

        public virtual int Count => RealObject
            .OfType<TNew>()
            .Count();

        public virtual bool IsReadOnly => false;

        public virtual bool Remove(TNew item) => RealObject.Remove(item);

        public virtual IEnumerator<TNew> GetEnumerator() => RealObject
            .OfType<TNew>()
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => RealObject
            .OfType<TNew>()
            .GetEnumerator();

        public IGroupedCollection<TGroup, TOriginal> RealObject { get; private set; }

        public virtual void SetProxiedObject(IGroupedCollection<TGroup, TOriginal> realObject)
        {
            RealObject = realObject;
        }
    }
}
