using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.DeepLink
{
    [Serializable]
    public class DeepLinkData
    {
        [JsonProperty("portal")]
        [DataMember(Name = "portal")]
        public string Portal { get; set; }

        [JsonProperty("email")]
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [JsonProperty("file")]
        [DataMember(Name = "file")]
        public DeepLinkDataFile File { get; set; }

        [JsonProperty("folder")]
        [DataMember(Name = "folder")]
        public DeepLinkDataFolder Folder { get; set; }

        [JsonProperty("originalUrl")]
        [DataMember(Name = "originalUrl")]
        public string OriginalUrl { get; set; }
    }
}