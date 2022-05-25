using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migratingApiGroup", Namespace = "")]
    public class MigratingApiGroup : ImportableApiEntity
    {
        [DataMember(Name = "groupName")]
        public string GroupName { get; set; } = default;
        [JsonRequired]
        [DataMember(Name = "moduleName", IsRequired = true)]
        public string ModuleName { get; set; }
        [DataMember(Name ="userUidList")]
        public List<string> UserUidList { get; set; }
    }
}
