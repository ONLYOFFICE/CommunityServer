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
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.SourceServer
{
    /// <summary>
    /// A base class for source indexing a pdb symbol file.
    /// </summary>
    public abstract class SourceIndexBase : Task
    {
        /// <remarks />
        protected int Successful = 0;

        /// <remarks />
        protected int Failed = 0;

        /// <remarks />
        protected int Skipped = 0;

        private string sourceServerSdkPath;

        #region Properties

        /// <summary>
        /// Gets or sets the symbol files to have to source index added.
        /// </summary>
        /// <value>The symbol files.</value>
        [Required]
        public ITaskItem[] SymbolFiles { get; set; }

        /// <summary>
        /// Default install locations for the debugger tools if installed with the windows sdk (which I think is now the only option?).
        /// </summary>
        private static readonly string[] CandidateSdkPath =
        {
            @"C:\program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv",
            @"C:\program Files (x86)\Windows Kits\8.0\Debuggers\x64\srcsrv"
        };

        /// <summary>
        /// Gets or sets the source server SDK path.
        /// </summary>
        /// <value>The source server SDK path.</value>
        public string SourceServerSdkPath
        {
            get
            {
                if (sourceServerSdkPath == null)
                {
                    sourceServerSdkPath = CandidateSdkPath.FirstOrDefault(Directory.Exists);
                }

                return sourceServerSdkPath;
            }
            set { sourceServerSdkPath = value; }
        }

        /// <summary>
        /// Gets or sets the source command format. The SRCSRVCMD environment variable.
        /// </summary>
        /// <value>The source command format.</value>
        /// <remarks>
        /// Describes how to build the command to extract the file from source control.
        /// This includes the name of the executable file and its command-line parameters.
        /// See srcsrv.doc for full documentation on SRCSRVCMD.
        /// </remarks>
        public string SourceCommandFormat { get; set; }

        /// <summary>
        /// Gets or sets the source target format. The SRCSRVTRG environment variable.
        /// </summary>
        /// <value>The source target format.</value>
        /// <remarks>
        /// Describes how to build the target path for the extracted file. 
        /// See srcsrv.doc for full documentation on SRCSRVTRG.
        /// </remarks>
        public string SourceTargetFormat { get; set; }

        /// <summary>
        /// Gets or sets the fail on no source information found flag.
        /// </summary>
        /// <value><code>true</code> if finding no source information in a pdb should be treated as an error;
        /// otherwise <code>false</code>.</value>
        /// <remarks>
        /// Assemblies that only contain classes with auto properties, enums or constants may not contain
        /// any source information in the pdb.
        /// </remarks>
        public bool FailOnNoSourceInformationFound { get; set; }

        /// <summary>
        /// Gets the no source information found in 
        /// </summary>
        public bool NoSourceInformationInPdb { get; private set; }

        #endregion

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            foreach (ITaskItem symbolFile in SymbolFiles)
            {
                try
                {
                    IndexSymbolFile(symbolFile);
                }
                catch (Exception ex)
                {
                    Failed++;
                    Log.LogErrorFromException(ex);
                }
            }

            Log.LogMessage("Source Index Successful: {0}  Failed: {1}  Skipped: {2}", Successful, Failed, Skipped);

            return true;
        }

        /// <summary>
        /// Indexes the symbol file.
        /// </summary>
        /// <param name="item">The symbol file task item.</param>
        /// <returns><c>true</c> if index successfully; otherwise <c>false</c>.</returns>
        protected bool IndexSymbolFile(ITaskItem item)
        {
            bool result = false;

            // step 1: check if source already indexed
            // step 2: get source file list from pdb
            SymbolFile symbolFile = CreateSymbolFile(item);
            if (symbolFile == null)
                return false;

            // step 3: lookup svn info for each file
            result = AddSourceProperties(symbolFile);
            if (!result)
                return false;

            string indexFile = Path.GetTempFileName();

            try
            {
                // step 4: create SRCSRV file
                result = CreateSourceIndexFile(symbolFile, indexFile);
                if (!result)
                    return false;

                // step 5: write SRCSRV to pdb stream
                result = WriteSourceIndex(symbolFile, indexFile);

                if (result)
                {
                    Log.LogMessage("  '{0}' source indexed successfully.", symbolFile.File.Name);
                    Successful++;
                }

                return result;
            }
            finally
            {
                if (!KeepTempFileForDebugging)
                    File.Delete(indexFile);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to keep temporary source index file created during index for debugging purposes.
        /// </summary>
        /// <value>
        /// <c>true</c> to keep the temporary file; otherwise, <c>false</c>.
        /// </value>
        public bool KeepTempFileForDebugging { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="SymbolFile"/> from the symbol file task item and add the source file list to it.
        /// </summary>
        /// <param name="item">The symbol file task item.</param>
        /// <returns>An instance of <see cref="SymbolFile"/> or <c>null</c> if there was an error.</returns>
        protected virtual SymbolFile CreateSymbolFile(ITaskItem item)
        {
            SymbolFile symbolFile = new SymbolFile(item.ItemSpec);

            var srcTool = new SrcTool();
            CopyBuildEngine(srcTool);

            if (!string.IsNullOrEmpty(SourceServerSdkPath))
                srcTool.ToolPath = SourceServerSdkPath;

            srcTool.PdbFile = item;

            // step 1: check if source already indexed
            this.NoSourceInformationInPdb = false;
            srcTool.CountOnly = true;
            srcTool.Execute();
            if (srcTool.SourceCount > 0)
            {
                Log.LogWarning("'{0}' already has source indexing information.", symbolFile.File.Name);
                Skipped++;
                return null;
            }
            srcTool.CountOnly = false; // turn off 

            // step 2: get source file list from pdb
            srcTool.SourceOnly = true;
            if (!srcTool.Execute())
            {
                if (srcTool.SourceFiles != null && srcTool.SourceFiles.Length > 0 &&
                    srcTool.SourceFiles[0].StartsWith("No source information in pdb for"))
                {
                    this.NoSourceInformationInPdb = true;
                }

                if (this.FailOnNoSourceInformationFound)
                {
                    Log.LogError("Error getting source files from '{0}'.{1}", symbolFile.File.Name, srcTool.SourceFiles != null && srcTool.SourceFiles.Length > 0 ? srcTool.SourceFiles[0] : string.Empty);                
                    Failed++;
                }
                else
                {
                    Log.LogWarning("'{0}' has no source information inside the pdb. Check your pdb settings or perhaps your assembly doesn't have any real lines of code in it?", symbolFile.File.Name, srcTool.SourceFiles != null && srcTool.SourceFiles.Length > 0 ? srcTool.SourceFiles[0] : string.Empty);                
                    Skipped++;
                }
                return null;
            }

            foreach (string file in srcTool.SourceFiles)
            {
                // check that we didn't get garbage back from SrcTool.  
                if (!PathUtil.IsPathValid(file))
                {
                    Log.LogMessage(MessageImportance.Low, "  Invalid path for source file '{0}'.", file);
                    continue;
                }

                SourceFile sourceFile = new SourceFile(file);
                symbolFile.SourceFiles.Add(sourceFile);
            }

            return symbolFile;
        }

        /// <summary>
        /// Adds the source properties to the symbol file.
        /// </summary>
        /// <param name="symbolFile">The symbol file to add the source properties to.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        protected abstract bool AddSourceProperties(SymbolFile symbolFile);

        /// <summary>
        /// Creates the source index file.
        /// </summary>
        /// <param name="symbolFile">The symbol file to create the index file from.</param>
        /// <param name="sourceIndexFile">The source index file.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        protected abstract bool CreateSourceIndexFile(SymbolFile symbolFile, string sourceIndexFile);

        /// <summary>
        /// Writes the source index file to the symbol file.
        /// </summary>
        /// <param name="symbolFile">The symbol file.</param>
        /// <param name="sourceIndexFile">The source index file.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        protected virtual bool WriteSourceIndex(SymbolFile symbolFile, string sourceIndexFile)
        {
            if (!File.Exists(sourceIndexFile))
                return false;

            var pdbStr = new PdbStr();
            CopyBuildEngine(pdbStr);
            if (!string.IsNullOrEmpty(SourceServerSdkPath))
                pdbStr.ToolPath = SourceServerSdkPath;

            pdbStr.Command = "write";
            pdbStr.PdbFile = new TaskItem(symbolFile.File.FullName);
            pdbStr.StreamFile = new TaskItem(sourceIndexFile);
            pdbStr.StreamName = "srcsrv";

            return pdbStr.Execute();
        }

        /// <summary>
        /// Copies the build engine to the task.
        /// </summary>
        /// <param name="task">The task.</param>
        protected void CopyBuildEngine(ITask task)
        {
            task.BuildEngine = this.BuildEngine;
            task.HostObject = this.HostObject;
        }
    }
}