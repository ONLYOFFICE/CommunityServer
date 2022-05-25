using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migratingApiMail", Namespace = "")]
    public class MigratingApiMail : ImportableApiEntity
    {
        // mail count?
        [DataMember(Name = "messagesCount")]
        public int MessagesCount { get; set; } = default;
        [JsonRequired]
        [DataMember(Name = "moduleName", IsRequired = true)]
        public string ModuleName { get; set; }
    }
}

