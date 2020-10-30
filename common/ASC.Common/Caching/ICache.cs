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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ASC.Common.Caching
{
    public interface ICache
    {
        T Get<T>(string key) where T : class;

        void Insert(string key, object value, TimeSpan sligingExpiration);

        void Insert(string key, object value, DateTime absolutExpiration);

        void Remove(string key);

        void Remove(Regex pattern);


        IDictionary<string, T> HashGetAll<T>(string key);

        T HashGet<T>(string key, string field);

        void HashSet<T>(string key, string field, T value);
    }
}
