using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Migration.Core.Models.Api
{
    [DataContract(Name = "migratingApiCalendar", Namespace = "")]
    public class MigratingApiCalendar : ImportableApiEntity
    {
        [DataMember(Name = "calendarsCount")]
        public int CalendarsCount { get; set; }
        [DataMember(Name = "eventsCount")]
        public int EventsCount { get; set; }
        [JsonRequired]
        [DataMember(Name = "moduleName", IsRequired = true)]
        public string ModuleName { get; set; }
    }
}
