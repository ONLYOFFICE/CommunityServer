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
