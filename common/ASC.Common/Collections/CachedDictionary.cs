/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Web.Caching;

namespace ASC.Collections
{
    public sealed class CachedDictionary<T> : CachedDictionaryBase<T>
    {
        private readonly DateTime _absoluteExpiration;
        private readonly TimeSpan _slidingExpiration;

        public CachedDictionary(string baseKey, DateTime absoluteExpiration, TimeSpan slidingExpiration,
                                Func<T, bool> cacheCodition)
        {
            if (cacheCodition == null) throw new ArgumentNullException("cacheCodition");
            _baseKey = baseKey;
            _absoluteExpiration = absoluteExpiration;
            _slidingExpiration = slidingExpiration;
            _cacheCodition = cacheCodition;
            InsertRootKey(_baseKey);
        }

        public CachedDictionary(string baseKey)
            : this(baseKey, (x) => true)
        {
        }

        public CachedDictionary(string baseKey, Func<T, bool> cacheCodition)
            : this(baseKey, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, cacheCodition)
        {
        }

        protected override void InsertRootKey(string rootKey)
        {
#if DEBUG
            Debug.Print("inserted root key {0}", rootKey);
#endif
            MemoryCache.Default[rootKey] = DateTime.UtcNow.Ticks;
        }

        public override void Reset(string rootKey, string key)
        {
            MemoryCache.Default.Remove(BuildKey(key, rootKey));
        }

        protected override object GetObjectFromCache(string fullKey)
        {
            return MemoryCache.Default[fullKey];
        }

        public override void Add(string rootkey, string key, T newValue)
        {
            var builtrootkey = BuildKey(string.Empty, string.IsNullOrEmpty(rootkey)?"root":rootkey);
            if (MemoryCache.Default[builtrootkey] == null)
            {
#if DEBUG
                Debug.Print("added root key {0}",builtrootkey);
#endif
                //Insert root if no present
                MemoryCache.Default[builtrootkey] = DateTime.UtcNow.Ticks;
            }
            if (newValue != null)
            {
                var buildKey = BuildKey(key, rootkey);
                MemoryCache.Default[buildKey] = newValue;
            }
            else
            {
                MemoryCache.Default.Remove(BuildKey(key, rootkey));//Remove if null
            }
        }
    }
}