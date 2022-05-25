using System.Runtime.Serialization;

namespace ASC.Migration.Core.Models
{
    [DataContract(Name = "migrationModules", Namespace = "")]
    public class MigrationModules
    {
        public MigrationModules(){ }
        public MigrationModules(string migrationModule, string module)
        {
            MigrationModule = migrationModule;
            Module = module;
        }
        [DataMember(Name = "migrationModule")]
        public string MigrationModule { get; set; }
        [DataMember(Name = "module")]
        public string Module { get; set; }
    }
}
