using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ASC.Core.Common.Notify.FireBase.Dao
{
    public class NotifyFolderData
    {
        [JsonProperty("id")]
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [JsonProperty("parentId")]
        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }
        [JsonProperty("rootFolderType")]
        [DataMember(Name = "rootFolderType")]
        public int RootFolderType { get; set; }
    }
}
