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


using System;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingUser<TContacts, TCalendar, TFiles, TMail> : ImportableEntity
        where TContacts : MigratingContacts
        where TCalendar : MigratingCalendar
        where TFiles : MigratingFiles
        where TMail : MigratingMail
    {
        public string Key { get; set; }

        public abstract string Email { get; }
        public abstract string DisplayName { get; }
        public abstract string ModuleName { get; }

        public TContacts MigratingContacts { get; set; } = default;

        public TCalendar MigratingCalendar { get; set; } = default;

        public TFiles MigratingFiles { get; set; } = default;

        public TMail MigratingMail { get; set; } = default;

        public virtual MigratingApiUser ToApiInfo()
        {
            return new MigratingApiUser()
            {
                Key = Key,
                Email = Email,
                DisplayName = DisplayName,
                ModuleName = ModuleName,
                MigratingCalendar = MigratingCalendar.ToApiInfo(),
                MigratingContacts = MigratingContacts.ToApiInfo(),
                MigratingFiles = MigratingFiles.ToApiInfo(),
                MigratingMail = MigratingMail.ToApiInfo()
            };
        }

        protected MigratingUser(Action<string, Exception> log) : base(log) { }
    }
}
