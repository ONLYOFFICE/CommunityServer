using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migrationApiInfo", Namespace = "")]
    public class MigrationApiInfo
    {
        [DataMember(Name = "users")]
        public List<MigratingApiUser> Users { get; set; } = new List<MigratingApiUser>();

        [DataMember(Name = "migratorName")]
        public string MigratorName { get; set; }

        [DataMember(Name = "modules")]
        public List<MigrationModules> Modules { get; set; } = new List<MigrationModules>();

        [DataMember(Name = "failedArchives")]
        public List<string> FailedArchives { get; set; } = new List<string>();
        [DataMember(Name = "groups")]
        public List<MigratingApiGroup> Groups { get; set; } = new List<MigratingApiGroup>();
    }
}
