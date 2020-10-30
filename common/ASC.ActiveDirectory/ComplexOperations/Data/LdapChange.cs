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


using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ASC.ActiveDirectory.ComplexOperations.Data
{
    public class LdapChange
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LdapChangeAction Action { get; private set; }

        public string Sid { get; private set;  }

        public string Name { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LdapChangeType Type { get; private set; }

        public List<LdapItemChange> Changes { get; private set; }

        public LdapChange(string sid, string name, LdapChangeType type, LdapChangeAction action,
            List<LdapItemChange> changes = null) :this(sid, name, null, type, action, changes)
        {
        }

        public LdapChange(string sid, string name, string email, LdapChangeType type, LdapChangeAction action, List<LdapItemChange> changes = null)
        {
            Sid = sid;
            Name = name;
            Type = type;
            Action = action;
            Changes = changes ?? new List<LdapItemChange>();
            Email = email;
        }
    }
}