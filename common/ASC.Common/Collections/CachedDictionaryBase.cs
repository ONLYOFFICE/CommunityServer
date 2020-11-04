/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Diagnostics;
using System.Linq;

namespace ASC.Collections
{
    public abstract class CachedDictionaryBase<T>
    {
        protected string baseKey;
        protected Func<T, bool> condition;

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
            InsertRootKey(baseKey);
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
            return string.Format("{0}-{1}-{2}", baseKey, rootkey, key);
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
                OnHit(fullKey);
                return ReturnCached(objectCache);
            }
            if (defaults != null)
            {
                OnMiss(fullKey);
                T newValue = defaults();
                if (condition == null || condition(newValue))
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

        [Conditional("DEBUG")]
        protected virtual void OnHit(string fullKey)
        {
            Debug.Print("cache hit:{0}", fullKey);
        }

        [Conditional("DEBUG")]
        protected virtual void OnMiss(string fullKey)
        {
            Debug.Print("cache miss:{0}", fullKey);
        }
    }
}