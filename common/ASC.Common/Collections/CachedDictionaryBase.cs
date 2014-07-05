/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Diagnostics;
using System.Linq;

namespace ASC.Collections
{
    public abstract class CachedDictionaryBase<T>
    {
        protected string _baseKey;
        protected Func<T, bool> _cacheCodition;

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