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


using System.Collections.Generic;
using System.Linq;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigrationInfo<TUser, TContacts, TCalendar, TFiles, TMail, TGroup> : IMigrationInfo
        where TUser : MigratingUser<TContacts, TCalendar, TFiles, TMail>
        where TContacts : MigratingContacts
        where TCalendar : MigratingCalendar
        where TFiles : MigratingFiles
        where TMail : MigratingMail
        where TGroup: MigratingGroup
    {
        public Dictionary<string, TUser> Users = new Dictionary<string, TUser>();
        public string MigratorName { get; set; }
        public List<MigrationModules> Modules = new List<MigrationModules>();
        public List<string> failedArchives = new List<string>();
        public List<TGroup> Groups = new List<TGroup>();

        public virtual MigrationApiInfo ToApiInfo()
        {
            return new MigrationApiInfo()
            {
                Users = Users.Values.Select(u => u.ToApiInfo()).ToList(),
                MigratorName = MigratorName,
                Modules = Modules,
                FailedArchives = failedArchives,
                Groups = Groups.Select(g => g.ToApiInfo()).ToList()
            };
        }

        public virtual void Merge(MigrationApiInfo apiInfo)
        {
            foreach(var apiUser in apiInfo.Users)
            {
                if (!Users.ContainsKey(apiUser.Key)) continue;

                var user = Users[apiUser.Key];
                user.ShouldImport = apiUser.ShouldImport;

                user.MigratingCalendar.ShouldImport = apiUser.MigratingCalendar.ShouldImport;
                user.MigratingContacts.ShouldImport = apiUser.MigratingContacts.ShouldImport;
                user.MigratingFiles.ShouldImport = apiUser.MigratingFiles.ShouldImport;
                user.MigratingMail.ShouldImport = apiUser.MigratingMail.ShouldImport;
            }
            foreach(var apiGroup in apiInfo.Groups)
            {
                if (!Groups.Exists(g => apiGroup.GroupName == g.GroupName)) continue;
                var group = Groups.Find(g => apiGroup.GroupName == g.GroupName);
                group.ShouldImport = apiGroup.ShouldImport;
            }
        }
    }
}
