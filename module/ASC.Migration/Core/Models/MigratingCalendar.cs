using System;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingCalendar : ImportableEntity
    {
        public abstract int CalendarsCount { get; }
        public abstract int EventsCount { get; }
        public abstract string ModuleName { get; }

        public virtual MigratingApiCalendar ToApiInfo()
        {
            return new MigratingApiCalendar()
            {
                CalendarsCount = CalendarsCount,
                ModuleName = ModuleName,
                EventsCount = EventsCount
            };
        }

        protected MigratingCalendar(Action<string, Exception> log) : base(log) { }
    }
}
