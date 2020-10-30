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


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ASC.ActiveDirectory.ComplexOperations.Data
{
    public class LdapItemChange
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LdapItemChangeKey Key { get; private set; }

        public string Before { get; private set; }
        public string After { get; private set; }

        public bool IsChanged { get; private set; }

        public LdapItemChange(LdapItemChangeKey key, string before, string after)
        {
            Key = key;
            Before = before;
            After = after;

            IsChanged = Before != null && !Before.Equals(After) || After != null && !After.Equals(Before);
        }
    }
}