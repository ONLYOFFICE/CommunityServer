using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migratingApiContacts", Namespace = "")]
    public class MigratingApiContacts : ImportableApiEntity
    {
        [DataMember(Name = "contactsCount")]
        public int ContactsCount { get; set; }
        [JsonRequired]
        [DataMember(Name = "moduleName", IsRequired = true)]
        public string ModuleName { get; set; }
    }
}
