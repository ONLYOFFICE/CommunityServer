/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Caching;
using ASC.Core;

namespace ASC.Bookmarking.Common
{

    public static class BookmarkingBusinessFactory
    {
        private static readonly ICache CacheAsc = AscCache.Memory;
        public static T GetObjectFromSession<T>() where T : class, new()
        {
            T obj;
            var key = typeof(T).ToString() + SecurityContext.CurrentAccount.ID.ToString();
            obj = CacheAsc.Get<T>(key);
            if (obj == null)
            {
                obj = new T();
                CacheAsc.Insert(key, obj, TimeSpan.FromMinutes(15));
            }
            return obj;

        }

        public static void UpdateObjectInSession<T>(T obj) where T : class, new()
        {
            var key = typeof(T).ToString() + SecurityContext.CurrentAccount.ID.ToString();
            CacheAsc.Insert(key, obj, TimeSpan.FromMinutes(15));
        }

        public static void UpdateDisplayMode(BookmarkDisplayMode mode)
        {
            var key = typeof(BookmarkDisplayMode).Name + SecurityContext.CurrentAccount.ID.ToString();

            CacheAsc.Insert(key, mode, TimeSpan.FromMinutes(15));
        }

        public static BookmarkDisplayMode GetDisplayMode()
        {
            var key = typeof(BookmarkDisplayMode).Name + SecurityContext.CurrentAccount.ID.ToString();

            var value = CacheAsc.Get<object>(key);
            if (value != null)
            {
                return (BookmarkDisplayMode)value;
            }

            return BookmarkDisplayMode.AllBookmarks;
        }
    }
}
