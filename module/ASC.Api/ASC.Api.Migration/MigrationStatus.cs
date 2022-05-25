using ASC.Migration.Core.Models.Api;

namespace ASC.Api.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class MigrationStatus
    {
        /// <summary>
        /// 
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProgressStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MigrationApiInfo ParseResult { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool MigrationEnded { get; set; }
    }
}
