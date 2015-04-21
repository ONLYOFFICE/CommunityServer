/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Runtime.Caching;
using System.Web.Caching;

namespace ASC.Collections
{
    public sealed class CachedDictionary<T> : CachedDictionaryBase<T>
    {
        public CachedDictionary(string baseKey, Func<T, bool> cacheCodition)
        {
            if (cacheCodition == null) throw new ArgumentNullException("cacheCodition");
            _baseKey = baseKey;
            _cacheCodition = cacheCodition;
            InsertRootKey(_baseKey);
        }

        public CachedDictionary(string baseKey)
            : this(baseKey, (x) => true)
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