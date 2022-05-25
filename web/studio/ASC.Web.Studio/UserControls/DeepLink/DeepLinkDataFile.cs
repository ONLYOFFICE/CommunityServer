using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.DeepLink
{
    [Serializable]
    public class DeepLinkDataFile
    {
        [JsonProperty("id")]
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [JsonProperty("extension")]
        [DataMember(Name = "extension")]
        public string Extension { get; set; }

    }
}