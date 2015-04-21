#region Copyright © 2008 Paul Welter. All rights reserved.
/*
Copyright © 2008 Paul Welter. All rights reserved.

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

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;



namespace MSBuild.Community.Tasks.Sandcastle
{
    /// <summary>
    /// MRefBuilder task for Sandcastle.
    /// </summary>
    public class MRefBuilder : SandcastleToolBase
    {
        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        [Required]
        public ITaskItem OutputFile { get; set; }

        /// <summary>
        /// Gets or sets the config file.
        /// </summary>
        /// <value>The config file.</value>
        public ITaskItem ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the references.
        /// </summary>
        /// <value>The references.</value>
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Gets or sets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include internal members.
        /// </summary>
        /// <value><c>true</c> to include internal; otherwise, <c>false</c>.</value>
        public bool IncludeInternal { get; set; }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "MRefBuilder.exe"; }
        }


        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitchIfNotNull("/out:", OutputFile);
            builder.AppendSwitchIfNotNull("/config:", ConfigFile);
            builder.AppendSwitchIfNotNull("/dep:", References, ",");
            if (IncludeInternal)
                builder.AppendSwitch("/internal+");
            builder.AppendFileNamesIfNotNull(Assemblies, " ");

            return builder.ToString();
        }
    }
}
