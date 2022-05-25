using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migratingApiUser", Namespace = "")]
    public class MigratingApiUser : ImportableApiEntity
    {
        [JsonRequired]
        [DataMember(Name = "key", IsRequired = true)]
        public string Key { get; set; }
        
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [JsonRequired]
        [DataMember(Name = "displayName", IsRequired = true)]
        public string DisplayName { get; set; }
        [JsonRequired]
        [DataMember(Name = "moduleName", IsRequired = true)]
        public string ModuleName { get; set; }

        [DataMember(Name = "migratingContacts")]
        public MigratingApiContacts MigratingContacts { get; set; } = default;

        [DataMember(Name = "migratingCalendar")]
        public MigratingApiCalendar MigratingCalendar { get; set; } = default;

        [DataMember(Name = "migratingFiles")]
        public MigratingApiFiles MigratingFiles { get; set; } = default;

        [DataMember(Name = "migratingMail")]
        public MigratingApiMail MigratingMail { get; set; } = default;
    }
}
