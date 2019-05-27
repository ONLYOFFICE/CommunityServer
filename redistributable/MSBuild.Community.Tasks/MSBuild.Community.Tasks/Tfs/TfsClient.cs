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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.Tfs
{
    /// <summary>
    /// A task for Team Foundation Server version control.
    /// </summary>
    public class TfsClient : ToolTask
    {        
        /// <summary>
        /// Gets or sets the Team Foundation Server command.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is recursive.
        /// </summary>
        /// <value>
        ///   <c>true</c> if recursive; otherwise, <c>false</c>.
        /// </value>
        public bool Recursive { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is all.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all; otherwise, <c>false</c>.
        /// </value>
        public bool All { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is overwrite.
        /// </summary>
        /// <value>
        ///   <c>true</c> if overwrite; otherwise, <c>false</c>.
        /// </value>
        public bool Overwrite { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is force.
        /// </summary>
        /// <value>
        ///   <c>true</c> if force; otherwise, <c>false</c>.
        /// </value>
        public bool Force { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is preview.
        /// </summary>
        /// <value>
        ///   <c>true</c> if preview; otherwise, <c>false</c>.
        /// </value>
        public bool Preview { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is remap.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remap; otherwise, <c>false</c>.
        /// </value>
        public bool Remap { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is silent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if silent; otherwise, <c>false</c>.
        /// </value>
        public bool Silent { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is saved.
        /// </summary>
        /// <value>
        ///   <c>true</c> if saved; otherwise, <c>false</c>.
        /// </value>
        public bool Saved { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is validate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if validate; otherwise, <c>false</c>.
        /// </value>
        public bool Validate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TfsClient"/> is bypass.
        /// </summary>
        /// <value>
        ///   <c>true</c> if bypass; otherwise, <c>false</c>.
        /// </value>
        public bool Bypass { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets the lock.
        /// </summary>
        public string Lock { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// Gets or sets the collection.
        /// </summary>
        public string Collection { get; set; }
        /// <summary>
        /// Gets or sets a the override reason.
        /// </summary>
        public string Override { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the name of the workspace.
        /// </summary>
        public string WorkspaceName { get; set; }
        /// <summary>
        /// Gets or sets the workspace owner.
        /// </summary>
        public string WorkspaceOwner { get; set; }
        /// <summary>
        /// Gets or sets the name of the shelve set.
        /// </summary>
        public string ShelveSetName { get; set; }
        /// <summary>
        /// Gets or sets the shelve set owner.
        /// </summary>
        public string ShelveSetOwner { get; set; }

        /// <summary>
        /// Gets the output resulting from executing this command.
        /// </summary>
        public StringBuilder Output { get; private set; }

        /// <summary>
        /// Gets or sets the working directory used when executing this tool.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        [Output]
        public string Changeset { get; set; }

        /// <summary>
        /// Gets or sets the changeset version passed in the version spec parameter
        /// </summary>
        /// <value>
        /// The changeset version.
        /// </value>
        /// <example>
        /// /v:C{ChangesetVersion}
        /// </example>
        public string ChangesetVersion { get; set; }

        /// <summary>
        /// Gets or sets the server path.
        /// </summary>
        [Output]
        public string ServerPath { get; set; }

        private static readonly string[] candidatePaths =
        {
            @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE",
            @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE",
            @"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE",
            @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE"
        };

        private const int MaxCommandlineLength = 32000;        

        private string FindToolPath(string toolName)
        {
            return candidatePaths.FirstOrDefault(Directory.Exists);
        }

        /// <inheritdoc />
        protected override string GetWorkingDirectory()
        {
            if (!string.IsNullOrEmpty(this.WorkingDirectory))
            {
                this.Log.LogMessage("Setting working directory to {0}.", this.WorkingDirectory);
            }

            return this.WorkingDirectory;
        }

        /// <summary>
        /// Generates the command.
        /// </summary>
        /// <param name="builder">The <see cref="CommandLineBuilder"/>.</param>
        protected virtual void GenerateCommand(CommandLineBuilder builder)
        {
            builder.AppendSwitch(Command);
            builder.AppendFileNamesIfNotNull(Files, " ");
        }

        /// <summary>
        /// Generates the arguments.
        /// </summary>
        /// <param name="builder">The <see cref="CommandLineBuilder"/>.</param>
        protected virtual void GenerateArguments(CommandLineBuilder builder)
        {
            builder.AppendSwitch("/noprompt");

            builder.AppendSwitchIfNotNull("/comment:", Comment);
            builder.AppendSwitchIfNotNull("/version:", Version);
            builder.AppendSwitchIfNotNull("/lock:", Lock);
            builder.AppendSwitchIfNotNull("/type:", Type);
            builder.AppendSwitchIfNotNull("/author:", Author);
            builder.AppendSwitchIfNotNull("/notes:", Notes);
            builder.AppendSwitchIfNotNull("/format:", Format);
            builder.AppendSwitchIfNotNull("/collection:", Collection);
            builder.AppendSwitchIfNotNull("/v:C", ChangesetVersion);
            builder.AppendSwitchIfNotNull("/override:", Override);

            if (Recursive)
                builder.AppendSwitch("/recursive");
            if (All)
                builder.AppendSwitch("/all");
            if (Overwrite)
                builder.AppendSwitch("/overwrite");
            if (Force)
                builder.AppendSwitch("/force");
            if (Preview)
                builder.AppendSwitch("/preview");
            if (Remap)
                builder.AppendSwitch("/remap");
            if (Silent)
                builder.AppendSwitch("/silent");
            if (Saved)
                builder.AppendSwitch("/saved");
            if (Validate)
                builder.AppendSwitch("/validate");
            if (Bypass)
                builder.AppendSwitch("/bypass");

            if (!string.IsNullOrEmpty(UserName))
            {
                string login = "/login:" + UserName;
                if (!string.IsNullOrEmpty(Password))
                    login += "," + Password;

                builder.AppendSwitch(login);
            }

            if (!string.IsNullOrEmpty(WorkspaceName))
            {
                string workspace = "/workspace:" + WorkspaceName;
                if (!string.IsNullOrEmpty(WorkspaceOwner))
                    workspace += "," + WorkspaceOwner;

                builder.AppendSwitch(workspace);
            }

            if (!string.IsNullOrEmpty(ShelveSetName))
            {
                string shelveset = "/shelveset:" + ShelveSetName;
                if (!string.IsNullOrEmpty(ShelveSetOwner))
                    shelveset += "," + ShelveSetOwner;

                builder.AppendSwitch(shelveset);
            }
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            if (string.IsNullOrEmpty(ToolPath))
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
            get { return "tf.exe"; }
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
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            bool isError = messageImportance == StandardErrorLoggingImportance;

            if (isError)
                base.LogEventsFromTextOutput(singleLine, messageImportance);

            if (string.IsNullOrEmpty(singleLine))
                return;

            ParseOutput(singleLine);
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            this.Output = new StringBuilder();
            if (BatchRequired.GetValueOrDefault())
            {
                return ExecuteBatchMode();
            }

            return base.Execute();
        }

        private bool ExecuteBatchMode()
        {
            var originalFiles = (ITaskItem[]) this.Files.Clone();
            var executeSuccess = true;

            int index = 0;

            while (executeSuccess && index < originalFiles.Length)
            {
                var batchFiles = new List<ITaskItem>();
                
                this.GetNextBatch(index, originalFiles, batchFiles);
                index = index + batchFiles.Count;

                this.Files = batchFiles.ToArray();
                executeSuccess = base.Execute();                
            }

            return executeSuccess;
        }

        private void GetNextBatch(int indexOffSet, ITaskItem[] originalFiles, List<ITaskItem> batchFiles)
        {
            const int offSet = 1000; // for other switches            
            int arrayLength = 0 + offSet;
            var buildBatch = true;                
            while (buildBatch)
            {
                if (indexOffSet >= originalFiles.Length)
                {
                    buildBatch = false;
                    continue;
                }

                var fileLength = originalFiles[indexOffSet].ItemSpec.Length + 1; // + 1 for the space
                if (fileLength + arrayLength > MaxCommandlineLength)
                {
                    buildBatch = false;
                    continue;
                }

                batchFiles.Add(originalFiles[indexOffSet]);
                indexOffSet = indexOffSet + 1;
                arrayLength = arrayLength + fileLength;
            }
        }

        private bool? batchRequired;
        /// <summary>
        /// Gets a value determining if this command must be processed in batch mode, due
        /// to the length of the commandline arguments.
        /// </summary>
        public bool? BatchRequired
        {
            get
            {
                if (batchRequired == null)
                {
                    if (GenerateCommandLineCommands().Length > MaxCommandlineLength)
                    {
                        batchRequired = true;
                    }

                }

                return batchRequired;
            }
            private set { batchRequired = value; }
        }


        private void ParseOutput(string singleLine)
        {
            this.Output.AppendLine(singleLine);
            Match m = Regex.Match(singleLine, @"(?<Name>[\w ]+)\s*\:(?<Value>[^\r\n]+)");
            if (!m.Success)
                return;

            string name = m.Groups["Name"].Value.Trim();
            string value = m.Groups["Value"].Value.Trim();

            switch (name)
            {
                case "Changeset":
                    Changeset = value;
                    break;
                case "Server path":
                    ServerPath = value;
                    break;
            }
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
                Log.LogError(Properties.Resources.ParameterRequired, "TfsClient", "Command");
                return false;
            }
            return base.ValidateParameters();
        }
    }
}
