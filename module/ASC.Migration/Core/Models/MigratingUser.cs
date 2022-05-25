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
