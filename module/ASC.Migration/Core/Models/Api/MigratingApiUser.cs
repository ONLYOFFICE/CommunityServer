/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


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
