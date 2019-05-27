using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Installs a package using the specified sources.
    /// </summary>
    public class NuGetInstall : NuGetBase
    {
        /// <summary>
        /// Specify the id of the package to install. If a path to a packages.config file is used instead of an id, all the packages it contains are installed.
        /// </summary>
        [Required]
        public string Package { get; set; }

        /// <summary>
        /// A list of packages sources to use for the install.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The directory in which packages will be installed. If none specified, uses the current directory.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// The version of the package to install.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// If set, the destination directory will contain only the package name, not the version number
        /// </summary>
        public bool ExcludeVersion { get; set; }

        /// <summary>
        /// Allows prerelease packages to be installed. This flag is not required when restoring packages by installing from packages.config.
        /// </summary>
        public bool Prerelease { get; set; }

        /// <summary>
        /// Disable looking up packages from local machine cache.
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// Solution root for package restore.
        /// </summary>
        public string SolutionDirectory { get; set; }

        /// <summary>
        /// The NuGet configuation file. If not specified, file %AppData%\NuGet\NuGet.config is used as configuration file.
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
        /// Specifies the types of files to save after package installation: one of nuspec, nupkg, or nuspec;nupkg.
        /// </summary>
        public string PackageSaveMode { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("install");
            builder.AppendFileNameIfNotNull(Package);
            builder.AppendSwitchIfNotNull("-Version ", Version);
            builder.AppendSwitchIfNotNull("-Source ", Source);
            builder.AppendSwitchIfNotNull("-OutputDirectory ", OutputDirectory);
            builder.AppendSwitchIfNotNull("-SolutionDirectory ", SolutionDirectory);
            builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
            builder.AppendSwitchIfNotNull("-ConfigFile ", ConfigFile);
            builder.AppendSwitchIfNotNull("-PackageSaveMode ", PackageSaveMode);

            if (ExcludeVersion)
                builder.AppendSwitch("-ExcludeVersion");
            if (Prerelease)
                builder.AppendSwitch("-Prerelease");
            if (NoCache)
                builder.AppendSwitch("-NoCache");
            if (ForceEnglishOutput)
                builder.AppendSwitch("-ForceEnglishOutput");

            builder.AppendSwitch("-NonInteractive");

            return builder.ToString();
        }

    }
}
