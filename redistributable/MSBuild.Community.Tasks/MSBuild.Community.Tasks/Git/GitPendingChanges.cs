using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.Git
{
    /// <summary>
    /// A task for git to detect if there are pending changes
    /// </summary>
    public class GitPendingChanges : GitClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitPendingChanges"/> class.
        /// </summary>
        public GitPendingChanges()
        {
            Command = "status";
        }

        /// <summary>
        /// Gets or sets whether the working directory has pending changes.
        /// </summary>
        [Output]
        public bool HasPendingChanges { get; set; }

        /// <summary>
        /// Generates the arguments.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void GenerateArguments(CommandLineBuilder builder)
        {
            builder.AppendSwitch("--porcelain");
            base.GenerateArguments(builder);
        }

        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            HasPendingChanges = HasPendingChanges || singleLine.Trim().Length > 0;
        }
    }
}
