using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Core.Common.Notify.FireBase.Dao
{
    [Serializable]
    public class NotifyData
    {
        [JsonProperty("portal")]
        [DataMember(Name = "portal")]
        public string Portal { get; set; }

        [JsonProperty("email")]
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [JsonProperty("file")]
        [DataMember(Name = "file")]
        public NotifyFileData File { get; set; }

        [JsonProperty("folder")]
        [DataMember(Name = "folder")]
        public NotifyFolderData Folder { get; set; }

        [JsonProperty("originalUrl")]
        [DataMember(Name = "originalUrl")]
        public string OriginalUrl { get; set; }
    }
}
