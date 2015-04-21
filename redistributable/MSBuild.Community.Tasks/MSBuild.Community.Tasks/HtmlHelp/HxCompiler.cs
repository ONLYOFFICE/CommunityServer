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



namespace MSBuild.Community.Tasks.HtmlHelp
{
    /// <summary>
    /// A Html Help 2.0 compiler task.
    /// </summary>
    public class HxCompiler : ToolTask
    {
        #region Properties
        private ITaskItem _projectFile;

        /// <summary>
        /// Gets or sets the project file path.
        /// </summary>
        /// <value>The project file path.</value>
        [Required]
        public ITaskItem ProjectFile
        {
            get { return _projectFile; }
            set { _projectFile = value; }
        }

        private string _logFile;

        /// <summary>
        /// Gets or sets the log file.
        /// </summary>
        /// <value>The log file.</value>
        public string LogFile
        {
            get { return _logFile; }
            set { _logFile = value; }
        }

        private string _projectRoot;

        /// <summary>
        /// Gets or sets the project root.
        /// </summary>
        /// <value>The project root.</value>
        public string ProjectRoot
        {
            get { return _projectRoot; }
            set { _projectRoot = value; }
        }

        private ITaskItem _outputFile;

        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        public ITaskItem OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        private bool _noInfoMessages;

        /// <summary>
        /// Gets or sets a value indicating whether no info messages will be output.
        /// </summary>
        /// <value><c>true</c> if no info messages; otherwise, <c>false</c>.</value>
        public bool NoInfoMessages
        {
            get { return _noInfoMessages; }
            set { _noInfoMessages = value; }
        }

        private bool _noWarningMessages;

        /// <summary>
        /// Gets or sets a value indicating whether no warning messages will be output.
        /// </summary>
        /// <value><c>true</c> if no warning messages; otherwise, <c>false</c>.</value>
        public bool NoWarningMessages
        {
            get { return _noWarningMessages; }
            set { _noWarningMessages = value; }
        }

        private bool _noErrorMessages;

        /// <summary>
        /// Gets or sets a value indicating whether no error messages will be output.
        /// </summary>
        /// <value><c>true</c> if no error messages; otherwise, <c>false</c>.</value>
        public bool NoErrorMessages
        {
            get { return _noErrorMessages; }
            set { _noErrorMessages = value; }
        }

        private bool _quiteMode;

        /// <summary>
        /// Gets or sets a value indicating quite mode.
        /// </summary>
        /// <value><c>true</c> if quite mode; otherwise, <c>false</c>.</value>
        public bool QuiteMode
        {
            get { return _quiteMode; }
            set { _quiteMode = value; }
        }

        private ITaskItem _uncompileFile;

        /// <summary>
        /// Gets or sets the uncompile file.
        /// </summary>
        /// <value>The uncompile file.</value>
        public ITaskItem UncompileFile
        {
            get { return _uncompileFile; }
            set { _uncompileFile = value; }
        }

        private string _uncompileDirectory;

        /// <summary>
        /// Gets or sets the uncompile directory.
        /// </summary>
        /// <value>The uncompile directory.</value>
        public string UncompileDirectory
        {
            get { return _uncompileDirectory; }
            set { _uncompileDirectory = value; }
        }
        #endregion

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            folder = Path.Combine(folder, @"Common Files\microsoft shared\Help 2.0 Compiler");

            if (Directory.Exists(folder))
                return Path.Combine(folder, ToolName);
            else
                return ToolName;
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "hxcomp.exe"; }
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

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitchIfNotNull("-p ", ProjectFile);
            builder.AppendSwitchIfNotNull("-l ", LogFile);
            builder.AppendSwitchIfNotNull("-r ", ProjectRoot);
            builder.AppendSwitchIfNotNull("-o ", OutputFile);
            if (NoInfoMessages)
                builder.AppendSwitch("-i");
            if (NoWarningMessages)
                builder.AppendSwitch("-w");
            if (NoErrorMessages)
                builder.AppendSwitch("-e");
            if (QuiteMode)
                builder.AppendSwitch("-q");
            builder.AppendSwitchIfNotNull("-u ", UncompileFile);
            builder.AppendSwitchIfNotNull("-d ", UncompileDirectory);

            return builder.ToString();
        }

        /// <summary>
        /// Handles execution errors raised by the executable file.
        /// </summary>
        /// <returns>
        /// true if the method runs successfully; otherwise, false.
        /// </returns>
        protected override bool HandleTaskExecutionErrors()
        {
            if (ExitCode != 0)
                Log.LogWarning("'{0}' exited with code {1}.", ToolName, ExitCode);

            //ignore exitcode
            return !Log.HasLoggedErrors;
        }

    }
}
