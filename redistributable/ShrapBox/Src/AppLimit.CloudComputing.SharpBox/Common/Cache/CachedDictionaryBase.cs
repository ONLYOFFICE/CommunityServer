using System;
using System.Diagnostics;
using System.Linq;

namespace AppLimit.CloudComputing.SharpBox.Common.Cache
{
    public abstract class CachedDictionaryBase<T>
    {
        protected string _baseKey;
        protected Func<T, bool> _cacheCodition;
        protected long ClearId {get { return DateTime.UtcNow.Ticks; }}

        public T this[string key]
        {
            get { return Get(key); }
        }

        public T this[Func<T> @default]
        {
            get { return Get(@default); }
        }

        protected abstract void InsertRootKey(string rootKey);

        public void Clear()
        {
            InsertRootKey(_baseKey);
        }

        public void Clear(string rootKey)
        {
            InsertRootKey(BuildKey(string.Empty, rootKey));
        }

        public void Reset(string key)
        {
            Reset(string.Empty, key);
        }

        public abstract void Reset(string rootKey, string key);

        public T Get(string key)
        {
            return Get(string.Empty, key, null);
        }

        public T Get(string key, Func<T> defaults)
        {
            return Get(string.Empty, key, defaults);
        }


        public void Add(string key, T newValue)
        {
            Add(string.Empty, key, newValue);
        }

        public abstract void Add(string rootkey, string key, T newValue);

        public bool HasItem(string key)
        {
            return !Equals(Get(key), default(T));
        }

        protected string BuildKey(string key, string rootkey)
        {
            return string.Format("{0}-{1}-{2}", _baseKey, rootkey, key);
        }

        protected abstract object GetObjectFromCache(string fullKey);

        public T Get(Func<T> @default)
        {
            string key = string.Format("func {0} {2}.{1}({3})", @default.Method.ReturnType, @default.Method.Name,
                                       @default.Method.DeclaringType.FullName,
                                       string.Join(",",
                                                   @default.Method.GetGenericArguments().Select(x => x.FullName).ToArray
                                                       ()));
            return Get(key, @default);
        }

        protected virtual bool FitsCondition(object cached)
        {
            return cached != null && cached is T;
        }

        public virtual T Get(string rootkey, string key, Func<T> defaults)
        {
            string fullKey = BuildKey(key, rootkey);
            object objectCache = GetObjectFromCache(fullKey);
            if (FitsCondition(objectCache))
            {
#if (DEBUG)
                OnHit(fullKey);
#endif
                return ReturnCached(objectCache);
            }
            if (defaults != null)
            {
#if (DEBUG)
                OnMiss(fullKey);
#endif
                T newValue = defaults();
                if (_cacheCodition == null || _cacheCodition(newValue))
                {
                    Add(rootkey, key, newValue);
                }
                return newValue;
            }
            return default(T);
        }

        protected virtual T ReturnCached(object objectCache)
        {
            return (T)objectCache;
        }

        protected virtual void OnHit(string fullKey)
        {
            Debug.Print("cache hit:{0}", fullKey);
        }

        protected virtual void OnMiss(string fullKey)
        {
            Debug.Print("cache miss:{0}", fullKey);
        }
    }
}