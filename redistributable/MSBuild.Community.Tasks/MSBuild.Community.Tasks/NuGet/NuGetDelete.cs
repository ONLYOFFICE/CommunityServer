using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Deletes a package with a specific version. It can be useful if the server has disallow 
    /// to overwrite existing packages.
    /// </summary>
    public class NuGetDelete : NuGetBase
    {
        /// <summary>
        /// Specify the id of the package to delete.
        /// </summary>
        [Required]
        public string Package { get; set; }

        /// <summary>
        /// The version of the package to delete.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// A list of packages sources to use for the delete.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The API key for the server.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// (v2.5) The NuGet configuation file. If not specified, file %AppData%\NuGet\NuGet.config is used as configuration file.
        /// </summary>
        public string ConfigFile { get; set; }

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
            builder.AppendSwitch("delete");
            builder.AppendFileNameIfNotNull(Package);
            builder.AppendFileNameIfNotNull(Version);
            builder.AppendSwitchIfNotNull("-ApiKey ", ApiKey);
            builder.AppendSwitchIfNotNull("-Source ", Source);
            builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
            builder.AppendSwitchIfNotNull("-ConfigFile ", ConfigFile);

            if (ForceEnglishOutput)
                builder.AppendSwitch("-ForceEnglishOutput");

            builder.AppendSwitch("-NonInteractive");

            return builder.ToString();
        }
    }
}