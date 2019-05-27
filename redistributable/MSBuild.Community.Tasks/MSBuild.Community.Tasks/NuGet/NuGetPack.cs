#region Copyright © 2011 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;



namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Creates a NuGet package based on the specified nuspec or project file.
    /// </summary>
    public class NuGetPack : NuGetBase
    {
        /// <summary>
        /// The Regex used to get the full path to the NuGet Package created by the NuGetPack Task
        /// </summary>
        private static readonly Regex _outputFilePathParse = new Regex(@"Successfully created package '(?<filename>.*)'.", RegexOptions.Compiled);

        /// <summary>
        /// The location of the nuspec or project file to create a package.
        /// </summary>
        [Required]
        public ITaskItem File { get; set; }

        /// <summary>
        /// Specifies the directory for the created NuGet package.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Overrides the version number from the nuspec file.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Appends a pre-release suffix to the internally generated version number.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// The base path of the files defined in the nuspec file.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Specifies one or more wildcard patterns to exclude when creating a package.
        /// </summary>
        /// <remarks>
        /// Only available starting in version 1.3.
        /// </remarks>
        public string Exclude { get; set; }

        /// <summary>
        /// Prevent inclusion of empty directories when building the package
        /// </summary>
        public bool ExcludeEmptyDirectories { get; set; }

        /// <summary>
        /// Shows verbose output for package building.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Deprecated in NuGet 2.0.  Use <see cref="Verbosity"/> instead.
        /// </remarks>
        public bool Verbose { get; set; }

        /// <summary>
        /// Display this amount of details in the output.
        /// </summary>
        /// <value><c>normal</c>; <c>quiet</c>; ; <c>detailed</c></value>
        public string Verbosity { get; set; }

        /// <summary>
        /// Determines if the project should be built before building the package.
        /// </summary>
        /// <value><c>true</c> if project should build first; otherwise, <c>false</c>.</value>
        public bool Build { get; set; }

        /// <summary>
        /// Determines if the output files of the project should be in the tool folder.
        /// </summary>
        /// <value><c>true</c> if output files should be in the tool folder; otherwise, <c>false</c>.</value>
        public bool Tool { get; set; }

        /// <summary>
        /// Determines if a package containing sources and symbols should be created. When specified with a nuspec, 
        /// creates a regular NuGet package file and the corresponding symbols package.
        /// </summary>
        /// <value><c>true</c> if symbols; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Only available starting in version 1.4.
        /// </remarks>
        public bool Symbols { get; set; }

        /// <summary>
        /// Provides the ability to specify a semicolon ";" delimited list of properties when creating a package.
        /// </summary>
        public string Properties { get; set; }

        /// <summary>
        /// Prevent default exclusion of NuGet package files and files and folders starting with a dot e.g. .svn.
        /// </summary>
        /// <value><c>true</c> to prevent default exclusion; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Only available starting in version 1.3.
        /// </remarks>
        public bool NoDefaultExcludes { get; set; }

        /// <summary>
        /// Specify if the command should not run package analysis after building the package.
        /// </summary>
        public bool NoPackageAnalysis { get; set; }

        /// <summary>
        /// Include referenced projects either as dependencies or as part of the package. If a referenced project
        /// has a corresponding nuspec file that has the same name as the project, then that referenced project is
        /// added as a dependency. Otherwise, the referenced project is added as part of the package.
        /// </summary>
        /// <value><c>true</c> to include referenced projects; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Only available starting in version 2.5.
        /// </remarks>
        public bool IncludeReferencedProjects { get; set; }

        /// <summary>
        /// Set the minClientVersion attribute for the created package. This value will override the value of the
        /// existing minClientVersion attribute (if any) in the .nuspec file.
        /// </summary>
        /// <value>A version number; e.g. "2.5.0" or "2.8.0".</value>
        /// <remarks>
        /// Only available starting in version 2.5.
        /// </remarks>
        public string MinClientVersion { get; set; }

        /// <summary>
        /// (v3.5) Forces NuGet to run using an invariant, English-based culture.
        /// </summary>
        /// <remarks>
        /// Only available starting in version 3.5.
        /// </remarks>
        public bool ForceEnglishOutput { get; set; }

        /// <summary>
        /// The full file path of the NuGet package created by the NuGetPack task
        /// </summary>
        [Output]
        public string OutputFilePath { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("pack");
            builder.AppendFileNameIfNotNull(File);
            builder.AppendSwitchIfNotNull("-OutputDirectory ", OutputDirectory);
            builder.AppendSwitchIfNotNull("-BasePath ", BasePath);            
            builder.AppendSwitchIfNotNull("-Version ", Version);
            builder.AppendSwitchIfNotNull("-Suffix ", Suffix);
            builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
            builder.AppendSwitchIfNotNull("-MinClientVersion", MinClientVersion);

            if (ExcludeEmptyDirectories)
                builder.AppendSwitch("-ExcludeEmptyDirectories");

            if (Build)
                builder.AppendSwitch("-Build");

            if (Tool)
                builder.AppendSwitch("-Tool");

            // backward compatible with old Verbose property
            if (Verbosity == null && Verbose)
                builder.AppendSwitch("-Verbosity detailed");

            if (Symbols)
                builder.AppendSwitch("-Symbols");

            if (NoDefaultExcludes)
                builder.AppendSwitch("-NoDefaultExcludes");

            if (NoPackageAnalysis)
                builder.AppendSwitch("-NoPackageAnalysis");

            if (IncludeReferencedProjects)
                builder.AppendSwitch("-IncludeReferencedProjects");

            if (ForceEnglishOutput)
                builder.AppendSwitch("-ForceEnglishOutput");

            builder.AppendSwitchIfNotNull("-Exclude ", Exclude);
            builder.AppendSwitchIfNotNull("-Properties ", Properties);

            return builder.ToString();
        }

        /// <summary>
        /// Logs the events from text output.
        /// </summary>
        /// <param name="singleLine">The single line.</param>
        /// <param name="messageImportance">The message importance.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            bool isError = messageImportance == StandardErrorLoggingImportance;

            if (isError)
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
                return;
            }

            Match outputFilePathMatch = _outputFilePathParse.Match(singleLine);

            if (!outputFilePathMatch.Success)
                return;

            OutputFilePath = outputFilePathMatch.Groups["filename"].Value;
        }
    }
}
