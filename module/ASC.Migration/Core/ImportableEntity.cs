using System;

namespace ASC.Migration.Core
{
    public abstract class ImportableEntity
    {
        public bool ShouldImport { get; set; } = true;

        public abstract void Parse();

        public abstract void Migrate();

        protected Action<string, Exception> Log { get; set; }

        public ImportableEntity(Action<string, Exception> log)
        {
            Log = log;
        }
    }
}
