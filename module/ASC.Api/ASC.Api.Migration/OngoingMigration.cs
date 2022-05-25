using System.Threading;
using System.Threading.Tasks;

using ASC.Migration.Core;
using ASC.Migration.Core.Models.Api;

namespace ASC.Api.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class OngoingMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public CancellationTokenSource CancelTokenSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IMigration Migration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Task<MigrationApiInfo> ParseTask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Task MigrationTask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool MigrationEnded =>
            ParseTask != null && ParseTask.IsCompleted
            && MigrationTask != null && MigrationTask.IsCompleted;
    }
}
