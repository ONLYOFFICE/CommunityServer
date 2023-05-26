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
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ASC.Migration.GoogleWorkspace.Models.Parse
{
    public class GwsProfile
    {
        [JsonProperty("name")]
        public GwsName Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("emails", NullValueHandling = NullValueHandling.Ignore)]
        public List<GwsEmail> Emails { get; set; }

        [JsonProperty("birthday", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Birthday { get; set; }

        [JsonProperty("gender", NullValueHandling = NullValueHandling.Ignore)]
        public GwsGender Gender { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            switch(errorContext.Member)
            {
                case "birthday":
                case "gender":
                    errorContext.Handled = true; // Ignore those fields if we can't parse them
                    break;
            }
        }
    }

    public class GwsEmail
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class GwsGender
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class GwsName
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }

        [JsonProperty("formattedName")]
        public string FormattedName { get; set; }
    }
}
