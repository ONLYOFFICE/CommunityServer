using System;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingMail : ImportableEntity
    {
        public abstract int MessagesCount { get; }
        public abstract string ModuleName { get; }
        public virtual MigratingApiMail ToApiInfo()
        {
            return new MigratingApiMail()
            {
                MessagesCount = MessagesCount,
                ModuleName = ModuleName
            };
        }

        protected MigratingMail(Action<string, Exception> log) : base(log) { }
    }
}
