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
using ASC.Api.Exceptions;

namespace ASC.Api.Utils
{
    public static class Validate
    {
        public static T If<T>(this T item,Func<T,bool> @if, Func<T> then) where T : class
        {
            return @if(item) ? then() : item;
        }

        public static T IfNull<T>(this T item, Func<T> func) where T:class
        {
            return item.If((x)=>x==default(T),func);
        }

        public static T ThrowIfNull<T>(this T item, Exception e) where T : class
        {
            return item.IfNull(() => { throw e; });
        }

        public static T NotFoundIfNull<T>(this T item) where T : class
        {
            return NotFoundIfNull<T>(item, "Item not found");
        }

        public static T NotFoundIfNull<T>(this T item, string message) where T : class
        {
            return item.IfNull(() => { throw new ItemNotFoundException(message); });
        }
    }
}