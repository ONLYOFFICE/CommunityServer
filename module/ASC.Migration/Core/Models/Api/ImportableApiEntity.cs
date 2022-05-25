using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    public abstract class ImportableApiEntity
    {
        [JsonRequired]
        [DataMember(Name = "shouldImport", IsRequired = true)]
        public bool ShouldImport { get; set; } = true;
    }
}
