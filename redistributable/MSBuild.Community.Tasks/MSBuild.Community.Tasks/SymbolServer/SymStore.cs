#region Copyright © 2008 MSBuild Community Task Project. All rights reserved.
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



namespace MSBuild.Community.Tasks.SymbolServer
{
    #region Enums
    /// <summary>
    /// Commands for the SymStore tasks.
    /// </summary>
    public enum SymStoreCommands
    {
        /// <summary>
        /// Add to the symbol server store.
        /// </summary>
        add,

        /// <summary>
        /// Query the symbol server store.
        /// </summary>
        query,

        /// <summary>
        /// Delete from the symbol serer store.
        /// </summary>
        delete
    }

    #endregion Enums

    /// <summary>
    /// Task that wraps the Symbol Server SymStore.exe application.
    /// </summary>
    public class SymStore : ToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymStore"/> class.
        /// </summary>
        public SymStore()
        {
            Command = "add";
        }

        #region Properties

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        /// <enum cref="MSBuild.Community.Tasks.SymbolServer.SymStoreCommands"/>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets a value indicating SymStore will append new indexing information to an existing index file.
        /// </summary>
        /// <value><c>true</c> if append; otherwise, <c>false</c>.</value>
        public bool Append { get; set; }

        /// <summary>
        /// Gets or sets the comment for the transaction.
        /// </summary>
        /// <value>The comment for the transaction.</value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating SymStore will create a compressed version of each file copied to the symbol store instead of using an uncompressed copy of the file.
        /// </summary>
        /// <value><c>true</c> if compress; otherwise, <c>false</c>.</value>
        public bool Compress { get; set; }

        /// <summary>
        /// Gets or sets a log file to be used for command output. If this is not included, transaction information and other output is sent to stdout.
        /// </summary>
        /// <value>The log file to be used for command output.</value>
        public string LogFile { get; set; }

        /// <summary>
        /// Gets or sets the network path of files or directories to add.
        /// </summary>
        /// <value>The network path of files or directories to add.</value>
        public string Files { get; set; }

        /// <summary>
        /// Gets or sets the server and share where the symbol files were originally stored.
        /// </summary>
        /// <value>The server and share where the symbol files were originally stored.</value>
        public string Share { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID string.
        /// </summary>
        /// <value>The transaction ID string.</value>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file will be in a local directory rather than a network path.
        /// </summary>
        /// <value><c>true</c> if local; otherwise, <c>false</c>.</value>
        public bool Local { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SymStore will display verbose output.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SymStore will store a pointer to the file, rather than the file itself.
        /// </summary>
        /// <value><c>true</c> if pointer; otherwise, <c>false</c>.</value>
        public bool Pointer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SymStore will add files or directories recursively.
        /// </summary>
        /// <value><c>true</c> if recursive; otherwise, <c>false</c>.</value>
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets the root directory for the symbol store.
        /// </summary>
        /// <value>The root directory for the symbol store.</value>
        public string Store { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the version of the product.
        /// </summary>
        /// <value>The version of the product.</value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the message to be added to each file.
        /// </summary>
        /// <value>The message to be added to each file.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the paths in the file pointers will be relative.
        /// </summary>
        /// <value><c>true</c> if relative; otherwise, <c>false</c>.</value>
        public bool Relative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to omit the creation of reference pointer files for the files and pointers being stored.
        /// </summary>
        /// <value><c>true</c> to omit the creation of reference pointer; otherwise, <c>false</c>.</value>
        public bool NoReference { get; set; }

        /// <summary>
        /// Gets or sets the index file. Causes SymStore not to store the actual symbol files. Instead, SymStore records information in the IndexFile that will enable SymStore to access the symbol files at a later time.
        /// </summary>
        /// <value>The write index file.</value>
        public string WriteIndex { get; set; }

        /// <summary>
        /// Gets or sets the index file. Causes SymStore to read the data from a file created with WriteIndexFile.
        /// </summary>
        /// <value>The read index file.</value>
        public string ReadIndex { get; set; }
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
            if (string.Equals(Command, "query", StringComparison.OrdinalIgnoreCase))
                builder.AppendSwitch("query");
            else if (string.Equals(Command, "delete", StringComparison.OrdinalIgnoreCase))
                builder.AppendSwitch("del");
            else if (string.Equals(Command, "del", StringComparison.OrdinalIgnoreCase))
                builder.AppendSwitch("del");
            else
                builder.AppendSwitch("add");

            if (Append)
                builder.AppendSwitch("/a");
            builder.AppendSwitchIfNotNull("/c ", Comment);
            if (Compress)
                builder.AppendSwitch("/compress");
            builder.AppendSwitchIfNotNull("/d ", LogFile);
            builder.AppendSwitchIfNotNull("/g ", Share);
            builder.AppendSwitchIfNotNull("/i ", ID);
            if (Local)
                builder.AppendSwitch("/l");
            if (Verbose)
                builder.AppendSwitch("/o");
            if (Pointer)
                builder.AppendSwitch("/p");
            if (Recursive)
                builder.AppendSwitch("/r");
            builder.AppendSwitchIfNotNull("/s ", Store);
            builder.AppendSwitchIfNotNull("/t ", Product);
            builder.AppendSwitchIfNotNull("/v ", Version);

            builder.AppendSwitchIfNotNull("/x ", WriteIndex);
            builder.AppendSwitchIfNotNull("/y ", ReadIndex);

            builder.AppendSwitchIfNotNull("-:MSG ", Message);
            if (Relative)
                builder.AppendSwitch("-:REL");
            if (NoReference)
                builder.AppendSwitch("-:NOREFS");

            builder.AppendSwitchIfNotNull("/f ", Files);

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
                string path64 = Path.Combine(pf64, "Debugging Tools for Windows (x64)");
                if (Directory.Exists(path64))
                    return Path.Combine(path64, ToolName);
            }

            string path = Path.Combine(pf, "Debugging Tools for Windows (x86)");
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
            get { return "SymStore.exe"; }
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
