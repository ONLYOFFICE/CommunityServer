using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migratingApiFiles", Namespace = "")]
    public class MigratingApiFiles : ImportableApiEntity
    {
        [DataMember(Name = "foldersCount")]
        public int FoldersCount { get; set; }
        [DataMember(Name = "filesCount")]
        public int FilesCount { get; set; }
        [DataMember(Name = "bytesTotal")]
        public long BytesTotal { get; set; }
        [JsonRequired]
        [DataMember(Name = "moduleName", IsRequired = true)]
        public string ModuleName { get; set; }
    }
}
