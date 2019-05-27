#region Copyright © 2012 Paul Welter. All rights reserved.
/*
Copyright © 2012 Paul Welter. All rights reserved.

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

namespace MSBuild.Community.Tasks.Git
{
    /// <summary>
    /// A task for Git commands.
    /// </summary>
    public class GitClient : ToolTask
    {
        private string _initialToolPath;
        private readonly List<ITaskItem> _consoleOut;
        /// <summary>
        /// Default constructor. Creates a new GitClient task.
        /// </summary>
        public GitClient()
        {
            _consoleOut = new List<ITaskItem>();
            _initialToolPath = ToolPath;
        }

        /// <summary>
        /// Gets or sets the command to run.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the raw arguments to pass to the git command.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets the local or working path for git command.
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// Gets command console output messages.
        /// </summary>
        [Output]
        public ITaskItem[] ConsoleOutput
        {
            get { return _consoleOut.ToArray(); }
            set { }
        }

        private string FindToolPath(string toolName)
        {
            string toolPath =
                ToolPathUtil.FindInRegistry(toolName) ??
                ToolPathUtil.FindInPath(toolName) ??
                ToolPathUtil.FindInProgramFiles(toolName, @"Git\bin") ??
                ToolPathUtil.FindInLocalPath(toolName, LocalPath);

            if (toolPath == null)
            {
                throw new Exception("Could not find git.exe. Looked in PATH locations and various common folders inside Program Files as well as LocalPath.");
            }

            return toolPath;
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var commandLine = new CommandLineBuilder();
            GenerateCommand(commandLine);
            GenerateArguments(commandLine);
            return commandLine.ToString();
        }

        /// <summary>
        /// Generates the command.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected virtual void GenerateCommand(CommandLineBuilder builder)
        {
            builder.AppendSwitch(Command);
        }

        /// <summary>
        /// Generates the arguments.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected virtual void GenerateArguments(CommandLineBuilder builder)
        {
            if (!string.IsNullOrEmpty(Arguments))
                builder.AppendSwitch(Arguments);
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            if (string.IsNullOrEmpty(ToolPath) || ToolPath == _initialToolPath && !ToolPathUtil.SafeFileExists(ToolPath, ToolName))
                ToolPath = FindToolPath(ToolName);

            return Path.Combine(ToolPath, ToolName);
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
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardErrorLoggingImportance
        {
            get { return MessageImportance.High; }
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <returns>
        /// The name of the executable file to run.
        /// </returns>
        protected override string ToolName
        {
            get { return ToolPathUtil.MakeToolName("git"); }
        }

        /// <summary>
        /// Indicates whether all task paratmeters are valid.
        /// </summary>
        /// <returns>
        /// true if all task parameters are valid; otherwise, false.
        /// </returns>
        protected override bool ValidateParameters()
        {
            if (string.IsNullOrEmpty(Command))
            {
                Log.LogError(Properties.Resources.ParameterRequired, "GitClient", "Command");
                return false;
            }
            return base.ValidateParameters();
        }

        /// <summary>
        /// Returns the directory in which to run the executable file.
        /// </summary>
        /// <returns>
        /// The directory in which to run the executable file, or a null reference (Nothing in Visual Basic) if the executable file should be run in the current directory.
        /// </returns>
        protected override string GetWorkingDirectory()
        {
            if (string.IsNullOrEmpty(LocalPath))
                return base.GetWorkingDirectory();

            return LocalPath;
        }

        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            base.LogEventsFromTextOutput(singleLine, messageImportance);

            if (!string.IsNullOrWhiteSpace(singleLine))
            {
                var messageItem = new TaskItem(singleLine);
                _consoleOut.Add(messageItem);
            }
        }

    }
}
