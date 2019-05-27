using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Updates packages
    /// </summary>
    public class NuGetUpdate : NuGetBase
    {
        /// <summary>
        /// Specify the id of the package to update. If a path to a packages.config file is used instead of an id, all the packages it contains are updated.
        /// </summary>
        [Required]
        public string Package { get; set; }

        /// <summary>
        /// The version of the package to update.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// A list of packages sources to use for the update.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The directory in which packages will be updated. If none specified, uses the current directory.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Path to the local packages folder (location where packages are installed).
        /// </summary>
        public string RepositoryPath { get; set; }

        /// <summary>
        /// Looks for updates with the highest version available within the same major and minor version as the installed package.
        /// </summary>
        public bool Safe { get; set; }

        /// <summary>
        /// (v1.4) Update the running NuGet.exe to the newest version available from the server.
        /// </summary>
        public bool Self { get; set; }

        /// <summary>
        /// Allows updating to prerelease versions. This flag is not required when updating prerelease packages that are already installed.
        /// </summary>
        public bool Prerelease { get; set; }

        /// <summary>
        /// (v2.5) The NuGet configuation file. If not specified, file %AppData%\NuGet\NuGet.config is used as configuration file.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// (v2.5) The action to take, when asked to overwrite or ignore existing files referenced by the project: Overwrite, Ignore, None.
        /// </summary>
        public string FileConflictAction { get; set; }

        /// <summary>
        /// Display this amount of details in the output: normal, quiet, detailed.
        /// </summary>
        public string Verbosity { get; set; }
        
        /// <summary>
        /// (v3.5) Forces NuGet to run using an invariant, English-based culture.
        /// </summary>
        /// <remarks>
        /// Only available starting in version 3.5.
        /// </remarks>
        public bool ForceEnglishOutput { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("update");
            builder.AppendFileNameIfNotNull(Package);
            builder.AppendSwitchIfNotNull("-Version ", Version);
            builder.AppendSwitchIfNotNull("-Source ", Source);
            builder.AppendSwitchIfNotNull("-OutputDirectory ", OutputDirectory);
            builder.AppendSwitchIfNotNull("-RepositoryPath ", RepositoryPath);
            builder.AppendSwitchIfNotNull("-OutputDirectory ", OutputDirectory);
            builder.AppendSwitchIfNotNull("-FileConflictAction ", FileConflictAction);
            builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
            builder.AppendSwitchIfNotNull("-ConfigFile ", ConfigFile);

            if (Prerelease)
                builder.AppendSwitch("-Prerelease");
            if (Safe)
                builder.AppendSwitch("-Safe");
            if (Self)
                builder.AppendSwitch("-Self");
            if (ForceEnglishOutput)
                builder.AppendSwitch("-ForceEnglishOutput");

            builder.AppendSwitch("-NonInteractive");

            return builder.ToString();
        }
    }
}