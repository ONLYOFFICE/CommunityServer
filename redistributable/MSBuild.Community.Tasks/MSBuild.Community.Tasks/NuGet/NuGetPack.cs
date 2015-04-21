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



namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Creates a NuGet package based on the specified nuspec or project file.
    /// </summary>
    public class NuGetPack : NuGetBase
    {
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
        /// The base path of the files defined in the nuspec file.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Shows verbose output for package building.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        public bool Verbose { get; set; }

        /// <summary>
        /// Determines if a package containing sources and symbols should be created. When specified with a nuspec, 
        /// creates a regular NuGet package file and the corresponding symbols package.
        /// </summary>
        /// <value>
        ///   <c>true</c> if symbols; otherwise, <c>false</c>.
        /// </value>
        public bool Symbols { get; set; }

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
            if (Verbose)
                builder.AppendSwitch("-Verbose");
            if (Symbols)
                builder.AppendSwitch("-Symbols");

            return builder.ToString();
        }
    }
}