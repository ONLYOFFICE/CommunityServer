/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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