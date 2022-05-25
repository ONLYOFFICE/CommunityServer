using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core
{
    public interface IMigration : IDisposable
    {
        event Action<double, string> OnProgressUpdate;

        double GetProgress();
        string GetProgressStatus();


        void Init(string path, CancellationToken token);

        Task<MigrationApiInfo> Parse();

        Task Migrate(MigrationApiInfo migrationInfo);

        Stream GetLogs();

        List<Guid> GetGuidImportedUsers();
    }
}
