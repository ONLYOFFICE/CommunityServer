using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Core.Common.Notify.FireBase.Dao
{
    [Serializable]
    public class NotifyFileData
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
