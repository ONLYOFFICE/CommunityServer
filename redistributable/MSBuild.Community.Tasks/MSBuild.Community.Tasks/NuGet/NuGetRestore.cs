using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Downloads and unzips (restores) any packages missing from the packages folder.
    /// </summary>
    public class NuGetRestore : NuGetBase
    {
        /// <summary>
        /// If a solution is specified, this command restores NuGet packages that are 
        /// installed in the solution and in projects contained in the solution. Otherwise, 
        /// the command restores packages listed in the specified packages.config file.
        /// </summary>
        [Required]
        public string Solution { get; set; }

        /// <summary>
        /// A list of packages sources to use for the install.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The directory in which packages will be installed. If none specified, uses the current directory.
        /// </summary>
        public string PackagesDirectory { get; set; }

        /// <summary>
        /// Disable parallel nuget package restores.
        /// </summary>
        public bool DisableParallelProcessing { get; set; }

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
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("restore");
            builder.AppendFileNameIfNotNull(Solution);
            builder.AppendSwitchIfNotNull("-PackagesDirectory ", PackagesDirectory);
            builder.AppendSwitchIfNotNull("-SolutionDirectory ", SolutionDirectory);
            builder.AppendSwitchIfNotNull("-Source ", Source);
            builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
            builder.AppendSwitchIfNotNull("-ConfigFile ", ConfigFile);

            if (DisableParallelProcessing)
                builder.AppendSwitch("-DisableParallelProcessing");
            if (NoCache)
                builder.AppendSwitch("-NoCache");
            if (ForceEnglishOutput)
                builder.AppendSwitch("-ForceEnglishOutput");

            builder.AppendSwitch("-NonInteractive");

            return builder.ToString();
        }

    }
}


