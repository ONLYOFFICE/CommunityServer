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


#region usings

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

#endregion

namespace ASC.Api.Collections
{
    public static class Extensions
    {
        public static ItemList<T> ToItemList<T>(this IEnumerable<T> enumerable)
        {
            return new ItemList<T>(enumerable);
        }

        public static SmartList<T> ToSmartList<T>(this IEnumerable<T> enumerable)
        {
            return SmartListFactory.Create(enumerable);
        }

        public static ItemDictionary<TKey, TValue> ToItemDictionary<TKey, TValue>(
            this IDictionary<TKey, TValue> enumerable)
        {
            return new ItemDictionary<TKey, TValue>(enumerable);
        }

        public static NameValueCollection ToNameValueCollection(
            this IEnumerable<KeyValuePair<string, object>> enumerable)
        {
            var collection = new NameValueCollection();
            foreach (var keyValuePair in enumerable.Where(keyValuePair => keyValuePair.Value is string))
            {
                collection.Add(keyValuePair.Key,(string)keyValuePair.Value);
            }
            return collection;
        }
    }
}