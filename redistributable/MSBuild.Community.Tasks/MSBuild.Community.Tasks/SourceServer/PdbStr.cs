#region Copyright © 2009 MSBuild Community Task Project. All rights reserved.
/*
Copyright © 2008 MSBuild Community Task Project. All rights reserved.

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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.SourceServer
{
    #region Enums
    /// <summary>
    /// Commands for the <see cref="PdbStr"/> tasks.
    /// </summary>
    public enum PdbStrCommands
    {
        /// <summary>
        /// Read stream from pdb symbol file.
        /// </summary>
        read,

        /// <summary>
        /// Write stream to pdb symbol file.
        /// </summary>
        write,
    }
    #endregion Enums

    /// <summary>
    /// A task for the pdbstr from source server.
    /// </summary>
    public class PdbStr : ToolTask
    {
        /// <summary>
        /// Gets or sets the PDB file.
        /// </summary>
        /// <value>The PDB file.</value>
        public ITaskItem PdbFile { get; set; }

        /// <summary>
        /// Gets or sets the stream file.
        /// </summary>
        /// <value>The stream file.</value>
        public ITaskItem StreamFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the stream.
        /// </summary>
        /// <value>The name of the stream.</value>
        public string StreamName { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        /// <enum cref="MSBuild.Community.Tasks.SourceServer.PdbStrCommands"/>
        public string Command { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            // pdbstr -r/w -p:PdbFileName -i:StreamFileName -s:StreamName
            var builder = new CommandLineBuilder();
            
            if (string.Equals("write", Command, StringComparison.OrdinalIgnoreCase))
                builder.AppendSwitch("-w");
            else
                builder.AppendSwitch("-r");

            builder.AppendSwitchIfNotNull("-p:", PdbFile);
            builder.AppendSwitchIfNotNull("-i:", StreamFile);
            builder.AppendSwitchIfNotNull("-s:", StreamName);

            return builder.ToString();
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            if (!string.IsNullOrEmpty(ToolPath))
                return Path.Combine(ToolPath, ToolName);

            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (pf.EndsWith("(x86)"))
            {
                string pf64 = pf.Substring(0, pf.Length - 5).Trim();
                string path64 = Path.Combine(pf64, "Debugging Tools for Windows (x64)\\srcsrv");
                if (Directory.Exists(path64))
                    return Path.Combine(path64, ToolName);
            }

            string path = Path.Combine(pf, "Debugging Tools for Windows (x86)\\srcsrv");
            if (Directory.Exists(path))
                return Path.Combine(path, ToolName);

            return ToolName;
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The name of the executable file to run.
        /// </returns>
        protected override string ToolName
        {
            get { return "pdbstr.exe"; }
        }
    }
}
