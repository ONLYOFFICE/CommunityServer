using System;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingContacts : ImportableEntity
    {
        public abstract int ContactsCount { get; }
        public abstract string ModuleName { get; }

        public virtual MigratingApiContacts ToApiInfo()
        {
            return new MigratingApiContacts()
            {
                ContactsCount = ContactsCount,
                ModuleName = ModuleName
            };
        }

        protected MigratingContacts(Action<string, Exception> log) : base(log) { }
    }
}
