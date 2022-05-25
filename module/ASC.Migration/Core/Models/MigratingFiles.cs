using System;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingFiles : ImportableEntity
    {
        public abstract int FoldersCount { get; }
        public abstract int FilesCount { get; }
        public abstract long BytesTotal { get; }
        public abstract string ModuleName { get; }

        public virtual MigratingApiFiles ToApiInfo()
        {
            return new MigratingApiFiles()
            {
                BytesTotal = BytesTotal,
                FilesCount = FilesCount,
                FoldersCount = FoldersCount,
                ModuleName = ModuleName
            };
        }

        protected MigratingFiles(Action<string, Exception> log) : base(log) { }
    }
}
