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
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.SourceServer
{
    /*
      srctool: dumps source information from a pdb
         you must specify a pdb or srcsrv stream file on the command line
         optional '-u displays only source files that are not indexed
         optional '-r' dumps raw source data from the pdb
         optional '-l:<mask>' limits to  only source files that match this regular expression
         optional '-x' extracts the files, instead of simply listing them
         optional '-f' extracts files to a flat directory
         optional '-n' shows version control commands and output while extracting
         optional '-d:<dir>' specifies the directory to extract to
         optional '-c displays only the count of indexed files - no detail
    */

    /// <summary>
    /// A task for the srctool from source server.
    /// </summary>
    public class SrcTool : ToolTask
    {
        #region Properties
        /// <summary>
        /// Gets or sets the PDB file.
        /// </summary>
        /// <value>The PDB file.</value>
        public ITaskItem PdbFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to dumps raw source data from the PDB. The -r switch.
        /// </summary>
        /// <value><c>true</c> if source only; otherwise, <c>false</c>.</value>
        public bool SourceOnly { get; set; }

        /// <summary>
        /// Gets or sets the filter to only source files that match this regular expression. The -l switch.
        /// </summary>
        /// <value>The filter regular expression.</value>
        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to extracts the files, instead of simply listing them. The -x switch.
        /// </summary>
        /// <value><c>true</c> if extract; otherwise, <c>false</c>.</value>
        public bool Extract { get; set; }

        /// <summary>
        /// Gets or sets the directory to extract to. The -d switch.
        /// </summary>
        /// <value>The extract directory.</value>
        public string ExtractDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to get the count of indexed files. The -c switch.
        /// </summary>
        /// <value><c>true</c> if count only; otherwise, <c>false</c>.</value>
        public bool CountOnly { get; set; }

        private int _sourceCount;

        /// <summary>
        /// Gets or sets the number of source files.
        /// </summary>
        /// <value>The number of source files.</value>
        [Output]
        public int SourceCount
        {
            get { return _sourceCount; }
            set { _sourceCount = value; }
        }

        private List<string> _sourceFiles = new List<string>();

        /// <summary>
        /// Gets the source files. Populated when <see cref="SourceOnly"/> is <c>true</c>.
        /// </summary>
        /// <value>The source files.</value>
        [Output]
        public string[] SourceFiles
        {
            get { return _sourceFiles.ToArray(); }
        }

        private List<string> _extractedFiles = new List<string>();

        /// <summary>
        /// Gets the extracted files. Populated when <see cref="Extract"/> is <c>true</c>.
        /// </summary>
        /// <value>The extracted files.</value>
        [Output]
        public string[] ExtractedFiles
        {
            get { return _extractedFiles.ToArray(); }
        }
        #endregion

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();

            if (SourceOnly)
                builder.AppendSwitch("-r");
            if (Extract)
                builder.AppendSwitch("-x");
            if (CountOnly)
                builder.AppendSwitch("-c");

            builder.AppendSwitchIfNotNull("-d:", ExtractDirectory);

            builder.AppendFileNameIfNotNull(PdbFile);

            return builder.ToString();
        }

        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (string.IsNullOrEmpty(singleLine))
                return;

            if (SourceOnly)
            {
                // the line will be the source file path
                if (!singleLine.StartsWith(PdbFile.ItemSpec))
                {
                    _sourceFiles.Add(singleLine);
                }
            }
            else if (Extract)
            {
                // the line can be eiher the extracted file or the source count
                if (!singleLine.StartsWith(PdbFile.ItemSpec, StringComparison.OrdinalIgnoreCase))
                    _extractedFiles.Add(singleLine);
            }
            else if (!CountOnly)
            {
                // if not source, extract or count, forward to normal log
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }

            // always look for source count
            if (singleLine.StartsWith(PdbFile.ItemSpec, StringComparison.OrdinalIgnoreCase))
                ParseCount(singleLine);

            // always look for not indexed
            if (!CountOnly && singleLine.Contains("is not source indexed"))
                Log.LogWarning(singleLine);
        }

        private static readonly Regex _sourceCountRegex = new Regex(@"\d+(?= source)", RegexOptions.IgnoreCase);

        private void ParseCount(string line)
        {
            //file.pdb: 18 source files were extracted

            Match revMatch = _sourceCountRegex.Match(line);
            if (revMatch.Success)
                int.TryParse(revMatch.Value, out _sourceCount);
        }

        /// <summary>
        /// Handles execution errors raised by the executable file.
        /// </summary>
        /// <returns>
        /// true if the method runs successfully; otherwise, false.
        /// </returns>
        protected override bool HandleTaskExecutionErrors()
        {
            // skip error message on count only
            if (CountOnly || this.ExitCode == this.SourceCount)
                return true;

            return base.HandleTaskExecutionErrors();
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
            get { return "srctool.exe"; }
        }

        /// <summary>
        /// Logs the starting point of the run to all registered loggers.
        /// </summary>
        /// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
        protected override void LogToolCommand(string message)
        {
            Log.LogCommandLine(MessageImportance.Low, message);
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get { return MessageImportance.Normal; }
        }
    }
}
